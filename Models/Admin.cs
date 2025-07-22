using System.ComponentModel.DataAnnotations;

namespace FanucRelease.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
    }
}