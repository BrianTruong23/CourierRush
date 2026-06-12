using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    
    float acceleration = 5f;
    float deceleration = 5f;
    float maxSpeed = 6f;

    float turnSpeed = 200f;

    Rigidbody2D rb;

    Vector2 moveInput;

    float currentSpeed = 0f;

    public AudioSource audioSource;
    public AudioClip bumpSound; 
    float lastBumpSoundTime;
    float bumpSoundCoolTime = 0.25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        float moveAmount = moveInput.y;
        if (moveAmount != 0)
        {
            currentSpeed += moveAmount * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        float turnAmount = -moveInput.x * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + turnAmount);

        rb.linearVelocity = transform.up * currentSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Collision"))
        {
            return; 
        }

        if (Time.time < lastBumpSoundTime + bumpSoundCoolTime)
        {
            return;
        }

        if (audioSource != null && bumpSound != null)
        {
            audioSource.PlayOneShot(bumpSound);

            lastBumpSoundTime = Time.time;
        }




    }
}
