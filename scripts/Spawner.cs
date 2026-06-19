using Godot;

namespace Shapes;

public partial class Spawner : Node2D
{
    [Export]
    public PackedScene EnemyScene { get; set; } = null!;

    private float _spawnTimer;

    public override void _Ready()
    {
        _spawnTimer = GameConstants.SpawnIntervalBase;
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused || EnemyScene == null)
        {
            return;
        }

        if (GameManager.Instance.IsBossActive)
        {
            return;
        }

        _spawnTimer -= (float)delta;
        if (_spawnTimer > 0f)
        {
            return;
        }

        SpawnEnemy();
        _spawnTimer = GetCurrentSpawnInterval();
    }

    private float GetCurrentSpawnInterval()
    {
        return GameConstants.GetSpawnInterval(GameManager.Instance.GameTime);
    }

    private void SpawnEnemy()
    {
        var enemy = EnemyScene.Instantiate<Enemy>();
        enemy.Configure(RollEnemyKind());
        GetTree().CurrentScene.AddChild(enemy);
        enemy.GlobalPosition = GetRandomSpawnPosition();
    }

    private static EnemyKind RollEnemyKind()
    {
        return GD.Randf() < GameConstants.PentagonSpawnChance
            ? EnemyKind.Pentagon
            : EnemyKind.Square;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        var rect = GetViewport().GetVisibleRect();
        var margin = 40f;
        var x = (float)GD.RandRange(margin, rect.Size.X - margin);
        return new Vector2(x, -margin);
    }
}
