
using Fuentech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fuentech.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SolicitudService _solicitudService;

        public IndexModel(SolicitudService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        public List<SolicitudContacto> Solicitudes { get; set; } = new();

        public async Task OnGetAsync()
        {
            Solicitudes = await _solicitudService.ObtenerSolicitudesAsync();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (id <= 0)
                return BadRequest();

            await _solicitudService.EliminarSolicitudAsync(id);

            return RedirectToPage();
        }
    }
}
