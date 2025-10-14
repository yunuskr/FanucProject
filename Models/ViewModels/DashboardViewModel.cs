using System;
using System.Collections.Generic;
using FanucRelease.Models;

namespace FanucRelease.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Header stats
        public int ProgramCount { get; set; }
        public int ToplamKaynak { get; set; }
        public int BasariliKaynak { get; set; }
        public int HataToplam { get; set; }
        public string BugunToplamKaynakSureText { get; set; } = "—";
    // Daily time trend (today vs yesterday)
    public int TodayTimePercent { get; set; }
    public string TodayTimeTrendClass { get; set; } = "trend-up";
    public string TodayTimeTrendIcon { get; set; } = "fas fa-arrow-up";
    public string TodayTimeTrendText { get; set; } = "+0% bugün";

        // Weekly trend for ProgramCount (this week vs last week)
        public int ThisWeekProgramCount { get; set; }
        public int LastWeekProgramCount { get; set; }
        public int ProgramCountWoWPercent { get; set; }
        public string ProgramCountTrendClass { get; set; } = "trend-up"; // or "trend-down"
        public string ProgramCountTrendIcon { get; set; } = "fas fa-arrow-up"; // or arrow-down
        public string ProgramCountTrendText { get; set; } = "+0% bu hafta";

    // Weekly trend for Errors (Hata)
    public int ThisWeekHataCount { get; set; }
    public int LastWeekHataCount { get; set; }
    public int HataWoWPercent { get; set; }
    public string HataTrendClass { get; set; } = "trend-down"; // Down is good for errors; CSS still colors by card type
    public string HataTrendIcon { get; set; } = "fas fa-arrow-down";
    public string HataTrendText { get; set; } = "0% bu hafta";

        // Latest program card
        public ProgramVerisi? SonProgram { get; set; }
        public int SonProgramToplam { get; set; }
        public int SonProgramBasarili { get; set; }
        public int SonProgramBasarisiz { get; set; }
        public int SonProgramBasariYuzde { get; set; }
        public string SonProgramBaslangicText { get; set; } = "—";
        public string SonProgramBitisText { get; set; } = "—";

        // Last successful kaynak of latest program
        public Kaynak? SonBasariliKaynak { get; set; }
        public string SonBasariliKaynakToplamSureText { get; set; } = "—";

    // History table rows (typed)
    public IEnumerable<GecmisProgramRow> GecmisProgramlar { get; set; } = Array.Empty<GecmisProgramRow>();
    }
}
