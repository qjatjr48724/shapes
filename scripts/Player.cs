using Godot;
using System.Collections.Generic;

namespace Shapes;

public partial class Player : CharacterBody2D
{
    [Export]
    public PackedScene BulletScene { get; set; } = null!;

    [Export]
    public PackedScene HomingBulletScene { get; set; } = null!;

    [Export]
    public PackedScene CompanionScene { get; set; } = null!;

    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; }
    public int ExpToNextLevel { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public PlayerAbilities Abilities { get; } = new();
    public float CompanionFireInterval => GameConstants.GetCompanionFireInterval(_fireInterval);

    public float GetCompanionFireInterval(AbilityKind kind)
    {
        return kind == AbilityKind.HomingProjectile
            ? GameConstants.HomingFireInterval
            : CompanionFireInterval;
    }

    private float _moveSpeed;
    private float _fireInterval;
    private int _bulletDamage;
    private bool _isInvincible;
    private float _invincibleTimer;
    private float _fireCooldown;
    private float _blinkTimer;
    private bool _blinkVisible = true;
    private Area2D _hurtbox = null!;
    private Node2D _companionsRoot = null!;
    private readonly List<Companion> _companions = new();
    private readonly Queue<PendingAbilitySelection> _abilitySelectionQueue = new();

    private readonly struct PendingAbilitySelection
    {
        public AbilityChoice[] Choices { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
    }

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
        _companionsRoot = GetNode<Node2D>("Companions");
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
        Abilities.Reset();
        _abilitySelectionQueue.Clear();
        ApplyLevelStats();
        SyncCompanions();
    }

    public void ApplyAbilityChoice(AbilityChoice choice)
    {
        Abilities.Apply(choice);
        ApplyLevelStats();
        SyncCompanions();
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

        var dt = (float)delta;
        _fireCooldown -= dt;
        if (_fireCooldown <= 0f)
        {
            FireMain();
            _fireCooldown = _fireInterval;
        }

        UpdateInvincibility(dt);
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

        if (Level % GameConstants.AbilitySelectLevelInterval == 0)
        {
            EnqueueAbilitySelection(AbilityRoller.RollThreeChoices());
        }
    }

    public void EnqueueAbilitySelection(AbilityChoice[] choices, string? title = null, string? description = null)
    {
        if (choices.Length == 0)
        {
            return;
        }

        _abilitySelectionQueue.Enqueue(new PendingAbilitySelection
        {
            Choices = choices,
            Title = title,
            Description = description,
        });
        TryShowNextAbilitySelection();
    }

    public void OnAbilitySelectionClosed()
    {
        TryShowNextAbilitySelection();
    }

    private void TryShowNextAbilitySelection()
    {
        CallDeferred(MethodName.ProcessNextAbilitySelection);
    }

    private void ProcessNextAbilitySelection()
    {
        if (GameManager.Instance.IsAbilitySelectActive || _abilitySelectionQueue.Count == 0)
        {
            return;
        }

        var pending = _abilitySelectionQueue.Dequeue();
        var hud = GetTree().GetFirstNodeInGroup("hud") as HUD;
        hud?.ShowAbilitySelection(this, pending.Choices, pending.Title, pending.Description);
    }

    private void ApplyLevelStats()
    {
        var levelBonus = Level - 1;
        var baseDamage = GameConstants.BulletDamage + levelBonus * GameConstants.BulletDamagePerLevel;
        _bulletDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * Abilities.AttackMultiplier));

        var baseInterval = Mathf.Max(
            GameConstants.MinFireInterval,
            GameConstants.PlayerFireInterval - levelBonus * GameConstants.FireIntervalReductionPerLevel
        );
        _fireInterval = Mathf.Max(
            GameConstants.MinFireInterval,
            baseInterval / Abilities.FireRateMultiplier
        );

        _moveSpeed = GameConstants.PlayerMoveSpeed + levelBonus * GameConstants.MoveSpeedPerLevel;
    }

    public void FireFromCompanion(Companion companion)
    {
        var companionDamage = GetCompanionDamage();
        var homingDamage = GetHomingDamage();
        var diagonalAngle = Mathf.DegToRad(GameConstants.DiagonalShotAngleDegrees);
        var spawnPosition = companion.GlobalPosition;

        switch (companion.Kind)
        {
            case AbilityKind.StraightProjectile:
                SpawnBullet(spawnPosition, GameConstants.PlayerShootDirection, companionDamage);
                break;
            case AbilityKind.DiagonalProjectile:
                var direction = companion.GetDiagonalSideIndex() % 2 == 0
                    ? GameConstants.PlayerShootDirection.Rotated(-diagonalAngle)
                    : GameConstants.PlayerShootDirection.Rotated(diagonalAngle);
                SpawnBullet(spawnPosition, direction, companionDamage);
                break;
            case AbilityKind.HomingProjectile:
                SpawnHomingBullet(spawnPosition, homingDamage);
                break;
        }
    }

    private int GetCompanionDamage()
    {
        return Mathf.Max(1, Mathf.RoundToInt(_bulletDamage * GameConstants.CompanionDamageRatio));
    }

    private int GetHomingDamage()
    {
        return Mathf.Max(1, Mathf.RoundToInt(_bulletDamage * GameConstants.HomingDamageRatio));
    }

    private void FireMain()
    {
        SpawnBullet(GlobalPosition, GameConstants.PlayerShootDirection, _bulletDamage);
    }

    private void SyncCompanions()
    {
        var kinds = BuildCompanionKinds();
        var total = kinds.Count;

        while (_companions.Count > total)
        {
            var companion = _companions[^1];
            _companions.RemoveAt(_companions.Count - 1);
            companion.QueueFree();
        }

        while (_companions.Count < total)
        {
            if (CompanionScene == null)
            {
                return;
            }

            var companion = CompanionScene.Instantiate<Companion>();
            _companionsRoot.AddChild(companion);
            _companions.Add(companion);
        }

        for (var i = 0; i < total; i++)
        {
            _companions[i].Configure(this, kinds[i], i, total);
        }
    }

    private List<AbilityKind> BuildCompanionKinds()
    {
        var kinds = new List<AbilityKind>();
        for (var i = 0; i < Abilities.StraightCount; i++)
        {
            kinds.Add(AbilityKind.StraightProjectile);
        }

        for (var i = 0; i < Abilities.DiagonalCount; i++)
        {
            kinds.Add(AbilityKind.DiagonalProjectile);
        }

        for (var i = 0; i < Abilities.HomingCount; i++)
        {
            kinds.Add(AbilityKind.HomingProjectile);
        }

        return kinds;
    }

    private void SpawnBullet(Vector2 position, Vector2 direction, int damage)
    {
        if (BulletScene == null)
        {
            return;
        }

        var bullet = BulletScene.Instantiate<Bullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = position;
        bullet.Initialize(direction, damage);
    }

    private void SpawnHomingBullet(Vector2 position, int damage)
    {
        if (HomingBulletScene == null)
        {
            return;
        }

        var bullet = HomingBulletScene.Instantiate<HomingBullet>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = position;
        bullet.Initialize(damage);
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
            SetCompanionsVisible(_blinkVisible);
        }

        if (_invincibleTimer <= 0f)
        {
            _isInvincible = false;
            _blinkVisible = true;
            QueueRedraw();
            SetCompanionsVisible(true);
        }
    }

    private void SetCompanionsVisible(bool visible)
    {
        foreach (var companion in _companions)
        {
            companion.Visible = visible;
        }
    }

    private void EmitSignals()
    {
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        EmitSignal(SignalName.ExpChanged, CurrentExp, ExpToNextLevel, Level);
    }
}
