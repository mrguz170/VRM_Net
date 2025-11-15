using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using System.Reflection;
using VRM_Plugin.Blazor.Server.Components;
using VRM_Plugin.Blazor.Server.Services;
using VRM_Plugin.Blazor.Server.StateService;
using VRM_Plugin.Core.Abstractions;

// ==================== CONFIGURACIÓN DE SERILOG ====================
// ✅ Configurar Serilog ANTES de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "VRM_PluginDemo")
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
    Log.Information("🚀 Iniciando aplicación VRM_PluginDemo");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ Usar Serilog para logging
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
    // ⭐ NUEVO: Servicios de MudBlazor para componentes de UI
    builder.Services.AddMudServices();

    // ==================== STATE SERVICES (SLICED) ====================
    builder.Services.AddSingleton<ModeStateService>();

    // ==================== AUTENTICACIÓN Y AUTORIZACIÓN ====================
    // ⚠️ AUTENTICACIÓN SIMULADA (SOLO DESARROLLO)
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
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();

    // ✅ NUEVO: HttpContextAccessor para acceder a cookies
    builder.Services.AddHttpContextAccessor();

    // ⭐ CAMBIO CRÍTICO: Scoped con PersistentComponentState
    // DummyAuthenticationStateProvider ahora usa PersistentComponentState
    // para mantener autenticación entre SSR e Interactive Server
    builder.Services.AddScoped<DummyAuthenticationStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
        provider.GetRequiredService<DummyAuthenticationStateProvider>());

    // ==================== AUTORIZACIÓN GRANULAR DE MÓDULOS ====================
    // ⭐ NUEVO: Servicio para verificar permisos por acción
    builder.Services.AddScoped<IModuleAuthorizationService, ModuleAuthorizationService>();

    // ✅ NUEVO: Servicio para obtener nombres de visualización de roles
    builder.Services.AddSingleton<IRoleDisplayNameService, RoleDisplayNameService>();


    // ==================== SISTEMA DE PLUGINS ====================

    // Crear un ServiceProvider temporal solo para obtener el logger
    using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        loggingBuilder.AddConsole());
    var logger = loggerFactory.CreateLogger<ModuleLoader>();

    // Registrar el ModuleLoader como Singleton
    var moduleLoader = new ModuleLoader(logger);

    // Registrar IModuleManager para que otros servicios puedan consultarlo
    builder.Services.AddSingleton<IModuleManager>(moduleLoader);

    // ✅ CORREGIDO: Construir ruta absoluta a la carpeta Modules
    var baseDirectory = AppContext.BaseDirectory; // bin\Debug\net8.0\
    var modulesPath = Path.Combine(baseDirectory, "Modules");

    Log.Information("🔍 Buscando módulos en: {ModulesPath}", modulesPath);
    Log.Information("📂 Directorio base de aplicación: {BaseDirectory}", baseDirectory);

    // Verificar si la carpeta existe antes de cargar
    if (!Directory.Exists(modulesPath))
    {
        Log.Warning("⚠️ La carpeta Modules no existe: {ModulesPath}", modulesPath);
        Log.Warning("⚠️ Creando carpeta Modules...");
        Directory.CreateDirectory(modulesPath);
    }

    // Listar archivos DLL en la carpeta
    var dllFiles = Directory.GetFiles(modulesPath, "*.dll", SearchOption.AllDirectories);
    Log.Information("📋 Archivos DLL encontrados en Modules: {DllCount}", dllFiles.Length);
    foreach (var dll in dllFiles)
    {
        Log.Debug("   • {DllName}", Path.GetFileName(dll));
    }

    // Descubrir y cargar módulos desde la carpeta "Modules"
    var modulosEncontrados = await moduleLoader.DiscoverAndLoadModulesAsync(modulesPath);

    // ==================== REGISTRAR SERVICIOS DE MÓDULOS ====================

    var todosLosModulos = moduleLoader.GetAllModules();

    foreach (var modulo in todosLosModulos)
    {
        // Cada módulo registra sus propios servicios (repositorios, validadores, etc.)
        modulo.ConfigureServices(builder.Services, builder.Configuration);
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
        // ⭐ AGREGAR: Mejor debugging en desarrollo (como Sliced_web_app)
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // ⭐ IMPORTANTE: Agregar autenticación y autorización al pipeline
    app.UseAuthentication();
    app.UseAuthorization();

    // Obtener ensamblados de módulos para habilitar interactividad
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
        //Console.WriteLine("\n" + new string('=', 60));
        //Console.WriteLine("🚀 APLICACIÓN INICIADA");
        //Console.WriteLine(new string('=', 60));
        //Console.WriteLine($"🌐 Entorno: {environmentName}");
        //Console.WriteLine($"📍 URL: {urls.FirstOrDefault() ?? "No disponible"}");
        //Console.WriteLine("\n📦 MÓDULOS CARGADOS:");
        //Console.WriteLine(new string('-', 60));

        foreach (var modulo in todosLosModulos)
        {
            //Console.WriteLine($"  • {modulo.ModuleName,-20} (ID: {modulo.IdModule}) v{modulo.Version,-8}");
            //Console.WriteLine($"    {modulo.DisplayName}");
            //Console.WriteLine($"    Descripción: {modulo.Description}");
            
            var components = modulo.GetComponents();
            var actions = modulo.GetActions();
            
            //Console.WriteLine($"    📋 Componentes: {components.Count}");
            //Console.WriteLine($"    ⚡ Acciones: {actions.Count}");
            
            // Mostrar componentes raíz (categorías)
            var rootComponents = components.Where(c => c.IdParent == null && c.ShowInMenu);
            if (rootComponents.Any())
            {
                //Console.WriteLine($"    Menú principal:");
                foreach (var rc in rootComponents)
                {
                    Console.WriteLine($"      └─ {rc.Name} ({rc.Icon})");
                }
            }
        }
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
    // ✅ Asegurarse de que todos los logs se escriben antes de cerrar
    Log.Information("🔄 Cerrando sistema de logging...");
    Log.CloseAndFlush();
}