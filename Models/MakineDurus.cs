using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    /// <summary>
    /// Makine duruş zamanları ve sebep detayları
    /// </summary>
    public class MakineDurus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Duruş Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeOnly BaslangicSaati { get; set; }

        [Required]
        [Display(Name = "Duruş Sebebi")]
        public string Sebep { get; set; } = string.Empty;


        // Foreign Key
        [Required]
        [Display(Name = "Kaynak Döngüsü")]
        public int KaynakDongusuId { get; set; }

        // Navigation Property
        [ForeignKey("KaynakDongusuId")]
        public virtual KaynakDongusu KaynakDongusu { get; set; } = null!;
    }
}
