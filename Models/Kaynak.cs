using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    /// <summary>
    /// Kaynak döngüsü ana modeli - Her kaynak işlemini temsil eder
    /// </summary>
    public class Kaynak
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Time)]
        public DateTime BaslangicSaati { get; set; }

        [DataType(DataType.Time)]
        public DateTime BitisSaati { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly ToplamSure { get; set; }
        public int BaslangicSatiri { get; set; }
        public int KaynakUzunlugu { get; set; }
        public int BitisSatiri { get; set; }
        public int PrcNo { get; set; }
        public int SrcNo { get; set; }

        [ForeignKey("ProgramVerisiId")]
        public virtual ProgramVerisi ProgramVerisi { get; set; } = null!;
        // Bir KaynakDongusu'na ait birden fazla AnlikKaynak kaydı olabilir
        public virtual ICollection<AnlikKaynak> AnlikKaynaklar { get; set; } = new List<AnlikKaynak>();
    }
}
