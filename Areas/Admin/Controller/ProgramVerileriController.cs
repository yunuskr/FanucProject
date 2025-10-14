using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanucRelease.Services.Interfaces;
using FanucRelease.Models;
using FanucRelease.Data;

namespace FanucRelease.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/ProgramVerileri/[action]")]
    public class ProgramVerileriController : Controller
    {
        private readonly IGenericService<ProgramVerisi> _programService; // liste
        private readonly ApplicationDbContext _ctx;                       // silmeler

        public ProgramVerileriController(
            IGenericService<ProgramVerisi> programService,
            ApplicationDbContext ctx)
        {
            _programService = programService;
            _ctx = ctx;
        }

        // Listeleme
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _programService.GetAllAsync();
            return View(list);
        }

        // === TEKLİ SİL ===
        [HttpPost("/Admin/ProgramVerileri/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) { TempData["err"] = "Geçersiz id."; return RedirectToAction(nameof(Index)); }

            await using var tx = await _ctx.Database.BeginTransactionAsync();
            try
            {
                // 1) Kaynağa bağlı AnlikKaynaklar
                await _ctx.Database.ExecuteSqlRawAsync(@"
                    DELETE AK
                    FROM [AnlikKaynaklar] AK
                    WHERE AK.[KaynakId] IN (
                        SELECT K.[Id] FROM [Kaynaklar] K WHERE K.[ProgramVerisiId] = {0}
                    );", id);

                // 2) ProgramVerisi'ne bağlı Hatalar
                await _ctx.Database.ExecuteSqlRawAsync(@"
                    DELETE FROM [Hatalar] WHERE [ProgramVerisiId] = {0};", id);

                // 3) (Opsiyonel) ProgramVerisi'ne bağlı başka tablolar
                // await _ctx.Database.ExecuteSqlRawAsync(@"DELETE FROM [KaynakParametreleri] WHERE [ProgramVerisiId] = {0};", id);

                // 4) ProgramVerisi'ne bağlı Kaynaklar
                await _ctx.Database.ExecuteSqlRawAsync(@"
                    DELETE FROM [Kaynaklar] WHERE [ProgramVerisiId] = {0};", id);

                // 5) Ebeveyn
                await _ctx.Database.ExecuteSqlRawAsync(@"
                    DELETE FROM [ProgramVerileri] WHERE [Id] = {0};", id);

                await tx.CommitAsync();
                TempData["ok"] = "Program ve bağlı tüm kayıtlar silindi.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = "Silme sırasında hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // === TOPLU SİL ===
        [HttpPost("/Admin/ProgramVerileri/DeleteMany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMany([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["err"] = "Silmek için en az bir kayıt seçin.";
                return RedirectToAction(nameof(Index));
            }

            var uniq = ids.Distinct().ToArray();

            await using var tx = await _ctx.Database.BeginTransactionAsync();
            try
            {
                // Güvenli ve net: id id ilerle (performans gerekirse IN(...)'e geçeriz)
                foreach (var id in uniq)
                {
                    await _ctx.Database.ExecuteSqlRawAsync(@"
                        DELETE AK
                        FROM [AnlikKaynaklar] AK
                        WHERE AK.[KaynakId] IN (
                            SELECT K.[Id] FROM [Kaynaklar] K WHERE K.[ProgramVerisiId] = {0}
                        );", id);

                    await _ctx.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM [Hatalar] WHERE [ProgramVerisiId] = {0};", id);

                    // await _ctx.Database.ExecuteSqlRawAsync(@"DELETE FROM [KaynakParametreleri] WHERE [ProgramVerisiId] = {0};", id);

                    await _ctx.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM [Kaynaklar] WHERE [ProgramVerisiId] = {0};", id);

                    await _ctx.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM [ProgramVerileri] WHERE [Id] = {0};", id);
                }

                await tx.CommitAsync();
                TempData["ok"] = $"{uniq.Length} program ve bağlı kayıtları silindi.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = "Toplu silme sırasında hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
