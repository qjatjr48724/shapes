using Godot;

namespace Shapes;

public class AbilityChoice
{
    public AbilityKind Kind { get; init; }
    public AbilityRarity Rarity { get; init; }
    public int Amount { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public string RarityLabel => Rarity switch
    {
        AbilityRarity.Hidden => "히든",
        AbilityRarity.Unique => "유니크",
        AbilityRarity.Epic => "에픽",
        _ => "레어",
    };

    public Color RarityColor => Rarity switch
    {
        AbilityRarity.Hidden => new Color(1f, 0.35f, 0.55f),
        AbilityRarity.Unique => new Color(1f, 0.78f, 0.2f),
        AbilityRarity.Epic => new Color(0.65f, 0.35f, 0.95f),
        _ => new Color(0.35f, 0.65f, 1f),
    };
}
