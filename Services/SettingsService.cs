using FanucRelease.Data;
using FanucRelease.Models;
using FanucRelease.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace FanucRelease.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrUpdateAsync(Setting model)
        {
            var setting = _context.Settings.FirstOrDefault();
            if (setting == null)
            {
                _context.Settings.Add(model);
            }
            else
            {
                setting.RobotIp = model.RobotIp;
                setting.RobotUser = model.RobotUser;
                setting.RobotPassword = model.RobotPassword;
                setting.SqlIp = model.SqlIp;
                setting.Database = model.Database;
                setting.SqlUser = model.SqlUser;
                setting.SqlPassword = model.SqlPassword;
                setting.TrustServerCertificate = model.TrustServerCertificate;
                _context.Settings.Update(setting);
            }
            await _context.SaveChangesAsync();
        }

        public string? GetDynamicConnection()
        {
            var settings = _context.Settings.FirstOrDefault();
            if (settings == null)
                return null;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = settings.SqlIp,
                InitialCatalog = settings.Database,
                UserID = settings.SqlUser,
                Password = settings.SqlPassword,
                TrustServerCertificate = settings.TrustServerCertificate,
                IntegratedSecurity = false
            };

            return builder.ConnectionString;
        }

        public async Task<string> TestDynamicDbAsync()
        {
            var dynamicConn = GetDynamicConnection();
            if (string.IsNullOrEmpty(dynamicConn)) return "Ayarlar bulunamadı!";

            using var conn = new SqlConnection(dynamicConn);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT TOP 1 name FROM sys.tables", conn);
            var result = await cmd.ExecuteScalarAsync();
            return $"Bağlantı başarılı ✅, İlk tablo: {result}";
        }
    }
}
