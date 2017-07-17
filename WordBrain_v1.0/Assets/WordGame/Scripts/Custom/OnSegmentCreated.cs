using UnityEngine;

public class OnSegmentCreated : MonoBehaviour
{
    void OnEnable()
    {
        GameManager.Instance.PlayLetterSound();
    }
}