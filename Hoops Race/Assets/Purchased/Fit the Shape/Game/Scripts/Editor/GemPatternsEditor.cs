using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;


public class GemPatternsEditor
{

    private Object target;
    private LevelDatabase levelDatabase;
    private GemPattern tempGemPattern;
    private SerializedProperty gemPatternProperty;
    private int selectedGemPatternIndex = -1;
    public string[] gemPatternNames;
    private int tabIndex = 0;
    private GridButton gridButton;

    private readonly string[] EDITOR_TABS = new string[3] { "Bottom", "Middle", "Top"};

    private SerializedProperty selectedGemPatternProperty;
    private SerializedProperty selectedGemPatternNameProperty;
    private SerializedProperty selectedGemPatternGemsProperty;

    private LevelVisualizer visualizer;

    private GameObject gem;
    public GameObject straitLinePrefab;


    public GemPatternsEditor(LevelDatabase levelDatabase, Object target, GameObject gem, GameObject straitLinePrefab){
        this.levelDatabase = levelDatabase;
        this.target = target;
        this.gem = gem;
        this.straitLinePrefab = straitLinePrefab;
    }

    public void OnEnable(SerializedProperty gemPatternProperty, LevelVisualizer visualizer){
        this.gemPatternProperty = gemPatternProperty;
        tempGemPattern = new GemPattern();
        InitGemPatternNames();
        this.visualizer = visualizer;

        gridButton = new GridButton();
    }

    public void InitGemPatternNames()
    {
        if(levelDatabase.gemPatterns == null){
            gemPatternNames = null;
        } else {
            gemPatternNames = levelDatabase.gemPatterns.Select(x => x.name).ToArray();
        }
        
    }

    private void UnselectPattern()
    {
        // Unselect level
        selectedGemPatternIndex = -1;
        selectedGemPatternProperty = null;
    }

    public void GemPatternsEditorGUI(){
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(276));
        AddGemPatternButton();
        
        int arraySize = 0;
        if(gemPatternProperty != null){
            arraySize = gemPatternProperty.arraySize;
        }

        for (int i = 0; i < arraySize; i++)
        {
            bool isGemSelected = selectedGemPatternIndex == i;
            GemPatternGUI(isGemSelected, i);
            
        }
        EditorGUILayout.EndVertical();
    }

    private void AddGemPatternButton(){
        if(GUILayout.Button("Add Pattern"))
        {
            gemPatternProperty.arraySize++;
            SerializedProperty tempPattern = gemPatternProperty.GetArrayElementAtIndex(gemPatternProperty.arraySize - 1);
            tempPattern.FindPropertyRelative("gems").arraySize = 0;
            tempPattern.FindPropertyRelative("name").stringValue = "Pattern " + (gemPatternProperty.arraySize - 1);
            EditorApplication.delayCall += delegate
            {
                InitGemPatternNames();
            };
        }
    }

    private void GemPatternGUI(bool isGemSelected, int index){
        
        GemPatternBoxGUI(isGemSelected, index);
        
        if(selectedGemPatternIndex == index){
            GemPatternToolbarGUI();
        }
        if (isGemSelected)
            GUI.color = Color.white;
    }

    private void GemPatternToolbarGUI(){
        tabIndex = GUILayout.Toolbar(tabIndex, EDITOR_TABS);
            switch (tabIndex)
            {
                case 0:
                    gridButton.Init(5, 9);
                    break;
                case 1:
                    gridButton.Init(3, 9);
                    break;
                case 2:
                    gridButton.Init(1, 9);
                    break;
            }
            GridButtonGUI();
    }

    private void GridButtonGUI(){
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
            initButtons();
            bool buttonClicked = false;
            gridButton.DrawGridButtons(ref buttonClicked);
            if(buttonClicked)
            {
                readButtons();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
    }

    private void GemPatternBoxGUI(bool isGemSelected, int index){
        if (isGemSelected)
            GUI.color = EditorColor.green05;

        Rect patternRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
        GemPatternBoxDataGUI(index);
        GUI.color = Color.white;
        GemPatternBoxButtonGUI(patternRect, isGemSelected, index);
        EditorGUILayout.EndHorizontal();
    }

    private void GemPatternBoxDataGUI(int index){
        EditorGUILayout.LabelField(gemPatternProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue);

        GUI.color = Color.red;
        if (GUILayout.Button("x", GUILayout.Width(16), GUILayout.Height(16)))
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "This level will be removed!", "Remove", "Cancel"))
            {
                gemPatternProperty.RemoveFromVariableArrayAt(index);
                UnselectPattern();
                visualizer.DestroyGems();
            }
        }
    }

    private void GemPatternBoxButtonGUI(Rect patternRect, bool isGemSelected, int index){
        if (GUI.Button(patternRect, GUIContent.none, GUIStyle.none)) {
            if(isGemSelected){
                UnselectPattern();
                visualizer.DestroyGems();
            } else {
                if (selectedGemPatternIndex != -1){
                    UnselectPattern();
                    visualizer.DestroyGems();
                }

                selectedGemPatternIndex = index;
                selectedGemPatternProperty = gemPatternProperty.GetArrayElementAtIndex(index);

                selectedGemPatternNameProperty = selectedGemPatternProperty.FindPropertyRelative("name");
                selectedGemPatternGemsProperty = selectedGemPatternProperty.FindPropertyRelative("gems");
            }
        }
    }

    private void readButtons(){
        var level = gridButton.buttonValues;
        
        for(int i = 0; i < selectedGemPatternGemsProperty.arraySize; i++){
            var gem = selectedGemPatternGemsProperty.GetArrayElementAtIndex(i).vector3IntValue;
            if(gem.y == tabIndex){
                //if(level[(int)gem.x, (int)gem.z] == false){
                    selectedGemPatternGemsProperty.RemoveFromVariableArrayAt(i);
                    i--;
                //}
            }
        }

        var x = 3;
        switch(tabIndex){
            case 0:
                x = 5;
                break;
            case 1:
                x = 3;
                break;
            case 2:
                x = 1;
                break;
        }
        var y = 9;
        for(int i = 0; i < x; i++){
            for(int j = 0; j < y; j++){
                if(level[i,j] == true){
                    selectedGemPatternGemsProperty.arraySize++;
                    SerializedProperty tempVect = selectedGemPatternGemsProperty.GetArrayElementAtIndex(selectedGemPatternGemsProperty.arraySize - 1);
                    tempVect.vector3IntValue = new Vector3Int(i, tabIndex, j);
                }
            }
        }
    }

    private void initButtons(){
        var x = 3;
        var y = 9;
        switch(tabIndex){
            case 0:
                x = 5;
                break;
            case 1:
                x = 3;
                break;
            case 2:
                x = 1;
                break;
        }
        var level = new bool[x, y];
        for(int i = 0; i < x; i++){
            for(int j = 0; j < y; j++){
                level[i,j] = false;
            }
        }
        var pattern = levelDatabase.gemPatterns[selectedGemPatternIndex];
        foreach(var gem in pattern.gems){
            if(gem.y == tabIndex){
                level[gem.x, gem.z] = true;
            }
        }

        gridButton.buttonValues = level;
        visualizer.DestroyGems();

        visualizer.SpawnGemsEditor(pattern.gems);

    }
}
