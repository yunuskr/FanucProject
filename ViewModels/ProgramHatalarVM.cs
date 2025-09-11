namespace FanucRelease.ViewModels
{
    public class ProgramHatalarVM
    {
        public int ProgramId { get; set; }
        public string ProgramAdi { get; set; } = string.Empty;
        public string? OperatorAdSoyad { get; set; }
        public List<HataRow> Hatalar { get; set; } = new();

        public class HataRow
        {
            public int Id { get; set; }
            public string Tip { get; set; } = string.Empty;
            public string Aciklama { get; set; } = string.Empty;
            public DateTime Zaman { get; set; }
        }
    }
}
