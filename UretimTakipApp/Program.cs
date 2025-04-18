using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class ProductionRecord
{
    public int KayitNo { get; set; }
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public string Statu { get; set; } = "";

}

public class StandardBreak
{
    public TimeSpan BaslangicSaati { get; set; }
    public TimeSpan BitisSaati { get; set; }
    public string DurusNedeni { get; set; } = string.Empty;
}

public class DetailedReportEntry
{
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }

    public TimeSpan Sure => Bitis - Baslangic;
    public string Statu { get; set; } = string.Empty;
    public string? DurusNedeni { get; set; }


    public override string ToString()
    {

        string formattedDuration = $"{(int)Sure.TotalHours:00}:{Sure.Minutes:00}";

        return $"| {Baslangic:dd.MM.yyyy HH:mm} | {Bitis:dd.MM.yyyy HH:mm} | {formattedDuration} | {Statu,-15} | {DurusNedeni ?? "",-15} |";
    }
}

public class Program
{

    public static void Main(string[] args)
    {

        CultureInfo culture = new CultureInfo("tr-TR");

        var uretimKayitlari = new List<ProductionRecord>
        {

            new ProductionRecord { KayitNo = 1, Baslangic = DateTime.ParseExact("23.05.2020 07:30", "dd.MM.yyyy HH:mm", culture), Bitis = DateTime.ParseExact("23.05.2020 08:30", "dd.MM.yyyy HH:mm", culture), Statu = "ÜRETİM" },
            new ProductionRecord { KayitNo = 2, Baslangic = DateTime.ParseExact("23.05.2020 08:30", "dd.MM.yyyy HH:mm", culture), Bitis = DateTime.ParseExact("23.05.2020 12:00", "dd.MM.yyyy HH:mm", culture), Statu = "ÜRETİM" },
            new ProductionRecord { KayitNo = 3, Baslangic = DateTime.ParseExact("23.05.2020 12:00", "dd.MM.yyyy HH:mm", culture), Bitis = DateTime.ParseExact("23.05.2020 13:00", "dd.MM.yyyy HH:mm", culture), Statu = "ÜRETİM" },

            new ProductionRecord { KayitNo = 4, Baslangic = DateTime.ParseExact("23.05.2020 13:00", "dd.MM.yyyy HH:mm", culture), Bitis = DateTime.ParseExact("23.05.2020 13:45", "dd.MM.yyyy HH:mm", culture), Statu = "ARIZA" },
            new ProductionRecord { KayitNo = 5, Baslangic = DateTime.ParseExact("23.05.2020 13:45", "dd.MM.yyyy HH:mm", culture), Bitis = DateTime.ParseExact("23.05.2020 17:30", "dd.MM.yyyy HH:mm", culture), Statu = "ÜRETİM" }
        };



        var standartDuruslar = new List<StandardBreak>
        {

            new StandardBreak { BaslangicSaati = new TimeSpan(10, 00, 0), BitisSaati = new TimeSpan(10, 15, 0), DurusNedeni = "Çay Molası" },

            new StandardBreak { BaslangicSaati = new TimeSpan(12, 00, 0), BitisSaati = new TimeSpan(12, 30, 0), DurusNedeni = "Yemek Molası" },
            new StandardBreak { BaslangicSaati = new TimeSpan(15, 00, 0), BitisSaati = new TimeSpan(15, 15, 0), DurusNedeni = "Çay Molası" }
        };




        var detayliRapor = new List<DetailedReportEntry>();




        foreach (var kayit in uretimKayitlari.OrderBy(k => k.Baslangic))
        {

            DateTime currentTime = kayit.Baslangic;




            var applicableBreaks = new List<(DateTime Start, DateTime End, string Reason)>();
            DateTime currentDate = kayit.Baslangic.Date;


            while (currentDate <= kayit.Bitis.Date)
            {

                foreach (var durus in standartDuruslar)
                {


                    DateTime breakStart = currentDate.Add(durus.BaslangicSaati);
                    DateTime breakEnd = currentDate.Add(durus.BitisSaati);



                    if (breakStart < kayit.Bitis && breakEnd > kayit.Baslangic)
                    {



                        DateTime effectiveBreakStart = breakStart > kayit.Baslangic ? breakStart : kayit.Baslangic;

                        DateTime effectiveBreakEnd = breakEnd < kayit.Bitis ? breakEnd : kayit.Bitis;


                        if (effectiveBreakEnd > effectiveBreakStart)
                        {

                            applicableBreaks.Add((effectiveBreakStart, effectiveBreakEnd, durus.DurusNedeni));
                        }
                    }
                }


                if (kayit.Bitis.Date > currentDate)
                {
                    currentDate = currentDate.AddDays(1);
                }
                else
                {

                    break;
                }
            }



            applicableBreaks = applicableBreaks.OrderBy(b => b.Start).ToList();
            int breakIndex = 0;




            while (currentTime < kayit.Bitis)
            {


                var sonrakiDurus = applicableBreaks
                                    .Where((b, index) => index >= breakIndex && b.Start >= currentTime)
                                    .OrderBy(b => b.Start)
                                    .FirstOrDefault();


                if (sonrakiDurus != default && sonrakiDurus.Start < kayit.Bitis)
                {


                    if (sonrakiDurus.Start > currentTime)
                    {
                        detayliRapor.Add(new DetailedReportEntry
                        {
                            Baslangic = currentTime,
                            Bitis = sonrakiDurus.Start,
                            Statu = kayit.Statu
                        });
                    }




                    DateTime durusBitisZamani = sonrakiDurus.End < kayit.Bitis ? sonrakiDurus.End : kayit.Bitis;
                    detayliRapor.Add(new DetailedReportEntry
                    {
                        Baslangic = sonrakiDurus.Start,
                        Bitis = durusBitisZamani,
                        Statu = "DURUŞ",
                        DurusNedeni = sonrakiDurus.Reason
                    });


                    currentTime = durusBitisZamani;

                    breakIndex++;
                }
                else
                {


                    detayliRapor.Add(new DetailedReportEntry
                    {
                        Baslangic = currentTime,
                        Bitis = kayit.Bitis,
                        Statu = kayit.Statu
                    });

                    currentTime = kayit.Bitis;
                }
            }
        }


        Console.WriteLine("Tablo 3: Üretim Operasyon Bildirimleri");
        Console.WriteLine(new string('-', 87));




        Console.WriteLine($"| {"No",-1}| {"Başlangıç",-16} | {"Bitiş",-16} | {"Süre",-5} | {"Statü",-15} | {"Duruş Nedeni",-15} |");

        Console.WriteLine(new string('-', 87));

        int kayitNo = 0;

        foreach (var entry in detayliRapor.OrderBy(e => e.Baslangic))
        {


            if (entry.Sure.TotalSeconds <= 0) continue;

            kayitNo++;

            string formattedDuration = $"{(int)entry.Sure.TotalHours:00}:{entry.Sure.Minutes:00}";


            Console.WriteLine($"|{kayitNo,3}| {entry.Baslangic:dd.MM.yyyy HH:mm} | {entry.Bitis:dd.MM.yyyy HH:mm} | {formattedDuration,-5} | {entry.Statu,-15} | {entry.DurusNedeni ?? "",-15} |");
        }
        Console.WriteLine(new string('-', 80));
    }
}