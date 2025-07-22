using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    /// <summary>
    /// Kaynatılan trafo bilgileri
    /// </summary>
    public class TrafoBilgisi
    {
        [Key]
        public int Id { get; set; }


        [Display(Name = "Trafo Adı")]
        public string TrafoAdi { get; set; } = string.Empty;

        // Foreign Key
        public int KaynakDongusuId { get; set; }

        // Navigation Property
        [ForeignKey("KaynakDongusuId")]
        public virtual KaynakDongusu KaynakDongusu { get; set; } = null!;
    }
}
