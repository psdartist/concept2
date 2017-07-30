using UnityEngine;

public class OnSegmentCreated : MonoBehaviour
{
    void OnEnable()
    {
        if (SoundManager.Instance() != null)
            SoundManager.Instance().PlayLetterSound();
    }
}