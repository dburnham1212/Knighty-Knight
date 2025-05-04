using UnityEngine;

public class Inventory
{
    public enum Type
    {
        Key,
        Consumable,
        Upgrade
    }

    public enum Name
    {
        Sword,
        Potion,
        ThrowingKnife
    }

    public const int KEY_ITEMS = 1;
    public const int CONSUMABLES = 2;
    public KeyItem[] KeyItems { get; set; }
    public ConsumableItem[] Consumables { get; set; }

    public Inventory()
    {
        KeyItems = new KeyItem[KEY_ITEMS];
        Consumables = new ConsumableItem[CONSUMABLES];

        InitializeArrays();
    }

    void InitializeArrays()
    {
        for (int index = 0; index < KEY_ITEMS; index++)
            KeyItems[index] = new KeyItem();

        for (int index = 0; index < CONSUMABLES; index++)
            Consumables[index] = new ConsumableItem();
    }
}
