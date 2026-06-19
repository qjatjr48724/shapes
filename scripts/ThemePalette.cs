using Godot;

namespace Shapes;

public sealed class ThemePalette
{
    public Color Background { get; init; }
    public Color HudText { get; init; }
    public Color PauseOverlay { get; init; }
    public Color SetupOverlay { get; init; }
    public Color GameOverOverlay { get; init; }
    public Color PlayerFill { get; init; }
    public Color PlayerOutline { get; init; }
    public Color EnemySquareFill { get; init; }
    public Color ShapeOutline { get; init; }
    public Color PentagonFill { get; init; }
    public Color BossMidFill { get; init; }
    public Color BossFinalFill { get; init; }
    public Color BossCore { get; init; }
    public Color PlayerBullet { get; init; }
    public Color BossBulletFill { get; init; }
    public Color BossBulletOutline { get; init; }
    public Color HealthBarBackground { get; init; }
    public Color HealthBarBorder { get; init; }
    public Color HealthBarFill { get; init; }
    public Color TouchRingFill { get; init; }
    public Color TouchRingLine { get; init; }
    public Color TouchKnobFill { get; init; }
    public Color TouchKnobLine { get; init; }
    public Color ExpBarBackground { get; init; }
    public Color ExpBarFill { get; init; }

    public static ThemePalette For(GameTheme theme)
    {
        return theme == GameTheme.Light ? Light : Dark;
    }

    public static ThemePalette Dark { get; } = new()
    {
        Background = new Color(0.1f, 0.1f, 0.16f),
        HudText = Colors.White,
        PauseOverlay = new Color(0f, 0f, 0f, 0.45f),
        SetupOverlay = new Color(0.08f, 0.08f, 0.12f, 0.92f),
        GameOverOverlay = new Color(0f, 0f, 0f, 0.45f),
        PlayerFill = new Color(0.3f, 0.67f, 0.97f),
        PlayerOutline = Colors.White,
        EnemySquareFill = new Color(0.95f, 0.35f, 0.35f),
        ShapeOutline = Colors.White,
        PentagonFill = new Color(0.95f, 0.62f, 0.2f),
        BossMidFill = new Color(0.75f, 0.2f, 0.35f),
        BossFinalFill = new Color(0.55f, 0.15f, 0.75f),
        BossCore = new Color(1f, 0.85f, 0.3f),
        PlayerBullet = Colors.White,
        BossBulletFill = new Color(0.85f, 0.2f, 0.25f),
        BossBulletOutline = Colors.White,
        HealthBarBackground = new Color(0.15f, 0.15f, 0.15f, 0.85f),
        HealthBarBorder = new Color(1f, 1f, 1f, 0.35f),
        HealthBarFill = new Color(0.95f, 0.25f, 0.25f),
        TouchRingFill = new Color(1f, 1f, 1f, 0.1f),
        TouchRingLine = new Color(1f, 1f, 1f, 0.5f),
        TouchKnobFill = new Color(1f, 1f, 1f, 0.35f),
        TouchKnobLine = new Color(1f, 1f, 1f, 0.9f),
        ExpBarBackground = new Color(0.15f, 0.15f, 0.15f, 0.85f),
        ExpBarFill = new Color(0.3f, 0.67f, 0.97f),
    };

    public static ThemePalette Light { get; } = new()
    {
        Background = new Color(0.96f, 0.92f, 0.85f),
        HudText = new Color(0.16f, 0.16f, 0.2f),
        PauseOverlay = new Color(0.96f, 0.92f, 0.85f, 0.55f),
        SetupOverlay = new Color(0.98f, 0.96f, 0.92f, 0.96f),
        GameOverOverlay = new Color(0.96f, 0.92f, 0.85f, 0.55f),
        PlayerFill = new Color(0.2f, 0.55f, 0.9f),
        PlayerOutline = new Color(0.16f, 0.16f, 0.2f),
        EnemySquareFill = new Color(0.9f, 0.32f, 0.32f),
        ShapeOutline = new Color(0.16f, 0.16f, 0.2f),
        PentagonFill = new Color(0.92f, 0.58f, 0.15f),
        BossMidFill = new Color(0.72f, 0.18f, 0.34f),
        BossFinalFill = new Color(0.52f, 0.12f, 0.68f),
        BossCore = new Color(0.95f, 0.72f, 0.15f),
        PlayerBullet = new Color(0.16f, 0.16f, 0.2f),
        BossBulletFill = new Color(0.78f, 0.18f, 0.24f),
        BossBulletOutline = new Color(0.16f, 0.16f, 0.2f),
        HealthBarBackground = new Color(0.16f, 0.16f, 0.2f, 0.12f),
        HealthBarBorder = new Color(0.16f, 0.16f, 0.2f, 0.35f),
        HealthBarFill = new Color(0.85f, 0.22f, 0.22f),
        TouchRingFill = new Color(0.16f, 0.16f, 0.2f, 0.08f),
        TouchRingLine = new Color(0.16f, 0.16f, 0.2f, 0.45f),
        TouchKnobFill = new Color(0.16f, 0.16f, 0.2f, 0.2f),
        TouchKnobLine = new Color(0.16f, 0.16f, 0.2f, 0.75f),
        ExpBarBackground = new Color(0.16f, 0.16f, 0.2f, 0.12f),
        ExpBarFill = new Color(0.2f, 0.55f, 0.9f),
    };
}
