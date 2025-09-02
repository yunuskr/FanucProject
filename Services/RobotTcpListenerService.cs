using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FanucRelease.Data;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;

namespace FanucRelease.Services
{
    /// <summary>
    /// Fanuc robottan TCP socket ile veri alan background service
    /// Robot client olacak, biz server olacağız
    /// </summary>
    public class RobotTcpListenerService : BackgroundService
    {
    private readonly string _statusFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "robot_status.json");

    private string _robotStatus = "Durdu";
    private string _aktifProgram = "";

    public string RobotStatus => _robotStatus;
    public string AktifProgram => _aktifProgram;
    private readonly ILogger<RobotTcpListenerService> _logger;
    private readonly IHubContext<RobotStatusHub> _hubContext;
    private readonly IServiceProvider _services;
    private TcpListener? _server;
    private readonly int _port = 59002; // Karel ile aynı port

        private void SaveRobotStatusToFile()
        {
            try
            {
                var statusObj = new { status = _robotStatus, aktifProgram = _aktifProgram };
                var json = JsonSerializer.Serialize(statusObj);
                // Atomic write to app base dir
                var tmpPath = _statusFilePath + ".tmp";
                File.WriteAllText(tmpPath, json);
                File.Copy(tmpPath, _statusFilePath, true);
                try { File.Delete(tmpPath); } catch { }
            }
            catch (Exception ex)
            {
                // Sadece hata olursa log.txt'ye yaz
                try
                {
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
                    var logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] robot_status.json dosyası güncellenemedi! Yol: {_statusFilePath}\n{ex}\n";
                    File.AppendAllText(logPath, logMsg);
                }
                catch { }
            }
        }

        private void LoadRobotStatusFromFile()
        {
            if (File.Exists(_statusFilePath))
            {
                var json = File.ReadAllText(_statusFilePath);
                try
                {
                    var statusObj = JsonSerializer.Deserialize<RobotStatusDto>(json);
                    if (statusObj != null)
                    {
                        _robotStatus = statusObj.status ?? "Durdu";
                        _aktifProgram = statusObj.aktifProgram ?? "";
                    }
                }
                catch { _robotStatus = "Durdu"; _aktifProgram = ""; }
            }
        }

        private class RobotStatusDto
        {
            public string? status { get; set; }
            public string? aktifProgram { get; set; }
        }

        public RobotTcpListenerService(ILogger<RobotTcpListenerService> logger, IHubContext<RobotStatusHub> hubContext, IServiceProvider services)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            LoadRobotStatusFromFile();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPAddress localAddr = IPAddress.Any;
            _server = new TcpListener(localAddr, _port);
            _server.Start();
            _logger.LogInformation("Server started on port {Port}, waiting for Fanuc robot connection...", _port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Robot bağlantısını kabul et
                    var client = await _server.AcceptTcpClientAsync(stoppingToken);
                    _logger.LogInformation("Fanuc robot connected from: {Remote}", client.Client.RemoteEndPoint);

                    // Her robot bağlantısını ayrı task'te işle
                    _ = Task.Run(() => RobotBaglantisiniIsle(client, stoppingToken), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Service cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TCP listener");
            }
            finally
            {
                try { _server?.Stop(); } catch { }
                _logger.LogInformation("Robot TCP server stopped.");
            }
        }

        private async Task RobotBaglantisiniIsle(TcpClient client, CancellationToken ct)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[256];
                int bytesRead;

                // Basit birleştirme için StringBuilder
                StringBuilder veri = new StringBuilder();
                Kaynak kaynak;
                AnlikKaynak anlikKaynak;
                ProgramVerisi programVerisi = new ProgramVerisi();
                List<Kaynak> kaynaklar = new List<Kaynak>();
                List<AnlikKaynak> anlikKaynaklar = new List<AnlikKaynak>();
                // Sürekli veri bekle
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) != 0)
                {
                    string receivedMsg = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    veri.Append(receivedMsg.ToString().Replace(" ", string.Empty));

                    // Karel'e ACK gönder
                    string ack = "ACK";
                    byte[] ackBytes = Encoding.ASCII.GetBytes(ack);
                    await stream.WriteAsync(ackBytes, 0, ackBytes.Length, ct);

                    if (veri.ToString().Contains("RobotAktif"))
                    {
                        // RobotAktif dediğinde program başladı canlı izle modu aktif etme SignalR
                        string prog_baslat = veri.ToString().Replace("RobotAktif", string.Empty) ?? string.Empty;
                        // Trim kontrolü
                        prog_baslat = prog_baslat.Trim();
                        _logger.LogInformation($"RobotAktif sinyali alındı, ham veri: {veri.ToString()}, prog_baslat: '{prog_baslat}'");
                        // Robotun son durumunu güncelle
                        _robotStatus = "Calisiyor";
                        _aktifProgram = prog_baslat ?? string.Empty;
                        _logger.LogInformation($"_robotStatus set to {_robotStatus}, _aktifProgram set to '{_aktifProgram}'");
                        SaveRobotStatusToFile();
                        // SignalR ile robot durumu gönder - Sinyal geldiğinde robot çalışıyor
                        await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);
                        programVerisi.ProgramAdi = prog_baslat ?? string.Empty;
                        veri.Clear();
                    }

                    else if (veri.ToString().Contains("anlikveri"))
                    {
                        string anlik_veriler = veri.ToString().Replace("anlikveri", string.Empty);
                        string[] anlik_parcalar = anlik_veriler.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        anlikKaynak = new AnlikKaynak
                        {
                            OlcumZamani = anlik_parcalar.Length > 0 ? DateTime.Now : DateTime.MinValue,
                            Voltaj = anlik_parcalar.Length > 1 ? double.Parse(anlik_parcalar[0]) : 0,
                            Amper = anlik_parcalar.Length > 2 ? double.Parse(anlik_parcalar[1]) : 0,
                            TelSurmeHizi = anlik_parcalar.Length > 3 ? double.Parse(anlik_parcalar[2]) : 0,
                            KaynakHizi = 10,

                        };
                        anlikKaynaklar.Add(anlikKaynak);
                            // SignalR ile canlı veri gönder
                            _logger.LogInformation($"SignalR veri gönderildi: Amper={anlikKaynak.Amper}, Voltaj={anlikKaynak.Voltaj}, TelSurmeHizi={anlikKaynak.TelSurmeHizi}, Zaman={anlikKaynak.OlcumZamani:O}");
                            await _hubContext.Clients.All.SendAsync(
                                "ReceiveLiveData",
                                anlikKaynak.Amper,
                                anlikKaynak.Voltaj,
                                anlikKaynak.TelSurmeHizi,
                                anlikKaynak.OlcumZamani.ToString("o")
                            );
                        veri.Clear();

                    }

                    else if (veri.ToString().Contains("KayOFF"))
                    {
                        string kaynak_verileri = veri.ToString().Replace("KayOFF", string.Empty);
                        string[] kaynak_parcalar = kaynak_verileri.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        kaynak = new Kaynak
                        {
                            BaslangicSaati = kaynak_parcalar.Length > 0 ? Hesaplayici.stringDateParse(kaynak_parcalar[0]) : DateTime.MinValue,
                            ToplamSure = Hesaplayici.milisaniyeyiTimeOnlyeCevir(kaynak_parcalar.Length > 1 ? int.Parse(kaynak_parcalar[1]) : 0),
                            KaynakUzunlugu = kaynak_parcalar.Length > 2 ? int.Parse(kaynak_parcalar[2]) : 0,
                            BaslangicSatiri = kaynak_parcalar.Length > 3 ? int.Parse(kaynak_parcalar[3]) : 0,
                            BitisSatiri = kaynak_parcalar.Length > 4 ? int.Parse(kaynak_parcalar[4]) : 0,
                            BitisSaati = Hesaplayici.BaslangicaSureEkle(kaynak_parcalar[0], kaynak_parcalar[1]),
                            PrcNo = kaynak_parcalar.Length > 5 ? int.Parse(kaynak_parcalar[5]) : 0,
                            SrcNo = kaynak_parcalar.Length > 6 ? int.Parse(kaynak_parcalar[6]) : 0,
                            AnlikKaynaklar = anlikKaynaklar
                        };
                        kaynaklar.Add(kaynak);
                        veri.Clear();


                    }


                    else if (veri.ToString().Contains("progbitti"))
                    {
                        string prog_verisi = veri.ToString().Replace("progbitti", string.Empty);
                        programVerisi.KaynakSayisi = int.Parse(prog_verisi);
                        programVerisi.Operator = new Operator { Ad = "Ahmet", Soyad = "Çakar", KullaniciAdi = "ahmet.cakar" };
                        programVerisi.Kaynaklar = kaynaklar;

                        using (var scope = _services.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            db.ProgramVerileri.Add(programVerisi);
                            // db.Kaynaklar.AddRange(kaynaklar);
                            // db.AnlikKaynaklar.AddRange(anlikKaynaklar);
                            await db.SaveChangesAsync();
                        }

                        // Program bittiğinde robot durdu bilgisini ve aktif programı temizle
                        _robotStatus = "Durdu";
                        _aktifProgram = "";
                        SaveRobotStatusToFile();
                        await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);

                        // Reset all temporary data for next program
                        veri.Clear();
                        kaynaklar = new List<Kaynak>();
                        anlikKaynaklar = new List<AnlikKaynak>();
                        programVerisi = new ProgramVerisi();
                    }

                    _logger.LogInformation("Fanuc robot disconnected.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "Error handling robot connection");
            }
            finally
            {
                try { client.Close(); } catch { }
                _logger.LogInformation("Robot connection closed.");
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try { _server?.Stop(); } catch { }
            return base.StopAsync(cancellationToken);
        }
    }
}