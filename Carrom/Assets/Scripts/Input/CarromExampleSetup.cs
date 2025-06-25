using UnityEngine;
using System.Linq;

/// <summary>
/// Example setup script for CarromInputController
/// This demonstrates how to set up and integrate the input system
/// Can be used as a template or for quick testing
/// </summary>
public class CarromExampleSetup : MonoBehaviour
{
    [Header("Auto Setup Configuration")]
    [SerializeField] private bool autoCreateComponents = true;
    [SerializeField] private bool useBuiltInPhysics = true;
    [SerializeField] private Material strikerMaterial;
    [SerializeField] private Material coinMaterial;
    [SerializeField] private PhysicsMaterial2D physicsMaterial;
    
    [Header("Generated Components")]
    [SerializeField] private CarromInputController inputController;
    [SerializeField] private GameObject striker;
    [SerializeField] private GameObject boardCenter;
    
    [Header("Demo Controls")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.D;
    [SerializeField] private KeyCode collisionTestKey = KeyCode.C;
    [SerializeField] private KeyCode highSpeedTestKey = KeyCode.H;
    
    void Start()
    {
        if (autoCreateComponents)
        {
            SetupCarromGame();
        }
    }
    
    void Update()
    {
        HandleDemoControls();
    }
    
    /// <summary>
    /// Automatically set up a basic carrom game scene
    /// </summary>
    private void SetupCarromGame()
    {
        Debug.Log("Setting up Carrom game automatically...");
        
        // Create board center
        CreateBoardCenter();
        
        // Create striker
        CreateStriker();
        
        // Create some example coins
        CreateExampleCoins();
        
        // Set up input controller
        SetupInputController();
        
        // Configure camera
        ConfigureCamera();
        
        // Configure physics for high-speed collisions
        ConfigurePhysicsForHighSpeed();
        
        Debug.Log("Carrom game setup complete!");
    }
    
    /// <summary>
    /// Create board center GameObject
    /// </summary>
    private void CreateBoardCenter()
    {
        if (boardCenter == null)
        {
            boardCenter = new GameObject("BoardCenter");
            boardCenter.transform.position = Vector3.zero;
            
            // Add a visual indicator for the center
            GameObject centerIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            centerIndicator.name = "CenterIndicator";
            centerIndicator.transform.SetParent(boardCenter.transform);
            centerIndicator.transform.localPosition = Vector3.zero;
            centerIndicator.transform.localScale = Vector3.one * 0.1f;
            
            var renderer = centerIndicator.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = Color.blue;
        }
    }
    
    /// <summary>
    /// Create striker GameObject with appropriate components
    /// </summary>
    private void CreateStriker()
    {
        if (striker == null)
        {
            striker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            striker.name = "Striker";
            striker.transform.localScale = Vector3.one * 0.28f; // Scaled for larger board
            
            // Position at bottom of board - ensure Z=0 for 2D physics
            striker.transform.position = new Vector3(0, -1.6f, 0f);
            
            // Set up material
            var renderer = striker.GetComponent<Renderer>();
            if (strikerMaterial != null)
            {
                renderer.material = strikerMaterial;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = new Color(0.8f, 0.8f, 0.9f); // Light blue-gray
            }
            
            // Add physics components
            if (useBuiltInPhysics)
            {
                // Add 2D physics components
                var rb2D = striker.AddComponent<Rigidbody2D>();
                rb2D.gravityScale = 0; // No gravity for top-down view
                rb2D.linearDamping = 2f; // Add some drag
                rb2D.angularDamping = 5f;
                rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Best available for 2D
                rb2D.sleepMode = RigidbodySleepMode2D.NeverSleep; // Prevent sleeping during collisions
                
                var collider2D = striker.AddComponent<CircleCollider2D>();
                collider2D.radius = 0.5f; // This will be scaled with the transform
                
                // Create and assign physics material if not provided
                if (physicsMaterial == null)
                {
                    physicsMaterial = CreatePhysicsMaterial();
                }
                collider2D.sharedMaterial = physicsMaterial;
            }
        }
    }
    
    /// <summary>
    /// Create some example coins for testing
    /// </summary>
    private void CreateExampleCoins()
    {
        // Create a few coins in the center area for testing physics
        Color[] coinColors = { Color.white, Color.black, Color.magenta };
        string[] coinNames = { "WhiteCoin", "BlackCoin", "PinkCoin" };
        
        for (int i = 0; i < 3; i++)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = coinNames[i];
            coin.transform.localScale = Vector3.one * 0.2f;
            
            // Position in a small circle around center - ensure Z=0 for 2D physics
            float angle = i * 120f * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 0.3f, Mathf.Sin(angle) * 0.3f, 0f);
            coin.transform.position = position;
            
            // Set up material
            var renderer = coin.GetComponent<Renderer>();
            if (coinMaterial != null)
            {
                renderer.material = coinMaterial;
                renderer.material.color = coinColors[i];
            }
            else
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = coinColors[i];
            }
            
