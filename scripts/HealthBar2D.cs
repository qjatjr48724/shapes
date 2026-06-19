using Godot;

namespace Shapes;

public partial class HealthBar2D : Node2D
{
    [Export]
    public float BarWidth { get; set; } = 40f;

    [Export]
    public float BarHeight { get; set; } = 5f;

    [Export]
    public float OffsetY { get; set; } = -28f;

    private float _current;
    private float _max;

    public override void _Ready()
    {
        ThemeSettings.Instance.ThemeChanged += OnThemeChanged;
    }

    public override void _ExitTree()
    {
        if (ThemeSettings.Instance != null)
        {
            ThemeSettings.Instance.ThemeChanged -= OnThemeChanged;
        }
    }

    public void SetValues(float current, float max)
    {
        _current = Mathf.Max(current, 0f);
        _max = Mathf.Max(max, 0.001f);
        QueueRedraw();
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var palette = ThemeSettings.Instance.Palette;
        var topLeft = new Vector2(-BarWidth / 2f, OffsetY);
        var bgRect = new Rect2(topLeft, new Vector2(BarWidth, BarHeight));
        DrawRect(bgRect, palette.HealthBarBackground);
        DrawRect(bgRect, palette.HealthBarBorder, false, 1f);

        var fillWidth = BarWidth * (_current / _max);
        if (fillWidth > 0f)
        {
            DrawRect(new Rect2(topLeft, new Vector2(fillWidth, BarHeight)), palette.HealthBarFill);
        }
    }
}
