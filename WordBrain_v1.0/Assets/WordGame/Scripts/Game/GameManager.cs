using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;

/// <summary>
/// Holds information about each category in the game
/// </summary>
[System.Serializable]
public class CategoryInfo
{
    public string name;         // Name of the category, should be unique
    public string description;  // Short description, can be anything
    public Sprite icon;         // An icon that goes with the category
    public List<LevelInfo> levelInfos;		// The list of levels in this category
}

/// <summary>
/// Holds information about each level in the game.
/// </summary>
[System.Serializable]
public class LevelInfo
{
    public string[] words = null;
}

public class GameManager : SingletonComponent<GameManager>
{
    #region Data Classes

    /// <summary>
    /// Holds infomation about the state of a board that is being play.
    /// </summary>
    public class BoardState
    {
        public enum TileState
        {
            NotUsed,
            Found,
            UsedButNotFound
        }

        public string wordBoardId;      // Unique id for this board
        public int wordBoardSize;       // The size of the board
        public string[] words;              // The words in the board
        public bool[] foundWords;           // The words that have been found (index in foundWords corresponds to index in words)
        public TileState[] tileStates;          // The state of each tile on the board
        public char[] tileLetters;      // The letter that goes in each tile on the board
        public int nextHintIndex;       // The index into words that indicates which word will have a letter shown when the hint button it clicked
        public List<int[]> hintLettersShown;    // The letters that have been shown by hints. (List of int[2] where the first element is the word index and the second element is the letter index)
    }

    #endregion

    #region Inspector Variables

    [Tooltip("The number of hints that a player gets when they first start the game.")]
    [SerializeField]
    private int startingHints;

    [Tooltip("The GameObject from the Hierarchy that has the LetterBoard component attached to it.")]
    [SerializeField]
    private LetterBoard letterBoard;

    [Tooltip("The GameObject from the Hierarchy that has the WordGrid component attached to it.")]
    [SerializeField]
    private WordGrid wordGrid;

    [Tooltip("The prefab from the Project folder that has the LetterTile component attached to it.")]
    [SerializeField]
    private LetterTile letterTilePrefab;

    [Tooltip("All the categories that are in the game. Levels are assigned to categories.")]
    [SerializeField]
    private List<CategoryInfo> categoryInfos;

    [Tooltip("A list of levels which will be randomly choosen from when a daily puzzle starts.")]
    [SerializeField]
    private List<LevelInfo> dailyPuzzles;

    [Tooltip("The sprite that appears in the top bar when a daily puzzle is being played.")]
    [SerializeField]
    private Sprite dailyPuzzleIcon;

    public RewardedButton rewardedButton;

    #endregion

    #region Member Variables

    public static string dailyPuzzleId = "Daily Puzzle";

    private CategoryInfo dailyPuzzleInfo;

    #endregion

    #region Properties

    public static string SaveDataPath { get { return Application.persistentDataPath + "/save.dat"; } }
    public ObjectPool LetterTilePool { get; private set; }
    public int CurrentHints { get; private set; }
    public string ActiveCategory { get; private set; }
    public int ActiveLevelIndex { get; private set; }
    public int ActiveDailyPuzzleIndex { get; private set; }
    public BoardState ActiveBoardState { get; private set; }
    public Dictionary<string, BoardState> SavedBoardStates { get; private set; }
    public Dictionary<string, bool> CompletedLevels { get; private set; }
    public bool AnimatingWord { get; private set; }
    public System.DateTime NextDailyPuzzleAt { get; private set; }

    public List<CategoryInfo> CategoryInfos
    {
        get
        {
            // If the list of Category Infos doesn't already contain a daily puzzle info then add it
            if (!categoryInfos.Contains(DailyPuzzleInfo))
            {
                categoryInfos.Add(DailyPuzzleInfo);
            }

            return categoryInfos;
        }
    }

