using Godot;

namespace Shapes;

public partial class HUD : CanvasLayer
{
    private const string PauseMenuPath = "PauseOverlay/CenterContainer/PanelContainer/MarginContainer/PauseMenu";
    private const string SettingsPanelPath = "PauseOverlay/CenterContainer/PanelContainer/MarginContainer/SettingsPanel";
    private const string ControlSetupPath = "ControlSetupOverlay/CenterContainer/PanelContainer/MarginContainer/VBox";
    private const string AbilitySelectCardsPath = "AbilitySelectOverlay/CenterContainer/PanelContainer/MarginContainer/VBox/CardsRow";

    private const string StatsPanelPath = "TopBar/HBox/StatsPanel";
    private const string StatsVBoxPath = "TopBar/HBox/StatsPanel/MarginContainer/LeftVBox";

    private Label _scoreLabel = null!;
    private Label _healthLabel = null!;
    private Label _attackLabel = null!;
    private Label _fireRateLabel = null!;
    private Label _moveSpeedLabel = null!;
    private Label _timerLabel = null!;
    private Label _levelLabel = null!;
    private Label _expLabel = null!;
    private ProgressBar _expBar = null!;
    private Button _pauseButton = null!;
    private Button _devLevelUpButton = null!;
    private ColorRect _pauseOverlay = null!;
    private ColorRect _controlSetupOverlay = null!;
    private VBoxContainer _pauseMenu = null!;
    private VBoxContainer _settingsPanel = null!;
    private Button _joystickButton = null!;
    private Button _screenDragButton = null!;
    private Button _darkThemeButton = null!;
    private Button _lightThemeButton = null!;
    private Label _gameOverLabel = null!;
    private ColorRect _gameOverOverlay = null!;
    private ColorRect _abilitySelectOverlay = null!;
    private readonly AbilityChoice?[] _abilityChoices = new AbilityChoice?[3];
    private Player? _abilitySelectPlayer;

    private Player? _player;

