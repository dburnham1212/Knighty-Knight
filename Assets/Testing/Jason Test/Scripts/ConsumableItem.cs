using UnityEngine;

public class ConsumableItem
{
    static int count = 0;

    int upgradedMax;
    int upgrade;
    public string Name { get; }
    public int Max { get; set; }
    public int Count { get; set; }

    public void Upgrade()
    {
        if (Max < upgradedMax)
        {
            Debug.Log("Upgraded " + Name + " from " + Max + " to " + (Max + upgrade));
            Max += upgrade;
        }
        else
            Debug.Log("Max upgrade reached");
    }

    public void Add(int count)
    {
        if (Count + count < Max)
        {
            Debug.Log("Picked up " + count + " " + Name + "(s)");
            Count += count;
        }
        else
        {
            Debug.Log("Picked up " + (Max - Count) + " " + Name + "(s)");
            Count = Max;
        }
    }

    public ConsumableItem()
    {
        switch (count)
        {
            case 1:
                Name = "Throwing Knives";
                upgradedMax = 50;
                upgrade = 4;
                Max = 10;
                Count = 0;
                break;
            default:
                Name = "Potions";
                upgradedMax = 10;
                upgrade = 1;
                Max = 3;
                Count = 0;
                break;
        }
        Debug.Log(Name + " created");

        count++;
    }
}
