using System;
using System.Collections.Generic;
using FanucRelease.Models;

namespace FanucRelease.ViewModels
{
    public class RaporlarIndexVM
    {
        // Filters
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? OperatorId { get; set; }

        // Summary
        public int ToplamProgram { get; set; }
        public int ToplamKaynak { get; set; }
        public int BasariliKaynak { get; set; }
        public int BasarisizKaynak => Math.Max(0, ToplamKaynak - BasariliKaynak);
        public int ToplamHata { get; set; }
        public int BasariYuzdesi => ToplamKaynak > 0 ? (int)Math.Round(BasariliKaynak * 100.0 / ToplamKaynak) : 0;

        // Leaderboards / Top lists
        public IEnumerable<OperatorPerformanceRow> TopOperatorlar { get; set; } = Array.Empty<OperatorPerformanceRow>();
    public IEnumerable<FanucRelease.Models.ViewModels.GecmisProgramRow> TopProgramlar { get; set; } = Array.Empty<FanucRelease.Models.ViewModels.GecmisProgramRow>();
        public IEnumerable<ProgramDurationRow> UzunSurenProgramlar { get; set; } = Array.Empty<ProgramDurationRow>();

        // Error trend (placeholder structure)
        public List<(DateTime Date, int Count)> HataTrend { get; set; } = new();

        // Downtime (placeholder for future)
        public int ToplamDurusAdet { get; set; }
        public TimeSpan ToplamDurusSuresi { get; set; }

        // Executive KPIs: Current vs Previous period
        public int OncekiToplamProgram { get; set; }
        public int OncekiToplamKaynak { get; set; }
        public int OncekiBasariliKaynak { get; set; }
        public int OncekiToplamHata { get; set; }
        public int OncekiBasariYuzdesi => OncekiToplamKaynak > 0 ? (int)Math.Round(OncekiBasariliKaynak * 100.0 / OncekiToplamKaynak) : 0;

        public int DeltaProgram => ToplamProgram - OncekiToplamProgram;
        public int DeltaKaynak => ToplamKaynak - OncekiToplamKaynak;
        public int DeltaHata => ToplamHata - OncekiToplamHata;
        public int DeltaBasariYuzdesi => BasariYuzdesi - OncekiBasariYuzdesi;

        // Daily trend: program and success per day
        public List<(DateTime Date, int Program, int Kaynak, int BasariYuzdesi)> GunlukTrend { get; set; } = new();

        // Top Errors by code/description
        public List<(string Kod, string Aciklama, int Adet)> TopHatalar { get; set; } = new();

        // Compare section (Durus Ozeti replacement)
        public string CompareMode { get; set; } = "weekly"; // daily | weekly | monthly | yearly
        public DateTime? CompareABase { get; set; }
        public DateTime? CompareBBase { get; set; }
    public (string Label, int Program, int Kaynak, int Hata, int SureDakika) CompareA { get; set; }
    public (string Label, int Program, int Kaynak, int Hata, int SureDakika) CompareB { get; set; }
    public (int Program, int Kaynak, int Hata, int SureDakika) CompareDelta { get; set; }

        // Weekly selection UI (Year/Month/Week-of-month) for A and B
        public int AYear { get; set; }
        public int AMonth { get; set; }
        public int AWeek { get; set; } // 1..4

        public int BYear { get; set; }
        public int BMonth { get; set; }
        public int BWeek { get; set; } // 1..4
    }
}
