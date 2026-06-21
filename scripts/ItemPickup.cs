using Godot;

namespace Shapes;

public partial class ItemPickup : Area2D
{
    private ItemKind _kind = ItemKind.DamageUp50;
    private const float IconRadius = 14f;

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

    public void Configure(ItemKind kind)
    {
        _kind = kind;
        QueueRedraw();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            return;
        }

        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null)
        {
            return;
        }

        var offset = player.GlobalPosition - GlobalPosition;
        var distance = offset.Length();
        if (distance > GameConstants.ItemMagnetRadius || distance < 1f)
        {
            return;
        }

        var pullRatio = 1f - distance / GameConstants.ItemMagnetRadius;
        GlobalPosition += offset.Normalized() * GameConstants.ItemMagnetPullSpeed * pullRatio * (float)delta;
    }

    public override void _Draw()
    {
        var outline = ThemeSettings.Instance.Palette.ShapeOutline;
        var fill = GetIconFill();
        DrawCircle(Vector2.Zero, IconRadius, fill);
        DrawArc(Vector2.Zero, IconRadius, 0f, Mathf.Tau, 24, outline, 2f);
        DrawIconSymbol(outline);
    }

    private Color GetIconFill()
    {
        return _kind switch
        {
            ItemKind.DamageUp50 => new Color(0.95f, 0.3f, 0.3f),
            ItemKind.DamageUp30 => new Color(0.95f, 0.5f, 0.25f),
            ItemKind.FireRateUp300 => new Color(1f, 0.85f, 0.2f),
            ItemKind.FireRateUp200 => new Color(0.95f, 0.75f, 0.15f),
            ItemKind.MoveSpeedUp200 => new Color(0.3f, 0.85f, 0.45f),
            ItemKind.BulletSpeedUp300 => new Color(0.45f, 0.8f, 1f),
            ItemKind.HealthRecover => new Color(0.95f, 0.4f, 0.55f),
            _ => Colors.White,
        };
    }

    private void DrawIconSymbol(Color outline)
    {
        switch (_kind)
        {
            case ItemKind.DamageUp50:
            case ItemKind.DamageUp30:
                DrawLine(new Vector2(0, 6), new Vector2(0, -7), outline, 2.5f);
                DrawLine(new Vector2(-5, -2), new Vector2(0, -7), outline, 2.5f);
                DrawLine(new Vector2(5, -2), new Vector2(0, -7), outline, 2.5f);
                break;

            case ItemKind.FireRateUp300:
            case ItemKind.FireRateUp200:
                DrawLine(new Vector2(-6, 4), new Vector2(0, -6), outline, 2.5f);
                DrawLine(new Vector2(0, -6), new Vector2(6, 4), outline, 2.5f);
                DrawLine(new Vector2(0, -6), new Vector2(0, 6), outline, 2f);
                break;

            case ItemKind.MoveSpeedUp200:
                DrawLine(new Vector2(-7, 0), new Vector2(5, 0), outline, 2.5f);
                DrawLine(new Vector2(2, -5), new Vector2(7, 0), outline, 2.5f);
                DrawLine(new Vector2(2, 5), new Vector2(7, 0), outline, 2.5f);
                break;

            case ItemKind.BulletSpeedUp300:
                DrawLine(new Vector2(0, 6), new Vector2(0, -7), outline, 2.5f);
                DrawLine(new Vector2(-4, -1), new Vector2(0, -7), outline, 2f);
                DrawLine(new Vector2(4, -1), new Vector2(0, -7), outline, 2f);
                DrawLine(new Vector2(-7, 2), new Vector2(-3, -2), outline, 2f);
                DrawLine(new Vector2(7, 2), new Vector2(3, -2), outline, 2f);
                break;

            case ItemKind.HealthRecover:
                DrawLine(new Vector2(0, -6), new Vector2(0, 6), outline, 2.5f);
                DrawLine(new Vector2(-6, 0), new Vector2(6, 0), outline, 2.5f);
                break;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
        {
            return;
        }

        if (body is Player player)
        {
            player.ApplyItemBuff(_kind);
            QueueFree();
        }
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }
}
