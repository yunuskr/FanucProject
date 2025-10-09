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
    }
}
