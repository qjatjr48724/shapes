using Godot;

namespace Shapes;

public partial class Game : Node2D
{
    public override void _Ready()
    {
        GameManager.Instance.ResetRun();
        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        player?.ResetProgression();

        if (!InputSettings.Instance.IsControlConfigured)
        {
            CallDeferred(MethodName.BeginControlSetup);
        }

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

    private void BeginControlSetup()
    {
        GameManager.Instance.SetPaused(true);
    }

    public override void _Draw()
    {
        var rect = GetViewport().GetVisibleRect();
        DrawRect(rect, ThemeSettings.Instance.Palette.Background);
    }

    public override void _Input(InputEvent @event)
    {
        if (!GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (@event is InputEventScreenTouch { Pressed: true })
        {
            RestartGame();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event.IsActionPressed("restart"))
        {
            RestartGame();
            GetViewport().SetInputAsHandled();
        }
    }

    private void RestartGame()
    {
        GetTree().ReloadCurrentScene();
    }
}
