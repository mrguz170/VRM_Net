using VRM_Plugin.Blazor.Server.Components;
using VRM_Plugin.Blazor.Server.Services;
using VRM_Plugin.Blazor.Server.StateService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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
// ⭐ NUEVO: Servicio de estado de tema (dark/light mode)
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

// ==================== SISTEMA DE PLUGINS ====================

// Crear un ServiceProvider temporal solo para obtener el logger
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
    loggingBuilder.AddConsole());
var logger = loggerFactory.CreateLogger<ModuleLoader>();

// Registrar el ModuleLoader como Singleton
var moduleLoader = new ModuleLoader(logger);

// Registrar IModuleManager para que otros servicios puedan consultarlo
builder.Services.AddSingleton<IModuleManager>(moduleLoader);

// Descubrir y cargar módulos desde la carpeta "Modules"
var modulesPath = "Modules";
var modulosEncontrados = await moduleLoader.DiscoverAndLoadModulesAsync(modulesPath);

Console.WriteLine($"\n╔══════════════════════════════════════════════════════════╗");
Console.WriteLine($"║  🔌 SISTEMA DE PLUGINS INICIADO                         ║");
Console.WriteLine($"║  📦 Módulos cargados: {modulosEncontrados,-2}                              ║");
Console.WriteLine($"╚══════════════════════════════════════════════════════════╝\n");

// ==================== REGISTRAR SERVICIOS DE MÓDULOS ====================

var todosLosModulos = moduleLoader.GetAllModules();

foreach (var modulo in todosLosModulos)
{
    Console.WriteLine($"⚙️  Configurando servicios del módulo: {modulo.ModuleId}");

    // Cada módulo registra sus propios servicios (repositorios, validadores, etc.)
    modulo.ConfigureServices(builder.Services, builder.Configuration);
}

Console.WriteLine($"\n✅ Configuración de servicios completada\n");

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

Console.WriteLine($"🔌 Registrando {moduleAssemblies.Length} ensamblados de módulos para interactividad:");
foreach (var asm in moduleAssemblies)
{
    Console.WriteLine($"   • {asm.GetName().Name}");
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(moduleAssemblies);

// ==================== INFORMACIÓN DE MÓDULOS AL INICIAR ====================

// Capturar variables para el evento
var environmentName = app.Environment.EnvironmentName;
var urls = app.Urls;

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("🚀 APLICACIÓN INICIADA");
    Console.WriteLine(new string('=', 60));
    Console.WriteLine($"🌐 Entorno: {environmentName}");
    Console.WriteLine($"📍 URL: {urls.FirstOrDefault() ?? "No disponible"}");
    Console.WriteLine("\n📦 MÓDULOS CARGADOS:");
    Console.WriteLine(new string('-', 60));

    foreach (var modulo in todosLosModulos)
    {
        Console.WriteLine($"  • {modulo.ModuleId,-20} v{modulo.Version,-8} - {modulo.DisplayName}");
        Console.WriteLine($"    Categoría: {modulo.Category}");
        Console.WriteLine($"    Autor: {modulo.Author}");

        if (modulo.RequiredPermissions.Any())
        {
            Console.WriteLine($"    Permisos: {string.Join(", ", modulo.RequiredPermissions)}");
        }

        if (modulo.Dependencies.Any())
        {
            Console.WriteLine($"    Dependencias: {string.Join(", ", modulo.Dependencies)}");
        }

        Console.WriteLine();
    }

    Console.WriteLine(new string('=', 60) + "\n");
});

app.Run();