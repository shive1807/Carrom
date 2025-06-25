using UnityEngine;

/// <summary>
/// Simple collision detector for testing high-speed collisions
/// Logs when collisions occur and reports missed collisions
/// </summary>
public class HighSpeedCollisionDetector : MonoBehaviour
{
    private float lastCollisionTime = 0f;
    private Vector2 initialVelocity;
    private float initialSpeed;
    private bool hasCollided = false;
    
    void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            initialVelocity = rb.linearVelocity;
            initialSpeed = initialVelocity.magnitude;
        }
        
        // Auto-destroy this component after 5 seconds
        Destroy(this, 5f);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        hasCollided = true;
        lastCollisionTime = Time.time;
        
        Debug.Log($"HIGH-SPEED COLLISION DETECTED: {gameObject.name} hit {collision.gameObject.name}");
        Debug.Log($"  Collision speed: {initialSpeed:F2} units/sec");
        Debug.Log($"  Collision point: {collision.contacts[0].point}");
        Debug.Log($"  Relative velocity: {collision.relativeVelocity.magnitude:F2}");
        
        // Change color to indicate collision
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }
    
    void OnDestroy()
    {
        if (!hasCollided)
        {
            Debug.LogWarning($"HIGH-SPEED COLLISION MISSED: {gameObject.name} never collided!");
            Debug.LogWarning($"  Initial speed was: {initialSpeed:F2} units/sec");
            Debug.LogWarning("  This indicates physics tunneling - objects passed through each other!");
            Debug.LogWarning("  Consider: Smaller timestep, more solver iterations, or smaller objects");
        }
        else
        {
            Debug.Log($"HIGH-SPEED COLLISION SUCCESS: {gameObject.name} collided properly");
        }
    }
} 