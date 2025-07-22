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

        [DataType(DataType.Time)]
        public TimeOnly BaslangicSaati { get; set; }

        public string Sebep { get; set; } = string.Empty;


        // Foreign Key
        public int KaynakDongusuId { get; set; }

        // Navigation Property
        [ForeignKey("KaynakDongusuId")]
        public virtual KaynakDongusu KaynakDongusu { get; set; } = null!;
    }
}
