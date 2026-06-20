using System;
using System.Collections.Generic;
using Godot;

namespace Shapes;

public static class AbilityRoller
{
    private const float HiddenChance = 0.01f;
    private const float UniqueChance = 0.05f;
    private const float EpicChance = 0.13f;

    public static AbilityChoice[] RollThreeChoices()
    {
        var results = new List<AbilityChoice>();
        var usedKinds = new HashSet<AbilityKind>();
        var attempts = 0;

        while (results.Count < 3 && attempts < 40)
        {
            attempts++;
            var rarity = RollRarity();
            var kind = PickKind(rarity, usedKinds);
            if (kind == null)
            {
                continue;
            }

            usedKinds.Add(kind.Value);
            results.Add(CreateChoice(kind.Value, rarity));
        }

        while (results.Count < 3)
        {
            var rarity = AbilityRarity.Rare;
            var kind = PickKind(rarity, usedKinds) ?? AbilityKind.AttackSpeedBoost;
            usedKinds.Add(kind);
            results.Add(CreateChoice(kind, rarity));
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

    private static AbilityKind? PickKind(AbilityRarity rarity, HashSet<AbilityKind> used)
    {
        var pool = new List<AbilityKind>
        {
            AbilityKind.StraightProjectile,
            AbilityKind.DiagonalProjectile,
            AbilityKind.AttackSpeedBoost,
        };

        if (rarity >= AbilityRarity.Epic)
        {
            pool.Add(AbilityKind.HomingProjectile);
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

    private static AbilityChoice CreateChoice(AbilityKind kind, AbilityRarity rarity)
    {
        return kind switch
        {
            AbilityKind.StraightProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetProjectileAmount(rarity),
                Title = "직진형 발사체",
                Description = $"직선 탄환을 발사하는 발사체 {GetProjectileAmount(rarity)}개 추가\n",
            },
            AbilityKind.DiagonalProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetProjectileAmount(rarity),
                Title = "사선형 발사체",
                Description = $"사선 탄환을 발사하는 발사체 {GetProjectileAmount(rarity)}개 추가\n",
            },
            AbilityKind.HomingProjectile => new AbilityChoice
            {
                Kind = kind,
                Rarity = rarity,
                Amount = GetHomingAmount(rarity),
                Title = "유도탄",
                Description = $"유도 발사체 {GetHomingAmount(rarity)}개 추가\n(본체 공격력의 33%, 초당 0.4발)",
            },
            _ => CreateStatBoostChoice(rarity),
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
