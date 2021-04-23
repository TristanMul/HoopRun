using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Watermelon;
using Watermelon.Core;

/*
    Handles Bait's editor;
*/
public class BaitEditor
{
    private LevelDatabase levelDatabase;
    private Bait tempBait;
    private SerializedProperty baitsArrayProperty;

    private bool uniqueNameError = false;
    private int selectedBaitIndex = -1;
    public string[] baitNames;

    public BaitEditor(LevelDatabase levelDatabase)
    {
        this.levelDatabase = levelDatabase;
    }

    public void OnEnable(SerializedProperty baitsArrayProperty)
    {
        this.baitsArrayProperty = baitsArrayProperty;
        tempBait = new Bait();
        InitBaitNames();
    }

    /*
        Selects all names of baits
    */
    public void InitBaitNames()
    {
        if (levelDatabase.baits.IsNullOrEmpty())
        {
            baitNames = null;
        }
        else
        {
            baitNames = levelDatabase.baits.Select(x => x.name).ToArray();
        }
    }

    public void BaitEditorGUI()
    {
        NewBaitGUI();

        for (int i = 0; i < baitsArrayProperty.arraySize; i++)
        {
            bool isSelected = selectedBaitIndex == i;

            SerializedProperty baitProperty = this.baitsArrayProperty.GetArrayElementAtIndex(i);

            BaitGUI(baitProperty, isSelected, i);

        }
    }

    private void BaitGUI(SerializedProperty baitProperty, bool isSelected, int index)
    {

        SerializedProperty baitNameProperty = baitProperty.FindPropertyRelative("name");
        SerializedProperty baitPrefabProperty = baitProperty.FindPropertyRelative("prefab");
        SerializedProperty baitTextureProperty = baitProperty.FindPropertyRelative("texture");
        SerializedProperty baitLockedTextureProperty = baitProperty.FindPropertyRelative("lockedTexture");

        Rect rect = EditorGUILayout.BeginHorizontal(GUI.skin.box);

        EditorGUILayout.LabelField(baitNameProperty.stringValue);
        DeleteBaitButton(index);

        if (GUI.Button(rect, "", GUIStyle.none))
        {
            if (isSelected)
            {
                selectedBaitIndex = -1;
            }
            else
            {
                selectedBaitIndex = index;
            }
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (isSelected)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(baitNameProperty);
            EditorGUILayout.PropertyField(baitPrefabProperty);
            EditorGUILayout.PropertyField(baitTextureProperty);
            EditorGUILayout.PropertyField(baitLockedTextureProperty);
            EditorGUILayout.EndVertical();
        }
    }

    private void DeleteBaitButton(int index)
    {
        GUI.color = Color.red;
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16)))
        {
            baitsArrayProperty.RemoveFromVariableArrayAt(index);

            EditorApplication.delayCall += delegate
            {
                InitBaitNames();
            };
        }
        GUI.color = Color.white;
    }

    private void NewBaitGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        NewBaitNameGUI();
        NewBaitPrefabGUI();
        NewBaitAddButtonGUI();
        EditorGUILayout.EndVertical();
    }

    private void NewBaitNameGUI()
    {
        uniqueNameError = false;

        EditorGUI.BeginChangeCheck();
        tempBait.name = EditorGUILayout.TextField(new GUIContent("Name: "), tempBait.name);

        if (EditorGUI.EndChangeCheck())
        {
            uniqueNameError = System.Array.FindIndex(levelDatabase.baits, x => x.name == tempBait.name) == -1;
        }
    }

    private void NewBaitPrefabGUI()
    {
        tempBait.prefab = EditorGUILayout.ObjectField(new GUIContent("Prefab: "), tempBait.prefab, typeof(GameObject), false) as GameObject;
        EditorGUILayout.BeginHorizontal();
        tempBait.texture = EditorGUILayout.ObjectField(new GUIContent("Texture: "), tempBait.prefab, typeof(Sprite), false) as Sprite;
        tempBait.lockedTexture = EditorGUILayout.ObjectField(new GUIContent("Locked Texture: "), tempBait.prefab, typeof(Sprite), false) as Sprite;
        EditorGUILayout.EndHorizontal();
    }

    private void NewBaitAddButtonGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Bait"))
        {
            if (!uniqueNameError)
            {
                if (tempBait.name != null)
                {
                    if (tempBait.prefab != null)
                    {
                        baitsArrayProperty.arraySize++;

                        SerializedProperty newBaitProperty = baitsArrayProperty.GetArrayElementAtIndex(baitsArrayProperty.arraySize - 1);
                        newBaitProperty.FindPropertyRelative("name").stringValue = tempBait.name;
                        newBaitProperty.FindPropertyRelative("prefab").objectReferenceValue = tempBait.prefab;
                        newBaitProperty.FindPropertyRelative("texture").objectReferenceValue = tempBait.texture;
                        newBaitProperty.FindPropertyRelative("lockedTexture").objectReferenceValue = tempBait.lockedTexture;
                        tempBait = new Bait();

                        GUI.FocusControl(null);

                        EditorApplication.delayCall += delegate
                        {
                            InitBaitNames();
                        };
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Cannot create this bait", "Prefab cannot be null", "Ok");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Cannot create this bait", "You should name this bait", "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Cannot create this bait", "Name '" + tempBait.name + "' is not unique", "Ok");
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}