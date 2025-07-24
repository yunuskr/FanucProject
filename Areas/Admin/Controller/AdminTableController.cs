using Microsoft.AspNetCore.Mvc;
using FanucRelease.Services.Interfaces;
using FanucRelease.Models; // bu satırda da Admin tanımlı, ama çakışma varsa tam ad yazacağız

namespace FanucRelease.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/AdminTable/[action]")]
    public class AdminTableController : Controller
    {
        private readonly IGenericService<FanucRelease.Models.Admin> _adminService;

        public AdminTableController(IGenericService<FanucRelease.Models.Admin> adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            var adminler = await _adminService.GetAllAsync();
            return View(adminler);
        }
         [HttpPost]
        public async Task<IActionResult> AddAdmin(FanucRelease.Models.Admin admin)
        {
            if (ModelState.IsValid)
            {
                await _adminService.AddAsync(admin);
                return Ok();
            }
            return BadRequest();
        }

        // Admin Güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateAdmin(FanucRelease.Models.Admin admin)
        {
            if (ModelState.IsValid)
            {
                await _adminService.UpdateAsync(admin);
                return Ok();
            }
            return BadRequest();
        }

        // Silme
        // [HttpPost]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     var admin = await _adminService.GetByIdAsync(id);
        //     if (admin != null)
        //     {
        //         await _adminService.DeleteAsync(admin);
        //         return Ok();
        //     }
        //     return NotFound();
        // }
    }
}
