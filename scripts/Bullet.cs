using Godot;

namespace Shapes;

public partial class Bullet : Area2D
{
    private Vector2 _direction = Vector2.Up;
    private float _lifetime;
    private int _damage = GameConstants.BulletDamage;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
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

    public void Initialize(Vector2 direction, int damage)
    {
        _direction = direction.Normalized();
        _damage = damage;
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

        GlobalPosition += _direction * GameConstants.BulletSpeed * (float)delta;

        _lifetime += (float)delta;
        if (_lifetime >= GameConstants.BulletLifetime)
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
        DrawCircle(Vector2.Zero, 8f, ThemeSettings.Instance.Palette.PlayerBullet);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(_damage);
            QueueFree();
            return;
        }

        if (body is Boss boss)
        {
            boss.TakeDamage(_damage);
            QueueFree();
        }
    }
}
