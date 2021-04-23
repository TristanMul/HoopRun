using UnityEngine;
using UnityEditor;
using Watermelon;
using Watermelon.Core;

public class LevelDatabaseWindow : EditorWindow
{
    private LevelDatabase levelDatabase;
    private LevelDatabaseEditor levelDatabaseEditor;

    private Vector2 scrollView;

    [MenuItem("Tools/Editor/Level Database")]
    static void ShowWindow()
    {
        LevelDatabaseWindow window = (LevelDatabaseWindow)EditorWindow.GetWindow(typeof(LevelDatabaseWindow));
        window.titleContent = new GUIContent("Level Database");
        window.Show();
    }

    private void OnEnable()
    {
        if (!levelDatabase)
        {
            levelDatabase = EditorUtils.GetAsset<LevelDatabase>();

            levelDatabaseEditor = Editor.CreateEditor(levelDatabase, typeof(LevelDatabaseEditor)) as LevelDatabaseEditor;
        }
    }

    private void OnDisable()
    {
        GameObject editorGameObject = GameObject.Find("[EDITOR]");
        if (editorGameObject != null)
            DestroyImmediate(editorGameObject);
    }

    private void OnGUI()
    {
        if (levelDatabase != null && levelDatabaseEditor != null)
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView);

            levelDatabaseEditor.serializedObject.Update();
            levelDatabaseEditor.OnInspectorGUI();
            levelDatabaseEditor.serializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);

            EditorGUILayout.EndScrollView();
        }
    }

    private void OnSceneGUI() {
        if (levelDatabase != null && levelDatabaseEditor != null)
        {
            levelDatabaseEditor.OnSceneGUI();
        }
    }
}
