using System.ComponentModel.DataAnnotations;
using FanucRelease.Models;

public class RobotVerisi
{
    public int Id { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly OlcumZamani { get; set; }

    public string ProgramAdi { get; set; } = string.Empty;

    public int SatirNumarasi { get; set; }

    public string Durum { get; set; } = "Calisiyor"; // Calisiyor, Durdu, Hata

    public decimal HizYuzdesi { get; set; }

    public string? HataKodu { get; set; }

    public int KaynakDongusuId { get; set; }
    public virtual KaynakDongusu KaynakDongusu { get; set; } = null!;
}
