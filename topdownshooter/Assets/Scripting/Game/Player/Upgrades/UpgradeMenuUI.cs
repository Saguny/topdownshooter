using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeMenuUI : MonoBehaviour
{
    public GameObject panel;
    public Button[] upgradeButtons;
    public TMP_Text[] nameTexts;
    public TMP_Text[] descriptionTexts;
    public Image[] iconImages;

    private Action<UpgradeData> onUpgradeChosen;

    public void Open(List<UpgradeData> upgrades, Action<UpgradeData> callback)
    {
        panel.SetActive(true);
        onUpgradeChosen = callback;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < upgrades.Count)
            {
                UpgradeData data = upgrades[i];
                upgradeButtons[i].gameObject.SetActive(true);
                nameTexts[i].text = data.upgradeName;
                descriptionTexts[i].text = data.description;
                iconImages[i].sprite = data.icon;

                int index = i;
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => Choose(upgrades[index]));
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void Choose(UpgradeData chosen)
    {
        panel.SetActive(false);
        onUpgradeChosen?.Invoke(chosen);
    }
}
