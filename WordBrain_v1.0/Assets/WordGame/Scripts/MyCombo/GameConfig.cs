using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public int rewardedVideoPeriod;
    public int rewardedVideoAmount;
    
    public int GetInterstitialCallbackAmount()
    {
        if (GetPennyToss() == 0)
            return 1;
        return 2;
    }

    public static int GetPennyToss()
    {
        var randomNumber = Random.Range(0, 100);
        return (randomNumber > 50) ? 1 : 0;
    }

    public int adPeriod;
    public int completeDailyPuzzleAward;
    public int completeNormalLevelAward;

    public static GameConfig instance;
    private void Awake()
    {
        instance = this;
    }
}
