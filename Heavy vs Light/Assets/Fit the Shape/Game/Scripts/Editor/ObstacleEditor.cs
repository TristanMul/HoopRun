using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Watermelon;
using Watermelon.Core;

public class ObstacleEditor
{

    private LevelDatabase levelDatabase;

    private UnityEngine.Object target;

    private Obstacle tempObstacle;
    private SerializedProperty obstacleArrayProperty;
    public string[] obstacleNames;
    private int selectedObstacleIndex = -1;

    private ProjectDatabase projectDatabase;

    bool initialized = false;

    public Transform container;
    public Transform Container{
        get{
            if(container == null){
                GameObject tempContainer = GameObject.Find("[EDITOR]");
                if(tempContainer == null){
                    tempContainer = new GameObject("[EDITOR]");
                }
                if(tempContainer != null){
                    container = tempContainer.transform;
                }
            }
            return container;
        }
    }


    public ObstacleEditor(LevelDatabase levelDatabase, UnityEngine.Object target, ProjectDatabase projectDatabase){
        this.levelDatabase = levelDatabase;
        this.target = target;
        this.projectDatabase = projectDatabase;
    }

    public void OnEnable(SerializedProperty obstacleArrayProperty){
        this.obstacleArrayProperty = obstacleArrayProperty;
        tempObstacle = new Obstacle();
        InitObstacleNames();
        initialized = false;
    }

    public void InitObstacleNames()
    {
        if(levelDatabase.obstacles.IsNullOrEmpty()){
            obstacleNames = null;
        } else {
            obstacleNames = levelDatabase.obstacles.Select(x => x.name).ToArray();
        }
    }

    float playerScale = 0.0f;

    public void ObstacleEditorGUI(){
        
        NewObstacleGUI();

        for (int i = 0; i < obstacleArrayProperty.arraySize; i++)
        {
            bool isSelected = selectedObstacleIndex == i;

            SerializedProperty obstacleProperty = obstacleArrayProperty.GetArrayElementAtIndex(i);

            

            ObstacleGUI(obstacleProperty, isSelected, i);
        }

        

        //Debug.LogError(DateTimeOffset.Now.ToUnixTimeMilliseconds() - time);
    }

