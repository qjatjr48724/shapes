using Godot;

namespace Shapes;

public partial class Companion : Node2D
{
    public AbilityKind Kind { get; private set; }

    private Player? _player;
    private int _flankIndex;
    private float _fireCooldown;

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

    public void Configure(Player player, AbilityKind kind, int flankIndex, int totalCount)
    {
        _player = player;
        Kind = kind;
        _flankIndex = flankIndex;
        Position = GameConstants.GetCompanionFlankOffset(flankIndex, totalCount);
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null
            || GameManager.Instance.IsGameOver
            || GameManager.Instance.IsPaused)
        {
            return;
        }

        _fireCooldown -= (float)delta;
        if (_fireCooldown > 0f)
        {
            return;
        }

        _player.FireFromCompanion(this);
        _fireCooldown = _player.GetCompanionFireInterval(Kind);
    }

    public int GetDiagonalSideIndex() => _flankIndex;

    public override void _Draw()
    {
        var radius = GameConstants.CompanionRadius;
        var fill = Kind switch
        {
            AbilityKind.StraightProjectile => new Color(0.3f, 0.67f, 0.97f),
            AbilityKind.DiagonalProjectile => new Color(0.35f, 0.72f, 0.55f),
            AbilityKind.HomingProjectile => new Color(0.95f, 0.45f, 0.95f),
            _ => ThemeSettings.Instance.Palette.PlayerFill,
        };
        var outline = ThemeSettings.Instance.Palette.ShapeOutline;
        DrawCircle(Vector2.Zero, radius, fill);
        DrawArc(Vector2.Zero, radius, 0f, Mathf.Tau, 24, outline, 1.5f);
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }
}
