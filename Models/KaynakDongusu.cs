using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    /// <summary>
    /// Kaynak döngüsü ana modeli - Her kaynak işlemini temsil eder
    /// </summary>
    public class KaynakDongusu
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly BaslangicSaati { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly? BitisSaati { get; set; }

        public double? ToplamSureSaniye { get; set; }

        public bool Tamamlandi { get; set; } = false;

        [DataType(DataType.Date)]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Today;

        // Foreign Key
        public int OperatorId { get; set; }

        // Navigation Property
        [ForeignKey("OperatorId")]
        public virtual Operator Operator { get; set; } = null!;

        // Hesaplanmış özellik - Dakika cinsinden süre
        public double? ToplamSureDakika => ToplamSureSaniye.HasValue ? ToplamSureSaniye.Value / 60 : null;
    }
}
