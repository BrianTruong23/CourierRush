using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using TMPro;

public class DeliveryManager : MonoBehaviour
{
    enum DeliveryState
    {
        WaitingForPickup,
        CarryingPackage
    }

    public Transform player;
    public GameObject objectiveMarker;
    public WorldSpawner worldSpawner; 

    public Sprite pickupSprite;
    public Sprite deliverySprite; 

    public float collectDistance = 1f;
    public float minSpawnDistance = 10f; 

    public float obstacleCheckRadius = 0.75f;
    public LayerMask obstacleLayer;

    public TMP_Text timerText;
    public float deliveryTimeLimit = 20f;

    public TMP_Text DeliveryScore;
    public float deliveryScore = 0f; 

    public TMP_Text InstructionText;
    public float instructionDisplayTime = 3f; 

    float currentDeliveryTime;
    bool timerRunning; 

    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip deliverySound;


    DeliveryState currentState;
    SpriteRenderer objectiveRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectiveRenderer = objectiveMarker.GetComponent<SpriteRenderer>();
        currentState = DeliveryState.WaitingForPickup;
        UpdateDeliveryScore();
        Invoke(nameof(SpawnPickup), 0.2f);

        ShowInstructions();
        
    }

    // Update is called once per frame
    void Update()
    {

        if (player == null || objectiveMarker == null || worldSpawner == null)
        {
            return;
        }

        UpdateDeliveryTimer();

        
        float distance = Vector3.Distance(player.position, objectiveMarker.transform.position);

        if (distance < collectDistance)
        {
            ReachCurrentObjective();
        }

        Vector2Int objectiveCoord = worldSpawner.WorldToGrid(objectiveMarker.transform.position);

        if (!worldSpawner.GetActiveChunks().ContainsKey(objectiveCoord))
        {
            RespawnCurrentObjective();
        }


    }

    void ShowInstructions()
    {
        if (InstructionText != null)
        {
            InstructionText.SetText(
                "Rules are simple:\n" +
                "1. Pick up the trophy, then deliver it to the target before time runs out.\n" +
                "2. Avoid cones!"
            );

            InstructionText.gameObject.SetActive(true);
            InstructionText.color = Color.red;
            Invoke(nameof(HideInstructions), instructionDisplayTime);
        };
    }

    void HideInstructions()
    {
        if (InstructionText != null)
        {
            InstructionText.gameObject.SetActive(false);
        };
    }

    void ReachCurrentObjective()
    {
        if (currentState == DeliveryState.WaitingForPickup)
        {
            PickUpPackage();
        }
        else if (currentState == DeliveryState.CarryingPackage)
        {
            CompleteDelivery();
        }
    }

      void PickUpPackage()
      {
          PlaySound(pickupSound);
          currentState = DeliveryState.CarryingPackage;
          
          StartDeliveryTimer();
          SpawnDeliveryTarget();
      }

      void CompleteDelivery()
      {
          PlaySound(deliverySound);
          StopDeliveryTimer();
   

          deliveryScore++;
          UpdateDeliveryScore();
          currentState = DeliveryState.WaitingForPickup;
          SpawnPickup();

          
      }

      void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }
        audioSource.PlayOneShot(clip);
        
    }

      void SpawnPickup()
      {
          objectiveRenderer.sprite = pickupSprite;
          MoveObjectiveToRandomRoadPosition(false);
          objectiveMarker.SetActive(true);
      }

      void SpawnDeliveryTarget()
      {
          objectiveRenderer.sprite = deliverySprite;
          MoveObjectiveToRandomRoadPosition(false);
          objectiveMarker.SetActive(true);
      }

       void RespawnCurrentObjective()
      {
          if (currentState == DeliveryState.WaitingForPickup)
          {
              SpawnPickup();
          }
          else
          {
              SpawnDeliveryTarget();
          }
      }

    bool IsValidObjectivePosition(Vector3 position)
        {
            if (Vector3.Distance(player.position, position) < minSpawnDistance)
            {
                return false;
            }

            Collider2D obstacle = Physics2D.OverlapCircle(position, obstacleCheckRadius, obstacleLayer);

            if (obstacle != null)
            {
                return false;
            }

            return true;
        }


      void MoveObjectiveToRandomRoadPosition(bool ignoreDistance)
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

                        if (IsValidObjectivePosition(worldPosition))
                        {
                            candidates.Add(worldPosition);
                        }
                }
              }
          }

          if (candidates.Count == 0)
          {
              Debug.LogWarning("No valid objective position found!");
              return;
          }

          objectiveMarker.transform.position = candidates[Random.Range(0, candidates.Count)];
      }


    void UpdateDeliveryScore()
    {
        if (DeliveryScore != null)
        {
            DeliveryScore.text = "Deliveries: " + deliveryScore.ToString();

        }
    }

      void StartDeliveryTimer()
    {
        currentDeliveryTime = deliveryTimeLimit;
        timerRunning = true;
        UpdateTimerText();
    }

    void StopDeliveryTimer()
    {
        timerRunning = false;
        currentDeliveryTime = 0f;
        UpdateTimerText();
    }

    void UpdateDeliveryTimer()
    {
        if (!timerRunning)
        {
            return;
        }

        currentDeliveryTime -= Time.deltaTime;

        if (currentDeliveryTime <= 0f)
        {
            currentDeliveryTime = 0f;
            timerRunning = false;
            FailDelivery();
        }

        UpdateTimerText();
    }

    void FailDelivery()
    {
        currentState = DeliveryState.WaitingForPickup;
        SpawnPickup();
    }

    void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        if (!timerRunning)
        {
            timerText.text = "Time: --";
            return;
            
        }
       
       timerText.text = "Time: " + Mathf.CeilToInt(currentDeliveryTime).ToString();
    }


}
