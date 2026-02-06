using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SellManagement.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Intenta ejecutar la petición normal
                await _next(context);
            }
            catch (Exception error)
            {
                // Si algo falla, atrapa el error aquí
                var response = context.Response;
                response.ContentType = "application/json";

                // Definimos el código de estado según el error
                switch (error)
                {
                    case KeyNotFoundException e:
                        // Si no se encuentra algo (ej: ID no existe)
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    
                    case ArgumentException e:
                        // Si los datos enviados están mal
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    default:
                        // Error general del servidor
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                // Creamos una respuesta JSON clara
                var result = JsonSerializer.Serialize(new 
                { 
                    message = error?.Message, 
                    statusCode = response.StatusCode,
                    success = false
                });

                await response.WriteAsync(result);
            }
        }
    }
}