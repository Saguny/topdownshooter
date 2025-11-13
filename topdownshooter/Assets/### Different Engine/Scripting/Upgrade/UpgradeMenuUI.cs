using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TMP_Text[] nameTexts;
    [SerializeField] private TMP_Text[] descriptionTexts;
    [SerializeField] private Image[] iconImages;
    [SerializeField] private Sprite defaultIcon;

    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text[] lvlText;

    private readonly List<UpgradeData> current = new();
    private Action<UpgradeData> onChosen;
    private int selectedIndex = 0;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        if (upgradeButtons != null)
        {
            foreach (var b in upgradeButtons)
            {
                if (b != null)
                    b.onClick.RemoveAllListeners();
            }
        }
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;
        HandleKeyboard();
    }

    public void Open(List<UpgradeData> upgrades, Action<UpgradeData> callback, int level)
    {
        if (panel == null) return;
        onChosen = callback;

        if (levelText != null)
            levelText.text = $"Current Level: {level - 1}";

        var filtered = new List<UpgradeData>();
        if (upgrades != null)
        {
            foreach (var u in upgrades)
            {
                if (u == null) continue;
                if (!u.IsAtCap) filtered.Add(u);
            }
        }

        if (filtered.Count == 0)
        {
            Close();
            onChosen?.Invoke(null);
            return;
        }

        panel.SetActive(true);
        current.Clear();
        current.AddRange(filtered);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            bool has = i < current.Count;
            var btn = upgradeButtons[i];
            if (btn == null) continue;

            btn.gameObject.SetActive(has);
            btn.onClick.RemoveAllListeners();
            if (!has) continue;

            var data = current[i];

            if (nameTexts != null && i < nameTexts.Length && nameTexts[i] != null)
                nameTexts[i].text = data.GetDisplayTitle();

            if (lvlText != null && i < lvlText.Length && lvlText[i] != null)
                lvlText[i].text = data.GetLevelProgress();

            if (descriptionTexts != null && i < descriptionTexts.Length && descriptionTexts[i] != null)
                descriptionTexts[i].text = !string.IsNullOrEmpty(data.description)
                    ? data.description
                    : string.Empty;

            if (iconImages != null && i < iconImages.Length && iconImages[i] != null)
            {
                Sprite spriteToUse = data.icon != null ? data.icon : defaultIcon;

                if (spriteToUse != null)
                {
                    iconImages[i].sprite = spriteToUse;
                    iconImages[i].enabled = true;
                }
                else
                {
                    iconImages[i].enabled = false;
                }
            }

            int index = i;
            btn.onClick.AddListener(() => Choose(index));
        }

        selectedIndex = FirstActiveIndex();
        FocusButton(selectedIndex);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Choose(int index)
    {
        if (index < 0 || index >= current.Count) return;
        Close();
        onChosen?.Invoke(current[index]);
    }

    private void HandleKeyboard()
    {
        int step = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) step = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) step = +1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) step = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) step = +1;

        if (step != 0)
        {
            selectedIndex = NextActiveIndex(selectedIndex, step);
            FocusButton(selectedIndex);
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            if (IsActive(selectedIndex))
                Choose(selectedIndex);
        }
    }

    private int FirstActiveIndex()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
            if (IsActive(i)) return i;
        return -1;
    }

    private int NextActiveIndex(int start, int step)
    {
        if (upgradeButtons == null || upgradeButtons.Length == 0) return -1;
        int count = upgradeButtons.Length;
        int i = start;
        for (int k = 0; k < count; k++)
        {
            i = (i + step + count) % count;
            if (IsActive(i)) return i;
        }
        return start;
    }

    private bool IsActive(int i)
    {
        if (i < 0 || i >= upgradeButtons.Length) return false;
        var b = upgradeButtons[i];
        return b != null && b.gameObject.activeSelf && b.interactable;
    }

    private void FocusButton(int i)
    {
        if (!IsActive(i)) return;
        var b = upgradeButtons[i];
        EventSystem es = EventSystem.current;
        if (es != null)
            es.SetSelectedGameObject(b.gameObject);
        selectedIndex = i;
    }
}
