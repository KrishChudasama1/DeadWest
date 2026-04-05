using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static string spawnPointName = "";

    void Start()
    {
        if (spawnPointName != "")
        {
            GameObject spawnPoint = GameObject.Find(spawnPointName);
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (spawnPoint != null && player != null)
                player.transform.position = spawnPoint.transform.position;

            spawnPointName = "";
        }
    }
}