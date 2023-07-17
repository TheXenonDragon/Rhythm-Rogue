using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectGenerator : MonoBehaviour
{
    public float tileSize;

    //  Field of View
    public GameObject fieldOfViewPrefab;
    public int fieldOfViewWidth;
    public int fieldOfViewHeight;
    private GameObject[,] fieldOfViewGrid;
    private GameObject fieldOfViewHolder;
    private string fieldOfViewHolderName = "Field of View Holder";

    //  Walls
    public GameObject wallPrefab;
    public int wallCount;
    private GameObject wallHolder;
    private string wallHolderName = "Wall Holder";

    //  Floor Tiles
    public GameObject floorTilePrefab;
    public int floorTileCount;
    private GameObject floorTileHolder;
    private string floorTileHolderName = "Floor Tile Holder";

    //  Box
    public GameObject boxPrefab;
    public int boxCount;
    private GameObject boxHolder;
    private string boxHolderName = "Box Holder";

    //  Crate
    public GameObject cratePrefab;
    public int crateCount;
    private GameObject crateHolder;
    private string crateHolderName = "Crate Holder";

    //  Chest
    public GameObject chestPrefab;
    public int chestCount;
    private GameObject chestHolder;
    private string chestHolderName = "Chest Holder";



    //  Field of View Methods
    public void CreateFieldOfView(){
        fieldOfViewGrid = new GameObject[fieldOfViewWidth, fieldOfViewHeight];
        fieldOfViewHolder = new GameObject(fieldOfViewHolderName);
        fieldOfViewHolder.transform.parent = transform;

        for(int x = 0; x < fieldOfViewWidth; x++){
            for(int y = 0; y < fieldOfViewWidth; y++){
                fieldOfViewGrid[x, y] = Instantiate<GameObject>(fieldOfViewPrefab, new Vector3(x * tileSize, 0f, y * tileSize), fieldOfViewPrefab.transform.rotation);
                fieldOfViewGrid[x, y].transform.parent = fieldOfViewHolder.transform;
            }
        }
    }

    public void DeleteFieldOfView(){
        if(fieldOfViewHolder == null){
            //  If fieldOfViewHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == fieldOfViewHolderName){
                    fieldOfViewHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If fieldOfViewHolder was unable to be found, exit the method.
            if(fieldOfViewHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < fieldOfViewHolder.transform.childCount; j++){
            DestroyImmediate(fieldOfViewHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(fieldOfViewHolder);
        fieldOfViewGrid = null;
    }


    //  Wall Methods
    public void CreateWalls(){
        wallHolder = new GameObject(wallHolderName);
        wallHolder.transform.parent = transform;

        for(int i = 0; i < wallCount; i++){
            Instantiate<GameObject>(wallPrefab, Vector3.zero, wallPrefab.transform.rotation).transform.parent = wallHolder.transform;
        }
    }

    public void DeleteWalls(){
        if(wallHolder == null){
            //  If wallHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == wallHolderName){
                    wallHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If wallHolder was unable to be found, exit the method.
            if(wallHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < wallHolder.transform.childCount; j++){
            DestroyImmediate(wallHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(wallHolder);
    }


    //  Floor Tile Methods
    public void CreateFloorTiles(){
        floorTileHolder = new GameObject(floorTileHolderName);
        floorTileHolder.transform.parent = transform;

        for(int i = 0; i < floorTileCount; i++){
            Instantiate<GameObject>(floorTilePrefab, Vector3.zero, floorTilePrefab.transform.rotation).transform.parent = floorTileHolder.transform;
        }
    }

    public void DeleteFloorTiles(){
        if(floorTileHolder == null){
            //  If floorTileHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == floorTileHolderName){
                    floorTileHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If floorTileHolder was unable to be found, exit the method.
            if(floorTileHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < floorTileHolder.transform.childCount; j++){
            DestroyImmediate(floorTileHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(floorTileHolder);
    }


    //  Box Methods
    public void CreateBoxes(){
        boxHolder = new GameObject(boxHolderName);
        boxHolder.transform.parent = transform;

        for(int i = 0; i < boxCount; i++){
            Instantiate<GameObject>(boxPrefab, Vector3.zero, boxPrefab.transform.rotation).transform.parent = boxHolder.transform;
        }
    }

    public void DeleteBoxes(){
        if(boxHolder == null){
            //  If boxHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == boxHolderName){
                    boxHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If boxHolder was unable to be found, exit the method.
            if(boxHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < boxHolder.transform.childCount; j++){
            DestroyImmediate(boxHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(boxHolder);
    }


    //  Crate Methods
    public void CreateCrates(){
        crateHolder = new GameObject(crateHolderName);
        crateHolder.transform.parent = transform;

        for(int i = 0; i < crateCount; i++){
            Instantiate<GameObject>(cratePrefab, Vector3.zero, cratePrefab.transform.rotation).transform.parent = crateHolder.transform;
        }
    }

    public void DeleteCrates(){
        if(crateHolder == null){
            //  If crateHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == crateHolderName){
                    crateHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If crateHolder was unable to be found, exit the method.
            if(crateHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < crateHolder.transform.childCount; j++){
            DestroyImmediate(crateHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(crateHolder);
    }


    //  Chest Methods
    public void CreateChests(){
        chestHolder = new GameObject(chestHolderName);
        chestHolder.transform.parent = transform;

        for(int i = 0; i < wallCount; i++){
            Instantiate<GameObject>(chestPrefab, Vector3.zero, chestPrefab.transform.rotation).transform.parent = chestHolder.transform;
        }
    }

    public void DeleteChests(){
        if(chestHolder == null){
            //  If chestHolder is null, search for it under this gameObject.
            for(int i = 0; i < transform.childCount; i++){
                if(transform.GetChild(i).gameObject.name == chestHolderName){
                    chestHolder = transform.GetChild(i).gameObject;
                    break;
                }
            }

            //  If chestHolder was unable to be found, exit the method.
            if(chestHolder == null){
                return;
            }
        }

        //  Delete all children.
        for(int j = 0; j < chestHolder.transform.childCount; j++){
            DestroyImmediate(chestHolder.transform.GetChild(j).gameObject);
        }

        //  Destroy holders
        DestroyImmediate(chestHolder);
    }
}
