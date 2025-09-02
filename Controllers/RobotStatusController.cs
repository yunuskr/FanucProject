using Microsoft.AspNetCore.Mvc;
using FanucRelease.Services;

namespace FanucRelease.Controllers
{
    [ApiController]
    [Route("api/robot")]
    public class RobotStatusController : ControllerBase
    {
    // RobotTcpListenerService bağımlılığı kaldırıldı, dosyadan okuma yapılıyor

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            // Dosyadan en güncel robot durumu ve aktif programı oku
            string status = "Durdu";
            string aktifProgram = "";
        string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "robot_status.json");
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(filePath);
                    var statusObj = System.Text.Json.JsonSerializer.Deserialize<RobotStatusDto>(json);
                    if (statusObj != null)
                    {
                        status = statusObj.status ?? "Durdu";
                        aktifProgram = statusObj.aktifProgram ?? "";
                    }
                }
                catch { }
            }
            return Ok(new {
                status = status,
                aktifProgram = aktifProgram
            });
        }

        private class RobotStatusDto
        {
            public string? status { get; set; }
            public string? aktifProgram { get; set; }
        }
    }
}
