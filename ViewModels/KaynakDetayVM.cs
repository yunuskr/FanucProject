using FanucRelease.Models;
using System.Collections.Generic;

namespace FanucRelease.ViewModels
{
    public class KaynakDetayVM
    {
        public Kaynak Kaynak { get; set; } = null!;
        public List<AnlikKaynak> Ornekler { get; set; } = new();

        public double OrtalamaVoltaj { get; set; }
        public double OrtalamaAmper { get; set; }
        public double OrtalamaTelSurme { get; set; }
        public double OrtalamaKaynakHizi { get; set; }
    }
}
