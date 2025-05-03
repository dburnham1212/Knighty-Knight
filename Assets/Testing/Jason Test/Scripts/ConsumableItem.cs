using UnityEngine;

public class ConsumableItem
{
    public string Name { get; }
    public int Max { get; set; }
    public int Count { get; set; }

    public ConsumableItem(string type)
    {
        Name = type;

        switch (Name)
        {
            case "Potion":
                Max = 3;
                Count = Max;
                break;
            case "Throwing Knife":
                Max = 10;
                Count = Max;
                break;
        }
    }
}
