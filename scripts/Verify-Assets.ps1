# Script de Verificación de Assets VRM vs Sliced
# Autor: GitHub Copilot
# Fecha: Enero 2025

param(
    [switch]$Detailed = $false
)

# Variables de ruta
$slicedWwwroot = "C:\Users\lobo_\Documents\VRM\themeforest-D7eVh8Qb-sliced-mudblazor-admin-dashboard-template\Sliced_Mudblazor_1.0.0\Admin\Sliced_web_app\wwwroot"
$vrmWwwroot = "C:\Users\lobo_\Documents\VRM\VRM_Net\src\Host\VRM_PluginDemo.Blazor.Server\wwwroot"

Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?         VRM PLUGIN DEMO - VERIFICACIÓN DE ASSETS     ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Función para verificar archivo
function Test-Asset {
    param(
        [string]$RelativePath,
        [string]$Category
    )
    
    $slicedPath = Join-Path $slicedWwwroot $RelativePath
    $vrmPath = Join-Path $vrmWwwroot $RelativePath
    
    $slicedExists = Test-Path $slicedPath
    $vrmExists = Test-Path $vrmPath
    
    $result = [PSCustomObject]@{
        Category = $Category
        File = $RelativePath
        SlicedExists = $slicedExists
     VRMExists = $vrmExists
        Status = if($vrmExists){"?"}else{"?"}
        SlicedSize = if($slicedExists){(Get-Item $slicedPath).Length}else{0}
        VRMSize = if($vrmExists){(Get-Item $vrmPath).Length}else{0}
    }
    
    return $result
}

# Arrays de archivos a verificar
$cssFiles = @(
    "assets/css/tailwind.css",
    "assets/css/plugins.css",
    "assets/css/remixicon.css",
    "assets/css/style.css"
)

$jsFiles = @(
    "assets/js/main.js",
    "assets/js/pages/alpine.min.js",
    "assets/js/pages/alpine-persist.min.js",
    "assets/js/pages/alpine-collaspe.min.js"
)

$vrmExtras = @(
    "assets/js/vrm-init.js"
)

$libs = @(
    "assets/libs/@alpinejs",
    "assets/libs/@popperjs",
    "assets/libs/alpinejs",
    "assets/libs/apexcharts",
    "assets/libs/fancybox",
    "assets/libs/flatpickr",
    "assets/libs/magnific-popup",
    "assets/libs/remixicon",
    "assets/libs/simplebar",
 "assets/libs/sortablejs",
    "assets/libs/swiper",
    "assets/libs/tailwindcss",
    "assets/libs/tippy.js"
)

# Verificar CSS
Write-Host "?? ARCHIVOS CSS CRÍTICOS" -ForegroundColor Yellow
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor DarkGray

$cssResults = @()
foreach ($file in $cssFiles) {
    $result = Test-Asset -RelativePath $file -Category "CSS"
    $cssResults += $result
    
    Write-Host "  $($result.Status) " -NoNewline -ForegroundColor $(if($result.VRMExists){"Green"}else{"Red"})
    Write-Host "$($result.File)" -NoNewline
    
    if ($Detailed -and $result.VRMExists) {
        $sizeKB = [math]::Round($result.VRMSize / 1KB, 2)
     Write-Host " ($sizeKB KB)" -ForegroundColor Gray
    } else {
        Write-Host ""
    }
}
Write-Host ""

# Verificar JS
Write-Host "?? ARCHIVOS JAVASCRIPT CRÍTICOS" -ForegroundColor Yellow
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor DarkGray

$jsResults = @()
foreach ($file in $jsFiles) {
    $result = Test-Asset -RelativePath $file -Category "JavaScript"
    $jsResults += $result
    
    Write-Host "  $($result.Status) " -NoNewline -ForegroundColor $(if($result.VRMExists){"Green"}else{"Red"})
    Write-Host "$($result.File)" -NoNewline
 
    if ($Detailed -and $result.VRMExists) {
        $sizeKB = [math]::Round($result.VRMSize / 1KB, 2)
      $slicedSizeKB = [math]::Round($result.SlicedSize / 1KB, 2)
    
        if ($result.SlicedExists) {
  $diff = $result.VRMSize - $result.SlicedSize
    $diffPercent = [math]::Round(($diff / $result.SlicedSize) * 100, 1)
        
    if ($diff -ne 0) {
        Write-Host " (VRM: $sizeKB KB vs Sliced: $slicedSizeKB KB, " -NoNewline -ForegroundColor Gray
if ($diff -lt 0) {
      Write-Host "$diffPercent% más pequeño" -NoNewline -ForegroundColor Green
      } else {
        Write-Host "+$diffPercent% más grande" -NoNewline -ForegroundColor Yellow
         }
         Write-Host ")" -ForegroundColor Gray
            } else {
          Write-Host " ($sizeKB KB - Idéntico)" -ForegroundColor Gray
            }
        } else {
   Write-Host " ($sizeKB KB)" -ForegroundColor Gray
    }
    } else {
      Write-Host ""
    }
}
Write-Host ""

