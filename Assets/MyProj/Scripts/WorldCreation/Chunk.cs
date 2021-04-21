using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk: MonoBehaviour
{

    public Vector2 Seed;
    
    #region Mesh Generation
    
    private FastNoise _noise = new FastNoise();
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    #endregion

    private ChunkData _chunkData;
    
    
    #region Create Chunk Mesh
    
    public void Init(int chunkSize)
    {
        _mesh = new Mesh();
        _chunkData = new ChunkData();
        GetComponent<MeshFilter>().mesh = _mesh;
        CreateShape(chunkSize);
        UpdateMesh();
        
    }
    private void CreateShape(int chunkSize)
    {
        _vertices  = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        float lowestPoint = 300000;
        float highestPointPoint = -300000;
        
        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {

                float heightMap;
                
                //Make noise maps
                float simplex1 =
                    _noise.GetSimplex((Seed.x + x + transform.position.x) * .2F, 
                        (Seed.y + z + transform.position.z) * .2F) * 20;
                    
                float simplex2 = _noise.GetSimplex((Seed.x + x + transform.position.x) * 3f, 
                                     (Seed.y + z + transform.position.z) * 3f) * 5 * 
                                 (_noise.GetSimplex((Seed.x + x + transform.position.x) * .3f,
                                     (Seed.y + z + transform.position.z) * .3f) + .5f);
                float simplex3 =
                    _noise.GetSimplex((Seed.x + x + transform.position.x) * .2F, 
                        (Seed.y + z + transform.position.z) * .2F) * 12;
                
                //Combine noisemaps to create fun terrain
                heightMap = simplex1 + simplex2 + simplex3;
               
                
                if (heightMap < lowestPoint)
                {
                    lowestPoint = heightMap;
                }
                if (heightMap > highestPointPoint)
                {
                    highestPointPoint = heightMap;
                }
                
                _vertices[i] = new Vector3(x, heightMap, z) ;
                i++;
            }
        }
        
        //Base terrain type on highest poiint and the lowest point
        var distHighLow = highestPointPoint - lowestPoint;
        if (distHighLow > 2.5F)
        {
            this._chunkData.TerrainType = TerrainTypes.Hilly;
        }
        else
        {
            this._chunkData.TerrainType = TerrainTypes.Flat;
        }

        
        _triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                _triangles[0 + tris] = vert + 0;
                _triangles[1 + tris] = vert + chunkSize + 1;
                _triangles[2 + tris] = vert + 1;
                _triangles[3 + tris] = vert + 1;
                _triangles[4 + tris] = vert + chunkSize + 1;
                _triangles[5 + tris] = vert + chunkSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }
        
        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                i++;
            }
        }
    }
    
    /// <summary>
    /// Recalculate all mesh values
    /// </summary>
    void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = _mesh;
    }
    

    #endregion

}

[Serializable]
public class ChunkData
{
    public TerrainTypes TerrainType;
    public GameObject Parent;
    public bool ChunkActivated;
    public List<ObjectData> treesOnChunk = new List<ObjectData>();
    public List<ObjectData> propsOnChunk = new List<ObjectData>();
}

[Serializable]
public class ObjectData
{
    public int ObjectIndex;
    public Vector3 Position;
    public Quaternion Rotation;
}

public enum TerrainTypes
{
    Flat,
    Hilly
}
