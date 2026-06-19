using Godot;

namespace Shapes;

public partial class InputSettings : Node
{
    private const string SettingsPath = "user://settings.cfg";
    private const string Section = "input";
    private const string KeyConfigured = "control_configured";
    private const string KeyControlMode = "control_mode";

    public static InputSettings Instance { get; private set; } = null!;

    public InputControlMode ControlMode { get; private set; } = InputControlMode.ScreenDrag;
    public bool IsControlConfigured { get; private set; }

    [Signal]
    public delegate void ControlModeChangedEventHandler(InputControlMode mode);

    [Signal]
    public delegate void ControlSetupCompletedEventHandler();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        LoadSettings();
    }

    public void ConfirmControlMode(InputControlMode mode)
    {
        ControlMode = mode;
        IsControlConfigured = true;
        SaveSettings();
        EmitSignal(SignalName.ControlModeChanged, (int)mode);
        EmitSignal(SignalName.ControlSetupCompleted);
    }

    public void SetControlMode(InputControlMode mode)
    {
        if (ControlMode == mode && IsControlConfigured)
        {
            return;
        }

        ControlMode = mode;
        if (IsControlConfigured)
        {
            SaveSettings();
        }

        EmitSignal(SignalName.ControlModeChanged, (int)mode);
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        var error = config.Load(SettingsPath);
        if (error != Error.Ok)
        {
            IsControlConfigured = false;
            return;
        }

        IsControlConfigured = config.GetValue(Section, KeyConfigured, false).AsBool();
        if (!IsControlConfigured)
        {
            return;
        }

        var modeValue = config.GetValue(Section, KeyControlMode, (int)InputControlMode.ScreenDrag).AsInt32();
        ControlMode = (InputControlMode)modeValue;
    }

    private void SaveSettings()
    {
        var config = new ConfigFile();
        config.Load(SettingsPath);
        config.SetValue(Section, KeyConfigured, true);
        config.SetValue(Section, KeyControlMode, (int)ControlMode);
        config.Save(SettingsPath);
    }
}
