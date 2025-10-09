using System;
using System.Collections.Generic;

namespace FanucRelease.ViewModels
{
    public class OperatorProgramDetailRow
    {
        public int ProgramId { get; set; }
        public string ProgramAdi { get; set; } = string.Empty;
        public int KaynakToplam { get; set; }
        public int BasariliKaynak { get; set; }
        public int BasarisizKaynak => Math.Max(0, KaynakToplam - BasariliKaynak);
        public int HataSayisi { get; set; }
        public DateTime BitisZamani { get; set; }
        public TimeSpan? ProgramSuresi { get; set; }
        public string ProgramSureText => ProgramSuresi == null ? "—" : FormatSure(ProgramSuresi.Value);
        public int BasariYuzdesi => KaynakToplam > 0 ? (int)Math.Round(BasariliKaynak * 100.0 / KaynakToplam) : 0;

        private static string FormatSure(TimeSpan ts)
        {
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1) return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }
    }

    public class OperatorDetailVM
    {
        public int OperatorId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string KullaniciAdi { get; set; } = string.Empty;
        public IEnumerable<OperatorProgramDetailRow> Programlar { get; set; } = Array.Empty<OperatorProgramDetailRow>();
        public int Days { get; set; }
        public double? MaxProgramHours { get; set; }
        public DateTime OlusturmaZamani { get; set; } = DateTime.Now;
        public int ToplamProgram => Programlar.Count();
        public int ToplamKaynak => Programlar.Sum(p => p.KaynakToplam);
        public int ToplamBasarili => Programlar.Sum(p => p.BasariliKaynak);
        public int ToplamBasarisiz => Programlar.Sum(p => p.BasarisizKaynak);
        public int ToplamHata => Programlar.Sum(p => p.HataSayisi);
        public int GenelBasariYuzdesi => ToplamKaynak > 0 ? (int)Math.Round(ToplamBasarili * 100.0 / ToplamKaynak) : 0;
    }
}