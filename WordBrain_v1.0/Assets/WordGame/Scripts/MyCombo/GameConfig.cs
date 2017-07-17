using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public int rewardedVideoPeriod;
    public int rewardedVideoAmount;

    public int adPeriod;
    public int completeDailyPuzzleAward;
    public int completeNormalLevelAward;

    public static GameConfig instance;
    private void Awake()
    {
        instance = this;
    }
}
