using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Data;

namespace FanucRelease.Controllers
{
    public class PerformansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerformansController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            await Task.CompletedTask;
            return View();
        }
    }
}
