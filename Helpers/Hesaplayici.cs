
using System.Globalization;

namespace FanucRelease.Services
{
    public class Hesaplayici
    {
        public static TimeOnly milisaniyeyiTimeOnlyeCevir(int milisaniye)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milisaniye);
            return TimeOnly.FromTimeSpan(ts);
        }

        public static DateTime BaslangicaSureEkle(string baslangic_, string sure_)
        {
            // TimeOnly → TimeSpan

            TimeOnly sure = milisaniyeyiTimeOnlyeCevir(int.Parse(sure_));
            TimeSpan ts = sure.ToTimeSpan();
            DateTime baslangic = stringDateParse(baslangic_);
            // DateTime üzerine ekle
            return baslangic.Add(ts);
        }


        public static DateTime stringDateParse(string input)
        {
            // TimeOnly → TimeSpan
            string[] formats = { "dd-MMM-yyHH:mm:ss", "dd-MMM-yyHH:mm" };

            return DateTime.ParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None
            );
        }


        // public static DateTime DateParse(string input)
        // {
        //     // TimeOnly → TimeSpan

        //     DateTime dt = DateTime.ParseExact(
        //     input,
        //     "dd-MMM-yyHH:mm",
        //     CultureInfo.InvariantCulture
        //         );
        //     return dt;
        // }
    }
}