    public override void _Ready()
    {
        AddToGroup("hud");
        _scoreLabel = GetNode<Label>($"{StatsVBoxPath}/ScoreLabel");
        _healthLabel = GetNode<Label>($"{StatsVBoxPath}/HealthLabel");
        _attackLabel = GetNode<Label>($"{StatsVBoxPath}/AttackLabel");
        _fireRateLabel = GetNode<Label>($"{StatsVBoxPath}/FireRateLabel");
        _moveSpeedLabel = GetNode<Label>($"{StatsVBoxPath}/MoveSpeedLabel");
        _timerLabel = GetNode<Label>("TimerLabel");
        _pauseButton = GetNode<Button>("TopBar/HBox/PauseButton");
        _devLevelUpButton = GetNode<Button>("DevLevelUpButton");
        _levelLabel = GetNode<Label>("BottomBar/VBox/ExpRow/LevelLabel");
        _expLabel = GetNode<Label>("BottomBar/VBox/ExpRow/ExpLabel");
        _expBar = GetNode<ProgressBar>("BottomBar/VBox/ExpBar");
        _pauseOverlay = GetNode<ColorRect>("PauseOverlay");
        _controlSetupOverlay = GetNode<ColorRect>("ControlSetupOverlay");
        _pauseMenu = GetNode<VBoxContainer>(PauseMenuPath);
        _settingsPanel = GetNode<VBoxContainer>(SettingsPanelPath);
        _joystickButton = GetNode<Button>($"{SettingsPanelPath}/JoystickButton");
        _screenDragButton = GetNode<Button>($"{SettingsPanelPath}/ScreenDragButton");
        _darkThemeButton = GetNode<Button>($"{SettingsPanelPath}/DarkThemeButton");
        _lightThemeButton = GetNode<Button>($"{SettingsPanelPath}/LightThemeButton");
        _gameOverLabel = GetNode<Label>("GameOverLabel");
        _gameOverOverlay = GetNode<ColorRect>("GameOverOverlay");
        _abilitySelectOverlay = GetNode<ColorRect>("AbilitySelectOverlay");

        ApplySafeAreaMargins();
        ApplyStatsPanelStyle();

        _pauseButton.Pressed += OnPauseButtonPressed;
        _devLevelUpButton.Pressed += OnDevLevelUpPressed;
        _devLevelUpButton.Visible = GameConstants.DevLevelUpButtonEnabled;
        GetNode<Button>($"{PauseMenuPath}/ResumeButton").Pressed += OnResumePressed;
        GetNode<Button>($"{PauseMenuPath}/RestartButton").Pressed += OnRestartPressed;
        GetNode<Button>($"{PauseMenuPath}/SettingsButton").Pressed += OnSettingsPressed;
        GetNode<Button>($"{SettingsPanelPath}/SettingsBackButton").Pressed += OnSettingsBackPressed;
        GetNode<Button>($"{ControlSetupPath}/ButtonRow/SetupJoystickButton").Pressed += OnSetupJoystickPressed;
        GetNode<Button>($"{ControlSetupPath}/ButtonRow/SetupScreenDragButton").Pressed += OnSetupScreenDragPressed;
        _joystickButton.Pressed += OnJoystickPressed;
        _screenDragButton.Pressed += OnScreenDragPressed;
        _darkThemeButton.Pressed += OnDarkThemePressed;
        _lightThemeButton.Pressed += OnLightThemePressed;
        GetNode<Button>($"{AbilitySelectCardsPath}/Card0/VBox/SelectButton").Pressed += () => OnAbilitySelected(0);
        GetNode<Button>($"{AbilitySelectCardsPath}/Card1/VBox/SelectButton").Pressed += () => OnAbilitySelected(1);
        GetNode<Button>($"{AbilitySelectCardsPath}/Card2/VBox/SelectButton").Pressed += () => OnAbilitySelected(2);

        GameManager.Instance.ScoreChanged += OnScoreChanged;
        GameManager.Instance.GameOver += OnGameOver;
        GameManager.Instance.PauseChanged += OnPauseChanged;
        InputSettings.Instance.ControlModeChanged += OnControlModeChanged;
        ThemeSettings.Instance.ThemeChanged += OnThemeChanged;

        ApplyTheme();

        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (_player != null)
        {
            _player.HealthChanged += OnHealthChanged;
            _player.ExpChanged += OnExpChanged;
            _player.StatsChanged += OnStatsChanged;
            OnHealthChanged(_player.CurrentHealth, _player.MaxHealth);
            OnExpChanged(_player.CurrentExp, _player.ExpToNextLevel, _player.Level);
            OnStatsChanged();
        }

        OnScoreChanged(GameManager.Instance.Score);
        UpdateControlModeButtons(InputSettings.Instance.ControlMode);
        UpdateThemeButtons(ThemeSettings.Instance.CurrentTheme);
        _gameOverLabel.Visible = false;
        _gameOverOverlay.Visible = false;
        _pauseOverlay.Visible = false;
        _abilitySelectOverlay.Visible = false;

        if (!InputSettings.Instance.IsControlConfigured)
        {
            ShowControlSetup();
        }
        else
        {
            OnPauseChanged(GameManager.Instance.IsPaused);
        }

        _gameOverLabel.Text = MobileInput.IsMobilePlatform()
            ? "Game Over\nTap to restart"
            : "Game Over\nTap or press R to restart";

        UpdateTimerDisplay();
    }

    public override void _Process(double delta)
    {
        UpdateTimerDisplay();
    }

    public override void _ExitTree()
    {
        _pauseButton.Pressed -= OnPauseButtonPressed;
        _devLevelUpButton.Pressed -= OnDevLevelUpPressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged -= OnScoreChanged;
            GameManager.Instance.GameOver -= OnGameOver;
            GameManager.Instance.PauseChanged -= OnPauseChanged;
        }

        if (InputSettings.Instance != null)
        {
            InputSettings.Instance.ControlModeChanged -= OnControlModeChanged;
        }

        if (ThemeSettings.Instance != null)
        {
            ThemeSettings.Instance.ThemeChanged -= OnThemeChanged;
        }

