using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [Header("Physics Configuration")]
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float velocityThreshold = 0.01f;
    [SerializeField] private Vector2 boardSize = new Vector2(4, 4); // Match visual board size
    [SerializeField] private float pocketRadius = 0.16f; // Scale pocket radius proportionally
    
    // Physics Configuration - Pockets in corners (scaled for larger board)
    private List<Vector2> pocketPositions = new List<Vector2>
    {
        new Vector2(-2f, -2f),  // Bottom-left
        new Vector2(2f, -2f),   // Bottom-right
        new Vector2(-2f, 2f),   // Top-left
        new Vector2(2f, 2f)     // Top-right
    };

    public List<CircleObject> objects = new List<CircleObject>();
    
    public event Action<CircleObject> OnObjectPotted;
    public event Action OnSimulationStopped;

    private void FixedUpdate()
    {
        if (objects.Count == 0) return;

        float deltaTime = Time.fixedDeltaTime;

        // Update positions
        foreach (var obj in objects)
        {
            obj.Position += obj.Velocity * deltaTime;
        }

        // Apply friction
        foreach (var obj in objects)
        {
            obj.Velocity *= (1 - friction * deltaTime);
            if (obj.Velocity.magnitude < velocityThreshold)
                obj.Velocity = Vector2.zero;
        }

        // Handle collisions between objects
        for (int i = 0; i < objects.Count; i++)
        {
            for (int j = i + 1; j < objects.Count; j++)
            {
                ResolveCollision(objects[i], objects[j]);
            }
        }

        // Handle wall collisions
        foreach (var obj in objects)
        {
            HandleWallCollisions(obj);
        }

        // Check for potting
        CheckForPotting();

        // Check if all objects have stopped
        CheckSimulationEnd();
    }

    private void HandleWallCollisions(CircleObject obj)
    {
        if (obj.Position.x - obj.Radius < -boardSize.x / 2)
        {
            obj.Position = new Vector2(-boardSize.x / 2 + obj.Radius, obj.Position.y);
            obj.Velocity = new Vector2(-obj.Velocity.x, obj.Velocity.y);
        }
        if (obj.Position.x + obj.Radius > boardSize.x / 2)
        {
            obj.Position = new Vector2(boardSize.x / 2 - obj.Radius, obj.Position.y);
            obj.Velocity = new Vector2(-obj.Velocity.x, obj.Velocity.y);
        }
        if (obj.Position.y - obj.Radius < -boardSize.y / 2)
        {
            obj.Position = new Vector2(obj.Position.x, -boardSize.y / 2 + obj.Radius);
            obj.Velocity = new Vector2(obj.Velocity.x, -obj.Velocity.y);
        }
        if (obj.Position.y + obj.Radius > boardSize.y / 2)
        {
            obj.Position = new Vector2(obj.Position.x, boardSize.y / 2 - obj.Radius);
            obj.Velocity = new Vector2(obj.Velocity.x, -obj.Velocity.y);
        }
    }

    private void CheckForPotting()
    {
        List<CircleObject> potted = new List<CircleObject>();
        foreach (var obj in objects)
        {
            foreach (var pocket in pocketPositions)
            {
                if (Vector2.Distance(obj.Position, pocket) < pocketRadius)
                {
                    potted.Add(obj);
                    break;
                }
            }
        }
        
        foreach (var obj in potted)
        {
            objects.Remove(obj);
            OnObjectPotted?.Invoke(obj);
        }
    }

    private void CheckSimulationEnd()
    {
        bool allStopped = true;
        foreach (var obj in objects)
        {
            if (obj.Velocity.magnitude > velocityThreshold)
            {
                allStopped = false;
                break;
            }
        }
        
        if (allStopped)
        {
            OnSimulationStopped?.Invoke();
        }
    }

    private void ResolveCollision(CircleObject a, CircleObject b)
    {
        Vector2 delta = a.Position - b.Position;
        float distance = delta.magnitude;
        
        if (distance < a.Radius + b.Radius && distance > 0)
        {
            Vector2 normal = delta / distance;
            float overlap = (a.Radius + b.Radius - distance) / 2;
            
            a.Position += normal * overlap;
            b.Position -= normal * overlap;

            Vector2 relativeVelocity = a.Velocity - b.Velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);
            
            if (velocityAlongNormal > 0)
                return;

            float m1 = a.Mass;
            float m2 = b.Mass;
            float factor = 2 / (m1 + m2);
            
            a.Velocity -= factor * m2 * velocityAlongNormal * normal;
            b.Velocity += factor * m1 * velocityAlongNormal * normal;
        }
    }

    public void SetStrikerPosition(Vector2 position)
    {
        var striker = objects.Find(obj => obj.IsStriker);
        if (striker != null)
        {
            striker.Position = position;
            striker.Velocity = Vector2.zero;
        }
    }

    public void FlickStriker(Vector2 velocity)
    {
        var striker = objects.Find(obj => obj.IsStriker);
        if (striker != null)
        {
            striker.Velocity = velocity;
        }
    }

    public void AddObject(CircleObject obj)
    {
        objects.Add(obj);
    }

    public void RemoveObject(CircleObject obj)
    {
        objects.Remove(obj);
    }

    public void ClearObjects()
    {
        objects.Clear();
    }

    // Return a coin to the board (for foul handling)
    public void ReturnCoinToBoard(CircleObject coin, Vector2 position)
    {
        coin.Position = position;
        coin.Velocity = Vector2.zero;
        objects.Add(coin);
    }
} 