using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text turnIndicatorText;
    [SerializeField] private Text player1ScoreText;
    [SerializeField] private Text player2ScoreText;
    [SerializeField] private Text pinkCoinStatusText;
    [SerializeField] private Text winMessageText;
    [SerializeField] private Button restartButton;

    [Header("Visual GameObjects")]
    [SerializeField] private GameObject boardObject;
    [SerializeField] private GameObject[] pocketObjects = new GameObject[4];
    [SerializeField] private GameObject whiteCoinPrefab;
    [SerializeField] private GameObject blackCoinPrefab;
    [SerializeField] private GameObject pinkCoinPrefab;
    [SerializeField] private GameObject strikerPrefab;

    [Header("Input Settings")]
    [SerializeField] private float maxForceMultiplier = 5.0f;
    [SerializeField] private LineRenderer forceIndicator;

    private IServer server;
    private MockServer mockServer;

    // Visual representations
    private Dictionary<CoinType, List<GameObject>> coinVisuals;
    private GameObject strikerVisual;
    
    // Input handling
    private bool isDraggingStriker = false;
    private Vector3 strikerDragStart;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        
        // Initialize visual dictionaries
        coinVisuals = new Dictionary<CoinType, List<GameObject>>
        {
            { CoinType.White, new List<GameObject>() },
            { CoinType.Black, new List<GameObject>() },
            { CoinType.Pink, new List<GameObject>() }
        };
    }

    private void Start()
    {
        // Get or create MockServer
        mockServer = GetComponent<MockServer>();
        if (mockServer == null)
        {
            mockServer = gameObject.AddComponent<MockServer>();
        }
        server = mockServer;

        // Set up UI
        SetupUI();

        // Initialize the game
        StartGame();
    }

    private void SetupUI()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(false);
        }
    }

    public void StartGame()
    {
        // Initialize server
        server.InitializeGame();

        // Create visual representations
        CreateVisualBoard();
        CreateVisualCoins();

        // Update UI
        UpdateUI();
    }

    private void CreateVisualBoard()
    {
        // Create board if not assigned
        if (boardObject == null)
        {
            boardObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            boardObject.name = "Board";
            boardObject.transform.localScale = Vector3.one * 8f; // Scale to make board much larger
            
            // Add a material to make it visible
            var renderer = boardObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = new Color(0.8f, 0.6f, 0.4f); // Brown board color
        }

        // Create pockets if not assigned - positioned at the actual corners of the board
        if (pocketObjects[0] == null)
        {
            float pocketScale = 0.64f; // 2x larger pockets
            
            Vector2[] pocketPositions = { 
                new Vector2(-2f, -2f), // Bottom-left corner
                new Vector2(2f, -2f),  // Bottom-right corner
                new Vector2(-2f, 2f),  // Top-left corner
                new Vector2(2f, 2f)    // Top-right corner
            };

            for (int i = 0; i < 4; i++)
            {
                pocketObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pocketObjects[i].name = $"Pocket_{i}";
                pocketObjects[i].transform.position = new Vector3(pocketPositions[i].x, pocketPositions[i].y, -0.1f);
                pocketObjects[i].transform.localScale = Vector3.one * pocketScale; // 2x larger pockets
                
                var renderer = pocketObjects[i].GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = Color.black;
            }
        }
    }

    private void CreateVisualCoins()
    {
        // Clear existing visuals
        ClearVisualCoins();

        // Get coin positions from server
        var coinPositions = server.GetCoinPositions();

        foreach (var coin in coinPositions)
        {
            GameObject coinPrefab = GetCoinPrefab(coin.Type);
            if (coinPrefab != null)
            {
                GameObject coinVisual = Instantiate(coinPrefab);
                coinVisual.transform.position = new Vector3(coin.Position.x, coin.Position.y, 0);
                coinVisual.name = $"{coin.Type}Coin";

                coinVisuals[coin.Type].Add(coinVisual);
            }
        }

        // Create striker visual
        if (strikerPrefab != null)
        {
            strikerVisual = Instantiate(strikerPrefab);
            strikerVisual.name = "Striker";
            
            // Position striker at baseline
            Vector2 strikerPos = GetStrikerBaselinePosition(server.GetCurrentTurn());
            strikerVisual.transform.position = new Vector3(strikerPos.x, strikerPos.y, 0);
        }
    }

    private GameObject GetCoinPrefab(CoinType coinType)
    {
        switch (coinType)
        {
            case CoinType.White:
                return whiteCoinPrefab ?? CreateDefaultCoin(Color.white);
            case CoinType.Black:
                return blackCoinPrefab ?? CreateDefaultCoin(Color.black);
            case CoinType.Pink:
                return pinkCoinPrefab ?? CreateDefaultCoin(Color.magenta);
            default:
                return null;
        }
    }

    private GameObject CreateDefaultCoin(Color color)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.transform.localScale = Vector3.one * 0.2f; // Scale coins for larger board
        
        var renderer = coin.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = color;
        
        return coin;
    }

    private Vector2 GetStrikerBaselinePosition(int playerId)
    {
        float y = (playerId == 1) ? -1.6f : 1.6f; // Scale baseline positions for larger board
        return new Vector2(0, y);
    }

    private void ClearVisualCoins()
    {
        foreach (var coinList in coinVisuals.Values)
        {
            foreach (var coin in coinList)
            {
                if (coin != null)
                    DestroyImmediate(coin);
            }
            coinList.Clear();
        }

        if (strikerVisual != null)
        {
            DestroyImmediate(strikerVisual);
            strikerVisual = null;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateVisualPositions();
    }

    private void HandleInput()
    {
        if (server.GetGameState() == GameState.Win)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Check if clicking on striker
            if (strikerVisual != null)
            {
                float distance = Vector3.Distance(mouseWorldPos, strikerVisual.transform.position);
                if (distance < 0.1f) // Within striker bounds
                {
                    isDraggingStriker = true;
                    strikerDragStart = mouseWorldPos;
                    
                    if (forceIndicator != null)
                    {
                        forceIndicator.enabled = true;
                    }
                }
            }
        }

        if (isDraggingStriker)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Update striker position (constrained to baseline)
            Vector2 constrainedPos = ConstrainStrikerPosition(mouseWorldPos);
            strikerVisual.transform.position = new Vector3(constrainedPos.x, constrainedPos.y, 0);

            // Update force indicator
            if (forceIndicator != null)
            {
                Vector3 forceDirection = strikerDragStart - mouseWorldPos;
                forceIndicator.SetPosition(0, strikerVisual.transform.position);
                forceIndicator.SetPosition(1, strikerVisual.transform.position + forceDirection);
            }

            if (Input.GetMouseButtonUp(0))
            {
                // Calculate and apply force
                Vector2 forceVector = (strikerDragStart - mouseWorldPos) * maxForceMultiplier;
                Vector2 strikerPosition = new Vector2(strikerVisual.transform.position.x, strikerVisual.transform.position.y);

                // Send move to server
                bool success = server.MakeMove(server.GetCurrentTurn(), strikerPosition, forceVector);
                
                if (!success)
                {
                    Debug.LogWarning("Move failed!");
                }

                isDraggingStriker = false;
                if (forceIndicator != null)
                {
                    forceIndicator.enabled = false;
                }
            }
        }
    }

    private Vector2 ConstrainStrikerPosition(Vector3 worldPos)
    {
        int currentPlayer = server.GetCurrentTurn();
        float baselineY = (currentPlayer == 1) ? -1.6f : 1.6f;
        
        float constrainedX = Mathf.Clamp(worldPos.x, -1.6f, 1.6f); // Scale constraint bounds for larger board
        return new Vector2(constrainedX, baselineY);
    }

    private void UpdateVisualPositions()
    {
        // This will be called when simulation ends, but for now we'll update every frame
        // In the final implementation, this should be called only when needed
        if (Time.frameCount % 5 == 0) // Update every 5 frames for performance
        {
            var coinPositions = server.GetCoinPositions();
            
            // Clear and recreate coin visuals (simplified approach)
            // In a more optimized version, we'd track and update individual coins
            foreach (var coinList in coinVisuals.Values)
            {
                foreach (var coin in coinList)
                {
                    if (coin != null)
                        coin.SetActive(false);
                }
            }

            // Show coins that are still on board
            foreach (var coin in coinPositions)
            {
                var availableVisual = coinVisuals[coin.Type].Find(c => c != null && !c.activeInHierarchy);
                if (availableVisual != null)
                {
                    availableVisual.SetActive(true);
                    availableVisual.transform.position = new Vector3(coin.Position.x, coin.Position.y, 0);
                }
            }

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        int currentTurn = server.GetCurrentTurn();
        GameState gameState = server.GetGameState();
        
        // Update turn indicator
        if (turnIndicatorText != null)
        {
            if (gameState == GameState.Win)
            {
                turnIndicatorText.text = $"Player {currentTurn} Wins!";
                if (winMessageText != null)
                {
                    winMessageText.text = $"Player {currentTurn} Wins!";
                    winMessageText.gameObject.SetActive(true);
                }
            }
            else
            {
                turnIndicatorText.text = $"Player {currentTurn}'s Turn";
            }
        }

        // Update score displays
        var coinPositions = server.GetCoinPositions();
        int whiteCoinCount = coinPositions.FindAll(c => c.Type == CoinType.White).Count;
        int blackCoinCount = coinPositions.FindAll(c => c.Type == CoinType.Black).Count;
        bool pinkOnBoard = coinPositions.Exists(c => c.Type == CoinType.Pink);

        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"Player 1 (White): {9 - whiteCoinCount}/9";
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"Player 2 (Black): {9 - blackCoinCount}/9";
        }

        if (pinkCoinStatusText != null)
        {
            pinkCoinStatusText.text = pinkOnBoard ? "Pink: On Board" : "Pink: Potted";
        }
    }

    public void RestartGame()
    {
        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(false);
        }

        StartGame();
    }

    private void OnDestroy()
    {
        ClearVisualCoins();
    }
} 