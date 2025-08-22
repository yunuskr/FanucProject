using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FanucRelease.Data;
using FanucRelease.Models;

namespace FanucRelease.Services
{
    /// <summary>
    /// Arka planda çalışan TCP dinleyici servisidir.
    /// Uygulama başlarken otomatik başlar ve robottan gelen verileri kabul edip işler.
    /// DbContext gibi scoped servisleri kullanmak için IServiceProvider üzerinden scope oluşturulur.
    /// </summary>
    public class RobotTcpListenerService : BackgroundService
    {
        private readonly ILogger<RobotTcpListenerService> _logger;
        private readonly IServiceProvider _services;
        private TcpListener? _listener;
        private readonly int _port = 59002;

        public RobotTcpListenerService(IServiceProvider services, ILogger<RobotTcpListenerService> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _logger.LogInformation("Robot TCP dinleyicisi port {Port} üzerinde başlatıldı.", _port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Yeni bağlantı kabul et
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    // Her client'i ayrı task olarak işle (fire-and-forget)
                    _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Uygulama kapatılırken beklenen durum
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP dinleyici sırasında hata oluştu.");
            }
            finally
            {
                try { _listener?.Stop(); } catch { }
                _logger.LogInformation("Robot TCP dinleyicisi durduruldu.");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            _logger.LogInformation("Client bağlandı: {Remote}", client.Client.RemoteEndPoint);
            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[4096];

                while (!ct.IsCancellationRequested)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    }
                    catch (OperationCanceledException) { break; }

                    if (bytesRead == 0) break; // client bağlantıyı kapattı

                    var received = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    _logger.LogInformation("Gelen veri ({Bytes} bayt): {Msg}", bytesRead, received);

                    // Gelen veriyi parse edip veritabanına kaydet
                    try
                    {
                        var anlik = ParseToAnlikKaynak(received);
                        if (anlik != null)
                        {
                            using var scope = _services.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            db.AnlikKaynaklar.Add(anlik);
                            await db.SaveChangesAsync(ct);
                            _logger.LogInformation("AnlikKaynak veritabanına kaydedildi. Id: {Id}", anlik.Id);

                            // Burada istersen SignalR ile frontend'e anlık gönderim yapılabilir (IHubContext kullanarak)
                        }
                        else
                        {
                            _logger.LogWarning("Gelen veri parse edilemedi: {Msg}", received);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Gelen veriyi kaydederken hata oluştu.");
                    }

                    // ACK gönder
                    try
                    {
                        var ack = Encoding.ASCII.GetBytes("ACK");
                        await stream.WriteAsync(ack, 0, ack.Length, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ACK gönderilirken hata oluştu.");
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "Client işlenirken hata oluştu.");
            }
            finally
            {
                try { client.Close(); } catch { }
                _logger.LogInformation("Client bağlantısı kapatıldı.");
            }
        }

        // Basit parser: JSON önce, başarılı olmazsa CSV (voltaj,amper,telSurmeHizi[,kaynakDongusuId])
        private AnlikKaynak? ParseToAnlikKaynak(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // 1) JSON dene
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dto = JsonSerializer.Deserialize<AnlikKaynakDto>(raw, options);
                if (dto != null)
                {
                    return new AnlikKaynak
                    {
                        OlcumZamani = DateTime.Now,
                        Voltaj = dto.Voltaj,
                        Amper = dto.Amper,
                        TelSurmeHizi = dto.TelSurmeHizi,
                        KaynakId = dto.KaynakId ?? 0
                    };
                }
            }
            catch { /* JSON parse başarısızsa CSV'ye geç */ }

            // 2) CSV dene
        var parts = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(p => p.Trim()).ToArray();
            if (parts.Length >= 3)
            {
        bool okVolt = double.TryParse(parts[0].Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var volt);
        bool okAmper = double.TryParse(parts[1].Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amper);
        bool okTel = double.TryParse(parts[2].Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tel);
                int kaynakId = 0;
                if (parts.Length >= 4) int.TryParse(parts[3], out kaynakId);

                if (okVolt && okAmper && okTel)
                {
                    return new AnlikKaynak
                    {
            OlcumZamani = DateTime.Now,
            Voltaj = volt,
            Amper = amper,
            TelSurmeHizi = tel,
            KaynakId = kaynakId
                    };
                }
            }

            return null;
        }

        private class AnlikKaynakDto
        {
            public double Voltaj { get; set; }
            public double Amper { get; set; }
            public double TelSurmeHizi { get; set; }
            public int? KaynakId { get; set; }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try { _listener?.Stop(); } catch { }
            return base.StopAsync(cancellationToken);
        }
    }
}