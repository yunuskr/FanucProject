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
        private readonly ILogger<RobotTcpListenerService> _logger;
        private readonly IServiceProvider _services;
        private readonly IHubContext<RobotStatusHub> _hubContext;
        private TcpListener? _server;
        private readonly int _port = 59002; // Karel ile aynı port

        public RobotTcpListenerService(ILogger<RobotTcpListenerService> logger, IServiceProvider services, IHubContext<RobotStatusHub> hubContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
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

                // Sürekli veri bekle
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) != 0)
                {
                    string receivedMsg = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    veri.Append(receivedMsg.ToString().Replace(" ", string.Empty));


                    // Karel'e ACK gönder
                    string ack = "ACK";
                    byte[] ackBytes = Encoding.ASCII.GetBytes(ack);
                    await stream.WriteAsync(ackBytes, 0, ackBytes.Length, ct);
                    if (veri.ToString().Contains("programverisi"))
                    {
                        // SignalR ile robot durumu gönder - Sinyal geldiğinde robot çalışıyor
                        await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", "Calisiyor", "PRG-2025-08");
                        // string prog_verisi = veri.ToString().Replace("programverisi", string.Empty);
                        // string[] prog_parcalar = prog_verisi.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        // ProgramVerisi yeniProgram = new ProgramVerisi
                        // {
                        //     ProgramAdi = prog_parcalar.Length > 0 ? prog_parcalar[0] : "Unknown",
                        //     Durum = "Basladi",
                        //     HataKodu = "1325",
                        //     KaynakSayisi = prog_parcalar.Length > 1 ? int.Parse(prog_parcalar[1]) : 0,
                        // };
                        // using (var scope = _services.CreateScope())
                        // {
                        //     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        //     db.ProgramVerileri.Add(yeniProgram);
                        //     await db.SaveChangesAsync();
                        //     veri.Clear();
                        // }

                    }
                }

                _logger.LogInformation("Fanuc robot disconnected.");
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