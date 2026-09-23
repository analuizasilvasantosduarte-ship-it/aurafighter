using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Aura Fighter — leitor de chart simples.
/// Formato (1 nota por linha):  tempoSegundos|pista
/// Ex:
///   1.50|0
///   2.00|3
/// Linhas vazias e linhas com # são ignoradas. Pista = 0..3.
/// </summary>
[Serializable]
public class ChartNote
{
    public float time;
    public int lane;
}

public class chartReader
{
    public List<ChartNote> notes = new List<ChartNote>();

    internal object readChartFile(string filePath)
    {
        notes = ReadChartFile(filePath);
        return notes;
    }

    public static List<ChartNote> ReadChartFile(string filePath)
    {
        var list = new List<ChartNote>();
        if (string.IsNullOrEmpty(filePath)) return list;
        if (!System.IO.File.Exists(filePath)) return list;

        foreach (string raw in System.IO.File.ReadAllLines(filePath))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            string[] parts = line.Split('|');
            if (parts.Length < 2) continue;

            float time;
            int lane;
            if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out time)) continue;
            if (!int.TryParse(parts[1].Trim(), out lane)) continue;
            if (lane < 0 || lane > 3) continue;
            if (time < 0) continue;

            list.Add(new ChartNote { time = time, lane = lane });
        }
        list.Sort((a, b) => a.time.CompareTo(b.time));
        return list;
    }
}
