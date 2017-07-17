using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScreenCompleteOverlay : UIScreen
{
	#region Inspector Variables

	[SerializeField] private Image		categoryIconImage;
	[SerializeField] private Text		categoryNameText;
	[SerializeField] private Text		categoryLevelText;
	[SerializeField] private Text	    plusHintText;

	#endregion

	#region Member Variables
	#endregion

	#region Properties
	#endregion

	#region Unity Methods
	#endregion

	#region Public Methods

	public override void OnShowing(object data)
	{
		CategoryInfo categoryInfo = GameManager.Instance.GetCategoryInfo(GameManager.Instance.ActiveCategory);

		categoryIconImage.sprite	= categoryInfo.icon;
		categoryNameText.text		= GameManager.Instance.ActiveCategory;

		if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
		{
			categoryLevelText.gameObject.SetActive(false);
		}
		else
		{
			categoryLevelText.gameObject.SetActive(true);
			categoryLevelText.text = "Nivel " + (GameManager.Instance.ActiveLevelIndex + 1).ToString();
		}

        int number = (int)data;
        plusHintText.gameObject.SetActive(number > 0);
        plusHintText.text = "+ " + number + (number == 1 ? " Indiciu" : " Indicii");
	}

	#endregion

	#region Protected Methods
	#endregion

	#region Private Methods
	#endregion
}
