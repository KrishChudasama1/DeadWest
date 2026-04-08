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
        public bool purchasable = true;
        public int goldCost = 0;
        public int requiredLevel = 0;

        [NonSerialized] public bool isUnlocked;
    }

    public static bool IsInventoryOpen { get; private set; }

    public static InventoryManager Instance { get; private set; }

   
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
    [SerializeField] private Rect shopWindowRect = new Rect(410f, 24f, 360f, 500f);

    private int _selectedIndex;
    private bool _isShopOpen;
    private XPManager _xpManager;

    [Header("Items")]
    [SerializeField] private bool hasLasso = false;
    [Tooltip("Assign the LassoProjectile prefab here so it can be given to LassoThrow at runtime.")]
    [SerializeField] private GameObject lassoProjectilePrefab;
    private bool _lassoEquipped = false;

    private void Awake()
    { 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (revolver == null)
            revolver = FindFirstObjectByType<Revolver>();

        _xpManager = FindFirstObjectByType<XPManager>();

        InitializeUnlockState();

       
        SyncSelectionWithCurrentGun();
    }

    private void HandleLassoPickedUp()
    {
        hasLasso = true;
        Debug.Log("[InventoryManager] Lasso added to inventory.");
    }

    private void OnEnable()
    {
        LassoPickup.OnLassoPickedUp += HandleLassoPickedUp;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        LassoPickup.OnLassoPickedUp -= HandleLassoPickedUp;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        if (IsInventoryOpen)
            SetInventoryOpen(false);

        _isShopOpen = false;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        revolver = FindFirstObjectByType<Revolver>();
        _xpManager = FindFirstObjectByType<XPManager>();


        _lassoEquipped = false;

        SyncSelectionWithCurrentGun();
        Debug.Log($"[InventoryManager] Scene '{scene.name}' loaded — re-linked references. hasLasso={hasLasso}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();
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

        // Items section
        GUILayout.Label("Items:");
        if (hasLasso)
        {
            using (new GUILayout.HorizontalScope())
            {
                string lassoLabel = _lassoEquipped ? "Lasso  [EQUIPPED]" : "Lasso";
                GUILayout.Label(lassoLabel, GUILayout.Width(200f));

                string btnText = _lassoEquipped ? "Unequip" : "Equip";
                if (GUILayout.Button(btnText, GUILayout.Height(24f)))
                {
                    ToggleLassoEquip();
                }
            }
        }
        else
        {
            GUILayout.Label("(no items)");
        }

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
        int currentGold = GetCurrentGold();
        int currentLevel = GetCurrentLevel();

        GUILayout.Label($"Gold: {currentGold}");
        GUILayout.Label($"Level: {currentLevel}");
        GUILayout.Space(8f);

        if (gunEntries.Count == 0)
        {
            GUILayout.Label("No guns configured for shop.");
        }
        else
        {
            for (int i = 0; i < gunEntries.Count; i++)
            {
                GunEntry entry = gunEntries[i];

                if (entry.gunData == null)
                {
                    GUILayout.Label($"Gun {i + 1}: Missing data");
                    continue;
                }

                string gunName = entry.gunData.weaponName;
                if (entry.isUnlocked)
                {
                    GUILayout.Label($"{gunName} - Owned");
                    continue;
                }

                if (!entry.purchasable)
                {
                    GUILayout.Label($"{gunName}");
                    continue;
                }

                bool meetsLevel = currentLevel >= entry.requiredLevel;
                bool canAfford = currentGold >= entry.goldCost;
                string buyLabel = $"Buy {gunName} ({entry.goldCost}g, Lv {entry.requiredLevel})";

                GUI.enabled = meetsLevel && canAfford;
                if (GUILayout.Button(buyLabel, GUILayout.Height(28f)))
                    TryPurchaseGun(i);
                GUI.enabled = true;

                if (!meetsLevel)
                    GUILayout.Label("  Requires higher level");
                else if (!canAfford)
                    GUILayout.Label("  Not enough gold");
            }
        }

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

        // Unequip lasso when a gun is equipped
        if (_lassoEquipped)
        {
            _lassoEquipped = false;
            var lassoThrow = StableLevel.LassoThrow.Instance;
            if (lassoThrow != null) lassoThrow.Unequip();
        }

        SetInventoryOpen(false);
    }

    private void ToggleLassoEquip()
    {
        _lassoEquipped = !_lassoEquipped;

        // Ensure LassoThrow exists on the Player (adds it at runtime if missing)
        var lassoThrow = StableLevel.LassoThrow.Instance;
        if (lassoThrow == null)
            lassoThrow = StableLevel.LassoThrow.EnsureOnPlayer();

        if (lassoThrow == null)
        {
            Debug.LogWarning("[InventoryManager] Could not find or create LassoThrow on player.");
            _lassoEquipped = false;
            return;
        }

        // Make sure it has the projectile prefab
        if (lassoThrow.lassoProjectilePrefab == null && lassoProjectilePrefab != null)
            lassoThrow.lassoProjectilePrefab = lassoProjectilePrefab;

        if (_lassoEquipped)
        {
            lassoThrow.Equip();
            Debug.Log("[InventoryManager] Lasso equipped — press L to throw.");
        }
        else
        {
            lassoThrow.Unequip();
            Debug.Log("[InventoryManager] Lasso unequipped.");
        }

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
    }

    private void TryPurchaseGun(int index)
    {
        if (index < 0 || index >= gunEntries.Count)
            return;

        GunEntry entry = gunEntries[index];
        if (entry.gunData == null || entry.isUnlocked || !entry.purchasable)
            return;

        int currentLevel = GetCurrentLevel();
        if (currentLevel < entry.requiredLevel)
        {
            Debug.Log("[InventoryManager] Level too low to purchase this gun.");
            return;
        }

        CoinManager coinManager = CoinManager.instance;
        if (coinManager == null)
        {
            Debug.LogWarning("[InventoryManager] CoinManager instance not found.");
            return;
        }

        if (!coinManager.SpendCoins(entry.goldCost))
        {
            Debug.Log("[InventoryManager] Not enough gold to purchase this gun.");
            return;
        }

        entry.isUnlocked = true;

        if (!HasValidSelectedGun())
            _selectedIndex = index;

        Debug.Log($"[InventoryManager] Purchased and unlocked {entry.gunData.weaponName}.");
    }

    private int GetCurrentGold()
    {
        CoinManager coinManager = CoinManager.instance;
        return coinManager != null ? coinManager.coins : 0;
    }

    private int GetCurrentLevel()
    {
        if (_xpManager == null)
            _xpManager = FindFirstObjectByType<XPManager>();

        return _xpManager != null ? _xpManager.level : 0;
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
