using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetDragThreshold : MonoBehaviour {
    public Canvas myCanvas;

    private void Start()
    {
        GetComponent<EventSystem>().pixelDragThreshold = (int)(10 * myCanvas.scaleFactor);
    }
}
