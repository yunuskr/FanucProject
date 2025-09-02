using Microsoft.Data.SqlClient;
using FanucRelease.Data; // DbContext namespace
using FanucRelease.Models; // Setting modeli

public static class ConnectionHelper
{
    public static string? GetDynamicConnection(ApplicationDbContext context)
    {
        var settings = context.Settings.FirstOrDefault();
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
}
