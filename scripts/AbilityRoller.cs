using System;
using System.Collections.Generic;
using Godot;

namespace Shapes;

public static class AbilityRoller
{
    private const float HiddenChance = 0.01f;
    private const float UniqueChance = 0.05f;
    private const float EpicChance = 0.13f;

    public static AbilityChoice[] RollThreeChoices(PlayerAbilities abilities)
    {
        var results = new List<AbilityChoice>();
        var usedKinds = new HashSet<AbilityKind>();
        var attempts = 0;

        while (results.Count < 3 && attempts < 40)
        {
            attempts++;
            var rarity = RollRarity();
            var kind = PickKind(rarity, usedKinds, abilities);
            if (kind == null)
            {
                continue;
            }

            usedKinds.Add(kind.Value);
            results.Add(CreateChoice(kind.Value, rarity, abilities));
        }

        while (results.Count < 3)
        {
            var rarity = AbilityRarity.Rare;
            var kind = PickKind(rarity, usedKinds, abilities) ?? AbilityKind.AttackSpeedBoost;
            usedKinds.Add(kind);
            results.Add(CreateChoice(kind, rarity, abilities));
        }

        return results.ToArray();
    }

    private static AbilityRarity RollRarity()
    {
        var roll = GD.Randf();
        if (roll < HiddenChance)
        {
            return AbilityRarity.Hidden;
        }

        if (roll < HiddenChance + UniqueChance)
        {
            return AbilityRarity.Unique;
        }

        if (roll < HiddenChance + UniqueChance + EpicChance)
        {
            return AbilityRarity.Epic;
        }

        return AbilityRarity.Rare;
    }

    private static AbilityKind? PickKind(AbilityRarity rarity, HashSet<AbilityKind> used, PlayerAbilities abilities)
    {
        var pool = new List<AbilityKind>
        {
            AbilityKind.StraightProjectile,
            AbilityKind.DiagonalProjectile,
            AbilityKind.AttackSpeedBoost,
            AbilityKind.BulletSpeedBoost,
        };

        if (rarity >= AbilityRarity.Epic)
        {
            pool.Add(AbilityKind.HomingProjectile);
        }

        if (abilities.DiagonalCount > 0)
        {
            pool.Add(AbilityKind.BulletBounce);
        }

        if (abilities.HasStraightOrDiagonal)
        {
            pool.Add(AbilityKind.BulletPierce);
        }

        pool.RemoveAll(used.Contains);
        if (pool.Count == 0)
        {
            return null;
        }

        return pool[(int)(GD.Randi() % pool.Count)];
    }

    public static int GetProjectileAmount(AbilityRarity rarity)
    {
        return rarity switch
        {
            AbilityRarity.Hidden => 5,
            AbilityRarity.Unique => 3,
            AbilityRarity.Epic => 2,
            _ => 1,
        };
    }

    public static int GetHomingAmount(AbilityRarity rarity)
    {
        return rarity switch
        {
            AbilityRarity.Hidden => 3,
            AbilityRarity.Unique => 2,
            AbilityRarity.Epic => 1,
            _ => 0,
        };
    }

    public static (float attack, float fireRate) GetStatBoost(AbilityRarity rarity)
    {
        return rarity switch
        {
            AbilityRarity.Unique => (0.5f, 0.3f),
            AbilityRarity.Epic => (0.3f, 0.15f),
            AbilityRarity.Hidden => (0.5f, 0.3f),
            _ => (0.15f, 0.05f),
        };
    }

    public static float GetBulletSpeedBoost(AbilityRarity rarity)
    {
        return rarity switch
        {
            AbilityRarity.Hidden => 1f,
            AbilityRarity.Unique => 0.5f,
            AbilityRarity.Epic => 0.3f,
            _ => 0.1f,
        };
    }

    private static AbilityChoice CreateChoice(AbilityKind kind, AbilityRarity rarity, PlayerAbilities abilities)
    {
        return kind switch
        {
            AbilityKind.StraightProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetProjectileAmount(rarity),
                Title = "직진형 발사체",
                Description = $"직선 탄환을 발사하는 발사체 {GetProjectileAmount(rarity)}개 추가\n(본체 공격력의 50%, 초당 0.6발)",
            },
            AbilityKind.DiagonalProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetProjectileAmount(rarity),
                Title = "사선형 발사체",
                Description = $"사선 탄환을 발사하는 발사체 {GetProjectileAmount(rarity)}개 추가\n(본체 공격력의 50%, 초당 0.6발)",
            },
            AbilityKind.HomingProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetHomingAmount(rarity),
                Title = "유도탄",
                Description = $"유도 발사체 {GetHomingAmount(rarity)}개 추가\n(본체 공격력의 135%, 초당 0.4발)",
            },
            AbilityKind.BulletSpeedBoost => CreateBulletSpeedChoice(rarity),
            AbilityKind.BulletBounce => new AbilityChoice
            {
                Kind = kind,
                Rarity = AbilityRarity.Rare,
                Amount = 1,
                Title = "발사체 튕기기",
                Description = $"직진·사선 탄환이 좌우 벽에 반사됩니다.\n튕김 횟수 +1 (현재 {abilities.BounceCount}회)",
            },
            AbilityKind.BulletPierce => new AbilityChoice
            {
                Kind = kind,
                Rarity = AbilityRarity.Rare,
                Amount = 1,
                Title = "발사체 관통",
                Description = $"직진·사선 탄환이 적을 관통합니다.\n관통 횟수 +1 (현재 {abilities.PierceCount}회)",
            },
            _ => CreateStatBoostChoice(rarity),
        };
    }

    private static AbilityChoice CreateBulletSpeedChoice(AbilityRarity rarity)
    {
        var boostPercent = Mathf.RoundToInt(GetBulletSpeedBoost(rarity) * 100f);
        return new AbilityChoice
        {
            Kind = AbilityKind.BulletSpeedBoost,
            Rarity = rarity,
            Amount = 0,
            Title = "발사체 속도 증가",
            Description = $"발사체 속도 +{boostPercent}%",
        };
    }

    private static AbilityChoice CreateStatBoostChoice(AbilityRarity rarity)
    {
        var (attack, fireRate) = GetStatBoost(rarity);
        var attackPercent = Mathf.RoundToInt(attack * 100f);
        var fireRatePercent = Mathf.RoundToInt(fireRate * 100f);
        return new AbilityChoice
        {
            Kind = AbilityKind.AttackSpeedBoost,
            Rarity = rarity,
            Amount = 0,
            Title = "공격 강화",
            Description = $"공격력 +{attackPercent}%, 공격속도 +{fireRatePercent}%\n 증가",
        };
    }
}
