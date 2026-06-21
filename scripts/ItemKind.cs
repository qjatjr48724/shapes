using Godot;

namespace Shapes;

public enum ItemKind
{
    DamageUp50,
    DamageUp30,
    FireRateUp300,
    FireRateUp200,
    MoveSpeedUp200,
    BulletSpeedUp300,
    HealthRecover,
}

public static class ItemKindExtensions
{
    public static float GetDuration(this ItemKind kind)
    {
        return kind switch
        {
            ItemKind.DamageUp50 => 5f,
            ItemKind.DamageUp30 => 10f,
            ItemKind.FireRateUp300 => 5f,
            ItemKind.FireRateUp200 => 5f,
            ItemKind.MoveSpeedUp200 => 10f,
            ItemKind.BulletSpeedUp300 => 10f,
            _ => 0f,
        };
    }

    public static float GetDamageMultiplier(this ItemKind kind)
    {
        return kind switch
        {
            ItemKind.DamageUp50 => 1.5f,
            ItemKind.DamageUp30 => 1.3f,
            _ => 1f,
        };
    }

    public static float GetFireRateMultiplier(this ItemKind kind)
    {
        return kind switch
        {
            ItemKind.FireRateUp300 => 4f,
            ItemKind.FireRateUp200 => 3f,
            ItemKind.BulletSpeedUp300 => 1.15f,
            _ => 1f,
        };
    }

    public static float GetMoveSpeedMultiplier(this ItemKind kind)
    {
        return kind switch
        {
            ItemKind.MoveSpeedUp200 => 1.3f,
            _ => 1f,
        };
    }

    public static float GetBulletSpeedMultiplier(this ItemKind kind)
    {
        return kind switch
        {
            ItemKind.BulletSpeedUp300 => 2f,
            _ => 1f,
        };
    }

    public static ItemKind RollRandomBuff()
    {
        return (ItemKind)(GD.Randi() % 6);
    }
}
