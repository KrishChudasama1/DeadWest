using UnityEngine;

public class HubEnemySpawner : MonoBehaviour
{
    public GameObject ghostPrefab; 
    public Transform[] spawnPoints;
    
    public float baseSpawnRate = 5f;
    private float spawnTimer = 0f;

    void Update()
    {
        // Do not spawn enemies if the player has finished the clue hunt
        if (PlayerPrefs.GetInt("HubProgress", 0) > 0) return;

        spawnTimer += Time.deltaTime;

        // Dynamic Difficulty: faster spawns based on clues found
        int cluesFound = StreetLevelManager.Instance.GetCluesFound();
        float currentSpawnRate = Mathf.Max(1f, baseSpawnRate - cluesFound); 

        if (spawnTimer >= currentSpawnRate)
        {
            SpawnGhost();
            spawnTimer = 0f;
        }
    }

    void SpawnGhost()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(ghostPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
    }
}