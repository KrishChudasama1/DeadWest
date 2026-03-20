using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    [SerializeField] private int xpReward = 25;
    [SerializeField] private int goldReward = 10;

    public int XPReward => xpReward;
    public int GoldReward => goldReward;
}