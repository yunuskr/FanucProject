
namespace FanucRelease.Services
{
    public class Hesaplayici
    {
        public static TimeOnly HesaplaSure(TimeOnly baslangic, TimeOnly bitis)
        {
            var baslangicSpan = baslangic.ToTimeSpan();
            var bitisSpan = bitis.ToTimeSpan();

            TimeSpan fark;
            if (bitisSpan < baslangicSpan)
            {
                // Gece yarısını geçtiyse 1 gün ekliyoruz
                fark = (bitisSpan + TimeSpan.FromDays(1)) - baslangicSpan;
            }
            else
            {
                fark = bitisSpan - baslangicSpan;
            }

            return TimeOnly.FromTimeSpan(fark);
        }
    }
}
