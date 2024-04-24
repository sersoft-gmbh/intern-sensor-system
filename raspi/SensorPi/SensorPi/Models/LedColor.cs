namespace SensorPi.Models;

public readonly record struct LedColor {
    private enum ColorPart {
        Red,
        Green,
        Blue,
    }

    private readonly HashSet<ColorPart> _colors = [];

    private LedColor(HashSet<ColorPart> colors) {
        _colors = colors;
    }
    
    public static LedColor Off => new([]);

    public static LedColor Red => new([ColorPart.Red]);
    public static LedColor Green => new([ColorPart.Green]);
    public static LedColor Blue => new([ColorPart.Blue]);

    public LedColor WithRed() {
        _colors.Add(ColorPart.Red);
        return this;
    }

    public LedColor WithGreen() {
        _colors.Add(ColorPart.Green);
        return this;
    }

    public LedColor WithBlue() {
        _colors.Add(ColorPart.Blue);
        return this;
    }

    public bool HasRed() => _colors.Contains(ColorPart.Red);
    public bool HasGreen() => _colors.Contains(ColorPart.Green);
    public bool HasBlue() => _colors.Contains(ColorPart.Blue);
}