using Godot;

namespace Shapes;

public partial class TouchInput : Control
{
    private const int MousePointerId = -1;

    public static TouchInput Instance { get; private set; } = null!;

    public Vector2 InputVector { get; private set; }

    private int _pointerIndex = -2;
    private Vector2 _touchStart;
    private Vector2 _joystickCenter;
    private Vector2 _knobOffset;
    private Vector2 _dragDelta;
    private bool _showDragOrigin;
    private bool _showJoystick;

    private const float JoystickKnobRadius = 28f;
    private const float JoystickRingWidth = 2.5f;

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        GameManager.Instance.GameOver += OnGameOver;
        GameManager.Instance.PauseChanged += OnPauseChanged;
        InputSettings.Instance.ControlSetupCompleted += OnControlSetupCompleted;
        InputSettings.Instance.ControlModeChanged += OnControlModeChanged;
        ThemeSettings.Instance.ThemeChanged += OnThemeChanged;
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver -= OnGameOver;
            GameManager.Instance.PauseChanged -= OnPauseChanged;
        }

        if (InputSettings.Instance != null)
        {
            InputSettings.Instance.ControlSetupCompleted -= OnControlSetupCompleted;
            InputSettings.Instance.ControlModeChanged -= OnControlModeChanged;
        }

        if (ThemeSettings.Instance != null)
        {
            ThemeSettings.Instance.ThemeChanged -= OnThemeChanged;
        }
    }

    private void OnThemeChanged(GameTheme theme)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_showJoystick)
        {
            DrawJoystickVisual();
        }

        if (_showDragOrigin)
        {
            DrawDragOriginVisual();
        }
    }

    private void DrawJoystickVisual()
    {
        var palette = ThemeSettings.Instance.Palette;
        var maxRadius = GameConstants.JoystickMaxRadiusPixels;
        DrawCircle(_joystickCenter, maxRadius, palette.TouchRingFill);
        DrawArc(_joystickCenter, maxRadius, 0f, Mathf.Tau, 48, palette.TouchRingLine, JoystickRingWidth);

        var knobCenter = _joystickCenter + _knobOffset;
        DrawCircle(knobCenter, JoystickKnobRadius, palette.TouchKnobFill);
        DrawArc(knobCenter, JoystickKnobRadius, 0f, Mathf.Tau, 32, palette.TouchKnobLine, 2f);
    }

    private void DrawDragOriginVisual()
    {
        var palette = ThemeSettings.Instance.Palette;
        const float outerRadius = 14f;
        DrawCircle(_touchStart, outerRadius, palette.TouchRingFill);
        DrawArc(_touchStart, outerRadius, 0f, Mathf.Tau, 32, palette.TouchRingLine, 2f);
        DrawCircle(_touchStart, 4f, palette.TouchKnobLine);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!CanAcceptInput())
        {
            return;
        }

        switch (@event)
        {
            case InputEventScreenTouch { Pressed: true } touch when !IsTrackingPointer():
                BeginPointer(touch.Index, touch.Position);
                AcceptEvent();
                break;

            case InputEventScreenTouch { Pressed: false } touch when touch.Index == _pointerIndex:
                ResetInput();
                AcceptEvent();
                break;

            case InputEventScreenDrag drag when drag.Index == _pointerIndex:
                UpdatePointerPosition(drag.Position);
                AcceptEvent();
                break;

            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mouseDown when !IsTrackingPointer():
                BeginPointer(MousePointerId, mouseDown.Position);
                AcceptEvent();
                break;

            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } when _pointerIndex == MousePointerId:
                ResetInput();
                AcceptEvent();
                break;

            case InputEventMouseMotion motion when _pointerIndex == MousePointerId && (motion.ButtonMask & MouseButtonMask.Left) != 0:
                UpdatePointerPosition(motion.Position);
                AcceptEvent();
                break;
        }
    }

    private void OnGameOver()
    {
        ResetInput();
    }

    private void OnPauseChanged(bool isPaused)
    {
        if (isPaused)
        {
            ResetInput();
        }
    }

    private void OnControlSetupCompleted()
    {
        ResetInput();
    }

    private void OnControlModeChanged(InputControlMode mode)
    {
        if (mode != InputControlMode.ScreenDrag)
        {
            _showDragOrigin = false;
        }

        if (mode != InputControlMode.Joystick)
        {
            _showJoystick = false;
        }

        QueueRedraw();
    }

    private bool CanAcceptInput()
    {
        return InputSettings.Instance.IsControlConfigured
            && !GameManager.Instance.IsGameOver
            && !GameManager.Instance.IsPaused;
    }

    private bool IsTrackingPointer()
    {
        return _pointerIndex != -2;
    }

    private void BeginPointer(int index, Vector2 position)
    {
        _pointerIndex = index;
        _touchStart = position;
        _joystickCenter = position;
        _knobOffset = Vector2.Zero;
        _dragDelta = Vector2.Zero;
        var mode = InputSettings.Instance.ControlMode;
        _showDragOrigin = mode == InputControlMode.ScreenDrag;
        _showJoystick = mode == InputControlMode.Joystick;
        UpdateInputVector();
        QueueRedraw();
    }

    private void UpdatePointerPosition(Vector2 position)
    {
        if (InputSettings.Instance.ControlMode == InputControlMode.Joystick)
        {
            _knobOffset = position - _joystickCenter;
            var maxRadius = GameConstants.JoystickMaxRadiusPixels;
            if (_knobOffset.Length() > maxRadius)
            {
                _knobOffset = _knobOffset.Normalized() * maxRadius;
            }

            UpdateInputVector();
            QueueRedraw();
            return;
        }

        _dragDelta = position - _touchStart;
        UpdateInputVector();
    }

    private void UpdateInputVector()
    {
        InputVector = InputSettings.Instance.ControlMode == InputControlMode.Joystick
            ? GetJoystickInputVector()
            : GetScreenDragInputVector();
    }

    private Vector2 GetJoystickInputVector()
    {
        var knobLength = _knobOffset.Length();
        if (knobLength < GameConstants.JoystickDeadzonePixels)
        {
            return Vector2.Zero;
        }

        var direction = _knobOffset / knobLength;
        var speedRatio = Mathf.Clamp(knobLength / GameConstants.JoystickMaxRadiusPixels, 0f, 1f);
        return direction * speedRatio;
    }

    private Vector2 GetScreenDragInputVector()
    {
        var distance = _dragDelta.Length();
        var deadzone = GameConstants.TouchDragDeadzonePixels;
        var fullSpeedDistance = GameConstants.TouchDragFullSpeedDistancePixels;

        if (distance < deadzone)
        {
            return Vector2.Zero;
        }

        var direction = _dragDelta / distance;
        var speedRatio = (distance - deadzone) / (fullSpeedDistance - deadzone);
        speedRatio = Mathf.Clamp(speedRatio, 0f, 1f);
        return direction * speedRatio;
    }

    private void ResetInput()
    {
        _pointerIndex = -2;
        _knobOffset = Vector2.Zero;
        _dragDelta = Vector2.Zero;
        InputVector = Vector2.Zero;
        _showDragOrigin = false;
        _showJoystick = false;
        QueueRedraw();
    }
}
