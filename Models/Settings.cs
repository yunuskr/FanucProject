public class Setting
{
    public int Id { get; set; }
    public string RobotIp { get; set; } = string.Empty;
    public string RobotUser { get; set; } = string.Empty;
    public string RobotPassword { get; set; } = string.Empty;

    public string SqlIp { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string SqlUser { get; set; } = string.Empty;
    public string SqlPassword { get; set; } = string.Empty;

    public bool TrustServerCertificate { get; set; }
}
