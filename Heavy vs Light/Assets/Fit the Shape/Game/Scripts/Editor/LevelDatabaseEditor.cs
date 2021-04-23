#pragma warning disable 414

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;

[CustomEditor(typeof(LevelDatabase))]
public class LevelDatabaseEditor : Editor
{

    private BaitEditor baitsEditor;
    private ObstacleEditor obstaclesEditor;
    private GemPatternsEditor gemPatternsEditor;
    private LevelsEditor levelsEditor;

    private bool isInited = false;

    private GUIContent addGUIContent;
    private GUIStyle addCenteredStyle;

    public ProjectDatabase projectDatabase;

    private LevelDatabase levelDatabase;

    private Vector2 scrollView = Vector2.zero;

    private int tabIndex = 0;
    
    private const string LEVEL_OBSTICLE_TYPES_PROPERTY_NAME = "obstacles";
    private const string LEVEL_BAIT_TYPES_PROPERTY_NAME = "baits";
    private const string LEVEL_GEM_PATTERNS_PROPERTY_NAME = "gemPatterns";
    private const string LEVELS_PROPERTY_NAME = "levels";

    private readonly string[] EDITOR_TABS = new string[6] { "Levels", "Baits", "Obstacles", "Gem Patterns", "Color Presets", "Skins" };

    private static LevelVisualizer visualizer;


