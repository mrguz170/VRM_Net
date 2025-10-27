# Solución: Dark Mode no persiste al navegar a módulos

**Fecha**: 2024  
**Problema**: El dark mode se pierde al navegar a un módulo (`/finanzas`, `/prospectos`)  
**Estado**: ✅ Resuelto

---

## 🔴 Problema Identificado

### Síntomas

1. Usuario está en dark mode
2. Usuario navega a un módulo (ej: `/finanzas`)
3. El módulo se carga en **light mode** (incorrecto)
4. No aparece mensaje de consola `🌓 Modo cambiado a: dark`

### Causa Raíz

**Blazor con `@rendermode InteractiveServer` recarga componentes** pero Alpine.js **NO** estaba reaplicando el modo guardado en localStorage.

```razor
<!-- Módulos con InteractiveServer -->
@page "/finanzas"
@rendermode InteractiveServer  <!-- ⚠️ Recarga el componente -->
```

**Flujo del problema:**

```
1. Usuario activa dark mode
   ↓
2. Alpine guarda en localStorage: _x_mode = "dark"
   ↓
3. Usuario navega a /finanzas
   ↓
4. Blazor recarga el componente con InteractiveServer
   ↓
5. Alpine NO reaplicaba el modo guardado
   ↓
6. ❌ Resultado: Vuelve a light mode
```

---

## ✅ Solución Implementada

### 1. **Nuevo método `applyStoredMode()` en `main.js`**

```javascript
// main.js
Alpine.store("app", {
    // ... propiedades existentes ...
    
 // ✅ NUEVO: Método para aplicar el modo guardado
  applyStoredMode() {
        const savedMode = this.mode;
        console.log('🔄 Aplicando modo guardado:', savedMode);
        this.toggleMode(savedMode);
    }
});
```

**¿Qué hace?**
- Lee el modo guardado de `Alpine.$persist` (que a su vez lee de localStorage)
- Llama a `toggleMode()` para actualizar el DOM
- Asegura que la clase `.dark` se agregue/remueva correctamente

### 2. **Llamar `applyStoredMode()` al inicializar Alpine**

```javascript
// main.js
document.addEventListener("alpine:init", () => {
    // ... configuración del store ...
    
    // ✅ Aplicar modo al inicializar Alpine
    console.log('🎨 Inicializando Alpine.js store...');
    Alpine.store('app').applyStoredMode();
});
```

### 3. **Reaplicar modo cuando el DOM esté listo**

```javascript
// main.js
document.addEventListener('DOMContentLoaded', () => {
    console.log('📄 DOM cargado, verificando estado de Alpine...');
    
  // Esperar a que Alpine esté completamente inicializado
    setTimeout(() => {
        if (window.Alpine && Alpine.store && Alpine.store('app')) {
            console.log('✅ Alpine detectado, aplicando modo guardado...');
            Alpine.store('app').applyStoredMode();
        }
    }, 100);
});
```

### 4. **Escuchar navegación de Blazor** ⭐ **CLAVE**

```javascript
// App.razor
Blazor.addEventListener('enhancedload', () => {
    console.log('🚀 Blazor enhancedload - Reaplicando modo...');
    if (typeof Alpine !== 'undefined' && Alpine.store && Alpine.store('app')) {
        Alpine.store('app').applyStoredMode();
    }
});
```

**¿Qué es `enhancedload`?**
- Evento de Blazor que se dispara **después** de una navegación
- Se dispara cuando Blazor carga un componente con `@rendermode InteractiveServer`
- Perfecto para reaplicar configuraciones de Alpine.js

---

## 🔬 Flujo Completo (Correcto)

```
1. Usuario activa dark mode
   ├─→ Alpine.store('app').toggleMode('dark')
   ├─→ this.mode = 'dark' (guardado con Alpine.$persist)
   ├─→ localStorage._x_mode = "dark"
   └─→ document.documentElement.classList.add('dark')
   
2. Usuario navega a /finanzas
   ├─→ Blazor recarga componente (InteractiveServer)
   ├─→ Evento 'enhancedload' se dispara
   ├─→ Listener ejecuta: Alpine.store('app').applyStoredMode()
   ├─→ applyStoredMode() lee: this.mode (persistido con Alpine.$persist)
   ├─→ toggleMode('dark') actualiza DOM
   └─→ ✅ Dark mode se mantiene
```

---

## 📊 Comparación: Antes vs Después

| Escenario | ❌ ANTES | ✅ DESPUÉS |
|-----------|----------|-----------|
| **Navegar a /finanzas** | Vuelve a light mode | Mantiene dark mode |
| **Recargar página (F5)** | Mantiene dark mode | Mantiene dark mode |
| **Cambio manual de modo** | Funciona | Funciona |
| **Navegación entre módulos** | Pierde dark mode | Mantiene dark mode |
| **Consola muestra logs** | No | Sí (`🔄 Aplicando modo guardado`) |

---

## 🧪 Testing

### Test 1: Navegación a módulo

