using UnityEngine;

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
    
    [Header("Generated Components")]
    [SerializeField] private CarromInputController inputController;
    [SerializeField] private GameObject striker;
    [SerializeField] private GameObject boardCenter;
    
    [Header("Demo Controls")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.D;
    
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
            
            // Position at bottom of board
            striker.transform.position = new Vector3(0, -1.6f, 0);
            
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
                
                var collider2D = striker.AddComponent<CircleCollider2D>();
                collider2D.radius = 0.5f;
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
            
            // Position in a small circle around center
            float angle = i * 120f * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 0.3f, Mathf.Sin(angle) * 0.3f, 0);
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
                
                var collider2D = coin.AddComponent<CircleCollider2D>();
                collider2D.radius = 0.5f;
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