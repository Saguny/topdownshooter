using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public int gearCount = 0;
    public int gearsForUpgrade = 5; // wie viele Zahnr�der bis zum Upgrade

    public void AddGears(int amount)
    {
        gearCount += amount;
        Debug.Log("Gears collected: " + gearCount);

        if (gearCount >= gearsForUpgrade)
        {
            // Spieler kann Upgrade ausw�hlen
            gearCount -= gearsForUpgrade;
            OpenUpgradeMenu();
        }
    }

    private void OpenUpgradeMenu()
    {
        Debug.Log("Upgrade menu open!");
        // Hier kannst du z.B. ein UI-Panel aktivieren, Buttons anzeigen usw.
    }
}
