using Microsoft.AspNetCore.Mvc;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;

namespace FanucRelease.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/OperatorTable/[action]")]
    public class OperatorTableController : Controller
    {
        private readonly IGenericService<Operator> _operatorService;

        public OperatorTableController(IGenericService<Operator> operatorService)
        {
            _operatorService = operatorService;
        }

        public async Task<IActionResult> Index()
        {
            var operators = await _operatorService.GetAllAsync();
            return View(operators); // --> Views/Admin/OperatorTable/Index.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOperator([FromForm] Operator model)
        {
            if (ModelState.IsValid)
            {
                await _operatorService.AddAsync(model);
                return Ok();
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOperator([FromForm] Operator model)
        {
            if (ModelState.IsValid)
            {
                await _operatorService.UpdateAsync(model);
                return Ok();
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _operatorService.DeleteAsync(id);
            return Ok();
        }
    }
}
