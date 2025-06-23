using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MockServer : MonoBehaviour, IServer
{
    [Header("Game Configuration")]
    [SerializeField] private float coinRadius = 0.1f; // Scale coin radius for larger board
    [SerializeField] private float strikerRadius = 0.14f; // Scale striker radius for larger board
    [SerializeField] private float coinMass = 1.0f;
    [SerializeField] private float strikerMass = 1.5f;

    private PhysicsManager physicsManager;
    private GameStateResponse currentGameState;
    private List<CircleObject> pottedCoins;
    private bool isSimulationRunning = false;
    private MoveResult pendingMoveResult;

    // Current turn tracking
    private int currentTurn = 1; // Player 1 starts
    
    private void Awake()
    {
        physicsManager = GetComponent<PhysicsManager>();
        if (physicsManager == null)
        {
            physicsManager = gameObject.AddComponent<PhysicsManager>();
        }

        // Subscribe to physics events
        physicsManager.OnObjectPotted += HandleObjectPotted;
        physicsManager.OnSimulationStopped += HandleSimulationStopped;

        InitializeGameState();
    }

    private void InitializeGameState()
    {
        currentGameState = new GameStateResponse
        {
            CurrentTurn = 1,
            GameState = GameState.Ongoing,
            PottedCoinsCountPerPlayer = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } }
        };
        
        pottedCoins = new List<CircleObject>();
    }

    public void InitializeGame()
    {
        // Clear physics objects
        physicsManager.ClearObjects();
        
        // Reset game state
        InitializeGameState();
        
        // Create pink coin at center
        var pinkCoin = new CircleObject(Vector2.zero, coinRadius, coinMass, false, CoinType.Pink);
        physicsManager.AddObject(pinkCoin);

        // Create inner circle: 6 coins at radius 0.24f (W-B-W-B-W-B) - scaled for larger board
        float innerRadius = 0.24f;
        CoinType[] innerPattern = { CoinType.White, CoinType.Black, CoinType.White, CoinType.Black, CoinType.White, CoinType.Black };
        
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad; // 0°, 60°, 120°, 180°, 240°, 300°
            Vector2 position = new Vector2(
                innerRadius * Mathf.Cos(angle),
                innerRadius * Mathf.Sin(angle)
            );
            
            var coin = new CircleObject(position, coinRadius, coinMass, false, innerPattern[i]);
            physicsManager.AddObject(coin);
        }

        // Create outer circle: 12 coins at radius 0.48f (continuing W-B pattern) - scaled for larger board
        float outerRadius = 0.48f;
        CoinType[] outerPattern = { CoinType.White, CoinType.Black }; // Alternating pattern
        
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad; // 0°, 30°, 60°, ..., 330°
            Vector2 position = new Vector2(
                outerRadius * Mathf.Cos(angle),
                outerRadius * Mathf.Sin(angle)
            );
            
            var coin = new CircleObject(position, coinRadius, coinMass, false, outerPattern[i % 2]);
            physicsManager.AddObject(coin);
        }

        // Create striker
        Vector2 strikerPosition = GetStrikerBaselinePosition(currentTurn);
        var striker = new CircleObject(strikerPosition, strikerRadius, strikerMass, true, CoinType.None);
        physicsManager.AddObject(striker);

        // Update coin positions in game state
        UpdateCoinPositions();
    }

    private Vector2 GetStrikerBaselinePosition(int playerId)
    {
        // Player 1: bottom edge, Player 2: top edge - scaled for larger board
        float y = (playerId == 1) ? -1.6f : 1.6f;
        return new Vector2(0, y);
    }

    public bool MakeMove(int playerId, Vector2 strikerPosition, Vector2 strikerForce)
    {
        // Validate move
        if (playerId != currentTurn)
        {
            Debug.LogWarning($"Invalid move: Not player {playerId}'s turn");
            return false;
        }

        if (isSimulationRunning)
        {
            Debug.LogWarning("Simulation already running");
            return false;
        }

        // Validate striker position is within baseline bounds
        if (!IsValidStrikerPosition(playerId, strikerPosition))
        {
            Debug.LogWarning("Invalid striker position");
            return false;
        }

        // Set up move result tracking
        pendingMoveResult = new MoveResult
        {
            Success = true,
            PottedCoinsThisTurn = new List<CoinType>()
        };

        // Execute move
        physicsManager.SetStrikerPosition(strikerPosition);
        physicsManager.FlickStriker(strikerForce);
        
        isSimulationRunning = true;
        return true;
    }

    private bool IsValidStrikerPosition(int playerId, Vector2 position)
    {
        float expectedY = (playerId == 1) ? -1.6f : 1.6f;
        float tolerance = 0.4f; // Scale tolerance for larger board
        
        return Mathf.Abs(position.y - expectedY) < tolerance && 
               position.x >= -1.6f && position.x <= 1.6f; // Scale position bounds for larger board
    }

    private void HandleObjectPotted(CircleObject pottedObject)
    {
        pottedCoins.Add(pottedObject);
        
        if (pendingMoveResult != null)
        {
            pendingMoveResult.PottedCoinsThisTurn.Add(pottedObject.Type);
        }

        // Handle striker potting (foul)
        if (pottedObject.IsStriker)
        {
            HandleStrikerPottedFoul();
        }
        
        // Handle pink coin potting
        if (pottedObject.Type == CoinType.Pink)
        {
            HandlePinkCoinPotted();
        }
        
        // Update potted coins count
        if (pottedObject.Type == CoinType.White || pottedObject.Type == CoinType.Black)
        {
            int playerForCoin = (pottedObject.Type == CoinType.White) ? 1 : 2;
            currentGameState.PottedCoinsCountPerPlayer[playerForCoin]++;
        }
    }

    private void HandleStrikerPottedFoul()
    {
        // Return one previously potted coin to the board for the current player
        var playerCoinType = (currentTurn == 1) ? CoinType.White : CoinType.Black;
        var coinToReturn = pottedCoins.LastOrDefault(c => c.Type == playerCoinType);
        
        if (coinToReturn != null)
        {
            pottedCoins.Remove(coinToReturn);
            physicsManager.ReturnCoinToBoard(coinToReturn, Vector2.zero);
            currentGameState.PottedCoinsCountPerPlayer[currentTurn]--;
        }
    }

    private void HandlePinkCoinPotted()
    {
        var playerCoinType = (currentTurn == 1) ? CoinType.White : CoinType.Black;
        bool allColorCoinsPotted = currentGameState.PottedCoinsCountPerPlayer[currentTurn] >= 9;
        
        if (!allColorCoinsPotted)
        {
            // Foul: Return pink coin to center
            var pinkCoin = pottedCoins.LastOrDefault(c => c.Type == CoinType.Pink);
            if (pinkCoin != null)
            {
                pottedCoins.Remove(pinkCoin);
                physicsManager.ReturnCoinToBoard(pinkCoin, Vector2.zero);
                
                if (pendingMoveResult != null)
                {
                    pendingMoveResult.Foul = true;
                }
            }
        }
    }

    private void HandleSimulationStopped()
    {
        isSimulationRunning = false;
        
        if (pendingMoveResult == null) return;

        // Update coin positions
        UpdateCoinPositions();
        pendingMoveResult.NewCoinPositions = new List<Coin>(currentGameState.CoinPositions);

        // Check win condition
        CheckWinCondition();
        
        // Determine next turn (with turn continuation rule)
        bool pottedPlayerCoin = false;
        var playerCoinType = (currentTurn == 1) ? CoinType.White : CoinType.Black;
        
        foreach (var coinType in pendingMoveResult.PottedCoinsThisTurn)
        {
            if (coinType == playerCoinType)
            {
                pottedPlayerCoin = true;
                break;
            }
        }

        // Turn continuation: if player potted their coin, they get another turn
        if (pottedPlayerCoin && currentGameState.GameState == GameState.Ongoing)
        {
            pendingMoveResult.NextTurn = currentTurn; // Same player
        }
        else
        {
            // Switch turns
            currentTurn = (currentTurn == 1) ? 2 : 1;
            pendingMoveResult.NextTurn = currentTurn;
            
            // Reset striker position for new player
            Vector2 newStrikerPos = GetStrikerBaselinePosition(currentTurn);
            physicsManager.SetStrikerPosition(newStrikerPos);
        }

        currentGameState.CurrentTurn = currentTurn;
        pendingMoveResult.GameState = currentGameState.GameState;
        pendingMoveResult.Winner = currentGameState.Winner;

        // Clear pending result
        pendingMoveResult = null;
    }

    private void CheckWinCondition()
    {
        // Check if current player won
        var playerCoinType = (currentTurn == 1) ? CoinType.White : CoinType.Black;
        bool allColorCoinsPotted = currentGameState.PottedCoinsCountPerPlayer[currentTurn] >= 9;
        bool pinkCoinPotted = pottedCoins.Any(c => c.Type == CoinType.Pink);
        
        if (allColorCoinsPotted && pinkCoinPotted)
        {
            currentGameState.GameState = GameState.Win;
            currentGameState.Winner = currentTurn;
        }
    }

    private void UpdateCoinPositions()
    {
        currentGameState.CoinPositions.Clear();
        
        foreach (var obj in physicsManager.objects)
        {
            if (!obj.IsStriker)
            {
                currentGameState.CoinPositions.Add(new Coin(obj.Type, obj.Position));
            }
        }
    }

    // IServer Implementation
    public GameState GetGameState()
    {
        return currentGameState.GameState;
    }

    public int GetCurrentTurn()
    {
        return currentGameState.CurrentTurn;
    }

    public List<Coin> GetCoinPositions()
    {
        UpdateCoinPositions();
        return new List<Coin>(currentGameState.CoinPositions);
    }
} 