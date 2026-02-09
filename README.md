# Sistema de Gestión de Ventas (Sell Management System)

Este proyecto es una aplicación completa de gestión de ventas, diseñada con una arquitectura moderna de capas separadas. Combina un backend robusto en **.NET 8** con un frontend reactivo en **Vue 3**, permitiendo administrar productos, clientes y transacciones de venta en tiempo real.

# Características Principales

*   **Gestión de Inventario**: CRUD completo de productos con control de stock.
*   **Cartera de Clientes**: Administración de información de clientes.
*   **Terminal Punto de Venta (POS)**: Interfaz ágil para registrar ventas con cálculo automático de totales y validación de stock.
*   **Historial de Transacciones**: Registro detallado de todas las ventas realizadas.
*   **Autenticación Segura**: Sistema de login basado en JWT (JSON Web Tokens).
*   **Diseño Corporativo**: Interfaz profesional siguiendo la identidad visual DGA (Azul/Verde).

# Tecnologías Utilizadas

### Backend (.NET 8 API)
*   **C# / .NET 8**: Plataforma de desarrollo.
*   **SQL Server**: Motor de base de datos.
*   **ADO.NET**: Acceso a datos de alto rendimiento con transacciones SQL.
*   **Swagger/OpenAPI**: Documentación interactiva de la API.
*   **JWT Bearer**: Seguridad y autenticación.

### Frontend (Vue 3)
*   **Vue.js 3 (Composition API)**: Framework de JavaScript.
*   **Vite**: Entorno de desarrollo ultrarrápido.
*   **Axios**: Cliente HTTP para comunicación con la API.
*   **CSS3 Variables**: Diseño responsivo y personalizable.

---

## Endpoints de la API

La API está documentada con Swagger. Una vez ejecutado el backend, puedes visitar `http://localhost:5227/swagger` para probarlos.

### Autenticación (`/api/auth`)
| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `POST` | `/register` | Registra un nuevo usuario en el sistema. |
| `POST` | `/login` | Inicia sesión y devuelve un token JWT. |

### Productos (`/api/products`)
| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `GET` | `/` | Obtiene la lista completa de productos. |
| `POST` | `/` | Crea un nuevo producto. |
| `PUT` | `/{id}` | Actualiza un producto existente. |
| `DELETE` | `/{id}` | Elimina un producto (si no tiene ventas). |

### Clientes (`/api/clients`)
| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `GET` | `/` | Obtiene la lista de clientes. |
| `POST` | `/` | Registra un nuevo cliente. |
| `PUT` | `/{id}` | Actualiza los datos de un cliente. |
| `DELETE` | `/{id}` | Elimina un cliente. |

### Ventas (`/api/sales`)
| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `POST` | `/` | **Crear Venta**: Transacción compleja que guarda la venta, sus detalles y descuenta el stock automáticamente. |
| `GET` | `/` | **Historial**: Obtiene todas las ventas con detalles de productos y clientes. |

---

## Instalación y Configuración

### Prerrequisitos
*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) (v16 o superior)
*   SQL Server (LocalDB o instancia completa)

### 1. Configurar el Backend
1.  Navega a la carpeta del API:
    ```bash
    cd SellManagement.Api
    ```
2.  Configura tu cadena de conexión en `appsettings.json` (si es diferente a la predeterminada).
3.  Ejecuta el proyecto (La base de datos se creará automáticamente):
    ```bash
    dotnet run
    ```
    *El backend iniciará en `http://localhost:5227`*

### 2. Configurar el Frontend
1.  Abre una nueva terminal y navega a la carpeta del frontend:
    ```bash
    cd vue-project
    ```
2.  Instala las dependencias:
    ```bash
    npm install
    ```
3.  Inicia el servidor de desarrollo:
    ```bash
    npm run dev
    ```
4.  Abre tu navegador en la URL que aparece (generalmente `http://localhost:5173`).

### 3. Ejecutar Pruebas Unitarias
El proyecto incluye un conjunto de pruebas unitarias para validar la lógica del backend automáticamente.

1.  Navega a la carpeta de pruebas:
    ```bash
    cd SellManagement.Tests
    ```
2.  Ejecuta los tests:
    ```bash
    dotnet test
    ```
3.  Verás un reporte en consola indicando cuántas pruebas pasaron.

---

## Credenciales de Prueba (Demo)

Para acceder al sistema en modo demostración, utiliza las siguientes credenciales en la pantalla de login:

*   **Usuario/Email**: `admin@test.com`
*   **Contraseña**: `123456`

---

## Estructura del Proyecto

```
sell_managment/
├── SellManagement.Api/       # Backend .NET Core
│   ├── Controllers/          # Controladores de API
│   ├── Models/               # Modelos de datos y DTOs
│   ├── Services/             # Lógica de acceso a datos
│   └── DbScript.sql          # Script de inicialización de BD
│
├── vue-project/              # Frontend Vue.js
│   ├── src/
│   │   ├── components/       # Componentes Vue (Vistas y Formularios)
│   │   ├── services/         # Servicios de comunicación con API
│   │   └── assets/           # Estilos CSS y recursos
```
