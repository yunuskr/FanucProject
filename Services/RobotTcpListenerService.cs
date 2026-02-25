
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using FanucRelease.Models;
using FanucRelease.Data;
using FanucRelease.Services;
using Microsoft.EntityFrameworkCore;
namespace FanucRelease.Services
{
    /// <summary>
    /// Fanuc robottan TCP socket ile veri alan background service
    /// Robot client olacak, biz server olacağız
    /// </summary>
    public class RobotTcpListenerService : BackgroundService
    {
        // Dosya yolları
        private readonly string _statusFilePath;
        private readonly string? _errorFilePath;
        private readonly string _dataDirectory;

        // Robot durumları
        private string _robotStatus = "Durdu";
        private string _aktifProgram = "";
        private bool _hasFault = false; // sistem fault durum bayrağı

        // Sabitler
        private const int RobotPort = 59002; // Karel ile aynı port
        private readonly IHubContext<RobotStatusHub> _hubContext;
        private readonly ICurrentUserService _currentUser;
        private readonly IServiceProvider _services;
        private TcpListener? _server;

        public string RobotStatus => _robotStatus;
        public string AktifProgram => _aktifProgram;

        private void LogToFile(string message, Exception? ex = null)
        {
            if (string.IsNullOrEmpty(_errorFilePath)) return;
            try
            {
                var logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                if (ex != null)
                {
                    logMsg += $"\nException: {ex}";
                }
                logMsg += "\n";
                File.AppendAllText(_errorFilePath, logMsg);
            }
            catch (Exception logEx)
            {
                // İsteğe bağlı: log hatasını konsola yazdır
                Console.Error.WriteLine($"Log dosyasına yazılamadı: {logEx.Message}");
            }
        }
        private void SaveRobotStatusToFile()
        {
            try
            {
                var statusObj = new { status = _robotStatus, aktifProgram = _aktifProgram };
                var json = JsonSerializer.Serialize(statusObj);
                // Atomic write to app base dir
                var tmpPath = _statusFilePath + ".tmp";
                File.WriteAllText(tmpPath, json);
                File.Move(tmpPath, _statusFilePath, true);
                try { File.Delete(tmpPath); }
                catch (Exception ex)
                {
                    LogToFile("Beklenmeyen hata oluştu-1", ex);
                }
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
                catch
                {
                    LogToFile("Beklenmeyen hata oluştu-2", ex);
                }
            }
        }
        private double FixFloat(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return 0;

            // SQL FLOAT için güvenli aralık
            if (value > 1_000_000) return 0;
            if (value < -1_000_000) return 0;

            return Math.Round(value, 3);
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

       public RobotTcpListenerService(
        IHubContext<RobotStatusHub> hubContext,
        IServiceProvider services,
        ICurrentUserService currentUser)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _currentUser = currentUser;

            try
            {
                // Tüm runtime dosyalarını bin/.../data klasörüne alalım (dotnet watch/browser refresh tetiklenmesin)
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _dataDirectory = Path.Combine(baseDir, "data");
                Directory.CreateDirectory(_dataDirectory);

                _statusFilePath = Path.Combine(_dataDirectory, "robot_status.json");
                _errorFilePath = Path.Combine(_dataDirectory, "Logs.txt");


            }
            catch
            {
                // Her durumda en azından bin/.../data içerisinde çalışmayı garantile
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _dataDirectory = Path.Combine(baseDir, "data");
                try { Directory.CreateDirectory(_dataDirectory); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-5", ex); }
                _statusFilePath = Path.Combine(_dataDirectory, "robot_status.json");
            }

            LoadRobotStatusFromFile();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPAddress localAddr = IPAddress.Any;
            _server = new TcpListener(localAddr, RobotPort);
            _server.Start();
            LogToFile($"Server started on port {RobotPort}, waiting for Fanuc robot connection...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Robot bağlantısını kabul et
                    var client = await _server.AcceptTcpClientAsync(stoppingToken);
                    LogToFile($"Fanuc robot connected from: {client.Client.RemoteEndPoint}");

                    // Her robot bağlantısını ayrı task'te işle
                    _ = Task.Run(() => RobotBaglantisiniIsle(client, stoppingToken), stoppingToken);
                }
            }
            catch (OperationCanceledException ex)
            {
                LogToFile("Service cancellation requested", ex);
            }
            catch (Exception ex)
            {
                LogToFile("Error in TCP listener", ex);
            }
            finally
            {
                try { _server?.Stop(); } catch (Exception ex) { LogToFile("TCP server stop error", ex); }
                LogToFile("Robot TCP server stopped.");
            }
        }

