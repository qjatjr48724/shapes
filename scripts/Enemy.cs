using Godot;

namespace Shapes;

public partial class Enemy : CharacterBody2D
{
    private EnemyKind _kind = EnemyKind.Square;
    private float _healthMultiplier = 1f;
    private float _bodySize = GameConstants.EnemySize;
    private float _health;
    private float _maxHealth;
    private HealthBar2D _healthBar = null!;

    public void Configure(EnemyKind kind)
    {
        _kind = kind;
        _bodySize = kind == EnemyKind.Pentagon
            ? GameConstants.PentagonSize
            : GameConstants.EnemySize;

        if (kind == EnemyKind.Pentagon)
        {
            _healthMultiplier = (float)GD.RandRange(
                GameConstants.PentagonHealthMultiplierMin,
                GameConstants.PentagonHealthMultiplierMax
            );
        }
    }

    public override void _Ready()
    {
        AddToGroup("enemies");
        _maxHealth = GameManager.GetEnemyMaxHealth(GameManager.Instance.GameTime) * _healthMultiplier;
        _health = _maxHealth;
        _healthBar = GetNode<HealthBar2D>("HealthBar");
        ApplyBodySize();
        _healthBar.SetValues(_health, _maxHealth);
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

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            return;
        }

        Velocity = GameConstants.EnemyMoveDirection
            * GameConstants.GetEnemySpeed(GameManager.Instance.GameTime);
        MoveAndSlide();

        var rect = GetViewport().GetVisibleRect();
        if (GlobalPosition.Y > rect.Size.Y + _bodySize)
        {
            QueueFree();
        }
    }

    public override void _Draw()
    {
        var palette = ThemeSettings.Instance.Palette;

        if (_kind == EnemyKind.Pentagon)
        {
            DrawPentagon(_bodySize / 2f, palette.PentagonFill, palette.ShapeOutline);
            return;
        }

        var half = _bodySize / 2f;
        var rect = new Rect2(-half, -half, _bodySize, _bodySize);
        DrawRect(rect, palette.EnemySquareFill);
        DrawRect(rect, palette.ShapeOutline, false, 2f);
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        _health -= damage;
        _healthBar.SetValues(_health, _maxHealth);

        if (_health <= 0f)
        {
            Die();
        }
    }

    private void ApplyBodySize()
    {
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        if (collision.Shape is RectangleShape2D rectangle)
        {
            rectangle.Size = new Vector2(_bodySize, _bodySize);
        }

        _healthBar.BarWidth = _bodySize;
        _healthBar.OffsetY = -(_bodySize / 2f + 8f);
    }

    private void Die()
    {
        GameManager.Instance.AddScore(GameConstants.ScorePerKill);
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        var baseExp = _kind == EnemyKind.Pentagon
            ? Mathf.RoundToInt(GameConstants.ExpPerKill * GameConstants.PentagonExpMultiplier)
            : GameConstants.ExpPerKill;
        player?.AddExp(GameConstants.RollExpReward(baseExp));
        ItemDropper.Drop(GetTree().CurrentScene, GlobalPosition);
        QueueFree();
    }

    private void DrawPentagon(float radius, Color fill, Color outline)
    {
        var points = new Vector2[5];
        for (var i = 0; i < 5; i++)
        {
            var angle = -Mathf.Pi / 2f + i * Mathf.Tau / 5f;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        DrawColoredPolygon(points, fill);

        for (var i = 0; i < 5; i++)
        {
            DrawLine(points[i], points[(i + 1) % 5], outline, 2f);
        }
    }
}
