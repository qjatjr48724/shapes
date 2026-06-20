using Godot;

namespace Shapes;

public partial class HomingBullet : Area2D
{
    private float _lifetime;
    private int _damage = 1;
    private Node2D? _target;

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

    public void Initialize(int damage)
    {
        _damage = damage;
        _target = FindTarget();
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

        if (_target == null || !GodotObject.IsInstanceValid(_target))
        {
            _target = FindTarget();
        }

        var direction = Vector2.Up;
        if (_target != null)
        {
            direction = (_target.GlobalPosition - GlobalPosition).Normalized();
        }

        GlobalPosition += direction * GameConstants.HomingBulletSpeed * (float)delta;
        Rotation = direction.Angle() + Mathf.Pi / 2f;

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
        DrawCircle(Vector2.Zero, 7f, new Color(0.95f, 0.55f, 0.95f));
        DrawArc(Vector2.Zero, 7f, 0f, Mathf.Tau, 16, ThemeSettings.Instance.Palette.ShapeOutline, 1.5f);
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }

    private Node2D? FindTarget()
    {
        Node2D? closest = null;
        var closestDistance = float.MaxValue;

        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Node2D enemy)
            {
                continue;
            }

            var distance = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        foreach (var node in GetTree().GetNodesInGroup("bosses"))
        {
            if (node is not Node2D boss)
            {
                continue;
            }

            var distance = GlobalPosition.DistanceSquaredTo(boss.GlobalPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = boss;
            }
        }

        return closest;
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