# Verificar archivos extra de VRM
Write-Host "?? MEJORAS EXCLUSIVAS DE VRM" -ForegroundColor Magenta
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor DarkGray

foreach ($file in $vrmExtras) {
    $vrmPath = Join-Path $vrmWwwroot $file
    $exists = Test-Path $vrmPath
    
    Write-Host "  ? " -NoNewline -ForegroundColor Green
    Write-Host "$file" -NoNewline
    
    if ($Detailed -and $exists) {
        $sizeKB = [math]::Round((Get-Item $vrmPath).Length / 1KB, 2)
        Write-Host " ($sizeKB KB)" -ForegroundColor Gray
    } else {
   Write-Host ""
  }
}
Write-Host ""

# Verificar Librerías
Write-Host "?? LIBRERÍAS JAVASCRIPT" -ForegroundColor Yellow
Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor DarkGray

$libResults = @()
$libsOk = 0
foreach ($lib in $libs) {
  $vrmPath = Join-Path $vrmWwwroot $lib
    $exists = Test-Path $vrmPath
    
    if ($exists) { $libsOk++ }
    
    $status = if($exists){"?"}else{"?"}
    $color = if($exists){"Green"}else{"Red"}
    
    Write-Host "  $status " -NoNewline -ForegroundColor $color
    Write-Host ($lib -replace "assets/libs/", "")
}
Write-Host ""

# Resumen
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?      RESUMEN                 ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$cssOk = ($cssResults | Where-Object { $_.VRMExists }).Count
$jsOk = ($jsResults | Where-Object { $_.VRMExists }).Count
$totalLibs = $libs.Count

Write-Host "  ?? CSS Files:        " -NoNewline
Write-Host "$cssOk/$($cssFiles.Count) " -NoNewline -ForegroundColor $(if($cssOk -eq $cssFiles.Count){"Green"}else{"Red"})
Write-Host "$(if($cssOk -eq $cssFiles.Count){"?"}else{"?"})" -ForegroundColor $(if($cssOk -eq $cssFiles.Count){"Green"}else{"Red"})

Write-Host "  ?? JS Files:         " -NoNewline
Write-Host "$jsOk/$($jsFiles.Count) " -NoNewline -ForegroundColor $(if($jsOk -eq $jsFiles.Count){"Green"}else{"Red"})
Write-Host "$(if($jsOk -eq $jsFiles.Count){"?"}else{"?"})" -ForegroundColor $(if($jsOk -eq $jsFiles.Count){"Green"}else{"Red"})

Write-Host "  ?? Libraries:        " -NoNewline
Write-Host "$libsOk/$totalLibs " -NoNewline -ForegroundColor $(if($libsOk -eq $totalLibs){"Green"}else{"Red"})
Write-Host "$(if($libsOk -eq $totalLibs){"?"}else{"?"})" -ForegroundColor $(if($libsOk -eq $totalLibs){"Green"}else{"Red"})

Write-Host "  ?? VRM Extras:       " -NoNewline
Write-Host "$($vrmExtras.Count) archivos" -ForegroundColor Magenta
Write-Host ""

# Estado final
$allOk = ($cssOk -eq $cssFiles.Count) -and ($jsOk -eq $jsFiles.Count) -and ($libsOk -eq $totalLibs)

if ($allOk) {
    Write-Host "?? ESTADO: " -NoNewline
    Write-Host "COMPLETO Y FUNCIONAL" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Tu proyecto VRM tiene todos los archivos necesarios de Sliced" -ForegroundColor Gray
    Write-Host "  además de mejoras exclusivas para integración con Blazor." -ForegroundColor Gray
} else {
 Write-Host "??  ESTADO: " -NoNewline
    Write-Host "FALTAN ARCHIVOS" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Ejecuta el script de copia para completar los archivos faltantes." -ForegroundColor Yellow
}

Write-Host ""

# Comparación de tamaños totales (opcional)
if ($Detailed) {
 Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "?          DETALLES AVANZADOS         ?" -ForegroundColor Cyan
    Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "?? Comparación de Tamaños:" -ForegroundColor Yellow
    Write-Host ""
    
    # Contar archivos totales
    $slicedTotal = (Get-ChildItem -Path "$slicedWwwroot\assets" -Recurse -File).Count
    $vrmTotal = (Get-ChildItem -Path "$vrmWwwroot\assets" -Recurse -File).Count
    
    Write-Host "  Total archivos en assets/:" -ForegroundColor Gray
    Write-Host "    Sliced:  $slicedTotal archivos" -ForegroundColor Gray
    Write-Host "    VRM:   $vrmTotal archivos" -ForegroundColor Gray
    
    if ($vrmTotal -gt $slicedTotal) {
        $diff = $vrmTotal - $slicedTotal
        Write-Host "    VRM tiene $diff archivos adicionales (mejoras custom)" -ForegroundColor Magenta
    }
    
    Write-Host ""
}

Write-Host "?????????????????????????????????????????????????????????????????" -ForegroundColor DarkGray
Write-Host "Ejecuta con -Detailed para ver información detallada de tamaños" -ForegroundColor Gray
Write-Host ""
