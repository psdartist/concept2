using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CategoryListItem : MonoBehaviour
{
	#region Inspector Variables

	[SerializeField] private Text	categoryText;
	[SerializeField] private Text	infoText;
	[SerializeField] private Image	iconImage;
	[SerializeField] private Image	completedImage;

    [SerializeField] private RectTransform progressPanel;
    [SerializeField] private Text progressNumber;
    [SerializeField] private Image blockedImage;
    [SerializeField] private Image lockedImage;

    private bool _setupIsComplete;
    private bool _isLocked;
    private float _percentCompleted;

    #endregion

    #region Member Variables

    private string categoryName;

	#endregion

	#region Public Methods

	public bool Setup(CategoryInfo categoryInfo)
	{
		this.categoryName = categoryInfo.name;
	    
		float numberOfLevels			= categoryInfo.levelInfos.Count;
		float numberOfCompletedLevels	= GameManager.Instance.GetCompletedLevelCount(categoryInfo);

		categoryText.text	= categoryInfo.name.ToUpper();
		infoText.text		= string.Format("NIVELE: {0}/{1}", numberOfCompletedLevels, numberOfLevels);
		iconImage.sprite	= categoryInfo.icon;

		completedImage.enabled = (numberOfLevels == numberOfCompletedLevels);

	    _percentCompleted = GetValuePercent(numberOfCompletedLevels, numberOfLevels);
        
        blockedImage.enabled = lockedImage.enabled = false;
	    progressPanel.gameObject.SetActive(false);

	    _setupIsComplete = true;
        
	    return completedImage.enabled;
	}

	public void OnClick()
	{
	    if (_isLocked)
	        return;

		// Show the category levels screen
		UIScreenController.Instance.Show(UIScreenController.CategoryLevelsScreenId, false, true, false, Tween.TweenStyle.EaseOut, null, categoryName);
	}

    public void SetLocked(bool val)
    {
        this._isLocked = val;
        blockedImage.enabled = lockedImage.enabled = val;
        //Debug.Log(categoryText.text + " : locked ? =  " + _isLocked);
    }

    void OnEnable()
    {
        if (_percentCompleted > 2)
            StartCoroutine(DisplayProgress());
    }

    readonly float duration = 3f; // This will be your time in seconds.
    readonly float smoothness = 0.02f; // This will determine the smoothness of the lerp. Smaller values are smoother. Really it's the time between updates.

    private Vector2 _desiredWidth;

    IEnumerator DisplayProgress()
    {
        yield return new WaitForSeconds(0.01f);



        progressPanel.gameObject.SetActive(true);

        if (_percentCompleted > 23)
            progressNumber.text = ((int)_percentCompleted).ToString() + "%    ";
        else
            progressNumber.enabled = false;

        var parentSize = progressPanel.transform.parent.GetComponent<RectTransform>().sizeDelta;

        progressPanel.sizeDelta = new Vector2(0, parentSize.y);
        _desiredWidth = new Vector2(GetPercent(parentSize.x, _percentCompleted), parentSize.y);
        progressPanel.localPosition = new Vector2((-parentSize.x / 2), 0);

        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness / duration; //The amount of change to apply.
        while (progress < 1)
        {
            progressPanel.sizeDelta = Vector2.Lerp(progressPanel.sizeDelta, _desiredWidth, progress);
            progress += increment;
            yield return new WaitForSeconds(smoothness);
        }

        yield break;
    }

    private float GetValuePercent(float value, float maxValue)
    {
        return (value * 100f) / maxValue;
    }

    private float GetPercent(float value, float percent)
    {
        return (value / 100f) * percent;
    }

    #endregion
}
