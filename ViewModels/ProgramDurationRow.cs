using System;

namespace FanucRelease.ViewModels
{
    public class ProgramDurationRow
    {
        public int ProgramId { get; set; }
        public string ProgramAdi { get; set; } = "—";
        public string? OperatorAdSoyad { get; set; }
        public int KaynakSayisi { get; set; }
        public int BasariYuzdesi { get; set; }
        public TimeSpan Sure { get; set; }
        public string SureText => Sure.TotalHours >= 1
            ? $"{(int)Sure.TotalHours}h {Sure.Minutes}m"
            : (Sure.TotalMinutes >= 1 ? $"{Sure.Minutes}m {Sure.Seconds}s" : $"{Sure.Seconds}s");
    }
}
