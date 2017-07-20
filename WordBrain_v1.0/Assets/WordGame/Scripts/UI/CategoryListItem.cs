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

    [SerializeField] private Image blockedImage;
    [SerializeField] private Image lockedImage;

    private bool _isLocked;

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

	    blockedImage.enabled = lockedImage.enabled = false;

        if (completedImage.isActiveAndEnabled)
	        return true;
	    return false;
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

    #endregion
}
