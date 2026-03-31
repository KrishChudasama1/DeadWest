using System;
using System.Collections.Generic;
using UnityEngine;



public class InventoryManager : MonoBehaviour
{
    [Serializable]
    private class GunEntry
    {
        public RevolverData gunData;
        public bool unlockedAtStart = true;

        [NonSerialized] public bool isUnlocked;
    }

    public static bool IsInventoryOpen { get; private set; }

   
    [Header("Inventory Toggle")]
    [SerializeField] private KeyCode inventoryKey = KeyCode.Tab;

    [Header("References")]
    [Tooltip("If empty, InventoryManager will try to find a Revolver in the scene.")]
    [SerializeField] private Revolver revolver;

    [Header("Gun Options")]
    [Tooltip("Configure all gun assets that can appear in the inventory.")]
    [SerializeField] private List<GunEntry> gunEntries = new List<GunEntry>();

    [Header("Window")]
    [SerializeField] private Rect windowRect = new Rect(24f, 24f, 360f, 300f);
    [SerializeField] private Rect shopWindowRect = new Rect(410f, 24f, 320f, 220f);

    private int _selectedIndex;
    private bool _isShopOpen;

    private void Awake()
    {
        if (revolver == null)
            revolver = FindFirstObjectByType<Revolver>();

        InitializeUnlockState();

       
        SyncSelectionWithCurrentGun();
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();
    }

    private void OnDisable()
    {
        
        if (IsInventoryOpen)
            SetInventoryOpen(false);

        _isShopOpen = false;
    }

    private void OnGUI()
    {
        if (!IsInventoryOpen)
            return;

        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawInventoryWindow, "Inventory");

        if (_isShopOpen)
            shopWindowRect = GUI.Window(GetInstanceID() + 1, shopWindowRect, DrawShopWindow, "Shop");
    }

    private void DrawInventoryWindow(int windowId)
    {
        GUILayout.Space(8f);

        
        if (revolver == null)
        {
            GUILayout.Label("No Revolver found in scene.");
            if (GUILayout.Button("Close"))
                SetInventoryOpen(false);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
            return;
        }

        
        if (gunEntries.Count == 0)
        {
            GUILayout.Label("No gun assets configured.");
            if (GUILayout.Button("Close"))
                SetInventoryOpen(false);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
            return;
        }

        GUILayout.Label("Select Gun:");

        bool hasUnlockedGuns = false;

        for (int i = 0; i < gunEntries.Count; i++)
        {
            GunEntry entry = gunEntries[i];
            string label = entry.gunData != null
                ? entry.gunData.weaponName
                : $"Gun {i + 1}";

            if (!entry.isUnlocked)
                label += " (Locked)";

            if (entry.isUnlocked)
                hasUnlockedGuns = true;

            bool selected = i == _selectedIndex;
            GUI.enabled = entry.isUnlocked;
            bool clicked = GUILayout.Toggle(selected, label, "Button", GUILayout.Height(28f));
            GUI.enabled = true;

            if (clicked && entry.isUnlocked)
                _selectedIndex = i;
        }

        if (!hasUnlockedGuns)
            GUILayout.Label("No unlocked guns yet.");

        GUILayout.Space(10f);

        using (new GUILayout.HorizontalScope())
        {
            GUI.enabled = HasValidSelectedGun();
            if (GUILayout.Button("Equip", GUILayout.Height(30f)))
                ApplySelectedGun();
            GUI.enabled = true;

            if (GUILayout.Button("Shop", GUILayout.Height(30f)))
                _isShopOpen = true;

            if (GUILayout.Button("Close", GUILayout.Height(30f)))
                SetInventoryOpen(false);
        }

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private void ToggleInventory()
    {
        bool open = !IsInventoryOpen;
        if (open)
            SyncSelectionWithCurrentGun();

        SetInventoryOpen(open);
    }

    private void SetInventoryOpen(bool open)
    {
        IsInventoryOpen = open;
        if (!open)
            _isShopOpen = false;

        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = open;
    }

    private void DrawShopWindow(int windowId)
    {
        GUILayout.Space(8f);
        GUILayout.Label("Shop window (placeholder)");
        GUILayout.Label("Adding purchasable items here next.");

        GUILayout.Space(10f);
        if (GUILayout.Button("Back", GUILayout.Height(30f)))
            _isShopOpen = false;

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    
    private void ApplySelectedGun()
    {
        if (revolver == null)
            return;

        if (_selectedIndex < 0 || _selectedIndex >= gunEntries.Count)
            return;

        GunEntry selectedEntry = gunEntries[_selectedIndex];
        if (!selectedEntry.isUnlocked || selectedEntry.gunData == null)
        {
            Debug.LogWarning("[InventoryManager] Selected gun is locked or missing data.");
            return;
        }

        revolver.Equip(selectedEntry.gunData);
        SetInventoryOpen(false);
    }

    public void UnlockGun(RevolverData gunData)
    {
        if (gunData == null)
            return;

        for (int i = 0; i < gunEntries.Count; i++)
        {
            if (gunEntries[i].gunData == gunData)
            {
                gunEntries[i].isUnlocked = true;
                return;
            }
        }

        // If a gun is unlocked at runtime but was not preconfigured, add it automatically.
        gunEntries.Add(new GunEntry
        {
            gunData = gunData,
            unlockedAtStart = true,
            isUnlocked = true
        });
    }

    public bool IsGunUnlocked(RevolverData gunData)
    {
        if (gunData == null)
            return false;

        for (int i = 0; i < gunEntries.Count; i++)
        {
            if (gunEntries[i].gunData == gunData)
                return gunEntries[i].isUnlocked;
        }

        return false;
    }

    private void InitializeUnlockState()
    {
        for (int i = 0; i < gunEntries.Count; i++)
            gunEntries[i].isUnlocked = gunEntries[i].unlockedAtStart;
    }

    private bool HasValidSelectedGun()
    {
        if (_selectedIndex < 0 || _selectedIndex >= gunEntries.Count)
            return false;

        GunEntry selectedEntry = gunEntries[_selectedIndex];
        return selectedEntry.isUnlocked && selectedEntry.gunData != null;
    }

    private void SyncSelectionWithCurrentGun()
    {
        if (revolver == null || gunEntries.Count == 0)
            return;

        RevolverData currentData = revolver.CurrentData;
        if (currentData == null)
            return;

        for (int i = 0; i < gunEntries.Count; i++)
        {
            if (gunEntries[i].gunData == currentData)
            {
                _selectedIndex = i;
                return;
            }
        }

        for (int i = 0; i < gunEntries.Count; i++)
        {
            if (gunEntries[i].isUnlocked && gunEntries[i].gunData != null)
            {
                _selectedIndex = i;
                return;
            }
        }

        _selectedIndex = 0;
    }
}
