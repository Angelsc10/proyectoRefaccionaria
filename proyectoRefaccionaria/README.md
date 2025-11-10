*Recommended Markdown Viewer: [Markdown Editor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor2)*

## Getting Started

Browse and address `TODO:` comments in `View -> Task List` to learn the codebase and understand next steps for turning the generated code into production code.

Explore the [WinUI Gallery](https://www.microsoft.com/store/productId/9P3JFPWWDZRC) to learn about available controls and design patterns.

Relaunch Template Studio to modify the project by right-clicking on the project in `View -> Solution Explorer` then selecting `Add -> New Item (Template Studio)`.

## Publishing

For projects with MSIX packaging, right-click on the application project and select `Package and Publish -> Create App Packages...` to create an MSIX package.

For projects without MSIX packaging, follow the [deployment guide](https://docs.microsoft.com/windows/apps/windows-app-sdk/deploy-unpackaged-apps) or add the `Self-Contained` Feature to enable xcopy deployment.

## CI Pipelines

See [README.md](https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/pipelines/README.md) for guidance on building and testing projects in CI pipelines.

## Changelog

See [releases](https://github.com/microsoft/TemplateStudio/releases) and [milestones](https://github.com/microsoft/TemplateStudio/milestones).

## Feedback

Bugs and feature requests should be filed at https://aka.ms/templatestudio.

# Punto de Venta para Refaccionaria (WinUI 3)

Un proyecto de aplicación de escritorio completo para Windows, construido con WinUI 3 y C#, que simula un Punto de Venta (POS) y un sistema de gestión de inventario para una tienda de refacciones automotrices.

![]

---

## 🚀 Características Principales

Este proyecto va más allá de un simple CRUD y presenta un conjunto de características de nivel profesional:

### 1. Sistema de Autenticación
* **Login Seguro:** El inicio de sesión ya no está "hard-coded". Los usuarios se validan contra una tabla `Usuarios` en la base de datos.
* **Roles de Usuario:** El sistema diferencia entre "admin" y "usuario", mostrando la ventana correspondiente para cada rol.

### 2. Módulo de Administrador
El panel de administrador es el centro de control de la tienda.
* **Gestión de Inventario (CRUD):** Funcionalidad completa para Crear, Leer, Editar y Eliminar refacciones de la base de datos.
* **Gestión de Usuarios:** El admin puede crear y eliminar cuentas de usuario (admin/usuario) directamente desde la aplicación.
* **Reporte de Ventas:** Una ventana dedicada que muestra un historial de todas las ventas. Permite seleccionar un "ticket" de venta y ver un detalle de los productos vendidos en esa transacción.
* **Categorización:** Sistema para asignar categorías a los productos (ej. "Frenos", "Motor").

### 3. Módulo de Punto de Venta (POS)
La interfaz principal para el empleado/usuario.
* **Catálogo Filtrable:** Muestra todos los productos y permite filtrarlos por categoría.
* **Carrito de Compras:** Un carrito de compras funcional para añadir o quitar productos.
* **Validación de Stock en Tiempo Real:** La aplicación comprueba el stock de la base de datos *antes* de añadir un producto al carrito y una vez más *antes* de confirmar la compra, previniendo ventas de productos agotados.
* **Integridad de Datos:** Al confirmar una compra, el sistema usa **Transacciones SQL** para asegurar que el registro de la venta y la actualización del stock ocurran juntos. Si algo falla, la operación se revierte, garantizando que la base de datos nunca quede inconsistente.

### 4. Diseño y UI/UX
* **UI Moderna:** Toda la aplicación utiliza los efectos de diseño de Windows 11, como el fondo **Mica**.
* **Layouts Profesionales:** Se reemplazaron los `StackPanel` básicos por layouts de `Grid` para alinear formularios y crear interfaces complejas, como el POS de 2 columnas.
* **Controles Avanzados:** Se utiliza el control `DataGrid` del Community Toolkit para mostrar listas de datos (refacciones, ventas, usuarios) que se pueden ordenar.
* **Prevención de Errores:** Se implementó lógica para prevenir errores comunes, como el crasheo por doble clic en el botón de login.

---

## 🛠️ Tecnologías Utilizadas

* **Framework:** WinUI 3 (Windows App SDK)
* **Lenguaje:** C#
* **Base de Datos:** MySQL
* **Librerías Adicionales:**
    * `WinUIEx`: Para la gestión avanzada de ventanas y el efecto Mica.
    * `CommunityToolkit.WinUI.UI.Controls.DataGrid`: Para las cuadrículas de datos profesionales.
    * `MySql.Data`: El conector oficial para la comunicación con la base de datos.

---

## 📦 Instalación y Configuración

1.  **Clonar el Repositorio:**
    ```sh
    git clone [URL_DE_TU_REPOSITORIO]
    ```
2.  **Base de Datos:**
    * Asegúrate de tener un servidor MySQL local (como XAMPP o MySQL Workbench).
    * Crea una base de datos llamada `refaccionaria`.
    * Ejecuta los scripts SQL (que puedes añadir a tu repositorio) para crear las tablas: `spareparts`, `Ventas`, `DetalleVenta`, y `Usuarios`.
    * Inserta los usuarios iniciales (`admin` y `usuario`).
3.  **Configurar Conexión:**
    * Abre el archivo `MySqlHelper.cs`.
    * Modifica la variable `connectionString` con tu usuario y contraseña de MySQL:
        ```csharp
        private static string connectionString = "server=localhost;database=refaccionaria;user=root;password=TU_CONTRASEÑA;";
        ```
4.  **Ejecutar:**
    * Abre el archivo `.sln` con Visual Studio 2022.
    * Restaura los paquetes NuGet.
    * Compila y ejecuta el proyecto.