using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
    public Image Splash;

    public Sprite CompanySplash;
    public Sprite GameSplash;

    readonly float duration = 0.5f; // This will be your time in seconds.
    readonly float smoothness = 0.04f; // This will determine the smoothness of the lerp. Smaller values are smoother. Really it's the time between updates.

    void Start()
    {
        StartCoroutine(ShowCompanySplash());
    }

    IEnumerator ShowCompanySplash()
    {
        Splash.color = Color.white;
        Splash.sprite = CompanySplash;

        SoundManager.Instance().Play("Throw_Knife");

        //---------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        yield return new WaitForSeconds(2f);
        //---------------------------------------------------------------------------------------------------------------------------------

        Splash.sprite = GameSplash;

        //---------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        yield return new WaitForSeconds(1f);
        //---------------------------------------------------------------------------------------------------------------------------------

        StartCoroutine(FadeBlackIn());
        
        //---------------------------------------------------------------------------------------------------------------------------------
        yield return new WaitForSeconds(1f);
        //---------------------------------------------------------------------------------------------------------------------------------

        StartCoroutine(FadeBlackOut());
    }

    IEnumerator FadeBlackIn()
    {
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness / duration; //The amount of change to apply.
        while (progress < 1)
        {
            Splash.color = Color.Lerp(Color.white, Color.black, progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }

        yield break;
    }

    IEnumerator FadeBlackOut()
    {
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness / duration; //The amount of change to apply.
        while (progress < 1)
        {
            Splash.color = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }

        Destroy(Splash);

        yield break;
    }
}