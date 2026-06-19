using Godot;

namespace Shapes;

public partial class BossSpawner : Node2D
{
    [Export]
    public PackedScene BossScene { get; set; } = null!;

    private int _nextBossIndex;

    private static readonly BossKind[] BossSchedule =
    {
        BossKind.MidBoss1,
        BossKind.MidBoss2,
        BossKind.FinalBoss,
    };

    public override void _Ready()
    {
        _nextBossIndex = 0;
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance.IsGameOver || BossScene == null || GameManager.Instance.IsPaused)
        {
            return;
        }

        if (GameManager.Instance.IsBossActive)
        {
            return;
        }

        if (_nextBossIndex >= BossSchedule.Length)
        {
            return;
        }

        var spawnTime = GameConstants.BossSpawnInterval * (_nextBossIndex + 1);
        if (GameManager.Instance.GameTime < spawnTime)
        {
            return;
        }

        SpawnBoss(BossSchedule[_nextBossIndex]);
        _nextBossIndex++;
    }

    private void SpawnBoss(BossKind kind)
    {
        ClearRegularEnemies();

        var boss = BossScene.Instantiate<Boss>();
        boss.Configure(kind);
        GetTree().CurrentScene.AddChild(boss);

        var rect = GetViewport().GetVisibleRect();
        boss.GlobalPosition = new Vector2(rect.Size.X / 2f, -GameConstants.GetBossSize(kind) / 2f);
    }

    private void ClearRegularEnemies()
    {
        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
            {
                enemy.QueueFree();
            }
        }
    }
}
