using Godot;

namespace Shapes;

public static class MobileInput
{
    public static Vector2 GetMoveVector(Node context)
    {
        if (InputSettings.Instance == null || !InputSettings.Instance.IsControlConfigured)
        {
            return Vector2.Zero;
        }

        if (TouchInput.Instance != null && TouchInput.Instance.InputVector.LengthSquared() > 0.0001f)
        {
            return TouchInput.Instance.InputVector;
        }

        if (!IsMobilePlatform())
        {
            return Input.GetVector("move_left", "move_right", "move_up", "move_down");
        }

        return Vector2.Zero;
    }

    public static bool IsMobilePlatform()
    {
        var name = OS.GetName();
        return name == "Android" || name == "iOS";
    }
}
