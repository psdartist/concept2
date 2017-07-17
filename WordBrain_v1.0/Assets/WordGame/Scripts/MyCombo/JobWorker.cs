using UnityEngine;
using System;

public class JobWorker : MonoBehaviour
{

    public Action onShowInterstitial;
    public Action onShowRewardedVideo;
    public Action onLink2Store;
    public bool isVungleAdPlayable;

    public static JobWorker instance;

    private void Awake()
    {
        instance = this;
    }
}
