/// <summary>
/// IWeapon.cs
/// Interface all weapons must implement.
/// </summary>
public interface IWeapon
{
    string WeaponName  { get; }
    bool   IsOnCooldown { get; }
    void   Shoot(UnityEngine.Vector2 shootDirection);
}