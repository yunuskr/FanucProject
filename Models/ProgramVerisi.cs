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

    // Foreign keys made nullable so ProgramVerisi can be recorded without requiring existing Operator/Kaynak rows
    public int? OperatorId { get; set; }

    // Navigation Property
    [ForeignKey("OperatorId")]
    public virtual Operator? Operator { get; set; }


    public int? KaynakId { get; set; }
    public virtual Kaynak? Kaynak { get; set; }
}
