using Godot;

namespace Shapes;

public partial class Player : CharacterBody2D
{
    [Export]
    public PackedScene BulletScene { get; set; } = null!;

    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; }
    public int ExpToNextLevel { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    private float _moveSpeed;
    private float _fireInterval;
    private int _bulletDamage;
    private bool _isInvincible;
    private float _invincibleTimer;
    private float _fireCooldown;
    private float _blinkTimer;
    private bool _blinkVisible = true;
    private Area2D _hurtbox = null!;

    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

    [Signal]
    public delegate void ExpChangedEventHandler(int currentExp, int expToNextLevel, int level);

    [Signal]
    public delegate void LeveledUpEventHandler(int newLevel);

    public override void _Ready()
    {
        AddToGroup("player");
        ResetProgression();
        _hurtbox = GetNode<Area2D>("Hurtbox");
        _hurtbox.BodyEntered += OnHurtboxBodyEntered;
        _hurtbox.AreaEntered += OnHurtboxAreaEntered;
        EmitSignals();
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

    public void ResetProgression()
    {
        Level = 1;
        CurrentExp = 0;
        ExpToNextLevel = GameConstants.GetExpRequiredForLevel(Level);
        MaxHealth = GameConstants.PlayerMaxHealth;
        CurrentHealth = MaxHealth;
        ApplyLevelStats();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            Velocity = Vector2.Zero;
            return;
        }

        var input = MobileInput.GetMoveVector(this);
        Velocity = input * _moveSpeed;
        MoveAndSlide();
        ClampToViewport();

        _fireCooldown -= (float)delta;
        if (_fireCooldown <= 0f)
        {
            Fire();
            _fireCooldown = _fireInterval;
        }

        UpdateInvincibility((float)delta);
    }

    public override void _Draw()
    {
        if (!_blinkVisible)
        {
            return;
        }

        var palette = ThemeSettings.Instance.Palette;
        DrawCircle(Vector2.Zero, GameConstants.PlayerRadius, palette.PlayerFill);
        DrawArc(Vector2.Zero, GameConstants.PlayerRadius, 0f, Mathf.Tau, 32, palette.PlayerOutline, 2f);
    }

    public void AddExp(int amount)
    {
        if (GameManager.Instance.IsGameOver || amount <= 0)
        {
            return;
        }

        CurrentExp += amount;
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            LevelUp();
        }

        EmitSignal(SignalName.ExpChanged, CurrentExp, ExpToNextLevel, Level);
    }

    private void LevelUp()
    {
        Level++;
        ExpToNextLevel = GameConstants.GetExpRequiredForLevel(Level);
        ApplyLevelStats();
        EmitSignal(SignalName.LeveledUp, Level);
        EmitSignal(SignalName.ExpChanged, CurrentExp, ExpToNextLevel, Level);
    }

    private void ApplyLevelStats()
    {
        var levelBonus = Level - 1;
        _moveSpeed = GameConstants.PlayerMoveSpeed + levelBonus * GameConstants.MoveSpeedPerLevel;
        _fireInterval = Mathf.Max(
            GameConstants.MinFireInterval,
            GameConstants.PlayerFireInterval - levelBonus * GameConstants.FireIntervalReductionPerLevel
        );
        _bulletDamage = GameConstants.BulletDamage + levelBonus * GameConstants.BulletDamagePerLevel;
    }

    private void ClampToViewport()
    {
        var rect = GetViewport().GetVisibleRect();
        var margin = GameConstants.PlayerRadius + 4f;
        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, margin, rect.Size.X - margin),
            Mathf.Clamp(GlobalPosition.Y, margin, rect.Size.Y - margin)
        );
    }

    private void Fire()
    {
        if (BulletScene == null)
        {
            return;
        }

        var bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition;
        bullet.Initialize(GameConstants.PlayerShootDirection, _bulletDamage);
    }

    private void OnHurtboxBodyEntered(Node2D body)
    {
        if (body is Enemy or Boss)
        {
            TakeDamage();
        }
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        if (area is BossBullet)
        {
            TakeDamage(GameConstants.BossBulletDamage);
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (_isInvincible || GameManager.Instance.IsGameOver)
        {
            return;
        }

        CurrentHealth -= amount;
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            GameManager.Instance.TriggerGameOver();
            return;
        }

        _isInvincible = true;
        _invincibleTimer = GameConstants.InvincibilityDuration;
        _blinkTimer = 0f;
        _blinkVisible = true;
        QueueRedraw();
    }

    private void UpdateInvincibility(float delta)
    {
        if (!_isInvincible)
        {
            return;
        }

        _invincibleTimer -= delta;
        _blinkTimer += delta;

        if (_blinkTimer >= 0.1f)
        {
            _blinkTimer = 0f;
            _blinkVisible = !_blinkVisible;
            QueueRedraw();
        }

        if (_invincibleTimer <= 0f)
        {
            _isInvincible = false;
            _blinkVisible = true;
            QueueRedraw();
        }
    }

    private void EmitSignals()
    {
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        EmitSignal(SignalName.ExpChanged, CurrentExp, ExpToNextLevel, Level);
    }
}