    private void Recalculate(SerializedProperty obstacleProperty, SerializedProperty obstaclePrefabProperty, SerializedProperty obstacleDetailedPrefabProperty, string name, bool [,] obstacleArray){
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        var isCalculated = new bool[15, 15];
        for(int i = 0; i < 15; i++){
            for(int j = 0; j < 15; j++){
                if(isCalculated[i, j]){
                    continue;
                }
                if(obstacleArray[i,j]){
                    int width = 0;
                    int height = 0;
                    for(int ii = i; ii < 15; ii++){
                        if(obstacleArray[ii, j] && !isCalculated[ii, j]){
                            width++;
                        } else {
                            break;
                        }
                    }
                    

                    for(int jj = j; jj < 15; jj++){
                        bool all = true;
                        for(int ii = i; ii < i + width; ii++){
                            if(!obstacleArray[ii, jj] || isCalculated[ii, jj]){
                                all = false;
                                break;
                            }
                        }
                        if(all){
                            height++;
                        } else {
                            break;
                        }
                    }

                    for(int ii = i; ii < i + width; ii++){
                        for(int jj = j; jj < j + height; jj++){
                            isCalculated[ii, jj] = true;
                        }
                    }

                    var top = 3f - j * 0.2f;
                    var bottom = 3f - (j + height) * 0.2f;
                    var left = -1.5f + i * 0.2f;
                    var right = -1.5f + (i + width) * 0.2f;

                    var topLeft = new Vector3(left, top,  0.05f);
                    var topRight = new Vector3(right, top,  0.05f);
                    var bottomRight = new Vector3(right, bottom,  0.05f);
                    var bottomLeft = new Vector3(left, bottom,  0.05f);

                    var topLeftBack = new Vector3(left, top,  -0.05f);
                    var topRightBack = new Vector3(right, top,  -0.05f);
                    var bottomRightBack = new Vector3(right, bottom,  -0.05f);
                    var bottomLeftBack = new Vector3(left, bottom,  -0.05f);

                    if(!vertices.Contains(topLeft)) vertices.Add(topLeft);
                    if(!vertices.Contains(topRight)) vertices.Add(topRight);
                    if(!vertices.Contains(bottomRight)) vertices.Add(bottomRight);
                    if(!vertices.Contains(bottomLeft)) vertices.Add(bottomLeft);

                    if(!vertices.Contains(topLeftBack)) vertices.Add(topLeftBack);
                    if(!vertices.Contains(topRightBack)) vertices.Add(topRightBack);
                    if(!vertices.Contains(bottomRightBack)) vertices.Add(bottomRightBack);
                    if(!vertices.Contains(bottomLeftBack)) vertices.Add(bottomLeftBack);

                    /*Debug.Log("topLeft: " + topLeft);
                    Debug.Log("topRight: " + topRight);
                    Debug.Log("bottomRight: " + bottomRight);
                    Debug.Log("bottomLeft: " + bottomLeft);*/

                    int topLeftIndex = vertices.IndexOf(topLeft);
                    int topRightIndex = vertices.IndexOf(topRight);
                    int bottomRightIndex = vertices.IndexOf(bottomRight);
                    int bottomLeftIndex = vertices.IndexOf(bottomLeft);

                    int topLeftIndexBack = vertices.IndexOf(topLeftBack);
                    int topRightIndexBack = vertices.IndexOf(topRightBack);
                    int bottomRightIndexBack = vertices.IndexOf(bottomRightBack);
                    int bottomLeftIndexBack = vertices.IndexOf(bottomLeftBack);

                    triangles.Add(topRightIndex);
                    triangles.Add(topLeftIndex);
                    triangles.Add(bottomLeftIndex);

                    triangles.Add(bottomLeftIndex);
                    triangles.Add(bottomRightIndex);
                    triangles.Add(topRightIndex);

                    triangles.Add(topLeftIndexBack);
                    triangles.Add(topRightIndexBack);
                    triangles.Add(bottomLeftIndexBack);

                    triangles.Add(bottomRightIndexBack);
                    triangles.Add(bottomLeftIndexBack);
                    triangles.Add(topRightIndexBack);
                }
            }
        }
        


        List<Vector3> bodyVertices = new List<Vector3>();
        List<Vector3Int> bodyIndicies = new List<Vector3Int>();
        List<int> bodyTriangles = new List<int>();
        var isCalculatedBody = new bool[15, 15];
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;

        for(int i = 0; i < 15; i++){
            for(int j = 0; j < 15; j++){
                if(!obstacleArray[i, j]){
                    continue;
                }
                var newMinX = -1.5f + i * 0.2f;
                var newMaxX = -1.5f + (i + 1) * 0.2f;
                var newMaxY = 3f - j * 0.2f;
                if(minX > newMinX) minX = newMinX;
                if(maxX < newMaxX) maxX = newMaxX;
                if(maxY < newMaxY) maxY = newMaxY;

                if(i == 7){
                    var newMinY = 3f - (j + 1) * 0.2f;
                    if(minY > newMinY) minY = newMinY;
                }
                
                if(j == 0 || !obstacleArray[i, j - 1]){
                    
                    var left = -1.5f + i * 0.2f;
                    var right = -1.5f + (i + 1) * 0.2f; 
                    var y = 3f - j * 0.2f;
                    var leftForward = new Vector3(left, y, 0.05f);
                    var leftBack = new Vector3(left, y, -0.05f);
                    var rightForward = new Vector3(right, y, 0.05f);
                    var rightBack = new Vector3(right, y, -0.05f);

                    if(!bodyVertices.Contains(leftForward)) bodyVertices.Add(leftForward);
                    if(!bodyVertices.Contains(leftBack)) bodyVertices.Add(leftBack);
                    if(!bodyVertices.Contains(rightForward)) bodyVertices.Add(rightForward);
                    if(!bodyVertices.Contains(rightBack)) bodyVertices.Add(rightBack);

                    int leftForwardIndex = bodyVertices.IndexOf(leftForward);
                    int leftBackIndex = bodyVertices.IndexOf(leftBack);
                    int rightForwardIndex = bodyVertices.IndexOf(rightForward);
                    int rightBackIndex = bodyVertices.IndexOf(rightBack);

                    bodyTriangles.Add(leftBackIndex);
                    bodyTriangles.Add(leftForwardIndex);
                    bodyTriangles.Add(rightBackIndex);

                    bodyTriangles.Add(leftForwardIndex);
                    bodyTriangles.Add(rightForwardIndex);
                    bodyTriangles.Add(rightBackIndex);
                
                } 
                if(j == 14 || !obstacleArray[i, j + 1]) {
                    var left = -1.5f + i * 0.2f;
                    var right = -1.5f + (i + 1) * 0.2f; 
                    var y = 3f - (j + 1) * 0.2f;

                    var leftForward = new Vector3(left, y, 0.05f);
                    var leftBack = new Vector3(left, y, -0.05f);
                    var rightForward = new Vector3(right, y, 0.05f);
                    var rightBack = new Vector3(right, y, -0.05f);

                    if(!bodyVertices.Contains(leftForward)) bodyVertices.Add(leftForward);
                    if(!bodyVertices.Contains(leftBack)) bodyVertices.Add(leftBack);
                    if(!bodyVertices.Contains(rightForward)) bodyVertices.Add(rightForward);
                    if(!bodyVertices.Contains(rightBack)) bodyVertices.Add(rightBack);

                    int leftForwardIndex = bodyVertices.IndexOf(leftForward);
                    int leftBackIndex = bodyVertices.IndexOf(leftBack);
                    int rightForwardIndex = bodyVertices.IndexOf(rightForward);
                    int rightBackIndex = bodyVertices.IndexOf(rightBack);

                    bodyTriangles.Add(leftForwardIndex);
                    bodyTriangles.Add(leftBackIndex);
                    bodyTriangles.Add(rightBackIndex);

                    bodyTriangles.Add(rightForwardIndex);
                    bodyTriangles.Add(leftForwardIndex);
                    bodyTriangles.Add(rightBackIndex);
                }

                if(i == 0 || !obstacleArray[i - 1, j]) {

                    var x = -1.5f + i * 0.2f;
                    var top = 3f - j * 0.2f;
                    var bottom = 3f - (j + 1) * 0.2f; 
                    
                    var topForward = new Vector3(x, top, 0.05f);
                    var topBack = new Vector3(x, top, -0.05f);
                    var bottomForward = new Vector3(x, bottom, 0.05f);
                    var bottomBack = new Vector3(x, bottom, -0.05f);

                    if(!bodyVertices.Contains(topForward)) bodyVertices.Add(topForward);
                    if(!bodyVertices.Contains(topBack)) bodyVertices.Add(topBack);
                    if(!bodyVertices.Contains(bottomForward)) bodyVertices.Add(bottomForward);
                    if(!bodyVertices.Contains(bottomBack)) bodyVertices.Add(bottomBack);

                    int topForwardIndex = bodyVertices.IndexOf(topForward);
                    int topBackIndex = bodyVertices.IndexOf(topBack);
                    int bottomForwardIndex = bodyVertices.IndexOf(bottomForward);
                    int bottomBackIndex = bodyVertices.IndexOf(bottomBack);

                    bodyTriangles.Add(topBackIndex);
                    bodyTriangles.Add(bottomBackIndex);
                    bodyTriangles.Add(topForwardIndex);

                    bodyTriangles.Add(bottomBackIndex);
                    bodyTriangles.Add(bottomForwardIndex);
                    bodyTriangles.Add(topForwardIndex);
                }
                if(i == 14 || !obstacleArray[i + 1, j]){
                    var x = -1.5f + (i + 1) * 0.2f;
                    var top = 3f - j * 0.2f;
                    var bottom = 3f - (j + 1) * 0.2f; 
                    
                    var topForward = new Vector3(x, top, 0.05f);
                    var topBack = new Vector3(x, top, -0.05f);
                    var bottomForward = new Vector3(x, bottom, 0.05f);
                    var bottomBack = new Vector3(x, bottom, -0.05f);

                    if(!bodyVertices.Contains(topForward)) bodyVertices.Add(topForward);
                    if(!bodyVertices.Contains(topBack)) bodyVertices.Add(topBack);
                    if(!bodyVertices.Contains(bottomForward)) bodyVertices.Add(bottomForward);
                    if(!bodyVertices.Contains(bottomBack)) bodyVertices.Add(bottomBack);

                    int topForwardIndex = bodyVertices.IndexOf(topForward);
                    int topBackIndex = bodyVertices.IndexOf(topBack);
                    int bottomForwardIndex = bodyVertices.IndexOf(bottomForward);
                    int bottomBackIndex = bodyVertices.IndexOf(bottomBack);

                    bodyTriangles.Add(bottomBackIndex);
                    bodyTriangles.Add(topBackIndex);
                    bodyTriangles.Add(topForwardIndex);

                    bodyTriangles.Add(bottomForwardIndex);
                    bodyTriangles.Add(bottomBackIndex);
                    bodyTriangles.Add(topForwardIndex);
                }
            }
        }

        obstacleProperty.FindPropertyRelative("minX").floatValue = minX;
        obstacleProperty.FindPropertyRelative("maxX").floatValue = maxX;
        obstacleProperty.FindPropertyRelative("minY").floatValue = minY;
        obstacleProperty.FindPropertyRelative("maxY").floatValue = maxY;
        

        Mesh body = new Mesh();
        body.vertices = bodyVertices.ToArray();
        body.triangles = bodyTriangles.ToArray();

        Mesh surface = new Mesh();
        surface.vertices = vertices.ToArray();
        surface.triangles = triangles.ToArray();


        GameObject obstacleSurface = new GameObject("surface", typeof(MeshFilter), typeof(MeshRenderer));
        obstacleSurface.GetComponent<MeshFilter>().mesh = surface;
        obstacleSurface.GetComponent<Renderer>().material = projectDatabase.obstacleSurfaceMaterial;

        GameObject obstacleBody = new GameObject("body", typeof(MeshFilter), typeof(MeshRenderer));
        obstacleBody.GetComponent<MeshFilter>().mesh = body;
        obstacleBody.GetComponent<Renderer>().material = projectDatabase.obstacleBodyMaterial;

        

        GameObject obstacle = new GameObject("obstacle");
        GameObject detailedObstacle = new GameObject("detailed obstacle");

        detailedObstacle.tag = "Obstacle";
        BoxCollider boxCollider = detailedObstacle.AddComponent<BoxCollider>();

        

        obstacleSurface.transform.SetParent(obstacle.transform);
        obstacleBody.transform.SetParent(obstacle.transform);

        detailedObstacle.transform.position = new Vector3(0,0,0);

        boxCollider.center = new Vector3(0, 0.5f, 0.25f);
        boxCollider.size = new Vector3(1, 1, 0.1f);
        boxCollider.isTrigger = true;


        List<GameObject> copies = new List<GameObject>();
        for(int i = 0; i < 15; i++){
            for(int j = 0; j < 15; j++){
                if(obstacleArray[i,j]){
                    var copy = UnityEngine.Object.Instantiate(projectDatabase.obstacleBlock);
                    copy.transform.position = new Vector3(-1.5f + i * 0.2f + 0.1f ,3f - j * 0.2f - 0.1f, 0);
                    copies.Add(copy);
                    copy.transform.SetParent(detailedObstacle.transform);
                }
                
            }
        }



        String bodyPath = "Assets/Jelly Shift/Game/Models/Obstacles/" + name + "_body.mesh";
        String surfacePath = "Assets/Jelly Shift/Game/Models/Obstacles/" + name + "_surface.mesh";
        String prefabPath = "Assets/Jelly Shift/Game/Prefabs/Obstacles/" + name + ".prefab";
        String detailedPrefabPath = "Assets/Jelly Shift/Game/Prefabs/Obstacles/" + name + "_detailed.prefab";

        AssetDatabase.DeleteAsset(bodyPath);
        AssetDatabase.DeleteAsset(surfacePath);
        AssetDatabase.DeleteAsset(prefabPath);
        AssetDatabase.DeleteAsset(detailedPrefabPath);

        AssetDatabase.CreateAsset(body, bodyPath);
        AssetDatabase.CreateAsset(surface, surfacePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        obstaclePrefabProperty.objectReferenceValue = PrefabUtility.SaveAsPrefabAsset(obstacle, prefabPath);
        obstacleDetailedPrefabProperty.objectReferenceValue = PrefabUtility.SaveAsPrefabAsset(detailedObstacle, detailedPrefabPath);
        if(!initialized){
            PoolManager.InitSingletone();
            initialized = true;
        }
        

        Pool solidPool = PoolManager.instance.pools.Find(x => x.name.Equals(name + "_OBSTACLE"));
        
        if(solidPool == null){
            solidPool = new Pool();
            solidPool.name = name + "_OBSTACLE";
            solidPool.willGrow = true;
            solidPool.poolType = Pool.PoolType.Single;
            solidPool.poolSize = 10;
            PoolManager.instance.pools.Add(solidPool);
        }

        solidPool.poolSize = 10;
        solidPool.prefab = (GameObject) obstaclePrefabProperty.objectReferenceValue;

        Pool detailedPool = PoolManager.instance.pools.Find(x => x.name.Equals(name + "_OBSTACLE_DETAILED"));
        if (detailedPool == null){
            detailedPool = new Pool();
            detailedPool.name = name + "_OBSTACLE_DETAILED";
            detailedPool.willGrow = true;
            detailedPool.poolType = Pool.PoolType.Single;
            detailedPool.poolSize = 10;
            PoolManager.instance.pools.Add(detailedPool);
        } else {
            
        }
        detailedPool.poolSize = 10;
        detailedPool.prefab = (GameObject) obstacleDetailedPrefabProperty.objectReferenceValue;

        //PoolManager.instance.SaveCache();
        
        //UnityEngine.Object.Instantiate(obstacle);
        
        

        UnityEngine.Object.DestroyImmediate(obstacle);
        
        foreach(var copy in copies){
            UnityEngine.Object.DestroyImmediate(copy);
        }
        UnityEngine.Object.DestroyImmediate(detailedObstacle);
        var pooledObjects = GameObject.Find("[PooledObjects]");
        if(pooledObjects != null){
            UnityEngine.Object.DestroyImmediate(pooledObjects);
        }
        //
    }

    private void ObstacleGUI(SerializedProperty obstacleProperty, bool isSelected, int index){
        
        SerializedProperty obstacleNameProperty = obstacleProperty.FindPropertyRelative("name");
        SerializedProperty obstaclePrefabProperty = obstacleProperty.FindPropertyRelative("prefab");
        SerializedProperty obstacleDetailedPrefabProperty = obstacleProperty.FindPropertyRelative("detailedPrefab");
        SerializedProperty obstacleLinesProperty = obstacleProperty.FindPropertyRelative("lines");
        SerializedProperty obstacleBaitPoint = obstacleProperty.FindPropertyRelative("baitPoint");
        
        Rect rect = EditorGUILayout.BeginHorizontal(GUI.skin.box);

        EditorGUILayout.LabelField(obstacleNameProperty.stringValue);
        ObstacleDeleteButton(index);
        if (selectedObstacleIndex == -1) isSelected = false;
        
        if (GUI.Button(rect, "", GUIStyle.none)){
            if (isSelected){
                selectedObstacleIndex = -1;
            }
            else{
                selectedObstacleIndex = index;
            }
            return;
        }

        EditorGUILayout.EndHorizontal();

        if(isSelected)
        {
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(obstacleNameProperty);
            EditorGUILayout.PropertyField(obstaclePrefabProperty);
            EditorGUILayout.PropertyField(obstacleDetailedPrefabProperty);
            EditorGUILayout.EndVertical();
            
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Player width: ");
            playerScale = EditorGUILayout.Slider(playerScale, 0.25f, 1.75f);
            EditorGUILayout.EndHorizontal();

            var obstacleArray = new bool[15, 15];
            for(int i = 0; i < obstacleLinesProperty.arraySize; i++){
                var lineProperty = obstacleLinesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("line");
                for(int j = 0; j < lineProperty.arraySize; j++){
                    obstacleArray[i, j] = lineProperty.GetArrayElementAtIndex(j).boolValue;
                }
            }





            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Recalculate (May take a few seconds)"))
            {
                Recalculate(obstacleProperty, obstaclePrefabProperty, obstacleDetailedPrefabProperty, obstacleNameProperty.stringValue, obstacleArray);
            }
            EditorGUILayout.EndHorizontal();
            
            var lastRect = GUILayoutUtility.GetLastRect(); 

            var playerSize = new Vector2(playerScale * 150, (2 - playerScale) * 150);
            var PlayerRect = new Rect(lastRect.xMin + 225 - playerSize.x / 2, lastRect.yMax + 450 - playerSize.y, playerSize.x , playerSize.y);
            
            

            //var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            //var paintBlock = new Rect(lastRect.xMin, lastRect.yMax, 300, 300);
            GUILayout.Space(450);
            EditorGUILayout.BeginVertical(GUILayout.Width(300), GUILayout.Height(300));
            //m_Value = GUI.HorizontalSlider(new Rect(100, 300, 100, 30), m_Value, 1.0f, 250.0f);
            for(int i = 0; i < 15; i++){
                for(int j = 0; j < 15; j++){
                    EditorGUI.DrawRect(new Rect(lastRect.xMin + (i * 30), lastRect.yMax + (j * 30), 30, 30), Color.black);
                    if(!obstacleArray[i,j]){
                        EditorGUI.DrawRect(new Rect(lastRect.xMin +(i * 30) + 1, lastRect.yMax + (j * 30) + 1, 28, 28), Color.white);
                    }
                    
                }
            }
            EditorGUI.DrawRect(PlayerRect, Color.yellow);
            EditorGUI.DrawRect(new Rect(lastRect.xMin +(obstacleBaitPoint.vector2IntValue.x * 30) + 1, lastRect.yMax + (obstacleBaitPoint.vector2IntValue.y * 30) + 1, 28, 28), Color.green);

            if(Event.current.type == EventType.MouseDown) {
                var mouse = Event.current.mousePosition;
                var mouseRelative = mouse;//new Vector2(mouse.x - lastRect.xMin, mouse.y - lastRect.yMin);
                for(int i = 0; i < 15; i++){
                    for(int j = 0; j < 15; j++){
                        if(new Rect(lastRect.xMin + (i * 30), lastRect.yMax + (j * 30), 30, 30).Contains(mouseRelative)){
                            if(Event.current.button == 0){
                                obstacleArray[i,j] = !obstacleArray[i,j];
                                var lineProperty = obstacleLinesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("line");
                                lineProperty.GetArrayElementAtIndex(j).boolValue = obstacleArray[i, j];
                                Event.current.Use();
                            } else {
                                obstacleBaitPoint.vector2IntValue = new Vector2Int(i, j);
                                Event.current.Use();
                            }
                            break;
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();


            /*for(int i = 0; i < obstacleLinesProperty.arraySize; i++){
                var lineProperty = obstacleLinesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("lines").FindPropertyRelative("line");
                for(int j = 0; j < lineProperty.arraySize; j++){
                    lineProperty.GetArrayElementAtIndex(j).boolValue = obstacleArray[i, j];
                }
            }*/
            
        }
    }

    private void ObstacleDeleteButton(int index){
        GUI.color = Color.red;
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16)))
        {
            selectedObstacleIndex = -1;
            obstacleArrayProperty.RemoveFromVariableArrayAt(index);
            

            EditorApplication.delayCall += delegate
            {
                InitObstacleNames();
            };
        }
        GUI.color = Color.white;
    }

    private void NewObstacleGUI(){
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        NewObstacleNameGUI();
        //tempObstacle.prefab = EditorGUILayout.ObjectField(new GUIContent("Obstacle: "), tempObstacle.prefab, typeof(GameObject), false) as GameObject;
        //tempObstacle.detailedPrefab = EditorGUILayout.ObjectField(new GUIContent("Detailed obstacle: "), tempObstacle.detailedPrefab, typeof(GameObject), false) as GameObject;
        NewObstacleAddButtonGUI();
        EditorGUILayout.EndVertical();
    }
    bool uniqueNameError = false;
    private void NewObstacleNameGUI(){
        
        EditorGUI.BeginChangeCheck();
        tempObstacle.name = EditorGUILayout.TextField(new GUIContent("Name: "), tempObstacle.name);

        if(EditorGUI.EndChangeCheck())
        {
            uniqueNameError = System.Array.FindIndex(levelDatabase.obstacles, x => x.name == tempObstacle.name) == -1;
        }
    }

    private void NewObstacleAddButtonGUI(){
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Obstacle"))
        {
            if(uniqueNameError){
                if(tempObstacle.name != null){
                    
                    obstacleArrayProperty.arraySize++;

                    SerializedProperty newObstacleProperty = obstacleArrayProperty.GetArrayElementAtIndex(obstacleArrayProperty.arraySize - 1);
                    newObstacleProperty.FindPropertyRelative("name").stringValue = tempObstacle.name;
                    newObstacleProperty.FindPropertyRelative("prefab").objectReferenceValue = tempObstacle.prefab;
                    newObstacleProperty.FindPropertyRelative("detailedPrefab").objectReferenceValue = tempObstacle.detailedPrefab;

                    tempObstacle = new Obstacle();

                    GUI.FocusControl(null);

                    EditorApplication.delayCall += delegate
                    {
                        InitObstacleNames();
                    };
                    
                } else {
                    EditorUtility.DisplayDialog("Cannot create this obstacle", "You should name this obstacle", "Ok");
                }
            } else {
                EditorUtility.DisplayDialog("Cannot create this obstacle", "Name '" + tempObstacle.name + "' is not unique", "Ok");
            }
            
        }
        EditorGUILayout.EndHorizontal();
    }
}
