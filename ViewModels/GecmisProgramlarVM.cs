namespace FanucRelease.ViewModels
{
    public class GecmisProgramRowVM
    {
        public int Id { get; set; }
        public string ProgramAdi { get; set; } = string.Empty;
        public int KaynakSayisi { get; set; }
        public string? OperatorAdSoyad { get; set; }
        public string ToplamSureText { get; set; } = "—";
        public DateTime? Tarih { get; set; }    // Programın bitiş tarihi gibi
        public string TarihText => Tarih.HasValue ? Tarih.Value.ToString("dd.MM.yyyy") : "—";
        public int BasariYuzde { get; set; } = 100; // Şimdilik sabit 100
        public string BadgeClass { get; set; } = "badge badge-success"; // Kaynak sayısına göre renk
    }

    public class GecmisProgramListVM
    {
        public List<GecmisProgramRowVM> Items { get; set; } = new();
    }
}
