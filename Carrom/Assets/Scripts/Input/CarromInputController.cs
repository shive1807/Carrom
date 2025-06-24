using UnityEngine;

/// <summary>
/// Carrom Pool Input Controller
/// Handles aiming by rotating around board center and power adjustment via backward drag
/// Supports both mouse and touch input with visual feedback
/// </summary>
public class CarromInputController : MonoBehaviour
{
    [Header("Striker Configuration")]
    [SerializeField] private Transform striker;
    [SerializeField] private Rigidbody2D strikerRigidbody2D; // For 2D physics
    [SerializeField] private Rigidbody strikerRigidbody3D;   // For 3D physics
    [SerializeField] private bool use2DPhysics = true;
    
    [Header("Board Configuration")]
    [SerializeField] private Transform boardCenter;
    [SerializeField] private float boardRadius = 2f;
    [SerializeField] private float strikerDistanceFromCenter = 1.8f;
    
    [Header("Input Settings")]
    [SerializeField] private float aimingSensitivity = 2f;
    [SerializeField] private float maxPowerDistance = 1f;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float minForceThreshold = 0.1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer powerLineRenderer;
    [SerializeField] private LineRenderer aimLineRenderer;
    [SerializeField] private Color powerLineColor = Color.red;
    [SerializeField] private Color aimLineColor = Color.yellow;
    [SerializeField] private float aimLineLength = 3f;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLines = true;
    [SerializeField] private bool enableDebugLog = false;
    
    // Private variables for input handling
    private Camera mainCamera;
    private Vector3 initialStrikerPosition;
    private Vector3 currentAimDirection;
    private Vector3 dragStartPosition;
    private Vector3 currentDragPosition;
    private bool isDragging = false;
    private bool isAiming = false;
    private bool isPowerAdjusting = false;
    private float currentPowerLevel = 0f;
    
    // Input state management
    private enum InputState
    {
        Idle,
        Aiming,
        PowerAdjusting,
        Firing,
        StrikerMoving
    }
    private InputState currentState = InputState.Idle;
    
    void Start()
    {
        InitializeComponents();
        SetupVisualFeedback();
        SetInitialStrikerPosition();
    }
    
    void Update()
    {
        HandleInput();
        UpdateStrikerState();
        UpdateVisualFeedback();
        DrawDebugLines();
    }
    
    #region Initialization
    
    /// <summary>
    /// Initialize required components and validate setup
    /// </summary>
    private void InitializeComponents()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // Set board center to world origin if not assigned
        if (boardCenter == null)
        {
            GameObject boardCenterObj = new GameObject("BoardCenter");
            boardCenterObj.transform.position = Vector3.zero;
            boardCenter = boardCenterObj.transform;
        }
        
        // Validate striker setup
        if (striker == null)
        {
            Debug.LogError("CarromInputController: Striker Transform not assigned!");
            return;
        }
        
        // Get or add appropriate rigidbody component
        if (use2DPhysics)
        {
            if (strikerRigidbody2D == null)
                strikerRigidbody2D = striker.GetComponent<Rigidbody2D>();
            if (strikerRigidbody2D == null)
                strikerRigidbody2D = striker.gameObject.AddComponent<Rigidbody2D>();
        }
        else
        {
            if (strikerRigidbody3D == null)
                strikerRigidbody3D = striker.GetComponent<Rigidbody>();
            if (strikerRigidbody3D == null)
                strikerRigidbody3D = striker.gameObject.AddComponent<Rigidbody>();
        }
        