        private async Task RobotBaglantisiniIsle(TcpClient client, CancellationToken ct)
        {
            ProgramVerisi programVerisi = new ProgramVerisi();
            List<Kaynak> kaynaklar = new List<Kaynak>();
            List<AnlikKaynak> anlikKaynaklar = new List<AnlikKaynak>();
            List<Hata> hatalar = new List<Hata>();
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[256];
                int bytesRead;

                // Basit birleştirme için StringBuilder
                StringBuilder veri = new StringBuilder();
                Kaynak kaynak;
                AnlikKaynak anlikKaynak;

                try
                {
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

                            // 🔹 Yeni program başladığında önce geçici verileri temizle
                            kaynaklar = new List<Kaynak>();
                            anlikKaynaklar = new List<AnlikKaynak>();
                            hatalar = new List<Hata>();
                            programVerisi = new ProgramVerisi();
                            // RobotAktif dediğinde program başladı, Errors.txt dosyasını temizle
                            try
                            {
                                if (!string.IsNullOrEmpty(_errorFilePath))
                                    File.WriteAllText(_errorFilePath, string.Empty);
                            }
                            catch (Exception ex)
                            {
                                LogToFile("Beklenmeyen hata oluştu-6", ex);
                            }
                            string prog_baslat = veri.ToString().Replace("RobotAktif", string.Empty) ?? string.Empty;

                            string[] veri_parca = prog_baslat.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            // Trim kontrolü
                            prog_baslat = veri_parca[0].Trim();
                            LogToFile($"RobotAktif sinyali alındı, ham veri: {veri.ToString()}, prog_baslat: '{prog_baslat}'");
                            // Robotun son durumunu güncelle
                            _robotStatus = "Calisiyor";
                            _aktifProgram = prog_baslat ?? string.Empty;
                            LogToFile($"{_aktifProgram} programı başlatıldı.");
                            SaveRobotStatusToFile();
                            // SignalR ile robot durumu gönder - Sinyal geldiğinde robot çalışıyor
                            await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);

                            // Program yeniden başlarken aktif fault varsa sıfırla ve Normal yayınla
                            if (_hasFault)
                            {
                                _hasFault = false;
                                try { await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", "Normal"); LogToFile("RobotAktif: Normal yayınlandı (fault reset)"); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-7", ex); }
                            }
                            programVerisi.ProgramAdi = prog_baslat ?? string.Empty;
                            programVerisi.BaslangicZamani = DateTime.Now;
                            programVerisi.Tarih = DateTime.Now.Date;

                            veri.Clear();
                        }

                        else if (veri.ToString().Contains("anlikveri"))
                        {
                            // Eğer hata modundan çıkıldıysa ilk gerçek zamanlı veriyle normal e dön
                            if (_hasFault)
                            {
                                _hasFault = false;
                                try { await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", "Normal"); LogToFile("anlikveri: Normal yayınlandı (fault reset)"); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-8", ex); }
                            }

