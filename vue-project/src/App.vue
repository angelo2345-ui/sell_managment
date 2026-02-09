<script setup>
import { ref, computed } from 'vue';
import ProductList from './components/ProductList.vue';
import ProductForm from './components/ProductForm.vue';
import ClientList from './components/ClientList.vue';
import ClientForm from './components/ClientForm.vue';
import SaleForm from './components/SaleForm.vue';
import SaleList from './components/SaleList.vue';
import Login from './components/Login.vue';

// Estado de autenticación
const isAuthenticated = ref(false);

// Navegación
const currentView = ref('products');
const isSidebarOpen = ref(false);

// Claves para recarga
const productKey = ref(0);
const clientKey = ref(0);

const reloadProducts = () => productKey.value++;
const reloadClients = () => clientKey.value++;

// Función para cerrar sidebar en móvil al navegar
const navigateTo = (view) => {
  currentView.value = view;
  isSidebarOpen.value = false; // Cerrar menú en móvil
};

const pageTitle = computed(() => {
  const titles = {
    'products': 'Inventario de Productos',
    'clients': 'Cartera de Clientes',
    'sales': 'Nueva Venta',
    'sale-history': 'Historial de Ventas'
  };
  return titles[currentView.value] || 'VentasApp';
});

const logout = () => {
  isAuthenticated.value = false;
  localStorage.removeItem('authToken');
};
</script>

<template>
  <!-- LOGIN VIEW (Pantalla completa) -->
  <div v-if="!isAuthenticated" class="login-wrapper">
    <Login @login-success="isAuthenticated = true" />
  </div>

  <!-- DASHBOARD LAYOUT -->
  <div v-else class="dashboard-layout">
    
    <!-- SIDEBAR (Menú Lateral) -->
    <aside class="sidebar" :class="{ 'sidebar-open': isSidebarOpen }">
      <div class="sidebar-header">
        <div class="brand-logo">
          <span class="dga">DGA</span>
          <span class="aduanas">ADUANAS</span>
        </div>
        <button class="close-btn-mobile" @click="isSidebarOpen = false">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
        </button>
      </div>
      
      <nav class="sidebar-nav">
        <a href="#" 
           @click.prevent="navigateTo('products')" 
           :class="{ active: currentView === 'products' }">
           <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path><polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline><line x1="12" y1="22.08" x2="12" y2="12"></line></svg>
           Productos
        </a>
        <a href="#" 
           @click.prevent="navigateTo('clients')" 
           :class="{ active: currentView === 'clients' }">
           <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M23 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path></svg>
           Clientes
        </a>
        <a href="#" 
           @click.prevent="navigateTo('sales')" 
           :class="{ active: currentView === 'sales' }">
           <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="9" cy="21" r="1"></circle><circle cx="20" cy="21" r="1"></circle><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"></path></svg>
           Nueva Venta
        </a>
        <a href="#" 
           @click.prevent="navigateTo('sale-history')" 
           :class="{ active: currentView === 'sale-history' }">
           <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path><polyline points="14 2 14 8 20 8"></polyline><line x1="16" y1="13" x2="8" y2="13"></line><line x1="16" y1="17" x2="8" y2="17"></line><polyline points="10 9 9 9 8 9"></polyline></svg>
           Historial
        </a>
      </nav>

      <div class="sidebar-footer">
        <button @click="logout" class="btn-logout">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
          Cerrar Sesión
        </button>
      </div>
    </aside>

    <!-- OVERLAY (Fondo oscuro en móvil) -->
    <div v-if="isSidebarOpen" class="sidebar-overlay" @click="isSidebarOpen = false"></div>

    <!-- MAIN CONTENT -->
    <main class="main-content">
      <!-- TOP BAR -->
      <header class="top-bar">
        <div class="top-bar-left">
          <button class="menu-btn" @click="isSidebarOpen = !isSidebarOpen">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="3" y1="12" x2="21" y2="12"></line><line x1="3" y1="6" x2="21" y2="6"></line><line x1="3" y1="18" x2="21" y2="18"></line></svg>
          </button>
          <h3 class="page-title-mobile">
            {{ pageTitle }}
          </h3>
        </div>

        <div class="top-bar-center-logo">
          <img src="/imagenes/dga-logo.jpeg" alt="DGA Aduanas" class="top-bar-logo-img" />
        </div>

        <div class="user-profile">
          <span>Admin</span>
          <div class="avatar">A</div>
        </div>
      </header>

      <!-- CONTENT AREA -->
      <div class="content-wrapper">
        
        <!-- VISTA PRODUCTOS -->
        <transition name="fade" mode="out-in">
          <div v-if="currentView === 'products'" key="products">
            <div class="page-header">
              <h2 class="page-title">Gestión de Productos</h2>
            </div>
            
            <div class="grid-layout">
              <div class="grid-col-form">
                <ProductForm @product-created="reloadProducts" />
              </div>
              <div class="grid-col-list">
                <ProductList :key="productKey" />
              </div>
            </div>
          </div>

          <!-- VISTA CLIENTES -->
          <div v-else-if="currentView === 'clients'" key="clients">
            <div class="page-header">
              <h2 class="page-title">Gestión de Clientes</h2>
            </div>
            
            <div class="grid-layout">
              <div class="grid-col-form">
                <ClientForm @client-created="reloadClients" />
              </div>
              <div class="grid-col-list">
                <ClientList :key="clientKey" />
              </div>
            </div>
          </div>

          <!-- VISTA VENTAS -->
          <div v-else-if="currentView === 'sales'" key="sales">
            <div class="page-header">
              <h2 class="page-title">Terminal de Venta (POS)</h2>
            </div>
            <SaleForm @sale-completed="reloadProducts" />
          </div>

          <!-- VISTA HISTORIAL VENTAS -->
          <div v-else-if="currentView === 'sale-history'" key="sale-history">
            <div class="page-header">
              <h2 class="page-title">Historial de Transacciones</h2>
            </div>
            <SaleList />
          </div>
        </transition>

      </div>
    </main>
  </div>
