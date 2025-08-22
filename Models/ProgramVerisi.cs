using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FanucRelease.Models;

public class ProgramVerisi
{
    public int Id { get; set; }

    public string ProgramAdi { get; set; } = string.Empty;

    public string Durum { get; set; } = "Basladi"; // Basladi, Bitti, Hata

    public string? HataKodu { get; set; }

    public int KaynakSayisi { get; set; }

    public int OperatorId { get; set; }

    // Navigation Property
    [ForeignKey("OperatorId")]
    public virtual Operator Operator { get; set; } = null!;


    public int KaynakId { get; set; }
    public virtual Kaynak Kaynak { get; set; } = null!;
}
