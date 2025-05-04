using System.Threading;
using UnityEngine;

public class KeyItem
{
    static int count = 0;

    int maxUpgrade;
    public string Name { get; }
    public int UpgradeLevel { get; set; } // 0 = unobtained

    public void Upgrade()
    {
        if (UpgradeLevel < maxUpgrade)
        {
            UpgradeLevel++;
            Debug.Log(Name + " upgraded to level " + UpgradeLevel);
        }
        else
            Debug.Log("Max Upgrade Reached");
    }

    public KeyItem()
    {
        switch (count)
        {
            default:
                Name = "Sword";
                maxUpgrade = 3;
                break;

        }

        Debug.Log(Name + " created");
        UpgradeLevel = 0;
        count++;
    }
}
