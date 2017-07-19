using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LetterTile : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Text letterText;
    [SerializeField]
    private Color backgroundNormalColor;
    [SerializeField]
    private Color backgroundSelectedColor;
    [SerializeField]
    private Color letterNormalColor;
    [SerializeField]
    private Color letterSelectedColor;
    [SerializeField]
    private Sprite normalSprite;
    [SerializeField]
    private Sprite selectedSprite;

    public int x;
    public int y;
    public bool FakeChecked;
    private Color _red = new Color32(255, 111, 111, 255);

    #endregion

    #region Properties

    public Text LetterText { get { return letterText; } }
    public int TileIndex { get; set; }
    public bool Selected { get; set; }
    public bool Found { get; set; }
    public char Letter { get; set; }
    

    #endregion

    #region Public Methods

    public void SetSelected(bool selected)
    {
        Selected = selected;

        backgroundImage.sprite = selected ? selectedSprite : normalSprite;
        backgroundImage.color = selected ? backgroundSelectedColor : backgroundNormalColor;
        letterText.color = selected ? letterSelectedColor : letterNormalColor;
    }
    #endregion

    readonly float duration = 0.5f; // This will be your time in seconds.
    readonly float smoothness = 0.04f; // This will determine the smoothness of the lerp. Smaller values are smoother. Really it's the time between updates.

    public void SetImpossible()
    {
        StartCoroutine(SetRed());
    }
    
    IEnumerator SetRed()
    {
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness / duration; //The amount of change to apply.
        while (progress < 1)
        {
            backgroundImage.color = Color.Lerp(backgroundNormalColor, _red, progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }
        
        yield break;
    }
}