    [InitializeOnLoadMethod]
    private static void LevelDatabaseStatic()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingEditMode)
        {
            if(visualizer != null){
                Transform tempPlatformContainer = visualizer.Container;
                if (tempPlatformContainer != null)
                {
                    DestroyImmediate(tempPlatformContainer.gameObject);
                }
            }
            
        }
    }

    
    private void OnEnable()
    {

        
        
        
        levelDatabase = (LevelDatabase) target;
        levelDatabase.Init();

        visualizer = new LevelVisualizer();
        visualizer.InitDatabase(projectDatabase, levelDatabase);

        baitsEditor = new BaitEditor(levelDatabase);
        obstaclesEditor = new ObstacleEditor(levelDatabase, target, projectDatabase);
        gemPatternsEditor = new GemPatternsEditor(levelDatabase, target, projectDatabase.gem, projectDatabase.straitLine);
        levelsEditor = new LevelsEditor();
        
        SerializedProperty  levelsProperty = serializedObject.FindProperty(LEVELS_PROPERTY_NAME);
        baitsEditor.OnEnable(serializedObject.FindProperty(LEVEL_BAIT_TYPES_PROPERTY_NAME));
        obstaclesEditor.OnEnable(serializedObject.FindProperty(LEVEL_OBSTICLE_TYPES_PROPERTY_NAME));
        gemPatternsEditor.OnEnable(serializedObject.FindProperty(LEVEL_GEM_PATTERNS_PROPERTY_NAME), visualizer);

        var generatorPresets = serializedObject.FindProperty("generatorPresets");
        
        levelsEditor.OnEnable(levelDatabase, gemPatternsEditor, obstaclesEditor, baitsEditor, levelsProperty, generatorPresets, visualizer);
    }

    private void InitStyles()
    {
        if (isInited)
            return;

        addGUIContent = new GUIContent("+");

        addCenteredStyle = GUI.skin.box;
        addCenteredStyle.alignment = TextAnchor.MiddleCenter;
        addCenteredStyle.fontSize = 36;
        addCenteredStyle.normal.textColor = GUI.skin.button.normal.textColor;

        isInited = true;
    }

    public void OnSceneGUI(){
        Handles.Label(new Vector3(1,2,0), " LABEL ");
        //EditorGUILayout.LabelField("Level Type");
        //tempPreset.levelType = (Level.LevelType) EditorGUILayout.EnumPopup(tempPreset.levelType);
        //EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        if(Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Editor doesn't work in play mode. Please, stop the game to edit levels!", MessageType.Warning, true);
            GUI.enabled = false;
        }

        scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
        serializedObject.Update();

        InitStyles();

        EditorGUI.BeginChangeCheck();
        tabIndex = GUILayout.Toolbar(tabIndex, EDITOR_TABS);
        if(EditorGUI.EndChangeCheck())
        {
            scrollView = Vector2.zero;
        }

        switch (tabIndex)
        {
            case 0:
                levelsEditor.LevelsEditorGUI();
                break;
            case 1:
                baitsEditor.BaitEditorGUI();
                break;
            case 2:
                obstaclesEditor.ObstacleEditorGUI();
                break;
            case 3:
                gemPatternsEditor.GemPatternsEditorGUI();
                break;
            case 4:
                CollorPresetsGUI();
                break;
            case 5:
                OnSkinsGUI();
                break;
        }

        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    private bool skinGroupsFoldout = false;
    private bool skinsFoldout = false;
    Skin newSkin = new Skin();

    public void OnSkinsGUI(){
        var groupsProperty = serializedObject.FindProperty("skinGroups");
        var costsProperty = serializedObject.FindProperty("skinGroupCosts");
        var skinsProperty = serializedObject.FindProperty("skins");
        bool exists;

        skinGroupsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(skinGroupsFoldout, "Skin Groups");
        if(skinGroupsFoldout){
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Jellies");
            EditorGUILayout.IntField(500);
            EditorGUILayout.EndHorizontal();
            for(int i = 0; i < groupsProperty.arraySize; i++){
                EditorGUILayout.BeginHorizontal();

                var text = groupsProperty.GetArrayElementAtIndex(i).stringValue;
                text = EditorGUILayout.TextField(text);
                exists = false;
                for(int j = 0; j < groupsProperty.arraySize; j++){
                    var name = groupsProperty.GetArrayElementAtIndex(j).stringValue;
                    if(name == text){
                        exists = true;
                        break;
                    }
                }
                if(!exists && text != "Jellies"){
                    groupsProperty.GetArrayElementAtIndex(i).stringValue = text;
                }
                

                int cost = 500;
                if(costsProperty.arraySize == i){
                    costsProperty.arraySize++;
                } else {
                    cost = costsProperty.GetArrayElementAtIndex(i).intValue;
                }
                
                cost = EditorGUILayout.IntField(cost);
                if(cost > 0){
                    costsProperty.GetArrayElementAtIndex(i).intValue = cost;
                }
                
                
                GUILayout.Space(20);
                if(GUILayout.Button("Remove")){
                    groupsProperty.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
            }
            if(GUILayout.Button("Add New Skin Group")){
                costsProperty.arraySize++;
                groupsProperty.arraySize++;
                costsProperty.GetArrayElementAtIndex(costsProperty.arraySize - 1).intValue = 500;
                groupsProperty.GetArrayElementAtIndex(groupsProperty.arraySize - 1).stringValue = "skin group " + groupsProperty.arraySize;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(20);

        if(skinsFoldouts.IsNullOrEmpty()){
            for(int i = 0; i < skinsProperty.arraySize; i++){
                skinsFoldouts.Add(false);
            }
        }

        newSkin.name = EditorGUILayout.TextField("Name: ", newSkin.name);
        newSkin.prefab = EditorGUILayout.ObjectField(new GUIContent("Prefab: "), newSkin.prefab, typeof(GameObject), false) as GameObject;
        newSkin.color = EditorGUILayout.ColorField("Color: ", newSkin.color);

        var groups = new List<string>();
        groups.Add("Jellies");
        for(int i = 0; i < groupsProperty.arraySize; i++){
            groups.Add(groupsProperty.GetArrayElementAtIndex(i).stringValue);
        }

        EditorGUI.BeginChangeCheck();
        int groupID = groups.IndexOf(newSkin.group);
        groupID = EditorGUILayout.Popup("Skin Group", groupID == -1? 0 : groupID, groups.ToArray());
        if(EditorGUI.EndChangeCheck()){
            newSkin.group = groups[groupID];
        }

        if(GUILayout.Button("Add New Skin")){

            exists = false;
            for(int i = 0; i < skinsProperty.arraySize; i++){
                var skinProp = skinsProperty.GetArrayElementAtIndex(i);
                var name = skinProp.FindPropertyRelative("name").stringValue;
                var group = skinProp.FindPropertyRelative("group").stringValue;
                if(name == newSkin.name && group == newSkin.group){
                    exists = true;
                    break;
                }
            }
            if(!exists && newSkin.name != ""){
                skinsProperty.arraySize++;
                var skinProperty = skinsProperty.GetArrayElementAtIndex(skinsProperty.arraySize - 1);
                skinProperty.FindPropertyRelative("prefab").objectReferenceValue = newSkin.prefab;
                skinProperty.FindPropertyRelative("color").colorValue = newSkin.color;
                skinProperty.FindPropertyRelative("group").stringValue = newSkin.group;
                skinProperty.FindPropertyRelative("name").stringValue = newSkin.name;
                newSkin = new Skin();
                skinsFoldouts.Add(true);
            }
        }

        for(int i = 0; i < skinsProperty.arraySize; i++){
            var skinProperty = skinsProperty.GetArrayElementAtIndex(i);
            var nameValue = skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;

            skinsFoldouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(skinsFoldouts[i], nameValue);
            if(skinsFoldouts[i]){
                nameValue = EditorGUILayout.TextField("Name: ", nameValue);
                var groupValue = skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("group").stringValue;

                exists = false;
                for(int j = 0; j < skinsProperty.arraySize; j++){
                    var skinProp = skinsProperty.GetArrayElementAtIndex(j);
                    var name = skinProp.FindPropertyRelative("name").stringValue;
                    var group = skinProp.FindPropertyRelative("group").stringValue;
                    if(name == nameValue && group == groupValue){
                        exists = true;
                        break;
                    }
                }
                if(!exists){
                    skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = nameValue;
                }

                var prefabValue = skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue;
                prefabValue = EditorGUILayout.ObjectField(new GUIContent("Prefab: "), prefabValue, typeof(GameObject), false) as GameObject;
                skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue = prefabValue;
                
                var colorValue = skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("color").colorValue;
                colorValue = EditorGUILayout.ColorField("Color: ", colorValue);
                skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("color").colorValue = colorValue;

                
                EditorGUI.BeginChangeCheck();
                groupID = groups.IndexOf(groupValue);
                groupID = EditorGUILayout.Popup("Skin Group", groupID == -1? 0 : groupID, groups.ToArray());
                if(EditorGUI.EndChangeCheck()){
                    groupValue = groups[groupID];
                    skinsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("group").stringValue = groupValue;
                }
                if(GUILayout.Button("Remove")){
                    skinsProperty.DeleteArrayElementAtIndex(i);
                    skinsFoldouts.RemoveAt(i);
                    i--;
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    float m_Value = 0;



    private bool turboFoldout = true;
    private bool gemRushFoldout = true;
    private bool defaultFoldout = true;
    private int selectedColorPresetIndex = -1;

    private List<bool> presetFoldouts = new List<bool>();
    private List<bool> skinsFoldouts = new List<bool>();

    public void CollorPresetsGUI(){
        var turboPreset = serializedObject.FindProperty("turboPreset");
        var gemRushPreset = serializedObject.FindProperty("gemRushPreset");
        var defaultPreset = serializedObject.FindProperty("defaultPreset");
        var colorPresets = serializedObject.FindProperty("colorPresets");
        if(presetFoldouts.IsNullOrEmpty()){
            for(int i = 0; i < colorPresets.arraySize; i++){
                presetFoldouts.Add(false);
            }
        }
        

        turboPreset.FindPropertyRelative("name").stringValue = "Turbo Preset";
        turboFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(turboFoldout, "Turbo Preset");
        if(turboFoldout){
            ColorField(turboPreset, "backgroundStart");
            ColorField(turboPreset, "backgroundFinish");
            ColorField(turboPreset, "platformBody");
            ColorField(turboPreset, "platformSurface");
            ColorField(turboPreset, "obstacleBody");
            ColorField(turboPreset, "obstacleSurface");
            ColorField(turboPreset, "railBody");
            ColorField(turboPreset, "railSurface");
            ColorField(turboPreset, "railPart");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        gemRushPreset.FindPropertyRelative("name").stringValue = "Gem Rush Preset";
        gemRushFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gemRushFoldout, "Gem Rush Preset");
        if(gemRushFoldout){
            ColorField(gemRushPreset, "backgroundStart");
            ColorField(gemRushPreset, "backgroundFinish");
            ColorField(gemRushPreset, "platformBody");
            ColorField(gemRushPreset, "platformSurface");
            ColorField(gemRushPreset, "obstacleBody");
            ColorField(gemRushPreset, "obstacleSurface");
            ColorField(gemRushPreset, "railBody");
            ColorField(gemRushPreset, "railSurface");
            ColorField(gemRushPreset, "railPart");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        defaultPreset.FindPropertyRelative("name").stringValue = "Default Preset";
        defaultFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(defaultFoldout, "Default Preset");
        if(defaultFoldout){
            ColorField(defaultPreset, "backgroundStart");
            ColorField(defaultPreset, "backgroundFinish");
            ColorField(defaultPreset, "platformBody");
            ColorField(defaultPreset, "platformSurface");
            ColorField(defaultPreset, "obstacleBody");
            ColorField(defaultPreset, "obstacleSurface");
            ColorField(defaultPreset, "railBody");
            ColorField(defaultPreset, "railSurface");
            ColorField(defaultPreset, "railPart");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //colorPresets.arraySize = 0;
        //presetFoldouts.Clear();
        //Rect button = EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add New Color Preset")){
            colorPresets.arraySize++;
            var newPreset = colorPresets.GetArrayElementAtIndex(colorPresets.arraySize - 1);
            newPreset.FindPropertyRelative("backgroundStart").colorValue = defaultPreset.FindPropertyRelative("backgroundStart").colorValue;
            newPreset.FindPropertyRelative("backgroundFinish").colorValue = defaultPreset.FindPropertyRelative("backgroundFinish").colorValue;
            newPreset.FindPropertyRelative("platformBody").colorValue = defaultPreset.FindPropertyRelative("platformBody").colorValue;
            newPreset.FindPropertyRelative("platformSurface").colorValue = defaultPreset.FindPropertyRelative("platformSurface").colorValue;
            newPreset.FindPropertyRelative("obstacleBody").colorValue = defaultPreset.FindPropertyRelative("obstacleBody").colorValue;
            newPreset.FindPropertyRelative("obstacleSurface").colorValue = defaultPreset.FindPropertyRelative("obstacleSurface").colorValue;
            newPreset.FindPropertyRelative("railBody").colorValue = defaultPreset.FindPropertyRelative("railBody").colorValue;
            newPreset.FindPropertyRelative("railSurface").colorValue = defaultPreset.FindPropertyRelative("railSurface").colorValue;
            newPreset.FindPropertyRelative("railPart").colorValue = defaultPreset.FindPropertyRelative("railPart").colorValue;
            colorPresets.GetArrayElementAtIndex(colorPresets.arraySize - 1).FindPropertyRelative("name").stringValue = "Color Preset " + colorPresets.arraySize;
            presetFoldouts.Add(false);
            
        }

        GUILayout.Space(20);
        //EditorGUILayout.EndHorizontal();


        for(int i = 0; i < colorPresets.arraySize; i++){
            var presetProperty = colorPresets.GetArrayElementAtIndex(i);

            presetFoldouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(presetFoldouts[i], presetProperty.FindPropertyRelative("name").stringValue);

            if(presetFoldouts[i]){
                StringField(presetProperty, "name", true);
                ColorField(presetProperty, "backgroundStart");
                ColorField(presetProperty, "backgroundFinish");
                ColorField(presetProperty, "platformBody");
                ColorField(presetProperty, "platformSurface");
                ColorField(presetProperty, "obstacleBody");
                ColorField(presetProperty, "obstacleSurface");
                ColorField(presetProperty, "railBody");
                ColorField(presetProperty, "railSurface");
                ColorField(presetProperty, "railPart");

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Remove Color Preset")){
                    colorPresets.DeleteArrayElementAtIndex(i);
                    presetFoldouts.RemoveAt(i);
                    i--;
                }
                if(GUILayout.Button("Copy Color Preset")){
                    colorPresets.arraySize++;
                    var newPresetProperty = colorPresets.GetArrayElementAtIndex(colorPresets.arraySize - 1);

                    newPresetProperty.FindPropertyRelative("name").stringValue = "Color Preset " + colorPresets.arraySize;
                    newPresetProperty.FindPropertyRelative("backgroundStart").colorValue = presetProperty.FindPropertyRelative("backgroundStart").colorValue;
                    newPresetProperty.FindPropertyRelative("backgroundFinish").colorValue = presetProperty.FindPropertyRelative("backgroundFinish").colorValue;
                    newPresetProperty.FindPropertyRelative("platformBody").colorValue = presetProperty.FindPropertyRelative("platformBody").colorValue;
                    newPresetProperty.FindPropertyRelative("platformSurface").colorValue = presetProperty.FindPropertyRelative("platformSurface").colorValue;
                    newPresetProperty.FindPropertyRelative("obstacleBody").colorValue = presetProperty.FindPropertyRelative("obstacleBody").colorValue;
                    newPresetProperty.FindPropertyRelative("obstacleSurface").colorValue = presetProperty.FindPropertyRelative("obstacleSurface").colorValue;
                    newPresetProperty.FindPropertyRelative("railBody").colorValue = presetProperty.FindPropertyRelative("railBody").colorValue;
                    newPresetProperty.FindPropertyRelative("railSurface").colorValue = presetProperty.FindPropertyRelative("railSurface").colorValue;
                    newPresetProperty.FindPropertyRelative("railPart").colorValue = presetProperty.FindPropertyRelative("railPart").colorValue;

                    presetFoldouts.Add(true);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }
        //EditorGUILayout.EndVertical();
        
    }

    private void StringField(SerializedProperty presetProperty, string name, bool unique){
        var text = presetProperty.FindPropertyRelative(name).stringValue;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name + ":");
        if(unique){
            var tempName = EditorGUILayout.TextField(text);

            // TODO: Checks

            presetProperty.FindPropertyRelative(name).stringValue = tempName;
        } else {
            presetProperty.FindPropertyRelative(name).stringValue = EditorGUILayout.TextField(text);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void ColorField(SerializedProperty presetProperty, string name){
        var color = presetProperty.FindPropertyRelative(name).colorValue;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name + ":");
        presetProperty.FindPropertyRelative(name).colorValue = EditorGUILayout.ColorField(color);
        EditorGUILayout.EndHorizontal();
    }

}