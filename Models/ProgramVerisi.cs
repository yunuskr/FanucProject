using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FanucRelease.Models;
namespace FanucRelease.Models;
public class ProgramVerisi
{
    public int Id { get; set; }
    
    [Required, Display(Name = "Program Adı")]
    public string ProgramAdi { get; set; } = string.Empty;

    [Display(Name = "Kaynak Sayısı")]
    public int KaynakSayisi { get; set; }
    public int? OperatorId { get; set; }

    [ForeignKey("OperatorId")]
    public virtual Operator? Operator { get; set; }
    public virtual ICollection<Kaynak> Kaynaklar { get; set; } = new List<Kaynak>();
    public virtual ICollection<Hata> Hatalar { get; set; } = new List<Hata>();

}
