using Godot;

namespace Shapes;

public partial class Boss : CharacterBody2D
{
    private enum BossPhase
    {
        Descending,
        Patrolling,
    }

    private BossKind _kind = BossKind.MidBoss1;
    private BossPhase _phase = BossPhase.Descending;
    private float _health;
    private float _maxHealth;
    private float _bodySize;
    private float _fireInterval;
    private float _fireCooldown;
    private float _horizontalDirection = 1f;
    private HealthBar2D _healthBar = null!;

    [Export]
    public PackedScene BossBulletScene { get; set; } = null!;

    public void Configure(BossKind kind)
    {
        _kind = kind;
        _maxHealth = GameConstants.GetBossHealth(kind);
        _bodySize = GameConstants.GetBossSize(kind);
        _fireInterval = GameConstants.GetBossFireInterval(kind);
    }

    public override void _Ready()
    {
        AddToGroup("bosses");
        _health = _maxHealth;
        _healthBar = GetNode<HealthBar2D>("HealthBar");
        ApplyBodySize();
        _healthBar.SetValues(_health, _maxHealth);
        _fireCooldown = _fireInterval * 0.5f;
        GameManager.Instance.SetBossActive(true);
        ThemeSettings.Instance.ThemeChanged += OnThemeChanged;
        QueueRedraw();
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (ThemeSettings.Instance != null)
        {
            ThemeSettings.Instance.ThemeChanged -= OnThemeChanged;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            Velocity = Vector2.Zero;
            return;
        }

        var dt = (float)delta;

        switch (_phase)
        {
            case BossPhase.Descending:
                UpdateDescending(dt);
                break;
            case BossPhase.Patrolling:
                UpdatePatrolling(dt);
                UpdateFiring(dt);
                break;
        }

        MoveAndSlide();
    }

    public override void _Draw()
    {
        var palette = ThemeSettings.Instance.Palette;
        var half = _bodySize / 2f;
        var fill = _kind == BossKind.FinalBoss ? palette.BossFinalFill : palette.BossMidFill;
        var rect = new Rect2(-half, -half, _bodySize, _bodySize);
        DrawRect(rect, fill);
        DrawRect(rect, palette.ShapeOutline, false, 3f);

        if (_kind == BossKind.FinalBoss)
        {
            DrawCircle(Vector2.Zero, half * 0.35f, palette.BossCore);
        }
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

    private void UpdateDescending(float delta)
    {
        Velocity = Vector2.Down * GameConstants.BossDescendSpeed;

        if (GlobalPosition.Y >= GameConstants.BossDescendTargetY)
        {
            GlobalPosition = new Vector2(GlobalPosition.X, GameConstants.BossDescendTargetY);
            _phase = BossPhase.Patrolling;
            Velocity = Vector2.Zero;
        }
    }

    private void UpdatePatrolling(float delta)
    {
        var rect = GetViewport().GetVisibleRect();
        var margin = _bodySize / 2f + 16f;
        Velocity = new Vector2(_horizontalDirection * GameConstants.BossHorizontalSpeed, 0f);

        if (GlobalPosition.X <= margin)
        {
            GlobalPosition = new Vector2(margin, GlobalPosition.Y);
            _horizontalDirection = 1f;
        }
        else if (GlobalPosition.X >= rect.Size.X - margin)
        {
            GlobalPosition = new Vector2(rect.Size.X - margin, GlobalPosition.Y);
            _horizontalDirection = -1f;
        }
    }

    private void UpdateFiring(float delta)
    {
        _fireCooldown -= delta;
        if (_fireCooldown > 0f)
        {
            return;
        }

        FireAtPlayer();
        _fireCooldown = _fireInterval;
    }

    private void FireAtPlayer()
    {
        if (BossBulletScene == null)
        {
            return;
        }

        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null)
        {
            return;
        }

        var direction = (player.GlobalPosition - GlobalPosition).Normalized();
        var bullet = BossBulletScene.Instantiate<BossBullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition;
        bullet.Initialize(direction);
    }

    private void Die()
    {
        GameManager.Instance.AddScore(GameConstants.GetBossScore(_kind));
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        player?.AddExp(GameConstants.RollExpReward(GameConstants.GetBossExp(_kind)));
        if (player != null)
        {
            player.EnqueueAbilitySelection(
                AbilityRoller.RollThreeChoices(player.Abilities),
                "보스 보상",
                "보스 처치 보상으로 능력 하나를 선택하세요."
            );
        }
        ItemDropper.Drop(GetTree().CurrentScene, GlobalPosition, guaranteedHeal: true);
        GameManager.Instance.SetBossActive(false);
        QueueFree();
    }

    private void ApplyBodySize()
    {
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        if (collision.Shape is RectangleShape2D rectangle)
        {
            rectangle.Size = new Vector2(_bodySize, _bodySize);
        }

        _healthBar.BarWidth = _bodySize;
        _healthBar.OffsetY = -(_bodySize / 2f + 12f);
    }
}
