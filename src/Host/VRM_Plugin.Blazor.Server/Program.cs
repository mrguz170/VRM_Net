using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using System.Reflection;
using VRM_Plugin.Blazor.Server.Components;
using VRM_Plugin.Blazor.Server.Services;
using VRM_Plugin.Blazor.Server.Authentication;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Core.Abstractions.Common;

// ==================== CONFIGURACIÓN DE SERILOG ====================
// Configurar Serilog ANTES de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "VRM_Plugin")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/vrm-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10_485_760  // 10 MB
    )
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando aplicación VRM_Plugin");

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog para logging
    builder.Host.UseSerilog();

    // ==================== SERVICIOS BÁSICOS DE BLAZOR ====================
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // ==================== CONFIGURACIÓN DE BLAZOR SERVER CIRCUITS ====================
    builder.Services.AddServerSideBlazor(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.DisconnectedCircuitMaxRetained = 100;
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    });

    // ==================== MUDBLAZOR ====================
    builder.Services.AddMudServices();
        

    // ==================== AUTENTICACIÓN Y AUTORIZACIÓN ====================   
    // Configurar esquema de autenticación por defecto para Blazor Server
    builder.Services.AddAuthentication(options =>
    {
        // Blazor Server usa cookies para mantener la sesión
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<VRMAuthenticationStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
        provider.GetRequiredService<VRMAuthenticationStateProvider>());

    // Registrar servicio de usuario actual
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddScoped<IModuleAuthorizationService, ModuleAuthorizationService>();

    // ==================== SERVICIOS DE DATOS ====================
    
    // Registrar DatabaseHelper (infraestructura compartida)
    builder.Services.AddScoped<DatabaseHelper>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found in configuration");
        return new DatabaseHelper(connectionString);
    });
    
    // Registrar servicio de metadata de módulos
    builder.Services.AddScoped<IModuleMetadataService, ModuleMetadataRepository>();
    
    // Registrar servicio de usuarios
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    
    Log.Information("✅ Servicios de datos registrados");

    // ==================== SISTEMA DE PLUGINS ====================

    // Declarar moduleManager fuera del using para usarlo después
    ModuleManager moduleManager;
    
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ModuleManager>>();
        var metadataService = scope.ServiceProvider.GetRequiredService<IModuleMetadataService>();
        
        moduleManager = new ModuleManager(logger, metadataService);
        
        // Registrar IModuleManager para que otros servicios puedan consultarlo
        builder.Services.AddSingleton<IModuleManager>(moduleManager);

        // Construir ruta absoluta a la carpeta Modules
        var baseDirectory = AppContext.BaseDirectory; // bin\Debug\net8.0\
        var modulesPath = Path.Combine(baseDirectory, "Modules");

        // Verificar si la carpeta existe antes de cargar
        if (!Directory.Exists(modulesPath))
        {
            Log.Warning("⚠️ La carpeta Modules no existe: {ModulesPath}", modulesPath);
            Log.Warning("⚠️ Creando carpeta Modules...");
            Directory.CreateDirectory(modulesPath);
        }

        // Descubrir y cargar módulos con metadata desde BD
        var modulosEncontrados = await moduleManager.DiscoverAndLoadModulesAsync(modulesPath);
        
        Log.Information(
            "✅ {ModulosEncontrados} módulos cargados con metadata desde BD",
            modulosEncontrados);

        // Obtener módulos una sola vez
        var modulos = moduleManager.GetAllModules();

        foreach (var modulo in modulos)
        {
            // Cada módulo registra sus propios servicios
            modulo.ConfigureServices(builder.Services, builder.Configuration);
        }
    }

    // ==================== CONSTRUIR LA APLICACIÓN ====================

    var app = builder.Build();

    // ==================== PIPELINE DE MIDDLEWARE ====================

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }
    else
    {       
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // Agregar autenticación y autorización al pipeline
    app.UseAuthentication();
    app.UseAuthorization();

    // Obtener módulos del moduleManager (ya está fuera del using)
    var todosLosModulos = moduleManager.GetAllModules();
    
    var moduleAssemblies = todosLosModulos
        .Select(m => m.GetType().Assembly)
        .Distinct()
        .ToArray();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddAdditionalAssemblies(moduleAssemblies);

    // ==================== INFORMACIÓN DE MÓDULOS AL INICIAR ====================

    // Capturar variables para el evento
    var environmentName = app.Environment.EnvironmentName;
    var urls = app.Urls;

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("\n" + new string('=', 60));
        Log.Information("🚀 APLICACIÓN INICIADA");
        Log.Information(new string('=', 60));
        Log.Information("🌐 Entorno: {EnvironmentName}", environmentName);
        Log.Information("📍 URL: {Url}", urls.FirstOrDefault() ?? "No disponible");
        Log.Information("\n📦 MÓDULOS CARGADOS:");
        Log.Information(new string('-', 60));

        foreach (var modulo in todosLosModulos)
        {
            Log.Information("  • {ModuleName,-20} (ID: {ModuleId}) v{Version,-8}",
                modulo.ModuleName,
                modulo.ModuleId,
                modulo.Version);
            Log.Information("    {DisplayName}", modulo.DisplayName);
            Log.Information("    Descripción: {Description}", modulo.Description);
            
            var components = modulo.GetComponents();
            var actions = modulo.GetActions();
            
            Log.Information("    📋 Componentes: {ComponentCount}", components.Count);
            Log.Information("    ⚡ Acciones: {ActionCount}", actions.Count);
            
            // Mostrar componentes raíz (categorías)
            var rootComponents = components.Where(c => c.ParentId == null && c.ShowInMenu);
            if (rootComponents.Any())
            {
                Log.Information("    Menú principal:");
                foreach (var rc in rootComponents)
                {
                    Log.Information("      └─ {Name} ({Icon})", rc.Name, rc.Icon);
                }
            }
        }
        
        Log.Information(new string('=', 60));
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "🛑 La aplicación no pudo iniciarse correctamente");
    throw;
}
finally
{
    // Asegurarse de que todos los logs se escriben antes de cerrar
    Log.Information("🔄 Cerrando sistema de logging...");
    Log.CloseAndFlush();
}