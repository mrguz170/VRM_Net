/*
 * VRM Plugin Demo - Alpine.js Enhancement
 * Este archivo COMPLEMENTA el store 'app' definido en main.js
 * NO duplica el registro - solo añade funcionalidad extra
 */

console.log('?? VRM Init: Inicializando...');

// ==================== FUNCIÓN GLOBAL 'MAIN' ====================
// Define la función global 'main' que Alpine necesita
// NOTA: main.js ya registró el store 'app', no lo duplicamos aquí
window.main = function() {
    return {
        // Estado local del componente main
        isInitialized: false,
     
        // Función init que Alpine llama automáticamente
  init() {
        this.isInitialized = true;
      console.log('?? Main component inicializado');
        
   // Sincronizar con el store
            const store = Alpine.store('app');
   if (store && store.mode) {
      console.log(`?? Estado inicial del store: mode=${store.mode}, sidebar=${store.sidebar}`);
        }
      },
     
        // Método para refrescar el estado
        refresh() {
      console.log('?? Refrescando main component');
        }
    };
};

// ==================== MEJORAR EL STORE EXISTENTE ====================
document.addEventListener('alpine:init', () => {
    console.log('?? VRM Init: Mejorando Alpine store...');
    
    // Esperar un momento para que main.js registre el store primero
    setTimeout(() => {
    const store = Alpine.store('app');
        
        if (!store) {
            console.error('? El store "app" no existe. Asegúrate de que main.js se cargó primero.');
    return;
        }
     
        console.log('? Store "app" detectado, añadiendo métodos mejorados...');
   
        // Añadir método setMode mejorado si no existe
        if (!store.setMode) {
            store.setMode = function(mode) {
            this.mode = mode;
        localStorage.setItem('_x_mode', mode);
     
        // Aplicar clase dark al documento
   if (mode === 'dark') {
   document.documentElement.classList.add('dark');
  document.documentElement.setAttribute('data-mode', 'dark');
    } else {
       document.documentElement.classList.remove('dark');
         document.documentElement.setAttribute('data-mode', 'light');
        }
            
    console.log(`? Modo cambiado a: ${mode}`);
            };
        }
        
  // Añadir método setDirection mejorado si no existe
        if (!store.setDirection) {
      store.setDirection = function(dir) {
     this.direction = dir;
     localStorage.setItem('_x_direction', dir);
      document.documentElement.setAttribute('dir', dir);
            console.log(`? Dirección cambiada a: ${dir}`);
       };
 }
   
  // Añadir método setSidebarMode mejorado si no existe
      if (!store.setSidebarMode) {
store.setSidebarMode = function(mode) {
            this.sidebarMode = mode;
                localStorage.setItem('_x_sidebarMode', mode);
  console.log(`? Sidebar mode cambiado a: ${mode}`);
            };
    }
      
        // Añadir método setLayout mejorado si no existe
        if (!store.setLayout) {
store.setLayout = function(layout) {
            this.layout = layout;
         localStorage.setItem('_x_layout', layout);
                console.log(`? Layout cambiado a: ${layout}`);
         };
        }
        
    console.log('? VRM Init: Métodos mejorados añadidos al store "app"');
    }, 50); // Pequeño delay para que main.js se ejecute primero
});

// ==================== APLICAR CONFIGURACIÓN INICIAL ====================
document.addEventListener('alpine:initialized', () => {
    console.log('? Alpine completamente inicializado');
 
    // Aplicar modo guardado al DOM
    const savedMode = localStorage.getItem('_x_mode');
    if (savedMode) {
      if (savedMode === 'dark') {
            document.documentElement.classList.add('dark');
            document.documentElement.setAttribute('data-mode', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
        document.documentElement.setAttribute('data-mode', 'light');
        }
    }
    
// Aplicar dirección guardada
    const savedDir = localStorage.getItem('_x_direction');
if (savedDir) {
        document.documentElement.setAttribute('dir', savedDir);
    }
 
console.log('?? Configuración inicial aplicada');
});

// ==================== SINCRONIZACIÓN CON BLAZOR ====================
document.addEventListener('DOMContentLoaded', () => {
    console.log('?? DOM Cargado - VRM Init');
    
    // Re-aplicar configuración después de navegación Blazor
    setTimeout(() => {
        if (typeof Alpine !== 'undefined' && Alpine.store && Alpine.store('app')) {
            const savedMode = localStorage.getItem('_x_mode');
  const currentMode = Alpine.store('app').mode;
    
       if (savedMode && savedMode !== currentMode) {
            console.log(`?? Sincronizando modo: ${currentMode} ? ${savedMode}`);
                
      // Usar toggleMode si existe, sino actualizar directamente
   if (typeof Alpine.store('app').toggleMode === 'function') {
          Alpine.store('app').toggleMode(savedMode);
       } else if (typeof Alpine.store('app').setMode === 'function') {
  Alpine.store('app').setMode(savedMode);
     } else {
  Alpine.store('app').mode = savedMode;
      document.documentElement.setAttribute('data-mode', savedMode);
                }
      }
    }
    }, 200);
});

// ==================== FUNCIONES DE UTILIDAD ====================
// Función para verificar el estado del store (debugging)
window.checkAlpineStore = function() {
    if (typeof Alpine !== 'undefined' && Alpine.store && Alpine.store('app')) {
        const store = Alpine.store('app');
    console.log('?? Estado del Alpine store:', {
         mode: store.mode,
            direction: store.direction,
            sidebarMode: store.sidebarMode,
      sidebar: store.sidebar,
            layout: store.layout
        });
        return store;
    } else {
        console.error('? Alpine store no está disponible');
   return null;
    }
};

// Función para forzar actualización del modo
window.forceUpdateMode = function(mode) {
    if (typeof Alpine !== 'undefined' && Alpine.store && Alpine.store('app')) {
     if (typeof Alpine.store('app').toggleMode === 'function') {
            Alpine.store('app').toggleMode(mode);
     } else if (typeof Alpine.store('app').setMode === 'function') {
  Alpine.store('app').setMode(mode);
    } else {
       Alpine.store('app').mode = mode;
          document.documentElement.setAttribute('data-mode', mode);
            localStorage.setItem('_x_mode', mode);
        }
        console.log(`? Modo forzado a: ${mode}`);
} else {
        console.error('? No se puede actualizar el modo - Alpine no disponible');
    }
};

console.log('? VRM Init: Completado');
