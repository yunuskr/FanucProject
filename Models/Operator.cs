using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace FanucRelease.Models
{

    [Index(nameof(KullaniciAdi), IsUnique = true)]  // 🔹 EKLE BUNU
    public class Operator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        public static implicit operator Operator(string v)
        {
            throw new NotImplementedException();
        }
    }
}