using FanucRelease.Models;

namespace FanucRelease.ViewModels
{
    public class ProgramDetayIndexVM
    {
        public ProgramVerisi Program { get; set; } = null!;
        public IEnumerable<ProgramVerisi> SonProgramlar { get; set; } = Enumerable.Empty<ProgramVerisi>();
    }
}
