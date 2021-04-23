using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;
using System;

public class LevelsEditor
{
    private BaitEditor baitsEditor;
    private ObstacleEditor obstaclesEditor;
    private GemPatternsEditor patternsEditor;

    private SerializedProperty levelsProperty;

    private Pagination pagination = new Pagination(10, 5);

    private int selectedLevelIndex = -1;

    private SerializedProperty selectedLevelProperty;

    private int selectedBlockIndex = -1;


    private SerializedProperty selectedLevelTypeProperty;
    private SerializedProperty selectedLevelBlocksProperty;
    private SerializedProperty selectedLevelBaitProperty;

    private LevelDatabase database;

    private LevelVisualizer visualizer;

    private LevelGenerator generator;

    SerializedProperty presetsProperty;

    LevelGeneratorPreset tempPreset;

    public void OnEnable(LevelDatabase database, GemPatternsEditor patternsEditor, ObstacleEditor obstaclesEditor, BaitEditor baitsEditor, SerializedProperty levelsProperty, SerializedProperty presetsProperty, LevelVisualizer visualizer){
        this.database = database;
        this.patternsEditor = patternsEditor;
        this.obstaclesEditor = obstaclesEditor;
        this.baitsEditor = baitsEditor;
        this.levelsProperty = levelsProperty;
        this.visualizer = visualizer;
        this.presetsProperty = presetsProperty;
        pagination.Init(levelsProperty);

        generator = new LevelGenerator(database);
        tempPreset = null;

        baitsEditor.InitBaitNames();
        obstaclesEditor.InitObstacleNames();
        UnselectLevel();
    }

    private void UnselectLevel(){
        selectedLevelIndex = -1;
        selectedLevelProperty = null;
        UnselectBlock();
    }

    private void UnselectBlock(){
        selectedBlockIndex = -1;
    }

    bool isLevelGeneratorFold = true;

    int selectedPreset = -1;

    /*
        Draws entire editor of levels and blocks
    */
    public void LevelsEditorGUI(){
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(276));

        if (!isLevelGeneratorFold)
            GUI.color = EditorColor.green05;

        // Level Generator GUI

        Rect levelGeneratorRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
        EditorGUILayout.LabelField("Level Generator Settings");
        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;
        if (GUI.Button(levelGeneratorRect, GUIContent.none, GUIStyle.none)){
            isLevelGeneratorFold = !isLevelGeneratorFold;
        }
        
