using Godot;

namespace Shapes;

public partial class BossBullet : Area2D
{
    private Vector2 _direction = Vector2.Down;
    private float _lifetime;

    public override void _Ready()
    {
        ThemeSettings.Instance.ThemeChanged += OnThemeChanged;
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (ThemeSettings.Instance != null)
        {
            ThemeSettings.Instance.ThemeChanged -= OnThemeChanged;
        }
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }

    public void Initialize(Vector2 direction)
    {
        _direction = direction.Normalized();
        Rotation = _direction.Angle() + Mathf.Pi / 2f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.IsGameOver)
        {
            QueueFree();
            return;
        }

        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        GlobalPosition += _direction * GameConstants.BossBulletSpeed * (float)delta;
        _lifetime += (float)delta;

        if (_lifetime >= GameConstants.BossBulletLifetime)
        {
            QueueFree();
            return;
        }

        var rect = GetViewport().GetVisibleRect();
        if (!rect.HasPoint(GlobalPosition))
        {
            QueueFree();
        }
    }

    public override void _Draw()
    {
        var palette = ThemeSettings.Instance.Palette;
        DrawCircle(Vector2.Zero, 10f, palette.BossBulletFill);
        DrawArc(Vector2.Zero, 10f, 0f, Mathf.Tau, 24, palette.BossBulletOutline, 1.5f);
    }
}