    public CategoryInfo DailyPuzzleInfo
    {
        get
        {
            // Create a new CategoryInfo for the daily puzzles
            if (dailyPuzzleInfo == null)
            {
                dailyPuzzleInfo = new CategoryInfo();
                dailyPuzzleInfo.name = dailyPuzzleId;
                dailyPuzzleInfo.icon = dailyPuzzleIcon;
                dailyPuzzleInfo.levelInfos = dailyPuzzles;
            }

            return dailyPuzzleInfo;
        }

        set
        {
            dailyPuzzleInfo = value;
        }
    }

    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();

        LetterTilePool = new ObjectPool(letterTilePrefab.gameObject, 16, transform);
        SavedBoardStates = new Dictionary<string, BoardState>();
        CompletedLevels = new Dictionary<string, bool>();

        // Load any save data
        if (!LoadSave())
        {
            // If there as not save data then set the current hints to the starting hints
            CurrentHints = startingHints;
            ActiveDailyPuzzleIndex = -1;
        }

        // Initialize all our important things
        letterBoard.Initialize();
        wordGrid.Initialize();

        // Setup events
        letterBoard.OnWordFound += OnWordFound;
    }

    #endregion

    #region Public Methods

    public void StartLevel(string category, int levelIndex)
    {
        ActiveCategory = category;
        ActiveLevelIndex = levelIndex;

        // Get the board id for the level and load the WordBoard from Resources
        string boardId = Utilities.FormatBoardId(ActiveCategory, ActiveLevelIndex);
        WordBoard wordBoard = Utilities.LoadWordBoard(boardId);

        if (wordBoard == null)
        {
            Debug.LogError("Could not load WordBoard with the boardId: " + boardId);
            return;
        }

        // If a saved BoardState does not already exist then create one
        if (!SavedBoardStates.ContainsKey(boardId))
        {
            SavedBoardStates.Add(boardId, CreateNewBoardState(wordBoard));
        }

        // Try and get a saved board state if one exists
        ActiveBoardState = SavedBoardStates[boardId];

        // Save the game
        Save();

        // Setup the display using the assigned activeBoardState
        SetupActiveBoard();
    }

    /// <summary>
    /// Starts the daily puzzle.
    /// </summary>
    public void StartDailyPuzzle()
    {
        if (dailyPuzzles.Count == 0)
        {
            return;
        }

        // Check if we need to pick a new daily puzzle
        if (ActiveDailyPuzzleIndex == -1 || System.DateTime.Now >= NextDailyPuzzleAt)
        {
            if (ActiveDailyPuzzleIndex != -1)
            {
                string boardId = Utilities.FormatBoardId(dailyPuzzleId, ActiveDailyPuzzleIndex);

                // Remove any save data for the previous daily puzzle
                SavedBoardStates.Remove(boardId);
                CompletedLevels.Remove(boardId);
            }

            // Get a new random daily puzzle level index to use
            ActiveDailyPuzzleIndex = Random.Range(0, dailyPuzzles.Count);
        }

        // Start the daily puzzle
        StartLevel(dailyPuzzleId, ActiveDailyPuzzleIndex);
    }

    /// <summary>
    /// Displays one letter in the WordGrid as a hint.
    /// </summary>
    public void DisplayNextHint()
    {
        if (CurrentHints == 0)
        {
            if (rewardedButton.IsAvailableToShow())
            {
                rewardedButton.OnClick();
            }
            else if (!rewardedButton.IsActionAvailable())
            {
                int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime("rewarded_video"));
                Toast.instance.ShowMessage("Ad is not available now. Please wait " + remainTime + " seconds");
            }
            else
            {
                Toast.instance.ShowMessage("Ad is not available now. Please wait");
            }
        }
        else if (ActiveBoardState != null && CurrentHints > 0)
        {
            // Call DisplayNextHint in wordGrid, giving it the last hint index that was displayed. DisplayNextHint will return the word and letter that was displayed
            int hintWordIndex;
            int hintLetterIndex;
            bool hintDisplayed = wordGrid.DisplayNextHint(ref ActiveBoardState.nextHintIndex, out hintWordIndex, out hintLetterIndex);

            // Check if a hint was actually displayed
            if (hintDisplayed)
            {
                // Decrement the amount of hints
                CurrentHints--;

                // Update the board state so we know what letters where shown because of hints (so if the board is loaded from save we can lpace the hint words)
                ActiveBoardState.hintLettersShown.Add(new int[] { hintWordIndex, hintLetterIndex });

                // Save the game
                Save();
            }
        }
    }

    /// <summary>
    /// Resets the board so all the letters are back on the GameBoard and the WordGrid is only showing the hints.
    /// </summary>
    public void RestartBoard()
    {
        if (ActiveBoardState != null)
        {
            // Set all the words to not found on the BoardState
            for (int i = 0; i < ActiveBoardState.foundWords.Length; i++)
            {
                ActiveBoardState.foundWords[i] = false;
            }

            // Set all Found tile states back to UsedButNotFound
            for (int i = 0; i < ActiveBoardState.tileStates.Length; i++)
            {
                if (ActiveBoardState.tileStates[i] == BoardState.TileState.Found)
                {
                    ActiveBoardState.tileStates[i] = BoardState.TileState.UsedButNotFound;
                }
            }

            // Save the game
            Save();

            SetupActiveBoard();
        }
    }

    /// <summary>
    /// Adds one hint to the current number of hints
    /// </summary>
    public void AddHint(int number = 1)
    {
        CurrentHints += number;
        Save();
    }

    /// <summary>
    /// Returns true if the given category and level is completed
    /// </summary>
    public bool IsLevelCompleted(CategoryInfo categoryInfo, int levelIndex)
    {
        string boardId = Utilities.FormatBoardId(categoryInfo.name, levelIndex);
        return CompletedLevels.ContainsKey(boardId) && CompletedLevels[boardId];
    }

    /// <summary>
    /// Returns the number of completed levels for the given category
    /// </summary>
    public int GetCompletedLevelCount(CategoryInfo categoryInfo)
    {
        int numberOfCompletedLevels = 0;

        for (int i = 0; i < categoryInfo.levelInfos.Count; i++)
        {
            if (IsLevelCompleted(categoryInfo, i))
            {
                numberOfCompletedLevels++;
            }
        }

        return numberOfCompletedLevels;
    }

    /// <summary>
    /// Returns the CategoryInfo with the given category name.
    /// </summary>
    public CategoryInfo GetCategoryInfo(string categoryName)
    {
        for (int i = 0; i < CategoryInfos.Count; i++)
        {
            if (categoryName == CategoryInfos[i].name)
            {
                return CategoryInfos[i];
            }
        }

        return null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Called when the player finds a word
    /// </summary>
    private void OnWordFound(string word, List<LetterTile> letterTile, bool foundAllWords)
    {
        // Set all the tileStates for the found game tiles to BoardState.TileState.Found to indicate the tile has been found
        for (int i = 0; i < letterTile.Count; i++)
        {
            ActiveBoardState.tileStates[letterTile[i].TileIndex] = BoardState.TileState.Found;
        }

        // Set the flag of the word to found
        for (int i = 0; i < ActiveBoardState.words.Length; i++)
        {
            if (word == ActiveBoardState.words[i])
            {
                ActiveBoardState.foundWords[i] = true;
            }
        }

        // Save the game
        Save();

        // Cannot transition screens while a word is animating or bad things happen
        AnimatingWord = true;

        // Trasition the LetterTiles from the board to the word grid
        if (foundAllWords)
        {
            // If we found all the words then when the tile animation is finished call BoardComplete
            wordGrid.FoundWord(word, letterTile, (GameObject obj, object[] objs) =>
            {
                BoardComplete();
                AnimatingWord = false;
            });
        }
        else
        {
            wordGrid.FoundWord(word, letterTile, (GameObject obj, object[] objs) =>
            {
                AnimatingWord = false;
            });
        }

        Timer.Schedule(this, 0.5f, () =>
        {
            CUtils.ShowInterstitialAd();
        });
    }

    /// <summary>
    /// Sets up the GameBoard and WordGrid using the current active BoardState
    /// </summary>
    private void SetupActiveBoard()
    {
        if (ActiveBoardState == null)
        {
            Debug.LogError("[GameManager] No activeBoardState when SetupActiveBoard was called.");

            return;
        }

        // Setup the GameBoard and WordGrid
        letterBoard.Setup(ActiveBoardState);
        wordGrid.Setup(ActiveBoardState);
    }

    /// <summary>
    /// Creates a new BoardState object using the values defined in the given WordBoard
    /// </summary>
    private BoardState CreateNewBoardState(WordBoard wordBoard)
    {
        BoardState boardState = new BoardState();

        boardState.wordBoardId = wordBoard.id;
        boardState.wordBoardSize = wordBoard.size;
        boardState.words = wordBoard.words;
        boardState.nextHintIndex = 0;

        boardState.foundWords = new bool[wordBoard.words.Length];
        boardState.tileLetters = new char[wordBoard.wordTiles.Length];
        boardState.tileStates = new BoardState.TileState[wordBoard.wordTiles.Length];
        boardState.hintLettersShown = new List<int[]>();

        for (int i = 0; i < boardState.tileStates.Length; i++)
        {
            boardState.tileLetters[i] = wordBoard.wordTiles[i].hasLetter ? wordBoard.wordTiles[i].letter : (char)0;
            boardState.tileStates[i] = wordBoard.wordTiles[i].hasLetter ? BoardState.TileState.UsedButNotFound : BoardState.TileState.NotUsed;
        }

        return boardState;
    }

    /// <summary>
    /// Called when the current active board is completed by the player (ie. they found the last word)
    /// </summary>
    private void BoardComplete()
    {
        string boardId = Utilities.FormatBoardId(ActiveCategory, ActiveLevelIndex);
        int awardNumber = 0;

        // Check if the completed category was a daily puzzle, if so check if we want to award a hint
        if (ActiveCategory != dailyPuzzleId)
        {
            bool awardHint = !CompletedLevels.ContainsKey(boardId) || !CompletedLevels[boardId];
            awardNumber = awardHint ? GameConfig.instance.completeNormalLevelAward : 0;
        }
        else
        {
            awardNumber = GameConfig.instance.completeDailyPuzzleAward;
            // Set the next daily puzzle to start at the start of the next day
            NextDailyPuzzleAt = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day).AddDays(1);
        }

        // Award hints for completing the level or daily puzzle
        AddHint(awardNumber);

        // Set the completed flag on the level
        CompletedLevels[boardId] = true;

        // The board has been completed, we no longer need to save it
        ActiveBoardState = null;

        // Remove the BoardState from the list of saved BoardStates
        SavedBoardStates.Remove(boardId);

        Save();

        UIScreenController.Instance.Show(UIScreenController.CompleteScreenId, false, true, true, Tween.TweenStyle.EaseOut, OnCompleteScreenShown, awardNumber);
        GoogleAnalyticsV3.instance.LogEvent("Level", "Complete Level", boardId, 0);
    }

    private void OnCompleteScreenShown()
    {
        CategoryInfo categoryInfo = GetCategoryInfo(ActiveCategory);
        int nextLevelIndex = ActiveLevelIndex + 1;

        // Check if the category has been completed or it was the daily puzzle
        if (ActiveCategory == dailyPuzzleId || nextLevelIndex >= categoryInfo.levelInfos.Count)
        {

            // If we completed the daily puzzle then move back to the main screen else move to the categories screen
            string screenToShow = (ActiveCategory == dailyPuzzleId) ? UIScreenController.MainScreenId : UIScreenController.CategoriesScreenId;

            // Set the active category to nothing
            ActiveCategory = "";
            ActiveLevelIndex = -1;

            Save();

            // Force the category screen to show right away (behind the now showing overlay)
            UIScreenController.Instance.Show(screenToShow, true, false);
        }
        else
        {
            // Start the next level
            StartLevel(ActiveCategory, nextLevelIndex);
        }

        StartCoroutine(WaitThenHideCompleteScreen());
    }

    private IEnumerator WaitThenHideCompleteScreen()
    {
        yield return new WaitForSeconds(1f);

#if UNITY_ADS
		// If Unity Ads is on and the player completed the proper amount of levels then show an ad
		if (enableAds && LevelsToCompleteBeforeAd <= 0)
		{
			LevelsToCompleteBeforeAd = adLevelCompleteAmount;

			UnityEngine.Advertisements.ShowOptions adShowOptions = new UnityEngine.Advertisements.ShowOptions();

			// Show the ad first and when its done then hide the complete overlay
			adShowOptions.resultCallback = (UnityEngine.Advertisements.ShowResult adShowResult) => 
			{
				UIScreenController.Instance.HideOverlay(UIScreenController.CompleteScreenId, true, Tween.TweenStyle.EaseIn);
			};

			UnityEngine.Advertisements.Advertisement.Show(zoneId, adShowOptions);
		}
		else
		{
			UIScreenController.Instance.HideOverlay(UIScreenController.CompleteScreenId, true, Tween.TweenStyle.EaseIn);
		}
#else
        UIScreenController.Instance.HideOverlay(UIScreenController.CompleteScreenId, true, Tween.TweenStyle.EaseIn);
#endif
    }

    /// <summary>
    /// Saves the game
    /// </summary>
    private void Save()
    {
        Dictionary<string, object> jsonObj = new Dictionary<string, object>();

        jsonObj.Add("currentHints", CurrentHints);
        jsonObj.Add("activeCategory", ActiveCategory);
        jsonObj.Add("activeLevelIndex", ActiveLevelIndex);
        jsonObj.Add("ActiveDailyPuzzleIndex", ActiveDailyPuzzleIndex);
        jsonObj.Add("NextDailyPuzzleAt", NextDailyPuzzleAt.ToString("yyyyMMdd"));

        // Get all the saved board states
        List<object> savedBoardStatesObj = new List<object>();

        foreach (KeyValuePair<string, BoardState> pair in SavedBoardStates)
        {
            savedBoardStatesObj.Add(ConvertBoardStateToJson(pair.Value));
        }

        jsonObj.Add("savedBoardStates", savedBoardStatesObj);

        // Get all the completed levels
        List<object> completedLevelsObj = new List<object>();

        foreach (KeyValuePair<string, bool> pair in CompletedLevels)
        {
            if (pair.Value)
            {
                completedLevelsObj.Add(pair.Key);
            }
        }

        jsonObj.Add("completedLevels", completedLevelsObj);

        // Now convert the jsonObj to a json string and save it to the save file
        System.IO.File.WriteAllText(SaveDataPath, Utilities.ConvertToJsonString(jsonObj));
    }

    /// <summary>
    /// Loads the save game
    /// </summary>
    private bool LoadSave()
    {
        if (File.Exists(SaveDataPath))
        {
            string jsonStr = System.IO.File.ReadAllText(SaveDataPath);
            JSONNode json = JSON.Parse(jsonStr);

            // Load the number of current hints
            CurrentHints = json["currentHints"].AsInt;

            // Parse the saved board states
            JSONArray savedBoardStatesJson = json["savedBoardStates"].AsArray;
            SavedBoardStates = new Dictionary<string, BoardState>(savedBoardStatesJson.Count);

            for (int i = 0; i < savedBoardStatesJson.Count; i++)
            {
                BoardState boardState = CreateBoardStateFromJson(savedBoardStatesJson[i]);
                SavedBoardStates.Add(boardState.wordBoardId, boardState);
            }

            // Get the active category and level index
            ActiveCategory = json["activeCategory"].Value;
            ActiveLevelIndex = json["activeLevelIndex"].AsInt;
            ActiveDailyPuzzleIndex = json["ActiveDailyPuzzleIndex"].AsInt;

            // Get the next daily puzzle at time
            string time = json["NextDailyPuzzleAt"].Value;

            if (string.IsNullOrEmpty(time))
            {
                NextDailyPuzzleAt = System.DateTime.Now;
            }
            else
            {
                NextDailyPuzzleAt = System.DateTime.ParseExact(time, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }

            // Parse the completed levels
            JSONArray completedLevelsJson = json["completedLevels"].AsArray;

            for (int i = 0; i < completedLevelsJson.Count; i++)
            {
                CompletedLevels[completedLevelsJson[i].Value] = true;
            }

            return true;
        }

        return false;
    }

    private Dictionary<string, object> ConvertBoardStateToJson(BoardState boardState)
    {
        Dictionary<string, object> jsonObj = new Dictionary<string, object>();

        jsonObj.Add("wordBoardId", boardState.wordBoardId);
        jsonObj.Add("wordBoardSize", boardState.wordBoardSize);
        jsonObj.Add("NextDailyPuzzleAt", boardState.nextHintIndex);

        // Get all the words
        List<object> wordsObj = new List<object>();

        for (int i = 0; i < boardState.words.Length; i++)
        {
            wordsObj.Add(boardState.words[i]);
        }

        jsonObj.Add("words", wordsObj);

        // Get all the found words
        List<object> foundWordsObj = new List<object>();

        for (int i = 0; i < boardState.foundWords.Length; i++)
        {
            foundWordsObj.Add(boardState.foundWords[i]);
        }

        jsonObj.Add("foundWords", foundWordsObj);

        // Get all the tiel states
        List<object> tileStatesObj = new List<object>();

        for (int i = 0; i < boardState.tileStates.Length; i++)
        {
            tileStatesObj.Add((int)boardState.tileStates[i]);
        }

        jsonObj.Add("tileStates", tileStatesObj);

        // Get all the tile letters
        List<object> tileLettersObj = new List<object>();

        for (int i = 0; i < boardState.tileLetters.Length; i++)
        {
            tileLettersObj.Add(boardState.tileLetters[i].ToString());
        }

        jsonObj.Add("tileLetters", tileLettersObj);

        // Get all the hint letters shown
        List<object> hintLettersObj = new List<object>();

        for (int i = 0; i < boardState.hintLettersShown.Count; i++)
        {
            hintLettersObj.Add(string.Format("{0},{1}", boardState.hintLettersShown[i][0], boardState.hintLettersShown[i][1]));
        }

        jsonObj.Add("hintLetters", hintLettersObj);

        return jsonObj;
    }

    private BoardState CreateBoardStateFromJson(JSONNode json)
    {
        BoardState boardState = new BoardState();

        boardState.wordBoardId = json["wordBoardId"].Value;
        boardState.wordBoardSize = json["wordBoardSize"].AsInt;
        boardState.nextHintIndex = json["nextHintIndex"].AsInt;

        // Parse the words
        JSONArray wordsJson = json["words"].AsArray;
        boardState.words = new string[wordsJson.Count];

        for (int i = 0; i < wordsJson.Count; i++)
        {
            boardState.words[i] = wordsJson[i].Value;
        }

        // Parse the found words
        JSONArray foundWordsJson = json["foundWords"].AsArray;
        boardState.foundWords = new bool[foundWordsJson.Count];

        for (int i = 0; i < foundWordsJson.Count; i++)
        {
            boardState.foundWords[i] = foundWordsJson[i].AsBool;
        }

        // Parse the tile states
        JSONArray tileStatesJson = json["tileStates"].AsArray;
        boardState.tileStates = new BoardState.TileState[tileStatesJson.Count];

        for (int i = 0; i < tileStatesJson.Count; i++)
        {
            boardState.tileStates[i] = (BoardState.TileState)tileStatesJson[i].AsInt;
        }

        // Parse the tile lettes
        JSONArray tileLettersJson = json["tileLetters"].AsArray;
        boardState.tileLetters = new char[tileLettersJson.Count];

        for (int i = 0; i < tileLettersJson.Count; i++)
        {
            boardState.tileLetters[i] = System.Convert.ToChar(tileLettersJson[i].Value);
        }

        // Parse the hint letters
        JSONArray hintLettersJson = json["hintLetters"].AsArray;
        boardState.hintLettersShown = new List<int[]>(hintLettersJson.Count);

        for (int i = 0; i < hintLettersJson.Count; i++)
        {
            string[] hintLetter = hintLettersJson[i].Value.Split(',');
            boardState.hintLettersShown.Add(new int[2] { System.Convert.ToInt32(hintLetter[0]), System.Convert.ToInt32(hintLetter[1]) });
        }

        return boardState;
    }

    #endregion

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            CUtils.ShowInterstitialAd();
        }
    }

    private AudioSource _audio;

    private AudioClip _audioClipLetter;
    private AudioClip _audioClipWin;

    public void PlayLetterSound()
    {
        if (_audioClipLetter == null)
        {
            _audioClipLetter = Resources.Load("Sounds/you-wouldnt-believe", typeof(AudioClip)) as AudioClip;
        }

        //if (bothPlaying() == false)
        //CrossFade(_audioClipLetter, 0.7f, 0.01f, 0f);
        if (bothPlaying())
        {
            if (!_isFirst)
            {
                StopCoroutine(_zerothSourceFadeRoutine);

                _source0.PlayOneShot(_audioClipLetter);
                _source0.volume = 0.3f;

            }
            else
            {
                StopCoroutine(_firstSourceFadeRoutine);
                _source1.PlayOneShot(_audioClipLetter);
                _source0.volume = 0.3f;

            }
        }
        else
        {
            CrossFade(_audioClipLetter, 0.7f, 0.01f, 0f);
        }
    }

    public void PlayWinSound()
    {
        if (_audioClipWin == null)
        {
            _audioClipWin = Resources.Load("Sounds/gmae", typeof(AudioClip)) as AudioClip;
        }

        if (_audio == null)
            _audio = transform.gameObject.AddComponent<AudioSource>();
        _audio.PlayOneShot(_audioClipWin);
    }

    AudioSource _source0;
    AudioSource _source1;


    #region internal vars
    bool _isFirst = true; //is _source0 currently the active AudioSource (plays some sound right now)

    Coroutine _zerothSourceFadeRoutine = null;
    Coroutine _firstSourceFadeRoutine = null;
    #endregion


    #region internal functionality

    void Update()
    {
        //constantly check if our game object doesn't contain audio sources which we are referencing.

        //if the _source0 or _source1 contain obsolete references (most likely 'null'), then
        //we will re-init them:
        if (_source0 == null || _source1 == null)
        {

            //re-connect _soruce0 and _source1 to the ones in attachedSources[]
            Component[] attachedSources = gameObject.GetComponents(typeof(AudioSource));
            //For some reason, unity doesn't accept "as AudioSource[]" casting. We would get
            //'null' array instead if we would attempt. Need to re-create a new array:
            AudioSource[] sources = attachedSources.Select(c => c as AudioSource).ToArray();

            InitSources(sources);

            return;
        }

    }


    //re-establishes references to audio sources on this game object:
    void InitSources(AudioSource[] audioSources)
    {

        if (ReferenceEquals(audioSources, null) || audioSources.Length == 0)
        {
            _source0 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            _source1 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            //DefaultTheSource(_source0);
            // DefaultTheSource(_source1);  //remove? we do this in editor only
            return;
        }

        switch (audioSources.Length)
        {
            case 1:
                {
                    _source0 = audioSources[0];
                    _source1 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                    //DefaultTheSource(_source1);  //TODO remove?  we do this in editor only
                }
                break;
            default:
                { //2 and more
                    _source0 = audioSources[0];
                    _source1 = audioSources[1];
                }
                break;
        }//end switch
    }

    #endregion




    //gradually shifts the sound comming from our audio sources to the this clip:
    // maxVolume should be in 0-to-1 range
    public void CrossFade(AudioClip playMe,
        float maxVolume,
        float fadingTime,
        float delay_before_crossFade = 0)
    {

        var fadeRoutine = StartCoroutine(Fade(playMe,
            maxVolume,
            fadingTime,
            delay_before_crossFade));
    }//end CrossFade()



    IEnumerator Fade(AudioClip playMe,
        float maxVolume,
        float fadingTime,
        float delay_before_crossFade = 0)
    {


        if (delay_before_crossFade > 0)
        {
            yield return new WaitForSeconds(delay_before_crossFade);
        }

        if (_isFirst)
        { // _source0 is currently playing the most recent AudioClip
            //so launch on source1
            _source1.clip = playMe;
            _source1.Play();
            _source1.volume = 0;

            if (_firstSourceFadeRoutine != null)
            {
                StopCoroutine(_firstSourceFadeRoutine);
            }
            _firstSourceFadeRoutine = StartCoroutine(fadeSource(_source1,
                _source1.volume,
                maxVolume,
                fadingTime));
            if (_zerothSourceFadeRoutine != null)
            {
                StopCoroutine(_zerothSourceFadeRoutine);
            }
            _zerothSourceFadeRoutine = StartCoroutine(fadeSource(_source0,
                _source0.volume,
                0,
                fadingTime));
            _isFirst = false;

            yield break;
        }

        //otherwise, _source1 is currently active, so play on _source0
        _source0.clip = playMe;
        _source0.Play();
        _source0.volume = 0;

        if (_zerothSourceFadeRoutine != null)
        {
            StopCoroutine(_zerothSourceFadeRoutine);
        }
        _zerothSourceFadeRoutine = StartCoroutine(fadeSource(_source0,
            _source0.volume,
            maxVolume,
            fadingTime));

        if (_firstSourceFadeRoutine != null)
        {
            StopCoroutine(_firstSourceFadeRoutine);
        }
        _firstSourceFadeRoutine = StartCoroutine(fadeSource(_source1,
            _source1.volume,
            0,
            fadingTime));
        _isFirst = true;
    }



    IEnumerator fadeSource(AudioSource sourceToFade, float startVolume, float endVolume, float duration)
    {
        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;

            sourceToFade.volume = Mathf.Clamp01(Mathf.Lerp(startVolume,
                endVolume,
                elapsed / duration));

            if (sourceToFade.volume == endVolume)
            {
                break;
            }
            yield return null;
        }//end while
    }


    //returns false if BOTH sources are not playing and there are no sounds are staged to be played.
    //also returns false if one of the sources is not yet initialized
    public bool isPlaying
    {
        get
        {
            if (_source0 == null || _source1 == null)
            {
                return false;
            }

            //otherwise, both sources are initialized. See if any is playing:
            if (_source0.isPlaying || _source1.isPlaying)
            {
                return true;
            }

            //none is playing:
            return false;
        }//end get
    }

    public bool bothPlaying()
    {
        if (_source0 != null && _source1 != null && _source0.isPlaying && _source1.isPlaying)
        {
            return true;
        }
        return false;
    }
}
