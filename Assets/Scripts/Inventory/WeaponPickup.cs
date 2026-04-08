using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private RevolverData weaponData;

    private bool _pickedUp;

    public void SetWeaponData(RevolverData data)
    {
        weaponData = data;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        PickUp();
    }

    private void PickUp()
    {
        if (_pickedUp)
            return;

        _pickedUp = true;

        if (weaponData == null)
        {
            Debug.LogWarning("WeaponPickup: no RevolverData assigned.");
            Destroy(gameObject);
            return;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("WeaponPickup: InventoryManager instance not found.");
        }
        else
        {
            inventory.UnlockGun(weaponData);
            Debug.Log($"WeaponPickup: unlocked {weaponData.weaponName}.");
        }

        Destroy(gameObject);
    }
}