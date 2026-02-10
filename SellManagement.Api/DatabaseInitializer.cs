using Microsoft.Data.SqlClient;

namespace SellManagement.Api
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            // Primero nos conectamos a 'master' para crear la base de datos si no existe
            var builder = new SqlConnectionStringBuilder(connectionString);
            string originalDb = builder.InitialCatalog;
            builder.InitialCatalog = "master"; // Cambiamos a master temporalmente

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                // 1. Crear la BD si no existe
                var checkDbCmd = new SqlCommand($"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{originalDb}') CREATE DATABASE {originalDb}", connection);
                checkDbCmd.ExecuteNonQuery();
            }

            // Ahora nos conectamos a la base de datos correcta para crear las tablas
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Leer el script SQL
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbScript.sql");
                
                // Si no está en bin, buscar en la raíz del proyecto (para desarrollo)
                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "DbScript.sql");
                }

                if (File.Exists(scriptPath))
                {
                    string script = File.ReadAllText(scriptPath);
                    
                    // Separar por GO porque SQL Server no permite ejecutar todo el bloque junto en un solo comando desde C#
                    // (Nota: Esta es una implementación simple, 'GO' debe estar en su propia línea)
                    var commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var commandText in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(commandText))
                        {
                            try 
                            {
                                var command = new SqlCommand(commandText, connection);
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error ejecutando parte del script: {ex.Message}");
                            }
                        }
                    }

                    // --- MIGRACIONES MANUALES ---
                    // 1. Agregar columna IsActive a Products si no existe (Soft Delete)
                    try 
                    {
                        string migrationProductsSql = @"
                            IF EXISTS(SELECT * FROM sys.tables WHERE name = 'Products')
                            BEGIN
                                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'Products'))
                                BEGIN
                                    ALTER TABLE Products ADD IsActive BIT NOT NULL DEFAULT 1;
                                    PRINT 'Columna IsActive agregada a Products.';
                                END
                            END";
                        using (var migrationCmd = new SqlCommand(migrationProductsSql, connection))
                        {
                            migrationCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"Error en migración Products.IsActive: {ex.Message}");
                    }

                    // 2. Agregar columna IsActive a Clients si no existe (Soft Delete)
                    try 
                    {
                        string migrationClientsSql = @"
                            IF EXISTS(SELECT * FROM sys.tables WHERE name = 'Clients')
                            BEGIN
                                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'Clients'))
                                BEGIN
                                    ALTER TABLE Clients ADD IsActive BIT NOT NULL DEFAULT 1;
                                    PRINT 'Columna IsActive agregada a Clients.';
                                END
                            END";
                        using (var migrationCmd = new SqlCommand(migrationClientsSql, connection))
                        {
                            migrationCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                         Console.WriteLine($"Error en migración Clients.IsActive: {ex.Message}");
                    }

                    Console.WriteLine("Base de datos inicializada correctamente.");
                }
                else
                {
                    Console.WriteLine("No se encontró el archivo DbScript.sql.");
                }
            }
        }
    }
}
