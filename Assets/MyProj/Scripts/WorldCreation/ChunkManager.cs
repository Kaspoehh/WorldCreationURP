using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Cameras;
using Random = UnityEngine.Random;

public class ChunkManager : MonoBehaviour
{
    [Header("ChunkGeneration")] 
    [SerializeField] private Vector2Reference seed = default(Vector2Reference);
    [SerializeField] private Material[] terrainMaterialsFull;
    [SerializeField] private int chunkSize;
    //Chunk render distance
    [SerializeField] private IntReference chunkDist = default(IntReference);

    [Header("World Object")] 
    [SerializeField] private WorldPropManager propManager;
    
    [Header("Buildings")]
    [SerializeField] private int villageIndex;
    [SerializeField] private int campIndex;
    [SerializeField] private int castleIndex;

    [Header("Player")] 
    [SerializeField]private Transform player;
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Saving And Loading")] 
    [SerializeField] private bool newWorld;

    [SerializeField] private StringReference saveName = default(StringReference);
    [SerializeField] private StringReference worldName = default(StringReference);
    
    
    public Dictionary<Vector2, GameObject> RenderedChunks = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, ChunkData> ChunkDict = new Dictionary<Vector2, ChunkData>();

    FastNoise _noise = new FastNoise();

    public int ChunkSize => chunkSize;

    private void Start()
    {
        if (newWorld)
        {
            for (int z = -3; z < 3; z++)
            {
                for (int x = -3; x < 3; x++)
                {
                    CreateChunk(new Vector2(x * chunkSize, z * chunkSize), true);
                }
            }
        }

        if (!newWorld)
        {
            LoadWorld();
        }
    }

    private void Update()
    {
        LoadChunk();
    }

    /// <summary>
    /// Create an new chunk 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="createFullyNewChunk"></param>
    /// <return>"chunkGameObject" So you can parent castles, trees etc when you load the chunk</returns>
    private GameObject CreateChunk(Vector2 pos, bool createFullyNewChunk)
    {
        GameObject chunkGameObject = new GameObject();
        chunkGameObject.transform.position = new Vector3(pos.x, 0, pos.y);
        RenderedChunks.Add(pos, chunkGameObject);
        chunkGameObject.name = "Chunk: " + pos;
        chunkGameObject.layer = 6;
        var chunk = chunkGameObject.AddComponent<Chunk>();
        chunk.Seed = seed.Value;
        chunkGameObject.GetComponent<MeshRenderer>().materials = terrainMaterialsFull;
        chunk.Init(chunkSize);
        
        if (createFullyNewChunk)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.Parent = chunkGameObject;
            ChunkDict.Add(pos, chunkData);
            CreateTrees(pos, chunkData.Parent.transform, chunkData);
            chunkData.ChunkActivated = true;
        }
        
