/*
-------------------------------------------------------------------------
* VRM Plugin Demo - Alpine.js Initialization  *
* Based on Sliced Template by SRBThemes    *
* Modified for Blazor Server with enhanced debugging        *
*------------------------------------------------------------------------
*/

(function () {
    ("use strict");

    console.log('[main.js] ?? Iniciando configuración de Alpine.js...');

    document.addEventListener("alpine:init", () => {
        console.log('[main.js] ?? Evento alpine:init disparado');

        // ? COMPONENTE: Collapse (Sidebar)
        Alpine.data("collapse", () => ({
        collapse: false,
            collapseSidebar() {
        console.log('[Collapse] Toggle sidebar collapse:', !this.collapse);
           this.collapse = !this.collapse;
     },
        }));

        // ? COMPONENTE: Dropdown genérico
        Alpine.data("dropdown", (initialOpenState = false) => ({
     open: initialOpenState,
        toggle() {
    console.log('[Dropdown] Toggle - Estado actual:', this.open);
     this.open = !this.open;
     console.log('[Dropdown] Toggle - Nuevo estado:', this.open);
        },
        }));

        // ? COMPONENTE: Modals (Settings)
        Alpine.data("modals", (initialOpenState = false) => ({
            open: initialOpenState,
      toggle() {
  console.log('[Modals/Settings] ?? Toggle LLAMADO - Estado actual:', this.open);
        this.open = !this.open;
             console.log('[Modals/Settings] ? Toggle COMPLETADO - Nuevo estado:', this.open);
            },
        }));

        // main - custom functions
        Alpine.data("main", (value) => { });

        console.log('[main.js] ? Componentes Alpine registrados: collapse, dropdown, modals');

     // ? STORE PRINCIPAL con Alpine.$persist
   Alpine.store("app", {
       // Sidebar
     sidebar: false,
            toggleSidebar() {
                console.log('[Store.app] ?? toggleSidebar() - Estado actual:', this.sidebar);
                this.sidebar = !this.sidebar;
       console.log('[Store.app] ? toggleSidebar() - Nuevo estado:', this.sidebar);
  },

// ? PERSISTENCIA: Light and dark Mode
         mode: Alpine.$persist('light'),
            sidebarMode: Alpine.$persist('light'),
            layout: Alpine.$persist('vertical'),
direction: Alpine.$persist('ltr'),
     showSettings: false,

       toggleMode(val) {
    console.log('[Store.app] ?? toggleMode() - Valor recibido:', val);
                
                if (!val) {
    val = this.mode || "light";
          }
    
           this.mode = val;
    console.log('[Store.app] ? toggleMode() - Modo aplicado:', this.mode);

            },

            toggleFullScreen() {
       console.log('[Store.app] ??? toggleFullScreen() LLAMADO');
                if (document.fullscreenElement) {
              document.exitFullscreen();
     console.log('[Store.app] ??? Saliendo de fullscreen');
  } else {
        document.documentElement.requestFullscreen();
        console.log('[Store.app] ??? Entrando a fullscreen');
         }
  },

            setLayout() {
                console.log('[Store.app] ?? setLayout() LLAMADO');
           
              // Set the layout based on current settings
                this.layout = this.layout || 'vertical';
                this.mode = this.mode || 'light';
                this.sidebarMode = this.sidebarMode || 'light';
                this.direction = this.direction || 'ltr';
                this.open = false;

                console.log('[Store.app] ? setLayout() - Configuración guardada:', {
                    layout: this.layout,
                   mode: this.mode,
                   sidebarMode: this.sidebarMode,
                   direction: this.direction
                   });
     },

         resetLayout() {
            console.log('[Store.app] ?? resetLayout() LLAMADO'); 
            // Reset to default layout settings
            this.layout = 'vertical';
            this.mode = 'light';
            this.sidebarMode = 'light';
            this.direction = 'ltr';
            this.open = false;

            // Remover dark mode
            //document.documentElement.classList.remove('dark');

            console.log('[Store.app] ? resetLayout() - Configuración reiniciada');
  }
        });

        // sidebar menu activation
        const activeMenuFromStorage = localStorage.getItem('activeMenu');
        const activeMenu = activeMenuFromStorage ? activeMenuFromStorage : '';



    console.log('[main.js] ? Store "app" registrado con Alpine.$persist');

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
                    this.setActiveClass();
                }, 0);
            },
            setActiveClass() {
                var currentPath = window.location.pathname;
                var activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');

                console.log('[SidebarMenu] Ruta actual:', currentPath);

                if (activeItem) {
                    activeItem.classList.add('active');
                    console.log('[SidebarMenu] ✅ Item activo:', activeItem.textContent?.trim());
                } else {
                    currentPath = currentPath.substring(currentPath.lastIndexOf('/') + 1);
                    activeItem = document.querySelector('.sidebar ul li a[href="' + currentPath + '"]');
                    if (activeItem) {
                        activeItem.classList.add('active');
                        console.log('[SidebarMenu] ✅ Item activo (fallback):', activeItem.textContent?.trim());
                    }
                }
            }
        }));

        console.log('[main.js] ? Componente "sidebarMenu" registrado');
    });

    // ? LISTENER: Alpine inicializado
    document.addEventListener('alpine:initialized', () => {
        console.log('[main.js] 🎉 Alpine.js COMPLETAMENTE INICIALIZADO');

        const store = Alpine.store('app');
        console.log('[main.js] 📊 Estado inicial del store:', {
            mode: store.mode,
            sidebarMode: store.sidebarMode,
            layout: store.layout,
            direction: store.direction,
            sidebar: store.sidebar
        });

        // ✅ NUEVO: Exponer Alpine globalmente para debugging
        window.AlpineDebug = {
            store: () => Alpine.store('app'),
            toggleSidebar: () => Alpine.store('app').toggleSidebar(),
            toggleFullScreen: () => Alpine.store('app').toggleFullScreen()
        };

        console.log('[main.js] ✅ Alpine expuesto en window.AlpineDebug');
    });

    console.log('[main.js] ? Listeners de Alpine configurados');

})();