```bash
# 1. Activar dark mode
Alpine.store('app').toggleMode('dark')

# 2. Verificar modo
Alpine.store('app').mode
# Resultado: "dark"

# 3. Navegar a /finanzas

# 4. Verificar que se mantiene dark
document.documentElement.classList.contains('dark')
# Resultado: true
```

### Test 2: Verificar logs de consola

Al navegar a un módulo, deberías ver:

```
🚀 Blazor enhancedload - Reaplicando modo...
🔄 Aplicando modo guardado: dark
🌓 Modo cambiado a: dark
```

### Test 3: Navegación múltiple

```
1. Dark mode activado
2. Navegar a /finanzas → Mantiene dark
3. Navegar a /prospectos → Mantiene dark
4. Navegar a / → Mantiene dark
5. ✅ Persistencia completa
```

---

## 📁 Archivos Modificados

### 1. `src/Host/VRM_Plugin.Blazor.Server/wwwroot/assets/js/main.js`

```diff
Alpine.store("app", {
    // ... propiedades existentes ...
    
+   // ✅ NUEVO: Método para aplicar el modo guardado
+   applyStoredMode() {
+       const savedMode = this.mode;
+       console.log('🔄 Aplicando modo guardado:', savedMode);
+       this.toggleMode(savedMode);
+ }
});

// ✅ Al inicializar Alpine, aplicar el modo guardado
+ console.log('🎨 Inicializando Alpine.js store...');
+ Alpine.store('app').applyStoredMode();

+ // ✅ NUEVO: Escuchar cambios de Blazor y reaplicar el modo
+ document.addEventListener('DOMContentLoaded', () => {
+     console.log('📄 DOM cargado, verificando estado de Alpine...');
+     
+     setTimeout(() => {
+         if (window.Alpine && Alpine.store && Alpine.store('app')) {
+             console.log('✅ Alpine detectado, aplicando modo guardado...');
+       Alpine.store('app').applyStoredMode();
+         }
+   }, 100);
+ });
```

### 2. `src/Host/VRM_Plugin.Blazor.Server/Components/App.razor`

```diff
+ // ✅ NUEVO: Escuchar eventos de navegación de Blazor
+ Blazor.addEventListener('enhancedload', () => {
+     console.log('🚀 Blazor enhancedload - Reaplicando modo...');
+     if (typeof Alpine !== 'undefined' && Alpine.store && Alpine.store('app')) {
+         Alpine.store('app').applyStoredMode();
+     }
+ });
```

---

## 🎯 Por qué funciona ahora

### Antes (Problema)

Alpine.js solo aplicaba el modo al **inicializar**, pero Blazor **recarga componentes** sin recargar Alpine.

### Ahora (Solución)

1. **Alpine se inicializa** → Aplica modo guardado
2. **DOM se carga** → Aplica modo guardado (por si acaso)
3. **Blazor navega** → Evento `enhancedload` → Aplica modo guardado
4. ✅ **Modo siempre sincronizado**

---

## 🔧 Mantenimiento

### Si agregas un nuevo módulo

**No necesitas hacer nada especial.** El listener de `enhancedload` funcionará automáticamente para todos los módulos con `@rendermode InteractiveServer`.

### Si quieres forzar la reaplicación manualmente

```javascript
// Desde la consola del navegador
Alpine.store('app').applyStoredMode()
```

---

## 📚 Eventos de Blazor

### Eventos disponibles

| Evento | Cuándo se dispara |
|--------|-------------------|
| `enhancedload` | **Después** de navegación o actualización |
| `enhancednavigation` | **Durante** navegación |
| `connected` | Conexión SignalR establecida |
| `reconnecting` | Intentando reconectar |

**Usamos `enhancedload`** porque se dispara **después** de que el componente se renderiza, momento perfecto para aplicar estilos.

---

## 🐛 Troubleshooting

### Problema: El modo sigue sin persistir

**Verificar:**

1. Abrir DevTools → Console
2. Buscar mensaje: `🚀 Blazor enhancedload - Reaplicando modo...`
3. Si NO aparece → Blazor no está disparando el evento

**Solución alternativa:**

```javascript
// Agregar en App.razor
setInterval(() => {
    if (window.Alpine && Alpine.store && Alpine.store('app')) {
  const currentMode = Alpine.store('app').mode;
 if (currentMode === 'dark' && !document.documentElement.classList.contains('dark')) {
            Alpine.store('app').applyStoredMode();
        }
  }
}, 500);
```

### Problema: Logs no aparecen en consola

**Verificar:**

1. `main.js` se cargó correctamente
2. Alpine.js está inicializado
3. Filtro de consola no está ocultando logs

**Comando de verificación:**

```javascript
window.Alpine && Alpine.store && Alpine.store('app')
// Debe retornar: { sidebar: false, mode: "dark", ... }
```

---

## ✅ Resultado Final

- ✅ Dark mode persiste al navegar a módulos
- ✅ Light mode funciona correctamente
- ✅ Logs de consola informativos
- ✅ Sincronización automática Blazor ↔ Alpine
- ✅ No requiere intervención manual

---

**Autor**: VRM Development Team  
**Fecha**: 2024  
**Estado**: ✅ Resuelto y Documentado
