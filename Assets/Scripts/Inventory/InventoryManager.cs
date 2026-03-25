using System;
using System.Collections.Generic;
using UnityEngine;


/// In-game inventory that can be toggled with a key.
/// Lets the player customize which bullet prefab they are actively using.

public class InventoryManager : MonoBehaviour
{
    [Serializable]
    private class BulletOption
    {
        public string label = "Bullet";
        public GameObject bulletPrefab;
    }

    public static bool IsInventoryOpen { get; private set; }

    // Shown in the inspector
    [Header("Inventory Toggle")]
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;

    [Header("References")]
    [Tooltip("If empty, InventoryManager will try to find a Revolver in the scene.")]
    [SerializeField] private Revolver revolver;

    [Header("Bullet Options")]
    [Tooltip("Configure all bullet prefabs the player can swap to.")]
    [SerializeField] private List<BulletOption> bulletOptions = new List<BulletOption>();

    [Header("Window")]
    [SerializeField] private Rect windowRect = new Rect(24f, 24f, 360f, 300f);

    private int _selectedIndex;

    private void Awake()
    {
        if (revolver == null)
            revolver = FindFirstObjectByType<Revolver>();

        // Default selection to whichever bullet the revolver is currently using.
        SyncSelectionWithCurrentBullet();
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();
    }

    private void OnDisable()
    {
        // Ensure game speed is restored if this component is disabled.
        if (IsInventoryOpen)
            SetInventoryOpen(false);
    }

    private void OnGUI()
    {
        if (!IsInventoryOpen)
            return;

        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawInventoryWindow, "Inventory");
    }

    private void DrawInventoryWindow(int windowId)
    {
        GUILayout.Space(8f);

        // If no revolver in the scene, show a message and a close button
        if (revolver == null)
        {
            GUILayout.Label("No Revolver found in scene.");
            if (GUILayout.Button("Close"))
                SetInventoryOpen(false);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
            return;
        }

        // If theres no bullet options, show a message and a close button
        if (bulletOptions.Count == 0)
        {
            GUILayout.Label("No bullet options configured.");
            if (GUILayout.Button("Close"))
                SetInventoryOpen(false);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
            return;
        }

        GUILayout.Label("Select Bullet Type:");

        for (int i = 0; i < bulletOptions.Count; i++)
        {
            BulletOption option = bulletOptions[i];
            string label = string.IsNullOrWhiteSpace(option.label)
                ? $"Bullet {i + 1}"
                : option.label;

            bool selected = i == _selectedIndex;
            bool clicked = GUILayout.Toggle(selected, label, "Button", GUILayout.Height(28f));
            if (clicked)
                _selectedIndex = i;
        }

        GUILayout.Space(10f);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply", GUILayout.Height(30f)))
                ApplySelectedBullet();

            if (GUILayout.Button("Close", GUILayout.Height(30f)))
                SetInventoryOpen(false);
        }

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private void ToggleInventory()
    {
        bool open = !IsInventoryOpen;
        if (open)
            SyncSelectionWithCurrentBullet();

        SetInventoryOpen(open);
    }

    private void SetInventoryOpen(bool open)
    {
        IsInventoryOpen = open;
        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = open;
    }

    // Applies the currently selected bullet option to the revolver and closes the inventory
    private void ApplySelectedBullet()
    {
        if (revolver == null)
            return;

        if (_selectedIndex < 0 || _selectedIndex >= bulletOptions.Count)
            return;

        GameObject selectedPrefab = bulletOptions[_selectedIndex].bulletPrefab;
        if (selectedPrefab == null)
        {
            Debug.LogWarning("[InventoryManager] Selected bullet option has no prefab assigned.");
            return;
        }

        revolver.SetBulletPrefab(selectedPrefab);
        SetInventoryOpen(false);
    }

    // Syncs the currently selected inventory 
    // option with the bullet prefab the revolver is currently using
    private void SyncSelectionWithCurrentBullet()
    {
        if (revolver == null || bulletOptions.Count == 0)
            return;

        GameObject currentPrefab = revolver.CurrentBulletPrefab;
        if (currentPrefab == null)
            return;

        for (int i = 0; i < bulletOptions.Count; i++)
        {
            if (bulletOptions[i].bulletPrefab == currentPrefab)
            {
                _selectedIndex = i;
                return;
            }
        }
    }
}
