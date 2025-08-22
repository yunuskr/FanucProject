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


    // Foreign Key -> Kaynak tablosuna bağlanır
    public int KaynakId { get; set; }

    // Navigation Property
    [ForeignKey("KaynakId")]
    public virtual Kaynak Kaynak { get; set; } = null!;
    }
}
