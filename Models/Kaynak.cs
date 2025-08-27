using System.ComponentModel.DataAnnotations;

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
        public TimeOnly BaslangicSaati { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly BitisSaati { get; set; }
        
        [DataType(DataType.Time)]
        public TimeOnly ToplamSure { get; set; }

        public int BaslangicSatiri { get; set; }

        public int BitisSatiri { get; set; }

         // Bir KaynakDongusu'na ait birden fazla AnlikKaynak kaydı olabilir
        public virtual ICollection<AnlikKaynak> AnlikKaynaklar { get; set; } = new List<AnlikKaynak>();
    }
}
