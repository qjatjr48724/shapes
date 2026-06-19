using Godot;

namespace Shapes;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    public int Score { get; private set; }
    public float GameTime { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsBossActive { get; private set; }

    [Signal]
    public delegate void ScoreChangedEventHandler(int newScore);

    [Signal]
    public delegate void GameOverEventHandler();

    [Signal]
    public delegate void PauseChangedEventHandler(bool isPaused);

    [Signal]
    public delegate void BossActiveChangedEventHandler(bool isBossActive);

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        ResetRun();
    }

    public override void _Process(double delta)
    {
        if (IsGameOver || IsPaused)
        {
            return;
        }

        GameTime += (float)delta;
    }

    public void ResetRun()
    {
        Score = 0;
        GameTime = 0f;
        IsGameOver = false;
        IsPaused = false;
        IsBossActive = false;
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    public void AddScore(int value)
    {
        if (IsGameOver)
        {
            return;
        }

        Score += value;
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        SetPaused(false);
        IsGameOver = true;
        EmitSignal(SignalName.GameOver);
    }

    public void TogglePause()
    {
        if (!InputSettings.Instance.IsControlConfigured)
        {
            return;
        }

        SetPaused(!IsPaused);
    }

    public void SetPaused(bool paused)
    {
        if (IsGameOver)
        {
            return;
        }

        if (IsPaused == paused)
        {
            return;
        }

        IsPaused = paused;
        EmitSignal(SignalName.PauseChanged, paused);
    }

    public void ForceResume()
    {
        if (IsGameOver)
        {
            return;
        }

        IsPaused = false;
        EmitSignal(SignalName.PauseChanged, false);
    }

    public void SetBossActive(bool active)
    {
        if (IsBossActive == active)
        {
            return;
        }

        IsBossActive = active;
        EmitSignal(SignalName.BossActiveChanged, active);
    }

    public static float GetEnemyMaxHealth(float gameTime)
    {
        return GameConstants.EnemyBaseHealth + gameTime * GameConstants.EnemyHealthPerSecond;
    }
}