        return chunkGameObject;
    }

    /// <summary>
    /// Fill the chunk with trees
    /// </summary>
    /// <param name="chunkData"></param>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    private void CreateTrees(Vector2 pos, Transform parent, ChunkData chunkData)
    {
        Debug.Log("Creating trees");
        System.Random randSeed = new System.Random((int)pos.x * 11200 + (int)pos.y);

        float simplex = _noise.GetSimplex((int)pos.x * .8f, (int)pos.y * .8f);

        if (simplex > 0)
        {
            simplex *= 2f;

            int treeCount = Mathf.FloorToInt((float) randSeed.NextDouble() * (chunkSize - 1) * simplex);

            for (int i = 0; i < treeCount; i++)
            {
                int xPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.x;
                int zPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.y;

                int y = 200;

                var randTree = Random.Range(0, propManager.TreesForInWorld.Count);

                var tree = Instantiate(propManager.TreesForInWorld[randTree].PropPrefab, new Vector3(xPos, y, zPos),
                    Quaternion.Euler(PickRandomRotation()));

                RaycastHit hit;
                
                if (Physics.Raycast(tree.transform.position, tree.transform.TransformDirection(Vector3.down), out hit,
                    10000))
                {
                    var trans = tree.transform;
                    trans.position = hit.point;
                    var randomRotation = Random.Range(0, 360);
                    trans.rotation = Quaternion.Euler(trans.rotation.x, randomRotation,
                        trans.rotation.z);
                    tree.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    
                    // if(tree.transform.rotation)
                    
                    SaveObjectOnChunk(tree, randTree ,chunkData, false);
                    
                    tree.transform.SetParent(parent);
                }
            }
        }
        CreateBuildings(pos, parent, chunkData);
    }
    

    // /// <summary>
    // /// Fill the chunk with grass
    // /// </summary>
    // /// <param name="pos"></param>
    // /// <param name="parent"></param>
    // private void CreateGrass(Vector2 pos, Transform parent, ChunkData chunkData )
    // {    
    //     
    //     System.Random randSeed = new System.Random((int)pos.x * 10000 + (int)pos.y);
    //
    //     float simplex = _noise.GetSimplex((int)pos.x * .8f, (int)pos.y * .8f);
    //
    //     if (simplex > -0.8F)
    //     {
    //         simplex *= 2f;
    //
    //         int grassCount = Mathf.FloorToInt((float) randSeed.NextDouble() * (chunkSize - 1) * simplex);
    //
    //         for (int i = 0; i < grassCount; i++)
    //         {
    //             int xPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.x;
    //             int zPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.y;
    //
    //             int y = 200;
    //
    //             var randGrass = Random.Range(0, grassPrefabs.Count);
    //
    //             var grass = Instantiate(grassPrefabs[randGrass], new Vector3(xPos, y, zPos),
    //                 Quaternion.Euler(PickRandomRotation()));
    //
    //             RaycastHit hit;
    //             
    //             if (Physics.Raycast(grass.transform.position, grass.transform.TransformDirection(Vector3.down), out hit,
    //                 10000))
    //             {
    //                 grass.transform.position = hit.point;
    //                 var randomRotation = Random.Range(0, 360);
    //                 grass.transform.rotation = Quaternion.Euler(grass.transform.rotation.x, randomRotation,
    //                     grass.transform.rotation.z);
    //                 grass.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    //                 
    //                 SaveObjectOnChunk(grass, grassPrefabs[randGrass] ,chunkData);
    //                 
    //                 grass.transform.SetParent(parent);
    //             }
    //         }
    //     }
    //
    // }

    /// <summary>
    /// Pick an building/village to create
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="chunkData"></param>
    private void CreateBuildings(Vector2 pos,Transform parent,ChunkData chunkData )
    {
        if (chunkData.treesOnChunk.Count > 0)
            return;

        var rand = Random.Range(0, 50);

        switch (rand)
        {
            case 1 when chunkData.TerrainType == TerrainTypes.Hilly:
                return;
            case 1:
                CreateBuilding(propManager.PropsForInWorld[castleIndex].PropPrefab ,pos, parent, chunkData, castleIndex);
                break;
            case 2:
            case 3:
            {
                if (chunkData.TerrainType == TerrainTypes.Hilly)
                    return;
                CreateBuilding(propManager.PropsForInWorld[villageIndex].PropPrefab ,pos, parent, chunkData, villageIndex);
                break;
            }
            case 4:
            case 5:
                CreateBuilding(propManager.PropsForInWorld[campIndex].PropPrefab ,pos, parent, chunkData, campIndex);
                break;
        }
    }
    
    /// <summary>
    /// Spawn an village
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="chunkData"></param>
    private void CreateBuilding(GameObject prefab,Vector2 pos,Transform parent, ChunkData chunkData, int index )
    {
        int y = 200;

        var instance = Instantiate(prefab, new Vector3(pos.x, y, pos.y),  Quaternion.identity);
                
        instance.transform.SetParent(parent);

        SaveObjectOnChunk(instance, index, chunkData, true);
    }
   
     /// <summary>
     /// Pick a random rotation
     /// </summary>
     /// <returns></returns>
     private Vector3 PickRandomRotation()
     {
         var rand = Random.Range(0, 360);
         
         return new Vector3(0, rand, 0);
     }

     /// <summary>
     /// Save the object data to the dictionary
     /// </summary>
     /// <param name="objInWorld"></param>
     /// <param name="prefabToSave"></param>
     /// <param name="chunkData"></param>
     private void SaveObjectOnChunk(GameObject objInWorld, int propIndex, ChunkData chunkData, bool isBuilding)
     {
         ObjectData obj = new ObjectData
         {
             ObjectIndex = propIndex,
             Position = objInWorld.transform.position,
             Rotation = objInWorld.transform.rotation
         };

         if (isBuilding)
         {
             chunkData.propsOnChunk.Add(obj);
         }
         else
         {
             chunkData.treesOnChunk.Add(obj);
         }

     }
     
     private Vector2 curChunk = new Vector2(-1, -1);
     
     /// <summary>
     /// Load an chunk
     /// </summary>
     public void LoadChunk()
     {
         var curChunkPosX = RoundDown((int)player.position.x);
         var curChunkPosZ = RoundDown((int)player.position.z);
         
         //entered a new chunk
         if (curChunk.x != curChunkPosX || curChunk.y != curChunkPosZ)
         {
             curChunk.x = curChunkPosX;
             curChunk.y = curChunkPosZ;

             //Create or load the chunks
             for (int i = curChunkPosX - chunkSize * chunkDist.Value; i <= curChunkPosX + chunkSize * chunkDist.Value; i += chunkSize)
             {
                 for (int j = curChunkPosZ - chunkSize * chunkDist.Value; j <= curChunkPosZ + chunkSize * chunkDist.Value; j += chunkSize)
                 {
                     Vector2 cp = new Vector2(i, j);

                     
                     //If chunk not already in dictionary create an fully new chunk
                     if(!ChunkDict.ContainsKey(cp))
                     {
                         Debug.Log("New Chunk");
                         CreateChunk(cp, true);
                     }
                     else
                     {
                         ChunkData chunkData = ChunkDict[cp];
                         
                         //If already activated and vissible in world
                         if (chunkData.ChunkActivated)
                         {
                             Debug.Log("Chunks Already activated");
                         }
                         else
                         {
                             //Create an new chunk (mesh only)
                             var chunkGameObject = CreateChunk(cp, false);
                             
                             chunkData.ChunkActivated = true;
                             
                            //Create the trees
                            for (int k = 0; k < chunkData.treesOnChunk.Count; k++)
                            {
                                var index = chunkData.treesOnChunk[k].ObjectIndex;
                                var obj = Instantiate(propManager.TreesForInWorld[index].PropPrefab,
                                    chunkData.treesOnChunk[k].Position,
                                    chunkData.treesOnChunk[k].Rotation);
                                obj.transform.SetParent(chunkGameObject.transform);
                            }
                            
                            //Create props building etc
                            for (int k = 0; k < chunkData.propsOnChunk.Count; k++)
                            {
                                var index = chunkData.propsOnChunk[k].ObjectIndex;
                                var obj = Instantiate(propManager.PropsForInWorld[index].PropPrefab,
                                    chunkData.propsOnChunk[k].Position,
                                    chunkData.propsOnChunk[k].Rotation);
                                obj.transform.SetParent(chunkGameObject.transform);
                            }
                         }
                     }
                 }
             }
             
             List<Vector2> toUnload = new List<Vector2>();

             //remove chunks that are too far away
             //unload chunks
             foreach(KeyValuePair<Vector2, GameObject> c in RenderedChunks)
             {
                 Vector2 cp = c.Key;
                 if(Mathf.Abs(curChunkPosX - cp.x) > chunkSize * (chunkDist.Value + 3) || 
                    Mathf.Abs(curChunkPosZ - cp.y) > chunkSize * (chunkDist.Value + 3))
                 {
                     RenderedChunks[cp].SetActive(false);
                     toUnload.Add(c.Key);
                 }
             }
             
             //Remove and delete the chunks from the list
             foreach(Vector2 cp in toUnload)
             {
                 GameObject objToDestroy = RenderedChunks[cp].gameObject;
                 
                 RenderedChunks.Remove(cp);
                 
                 Destroy(objToDestroy);
             
                 ChunkDict[cp].ChunkActivated = false;
             }
         }
     }
     
     int RoundDown(int toRound)
     {
         return toRound - toRound % ChunkSize;
     }

     
     /// <summary>
     /// Save the data
     /// </summary>
     /// <param name="quit"></param>
     public void Save(bool quitGame)
     {
         //Save The World
         
         List<ChunkData> chunkDatas = new List<ChunkData>();
         List<Vector2> chunkPositions = new List<Vector2>();

         foreach (var keyValuePair in ChunkDict)
         {
             chunkDatas.Add(keyValuePair.Value);
             chunkPositions.Add(keyValuePair.Key);
         }
         
         SaveObject saveObject = new SaveObject()
         {    
             seed = seed.Value,
             playerPos = player.transform.position,
             ChunkDatas = chunkDatas,
             ChunkDatasPostions = chunkPositions
         };
         
         string json = JsonUtility.ToJson(saveObject, true);
         
         File.WriteAllText(saveName.Value, json);
        
         //Save the world in the existing world list

         if (File.Exists(string.Concat(saveName.Value)))
         {
             //Create the new worlds data
             WorldData worldToSave = new WorldData();

             worldToSave.SavePath = saveName.Value;
             worldToSave.WorldName = worldName.Value;

             if (File.Exists(Application.dataPath + "/Resources/" + "existingWorlds" + ".json"))
             {
                 string saveStringWorlds =
                     File.ReadAllText(Application.dataPath + "/Resources/" + "existingWorlds" + ".json");

                 Worlds worldsSaveCollection = JsonUtility.FromJson<Worlds>(saveStringWorlds);

                 List<WorldData> existingWorldCollection = new List<WorldData>();

                 //Add all the old worlds
                 for (int i = 0; i < worldsSaveCollection.WorldDatas.Count; i++)
                 {
                     existingWorldCollection.Add(worldsSaveCollection.WorldDatas[i]);
                 }

                 bool canAdd = true;
                 //Add the new world to the local list
                 for (int i = 0; i < existingWorldCollection.Count; i++)
                 {
                     if (existingWorldCollection[i].SavePath == worldToSave.SavePath)
                         canAdd = false;
                 }

                 if (canAdd)
                 {
                     existingWorldCollection.Add(worldToSave);
                     Worlds worlds = new Worlds()
                     {
                         WorldDatas = existingWorldCollection
                     };

                     //Write the json file
                     string jsonWorlds = JsonUtility.ToJson(worlds, true);

                     File.WriteAllText(Application.dataPath + "/Resources/" + "existingWorlds" + ".json",
                         jsonWorlds);
                 }
             }
             //If the json is not there
             else
             {
                 Debug.Log("file not there creat ean new one");
             
                 //Create the new worlds data
                 WorldData worldToSaveN = new WorldData();
                 
                 worldToSaveN.SavePath = saveName.Value;
                 worldToSaveN.WorldName = worldName.Value;
             
                 List<WorldData> existingWorldCollection = new List<WorldData>();

                 //Add the new world to the local list
                 existingWorldCollection.Add(worldToSaveN);
             
                 Worlds worlds = new Worlds()
                 {
                     WorldDatas = existingWorldCollection
                 };
             
                 //Write the json file
                 string jsonWorlds = JsonUtility.ToJson(worlds, true);
                 File.WriteAllText(Application.dataPath + "/Resources/" + "existingWorlds" + ".json", jsonWorlds);
             }
         }
         


         if (quitGame)
             SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
     }
     
     /// <summary>
     /// Load the world data
     /// </summary>
     private void LoadWorld()
     {
         if (File.Exists(string.Concat(saveName.Value)))
         {
             string saveString = File.ReadAllText(saveName.Value);
             
             SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);
             
             //Create The Player
             var playerInstace = Instantiate(playerPrefab, saveObject.playerPos, Quaternion.identity);
             
             //Parse the data from the save file
             List<ChunkData> chunkDatas = saveObject.ChunkDatas;
             List<Vector2> chunkPositions = saveObject.ChunkDatasPostions;

             ChunkDict = new Dictionary<Vector2, ChunkData>();
             
             for (int i = 0; i < chunkDatas.Count; i++)
             {
                 chunkDatas[i].ChunkActivated = false;
                 ChunkDict.Add(chunkPositions[i], chunkDatas[i]);
             }

             seed.Value = saveObject.seed;
            
             this.player = playerInstace.transform;
             
             //Create the chunks
             LoadChunk();
         }
         else
         {
             Debug.LogError("File doesnt excist yet" + saveName.Value);
         }
     }
}

[Serializable]
public class SaveObject
{
    public Vector2 seed;
    public Vector3 playerPos;
    public List<ChunkData> ChunkDatas = new List<ChunkData>();
    public List<Vector2> ChunkDatasPostions = new List<Vector2>();
}

[Serializable]
public class Worlds
{
    public List<WorldData> WorldDatas = new List<WorldData>();
}

[Serializable]
public class WorldData
{
    public string SavePath;
    public string WorldName;
}
