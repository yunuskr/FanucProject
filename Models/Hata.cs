using System;

namespace FanucRelease.Models
{

	public class Hata
	{
		public int Id { get; set; }

		public bool KaynakAnindaMi { get; set; } = false;

		public string? KaynakAdi { get; set; } = null;
		public int Tip { get; set; }
		public string Kod { get; set; } = string.Empty;
		public string Aciklama { get; set; } = string.Empty;
		public DateTime Zaman { get; set; } = DateTime.Now;
		// İlgili program ile ilişkilendirmek için ProgramVerisiId eklenebilir
		public int? ProgramVerisiId { get; set; }
		public ProgramVerisi ProgramVerisi { get; set; }

	}
}
