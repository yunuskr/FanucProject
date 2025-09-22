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
