using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FanucRelease.Models;

public class ProgramVerisi
{
    public int Id { get; set; }

    public string ProgramAdi { get; set; } = string.Empty;



    public int KaynakSayisi { get; set; }

    // Foreign keys made nullable so ProgramVerisi can be recorded without requiring existing Operator/Kaynak rows
    public int? OperatorId { get; set; }

    // Navigation Property
    [ForeignKey("OperatorId")]
    public virtual Operator? Operator { get; set; }

    public virtual ICollection<Kaynak> Kaynaklar { get; set; } = new List<Kaynak>();

    public virtual ICollection<Hata> Hatalar { get; set; } = new List<Hata>();

}
