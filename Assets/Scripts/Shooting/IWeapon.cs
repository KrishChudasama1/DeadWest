/// <summary>
/// IWeapon.cs
/// Interface that all weapons must implement.
/// Designed for extensibility — future weapons (shotgun, rifle, etc.)
/// just implement this contract.
/// </summary>
public interface IWeapon
{
    /// <summary>Human-readable name shown in HUD.</summary>
    string WeaponName { get; }

    /// <summary>
    /// Called when the player requests a shot.
    /// Implementations handle animation, projectile spawning, ammo, fire rate, etc.
    /// </summary>
    /// <param name="shootDirection">Normalised world-space direction to fire.</param>
    void Shoot(UnityEngine.Vector2 shootDirection);

    /// <summary>
    /// Returns true while the weapon cannot fire
    /// (e.g. mid-animation, empty, reloading).
    /// </summary>
    bool IsOnCooldown { get; }
}