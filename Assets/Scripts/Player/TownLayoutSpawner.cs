using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns the western town layout at runtime from prefab references.
/// Attach to an empty "TownManager" GameObject in your scene.
/// Assign your building/prop prefabs in the Inspector.
///
/// If you prefer to hand-place everything in the editor, you can skip this
/// script entirely and just use it as a REFERENCE for positions & layout.
/// </summary>
public class TownLayoutSpawner : MonoBehaviour
{
    [Header("Building Prefabs")]
    [SerializeField] private GameObject saloonPrefab;
    [SerializeField] private GameObject sheriffOfficePrefab;
    [SerializeField] private GameObject generalStorePrefab;
    [SerializeField] private GameObject bankPrefab;
    [SerializeField] private GameObject stablePrefab;
    [SerializeField] private GameObject hotelPrefab;
    [SerializeField] private GameObject blacksmithPrefab;

    [Header("Prop Prefabs")]
    [SerializeField] private GameObject waterTowerPrefab;
    [SerializeField] private GameObject horseHitchPrefab;
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private GameObject cactusPrefab;
    [SerializeField] private GameObject tumbleweedPrefab;
    [SerializeField] private GameObject wagonPrefab;

    [Header("Ground / Environment")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject dirtRoadPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float streetWidth = 6f;
    [SerializeField] private float buildingSpacing = 8f;

    private Transform townParent;

    /// <summary>
    /// Predefined building placement data.
    /// Positions assume the main street runs along the X-axis at Y=0.
    /// North side of street = positive Y, South side = negative Y.
    /// </summary>
    private static readonly List<BuildingPlacement> TownLayout = new List<BuildingPlacement>
    {
        // ── NORTH SIDE OF MAIN STREET ──
        new BuildingPlacement("Saloon",         -12f,  5f,  "saloon"),
        new BuildingPlacement("General Store",   -4f,  5f,  "generalStore"),
        new BuildingPlacement("Bank",             4f,  5f,  "bank"),
        new BuildingPlacement("Hotel",           12f,  5f,  "hotel"),

        // ── SOUTH SIDE OF MAIN STREET ──
        new BuildingPlacement("Sheriff Office",  -8f, -5f,  "sheriffOffice"),
        new BuildingPlacement("Blacksmith",       0f, -5f,  "blacksmith"),
        new BuildingPlacement("Stable",           8f, -5f,  "stable"),
    };

    /// <summary>
    /// Predefined prop/decoration positions scattered around town.
    /// </summary>
    private static readonly List<PropPlacement> PropLayout = new List<PropPlacement>
    {
        // Horse hitches along the street
        new PropPlacement("HorseHitch", -12f,  2f,  "horseHitch"),
        new PropPlacement("HorseHitch",  -4f,  2f,  "horseHitch"),
        new PropPlacement("HorseHitch",   8f, -2f,  "horseHitch"),

        // Barrels near the saloon and store
        new PropPlacement("Barrel", -14f,  3.5f, "barrel"),
        new PropPlacement("Barrel", -13f,  3.5f, "barrel"),
        new PropPlacement("Barrel",  -2f,  3.5f, "barrel"),

        // Water tower near the stable
        new PropPlacement("WaterTower",  12f, -3f,  "waterTower"),

        // Wagon parked at the edge of town
        new PropPlacement("Wagon", -18f,  0f, "wagon"),

        // Cacti in the open desert around town
        new PropPlacement("Cactus", -22f,  8f,  "cactus"),
        new PropPlacement("Cactus",  18f, 10f,  "cactus"),
        new PropPlacement("Cactus",  25f, -6f,  "cactus"),
        new PropPlacement("Cactus", -20f, -9f,  "cactus"),
        new PropPlacement("Cactus",  30f,  3f,  "cactus"),
        new PropPlacement("Cactus", -28f, -4f,  "cactus"),
        new PropPlacement("Cactus",  22f, 12f,  "cactus"),
    };

    private void Start()
    {
        townParent = new GameObject("--- Town ---").transform;
        SpawnBuildings();
        SpawnProps();
    }

    private void SpawnBuildings()
    {
        foreach (var b in TownLayout)
        {
            GameObject prefab = GetBuildingPrefab(b.PrefabKey);
            if (prefab == null)
            {
                Debug.LogWarning($"[TownLayout] No prefab assigned for: {b.Name} (key: {b.PrefabKey})");
                continue;
            }

            Vector3 pos = new Vector3(b.X, b.Y, 0f);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, townParent);
            go.name = b.Name;
        }
    }

    private void SpawnProps()
    {
        foreach (var p in PropLayout)
        {
            GameObject prefab = GetPropPrefab(p.PrefabKey);
            if (prefab == null) continue;

            Vector3 pos = new Vector3(p.X, p.Y, 0f);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, townParent);
            go.name = p.Name;
        }
    }

    private GameObject GetBuildingPrefab(string key)
    {
        return key switch
        {
            "saloon"        => saloonPrefab,
            "sheriffOffice"  => sheriffOfficePrefab,
            "generalStore"   => generalStorePrefab,
            "bank"           => bankPrefab,
            "stable"         => stablePrefab,
            "hotel"          => hotelPrefab,
            "blacksmith"     => blacksmithPrefab,
            _ => null
        };
    }

    private GameObject GetPropPrefab(string key)
    {
        return key switch
        {
            "waterTower"  => waterTowerPrefab,
            "horseHitch"  => horseHitchPrefab,
            "barrel"      => barrelPrefab,
            "cactus"      => cactusPrefab,
            "tumbleweed"  => tumbleweedPrefab,
            "wagon"       => wagonPrefab,
            _ => null
        };
    }

    // ── Data Structs ──

    private struct BuildingPlacement
    {
        public string Name;
        public float X, Y;
        public string PrefabKey;
        public BuildingPlacement(string name, float x, float y, string key)
        { Name = name; X = x; Y = y; PrefabKey = key; }
    }

    private struct PropPlacement
    {
        public string Name;
        public float X, Y;
        public string PrefabKey;
        public PropPlacement(string name, float x, float y, string key)
        { Name = name; X = x; Y = y; PrefabKey = key; }
    }

    // Visualize the layout in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw main street
        Gizmos.color = new Color(0.8f, 0.6f, 0.3f, 0.4f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(50f, streetWidth, 0.1f));

        // Draw building positions
        Gizmos.color = Color.cyan;
        foreach (var b in TownLayout)
            Gizmos.DrawWireCube(new Vector3(b.X, b.Y, 0), new Vector3(6f, 4f, 0.1f));

        // Draw prop positions
        Gizmos.color = Color.green;
        foreach (var p in PropLayout)
            Gizmos.DrawWireSphere(new Vector3(p.X, p.Y, 0), 0.5f);
    }
}
