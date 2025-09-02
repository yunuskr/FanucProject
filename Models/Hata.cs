using System;

namespace FanucRelease.Models
{
	public enum HataTipi
	{
		Kirmizi, // Kritik hata
		Sari,    // Uyarı
		Mavi     // Bilgilendirme
	}

	public class Hata
	{
		public int Id { get; set; }
		public HataTipi Tip { get; set; }
		public string Aciklama { get; set; } = string.Empty;
		public DateTime Zaman { get; set; } = DateTime.Now;
		// İlgili program ile ilişkilendirmek için ProgramVerisiId eklenebilir
		public int? ProgramVerisiId { get; set; }
	}
}
