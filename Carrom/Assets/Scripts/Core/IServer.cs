using System.Collections.Generic;
using UnityEngine;

public interface IServer
{
    bool MakeMove(int playerId, Vector2 strikerPosition, Vector2 strikerForce);
    GameState GetGameState();
    int GetCurrentTurn();
    List<Coin> GetCoinPositions();
    void InitializeGame();
} 