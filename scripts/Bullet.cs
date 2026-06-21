using System.Collections.Generic;
using Godot;

namespace Shapes;

public partial class Bullet : Area2D
{
    private Vector2 _direction = Vector2.Up;
    private float _lifetime;
    private int _damage = GameConstants.BulletDamage;
    private bool _despawnAtScreenTop;
    private float _speed = GameConstants.BulletSpeed;
    private int _remainingPierces;
    private int _remainingBounces;
    private readonly HashSet<ulong> _hitTargets = new();
    private const float BulletRadius = 8f;

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

    public void Initialize(
        Vector2 direction,
        int damage,
        bool despawnAtScreenTop = false,
        float speed = GameConstants.BulletSpeed,
        int pierceCount = 0,
        int bounceCount = 0)
    {
        _direction = direction.Normalized();
        _damage = damage;
        _despawnAtScreenTop = despawnAtScreenTop;
        _speed = speed;
        _remainingPierces = pierceCount;
        _remainingBounces = bounceCount;
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

        GlobalPosition += _direction * _speed * (float)delta;

        var rect = GetViewport().GetVisibleRect();
        HandleVerticalWallBounce(rect);

        if (_despawnAtScreenTop)
        {
            if (GlobalPosition.Y <= rect.Position.Y + BulletRadius)
            {
                QueueFree();
                return;
            }

            if (GlobalPosition.X < rect.Position.X - BulletRadius
                || GlobalPosition.X > rect.End.X + BulletRadius)
            {
                QueueFree();
            }

            return;
        }

        _lifetime += (float)delta;
        if (_lifetime >= GameConstants.BulletLifetime)
        {
            QueueFree();
            return;
        }

        if (!rect.HasPoint(GlobalPosition))
        {
            QueueFree();
        }
    }

    private void HandleVerticalWallBounce(Rect2 rect)
    {
        if (_remainingBounces <= 0)
        {
            return;
        }

        var minX = rect.Position.X + BulletRadius;
        var maxX = rect.End.X - BulletRadius;
        var bounced = false;

        if (GlobalPosition.X < minX)
        {
            GlobalPosition = new Vector2(minX, GlobalPosition.Y);
            _direction = _direction.Bounce(Vector2.Right);
            bounced = true;
        }
        else if (GlobalPosition.X > maxX)
        {
            GlobalPosition = new Vector2(maxX, GlobalPosition.Y);
            _direction = _direction.Bounce(Vector2.Left);
            bounced = true;
        }

        if (!bounced)
        {
            return;
        }

        _remainingBounces--;
        Rotation = _direction.Angle() + Mathf.Pi / 2f;
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 8f, ThemeSettings.Instance.Palette.PlayerBullet);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            DamageTarget(enemy);
            return;
        }

        if (body is Boss boss)
        {
            DamageTarget(boss);
        }
    }

    private void DamageTarget(Node2D target)
    {
        var instanceId = target.GetInstanceId();
        if (_hitTargets.Contains(instanceId))
        {
            return;
        }

        _hitTargets.Add(instanceId);

        if (target is Enemy enemy)
        {
            enemy.TakeDamage(_damage);
        }
        else if (target is Boss boss)
        {
            boss.TakeDamage(_damage);
        }

        if (_remainingPierces > 0)
        {
            _remainingPierces--;
            return;
        }

        QueueFree();
    }
}
