

using UnityEngine;
using System.Collections.Generic;

public class WorldSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject[] chunkPrefabs;
    public float chunkSize = 10f;
    public int radius = 2;

    
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshGrid();
    }

    // Update is called once per frame
    void Update()
    {
        RefreshGrid();

    }

    void RefreshGrid()
    {
        Vector2Int playerCoord = WorldToGrid(player.position);

        HashSet<Vector2Int> desired = new HashSet<Vector2Int>();
        for (int x = playerCoord.x - radius; x <= playerCoord.x + radius; x++)
        {
            for (int y = playerCoord.y - radius; y <= playerCoord.y + radius; y++)
            {
                desired.Add(new Vector2Int(x, y));
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in activeChunks)
        {
            if (!desired.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (Vector2Int coord in toRemove)
        {
            Destroy(activeChunks[coord]);
            activeChunks.Remove(coord);
        }

        foreach (Vector2Int coord in desired)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                PlaceChunk(coord);
            }
        }
    }

    int PickCompatiblePrefab(Vector2Int coord)
    {
        ChunkPorts required = ChunkPorts.None;
        ChunkPorts forbidden = ChunkPorts.None;

        if (activeChunks.TryGetValue(coord + Vector2Int.up, out GameObject northChunk))
        {
            ChunkData northData = northChunk.GetComponent<ChunkData>();
            if (northData.HasPort(ChunkPorts.South))
            {
                required |= ChunkPorts.North;
            } else
            {
                forbidden |= ChunkPorts.North;
            }
        };


        if (activeChunks.TryGetValue(coord + Vector2Int.down, out GameObject southChunk))
        {
            ChunkData southData = southChunk.GetComponent<ChunkData>();
            if (southData.HasPort(ChunkPorts.North))
            {
                required |= ChunkPorts.South;
            } else
            {
                forbidden |= ChunkPorts.South;
            }
        };

        if (activeChunks.TryGetValue(coord + Vector2Int.right, out GameObject eastChunk))
        {
            ChunkData eastData = eastChunk.GetComponent<ChunkData>();
            if (eastData.HasPort(ChunkPorts.West))
            {
                required |= ChunkPorts.East;
            } else
            {
                forbidden |= ChunkPorts.East;
            }
        };

        if (activeChunks.TryGetValue(coord + Vector2Int.left, out GameObject westChunk))
        {
            ChunkData westData = westChunk.GetComponent<ChunkData>();
            if (westData.HasPort(ChunkPorts.East))
            {
                required |= ChunkPorts.West;
            } else
            {
                forbidden |= ChunkPorts.West;
            }
        };

        List<int> candidates = new List<int>();

        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            ChunkData chunkData = chunkPrefabs[i].GetComponent<ChunkData>();
            if (chunkData == null)
            {
                continue;
            }

            if ((chunkData.ports & required) != required)
            {
                continue;
            }

            if ((chunkData.ports & forbidden) != ChunkPorts.None)
            {
                continue;
            }

            candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No compatible chunk found!");
            return Random.Range(0, chunkPrefabs.Length);
        }
        
        return candidates[Random.Range(0, candidates.Count)];

    }

    void PlaceChunk(Vector2Int coord)
    {
        int randomIndex = PickCompatiblePrefab(coord);
        GameObject chunk = Instantiate(chunkPrefabs[randomIndex], GridToWorld(coord), Quaternion.identity);
        activeChunks.Add(coord, chunk);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }

    Vector3 GridToWorld(Vector2Int coord)
    {
        return new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);
    }

    public Dictionary<Vector2Int, GameObject> GetActiveChunks()
    {
        return activeChunks;
    }

    public bool IsChunkActive(Vector2Int coord)
    {
        return activeChunks.ContainsKey(coord);
    }

}




// using UnityEngine;
// using System.Collections.Generic;

// public class WorldSpawner : MonoBehaviour
// {
//     public Transform player;
//     public GameObject[] chunkPrefabs;
//     public float chunkSize = 10f;
//     public int radius = 2;

    
//     private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         RefreshGrid();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         RefreshGrid();
//     }

//     void RefreshGrid()
//     {
//         Vector2Int playerCoord = WorldToGrid(player.position);

