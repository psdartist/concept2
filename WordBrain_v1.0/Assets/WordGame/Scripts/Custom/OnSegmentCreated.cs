using UnityEngine;

public class OnSegmentCreated : MonoBehaviour
{
    void OnEnable()
    {
        SoundManager.Instance().PlayLetterSound();
    }
}