</template>

<style scoped>
/* --- LAYOUT STRUCTURE --- */
.dashboard-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--bg-body);
}

.login-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(135deg, var(--primary) 0%, #004080 100%);
}

/* --- SIDEBAR --- */
.sidebar {
  width: 260px;
  background-color: var(--bg-sidebar);
  color: white;
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  bottom: 0;
  left: 0;
  z-index: 50;
  transition: transform 0.3s ease;
  box-shadow: 4px 0 10px rgba(0,0,0,0.1);
}

.sidebar-header {
  padding: 1.5rem;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand-logo {
  font-family: var(--font-serif);
  font-size: 1.5rem;
  font-weight: 700;
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
.brand-logo .dga {
  color: #fff;
}
.brand-logo .aduanas {
  color: var(--secondary); /* Cyan */
}

.sidebar-nav {
  flex: 1;
  padding: 1.5rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.sidebar-nav a {
  color: rgba(255,255,255,0.7);
  text-decoration: none;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-weight: 500;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 12px;
}

.sidebar-nav a:hover, .sidebar-nav a.active {
  background-color: rgba(255,255,255,0.1);
  color: white;
  border-left: 4px solid var(--secondary);
}

.sidebar-footer {
  padding: 1.5rem;
  border-top: 1px solid rgba(255,255,255,0.1);
}

.btn-logout {
  width: 100%;
  background: rgba(255,255,255,0.1);
  color: white;
  border: none;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}
.btn-logout:hover {
  background: rgba(255,255,255,0.2);
}

/* --- MAIN CONTENT --- */
.main-content {
  flex: 1;
  margin-left: 260px; /* Ancho del sidebar */
  display: flex;
  flex-direction: column;
  width: calc(100% - 260px);
}

.top-bar {
  background: white;
  height: 64px;
  padding: 0 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: var(--shadow-sm);
  position: sticky;
  top: 0;
  z-index: 40;
}

.menu-btn {
  display: none; /* Oculto en desktop */
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-main);
  padding: 0.5rem;
}

.close-btn-mobile {
  display: none; /* Oculto en desktop */
}

.top-bar-left {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex: 1;
}

.top-bar-center-logo {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.top-bar-logo-img {
  max-height: 48px;
  width: auto;
  object-fit: contain;
}

.user-profile {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex: 1;
  gap: 10px;
  font-weight: 600;
}

.avatar {
  width: 36px;
  height: 36px;
  background-color: var(--primary);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

.content-wrapper {
  padding: 2rem;
  flex: 1;
  overflow-y: auto;
}

/* --- GRID SYSTEM FOR DASHBOARD --- */
.grid-layout {
  display: grid;
  grid-template-columns: 350px 1fr; /* Formulario fijo, Lista flexible */
  gap: 2rem;
  align-items: start;
}

/* Fix for Grid Item Overflow */
.grid-col-form, .grid-col-list {
  min-width: 0;
}

/* --- ANIMATIONS --- */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* --- RESPONSIVE DESIGN (MOBILE) --- */
@media (max-width: 1024px) {
  .grid-layout {
    grid-template-columns: 1fr; /* Stack vertical en tablets */
  }
}

@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
    box-shadow: none;
  }
  
  .sidebar.sidebar-open {
    transform: translateX(0);
    box-shadow: 0 0 20px rgba(0,0,0,0.5);
  }

  .main-content {
    margin-left: 0;
    width: 100%;
    padding-top: 64px; /* Altura del top-bar fijo */
  }

  .top-bar {
    padding: 0 1rem;
    left: 0;
    width: 100%;
    position: fixed;
  }

  .menu-btn {
    display: block;
  }
  
  .close-btn-mobile {
    display: block; /* Visible en móvil */
    background: none;
    border: none;
    color: white;
    cursor: pointer;
    padding: 0.5rem;
  }

  .content-wrapper {
    padding: 1rem;
    overflow-x: hidden; /* Evitar scroll horizontal global */
  }

  /* Ajustes para tablas en móviles */
  .table-responsive {
    display: block;
    width: 100%;
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    margin-bottom: 1rem;
  }
  
  /* Ajustes generales para evitar cortes */
  .card {
    padding: 1rem;
    margin-bottom: 1rem;
    overflow: hidden; /* Contener desbordamientos internos */
  }

  .sidebar-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0,0,0,0.5);
    z-index: 45;
  }
  
  .page-title {
    font-size: 1.25rem;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .page-header button {
    width: 100%;
    justify-content: center;
  }
}
</style>