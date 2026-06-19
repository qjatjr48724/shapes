using Godot;

namespace Shapes;

public partial class ThemeSettings : Node
{
    private const string SettingsPath = "user://settings.cfg";
    private const string Section = "appearance";
    private const string KeyTheme = "theme";

    public static ThemeSettings Instance { get; private set; } = null!;

    public GameTheme CurrentTheme { get; private set; } = GameTheme.Light;
    public ThemePalette Palette => ThemePalette.For(CurrentTheme);

    [Signal]
    public delegate void ThemeChangedEventHandler(GameTheme theme);

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        LoadSettings();
    }

    public void SetTheme(GameTheme theme)
    {
        if (CurrentTheme == theme)
        {
            return;
        }

        CurrentTheme = theme;
        SaveSettings();
        EmitSignal(SignalName.ThemeChanged, (int)theme);
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) != Error.Ok)
        {
            CurrentTheme = GameTheme.Light;
            return;
        }

        var themeValue = config.GetValue(Section, KeyTheme, (int)GameTheme.Light).AsInt32();
        CurrentTheme = themeValue == (int)GameTheme.Dark ? GameTheme.Dark : GameTheme.Light;
    }

    private void SaveSettings()
    {
        var config = new ConfigFile();
        config.Load(SettingsPath);
        config.SetValue(Section, KeyTheme, (int)CurrentTheme);
        config.Save(SettingsPath);
    }
}
