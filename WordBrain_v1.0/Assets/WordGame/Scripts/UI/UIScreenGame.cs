using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIScreenGame : UIScreen
{
	#region Inspector Variables

	[SerializeField] private Text 			categoryText;
	[SerializeField] private Text 			levelText;
	[SerializeField] private Image			iconImage;
	[SerializeField] private Text 			hintBtnText;
	[SerializeField] private Text 			selectedWordText;
	[SerializeField] private LetterBoard	letterBoard;
	
	#endregion

	#region Unity Methods

	private void Update()
	{
		hintBtnText.text = string.Format("INDICIU ({0})", GameManager.Instance.CurrentHints);
	}

	#endregion

	#region Public Methods

	public override void Initialize()
	{
		selectedWordText.text = "";

		letterBoard.OnSelectedWordChanged += (string word) => 
		{
			selectedWordText.text = word;
		};
	}

	public override void OnShowing(object data)
	{
		CategoryInfo categoryInfo = GameManager.Instance.GetCategoryInfo(GameManager.Instance.ActiveCategory);

		categoryText.text	= GameManager.Instance.ActiveCategory.ToUpper();
		hintBtnText.text	= string.Format("INDICIU ({0})", GameManager.Instance.CurrentHints);
		iconImage.sprite	= categoryInfo.icon;

		if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
		{
			levelText.text = string.Format("COMPLETEAZA PENTRU A PRIMII {0}" + (GameConfig.instance.completeDailyPuzzleAward == 1 ? "INDICIU" : "INDICII"), GameConfig.instance.completeDailyPuzzleAward);
		}
		else
		{
			levelText.text = string.Format("NIVEL {0}", GameManager.Instance.ActiveLevelIndex + 1);
		}

	}
	
	public override void OnBackClicked()
	{
		if (!GameManager.Instance.AnimatingWord)
		{
			if (GameManager.Instance.ActiveCategory == GameManager.dailyPuzzleId)
			{
				UIScreenController.Instance.Show(UIScreenController.MainScreenId, true);
			}
			else
			{
				UIScreenController.Instance.Show(UIScreenController.CategoryLevelsScreenId, true, true, false, Tween.TweenStyle.EaseOut, null, GameManager.Instance.ActiveCategory);
			}
		}
	}

	#endregion
}
