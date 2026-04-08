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
    }

    private void Update()
    {
        if (!hasLasso) return;

    }
}