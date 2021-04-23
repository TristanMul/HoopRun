using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;
using System;

[CustomEditor(typeof(ProjectDatabase))]
public class ProjectDataEditor : Editor
{

    private Vector2 scrollView = Vector2.zero;
    private bool isInited = false;

    private GUIContent addGUIContent;
    private GUIStyle addCenteredStyle;

    private ProjectDatabase projectDatabase;

    private SerializedProperty gem;
    private SerializedProperty start;
    private SerializedProperty finish;
    private SerializedProperty straitLine;
    private SerializedProperty turnLeft;
    private SerializedProperty turnRight;
    private SerializedProperty rails;
    private SerializedProperty ascendingRails;
    private SerializedProperty descendingRails;
    private SerializedProperty tramplin;
    private SerializedProperty pillar;
    private SerializedProperty obstacleBlock;

    private SerializedProperty obstacleSurface;
    private SerializedProperty obstacleBody;

    private void OnEnable(){
        projectDatabase = (ProjectDatabase) target;
        gem =             serializedObject.FindProperty("gem");
        pillar =          serializedObject.FindProperty("pillar");
        start =           serializedObject.FindProperty("start");
        finish =          serializedObject.FindProperty("finish");
        straitLine =      serializedObject.FindProperty("straitLine");
        turnLeft =        serializedObject.FindProperty("turnLeft");
        turnRight =       serializedObject.FindProperty("turnRight");
        rails =           serializedObject.FindProperty("rails");
        ascendingRails =  serializedObject.FindProperty("ascendingRails");
        descendingRails = serializedObject.FindProperty("descendingRails");
        tramplin =        serializedObject.FindProperty("tramplin");
        obstacleBlock =   serializedObject.FindProperty("obstacleBlock");
        obstacleSurface = serializedObject.FindProperty("obstacleSurfaceMaterial");
        obstacleBody =    serializedObject.FindProperty("obstacleBodyMaterial");
    }

    private void ProjectDataGUI()
    {
        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

        EditorGUILayoutCustom.Header("REFERENCES");

        gem.objectReferenceValue =              EditorGUILayout.ObjectField(new GUIContent("Gem: "),                gem.objectReferenceValue, typeof(GameObject), false) as GameObject;
        pillar.objectReferenceValue =           EditorGUILayout.ObjectField(new GUIContent("Pillar: "),             pillar.objectReferenceValue, typeof(GameObject), false) as GameObject;
        start.objectReferenceValue =            EditorGUILayout.ObjectField(new GUIContent("Start: "),              start.objectReferenceValue, typeof(GameObject), false) as GameObject;
        finish.objectReferenceValue =           EditorGUILayout.ObjectField(new GUIContent("Finish: "),             finish.objectReferenceValue, typeof(GameObject), false) as GameObject;
        straitLine.objectReferenceValue =       EditorGUILayout.ObjectField(new GUIContent("Strait Line: "),        straitLine.objectReferenceValue, typeof(GameObject), false) as GameObject;
        turnLeft.objectReferenceValue =         EditorGUILayout.ObjectField(new GUIContent("Turn Left: "),          turnLeft.objectReferenceValue, typeof(GameObject), false) as GameObject;
        turnRight.objectReferenceValue =        EditorGUILayout.ObjectField(new GUIContent("Turn Right: "),         turnRight.objectReferenceValue, typeof(GameObject), false) as GameObject;
        rails.objectReferenceValue =            EditorGUILayout.ObjectField(new GUIContent("Rails: "),              rails.objectReferenceValue, typeof(GameObject), false) as GameObject;
        ascendingRails.objectReferenceValue =   EditorGUILayout.ObjectField(new GUIContent("Ascending Rails: "),    ascendingRails.objectReferenceValue, typeof(GameObject), false) as GameObject;
        descendingRails.objectReferenceValue =  EditorGUILayout.ObjectField(new GUIContent("Descending Rails: "),   descendingRails.objectReferenceValue, typeof(GameObject), false) as GameObject;
        tramplin.objectReferenceValue =         EditorGUILayout.ObjectField(new GUIContent("Tramplin: "),           tramplin.objectReferenceValue, typeof(GameObject), false) as GameObject;
        obstacleBlock.objectReferenceValue =    EditorGUILayout.ObjectField(new GUIContent("Obstacle Blok: "),      obstacleBlock.objectReferenceValue, typeof(GameObject), false) as GameObject;
        obstacleSurface.objectReferenceValue =  EditorGUILayout.ObjectField(new GUIContent("Obstacle Surface: "),   obstacleSurface.objectReferenceValue, typeof(Material), false) as Material;
        obstacleBody.objectReferenceValue =     EditorGUILayout.ObjectField(new GUIContent("Obstacle Body: "),      obstacleBody.objectReferenceValue, typeof(Material), false) as Material;
        EditorGUILayout.EndVertical();
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

        // code here
        ProjectDataGUI();

        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    

    /*private void ProjectDataGUI(){
        List<Level.BlockType> types = new List<Level.BlockType>();
        var blockNames = Enum.GetValues(typeof(Level.BlockType));
        foreach (var block in blockNames)
        {
            types.Add((Level.BlockType)block);    
        }
        
        var arraySize = blockPrefabsProperty.arraySize;

        for(int i = 0; i < arraySize; i++){
            var blockPrefab = blockPrefabsProperty.GetArrayElementAtIndex(i);
            var existingType = (Level.BlockType) blockPrefab.FindPropertyRelative("block").enumValueIndex;
            types.Remove(existingType);
        }
      
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(276));

        EditorGUI.BeginChangeCheck();
        int levelTypeId = EditorGUILayout.Popup("Level Type: ", 0, types.Select(x => x.ToString()).ToArray());
        if(EditorGUI.EndChangeCheck()){
            tempBlockPrefab.block = (Level.BlockType) levelTypeId;
            tempObstacle.prefab = EditorGUILayout.ObjectField(new GUIContent("Obsticle: "), tempObstacle.prefab, typeof(GameObject), false) as GameObject;
        }

        EditorGUILayout.EndVertical();
    }*/

    
}
