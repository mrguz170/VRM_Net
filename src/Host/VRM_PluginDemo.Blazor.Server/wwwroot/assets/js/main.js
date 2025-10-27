/*
-------------------------------------------------------------------------
* Template Name : Sliced - Tailwind CSS Admin & Dashboard Template   * 
* Author           : SRBThemes     *
* Version      : 1.0.0          *
* Created: May 2024    *
* File Description : Main Js file of the template      *
*------------------------------------------------------------------------
*/

(function () {
    ("use strict");

    console.log('?? Main.js cargando...');
    
    // ? CRÍTICO: Registrar el store 'app' ANTES de que Alpine se inicie
    // No usamos 'alpine:init' porque queremos que esté disponible INMEDIATAMENTE
    
    if (typeof window.alpineReadyCallbacks === 'undefined') {
        window.alpineReadyCallbacks = [];
    }

    // Función para registrar el store inmediatamente
    function registerAlpineStore() {
if (typeof Alpine === 'undefined') {
  console.error('? Alpine no está disponible para registrar el store');
      return;
        }

        console.log('?? Registrando Alpine store "app"...');
        
        Alpine.data("collapse", () => ({
            collapse: false,
          collapseSidebar() {
              this.collapse = !this.collapse;
            },
        }));
        
        Alpine.data("dropdown", (initialOpenState = false) => ({
            open: initialOpenState,
   toggle() {
          this.open = !this.open;
},
        }));
   
        Alpine.data("modals", (initialOpenState = false) => ({
     open: initialOpenState,
 toggle() {
   this.open = !this.open;
            },
        }));

        // main - custom functions
        Alpine.data("main", (value) => { });

        Alpine.store("app", {
            // sidebar
            sidebar: false,
    toggleSidebar() {
                this.sidebar = !this.sidebar;
         },
       // Light and dark Mode
mode: Alpine.$persist('light'),
            sidebarMode: Alpine.$persist('light'),
            layout: Alpine.$persist('vertical'),
            direction: Alpine.$persist('ltr'),
 showSettings: false,
            toggleMode(val) {
      if (!val) {
    val = this.mode || "light"; // light And Dark
         }
              this.mode = val;
      },

   toggleFullScreen() {
                if (document.fullscreenElement) {
   document.exitFullscreen();
     } else {
        document.documentElement.requestFullscreen();
      }
      },

          setLayout() {
      // Set the layout based on current settings
          this.layout = this.layout || 'vertical';
       this.mode = this.mode || 'light';
                this.sidebarMode = this.sidebarMode || 'light';
    this.direction = this.direction || 'ltr';
    this.open = false;
       },

            resetLayout() {
 // Reset to default layout settings
     this.layout = 'vertical';
           this.mode = 'light';
    this.sidebarMode = 'light';
     this.direction = 'ltr';
                this.open = false;
    }
  });

        console.log('? Store "app" registrado exitosamente');
    }

    // ? CRÍTICO: Escuchar el evento alpine:init para registrar el store
    document.addEventListener("alpine:init", () => {
        console.log('?? Alpine:init event - Registrando stores y componentes...');
     registerAlpineStore();

  // sidebar menu activation
        const activeMenuFromStorage = localStorage.getItem('activeMenu');
        const activeMenu = activeMenuFromStorage ? activeMenuFromStorage : '';

        function setActiveClass() {
         var currentPath = window.location.pathname;
       // Extract the last part of the path (to handle directories)
   var activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');
            console.log("setActiveClass", activeItem, 'currentPath', currentPath)
            if (activeItem) {
        activeItem.classList.add('active');
            } else {
    currentPath = currentPath.substring(currentPath.lastIndexOf('/') + 1);
       var activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');
        activeItem?.classList.add('active');
            }
        }

        Alpine.data('sidebarMenu', () => ({
            init() {
                setTimeout(() => {
   setActiveClass();
      }, 0);
          setActiveClass();
            },
 isActive(menu) {
       return this.$store.sidebar?.activeMenu === menu;
            },
            toggle(menu) {
          this.$store.sidebar?.toggleMenu(menu);
     }
        }));
        
        console.log('? Todos los stores y componentes registrados');
    });

    console.log('? Main.js cargado completamente');

})();

/* === Asegurar arranque de Alpine de forma segura === */
(function ensureStartAlpine() {
 const tryStart = () => {
 if (typeof Alpine === 'undefined') return setTimeout(tryStart,50);
 // Esperar a que window.main exista (tu componente x-data="main")
 if (typeof window.main !== 'function') return setTimeout(tryStart,50);

 // Evitar arranques múltiples
 if (window.__alpine_started) {
 console.info('?? Alpine ya fue iniciado anteriormente');
 return;
 }

 try {
 Alpine.start();
 window.__alpine_started = true;
 console.info('? Alpine iniciado automáticamente desde main.js');
 } catch (err) {
 console.error('? Error al iniciar Alpine:', err);
 setTimeout(tryStart,200);
 }
 };

 if (document.readyState === 'loading') {
 document.addEventListener('DOMContentLoaded', tryStart);
 } else {
 tryStart();
 }
})();