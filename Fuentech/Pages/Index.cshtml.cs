
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fuentech.Services;
using MySqlConnector;

namespace Fuentech.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AiService _aiService;
        private readonly SolicitudService _solicitudService;

        public IndexModel(
            AiService aiService,
            SolicitudService solicitudService)
        {
            _aiService = aiService;
            _solicitudService = solicitudService;
        }

        public void OnGet()
        {
        }

        // CHATBOT
        public async Task<IActionResult> OnPostChatAsync(
            [FromForm] string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return new JsonResult(new
                {
                    success = false,
                    respuesta = "Escribe un mensaje para continuar."
                })
                {
                    StatusCode = 400
                };
            }

            try
            {
                string respuesta =
                    await _aiService.GetResponseAsync(mensaje);

                return new JsonResult(new
                {
                    success = true,
                    respuesta = respuesta
                });
            }
            catch (HttpRequestException)
            {
                return new JsonResult(new
                {
                    success = false,
                    respuesta = "No pude conectarme con el servicio de inteligencia artificial. Verifica que Ollama esté funcionando."
                })
                {
                    StatusCode = 503
                };
            }
            catch (TaskCanceledException)
            {
                return new JsonResult(new
                {
                    success = false,
                    respuesta = "La respuesta tardó demasiado. Intenta nuevamente."
                })
                {
                    StatusCode = 504
                };
            }
        }

        // FORMULARIO DE CONTACTO
        public async Task<IActionResult> OnPostSolicitudAsync(
            [FromForm] string nombre,
            [FromForm] string? correo,
            [FromForm] string? telefono,
            [FromForm] string proyecto)
        {
            nombre = nombre?.Trim() ?? "";
            correo = correo?.Trim();
            telefono = telefono?.Trim();
            proyecto = proyecto?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(proyecto))
            {
                return new JsonResult(new
                {
                    success = false,
                    mensaje = "Completa tu nombre y la descripción de tu proyecto."
                })
                {
                    StatusCode = 400
                };
            }

            if (string.IsNullOrWhiteSpace(correo) &&
                string.IsNullOrWhiteSpace(telefono))
            {
                return new JsonResult(new
                {
                    success = false,
                    mensaje = "Proporciona un correo o un teléfono para poder contactarte."
                })
                {
                    StatusCode = 400
                };
            }

            if (nombre.Length > 100 ||
                (correo?.Length ?? 0) > 254 ||
                (telefono?.Length ?? 0) > 20 ||
                proyecto.Length > 1500)
            {
                return new JsonResult(new
                {
                    success = false,
                    mensaje = "Algunos campos exceden la longitud permitida."
                })
                {
                    StatusCode = 400
                };
            }

            try
            {
                await _solicitudService.GuardarSolicitudAsync(
                    nombre,
                    string.IsNullOrWhiteSpace(correo) ? null : correo,
                    string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                    proyecto
                );

                return new JsonResult(new
                {
                    success = true,
                    mensaje = "¡Gracias! Tu solicitud fue enviada correctamente. Pronto nos pondremos en contacto contigo."
                });
            }
            catch (MySqlException)
            {
                return new JsonResult(new
                {
                    success = false,
                    mensaje = "No pudimos guardar tu solicitud. Intenta nuevamente más tarde."
                })
                {
                    StatusCode = 500
                };
            }
            catch (InvalidOperationException)
            {
                return new JsonResult(new
                {
                    success = false,
                    mensaje = "Hay un problema con la configuración de la base de datos."
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}
