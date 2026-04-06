using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")] public GameObject enemyPrefab;
    public int enemiesToSpawn = 6;
    public float timeBetweenSpawns = 2f;
    public float spawnOffset = 1.5f;

    [Header("Trigger")]
    [SerializeField] private NPCDialogue dialogueTrigger;

    private Camera cam;
    private bool hasStartedSpawning;

    void Start()
    {
        cam = Camera.main;

        if (dialogueTrigger == null)
            dialogueTrigger = FindFirstObjectByType<NPCDialogue>();

        if (dialogueTrigger != null)
        {
            if (dialogueTrigger.HasDialogueCompleted)
                BeginSpawning();
            else
                dialogueTrigger.DialogueFinished += BeginSpawning;
        }
        else
        {
            Debug.LogWarning("EnemySpawner could not find NPCDialogue. Spawning immediately.");
            BeginSpawning();
        }
    }

    void OnDestroy()
    {
        if (dialogueTrigger != null)
            dialogueTrigger.DialogueFinished -= BeginSpawning;
    }

    void BeginSpawning()
    {
        if (hasStartedSpawning)
            return;

        hasStartedSpawning = true;

        if (dialogueTrigger != null)
            dialogueTrigger.DialogueFinished -= BeginSpawning;

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
            case 0:
                spawnPos = new Vector2(
                    Random.Range(camPos.x - camWidth, camPos.x + camWidth),
                    camPos.y + camHeight + spawnOffset
                );
                break;
            case 1:
                spawnPos = new Vector2(
                    Random.Range(camPos.x - camWidth, camPos.x + camWidth),
                    camPos.y - camHeight - spawnOffset
                );
                break;
            case 2:
                spawnPos = new Vector2(
                    camPos.x - camWidth - spawnOffset,
                    Random.Range(camPos.y - camHeight, camPos.y + camHeight)
                );
                break;
            case 3:
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
