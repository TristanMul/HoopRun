using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.Core;

public class GridButton{

    public int width, height;
    public int gridWidth, gridHeight;
    public int cellWidth, cellHeight;

    public int HORIZONTAL_SPACE = 32;
    public int VERTICAL_SPACE = 32;

    public int MIN_CELL_WIDTH = 64;
    public int MAX_CELL_WIDTH = 128;
    public int MIN_CELL_HEIGHT = 64;
    public int MAX_CELL_HEIGHT = 128;

    private Rect[,] grid;

    private float uiSize;

    public bool[,] buttonValues;

    public void Init(int width, int height)
    {
        this.width = width;
        this.height = height;
        gridWidth = 512;
        gridHeight = 1536;

        cellWidth = (gridWidth - HORIZONTAL_SPACE * (width + 1)) / width;
        cellWidth = Mathf.Clamp(cellWidth, MIN_CELL_WIDTH, MAX_CELL_WIDTH);

        cellHeight = (gridHeight - VERTICAL_SPACE * (height + 1)) / height;
        cellHeight = Mathf.Clamp(cellHeight, MIN_CELL_HEIGHT, MAX_CELL_HEIGHT);

        buttonValues = new bool[width,height];

        grid = GetCells();

    }

    private Rect[,] GetCells(){
        Rect[,] tempGrid = new Rect[width, height];
        var leftSpace = (gridWidth - cellWidth * width - (width - 1) * HORIZONTAL_SPACE) / 2;
        var topSpace = (gridHeight - cellHeight * height - (height - 1) * VERTICAL_SPACE) / 2;

        var tempLeftPos = leftSpace;
        for(int i = 0; i < width; i++){
            var tempTopPos = topSpace;
            for(int j = 0; j < height; j++){
                tempGrid[i, j] = new Rect(tempLeftPos, tempTopPos, cellWidth, cellHeight);
                tempTopPos+= cellHeight + VERTICAL_SPACE;
            }
            tempLeftPos += cellWidth + HORIZONTAL_SPACE;
        }
        
        return tempGrid;
    }

    public void DrawGridButtons(ref bool button){
        button = false;

        Event e = Event.current;
        Rect zoneRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Height(Screen.width), GUILayout.MinHeight(Screen.width));
        Vector2 mousePosition = e.mousePosition;

        float widthDiff = zoneRect.width / gridWidth;
        float heightDiff = zoneRect.height / gridHeight;

        Texture2D whiteButtonTexture = new Texture2D(cellWidth, cellHeight, TextureFormat.ARGB32, false);
        Texture2D blackButtonTexture = new Texture2D(cellWidth, cellHeight, TextureFormat.ARGB32, false);
        for(int x = 0; x < cellWidth; x++){
            for(int y = 0; y < cellHeight; y++){
                whiteButtonTexture.SetPixel(x, y, Color.white);
                blackButtonTexture.SetPixel(x, y, Color.black);
            }
        }
        blackButtonTexture.Apply();
        whiteButtonTexture.Apply();

        for(int i = 0; i < width; i++){
            for(int j = 0; j < height; j++){
                bool isSelected = buttonValues[i,j];
                Texture2D tempButtonTexture = isSelected ? blackButtonTexture : whiteButtonTexture;

                Rect finishRect = new Rect(
                    zoneRect.x + (grid[i, j].x * widthDiff), 
                    zoneRect.y + grid[i, j].y * heightDiff, 
                    cellWidth * widthDiff, 
                    cellHeight * heightDiff);

                GUI.DrawTexture(finishRect, tempButtonTexture);
                
                if(finishRect.Contains(mousePosition)){
                    HandleUtility.Repaint();
                    
                    if(e.type == EventType.MouseDown){
                        buttonValues[i,j] = !buttonValues[i,j];
                        button = true;
                    }
                }
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        
    }
}

