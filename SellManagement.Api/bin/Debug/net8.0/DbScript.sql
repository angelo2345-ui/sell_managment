IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'SellManagementDb')
BEGIN
    CREATE DATABASE SellManagementDb
END
GO

USE SellManagementDb
GO

-- Tabla Productos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL
)
GO

-- Tabla Clientes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clients' AND xtype='U')
CREATE TABLE Clients (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20)
)
GO

-- Tabla Ventas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Sales' AND xtype='U')
CREATE TABLE Sales (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Date DATETIME DEFAULT GETDATE(),
    ClientId INT FOREIGN KEY REFERENCES Clients(Id),
    Total DECIMAL(18,2) NOT NULL
)
GO

-- Tabla Detalle de Ventas (Relación Venta-Producto)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SaleDetails' AND xtype='U')
CREATE TABLE SaleDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SaleId INT FOREIGN KEY REFERENCES Sales(Id),
    ProductId INT FOREIGN KEY REFERENCES Products(Id),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL
)
GO

-- Tabla Usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'User'
)
GO