//         HashSet<Vector2Int> desired = new HashSet<Vector2Int>();
//         for (int x = playerCoord.x - radius; x <= playerCoord.x + radius; x++)
//         {
//             for (int y = playerCoord.y - radius; y <= playerCoord.y + radius; y++)
//             {
//                 desired.Add(new Vector2Int(x, y));
//             }
//         }

//         List<Vector2Int> toRemove = new List<Vector2Int>();
        
//         foreach (var kvp in activeChunks)
//         {
//             if (!desired.Contains(kvp.Key))
//             {
//                 toRemove.Add(kvp.Key);
//             }
//         }

//         foreach (Vector2Int coord in toRemove)
//         {
//             Destroy(activeChunks[coord]);
//             activeChunks.Remove(coord);
//         }

//         foreach (Vector2Int coord in desired)
//         {
//             if(!activeChunks.ContainsKey(coord))
//             {
//                 PlaceChunk(coord);
//             }
//         }
//     }

//     int PickCompatiblePrefab(Vector2Int coord)
//     {
//         ChunkPorts required = ChunkPorts.None;
//         ChunkPorts forbidden = ChunkPorts.None;

//         if (activeChunks.TryGetValue(coord + Vector2Int.up, out GameObject northChunk))
//         {
//             ChunkData northData = northChunk.GetComponent<ChunkData>();
//             if (northData.HasPort(ChunkPorts.South))
//             {
//                 required |= ChunkPorts.North;
//             } else
//             {
//                 forbidden |= ChunkPorts.North;
//             }
//         }

//         if (activeChunks.TryGetValue(coord + Vector2Int.down, out GameObject southChunk))
//         {
//             ChunkData southData = southChunk.GetComponent<ChunkData>();
//             if (southData.HasPort(ChunkPorts.North))
//             {
//                 required |= ChunkPorts.South;
//             } else
//             {
//                 forbidden |= ChunkPorts.South;
//             }
//         }

//         if (activeChunks.TryGetValue(coord + Vector2Int.right, out GameObject eastChunk))
//         {
//             ChunkData eastData = eastChunk.GetComponent<ChunkData>();
//             if (eastData.HasPort(ChunkPorts.West))
//             {
//                 required |= ChunkPorts.East;
//             } else
//             {
//                 forbidden |= ChunkPorts.East;
//             }
//         }

//         if (activeChunks.TryGetValue(coord + Vector2Int.left, out GameObject westChunk))
//         {
//             ChunkData westData = westChunk.GetComponent<ChunkData>();
//             if (westData.HasPort(ChunkPorts.East))
//             {
//                 required |= ChunkPorts.West;
//             } else
//             {
//                 forbidden |= ChunkPorts.West;
//             }
//         }

//         List<int> candidates = new List<int>();

//         for(int i = 0; i < chunkPrefabs.Length; i++)
//         {
//             ChunkData chunkData = chunkPrefabs[i].GetComponent<ChunkData>();
//             if (chunkData == null)
//             {
//                 continue;
//             }

//             if ((chunkData.ports & required) != required)
//             {
//                 continue;
//             }

//             if ((chunkData.ports & forbidden) != ChunkPorts.None)
//             {
//                 continue;
//             }

//             candidates.Add(i);
//         }

//         if (candidates.Count == 0)
//         {
//             Debug.LogWarning("No compatible chunk found!");
//             return Random.Range(0, chunkPrefabs.Length);
//         }
        
//         return candidates[Random.Range(0, candidates.Count)];
//     }


//     void PlaceChunk(Vector2Int coord)
//     {
//         int randomIndex = PickCompatiblePrefab(coord);
//         GameObject chunk = Instantiate(chunkPrefabs[randomIndex], GridToWorld(coord), Quaternion.identity);
//         activeChunks.Add(coord, chunk);
//     }

//     public Vector2Int WorldToGrid(Vector3 worldPos)
//     {
//         return new Vector2Int(
//             Mathf.FloorToInt(worldPos.x / chunkSize),
//             Mathf.FloorToInt(worldPos.y / chunkSize)
//         );
//     }

//     Vector3 GridToWorld(Vector2Int coord)
//     {
//         return new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);
//     }

//     public Dictionary<Vector2Int, GameObject> GetActiveChunks()
//     {
//         return activeChunks;
//     }

//     public bool IsChunkActive(Vector2Int coord)
//     {
//         return activeChunks.ContainsKey(coord);
//     }
// }
