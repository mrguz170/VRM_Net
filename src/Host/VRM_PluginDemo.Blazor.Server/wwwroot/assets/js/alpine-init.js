/*
-------------------------------------------------------------------------
* VRM Plugin Demo - Alpine.js Store Initialization *
* Debe cargarse ANTES de alpine.min.js         *
*------------------------------------------------------------------------
*/

(function () {
    "use strict";

    //console.log('[alpine-init.js] 🚀 Preparando inicialización de Alpine.js...');

    // ✅ REGISTRAR LISTENER ANTES de que Alpine se cargue
    document.addEventListener("alpine:init", function() {
        //console.log('[alpine-init.js] 🎯 Evento alpine:init disparado');

        try {
            if (!window.Alpine) {
      console.error('[alpine-init.js] ❌ ERROR: Alpine no está disponible');
         return;
        }

  // ==================== COMPONENTES ====================

 // Componente: Collapse (Sidebar)
         Alpine.data("collapse", () => ({
     collapse: false,
 collapseSidebar() {
         //console.log('[Collapse] Toggle collapse:', !this.collapse);
          this.collapse = !this.collapse;
      },
  }));

    // Componente: Dropdown genérico
   Alpine.data("dropdown", (initialOpenState = false) => ({
              open: initialOpenState,
         toggle() {
          //console.log('[Dropdown] 🔽 Toggle - Estado:', this.open, '→', !this.open);
  this.open = !this.open;
           },
      close(focusAfter) {
             if (!this.open) return;
        this.open = false;
 focusAfter && focusAfter.focus();
         }
     }));

   // Componente: Modals (Settings)
            Alpine.data("modals", (initialOpenState = false) => ({
      open: initialOpenState,
            toggle() {
         //console.log('[Modals/Settings] ⚙️ Toggle - Estado:', this.open, '→', !this.open);
    this.open = !this.open;
         },
 close() {
              //console.log('[Modals/Settings] ❌ Close');
           this.open = false;
    }
        }));

            //console.log('[alpine-init.js] ✅ Componentes registrados: collapse, dropdown, modals');

       // ==================== STORE PRINCIPAL ====================

            Alpine.store("app", {
    // Estado
   sidebar: false,
          mode: Alpine.$persist('light').as('mode'),
                sidebarMode: Alpine.$persist('light').as('sidebarMode'),
                layout: Alpine.$persist('vertical').as('layout'),
                direction: Alpine.$persist('ltr').as('direction'),
    fullscreen: false,
             showSettings: false,
 hasCreative: false,
    hasdetached: false,

   // Métodos
      toggleSidebar() {
 //console.log('[Store.app] 🍔 toggleSidebar:', this.sidebar, '→', !this.sidebar);
            this.sidebar = !this.sidebar;
                  document.body.classList.toggle('toggle-sidebar');
           },

       toggleMode(val) {
          //console.log('[Store.app] 🌙 toggleMode - Recibido:', val);
             
if (!val) {
          val = this.mode || "light";
            }
            
        this.mode = val;
    
        // ✅ Aplicar clase 'dark' al <html>
        if (val === 'dark') {
             document.documentElement.classList.add('dark');
            //console.log('[Store.app] 🌙 Dark mode activado');
      } else {
    document.documentElement.classList.remove('dark');
        //console.log('[Store.app] ☀️ Light mode activado');
      }
            
  // ✅ NUEVO: Actualizar atributo data-mode (requerido por Tailwind)
      document.documentElement.setAttribute('data-mode', val);
 console.log('[Store.app] ✅ data-mode actualizado a:', val);
   },

         toggleFullScreen() {
     //console.log('[Store.app] 🖥️ toggleFullScreen');
            if (document.fullscreenElement) {
        document.exitFullscreen();
          } else {
  document.documentElement.requestFullscreen();
                    }
                },

    setLayout() {
      //console.log('[Store.app] ⚙️ setLayout');
        this.layout = this.layout || 'vertical';
    this.mode = this.mode || 'light';
   this.sidebarMode = this.sidebarMode || 'light';
this.direction = this.direction || 'ltr';
  
           //  console.log('[Store.app] ✅ Layout guardado:', {
           //layout: this.layout,
           //mode: this.mode,
           //sidebarMode: this.sidebarMode,
           //direction: this.direction
           // });
      },

           resetLayout() {
       //console.log('[Store.app] 🔄 resetLayout');
    this.layout = 'vertical';
  this.mode = 'light';
      this.sidebarMode = 'light';
           this.direction = 'ltr';
           document.documentElement.classList.remove('dark');
  //console.log('[Store.app] ✅ Layout reiniciado');
  }
            });

            //console.log('[alpine-init.js] ✅ Store "app" registrado con Alpine.$persist');

            // ==================== SIDEBAR MENU ====================

            Alpine.data('sidebarMenu', () => ({
        init() {
   setTimeout(() => {
    this.setActiveClass();
          }, 0);
     },
  setActiveClass() {
        let currentPath = window.location.pathname;
        let activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');
            
console.log('[SidebarMenu] Ruta actual:', currentPath);
    
  if (activeItem) {
        activeItem.classList.add('active');
         console.log('[SidebarMenu] ✅ Item activo:', activeItem.textContent?.trim());
            } else {
 currentPath = currentPath.substring(currentPath.lastIndexOf('/') + 1);
       activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');
      if (activeItem) {
 activeItem.classList.add('active');
    //console.log('[SidebarMenu] ✅ Item activo (fallback):', activeItem.textContent?.trim());
    }
      }
    }
     }));

      //console.log('[alpine-init.js] ✅ Componente "sidebarMenu" registrado');

      } catch (error) {
       console.error('[alpine-init.js] ❌ ERROR al registrar componentes:', error);
        }
 });

    // ==================== ALPINE INITIALIZED ====================

    document.addEventListener('alpine:initialized', function() {
        console.log('[alpine-init.js] 🎉 Alpine.js COMPLETAMENTE INICIALIZADO');
        
        try {
            const store = Alpine.store('app');
    
            if (!store) {
       console.error('[alpine-init.js] ❌ Store "app" no existe');
       return;
    }

 //  console.log('[alpine-init.js] 📊 Estado inicial del store:', {
 //mode: store.mode,
 //          sidebarMode: store.sidebarMode,
 //   layout: store.layout,
 //       direction: store.direction,
 //   sidebar: store.sidebar
 // });

        // ✅ APLICAR dark mode Y data-mode según el store
   if (store.mode === 'dark') {
     document.documentElement.classList.add('dark');
      document.documentElement.setAttribute('data-mode', 'dark');
           //console.log('[alpine-init.js] 🌙 Dark mode aplicado desde persistencia');
 } else {
        document.documentElement.classList.remove('dark');
      document.documentElement.setAttribute('data-mode', 'light');
      //console.log('[alpine-init.js] ☀️ Light mode aplicado desde persistencia');
}

 } catch (error) {
          console.error('[alpine-init.js] ❌ ERROR en alpine:initialized:', error);
        }
    });

    //console.log('[alpine-init.js] ✅ Listeners de Alpine configurados');

})();
