using System;

namespace FanucRelease.Models.ViewModels
{
    public class GecmisProgramRow
    {
        public int Id { get; set; }
        public string ProgramAdi { get; set; } = "—";
        public int KaynakSayisi { get; set; }
        public string OperatorAdSoyad { get; set; } = "—";
        public string ToplamSureText { get; set; } = "—";
        public string TarihText { get; set; } = "—";
        public int BasariYuzde { get; set; }
        public string BadgeClass { get; set; } = "badge";
    }
}
