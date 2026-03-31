using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")] public GameObject enemyPrefab;
    public int enemiesToSpawn = 6;
    public float timeBetweenSpawns = 2f;
    public float spawnOffset = 1.5f; // how far outside camera edge to spawn

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(i + 1);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    void SpawnEnemy(int enemyNumber)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (side)
        {
            case 0: // top
                spawnPos = new Vector2(
                    Random.Range(camPos.x - camWidth, camPos.x + camWidth),
                    camPos.y + camHeight + spawnOffset
                );
                break;
            case 1: // bottom
                spawnPos = new Vector2(
                    Random.Range(camPos.x - camWidth, camPos.x + camWidth),
                    camPos.y - camHeight - spawnOffset
                );
                break;
            case 2: // left
                spawnPos = new Vector2(
                    camPos.x - camWidth - spawnOffset,
                    Random.Range(camPos.y - camHeight, camPos.y + camHeight)
                );
                break;
            case 3: // right
                spawnPos = new Vector2(
                    camPos.x + camWidth + spawnOffset,
                    Random.Range(camPos.y - camHeight, camPos.y + camHeight)
                );
                break;
        }

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Spawned enemy " + enemyNumber + " of " + enemiesToSpawn);
    }
}