using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class UIScreen : MonoBehaviour
{
	#region Inspector Variables

	public string			id;
	public List<GameObject>	worldObjects;

	#endregion

	#region Properties

	public RectTransform RectT { get { return gameObject.GetComponent<RectTransform>(); } }

	#endregion

	#region Public Methods

	public virtual void Initialize()
	{

	}

	public virtual void OnShowing(object data)
	{

	}

    public virtual void OnBackClicked()
    {

    }

    #endregion
}
