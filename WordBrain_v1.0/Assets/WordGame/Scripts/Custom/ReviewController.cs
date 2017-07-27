using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ReviewController : MonoBehaviour
{
    public GameObject UiReview;

    public GameObject QuestionPanel;
    public GameObject DecisionPanel;

    public bool GaveReview;
    public bool RefusedReview;
    public bool SeenReviewScreen;
    public int ShowReviewAtLevels;
    public int CurrentCompletedLevelsNumber;

    public static ReviewController Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Init()
    {
        UiReview.SetActive(false);

        QuestionPanel.SetActive(true);
        DecisionPanel.SetActive(false);
    }

    public void TryStartView(bool force = false)
    {
        bool show;
        if (SeenReviewScreen && force == false)
        {
            if (RefusedReview || GaveReview)
            {
                show = false;
            }
            else
            {
                show = true;
            }
        }
        else
        {
            show = true;
        }

        if (show)
        {
            if (CurrentCompletedLevelsNumber >= ShowReviewAtLevels)
            {
                ShowReviewAtLevels += 7;
                UiReview.SetActive(true);
            }
        }
    }

    // Button Events
    public void OnNuChiar()
    {
        Init();
    }

    public void OnDa()
    {
        QuestionPanel.SetActive(false);
        DecisionPanel.SetActive(true);

        SeenReviewScreen = true;
    }

    public void OnNuMultumesc()
    {
        Init();

        RefusedReview = true;
        GameManager.Instance.ForceSaveGame();
    }

    public void OnNormal()
    {
        Application.OpenURL("market://details?id=com.psdartist.aicuvinte/");

        GaveReview = true;
        GameManager.Instance.ForceSaveGame();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.X))
        {
            TryStartView(true);
        }
    }
}
