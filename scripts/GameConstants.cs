using Godot;

using System;

namespace Shapes;

public static class GameConstants
{
    public const int PlayerMaxHealth = 5;
    public const float InvincibilityDuration = 1f;
    public const int ScorePerKill = 1;
    public const int BulletDamage = 1;

    public const float PlayerMoveSpeed = 320f;
    public const float PlayerFireInterval = 0.35f;
    public const float PlayerRadius = 20f;

    public const float EnemyStartSpeed = 90f;
    public const float EnemySpeedIncreaseInterval = 6f;
    public const float EnemySpeedIncreasePerInterval = 2f;
    public const float EnemyBaseHealth = 1f;
    public const float EnemyHealthPerSecond = 0.15f;
    public const float EnemySize = 40f;
    public const float PentagonSize = 56f;

    public const float PentagonSpawnChance = 0.18f;
    public const float PentagonHealthMultiplierMin = 1.5f;
    public const float PentagonHealthMultiplierMax = 2f;
    public const float PentagonExpMultiplier = 1.5f;

    public const float SpawnIntervalBase = 3.5f;
    public const float SpawnIntervalMin = 0.65f;
    public const float SpawnIntervalDecay = 0.018f;
    public const float SpawnWarmupDuration = 25f;

    public const float BulletSpeed = 500f;
    public const float BulletLifetime = 3f;

    public static readonly Vector2 PlayerShootDirection = Vector2.Up;
    public static readonly Vector2 EnemyMoveDirection = Vector2.Down;

    public const int ExpPerKill = 5;
    public const int BaseExpToLevel = 18;
    public const float MoveSpeedPerLevel = 20f;
    public const float FireIntervalReductionPerLevel = 0.025f;
    public const float MinFireInterval = 0.12f;
    public const int BulletDamagePerLevel = 1;

    public const float TouchDragDeadzonePixels = 16f;
    public const float TouchDragFullSpeedDistancePixels = 120f;

    public const float JoystickDeadzonePixels = 16f;
    public const float JoystickMaxRadiusPixels = 90f;

    public const float BossSpawnInterval = 180f;
    public const float MidBoss1Health = 1000f;
    public const float MidBoss2Health = 3200f;
    public const float FinalBossHealth = 5500f;
    public const float BossDescendTargetY = 280f;
    public const float BossDescendSpeed = 200f;
    public const float BossHorizontalSpeed = 140f;
    public const float BossMidSize = 72f;
    public const float BossFinalSize = 96f;
    public const float BossFireIntervalMid = 1.8f;
    public const float BossFireIntervalFinal = 1.2f;
    public const float BossBulletSpeed = 300f;
    public const float BossBulletLifetime = 4f;
    public const int BossBulletDamage = 1;
    public const int BossScoreMid = 50;
    public const int BossScoreFinal = 100;
    public const int BossExpMid = 50;
    public const int BossExpFinal = 100;

    public const string AndroidPackageName = "com.shapes.game";
    public const string IosBundleIdentifier = "com.shapes.game";

    public static int GetExpRequiredForLevel(int level)
    {
        return BaseExpToLevel * level;
    }

    public static float GetSpawnInterval(float gameTime)
    {
        if (gameTime < SpawnWarmupDuration)
        {
            return SpawnIntervalBase;
        }

        var rampTime = gameTime - SpawnWarmupDuration;
        return Math.Max(SpawnIntervalMin, SpawnIntervalBase - rampTime * SpawnIntervalDecay);
    }

    public static float GetEnemySpeed(float gameTime)
    {
        var steps = (int)(gameTime / EnemySpeedIncreaseInterval);
        return EnemyStartSpeed + steps * EnemySpeedIncreasePerInterval;
    }

    public static float GetBossHealth(BossKind kind)
    {
        return kind switch
        {
            BossKind.MidBoss1 => MidBoss1Health,
            BossKind.MidBoss2 => MidBoss2Health,
            BossKind.FinalBoss => FinalBossHealth,
            _ => MidBoss1Health,
        };
    }

    public static float GetBossSize(BossKind kind)
    {
        return kind == BossKind.FinalBoss ? BossFinalSize : BossMidSize;
    }

    public static float GetBossFireInterval(BossKind kind)
    {
        return kind == BossKind.FinalBoss ? BossFireIntervalFinal : BossFireIntervalMid;
    }

    public static int GetBossScore(BossKind kind)
    {
        return kind == BossKind.FinalBoss ? BossScoreFinal : BossScoreMid;
    }

    public static int GetBossExp(BossKind kind)
    {
        return kind == BossKind.FinalBoss ? BossExpFinal : BossExpMid;
    }
}
