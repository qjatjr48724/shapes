using Godot;

namespace Shapes;

public static class ItemDropper
{
    private static PackedScene? _pickupScene;

    public static void Drop(Node parent, Vector2 position, bool guaranteedHeal = false)
    {
        if (guaranteedHeal || GD.Randf() < GameConstants.ItemHealDropChance)
        {
            SpawnPickup(parent, position, ItemKind.HealthRecover);
        }

        if (GD.Randf() < GameConstants.ItemDropChance)
        {
            SpawnPickup(parent, position, ItemKindExtensions.RollRandomBuff());
        }
    }

    private static void SpawnPickup(Node parent, Vector2 position, ItemKind kind)
    {
        _pickupScene ??= GD.Load<PackedScene>("res://scenes/item/ItemPickup.tscn");
        if (_pickupScene == null)
        {
            return;
        }

        var pickup = _pickupScene.Instantiate<ItemPickup>();
        parent.AddChild(pickup);
        pickup.GlobalPosition = position;
        pickup.Configure(kind);
    }
}
