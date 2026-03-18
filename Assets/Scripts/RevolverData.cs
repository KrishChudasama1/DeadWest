using UnityEngine;

/// <summary>
/// RevolverData.cs  —  ScriptableObject
///
/// A data asset that defines the stats for one specific revolver.
/// Create as many of these as you have guns in the game.
///
/// To create a new gun asset:
///   Right-click in the Project panel
///   → Create → Weapons → Revolver Data
///   Then fill in the stats in the Inspector.
///
/// Examples:
///   BasicRevolver.asset   — slow, 6 shots, weak
///   MagnumRevolver.asset  — slow fire rate, 3 shots, high damage
///   RapidRevolver.asset   — fast fire rate, 8 shots, low damage
/// </summary>
[CreateAssetMenu(fileName = "BasicRevolver", menuName = "Weapons/Revolver Data")]
public class RevolverData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Basic Revolver";

    [Header("Stats")]
    [Tooltip("Seconds between each shot. Lower = faster.")]
    public float fireRate    = 0.5f;

    [Tooltip("Bullets per full chamber.")]
    public int   chamberSize = 6;

    [Tooltip("Seconds to fully reload.")]
    public float reloadTime  = 3f;

    [Tooltip("Damage dealt per bullet.")]
    public int   damage      = 10;

    [Header("Bullet")]
    [Tooltip("Swap to change bullet appearance for this gun.")]
    public GameObject bulletPrefab;
}