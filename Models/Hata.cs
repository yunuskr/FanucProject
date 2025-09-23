using System;

namespace FanucRelease.Models
{


	public class Hata
	{
		public int Id { get; set; }
		public int Tip { get; set; } //1: Kırmızı, 2: Sarı
		                             // public HataTipi Tip { get; set; }
		public string Kod { get; set; } = string.Empty;  
		public string Aciklama { get; set; } = string.Empty;
		public DateTime Zaman { get; set; } = DateTime.Now;
		// İlgili program ile ilişkilendirmek için ProgramVerisiId eklenebilir
		public int? ProgramVerisiId { get; set; }
	}
}
