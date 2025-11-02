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

# Publicar con más verbosidad
Write-Host "?? Publicando en: $OutputPath" -ForegroundColor White
Write-Host "   Ejecutando: dotnet publish..." -ForegroundColor Gray
Write-Host ""

dotnet publish $ProjectPath `
    --configuration Release `
    --output $OutputPath `
    --self-contained false `
    --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Error al publicar (código: $LASTEXITCODE)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Verificando estructura de publicación..." -ForegroundColor Cyan
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""

# Verificar carpeta Modules
$ModulesPath = Join-Path $OutputPath "Modules"
Write-Host "Buscando carpeta Modules en: $ModulesPath" -ForegroundColor Gray

if (Test-Path $ModulesPath) {
    Write-Host "? Carpeta Modules existe" -ForegroundColor Green
    
    $Modules = Get-ChildItem -Path $ModulesPath -Filter "*.dll"
    
    if ($Modules.Count -gt 0) {
        Write-Host ""
        Write-Host "? SUCCESS: Encontrados $($Modules.Count) módulos" -ForegroundColor Green
        Write-Host ""
        foreach ($module in $Modules) {
            $size = [math]::Round($module.Length / 1KB, 2)
            Write-Host "   ?? $($module.Name)" -ForegroundColor White
            Write-Host "      Tamaño: $size KB" -ForegroundColor Gray
            Write-Host "      Ruta: $($module.FullName)" -ForegroundColor DarkGray
            Write-Host ""
        }
    } else {
        Write-Host ""
        Write-Host "? FAIL: La carpeta Modules existe pero está vacía" -ForegroundColor Red
        Write-Host ""
        Write-Host "Contenido de la carpeta Modules:" -ForegroundColor Yellow
        Get-ChildItem -Path $ModulesPath | Format-Table -AutoSize
    }
} else {
    Write-Host ""
    Write-Host "? FAIL: No existe la carpeta Modules" -ForegroundColor Red
    Write-Host ""
    Write-Host "Carpetas encontradas en la raíz de publicación:" -ForegroundColor Yellow
    Get-ChildItem -Path $OutputPath -Directory | Select-Object Name | Format-Table -AutoSize
}

Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Estructura completa de archivos publicados:" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""

$items = Get-ChildItem $OutputPath | Select-Object Mode, Name, @{Name="Size";Expression={
    if ($_.PSIsContainer) { 
        $size = (Get-ChildItem $_.FullName -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        if ($size) { 
            [math]::Round($size / 1MB, 2).ToString() + " MB" 
        } else { 
            "0 MB" 
        }
    } else { 
        [math]::Round($_.Length / 1KB, 2).ToString() + " KB" 
    }
}}

$items | Format-Table -AutoSize

Write-Host ""
Write-Host "?? Archivos clave verificados:" -ForegroundColor Cyan
$keyFiles = @(
    "VRM_PluginDemo.Blazor.Server.dll",
    "VRM_PluginDemo.Blazor.Server.exe",
    "web.config",
    "appsettings.json"
)

foreach ($file in $keyFiles) {
    $fullPath = Join-Path $OutputPath $file
    if (Test-Path $fullPath) {
        $fileInfo = Get-Item $fullPath
        $size = [math]::Round($fileInfo.Length / 1KB, 2)
        Write-Host "   ? $file ($size KB)" -ForegroundColor Green
    } else {
        Write-Host "   ??  $file (no encontrado)" -ForegroundColor Yellow
    }
}

# Verificar también los módulos compilados en src
Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Verificando módulos compilados en src/Modules..." -ForegroundColor Cyan
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""

$SourceModules = Get-ChildItem -Path "src\Modules" -Recurse -Filter "VRM_PluginDemo.Modules.*.dll" | 
                 Where-Object { $_.FullName -like "*\bin\Release\net8.0\*" }

if ($SourceModules.Count -gt 0) {
    Write-Host "? Encontrados $($SourceModules.Count) módulos compilados en src:" -ForegroundColor Green
    foreach ($module in $SourceModules) {
        Write-Host "   ?? $($module.Name)" -ForegroundColor White
        Write-Host "      $($module.DirectoryName)" -ForegroundColor DarkGray
    }
} else {
    Write-Host "??  No se encontraron módulos compilados en src\Modules\**\bin\Release\net8.0\" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host "?? Prueba completada!" -ForegroundColor Green
Write-Host "????????????????????????????????????????" -ForegroundColor White
Write-Host ""
Write-Host "Para probar la aplicación publicada, ejecuta:" -ForegroundColor Cyan
Write-Host "   cd $OutputPath" -ForegroundColor White
Write-Host "   dotnet VRM_PluginDemo.Blazor.Server.dll" -ForegroundColor White
Write-Host ""
