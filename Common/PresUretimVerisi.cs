using InfluxDB.Client.Core;

namespace PresVeriToplama.Common;

[Measurement("uretim")]
public class PresUretimVerisi : IKaydedilcekVeri
{
    [Column(IsTimestamp = true)] public DateTime Time { get; set; }


    [Column("TabakDustu")] public int TabakDustu { get; set; }

    [Column("TabakAl")] public int TabakAl { get; set; }

    public PresUretimVerisi(int tabakAl, int tabakDustu, DateTime now)
    {
        TabakAl = tabakAl;
        TabakDustu = tabakDustu;
        Time = now;
    }
}