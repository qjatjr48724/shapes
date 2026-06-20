using Godot;

namespace Shapes;

public partial class AbilityIcon : Control
{
    public AbilityKind Kind { get; set; } = AbilityKind.StraightProjectile;

    public override void _Draw()
    {
        var center = Size / 2f;
        var radius = Mathf.Min(Size.X, Size.Y) * 0.38f;

        switch (Kind)
        {
            case AbilityKind.StraightProjectile:
                DrawCircle(center + new Vector2(0, -radius * 0.35f), radius * 0.22f, new Color(0.3f, 0.67f, 0.97f));
                DrawLine(center + new Vector2(-radius * 0.55f, radius * 0.2f), center + new Vector2(-radius * 0.55f, -radius * 0.55f), Colors.White, 3f);
                DrawLine(center + new Vector2(radius * 0.55f, radius * 0.2f), center + new Vector2(radius * 0.55f, -radius * 0.55f), Colors.White, 3f);
                break;

            case AbilityKind.DiagonalProjectile:
                DrawCircle(center, radius * 0.22f, new Color(0.3f, 0.67f, 0.97f));
                DrawLine(center, center + new Vector2(-radius * 0.55f, -radius * 0.55f), new Color(1f, 0.85f, 0.35f), 3f);
                DrawLine(center, center + new Vector2(radius * 0.55f, -radius * 0.55f), new Color(1f, 0.85f, 0.35f), 3f);
                break;

            case AbilityKind.HomingProjectile:
                DrawCircle(center, radius * 0.28f, new Color(0.95f, 0.45f, 0.95f));
                DrawArc(center, radius * 0.45f, -0.4f, 0.4f, 12, Colors.White, 2f);
                DrawCircle(center + new Vector2(radius * 0.35f, -radius * 0.15f), radius * 0.12f, new Color(1f, 0.85f, 0.35f));
                break;

            default:
                DrawCircle(center, radius * 0.35f, new Color(0.95f, 0.35f, 0.35f));
                DrawLine(center + new Vector2(-radius * 0.35f, 0), center + new Vector2(radius * 0.35f, 0), Colors.White, 3f);
                DrawLine(center + new Vector2(0, -radius * 0.35f), center + new Vector2(0, radius * 0.35f), Colors.White, 3f);
                break;
        }
    }

    public void SetKind(AbilityKind kind)
    {
        Kind = kind;
        QueueRedraw();
    }
}
