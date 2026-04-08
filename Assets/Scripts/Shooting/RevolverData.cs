using UnityEngine;

[CreateAssetMenu(fileName = "NewRevolver", menuName = "Weapons/Revolver Data")]
public class RevolverData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Revolver";

    [Header("Stats")]
    [Tooltip("Seconds between each shot. Lower = faster.")]
    public float fireRate    = 0.35f;

    [Tooltip("Bullets per full chamber.")]
    public int   chamberSize = 6;

    [Tooltip("Seconds to fully reload.")]
    public float reloadTime  = 2f;

    [Tooltip("Damage dealt per bullet.")]
    public int   damage      = 10;

    [Header("Firing Pattern")]
    [Tooltip("Number of bullets fired per trigger pull.")]
    [Min(1)]
    public int shotsPerTriggerPull = 1;

    [Tooltip("Side-to-side spacing between bullets when multiple shots are fired.")]
    public float shotSpread = 0.08f;

    [Header("Bullet")]
    [Tooltip("Swap to change bullet appearance for this gun.")]
    public GameObject bulletPrefab;
}
