using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Rotorz.ReorderableList;

[CustomEditor(typeof(GameManager))]
[ExecuteInEditMode]
public class GameManagerEditor : Editor
{
    #region Methods

    int categoryNumber = 1, levelNumber = 1;
    GameManager instance;
    WordBoardCreator wordBoardCreator;
    //SerializedProperty categoryInfos;

    private void OnEnable()
    {
        instance = (GameManager)target;
        //categoryInfos = serializedObject.FindProperty("categoryInfos");
        wordBoardCreator = instance.GetComponent<WordBoardCreator>();
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
	{
		serializedObject.Update();

        //ReorderableListGUI.Title("Category Infos");
        //ReorderableListGUI.ListField(categoryInfos);

        EditorGUILayout.Space();

		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startingHints"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("letterBoard"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("wordGrid"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("letterTilePrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardedButton"));

        DrawCategoryInfos();
		DrawDailyPuzzleLevels();

        EditorGUILayout.Space();

        categoryNumber = EditorGUILayout.IntField("Category Number: ", categoryNumber);
        levelNumber = EditorGUILayout.IntField("Category Number: ", levelNumber);

        if (GUILayout.Button("Create board file", GUILayout.MinHeight(30)))
        {
            CategoryInfo currentCategoryInfo = instance.CategoryInfos[categoryNumber - 1];
            string boardId = Utilities.FormatBoardId(currentCategoryInfo.name, levelNumber - 1);

            wordBoardCreator.StartCreatingBoard(boardId, currentCategoryInfo.levelInfos[levelNumber - 1].words, OnWordBoardFinished, 5000L);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete Saved Data", GUILayout.MinHeight(30)))
		{
			System.IO.File.Delete(GameManager.SaveDataPath);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Board File Creator Window", GUILayout.MinHeight(30)))
		{
			BoardFileCreatorWindow.ShowWindow();
		}

		EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
	}

    private void OnWordBoardFinished(WordBoard wordBoard)
    {
        if (wordBoard != null)
        {
            Utilities.SaveWordBoard(wordBoard, Utilities.BoardFilesDirectory);
            AssetDatabase.Refresh();
        }
    }

    private void Update()
    {
        wordBoardCreator.Update();
    }

    private void DrawCategoryInfos()
	{
		SerializedProperty categoryInfos = serializedObject.FindProperty("categoryInfos");
		
		// Draw the categoryInfos but not its children, this will just draw the foldout (little arrow thingy)
		EditorGUILayout.PropertyField(categoryInfos, false);
		
		// If categoryInfos is expanded then draw its children
		if (categoryInfos.isExpanded)
		{
			EditorGUI.indentLevel++;
			
			// Draw the "Size" property for the array
			EditorGUILayout.PropertyField(categoryInfos.FindPropertyRelative("Array.size"));
			
			// Draw each of the CategoryInfos in the categoryInfos list
			for (int i = 0; i < categoryInfos.arraySize; i++)
			{
				SerializedProperty categoryInfo = categoryInfos.GetArrayElementAtIndex(i);
				
				EditorGUILayout.BeginHorizontal();
				
				// This will just draw the "Category Infos" foldout and the tooltip for categoryInfos
				EditorGUILayout.PropertyField(categoryInfo, false);
				
				bool deleted = false;
				
				// Draw the remove button so you can remove elements in the middle of the list
				if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
				{
					categoryInfos.DeleteArrayElementAtIndex(i);
					deleted = true;
				}
				
				EditorGUILayout.EndHorizontal();
				
				// Now if it was not deleted and is expanded then draw its properties
				if (!deleted && categoryInfo.isExpanded)
				{
					EditorGUI.indentLevel++;
					
					/* If you want to add properties to CategoryInfo then add them here. */
					
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("name"));
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("description"));
					EditorGUILayout.PropertyField(categoryInfo.FindPropertyRelative("icon"));
					
					DrawCategoryLevelInfos(categoryInfo);
					
					EditorGUI.indentLevel--;
				}
			}
			
			EditorGUI.indentLevel--;
		}
	}

	private void DrawCategoryLevelInfos(SerializedProperty categoryInfo)
	{
		SerializedProperty levelInfos = categoryInfo.FindPropertyRelative("levelInfos");

		// Draw the levelInfos but not its children, this will just draw the foldout (little arrow thingy)
		EditorGUILayout.PropertyField(levelInfos, false);

		DrawLevelInfos(levelInfos, "Level");
	}

	private void DrawLevelInfos(SerializedProperty levelInfos, string prefix)
	{
		// If its expanded then draw its children
		if (levelInfos.isExpanded)
		{
			EditorGUI.indentLevel++;

			// Draw the "Size" property for the levelInfos array
			EditorGUILayout.PropertyField(levelInfos.FindPropertyRelative("Array.size"));

			// Draw each of the LevelInfos
			for (int i = 0; i < levelInfos.arraySize; i++)
			{
				SerializedProperty levelInfo = levelInfos.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				// Draw the foldout for the LevelInfo
				EditorGUILayout.PropertyField(levelInfo, new GUIContent(prefix + " " + (i + 1) + " Words"), false);

				bool deleted = false;

				// Draw the remove button so you can remove elements in the middle of the list
				if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
				{
					levelInfos.DeleteArrayElementAtIndex(i);
					deleted = true;
				}

				EditorGUILayout.EndHorizontal();

				// If its not deleted and is expanded
				if (!deleted && levelInfo.isExpanded)
				{
					EditorGUI.indentLevel++;

					DrawLevelInfoWords(levelInfo);

					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}
	}

	private void DrawLevelInfoWords(SerializedProperty levelInfo)
	{
		SerializedProperty levelWords = levelInfo.FindPropertyRelative("words");

		// Draw the "Size" property for words array
		EditorGUILayout.PropertyField(levelWords.FindPropertyRelative("Array.size"));

		// Draw each of the words in a TextField and a Button beside it that deletes the word when pressed
		for (int i = 0; i < levelWords.arraySize; i++)
		{
			EditorGUILayout.BeginHorizontal();

			// Draw the TextField
			levelWords.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(levelWords.GetArrayElementAtIndex(i).stringValue);

			// Draw the delete button
			if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(14)))
			{
				levelWords.DeleteArrayElementAtIndex(i);
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	private void DrawDailyPuzzleLevels()
	{
		SerializedProperty dailyPuzzles = serializedObject.FindProperty("dailyPuzzles");

		EditorGUILayout.PropertyField(dailyPuzzles, false);

		DrawLevelInfos(dailyPuzzles, "Puzzle");

		EditorGUILayout.PropertyField(serializedObject.FindProperty("dailyPuzzleIcon"));
	}

	#endregion
}
