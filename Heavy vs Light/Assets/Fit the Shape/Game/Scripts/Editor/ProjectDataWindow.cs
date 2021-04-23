using UnityEngine;
using UnityEditor;
using Watermelon;
using Watermelon.Core;

public class ProjectDataWindow : EditorWindow
{
    ProjectDatabase database;

    ProjectDataEditor databaseEditor;

    private Vector2 scrollView;

    [MenuItem("Tools/Editor/Project Database")]
    static void ShowWindow()
    {
        ProjectDataWindow window = (ProjectDataWindow)EditorWindow.GetWindow(typeof(ProjectDataWindow));
        window.titleContent = new GUIContent("Project Database");
        window.Show();
    }

    private void OnEnable()
    {
        if (!database)
        {
            database = EditorUtils.GetAsset<ProjectDatabase>();

            databaseEditor = Editor.CreateEditor(database, typeof(ProjectDataEditor)) as ProjectDataEditor;
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
        if (database != null && databaseEditor != null)
        {
            scrollView = EditorGUILayout.BeginScrollView(scrollView);

            databaseEditor.serializedObject.Update();
            databaseEditor.OnInspectorGUI();
            databaseEditor.serializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);

            EditorGUILayout.EndScrollView();
        }
    }
}
