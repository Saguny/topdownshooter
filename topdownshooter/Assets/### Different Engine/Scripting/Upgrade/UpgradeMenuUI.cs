using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TMP_Text[] nameTexts;
    [SerializeField] private TMP_Text[] descriptionTexts;
    [SerializeField] private Image[] iconImages;        
    [SerializeField] private Sprite defaultIcon;        

    private readonly List<UpgradeData> _current = new();
    private Action<UpgradeData> _onChosen;
    private int _selectedIndex = 0;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        // ensure no stale listeners
        if (upgradeButtons != null)
            foreach (var b in upgradeButtons) if (b != null) b.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;
        HandleKeyboard();
    }

    public void Open(List<UpgradeData> upgrades, Action<UpgradeData> callback)
    {
        if (panel == null) return;

        panel.SetActive(true);
        _onChosen = callback;
        _current.Clear();
        if (upgrades != null) _current.AddRange(upgrades);

        // populate up to button count
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            bool has = (upgrades != null && i < upgrades.Count);
            var btn = upgradeButtons[i];

            if (btn == null) continue;

            btn.gameObject.SetActive(has);
            btn.onClick.RemoveAllListeners();

            if (!has) continue;

            var data = upgrades[i];

            // texts
            if (nameTexts != null && i < nameTexts.Length && nameTexts[i] != null)
                nameTexts[i].text = !string.IsNullOrEmpty(data.title) ? data.title : "Upgrade";

            if (descriptionTexts != null && i < descriptionTexts.Length && descriptionTexts[i] != null)
                descriptionTexts[i].text = !string.IsNullOrEmpty(data.description) ? data.description : string.Empty;

            // icons (optional — UpgradeData has no icon by default; we use fallback if provided)
            if (iconImages != null && i < iconImages.Length && iconImages[i] != null)
            {
                if (defaultIcon != null)
                {
                    iconImages[i].sprite = defaultIcon;
                    iconImages[i].enabled = true;
                }
                else
                {
                    iconImages[i].enabled = false;
                }
            }

            int index = i; // cache for closure
            btn.onClick.AddListener(() => Choose(index));
        }

        // set initial selection to first active button for keyboard users
        _selectedIndex = FirstActiveIndex();
        FocusButton(_selectedIndex);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        
    }

    private void Choose(int index)
    {
        if (index < 0 || index >= _current.Count) return;

        Close();
        _onChosen?.Invoke(_current[index]);
    }

    // ===== Keyboard navigation =====
    private void HandleKeyboard()
    {
        // arrows or WASD
        int step = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) step = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) step = +1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) step = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) step = +1;

        if (step != 0)
        {
            _selectedIndex = NextActiveIndex(_selectedIndex, step);
            FocusButton(_selectedIndex);
        }

        // confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (IsActive(_selectedIndex)) Choose(_selectedIndex);
        }

        // optional: cancel (esc) – do nothing or close w/o choose (design choice)
        // if (Input.GetKeyDown(KeyCode.Escape)) { /* ignore or Close(); */ }
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
        // visually focus
        EventSystem es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(b.gameObject);
        _selectedIndex = i;
    }
}
