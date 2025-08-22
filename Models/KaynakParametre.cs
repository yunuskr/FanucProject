using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    /// <summary>
    /// Kaynak esnasındaki Voltaj, Amper ve Tel sürme hızı bilgileri
    /// </summary>
    public class KaynakParametre
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Ölçüm Zamanı")]
        [DataType(DataType.Time)]
        public TimeOnly OlcumZamani { get; set; }

        [Display(Name = "Voltaj (V)")]
        public decimal Voltaj { get; set; }

        [Display(Name = "Amper (A)")]
        public decimal Amper { get; set; }

        [Display(Name = "Tel Sürme Hızı")]
        public decimal TelSurmeHizi { get; set; }

    // Foreign Key -> Kaynak tablosuna bağlanır
    public int KaynakId { get; set; }

    // Navigation Property
    [ForeignKey("KaynakId")]
    public virtual Kaynak Kaynak { get; set; } = null!;
    }
}