        if(!isLevelGeneratorFold){
            var presetsNames = new string[presetsProperty.arraySize + 1];
            presetsNames[0] = "New Preset";

            if(tempPreset == null){
                if(presetsProperty.arraySize == 0){
                    tempPreset = new LevelGeneratorPreset();
                } else {
                    tempPreset = new LevelGeneratorPreset();
                    var tempPresetProperty = presetsProperty.GetArrayElementAtIndex(0);
                    tempPreset.levelType = (Level.LevelType) tempPresetProperty.FindPropertyRelative("levelType").enumValueIndex;
                    tempPreset.turnChance = tempPresetProperty.FindPropertyRelative("turnChance").floatValue;
                    tempPreset.railsChance = tempPresetProperty.FindPropertyRelative("railsChance").floatValue;
                    tempPreset.ascendingRailsChance = tempPresetProperty.FindPropertyRelative("ascendingRailsChance").floatValue;
                    tempPreset.descendingRailsChance = tempPresetProperty.FindPropertyRelative("descendingRailsChance").floatValue;
                    tempPreset.tramplinChance = tempPresetProperty.FindPropertyRelative("tramplinChance").floatValue;
                    tempPreset.obstacleChance = tempPresetProperty.FindPropertyRelative("obstacleChance").floatValue;
                    tempPreset.multipleObstacleChance = tempPresetProperty.FindPropertyRelative("multipleObstacleChance").floatValue;
                    tempPreset.gemPatternChance = tempPresetProperty.FindPropertyRelative("gemPatternChance").floatValue;
                    tempPreset.amountOfBlocks = tempPresetProperty.FindPropertyRelative("amountOfBlocks").intValue;
                    tempPreset.name = tempPresetProperty.FindPropertyRelative("name").stringValue;
                    selectedPreset = 0;
                }
            }

            for (int i = 0; i < presetsProperty.arraySize; i++){
                presetsNames[i + 1] = presetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
            }

            EditorGUI.BeginChangeCheck();
            int presetID = EditorGUILayout.Popup("Select Preset: ", selectedPreset + 1, presetsNames);
            if(EditorGUI.EndChangeCheck()){
                presetID--;
                if(presetID != -1){
                    var tempPresetProperty = presetsProperty.GetArrayElementAtIndex(presetID);
                    tempPreset.levelType = (Level.LevelType) tempPresetProperty.FindPropertyRelative("levelType").enumValueIndex;
                    tempPreset.turnChance = tempPresetProperty.FindPropertyRelative("turnChance").floatValue;
                    tempPreset.railsChance = tempPresetProperty.FindPropertyRelative("railsChance").floatValue;
                    tempPreset.ascendingRailsChance = tempPresetProperty.FindPropertyRelative("ascendingRailsChance").floatValue;
                    tempPreset.descendingRailsChance = tempPresetProperty.FindPropertyRelative("descendingRailsChance").floatValue;
                    tempPreset.tramplinChance = tempPresetProperty.FindPropertyRelative("tramplinChance").floatValue;
                    tempPreset.obstacleChance = tempPresetProperty.FindPropertyRelative("obstacleChance").floatValue;
                    tempPreset.multipleObstacleChance = tempPresetProperty.FindPropertyRelative("multipleObstacleChance").floatValue;
                    tempPreset.gemPatternChance = tempPresetProperty.FindPropertyRelative("gemPatternChance").floatValue;
                    tempPreset.amountOfBlocks = tempPresetProperty.FindPropertyRelative("amountOfBlocks").intValue;
                    tempPreset.name = tempPresetProperty.FindPropertyRelative("name").stringValue;
                    selectedPreset = presetID;
                } else {
                    selectedPreset = -1;
                    tempPreset = new LevelGeneratorPreset();
                }
            }
            

            var max = 1f;

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Type");
            tempPreset.levelType = (Level.LevelType) EditorGUILayout.EnumPopup(tempPreset.levelType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Amount of blocks");
            tempPreset.amountOfBlocks = EditorGUILayout.IntField(tempPreset.amountOfBlocks);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Turn Chance");
            tempPreset.turnChance = EditorGUILayout.Slider(tempPreset.turnChance, 0, max);
            EditorGUILayout.EndHorizontal();
            max -= tempPreset.turnChance;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rails Chance");
            tempPreset.railsChance = EditorGUILayout.Slider(tempPreset.railsChance, 0, max);
            EditorGUILayout.EndHorizontal();
            max -= tempPreset.railsChance;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ascending Rails Chance");
            tempPreset.ascendingRailsChance = EditorGUILayout.Slider(tempPreset.ascendingRailsChance, 0, max);
            EditorGUILayout.EndHorizontal();
            max -= tempPreset.ascendingRailsChance;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Descending Rails Chance");
            tempPreset.descendingRailsChance = EditorGUILayout.Slider(tempPreset.descendingRailsChance, 0, max);
            EditorGUILayout.EndHorizontal();
            max -= tempPreset.descendingRailsChance;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tramplin Chance");
            tempPreset.tramplinChance = EditorGUILayout.Slider(tempPreset.tramplinChance, 0, max);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Obstacle Chance");
            tempPreset.obstacleChance = EditorGUILayout.Slider(tempPreset.obstacleChance, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Obstacle Dublication Chance");
            tempPreset.multipleObstacleChance = EditorGUILayout.Slider(tempPreset.multipleObstacleChance, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gem Pattern Chance");
            tempPreset.gemPatternChance = EditorGUILayout.Slider(tempPreset.gemPatternChance, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(12);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preset Name");
            tempPreset.name = EditorGUILayout.TextField(tempPreset.name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Save Preset") && selectedPreset != -1){

                var uniqueName = true;
                for(int i = 0; i < presetsProperty.arraySize; i++){
                    if(i == selectedPreset) continue;
                    if(presetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == tempPreset.name){
                        uniqueName = false;
                        break;
                    }
                }
                if(uniqueName){
                    if(tempPreset.amountOfBlocks >= 10){
                        if(tempPreset.name != ""){
                            var tempPresetProperty = presetsProperty.GetArrayElementAtIndex(selectedPreset);
                            tempPresetProperty.FindPropertyRelative("levelType").enumValueIndex = (int) tempPreset.levelType;
                            tempPresetProperty.FindPropertyRelative("turnChance").floatValue = tempPreset.turnChance;
                            tempPresetProperty.FindPropertyRelative("railsChance").floatValue = tempPreset.railsChance;
                            tempPresetProperty.FindPropertyRelative("ascendingRailsChance").floatValue = tempPreset.ascendingRailsChance;
                            tempPresetProperty.FindPropertyRelative("descendingRailsChance").floatValue = tempPreset.descendingRailsChance;
                            tempPresetProperty.FindPropertyRelative("tramplinChance").floatValue = tempPreset.tramplinChance;
                            tempPresetProperty.FindPropertyRelative("obstacleChance").floatValue = tempPreset.obstacleChance;
                            tempPresetProperty.FindPropertyRelative("multipleObstacleChance").floatValue = tempPreset.multipleObstacleChance;
                            tempPresetProperty.FindPropertyRelative("gemPatternChance").floatValue = tempPreset.gemPatternChance;
                            tempPresetProperty.FindPropertyRelative("amountOfBlocks").intValue = tempPreset.amountOfBlocks;
                            tempPresetProperty.FindPropertyRelative("name").stringValue = tempPreset.name;
                        } else {
                            EditorUtility.DisplayDialog("Cannot save this preset", "You should name this preset", "Ok");
                        }
                    } else {
                        EditorUtility.DisplayDialog("Cannot save this preset", "There should be at least 10 blocks in single level", "Ok");
                    }
                } else {
                    EditorUtility.DisplayDialog("Cannot save this preset", "Name '" + tempPreset.name + "' is not unique", "Ok");
                }

                
            }

            if(GUILayout.Button("Create new Preset")){
                var uniqueName = true;
                for(int i = 0; i < presetsProperty.arraySize; i++){
                    if(presetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == tempPreset.name){
                        uniqueName = false;
                        break;
                    }
                }
                if(uniqueName){
                    if(tempPreset.amountOfBlocks >= 10){
                        if(tempPreset.name != ""){
                            presetsProperty.arraySize++;
                            var tempPresetProperty = presetsProperty.GetArrayElementAtIndex(presetsProperty.arraySize - 1);
                            tempPresetProperty.FindPropertyRelative("levelType").enumValueIndex = (int) tempPreset.levelType;
                            tempPresetProperty.FindPropertyRelative("turnChance").floatValue = tempPreset.turnChance;
                            tempPresetProperty.FindPropertyRelative("railsChance").floatValue = tempPreset.railsChance;
                            tempPresetProperty.FindPropertyRelative("ascendingRailsChance").floatValue = tempPreset.ascendingRailsChance;
                            tempPresetProperty.FindPropertyRelative("descendingRailsChance").floatValue = tempPreset.descendingRailsChance;
                            tempPresetProperty.FindPropertyRelative("tramplinChance").floatValue = tempPreset.tramplinChance;
                            tempPresetProperty.FindPropertyRelative("obstacleChance").floatValue = tempPreset.obstacleChance;
                            tempPresetProperty.FindPropertyRelative("multipleObstacleChance").floatValue = tempPreset.multipleObstacleChance;
                            tempPresetProperty.FindPropertyRelative("gemPatternChance").floatValue = tempPreset.gemPatternChance;
                            tempPresetProperty.FindPropertyRelative("amountOfBlocks").intValue = tempPreset.amountOfBlocks;
                            tempPresetProperty.FindPropertyRelative("name").stringValue = tempPreset.name;
                            selectedPreset = presetsProperty.arraySize - 1;
                        } else {
                            EditorUtility.DisplayDialog("Cannot create this preset", "You should name this preset", "Ok");
                        }
                    } else {
                        EditorUtility.DisplayDialog("Cannot create this preset", "There should be at least 10 blocks in single level", "Ok");
                    }
                } else {
                    EditorUtility.DisplayDialog("Cannot create this preset", "Name '" + tempPreset.name + "' is not unique", "Ok");
                }
            }
            if(GUILayout.Button("Remove Preset")){
                if(selectedPreset != -1){
                    presetsProperty.RemoveFromVariableArrayAt(selectedPreset);
                    selectedPreset = -1;
                }
                
            }
            EditorGUILayout.EndHorizontal();
            
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        
        // Levels
        for (int i = pagination.GetMinElementNumber(); i < pagination.GetMaxElementNumber(); i++){
            bool isLevelSelected = selectedLevelIndex == i;
            if (isLevelSelected)
                GUI.color = EditorColor.green05;

            LevelBoxGUI(isLevelSelected, i);
            
            if (isLevelSelected)
                GUI.color = Color.white;
        }
        pagination.DrawPagination();
        EditorGUILayout.EndVertical();

        AddLevelButtonGUI();
        GUILayout.Space(12);

        if(selectedLevelIndex != -1 && selectedLevelProperty != null){
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Space(3);
            SelectedLevelGUI();
            EditorGUILayout.EndVertical();
        }
    }

    int generateLevelPresetIndex = 0;

    /*
        Draws add button of Level editor.
        Also initializes start and finish blocks of a new level
    */
    private void AddLevelButtonGUI(){
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        if(GUILayout.Button("Add Level")){
            if (baitsEditor.baitNames.IsNullOrEmpty()){
                EditorUtility.DisplayDialog("Cannot add new level", "Please add some baits first.", "OK");
            } else {
                levelsProperty.arraySize++;

                SerializedProperty tempLevel = levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1);
                tempLevel.FindPropertyRelative("baitID").intValue = 0;
                tempLevel.FindPropertyRelative("type").enumValueIndex = 0;
                
                var tempLevelBlocks = tempLevel.FindPropertyRelative("blocks");
                tempLevelBlocks.arraySize = 2;

                var tempStartBlock = tempLevelBlocks.GetArrayElementAtIndex(0);
                var tempEndBlock = tempLevelBlocks.GetArrayElementAtIndex(1);

                tempStartBlock.FindPropertyRelative("type").enumValueIndex = 0;
                tempStartBlock.FindPropertyRelative("gemPatternID").intValue = -1;
                tempStartBlock.FindPropertyRelative("obstacleID").intValue = -1;
                tempStartBlock.FindPropertyRelative("ammountOfObstacles").intValue = 0;
                tempStartBlock.FindPropertyRelative("ammountOfPillars").intValue = 0;

                tempEndBlock.FindPropertyRelative("type").enumValueIndex = 1;
                tempEndBlock.FindPropertyRelative("gemPatternID").intValue = -1;
                tempEndBlock.FindPropertyRelative("obstacleID").intValue = -1;
                tempEndBlock.FindPropertyRelative("ammountOfObstacles").intValue = 0;
                tempEndBlock.FindPropertyRelative("ammountOfPillars").intValue = 0;
                pagination.Init(levelsProperty);
            }
        }

        if(presetsProperty.arraySize > 0){
            var presetsNames = new string[presetsProperty.arraySize];
            for (int i = 0; i < presetsProperty.arraySize; i++){
                presetsNames[i] = presetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
            }
            if(GUILayout.Button("Genetate Level")){
                var newLevel = generator.GenerateLevel(presetsProperty.GetArrayElementAtIndex(generateLevelPresetIndex));
                levelsProperty.arraySize++;
                SerializedProperty tempLevel = levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1);
                tempLevel.FindPropertyRelative("baitID").intValue = newLevel.baitID;
                tempLevel.FindPropertyRelative("type").enumValueIndex = (int) newLevel.type;
                var tempLevelBlocks = tempLevel.FindPropertyRelative("blocks");
                tempLevelBlocks.arraySize = newLevel.blocks.Length;
                for(int i = 0; i < newLevel.blocks.Length; i++){
                    var tempBlock = tempLevelBlocks.GetArrayElementAtIndex(i);
                    tempBlock.FindPropertyRelative("type").enumValueIndex = (int) newLevel.blocks[i].type;
                    tempBlock.FindPropertyRelative("gemPatternID").intValue = newLevel.blocks[i].gemPatternID;
                    tempBlock.FindPropertyRelative("obstacleID").intValue = newLevel.blocks[i].obstacleID;
                    tempBlock.FindPropertyRelative("ammountOfObstacles").intValue = newLevel.blocks[i].ammountOfObstacles;
                    tempBlock.FindPropertyRelative("ammountOfPillars").intValue = newLevel.blocks[i].ammountOfPillars;
                }
                pagination.Init(levelsProperty);
            }
            generateLevelPresetIndex = EditorGUILayout.Popup("Select Preset: ", generateLevelPresetIndex, presetsNames);
        
            EditorGUILayout.EndHorizontal();
        }
    }

    

    /*
        Draws list of levels
    */
    private void LevelBoxGUI(bool isLevelSelected, int index){
        Rect levelRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
        EditorGUILayout.LabelField("Level " + index.ToString());
        LevelBoxNavigationButtonsGUI(index);
        LevelBoxButtonGUI(levelRect, isLevelSelected, index);
        EditorGUILayout.EndHorizontal();
    }


    /*
        Draws and implements navigation buttons of a level.
    */
    private void LevelBoxNavigationButtonsGUI(int index){
        if (GUILayout.Button("▶", GUILayout.Width(16), GUILayout.Height(16))){
            PlayerPrefs.SetInt("Level", index);
        }
        if (GUILayout.Button("↑", GUILayout.Width(16), GUILayout.Height(16))){
            if (index > 0){
                levelsProperty.MoveArrayElement(index, index - 1);
                UnselectLevel();
            }
        }
        if (GUILayout.Button("↓", GUILayout.Width(16), GUILayout.Height(16))){
            if (index + 1 < levelsProperty.arraySize){
                levelsProperty.MoveArrayElement(index, index + 1);
                UnselectLevel();
            }
        }
        GUI.color = Color.red;
        if (GUILayout.Button("x", GUILayout.Width(16), GUILayout.Height(16))){
            int tempIndex = index;

            if (EditorUtility.DisplayDialog("Are you sure?", "This level will be removed!", "Remove", "Cancel")){
                levelsProperty.RemoveFromVariableArrayAt(tempIndex);
                UnselectLevel();
                pagination.Init(levelsProperty);
            }
        }
        GUI.color = Color.white;
    }

    /*
        Draws level selection button
    */
    private void LevelBoxButtonGUI(Rect levelRect, bool isLevelSelected, int index){
        if (GUI.Button(levelRect, GUIContent.none, GUIStyle.none)){
            if(isLevelSelected){
                UnselectLevel();
                visualizer.DestroyEditorLevel();
            }
            else{
                if (selectedLevelIndex != -1){
                    UnselectLevel();
                }
                
                selectedLevelIndex = index;
                selectedLevelProperty = levelsProperty.GetArrayElementAtIndex(index);

                selectedLevelTypeProperty = selectedLevelProperty.FindPropertyRelative("type");
                selectedLevelBaitProperty = selectedLevelProperty.FindPropertyRelative("baitID");
                selectedLevelBlocksProperty = selectedLevelProperty.FindPropertyRelative("blocks");
                visualizer.DestroyEditorLevel();
                EditorApplication.delayCall += delegate
                {
                    
                    visualizer.VisualizeLevel(database.levels[selectedLevelIndex]);
                };
            }
        }
    }

    /*
        Draws selected level editor
    */
    private void SelectedLevelGUI(){
        LevelDataTypeGUI();
        LevelDataBaitGUI();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        SelectedLevelBlocksGUI();
    }

    /*
        Draws selected level type
    */
    private void LevelDataTypeGUI(){
        EditorGUI.BeginChangeCheck();
        int levelTypeId = EditorGUILayout.Popup("Level Type: ", selectedLevelTypeProperty.enumValueIndex, selectedLevelTypeProperty.enumNames);
        if(EditorGUI.EndChangeCheck()){
            selectedLevelTypeProperty.enumValueIndex = levelTypeId;
        }
    }

    /*
        Draws selected level bait
    */
    private void LevelDataBaitGUI(){
        if(selectedLevelTypeProperty.enumValueIndex == 0){
            EditorGUI.BeginChangeCheck();
            int baitTypeID = EditorGUILayout.Popup("Bait: ", selectedLevelBaitProperty.intValue, baitsEditor.baitNames);
            if(EditorGUI.EndChangeCheck()){
                selectedLevelBaitProperty.intValue = baitTypeID;
            }
        }
    }

    /*
        Draws list of block
    */
    void SelectedLevelBlocksGUI(){
        SelectedLevelAddBlockButton();
        for(int i = selectedLevelBlocksProperty.arraySize - 1; i >= 0; i--){
            var selectedLevelBlockProperty = selectedLevelBlocksProperty.GetArrayElementAtIndex(i);
            bool isBlockSelected = selectedBlockIndex == i;
            BlocksGUI(selectedLevelBlockProperty, isBlockSelected, i);
        }
    }

    /*
        Draws add block button, also initializes new block
    */
    void SelectedLevelAddBlockButton(){
        if(GUILayout.Button("Add Block")){
            selectedLevelBlocksProperty.InsertArrayElementAtIndex(selectedLevelBlocksProperty.arraySize - 1);
            var tempBlock = selectedLevelBlocksProperty.GetArrayElementAtIndex(selectedLevelBlocksProperty.arraySize - 2);
            tempBlock.FindPropertyRelative("type").enumValueIndex = 2;
            tempBlock.FindPropertyRelative("gemPatternID").intValue = -1;
            tempBlock.FindPropertyRelative("obstacleID").intValue = -1;
            tempBlock.FindPropertyRelative("ammountOfObstacles").intValue = 0;
            tempBlock.FindPropertyRelative("ammountOfPillars").intValue = 0;

            EditorApplication.delayCall += delegate
            {
                visualizer.DestroyEditorLevel();
                visualizer.VisualizeLevel(database.levels[selectedLevelIndex]);
            };
            
        }
    }

    /*
        Draws block
    */
    private void BlocksGUI(SerializedProperty selectedLevelBlockProperty, bool isBlockSelected, int index){
        BlockBoxGUI(selectedLevelBlockProperty, isBlockSelected, index);
        if(selectedBlockIndex == index){
            SelectedBlockGUI(selectedLevelBlockProperty, index);
        }
    }

    /*
        Draws block's box and buttons
    */
    private void BlockBoxGUI(SerializedProperty selectedLevelBlockProperty, bool isBlockSelected, int index){
        if(isBlockSelected){
            GUI.color = EditorColor.green05;
        }
        Rect blockRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
        if(index == 0){
            EditorGUILayout.LabelField("Start");
        } else if(index == selectedLevelBlocksProperty.arraySize - 1) {
            EditorGUILayout.LabelField("Finish");
        } else {
            var type = (Level.BlockType) selectedLevelBlockProperty.FindPropertyRelative("type").enumValueIndex;
            EditorGUILayout.LabelField("Block - " + type);
            GUI.color = Color.red;
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16))){
                selectedLevelBlocksProperty.RemoveFromVariableArrayAt(index);
                selectedBlockIndex = -1;
                visualizer.DestroyEditorLevel();
                visualizer.VisualizeLevel(database.levels[selectedLevelIndex]);
            }
            GUI.color = Color.white;
        }
        GUI.color = Color.white;

