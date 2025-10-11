# Script de prueba rápida de publicación
# Ejecutar desde la raíz del proyecto

Write-Host "?? Prueba rápida de publicación de VRM Plugin Demo..." -ForegroundColor Cyan
Write-Host ""

$OutputPath = ".\publish-test"
$ProjectPath = "src\Host\VRM_PluginDemo.Blazor.Server\VRM_PluginDemo.Blazor.Server.csproj"

# Limpiar carpeta de prueba si existe
if (Test-Path $OutputPath) {
    Write-Host "?? Limpiando carpeta de prueba anterior..." -ForegroundColor Yellow
    Remove-Item $OutputPath -Recurse -Force
}

# Publicar
Write-Host "?? Publicando en: $OutputPath" -ForegroundColor White
Write-Host ""
dotnet publish $ProjectPath `
    --configuration Release `
    --output $OutputPath `
    --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Error al publicar" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Verificando módulos..." -ForegroundColor Cyan
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""

$ModulesPath = Join-Path $OutputPath "Modules"
if (Test-Path $ModulesPath) {
    $Modules = Get-ChildItem -Path $ModulesPath -Filter "*.dll"
    
    if ($Modules.Count -gt 0) {
        Write-Host "? SUCCESS: Encontrados $($Modules.Count) módulos" -ForegroundColor Green
        Write-Host ""
        foreach ($module in $Modules) {
            $size = [math]::Round($module.Length / 1KB, 2)
            Write-Host "   ?? $($module.Name) ($size KB)" -ForegroundColor White
        }
    } else {
        Write-Host "? FAIL: La carpeta Modules está vacía" -ForegroundColor Red
    }
} else {
    Write-Host "? FAIL: No existe la carpeta Modules" -ForegroundColor Red
}

Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Estructura de archivos publicados:" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""

$items = Get-ChildItem $OutputPath | Select-Object Mode, Name, @{Name="Size";Expression={
    if ($_.PSIsContainer) { 
        $size = (Get-ChildItem $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum
        if ($size) { 
            [math]::Round($size / 1MB, 2).ToString() + " MB" 
        } else { 
            "-" 
        }
    } else { 
        [math]::Round($_.Length / 1KB, 2).ToString() + " KB" 
    }
}}

$items | Format-Table -AutoSize

Write-Host ""
Write-Host "?? Archivos clave encontrados:" -ForegroundColor Cyan
$keyFiles = @(
    "VRM_PluginDemo.Blazor.Server.dll",
    "VRM_PluginDemo.Blazor.Server.exe",
    "web.config",
    "appsettings.json"
)

foreach ($file in $keyFiles) {
    $fullPath = Join-Path $OutputPath $file
    if (Test-Path $fullPath) {
        Write-Host "   ? $file" -ForegroundColor Green
    } else {
        Write-Host "   ??  $file (no encontrado)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? Prueba completada!" -ForegroundColor Green
Write-Host ""
Write-Host "Para probar la aplicación publicada, ejecuta:" -ForegroundColor Cyan
Write-Host "   cd $OutputPath" -ForegroundColor White
Write-Host "   dotnet VRM_PluginDemo.Blazor.Server.dll" -ForegroundColor White
Write-Host ""
