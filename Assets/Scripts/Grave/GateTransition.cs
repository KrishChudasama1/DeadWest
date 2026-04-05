using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTransition : MonoBehaviour
{
    public string sceneName = "MainScene";
    public string spawnPointName = "GraveyardReturnPoint";

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Gate touched by: " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            PlayerSpawnManager.spawnPointName = spawnPointName;
            SceneManager.LoadScene(sceneName);
        }
    }
}