        if (enableDebugLog)
            Debug.Log("CarromInputController: Components initialized successfully");
    }
    
    /// <summary>
    /// Setup visual feedback components (line renderers)
    /// </summary>
    private void SetupVisualFeedback()
    {
        // Create power line renderer if not assigned
        if (powerLineRenderer == null)
        {
            GameObject powerLineObj = new GameObject("PowerLine");
            powerLineRenderer = powerLineObj.AddComponent<LineRenderer>();
        }
        
        // Configure power line renderer
        powerLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        powerLineRenderer.startColor = powerLineColor;
        powerLineRenderer.endColor = powerLineColor;
        powerLineRenderer.startWidth = 0.05f;
        powerLineRenderer.endWidth = 0.02f;
        powerLineRenderer.positionCount = 2;
        powerLineRenderer.enabled = false;
        
        // Create aim line renderer if not assigned
        if (aimLineRenderer == null)
        {
            GameObject aimLineObj = new GameObject("AimLine");
            aimLineRenderer = aimLineObj.AddComponent<LineRenderer>();
        }
        
        // Configure aim line renderer
        aimLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        aimLineRenderer.startColor = aimLineColor;
        aimLineRenderer.startWidth = 0.03f;
        aimLineRenderer.endWidth = 0.01f;
        aimLineRenderer.positionCount = 2;
        aimLineRenderer.enabled = false;
    }
    
    /// <summary>
    /// Set the initial position of the striker on the board edge
    /// </summary>
    private void SetInitialStrikerPosition()
    {
        // Position striker at the bottom of the board initially
        Vector3 initialPosition = boardCenter.position + Vector3.down * strikerDistanceFromCenter;
        striker.position = initialPosition;
        initialStrikerPosition = initialPosition;
        currentAimDirection = Vector3.up; // Initially aiming towards center
        
        if (enableDebugLog)
            Debug.Log($"Striker initial position: {initialPosition}");
    }
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// Main input handling method - supports both mouse and touch
    /// </summary>
    private void HandleInput()
    {
        // Prevent input if striker is already moving
        if (IsStrikerMoving())
        {
            currentState = InputState.StrikerMoving;
            return;
        }
        
        bool inputDown = false;
        bool inputHeld = false;
        bool inputUp = false;
        Vector3 inputPosition = Vector3.zero;
        
        // Handle mouse input (for editor/desktop)
        if (Input.GetMouseButtonDown(0))
        {
            inputDown = true;
            inputPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            inputHeld = true;
            inputPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputUp = true;
            inputPosition = Input.mousePosition;
        }
        
        // Handle touch input (for mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    inputDown = true;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    inputHeld = true;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    inputUp = true;
                    break;
            }
        }
        
        // Process input based on current state
        if (inputDown)
        {
            StartDrag(inputPosition);
        }
        else if (inputHeld && isDragging)
        {
            UpdateDrag(inputPosition);
        }
        else if (inputUp && isDragging)
        {
            EndDrag(inputPosition);
        }
    }
    
    /// <summary>
    /// Start drag operation - determine if aiming or power adjusting
    /// </summary>
    private void StartDrag(Vector3 screenPosition)
    {
        Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
        dragStartPosition = worldPosition;
        currentDragPosition = worldPosition;
        isDragging = true;
        
        // Determine if we're close enough to the striker to start power adjustment
        float distanceToStriker = Vector3.Distance(worldPosition, striker.position);
        if (distanceToStriker <= 0.5f) // Threshold for power adjustment
        {
            isPowerAdjusting = true;
            isAiming = false;
            currentState = InputState.PowerAdjusting;
        }
        else
        {
            isAiming = true;
            isPowerAdjusting = false;
            currentState = InputState.Aiming;
        }
        
        if (enableDebugLog)
            Debug.Log($"Drag started - Mode: {(isPowerAdjusting ? "Power" : "Aiming")}, Position: {worldPosition}");
    }
    
    /// <summary>
    /// Update drag operation - handle aiming or power adjustment
    /// </summary>
    private void UpdateDrag(Vector3 screenPosition)
    {
        Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
        currentDragPosition = worldPosition;
        
        if (isAiming)
        {
            UpdateAiming(worldPosition);
        }
        else if (isPowerAdjusting)
        {
            UpdatePowerAdjustment(worldPosition);
        }
    }
    
    /// <summary>
    /// End drag operation - fire striker if power was adjusted
    /// </summary>
    private void EndDrag(Vector3 screenPosition)
    {
        if (isPowerAdjusting && currentPowerLevel > minForceThreshold)
        {
            FireStriker();
        }
        
        // Reset drag state
        isDragging = false;
        isAiming = false;
        isPowerAdjusting = false;
        currentPowerLevel = 0f;
        currentState = InputState.Idle;
        
        // Hide visual feedback
        powerLineRenderer.enabled = false;
        aimLineRenderer.enabled = false;
        
        if (enableDebugLog)
            Debug.Log("Drag ended");
    }
    
    #endregion
    
    #region Aiming System
    
    /// <summary>
    /// Update striker aiming by rotating around board center
    /// </summary>
    private void UpdateAiming(Vector3 dragPosition)
    {
        // Calculate drag delta
        Vector3 dragDelta = dragPosition - dragStartPosition;
        
        // Convert horizontal drag to rotation around board center
        float rotationAmount = dragDelta.x * aimingSensitivity;
        
        // Calculate new position by rotating around board center
        Vector3 directionFromCenter = (striker.position - boardCenter.position).normalized;
        Vector3 rotationAxis = use2DPhysics ? Vector3.forward : Vector3.up;
        
        // Apply rotation
        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, rotationAxis);
        Vector3 newDirection = rotation * directionFromCenter;
        Vector3 newPosition = boardCenter.position + newDirection * strikerDistanceFromCenter;
        
        // Update striker position and aim direction
        striker.position = newPosition;
        currentAimDirection = (boardCenter.position - newPosition).normalized;
        
        if (enableDebugLog)
            Debug.Log($"Aiming - Rotation: {rotationAmount}, Direction: {currentAimDirection}");
    }
    
    #endregion
    
    #region Power System
    
    /// <summary>
    /// Update power level based on backward drag distance
    /// </summary>
    private void UpdatePowerAdjustment(Vector3 dragPosition)
    {
        // Calculate distance from striker in the opposite direction of aim
        Vector3 strikerToCenter = (boardCenter.position - striker.position).normalized;
        Vector3 strikerToDrag = dragPosition - striker.position;
        
        // Project drag vector onto the line opposite to aiming direction
        float projectedDistance = Vector3.Dot(strikerToDrag, -strikerToCenter);
        
        // Clamp power level
        currentPowerLevel = Mathf.Clamp(projectedDistance, 0f, maxPowerDistance);
        
        if (enableDebugLog)
            Debug.Log($"Power Level: {currentPowerLevel} / {maxPowerDistance}");
    }
    
    /// <summary>
    /// Fire the striker with calculated force
    /// </summary>
    private void FireStriker()
    {
        float force = currentPowerLevel * forceMultiplier;
        Vector3 forceDirection = currentAimDirection;
        
        if (use2DPhysics && strikerRigidbody2D != null)
        {
            Vector2 force2D = new Vector2(forceDirection.x, forceDirection.y) * force;
            strikerRigidbody2D.AddForce(force2D, ForceMode2D.Impulse);
        }
        else if (!use2DPhysics && strikerRigidbody3D != null)
        {
            Vector3 force3D = forceDirection * force;
            strikerRigidbody3D.AddForce(force3D, ForceMode.Impulse);
        }
        
        currentState = InputState.Firing;
        
        if (enableDebugLog)
            Debug.Log($"Striker fired - Force: {force}, Direction: {forceDirection}");
    }
    
    #endregion
    
    #region State Management
    
    /// <summary>
    /// Update striker movement state
    /// </summary>
    private void UpdateStrikerState()
    {
        if (currentState == InputState.StrikerMoving || currentState == InputState.Firing)
        {
            if (!IsStrikerMoving())
            {
                currentState = InputState.Idle;
                if (enableDebugLog)
                    Debug.Log("Striker stopped moving");
            }
        }
    }
    
    /// <summary>
    /// Check if striker is currently moving
    /// </summary>
    private bool IsStrikerMoving()
    {
        if (use2DPhysics && strikerRigidbody2D != null)
        {
            return strikerRigidbody2D.linearVelocity.magnitude > 0.1f;
        }
        else if (!use2DPhysics && strikerRigidbody3D != null)
        {
            return strikerRigidbody3D.linearVelocity.magnitude > 0.1f;
        }
        return false;
    }
    
    #endregion
    
    #region Visual Feedback
    
    /// <summary>
    /// Update visual feedback lines (power and aim indicators)
    /// </summary>
    private void UpdateVisualFeedback()
    {
        // Update aim line
        if (isAiming || isPowerAdjusting)
        {
            aimLineRenderer.enabled = true;
            Vector3 aimStart = striker.position;
            Vector3 aimEnd = aimStart + currentAimDirection * aimLineLength;
            
            aimLineRenderer.SetPosition(0, aimStart);
            aimLineRenderer.SetPosition(1, aimEnd);
        }
        else
        {
            aimLineRenderer.enabled = false;
        }
        
        // Update power line
        if (isPowerAdjusting && currentPowerLevel > 0f)
        {
            powerLineRenderer.enabled = true;
            Vector3 powerStart = striker.position;
            Vector3 powerEnd = powerStart - currentAimDirection * currentPowerLevel;
            
            powerLineRenderer.SetPosition(0, powerStart);
            powerLineRenderer.SetPosition(1, powerEnd);
            
            // Change color based on power level
            float powerRatio = currentPowerLevel / maxPowerDistance;
            Color powerColor = Color.Lerp(Color.yellow, Color.red, powerRatio);
            powerLineRenderer.startColor = powerColor;
            powerLineRenderer.endColor   = powerColor;
        }
        else
        {
            powerLineRenderer.enabled = false;
        }
    }
    
    #endregion
    
    #region Debug Support
    
    /// <summary>
    /// Draw debug lines and information
    /// </summary>
    private void DrawDebugLines()
    {
        if (!enableDebugLines) return;
        
        // Draw board center
        Debug.DrawLine(boardCenter.position + Vector3.left * 0.2f, 
                      boardCenter.position + Vector3.right * 0.2f, Color.blue);
        Debug.DrawLine(boardCenter.position + Vector3.down * 0.2f, 
                      boardCenter.position + Vector3.up * 0.2f, Color.blue);
        
        // Draw striker to center line
        Debug.DrawLine(striker.position, boardCenter.position, Color.green);
        
        // Draw aim direction
        Debug.DrawRay(striker.position, currentAimDirection * 2f, Color.yellow);
        
        // Draw power indication
        if (isPowerAdjusting)
        {
            Vector3 powerDirection = -currentAimDirection * currentPowerLevel;
            Debug.DrawRay(striker.position, powerDirection, Color.red);
        }
        
        // Draw board radius
        int segments = 32;
        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = boardCenter.position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * boardRadius;
            Vector3 point2 = boardCenter.position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2)) * boardRadius;
            
            Debug.DrawLine(point1, point2, Color.cyan);
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Convert screen position to world position
    /// </summary>
    private Vector3 ScreenToWorldPosition(Vector3 screenPosition)
    {
        screenPosition.z = mainCamera.WorldToScreenPoint(striker.position).z;
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }
    
    /// <summary>
    /// Reset striker to initial position (can be called externally)
    /// </summary>
    public void ResetStriker()
    {
        striker.position = initialStrikerPosition;
        currentAimDirection = Vector3.up;
        
        // Stop any movement
        if (use2DPhysics && strikerRigidbody2D != null)
        {
            strikerRigidbody2D.linearVelocity = Vector2.zero;
            strikerRigidbody2D.angularVelocity = 0f;
        }
        else if (!use2DPhysics && strikerRigidbody3D != null)
        {
            strikerRigidbody3D.linearVelocity = Vector3.zero;
            strikerRigidbody3D.angularVelocity = Vector3.zero;
        }
        
        currentState = InputState.Idle;
        
        if (enableDebugLog)
            Debug.Log("Striker reset to initial position");
    }
    
    /// <summary>
    /// Get current input state (can be called externally)
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
    
    /// <summary>
    /// Get current power level (0-1 range)
    /// </summary>
    public float GetPowerLevel()
    {
        return currentPowerLevel / maxPowerDistance;
    }
    
    #endregion
} 