            // Add physics if using built-in system
            if (useBuiltInPhysics)
            {
                var rb2D = coin.AddComponent<Rigidbody2D>();
                rb2D.gravityScale = 0;
                rb2D.linearDamping = 3f;
                rb2D.angularDamping = 5f;
                rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
                rb2D.sleepMode = RigidbodySleepMode2D.NeverSleep; // Prevent sleeping during collisions
                
                var collider2D = coin.AddComponent<CircleCollider2D>();
                collider2D.radius = 0.5f; // This will be scaled with the transform
                
                // Use the same physics material as striker
                if (physicsMaterial == null)
                {
                    physicsMaterial = CreatePhysicsMaterial();
                }
                collider2D.sharedMaterial = physicsMaterial;
            }
        }
    }
    
    /// <summary>
    /// Set up the CarromInputController component
    /// </summary>
    private void SetupInputController()
    {
        // Add input controller to this GameObject
        if (inputController == null)
        {
            inputController = gameObject.AddComponent<CarromInputController>();
        }
        
        // Configure the input controller via reflection to set private fields
        // In practice, you'd set these in the inspector
        var strikerField = typeof(CarromInputController).GetField("striker", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        strikerField?.SetValue(inputController, striker.transform);
        
        var boardCenterField = typeof(CarromInputController).GetField("boardCenter", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        boardCenterField?.SetValue(inputController, boardCenter.transform);
        
        Debug.Log("Input controller configured. You may want to adjust settings in the inspector.");
    }
    
    /// <summary>
    /// Configure camera for optimal carrom game view
    /// </summary>
    private void ConfigureCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Set up orthographic view for top-down carrom
            mainCam.orthographic = true;
            mainCam.orthographicSize = 2.5f; // Show the full board plus some margin
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.transform.rotation = Quaternion.identity;
            
            Debug.Log("Camera configured for top-down carrom view");
        }
    }
    
    /// <summary>
    /// Create a physics material for realistic carrom collisions
    /// </summary>
    private PhysicsMaterial2D CreatePhysicsMaterial()
    {
        PhysicsMaterial2D material = new PhysicsMaterial2D("CarromPhysics");
        material.friction = 0.3f; // Some friction to prevent infinite sliding
        material.bounciness = 0.7f; // Good bounce for carrom coins
        return material;
    }
    
    /// <summary>
    /// Configure Physics2D settings for high-speed collision detection
    /// </summary>
    private void ConfigurePhysicsForHighSpeed()
    {
        // Increase solver iterations for better collision accuracy at high speeds
        Physics2D.velocityIterations = 12; // Default is 8, higher = more accurate
        Physics2D.positionIterations = 6;  // Default is 3, higher = more stable
        
        // Reduce fixed timestep for more frequent physics updates (better for fast collisions)
        Time.fixedDeltaTime = 0.008f; // Default is 0.02f (50Hz), this gives ~125Hz
        
        // Ensure gravity is disabled for top-down carrom
        Physics2D.gravity = Vector2.zero;
        
        // Configure collision detection thresholds
        // Note: These settings affect all physics objects globally
        Physics2D.defaultContactOffset = 0.01f; // How close objects need to be to collide
        
        Debug.Log("Physics configured for high-speed collisions:");
        Debug.Log($"  Velocity Iterations: {Physics2D.velocityIterations}");
        Debug.Log($"  Position Iterations: {Physics2D.positionIterations}");
        Debug.Log($"  Fixed Timestep: {Time.fixedDeltaTime}");
        Debug.Log($"  Contact Offset: {Physics2D.defaultContactOffset}");
    }
    
    /// <summary>
    /// Test collision setup by analyzing all physics objects in the scene
    /// </summary>
    private void TestCollisionSetup()
    {
        Debug.Log("=== COLLISION SETUP TEST ===");
        
        // Find all Rigidbody2D objects
        Rigidbody2D[] allRigidbodies = FindObjectsOfType<Rigidbody2D>();
        Debug.Log($"Found {allRigidbodies.Length} Rigidbody2D objects in scene");
        
        foreach (var rb in allRigidbodies)
        {
            var collider = rb.GetComponent<CircleCollider2D>();
            string info = $"Object: {rb.name}\n" +
                         $"  Position: {rb.transform.position}\n" +
                         $"  Scale: {rb.transform.localScale}\n" +
                         $"  Velocity: {rb.linearVelocity}\n" +
                         $"  Collision Mode: {rb.collisionDetectionMode}\n";
            
            if (collider != null)
            {
                float actualRadius = collider.radius * rb.transform.localScale.x;
                info += $"  Collider Radius: {collider.radius} (actual: {actualRadius:F3})\n" +
                       $"  Physics Material: {(collider.sharedMaterial != null ? collider.sharedMaterial.name : "None")}\n" +
                       $"  Is Trigger: {collider.isTrigger}\n";
            }
            else
            {
                info += "  No CircleCollider2D found!\n";
            }
            
            Debug.Log(info);
        }
        
        // Check Physics2D settings
        Debug.Log($"Physics2D Gravity: {Physics2D.gravity}");
        Debug.Log($"Physics2D Time Scale: {Time.timeScale}");
        Debug.Log($"Fixed Timestep: {Time.fixedDeltaTime}");
        
        // Test for overlapping objects
        var coinObjects = GameObject.FindGameObjectsWithTag("Untagged").Where(go => 
            go.name.Contains("Coin") && go.GetComponent<CircleCollider2D>() != null).ToArray();
        
        Debug.Log($"Found {coinObjects.Length} coin objects for overlap test");
        
        for (int i = 0; i < coinObjects.Length; i++)
        {
            for (int j = i + 1; j < coinObjects.Length; j++)
            {
                var obj1 = coinObjects[i];
                var obj2 = coinObjects[j];
                float distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
                
                var col1 = obj1.GetComponent<CircleCollider2D>();
                var col2 = obj2.GetComponent<CircleCollider2D>();
                
                if (col1 != null && col2 != null)
                {
                    float radius1 = col1.radius * obj1.transform.localScale.x;
                    float radius2 = col2.radius * obj2.transform.localScale.x;
                    float minDistance = radius1 + radius2;
                    
                    if (distance < minDistance)
                    {
                        Debug.LogWarning($"Objects {obj1.name} and {obj2.name} are overlapping! " +
                                       $"Distance: {distance:F3}, Min required: {minDistance:F3}");
                    }
                    else
                    {
                        Debug.Log($"Objects {obj1.name} and {obj2.name} properly spaced. " +
                                $"Distance: {distance:F3}, Min required: {minDistance:F3}");
                    }
                }
            }
        }
        
        Debug.Log("=== END COLLISION TEST ===");
    }
    
    /// <summary>
    /// Test high-speed collision detection by applying fast velocities to objects
    /// </summary>
    private void TestHighSpeedCollisions()
    {
        Debug.Log("=== HIGH-SPEED COLLISION TEST ===");
        
        // Find all coins for testing
        var coinObjects = FindObjectsOfType<Rigidbody2D>().Where(rb => 
            rb.name.Contains("Coin")).ToArray();
        
        if (coinObjects.Length < 2)
        {
            Debug.LogWarning("Need at least 2 coins for high-speed collision test!");
            return;
        }
        
        // Stop all objects first
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Position two coins for head-on collision
        var coin1 = coinObjects[0];
        var coin2 = coinObjects[1];
        
        coin1.transform.position = new Vector3(-0.5f, 0f, 0f);
        coin2.transform.position = new Vector3(0.5f, 0f, 0f);
        
        // Apply high velocities toward each other
        float highSpeed = 15f; // Very fast speed
        coin1.linearVelocity = Vector2.right * highSpeed;
        coin2.linearVelocity = Vector2.left * highSpeed;
        
        Debug.Log($"Applied high-speed velocities ({highSpeed} units/sec) to coins");
        Debug.Log($"Coin1 velocity: {coin1.linearVelocity}");
        Debug.Log($"Coin2 velocity: {coin2.linearVelocity}");
        Debug.Log("Watch for collision... If coins pass through each other, physics settings need adjustment!");
        Debug.Log("You should see the coins bounce off each other. If they pass through, there's a tunneling issue!");
        
        // Log current physics settings for reference
        Debug.Log($"Current physics settings:");
        Debug.Log($"  Fixed Timestep: {Time.fixedDeltaTime}");
        Debug.Log($"  Velocity Iterations: {Physics2D.velocityIterations}");
        Debug.Log($"  Position Iterations: {Physics2D.positionIterations}");
        
        Debug.Log("=== END HIGH-SPEED TEST SETUP ===");
    }
    
    /// <summary>
    /// Handle demo control keys
    /// </summary>
    private void HandleDemoControls()
    {
        // Reset striker position
        if (Input.GetKeyDown(resetKey) && inputController != null)
        {
            inputController.ResetStriker();
            Debug.Log("Striker reset!");
        }
        
        // Toggle debug visualization
        if (Input.GetKeyDown(toggleDebugKey) && inputController != null)
        {
            // Toggle debug via reflection (since it's a private field)
            var debugField = typeof(CarromInputController).GetField("enableDebugLines", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (debugField != null)
            {
                bool currentValue = (bool)debugField.GetValue(inputController);
                debugField.SetValue(inputController, !currentValue);
                Debug.Log($"Debug lines {(!currentValue ? "enabled" : "disabled")}");
            }
        }
        
        // Test collision detection
        if (Input.GetKeyDown(collisionTestKey))
        {
            TestCollisionSetup();
        }
        
        // Test high-speed collisions
        if (Input.GetKeyDown(highSpeedTestKey))
        {
            TestHighSpeedCollisions();
        }
    }
    
    /// <summary>
    /// Display help information in the console
    /// </summary>
    [ContextMenu("Show Help")]
    public void ShowHelp()
    {
        Debug.Log(@"
=== CARROM INPUT CONTROLLER DEMO ===

CONTROLS:
- DRAG LEFT/RIGHT: Aim the striker around the board
- DRAG NEAR STRIKER & PULL BACK: Adjust power (slingshot style)
- RELEASE: Fire the striker

DEMO KEYS:
- R: Reset striker position
- D: Toggle debug visualization
- C: Test collision setup (check console for details)
- H: Test high-speed collisions (watch for tunneling)

HOW TO USE:
1. Click and drag anywhere on screen to aim
2. Click and drag near the striker, then pull backward to charge power
3. Release to fire!

The yellow line shows your aim direction.
The red line shows your power level when charging.

For more information, see CarromInputController_README.md
        ");
    }
} 