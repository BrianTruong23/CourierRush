using UnityEngine;
  using UnityEngine.Tilemaps;
  using System.Collections.Generic;

  public class ConeSpawner : MonoBehaviour
  {
      public GameObject conePrefab;
      public Transform player;
      public WorldSpawner worldSpawner;

      public int conesPerChunk = 1;
      public float minDistanceFromPlayer = 8f;
      public float minDistanceBetweenCones = 3f;

      private HashSet<Vector2Int> spawnedChunks = new HashSet<Vector2Int>();
      private List<Vector3> spawnedConePositions = new List<Vector3>();

      void Start()
      {
          Invoke(nameof(SpawnConesForActiveChunks), 0.1f);
      }

      void Update()
      {
          SpawnConesForActiveChunks();
      }

      void SpawnConesForActiveChunks()
      {
          if (conePrefab == null || player == null || worldSpawner == null)
          {
              return;
          }

          RemoveInactiveChunkRecords();

          foreach (var kvp in worldSpawner.GetActiveChunks())
          {
              Vector2Int chunkCoord = kvp.Key;
              GameObject chunkObject = kvp.Value;

              if (spawnedChunks.Contains(chunkCoord))
              {
                  continue;
              }

              SpawnConesInChunk(chunkObject);
              spawnedChunks.Add(chunkCoord);
          }
      }

      void SpawnConesInChunk(GameObject chunkObject)
      {
          if (chunkObject == null)
          {
              return;
          }

          Tilemap road = chunkObject.transform.Find("Road")?.GetComponent<Tilemap>();

          if (road == null)
          {
              return;
          }

          List<Vector3> roadPositions = GetRoadPositions(road);
          int spawned = 0;

          while (spawned < conesPerChunk && roadPositions.Count > 0)
          {
              int randomIndex = Random.Range(0, roadPositions.Count);
              Vector3 spawnPosition = roadPositions[randomIndex];
              roadPositions.RemoveAt(randomIndex);

              if (!IsValidSpawnPosition(spawnPosition))
              {
                  continue;
              }

              Instantiate(conePrefab, spawnPosition, Quaternion.identity, chunkObject.transform);
              spawnedConePositions.Add(spawnPosition);
              spawned++;
          }
      }

      List<Vector3> GetRoadPositions(Tilemap road)
      {
          List<Vector3> positions = new List<Vector3>();

          foreach (Vector3Int cellPosition in road.cellBounds.allPositionsWithin)
          {
              if (road.HasTile(cellPosition))
              {
                  Vector3 worldPosition = road.GetCellCenterWorld(cellPosition);
                  positions.Add(worldPosition);
              }
          }

          return positions;
      }

      bool IsValidSpawnPosition(Vector3 position)
      {
          if (Vector3.Distance(player.position, position) < minDistanceFromPlayer)
          {
              return false;
          }

          foreach (Vector3 conePosition in spawnedConePositions)
          {
              if (Vector3.Distance(conePosition, position) < minDistanceBetweenCones)
              {
                  return false;
              }
          }

          return true;
      }

      void RemoveInactiveChunkRecords()
      {
          List<Vector2Int> chunksToRemove = new List<Vector2Int>();

          foreach (Vector2Int coord in spawnedChunks)
          {
              if (!worldSpawner.IsChunkActive(coord))
              {
                  chunksToRemove.Add(coord);
              }
          }

          foreach (Vector2Int coord in chunksToRemove)
          {
              spawnedChunks.Remove(coord);
          }
      }
  }