                            // Program bittiğinde robot durdu bilgisini ve aktif programı kesin olarak temizle
                            if (_robotStatus == "Durdu")
                            {
                                _robotStatus = "Calisiyor";
                                LogToFile($" {_aktifProgram} programı hatadan sonra devam ediyor, robot durumu =  {_robotStatus}");
                                SaveRobotStatusToFile();
                                // SignalR ile robot durumu gönder - Sinyal geldiğinde robot çalışıyor
                                await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);

                            }

                            string anlik_veriler = veri.ToString().Replace("anlikveri", string.Empty);
                            string[] anlik_parcalar = anlik_veriler.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            var power = anlik_parcalar.Length > 2 ? double.Parse(anlik_parcalar[3]) : 0;
                            var heat_input = anlik_parcalar.Length > 2 ? double.Parse(anlik_parcalar[4]) : 0;
                            double kaynak_hizi = 0.0;
                            try
                            {
                                kaynak_hizi = Math.Round(power / heat_input, 1);
                            }
                            catch (Exception)
                            {
                                kaynak_hizi = 0.0;

                            }
                            
                            anlikKaynak = new AnlikKaynak
                            {
                                OlcumZamani = anlik_parcalar.Length > 0 ? DateTime.Now : DateTime.MinValue,
                                    Voltaj = FixFloat(
                                anlik_parcalar.Length > 0 
                                ? double.Parse(anlik_parcalar[0]) 
                                : 0
                            ),

                            Amper = FixFloat(
                                anlik_parcalar.Length > 1 
                                ? double.Parse(anlik_parcalar[1]) 
                                : 0
                            ),

                            TelSurmeHizi = FixFloat(
                                anlik_parcalar.Length > 2 
                                ? double.Parse(anlik_parcalar[2]) 
                                : 0
                            ),

                            KaynakHizi = FixFloat(kaynak_hizi),


                            };
                            anlikKaynaklar.Add(anlikKaynak);
                            // SignalR ile canlı veri gönder
                            LogToFile($"SignalR veri gönderildi: Amper={anlikKaynak.Amper}, Voltaj={anlikKaynak.Voltaj}, TelSurmeHizi={anlikKaynak.TelSurmeHizi}, Zaman={anlikKaynak.OlcumZamani:yyyy-MM-dd HH:mm:ss}");
                            await _hubContext.Clients.All.SendAsync(
                                "ReceiveLiveData",
                                anlikKaynak.Amper,
                                anlikKaynak.Voltaj,
                                anlikKaynak.TelSurmeHizi,
                                anlikKaynak.KaynakHizi, 
                                anlikKaynak.OlcumZamani.ToString("yyyy-MM-dd HH:mm:ss")
                            );

                            // UI senkron değilse (örneğin Normal yayın kaçırıldıysa) Normal durumunu tekrar yayınla
                            // Her anlık veri geldiğinde fault yoksa Normal'i tekrar gönder (UI kaybını tolere etmek için)
                            if (!_hasFault)
                            {
                                try { await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", "Normal"); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-9", ex); }
                            }
                            veri.Clear();

                        }

                        else if (veri.ToString().Contains("KayOFF"))
                        {   
                        // 🔹 O ana kadar biriken anlık verilerin DERİN kopyası
                            var kaynakAnlik = anlikKaynaklar
                                .Select(a => new AnlikKaynak {
                                    OlcumZamani = a.OlcumZamani,
                                    Voltaj = a.Voltaj,
                                    Amper = a.Amper,
                                    TelSurmeHizi = a.TelSurmeHizi,
                                    KaynakHizi = a.KaynakHizi,
                                }).ToList();


                            string kaynak_verileri = veri.ToString().Replace("KayOFF", string.Empty);
                            string[] kaynak_parcalar = kaynak_verileri.Split('|', StringSplitOptions.RemoveEmptyEntries);
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
                                basarili_mi = kaynak_parcalar.Length > 6 ? bool.Parse(kaynak_parcalar[7]) : false,
                                KaynakAdi = "Kaynak-" + kaynak_parcalar[8].ToString(),
                                AnlikKaynaklar = kaynakAnlik,
                                ProgramVerisi = programVerisi // ✅ Foreign key yerine entity referansı
                            };
                            kaynaklar.Add(kaynak);
                            // 🔹 Yeni kaynak için temiz bir liste
                            anlikKaynaklar = new List<AnlikKaynak>();
                             // 4) ✨ KAYNAĞI ANINDA DB'YE YAZ ✨
                            try
                            {
                                using (var scope = _services.CreateScope())
                                {
                                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                                    // ProgramVerisi henüz DB'ye eklenmemişse ekle
                                    if (programVerisi.Id == 0)
                                    {
                                        db.ProgramVerileri.Add(programVerisi);
                                        await db.SaveChangesAsync();
                                    }
                                    else
                                    {
                                        db.Attach(programVerisi);  // 🔥 EKLENECEK
                                    }

                                    // Şimdi kaynağı kaydet
                                    db.Kaynaklar.Add(kaynak);
                                    if (hatalar.Count > 0)
                                    {
                                        db.Hatalar.AddRange(hatalar);
                                        await db.SaveChangesAsync();
                                        hatalar.Clear();
                                        LogToFile($"{hatalar.Count} KayOFF içindeki hata kaydı veritabanına yazıldı.");
                                    }
                                    await db.SaveChangesAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                LogToFile("Kaynak veri tabanına yazılırken hata oluştu", ex);
                            }

                            veri.Clear();

                        }

                        else if (veri.ToString().Contains("progbitti"))
                        {
                            string prog_verisi = veri.ToString().Replace("progbitti", string.Empty);
                            programVerisi.KaynakSayisi = int.Parse(prog_verisi);
                            // programVerisi.Operator = new Operator { Ad = "Ahmet", Soyad = "Çakar", KullaniciAdi = "ahmet.cakar" };
                            programVerisi.BitisZamani = programVerisi.BitisZamani == default ? DateTime.Now : programVerisi.BitisZamani;
                            // Eğer başlangıç/bitiş zamanları ayarlanmamışsa, kaynaklardan türet
                            if (programVerisi.BaslangicZamani == default)
                            {
                                var firstStart = kaynaklar
                                    .Where(k => k.BaslangicSaati != default && k.BaslangicSaati > DateTime.MinValue)
                                    .OrderBy(k => k.BaslangicSaati)
                                    .Select(k => k.BaslangicSaati)
                                    .FirstOrDefault();
                                if (firstStart != default && firstStart > DateTime.MinValue)
                                    programVerisi.BaslangicZamani = firstStart;
                            }
                            if (programVerisi.BitisZamani == default)
                            {
                                var lastEnd = kaynaklar
                                    .Where(k => k.BitisSaati != default && k.BitisSaati > DateTime.MinValue)
                                    .OrderByDescending(k => k.BitisSaati)
                                    .Select(k => k.BitisSaati)
                                    .FirstOrDefault();
                                if (lastEnd != default && lastEnd > DateTime.MinValue)
                                    programVerisi.BitisZamani = lastEnd;
                            }
                            programVerisi.Kaynaklar = kaynaklar;
                            programVerisi.Hatalar = hatalar;
                            programVerisi.TamamlandiMi = true;
                            LogToFile($"prog bitti çıkışı");
                            // Reset all temporary data for next program
                            veri.Clear();

                        }

                        else if (veri.ToString().Contains("hataalindi"))
                        {

                            string hata_verisi = veri.ToString().Replace("hataalindi", string.Empty);
                            string[] hata_parcalar = hata_verisi.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            int tip = hata_parcalar[1] == "1" ? 1 : 2;
                            DateTime hata_zamani = Hesaplayici.stringDateParse(hata_parcalar[3]);
                            bool kaynakAnindaMi = Convert.ToBoolean(int.Parse(hata_parcalar[4]));

                            string kaynakAdi = null;
                            if (kaynakAnindaMi)
                            {
                                kaynakAdi = "Kaynak-" + hata_parcalar[5].ToString();
                            }

                            Hata hata = new Hata
                            {
                                Kod = hata_parcalar[0],
                                Tip = tip,
                                Aciklama = hata_parcalar[2],
                                Zaman = hata_zamani,
                                KaynakAnindaMi = kaynakAnindaMi,
                                KaynakAdi = kaynakAdi,
                                ProgramVerisi = programVerisi // ✅ Navigation property üzerinden bağla
                            };
                            hatalar.Add(hata);
                            LogToFile($"Hata Log kaydedildi: Kod={hata.Kod}, Aciklama={hata.Aciklama}, Zaman={hata.Zaman:yyyy-MM-dd HH:mm:ss}");

                            // Hata olduğunda sistem fault durumuna geçsin
                            _robotStatus = "Durdu";
                            _aktifProgram = "";
                            SaveRobotStatusToFile();
                            await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);

                            _hasFault = true; // her hata durumunda set (üst üste gelse bile)
                            try { await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", "Fault"); LogToFile("hataalindi: Fault yayınlandı"); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-12", ex); }

                            veri.Clear();

                        }

                       

                    }

                }
                catch (IOException ioEx)
                {
                    LogToFile("TCP bağlantısı robot tarafından kapatıldı: " + ioEx.Message);
                    try { client.Close(); } catch { }
                    return; // bağlantı bittiği için metottan çık
                }

            }
            catch (OperationCanceledException ex)
            {
                LogToFile("Robot bağlantısı iptal edildi.", ex);
            }
            catch (Exception ex)
            {
                LogToFile("Error handling robot connection", ex);
            }
            finally
            {
                try { client.Close(); } catch (Exception ex) { LogToFile("Client kapanma hatası alındı", ex); }


                try
                {

                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        // 🟢 1. Login yapan operator’ü al
                        var operatorKullaniciAdi = _currentUser.Username;
                        Operator? mevcutOperator = null;

                        if (!string.IsNullOrWhiteSpace(operatorKullaniciAdi))
                        {
                            mevcutOperator = await db.Operators.FirstOrDefaultAsync(o => o.KullaniciAdi == operatorKullaniciAdi);
                        }


                        // 🟢 3. Program verisine operator ID’yi bağla
                        if (mevcutOperator != null)
                        {
                            programVerisi.Operator = mevcutOperator;
                        }


                        // db.ProgramVerileri.Update(programVerisi);

                        // 🔥 SORUNSUZ ÇÖZÜM BURADA 🔥
                        if (programVerisi.Id > 0)
                            db.ProgramVerileri.Update(programVerisi);
                        else
                            db.ProgramVerileri.Add(programVerisi);

                        db.Hatalar.AddRange(hatalar);
                        // db.Kaynaklar.AddRange(kaynaklar);
                        // db.AnlikKaynaklar.AddRange(anlikKaynaklar);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    LogToFile("ProgramVerisi kaydedilirken hata oluştu.", ex);
                }


                // Program tamamen bittiğinde varsa aktif fault durumunu sıfırla

                try { await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", "Normal"); LogToFile("progbitti: Normal yayınlandı (fault reset)"); } catch (Exception ex) { LogToFile("Beklenmeyen hata oluştu-10", ex); }
                _hasFault = false;


                // 🔔 Bağlantı koptu → robotu durdu kabul et
                _robotStatus = "Durdu";
                _aktifProgram = "";
                SaveRobotStatusToFile();
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveRobotStatus", _robotStatus, _aktifProgram);

                }
                catch (Exception ex)
                {
                    LogToFile("Yayını kesme başarısız oldu", ex);
                }

                LogToFile("Robot bağlantısı kapatıldı.");
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try { _server?.Stop(); } catch (Exception ex) { LogToFile("RobotTcpListenerService stop error", ex); }
            LogToFile("RobotTcpListenerService stopped.");
            return base.StopAsync(cancellationToken);
        }
    }
}