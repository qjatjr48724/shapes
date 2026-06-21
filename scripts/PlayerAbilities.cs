namespace Shapes;

public class PlayerAbilities
{
    public int StraightCount { get; private set; }
    public int DiagonalCount { get; private set; }
    public int HomingCount { get; private set; }
    public int BounceCount { get; private set; }
    public int PierceCount { get; private set; }
    public float AttackMultiplier { get; private set; } = 1f;
    public float FireRateMultiplier { get; private set; } = 1f;
    public float BulletSpeedMultiplier { get; private set; } = 1f;

    public bool HasStraightOrDiagonal => StraightCount > 0 || DiagonalCount > 0;

    public void Reset()
    {
        StraightCount = 0;
        DiagonalCount = 0;
        HomingCount = 0;
        BounceCount = 0;
        PierceCount = 0;
        AttackMultiplier = 1f;
        FireRateMultiplier = 1f;
        BulletSpeedMultiplier = 1f;
    }

    public void Apply(AbilityChoice choice)
    {
        switch (choice.Kind)
        {
            case AbilityKind.StraightProjectile:
                StraightCount += choice.Amount;
                break;
            case AbilityKind.DiagonalProjectile:
                DiagonalCount += choice.Amount;
                break;
            case AbilityKind.HomingProjectile:
                HomingCount += choice.Amount;
                break;
            case AbilityKind.AttackSpeedBoost:
                var (attack, fireRate) = AbilityRoller.GetStatBoost(choice.Rarity);
                AttackMultiplier *= 1f + attack;
                FireRateMultiplier *= 1f + fireRate;
                break;
            case AbilityKind.BulletSpeedBoost:
                BulletSpeedMultiplier *= 1f + AbilityRoller.GetBulletSpeedBoost(choice.Rarity);
                break;
            case AbilityKind.BulletBounce:
                BounceCount += choice.Amount;
                break;
            case AbilityKind.BulletPierce:
                PierceCount += choice.Amount;
                break;
        }
    }
}
