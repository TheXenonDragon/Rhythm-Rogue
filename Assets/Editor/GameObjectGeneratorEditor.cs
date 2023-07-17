using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObjectGenerator))]
public class GameObjectGeneratorEditor : Editor
{
    float spacing = 10f;

    public override void OnInspectorGUI()
    {
        GameObjectGenerator gameObjectGenerator = (GameObjectGenerator)target;

        //  Tile Size
        EditorGUILayout.LabelField("Tile Size");
        gameObjectGenerator.tileSize = EditorGUILayout.FloatField("Tile Size", gameObjectGenerator.tileSize);

        //  Field of View
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Field Of View");
        gameObjectGenerator.fieldOfViewPrefab = (GameObject)EditorGUILayout.ObjectField("Field of View Prefab", gameObjectGenerator.fieldOfViewPrefab, typeof(GameObject), true);
        gameObjectGenerator.fieldOfViewWidth = EditorGUILayout.IntField("Width", gameObjectGenerator.fieldOfViewWidth);
        gameObjectGenerator.fieldOfViewHeight = EditorGUILayout.IntField("Height", gameObjectGenerator.fieldOfViewHeight); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Field of View Objects")){
            gameObjectGenerator.CreateFieldOfView();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Field of View Objects")){
            gameObjectGenerator.DeleteFieldOfView();
        }


        //  Wall
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Wall");
        gameObjectGenerator.wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", gameObjectGenerator.wallPrefab, typeof(GameObject), true);
        gameObjectGenerator.wallCount = EditorGUILayout.IntField("Wall Count", gameObjectGenerator.wallCount); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Wall Objects")){
            gameObjectGenerator.CreateWalls();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Wall Objects")){
            gameObjectGenerator.DeleteWalls();
        }


        //  Floor Tile
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Floor Tile");
        gameObjectGenerator.floorTilePrefab = (GameObject)EditorGUILayout.ObjectField("Floor Tile Prefab", gameObjectGenerator.floorTilePrefab, typeof(GameObject), true);
        gameObjectGenerator.floorTileCount = EditorGUILayout.IntField("Floor Tile Count", gameObjectGenerator.floorTileCount); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Floor Tile Objects")){
            gameObjectGenerator.CreateFloorTiles();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Floor Tile Objects")){
            gameObjectGenerator.DeleteFloorTiles();
        }


        //  Box
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Box");
        gameObjectGenerator.boxPrefab = (GameObject)EditorGUILayout.ObjectField("Box Prefab", gameObjectGenerator.boxPrefab, typeof(GameObject), true);
        gameObjectGenerator.boxCount = EditorGUILayout.IntField("Box Count", gameObjectGenerator.boxCount); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Box Objects")){
            gameObjectGenerator.CreateBoxes();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Box Objects")){
            gameObjectGenerator.DeleteBoxes();
        }


        //  Crate
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Crate");
        gameObjectGenerator.cratePrefab = (GameObject)EditorGUILayout.ObjectField("Crate Prefab", gameObjectGenerator.cratePrefab, typeof(GameObject), true);
        gameObjectGenerator.crateCount = EditorGUILayout.IntField("Crate Count", gameObjectGenerator.crateCount); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Crate Objects")){
            gameObjectGenerator.CreateCrates();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Crate Objects")){
            gameObjectGenerator.DeleteCrates();
        }


        //  Chest
        EditorGUILayout.Space(spacing * 2f);
        EditorGUILayout.LabelField("Chest");
        gameObjectGenerator.chestPrefab = (GameObject)EditorGUILayout.ObjectField("Chest Prefab", gameObjectGenerator.chestPrefab, typeof(GameObject), true);
        gameObjectGenerator.chestCount = EditorGUILayout.IntField("Chest Count", gameObjectGenerator.chestCount); 

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Create Chest Objects")){
            gameObjectGenerator.CreateChests();
        }

        EditorGUILayout.Space(spacing);
        if(GUILayout.Button("Delete Chest Objects")){
            gameObjectGenerator.DeleteChests();
        }
    }
}
