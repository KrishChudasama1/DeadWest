using UnityEngine;

public class PlayerLasso : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool hasLasso = false;

    public bool HasLasso => hasLasso;

  
    public void UnlockLasso()
    {
        hasLasso = true;
        Debug.Log("[PlayerLasso] Lasso unlocked!");
        // TODO: enable lasso ability UI / input here
    }

    private void Update()
    {
        if (!hasLasso) return;

        // TODO: handle lasso throw input 
    }
}