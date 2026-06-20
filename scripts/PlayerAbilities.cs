namespace Shapes;

public class PlayerAbilities
{
    public int StraightCount { get; private set; }
    public int DiagonalCount { get; private set; }
    public int HomingCount { get; private set; }
    public float AttackMultiplier { get; private set; } = 1f;
    public float FireRateMultiplier { get; private set; } = 1f;

    public void Reset()
    {
        StraightCount = 0;
        DiagonalCount = 0;
        HomingCount = 0;
        AttackMultiplier = 1f;
        FireRateMultiplier = 1f;
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
        }
    }
}
