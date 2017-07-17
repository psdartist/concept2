
using System;
using UnityEngine;

public class CUtils
{
    #region Double
    public static void SetDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, DoubleToString(value));
    }

    public static double GetDouble(string key, double defaultValue)
    {
        string defaultVal = DoubleToString(defaultValue);
        return StringToDouble(PlayerPrefs.GetString(key, defaultVal));
    }

    public static double GetDouble(string key)
    {
        return GetDouble(key, 0d);
    }

    private static string DoubleToString(double target)
    {
        return target.ToString("R");
    }

    private static double StringToDouble(string target)
    {
        if (string.IsNullOrEmpty(target))
            return 0d;

        return double.Parse(target);
    }
    #endregion


    public static double GetCurrentTime()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalSeconds;
    }

    public static double GetCurrentTimeInDays()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalDays;
    }

    public static double GetCurrentTimeInMills()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalMilliseconds;
    }

    public static T GetRandom<T>(params T[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    public static bool IsActionAvailable(String action, int time, bool availableFirstTime = true)
    {
        if (!PlayerPrefs.HasKey(action + "_time")) // First time.
        {
            if (availableFirstTime == false)
            {
                SetActionTime(action);
            }
            return availableFirstTime;
        }

        int delta = (int)(GetCurrentTime() - GetActionTime(action));
        return delta >= time;
    }

    public static double GetActionDeltaTime(String action)
    {
        if (GetActionTime(action) == 0)
            return 0;
        return GetCurrentTime() - GetActionTime(action);
    }

    public static void SetActionTime(String action)
    {
        SetDouble(action + "_time", GetCurrentTime());
    }

    public static void SetActionTime(String action, double time)
    {
        SetDouble(action + "_time", time);
    }

    public static double GetActionTime(String action)
    {
        return GetDouble(action + "_time");
    }

    public static void ShowInterstitialAd()
    {
        if (IsActionAvailable("show_ads", GameConfig.instance.adPeriod))
        {
#if UNITY_ANDROID || UNITY_IPHONE
            bool result = AdmobController.instance.ShowInterstitial();
            if (result == false) AdmobController.instance.RequestInterstitial();
            if (result) SetActionTime("show_ads");
#else
            if (JobWorker.instance.onShowInterstitial != null)
            {
                JobWorker.instance.onShowInterstitial();
                SetActionTime("show_ads");
            }
#endif
        }
    }
}
