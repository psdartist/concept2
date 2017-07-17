using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardedButton : MonoBehaviour
{
    public Button button;
    public GameObject lightObj;

    private const string ACTION_NAME = "rewarded_video";

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (AdmobController.instance.rewardBasedVideo != null)
        {
            AdmobController.instance.rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        }
#else
        SetActive(false);
#endif

        InvokeRepeating("IUpdate", 0, 1);
        button.onClick.AddListener(OnClick);
    }

    private void IUpdate()
    {
        SetActive(IsAvailableToShow());
    }

    private void SetActive(bool isActive)
    {
        button.interactable = isActive;
        lightObj.SetActive(isActive);
    }

    public void OnClick()
    {
        AdmobController.instance.ShowRewardBasedVideo();

        GoogleAnalyticsV3.instance.LogEvent("Rewarded Video", "On Click", "On Click", 0);
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        SetActive(false);
    }

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    public bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable(ACTION_NAME, GameConfig.instance.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        if (AdmobController.instance.rewardBasedVideo == null) return false;
        bool isLoaded = AdmobController.instance.rewardBasedVideo.IsLoaded();
        return isLoaded;
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (AdmobController.instance.rewardBasedVideo != null)
        {
            AdmobController.instance.rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        }
#endif
    }
}