        if (_player != null && GodotObject.IsInstanceValid(_player))
        {
            _player.HealthChanged -= OnHealthChanged;
            _player.ExpChanged -= OnExpChanged;
            _player.StatsChanged -= OnStatsChanged;
        }
    }

    private void ShowControlSetup()
    {
        _controlSetupOverlay.Visible = true;
        _pauseButton.Disabled = true;
        _pauseOverlay.Visible = false;
        GameManager.Instance.SetPaused(true);
    }

    private void CompleteControlSetup(InputControlMode mode)
    {
        InputSettings.Instance.ConfirmControlMode(mode);
        _controlSetupOverlay.Visible = false;
        _pauseButton.Disabled = false;
        UpdateControlModeButtons(mode);
        GameManager.Instance.ForceResume();
    }

    private void OnSetupJoystickPressed()
    {
        CompleteControlSetup(InputControlMode.Joystick);
    }

    private void OnSetupScreenDragPressed()
    {
        CompleteControlSetup(InputControlMode.ScreenDrag);
    }

    private void OnPauseButtonPressed()
    {
        GameManager.Instance.TogglePause();
    }

    private void OnDevLevelUpPressed()
    {
        _player ??= GetTree().GetFirstNodeInGroup("player") as Player;
        _player?.DevLevelUp();
    }

    private void OnResumePressed()
    {
        GameManager.Instance.SetPaused(false);
    }

    private void OnRestartPressed()
    {
        GameManager.Instance.ForceResume();
        GetTree().ReloadCurrentScene();
    }

    private void OnSettingsPressed()
    {
        _pauseMenu.Visible = false;
        _settingsPanel.Visible = true;
    }

    private void OnSettingsBackPressed()
    {
        _settingsPanel.Visible = false;
        _pauseMenu.Visible = true;
    }

    private void OnJoystickPressed()
    {
        InputSettings.Instance.SetControlMode(InputControlMode.Joystick);
    }

    private void OnScreenDragPressed()
    {
        InputSettings.Instance.SetControlMode(InputControlMode.ScreenDrag);
    }

    private void OnDarkThemePressed()
    {
        ThemeSettings.Instance.SetTheme(GameTheme.Dark);
    }

    private void OnLightThemePressed()
    {
        ThemeSettings.Instance.SetTheme(GameTheme.Light);
    }

    private void OnControlModeChanged(InputControlMode mode)
    {
        UpdateControlModeButtons(mode);
    }

    private void OnThemeChanged(GameTheme theme)
    {
        ApplyTheme();
        UpdateThemeButtons(theme);
    }

    private void UpdateControlModeButtons(InputControlMode mode)
    {
        _joystickButton.ButtonPressed = mode == InputControlMode.Joystick;
        _screenDragButton.ButtonPressed = mode == InputControlMode.ScreenDrag;
    }

    private void UpdateThemeButtons(GameTheme theme)
    {
        _darkThemeButton.ButtonPressed = theme == GameTheme.Dark;
        _lightThemeButton.ButtonPressed = theme == GameTheme.Light;
    }

    private void ApplyTheme()
    {
        var palette = ThemeSettings.Instance.Palette;
        _pauseOverlay.Color = palette.PauseOverlay;
        _controlSetupOverlay.Color = palette.SetupOverlay;
        _abilitySelectOverlay.Color = palette.SetupOverlay;
        _gameOverOverlay.Color = palette.GameOverOverlay;
        ApplyHudTextColor(_scoreLabel, palette.HudText);
        ApplyHudTextColor(_healthLabel, palette.HudText);
        ApplyHudTextColor(_attackLabel, palette.HudText);
        ApplyHudTextColor(_fireRateLabel, palette.HudText);
        ApplyHudTextColor(_moveSpeedLabel, palette.HudText);
        ApplyHudTextColor(_timerLabel, palette.HudText);
        ApplyHudTextColor(_levelLabel, palette.HudText);
        ApplyHudTextColor(_expLabel, palette.HudText);
        ApplyHudTextColor(_gameOverLabel, palette.HudText);
        ApplyLabelsInContainer(_pauseMenu, palette.HudText);
        ApplyLabelsInContainer(_settingsPanel, palette.HudText);
        ApplyLabelsInContainer(GetNode<VBoxContainer>($"{ControlSetupPath}"), palette.HudText);
        ApplyLabelsInContainer(GetNode<VBoxContainer>("AbilitySelectOverlay/CenterContainer/PanelContainer/MarginContainer/VBox"), palette.HudText);

        var expBackground = new StyleBoxFlat { BgColor = palette.ExpBarBackground };
        var expFill = new StyleBoxFlat { BgColor = palette.ExpBarFill };
        _expBar.AddThemeStyleboxOverride("background", expBackground);
        _expBar.AddThemeStyleboxOverride("fill", expFill);
    }

    private void ApplyStatsPanelStyle()
    {
        var panel = GetNode<PanelContainer>(StatsPanelPath);
        var style = new StyleBoxFlat
        {
            BgColor = GameConstants.HudStatsPanelBackground,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusBottomLeft = 8,
            ContentMarginLeft = 12,
            ContentMarginTop = 10,
            ContentMarginRight = 12,
            ContentMarginBottom = 10,
        };
        panel.AddThemeStyleboxOverride("panel", style);
    }

    private static void ApplyHudTextColor(Label label, Color color)
    {
        label.AddThemeColorOverride("font_color", color);
    }

    private static void ApplyLabelsInContainer(Node container, Color color)
    {
        foreach (var child in container.GetChildren())
        {
            if (child is Label label)
            {
                ApplyHudTextColor(label, color);
            }
        }
    }

    private void ApplySafeAreaMargins()
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var windowSize = DisplayServer.WindowGetSize();
        var safeArea = DisplayServer.GetDisplaySafeArea();

        var safeMarginSide = 0f;
        var safeMarginTop = 0f;
        var safeMarginBottom = 0f;

        if (windowSize.X > 0 && windowSize.Y > 0)
        {
            safeMarginSide = safeArea.Position.X / (float)windowSize.X * viewportSize.X;
            safeMarginTop = safeArea.Position.Y / (float)windowSize.Y * viewportSize.Y;
            safeMarginBottom = (windowSize.Y - safeArea.End.Y) / (float)windowSize.Y * viewportSize.Y;
        }

        const float baseMargin = 24f;
        var topBar = GetNode<MarginContainer>("TopBar");
        topBar.AddThemeConstantOverride("margin_left", (int)(baseMargin + safeMarginSide));
        topBar.AddThemeConstantOverride("margin_top", (int)(baseMargin + safeMarginTop));
        topBar.AddThemeConstantOverride("margin_right", (int)(baseMargin + safeMarginSide));

        var bottomBar = GetNode<MarginContainer>("BottomBar");
        bottomBar.AddThemeConstantOverride("margin_left", (int)(baseMargin + safeMarginSide));
        bottomBar.AddThemeConstantOverride("margin_right", (int)(baseMargin + safeMarginSide));
        bottomBar.AddThemeConstantOverride("margin_bottom", (int)(baseMargin + safeMarginBottom));

        if (GameConstants.DevLevelUpButtonEnabled)
        {
            _devLevelUpButton.OffsetLeft = -(112f + safeMarginSide);
            _devLevelUpButton.OffsetRight = -(baseMargin + safeMarginSide);
            _devLevelUpButton.OffsetTop = -(168f + safeMarginBottom);
            _devLevelUpButton.OffsetBottom = -(96f + safeMarginBottom);
        }

        _timerLabel.OffsetTop = baseMargin + safeMarginTop;
        _timerLabel.OffsetBottom = baseMargin + safeMarginTop + 36f;
    }

    private void UpdateTimerDisplay()
    {
        var totalSeconds = Mathf.FloorToInt(GameManager.Instance.GameTime);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        _timerLabel.Text = $"{minutes}:{seconds:D2}";
    }

    private void OnScoreChanged(int score)
    {
        _scoreLabel.Text = $"Score: {score}";
    }

    private void OnHealthChanged(int current, int max)
    {
        _healthLabel.Text = $"HP {current}/{max}";
    }

    private void OnStatsChanged()
    {
        if (_player == null)
        {
            return;
        }

        _attackLabel.Text = $"공격력 {_player.AttackDamage}";
        _fireRateLabel.Text = $"공격속도 {_player.FireRatePerSecond:0.0}/s";
        _moveSpeedLabel.Text = $"이동속도 {Mathf.RoundToInt(_player.MoveSpeed)}";
    }

    private void OnExpChanged(int currentExp, int expToNextLevel, int level)
    {
        _levelLabel.Text = $"Lv.{level}";
        _expLabel.Text = $"{currentExp}/{expToNextLevel}";
        _expBar.MaxValue = expToNextLevel;
        _expBar.Value = currentExp;
    }

    private void OnPauseChanged(bool isPaused)
    {
        if (!InputSettings.Instance.IsControlConfigured)
        {
            _pauseOverlay.Visible = false;
            return;
        }

        if (GameManager.Instance.IsAbilitySelectActive)
        {
            _pauseOverlay.Visible = false;
            return;
        }

        _pauseOverlay.Visible = isPaused;

        if (isPaused)
        {
            _settingsPanel.Visible = false;
            _pauseMenu.Visible = true;
        }
    }

    private void OnGameOver()
    {
        _pauseButton.Disabled = true;
        _devLevelUpButton.Disabled = true;
        _controlSetupOverlay.Visible = false;
        _abilitySelectOverlay.Visible = false;
        _pauseOverlay.Visible = false;
        _gameOverLabel.Visible = true;
        _gameOverOverlay.Visible = true;
    }

    public void ShowAbilitySelection(Player player, AbilityChoice[] choices, string? title = null, string? description = null)
    {
        _abilitySelectPlayer = player;
        _abilityChoices[0] = choices.Length > 0 ? choices[0] : null;
        _abilityChoices[1] = choices.Length > 1 ? choices[1] : null;
        _abilityChoices[2] = choices.Length > 2 ? choices[2] : null;

        GetNode<Label>("AbilitySelectOverlay/CenterContainer/PanelContainer/MarginContainer/VBox/TitleLabel").Text =
            title ?? "능력 선택";
        GetNode<Label>("AbilitySelectOverlay/CenterContainer/PanelContainer/MarginContainer/VBox/DescLabel").Text =
            description ?? (choices.Length > 1 ? "능력 하나를 선택하세요." : "보상 능력을 선택하세요.");

        for (var i = 0; i < 3; i++)
        {
            UpdateAbilityCard(i, _abilityChoices[i]);
        }

        _pauseButton.Disabled = true;
        _abilitySelectOverlay.Visible = true;
        GameManager.Instance.SetAbilitySelectActive(true);
        GameManager.Instance.SetPaused(true);
    }

    private void UpdateAbilityCard(int index, AbilityChoice? choice)
    {
        var cardRoot = $"{AbilitySelectCardsPath}/Card{index}/VBox";
        var card = GetNode<Control>($"{AbilitySelectCardsPath}/Card{index}");
        if (choice == null)
        {
            card.Visible = false;
            return;
        }

        card.Visible = true;
        var icon = GetNode<AbilityIcon>($"{cardRoot}/Icon");
        icon.SetKind(choice.Kind);

        var rarityLabel = GetNode<Label>($"{cardRoot}/RarityLabel");
        rarityLabel.Visible = choice.ShowsRarity;
        rarityLabel.Text = choice.RarityLabel;
        rarityLabel.AddThemeColorOverride("font_color", choice.RarityColor);

        GetNode<Label>($"{cardRoot}/TitleLabel").Text = choice.Title;
        GetNode<Label>($"{cardRoot}/DescLabel").Text = choice.Description;
    }

    private void OnAbilitySelected(int index)
    {
        if (_abilitySelectPlayer == null || index < 0 || index >= _abilityChoices.Length)
        {
            return;
        }

        var choice = _abilityChoices[index];
        if (choice == null)
        {
            return;
        }

        _abilitySelectPlayer.ApplyAbilityChoice(choice);
        HideAbilitySelection();
    }

    private void HideAbilitySelection()
    {
        var player = _abilitySelectPlayer;
        _abilitySelectOverlay.Visible = false;
        _abilitySelectPlayer = null;
        _abilityChoices[0] = null;
        _abilityChoices[1] = null;
        _abilityChoices[2] = null;
        _pauseButton.Disabled = false;
        GameManager.Instance.SetAbilitySelectActive(false);
        GameManager.Instance.ForceResume();
        player?.OnAbilitySelectionClosed();
    }
}
