using System.Collections.Generic;
using UnityEngine;

public enum CoinType
{
    None,
    White,
    Black,
    Pink
}

public enum GameState
{
    Ongoing,
    Win
}

[System.Serializable]
public class Coin
{
    public CoinType Type { get; set; }
    public Vector2 Position { get; set; }

    public Coin(CoinType type, Vector2 position)
    {
        Type = type;
        Position = position;
    }
}

// Message Classes for Client-Server Communication
[System.Serializable]
public class MakeMoveRequest
{
    public int PlayerId { get; set; }
    public Vector2 StrikerPosition { get; set; }
    public Vector2 StrikerForce { get; set; }

    public MakeMoveRequest(int playerId, Vector2 strikerPosition, Vector2 strikerForce)
    {
        PlayerId = playerId;
        StrikerPosition = strikerPosition;
        StrikerForce = strikerForce;
    }
}

[System.Serializable]
public class MoveResult
{
    public bool Success { get; set; }
    public List<Coin> NewCoinPositions { get; set; }
    public List<CoinType> PottedCoinsThisTurn { get; set; }
    public bool Foul { get; set; }
    public int NextTurn { get; set; }
    public GameState GameState { get; set; }
    public int? Winner { get; set; }

    public MoveResult()
    {
        NewCoinPositions = new List<Coin>();
        PottedCoinsThisTurn = new List<CoinType>();
    }
}

[System.Serializable]
public class GameStateResponse
{
    public List<Coin> CoinPositions { get; set; }
    public Dictionary<int, int> PottedCoinsCountPerPlayer { get; set; }
    public int CurrentTurn { get; set; }
    public GameState GameState { get; set; }
    public int? Winner { get; set; }

    public GameStateResponse()
    {
        CoinPositions = new List<Coin>();
        PottedCoinsCountPerPlayer = new Dictionary<int, int>();
    }
} 