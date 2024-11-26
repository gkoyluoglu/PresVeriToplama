using InfluxDB.Client.Core;

namespace PresVeriToplama.Common;

[Measurement("uretim")]
public class YuRoUretimVerisi : IRotus
{
    private int _fireAdeti;
    private int _uretimAdeti;

    [Column(IsTimestamp = true)] public DateTime Time { get; set; }

    [Column("RotusFireAdet")]
    public int FireAdeti
    {
        get => _fireAdeti;
        set => _fireAdeti = value;
    }

    [Column("RotusUretimAdet")]
    public int UretimAdeti
    {
        get => _uretimAdeti;
        set => _uretimAdeti = value;
    }

    public YuRoUretimVerisi(int uretimAdeti , int fireAdeti , DateTime now)
    {
        _uretimAdeti = uretimAdeti;
        _fireAdeti = fireAdeti;
        Time = now;
    }
}