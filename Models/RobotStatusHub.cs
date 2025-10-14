using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class RobotStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
            try
            {
                // Resolve project root like the service so both read/write the same file
                string filePath;
                try
                {
                    // Öncelik: bin/.../data/robot_status.json (service ile aynı)
                    var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
                    var dataDir = Path.Combine(baseDir, "data");
                    filePath = Path.Combine(dataDir, "robot_status.json");
                    if (!File.Exists(filePath))
                    {
                        // Geriye dönük uyumluluk için eski konumlar
                        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
                        var legacyProjectPath = Path.Combine(projectRoot, "robot_status.json");
                        if (File.Exists(legacyProjectPath)) filePath = legacyProjectPath; else
                        {
                            var legacyBasePath = Path.Combine(baseDir, "robot_status.json");
                            if (File.Exists(legacyBasePath)) filePath = legacyBasePath;
                        }
                    }
                }
                catch
                {
                    filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "robot_status.json");
                }
            string status = "Durdu";
            string aktifProgram = string.Empty;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                try
                {
                    var dto = JsonSerializer.Deserialize<RobotStatusDto>(json);
                    if (dto != null)
                    {
                        status = dto.status ?? "Durdu";
                        aktifProgram = dto.aktifProgram ?? string.Empty;
                    }
                }
                catch { }
            }

            // Send current status only to the newly connected client
            await Clients.Caller.SendAsync("ReceiveRobotStatus", status, aktifProgram);
        }
        catch
        {
            // ignore
        }

        await base.OnConnectedAsync();
    }

    private class RobotStatusDto
    {
        public string? status { get; set; }
        public string? aktifProgram { get; set; }
    }
}