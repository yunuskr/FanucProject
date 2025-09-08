public class Setting
{
    public int Id { get; set; }

    // Robot ayarları
    public string RobotIp { get; set; }
    public string RobotUser { get; set; }
    public string RobotPassword { get; set; }

    // SQL ayarları
    public string SqlIp { get; set; }
    public string Database { get; set; }
    public string SqlUser { get; set; }
    public string SqlPassword { get; set; }

    public bool TrustServerCertificate { get; set; }
}
