using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FanucRelease.Models
{
    public class AnlikKaynak
    {
        [Key]
        public int Id { get; set; }

        // Ölçüm zamanı
        public DateTime OlcumZamani { get; set; }

        public double Voltaj { get; set; }

        public double Amper { get; set; }

        public double TelSurmeHizi { get; set; }

        // Opsiyonel ek alanlar
        public int ProgramSatiri { get; set; }

        public int KaynakHizi { get; set; }

        // Foreign key (Kaynak döngüsüne bağlamak için)
        public int KaynakId { get; set; }

    [ForeignKey("KaynakId")]
    public virtual Kaynak Kaynak { get; set; } = null!;
    }
}