        if (GUI.Button(blockRect, GUIContent.none, GUIStyle.none)){
            if(isBlockSelected){
                UnselectBlock();
            } else {
                selectedBlockIndex = index;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    
    /*
        Draws selected block data
    */
    private void SelectedBlockGUI(SerializedProperty selectedLevelBlockProperty, int index){
        var selectedBlockType = selectedLevelBlockProperty.FindPropertyRelative("type");
        if(index != 0 && index != selectedLevelBlocksProperty.arraySize - 1){
            selectedBlockTypeGUI(selectedBlockType, index);

            if(selectedBlockType.enumValueIndex == 2){
                
                
                var selectedBlockPillarsAmountProperty = selectedLevelBlockProperty.FindPropertyRelative("ammountOfPillars");

                SelectedBlockObstacleGUI(selectedLevelBlockProperty, index);

                SelectedBlockPatternGUI(selectedLevelBlockProperty);

                /*EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Pillars");

                int pillarsAmount = EditorGUILayout.IntSlider(selectedBlockPillarsAmountProperty.intValue, 0, 5);
                selectedBlockPillarsAmountProperty.intValue = pillarsAmount;

                EditorGUILayout.EndHorizontal();*/
            }
        }

        visualizer.DestroyEditorLevel();
        visualizer.VisualizeLevel(database.levels[selectedLevelIndex]);
    }

    /*
        Draws selected block's type
    */
    private void selectedBlockTypeGUI(SerializedProperty selectedBlockType, int index){
        var previousBlockIndex = selectedLevelBlocksProperty.GetArrayElementAtIndex(index - 1).FindPropertyRelative("type").intValue;
        var nextBlockIndex = selectedLevelBlocksProperty.GetArrayElementAtIndex(index + 1).FindPropertyRelative("type").intValue;
        
        var allBlockNames = Enum.GetNames(typeof(Level.BlockType));
        string[] blockNames;
        if(previousBlockIndex == 2 &&  nextBlockIndex == 2 ){
            blockNames = new string[allBlockNames.Length - 2];
            for(int j = 2; j < allBlockNames.Length; j++){
                blockNames[j - 2] = allBlockNames[j];
            }
        } else {
            blockNames = new string[1];
            for(int j = 2; j <= 2; j++){
                blockNames[j - 2] = allBlockNames[j];
            }
        }

        EditorGUI.BeginChangeCheck();
        int baitTypeID = EditorGUILayout.Popup("Block Type: ", selectedBlockType.enumValueIndex - 2, blockNames);
        if(EditorGUI.EndChangeCheck()){
            selectedBlockType.enumValueIndex = baitTypeID + 2;
        }
    }

    /*
        Draws selected block's obstacles and their amount
    */
    private void SelectedBlockObstacleGUI(SerializedProperty selectedLevelBlockProperty, int index){
        var selectedBlockAmountProperty = selectedLevelBlockProperty.FindPropertyRelative("ammountOfObstacles");
        var selectedBlockObstacleIDProperty = selectedLevelBlockProperty.FindPropertyRelative("obstacleID");

        string[] obstacleNames;
        if(obstaclesEditor.obstacleNames.IsNullOrEmpty()){
            obstaclesEditor.InitObstacleNames();
        }
        if(obstaclesEditor.obstacleNames.IsNullOrEmpty()){
            obstacleNames = new string[]{"None"};
        } else {
            obstacleNames = new string[obstaclesEditor.obstacleNames.Length + 1];
            obstacleNames[0] = "None";
            for(int i = 0; i < obstaclesEditor.obstacleNames.Length; i++){
                obstacleNames[i + 1] = obstaclesEditor.obstacleNames[i];
            }
        }

        EditorGUI.BeginChangeCheck();
        int obstacleID = EditorGUILayout.Popup("Obstacle: ", selectedBlockObstacleIDProperty.intValue + 1, obstacleNames);
        if(EditorGUI.EndChangeCheck()){
            selectedBlockObstacleIDProperty.intValue = obstacleID - 1;
        }

        EditorGUILayout.BeginHorizontal();
        if(selectedBlockObstacleIDProperty.intValue >= 0){
            EditorGUILayout.LabelField("Amount");
            int amount = EditorGUILayout.IntSlider(selectedBlockAmountProperty.intValue, 1, 3);
            selectedBlockAmountProperty.intValue = amount;
        }
        EditorGUILayout.EndHorizontal();
    }

    /*
        Draws selected block's pattern
    */
    private void SelectedBlockPatternGUI(SerializedProperty selectedLevelBlockProperty){
        var selectedBlockPatternIDProperty = selectedLevelBlockProperty.FindPropertyRelative("gemPatternID");
        string[] patternNames;
        if(patternsEditor.gemPatternNames.IsNullOrEmpty()){
            patternsEditor.InitGemPatternNames();
        }
        if(patternsEditor.gemPatternNames.IsNullOrEmpty()){
            patternNames = new string[]{"None"};
        } else {
            patternNames = new string[patternsEditor.gemPatternNames.Length + 1];
            patternNames[0] = "None";
            for(int i = 0; i < patternsEditor.gemPatternNames.Length; i++){
                patternNames[i + 1] = patternsEditor.gemPatternNames[i];
            }
        }

        EditorGUI.BeginChangeCheck();
        int patternID = EditorGUILayout.Popup("Gem Pattern: ", selectedBlockPatternIDProperty.intValue + 1, patternNames);
        if(EditorGUI.EndChangeCheck()){
            selectedBlockPatternIDProperty.intValue = patternID - 1;
        }
    }
}