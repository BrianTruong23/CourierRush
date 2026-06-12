using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject target;
    public WorldSpawner worldSpawner;

    float minSpawnDistance = 10f;
    float collectDistance = 1f;

    enum DeliveryState
    {
        WaitingForPickup,
        CarryingPackage
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(SpawnTargetInitial), 0.1f);
    }

    void SpawnTargetInitial()
    {
        SpawnTarget(true);
    }

    void SpawnTarget(bool ignoreDistance)
    {
        List<Vector3> candidates = new List<Vector3>();

        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            GameObject chunkObject = kvp.Value;

            if (chunkObject == null)
            {
                continue;
            }

            if (!ignoreDistance && Vector3.Distance(player.position, chunkObject.transform.position) < minSpawnDistance)
            {
                continue;
            }

            Tilemap road = chunkObject.transform.Find("Road")?.GetComponent<Tilemap>();

            if (road == null)
            {
                continue;
            }
            
            foreach (Vector3Int cellPosition in road.cellBounds.allPositionsWithin)
            {
                if (road.HasTile(cellPosition))
                {
                    Vector3 worldPosition = road.GetCellCenterWorld(cellPosition);
                    candidates.Add(worldPosition);
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No valid target spawn position found!");
            return;
        }

        target.transform.position = candidates[Random.Range(0, candidates.Count)];
        target.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || player == null || worldSpawner == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, target.transform.position);
        
        if (distance < collectDistance)
        {
            SpawnTarget(false);
            return;
        }

        Vector2Int targetCoord = worldSpawner.WorldToGrid(target.transform.position);

        if (!worldSpawner.IsChunkActive(targetCoord))
        {
            SpawnTarget(false);
        }
    }
}
