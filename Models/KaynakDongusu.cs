using System.ComponentModel.DataAnnotations;

namespace FanucRelease.Models
{
    /// <summary>
    /// Kaynak döngüsü ana modeli - Her kaynak işlemini temsil eder
    /// </summary>
    public class KaynakDongusu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeOnly BaslangicSaati { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeOnly? BitisSaati { get; set; }

        [Display(Name = "Toplam Kaynak Zamanı (saniye)")]
        public double? ToplamSureSaniye { get; set; }

        [Display(Name = "Döngü Tamamlandı")]
        public bool Tamamlandi { get; set; } = false;

        [Display(Name = "Oluşturulma Tarihi")]
        [DataType(DataType.Date)]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Today;

        // Hesaplanmış özellik - Dakika cinsinden süre
        [Display(Name = "Toplam Süre (dakika)")]
        public double? ToplamSureDakika => ToplamSureSaniye.HasValue ? ToplamSureSaniye.Value / 60 : null;
    }
}
