using System;
using System.Collections.Generic;

namespace FanucRelease.ViewModels
{
    public class OperatorPerformanceRow
    {
        public int OperatorId { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public int ProgramSayisi { get; set; }
        public int ToplamKaynak { get; set; }
        public int BasariliKaynak { get; set; }
        public int BasarisizKaynak => Math.Max(0, ToplamKaynak - BasariliKaynak);
        public int HataSayisi { get; set; }
        public int BasariYuzdesi => ToplamKaynak > 0 ? (int)Math.Round(BasariliKaynak * 100.0 / ToplamKaynak) : 0;
        public DateTime? SonProgramZamani { get; set; }
        public TimeSpan? OrtalamaProgramSuresi { get; set; }
        public string OrtalamaProgramSureText => OrtalamaProgramSuresi == null ? "—" : FormatSure(OrtalamaProgramSuresi.Value);
        public string SonProgramZamanText => SonProgramZamani?.ToString("yyyy-MM-dd HH:mm") ?? "—";

        private static string FormatSure(TimeSpan ts)
        {
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1) return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }
    }

    public class OperatorPerformanceIndexVM
    {
        public IEnumerable<OperatorPerformanceRow> Operatorlar { get; set; } = Array.Empty<OperatorPerformanceRow>();
        public DateTime OlusturmaZamani { get; set; } = DateTime.Now;
        public int MinProgram { get; set; }
        public int Days { get; set; }
        public double? MaxProgramHours { get; set; }
        // Summary totals (top level quick glance metrics)
        public int ToplamProgram { get; set; }
        public int ToplamKaynak { get; set; }
        public int ToplamBasariliKaynak { get; set; }
        public int ToplamBasarisizKaynak => Math.Max(0, ToplamKaynak - ToplamBasariliKaynak);
        public int ToplamHata { get; set; }
        public int GenelBasariYuzdesi => ToplamKaynak > 0 ? (int)Math.Round(ToplamBasariliKaynak * 100.0 / ToplamKaynak) : 0;
    }
}
