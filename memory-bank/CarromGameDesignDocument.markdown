# Game Design Document: Carrom Multiplayer

**Date:** Monday, October 28, 2024  
**Time:** 10:00 AM IST

---

## 1. Game Overview

- **Title:** Carrom Multiplayer
- **Genre:** Sports, Strategy, Physics-based
- **Platform:** Unity (single device with pass-and-play)
- **Target Audience:** All ages, fans of board games and physics-based challenges
- **Description:** A 2-player carrom game where players alternate turns to flick a striker and pot their assigned coins (white or black) into four central pots on a square board, followed by the pink coin to win. Custom physics ensures realistic coin movement and collisions.

---

## 2. Gameplay Mechanics

- **Players:** 2 players (Player 1: White coins, Player 2: Black coins)
- **Board Setup:** A square board with four pots positioned at the center of each side.
- **Coins:**
  - 9 White coins
  - 9 Black coins
  - 1 Pink coin (the "queen")
- **Objective:** Pot all assigned color coins (white or black) and then the pink coin to win.
- **Turns:** Players take turns flicking the striker to hit and pot coins.
- **Striker Mechanics:** Players position the striker along their baseline and flick it with adjustable force and direction.
- **Winning Condition:** The first player to pot all their color coins and then the pink coin wins.
- **Foul Rules (Optional):**
  - Potting the striker: Return a previously potted coin to the board.
  - Potting an opponent’s coin: Opponent gains a point or advantage.

---

## 3. Custom Physics

- **Coin Movement:** Custom physics simulates realistic friction, momentum, and coin trajectories.
- **Collision Detection:** Precise coin-to-coin and coin-to-striker collision handling.
- **Potting Mechanism:** Coins are potted when they enter one of the four central pots.
- **Striker Control:** Players adjust striker position and flick force/direction via input.

---

## 4. User Interface

- **Board Display:** Top-down view of the square board, showing coins, pots, and striker.
- **Turn Indicator:** Displays "Player 1's Turn" or "Player 2's Turn".
- **Score Display:** Tracks remaining coins per player.
- **Win Message:** Announces the winner (e.g., "Player 1 Wins!").
- **Restart Button:** Resets the game for a new match.

---

## 5. Assets

- **Sprites:**
  - White coins
  - Black coins
  - Pink coin
  - Striker
  - Board texture
  - Pot indicators
- **Sounds (Optional):**
  - Coin collision sound
  - Potting sound
  - Victory sound
- **Fonts:** For UI text (e.g., scores, turn indicators).

---

## 6. Technical Requirements

- **Unity Version:** Unity 2023.2
- **Scripting Language:** C#
- **Key Scripts:**
  - `GameManager`: Controls game flow, turns, and UI.
  - `Board`: Manages board display and player input.
  - `PhysicsEngine`: Handles custom physics for coin movement and collisions.
  - `MockServer`: Simulates server logic locally.
- **Mock Server Module:** Runs within Unity, mimicking network server behavior for scalability.

---

## 7. Mock Server Design

The mock server simulates network server functionality locally, enabling future network integration via an `IServer` interface.

### 7.1. IServer Interface

- `bool MakeMove(int playerId, Vector2 strikerPosition, Vector2 strikerForce)`: Validates and processes a striker flick.
- `GameState GetGameState()`: Returns current game state (e.g., ongoing, won).
- `int GetCurrentTurn()`: Identifies the current player.
- `List<Coin> GetCoinPositions()`: Provides current coin positions.

### 7.2. MockServer Implementation

- **Game State Management:** Tracks coin positions, potted coins, and turns.
- **Move Validation:** Ensures legal striker placement and flick parameters.
- **Physics Simulation:** Uses `PhysicsEngine` to update coin positions post-flick.
- **Win Condition Check:** Verifies if a player has won after each turn.

---

## 8. Pass-and-Play Mechanic

- Players share one device, alternating turns.
- After Player 1 flicks, the turn switches to Player 2 with an updated indicator.
- Input is locked to the current player’s turn.

---

## 9. Script Breakdown

### 9.1. GameManager
- **Responsibilities:** Initialize game, manage turns, update UI, handle restarts.
- **Key Methods:**
  - `StartGame()`: Sets up a new match.
  - `OnGameEnd(int winnerId)`: Shows win message.
  - `RestartGame()`: Resets the game.

### 9.2. Board
- **Responsibilities:** Display board/coins, process striker input, sync with mock server.
- **Key Methods:**
  - `OnStrikerFlick(Vector2 position, Vector2 force)`: Submits move to mock server.
  - `UpdateBoard(List<Coin> coinPositions)`: Refreshes coin positions.

### 9.3. PhysicsEngine
- **Responsibilities:** Simulate coin physics, collisions, and potting.
- **Key Methods:**
  - `SimulateFlick(Vector2 strikerPosition, Vector2 strikerForce)`: Computes coin movement.
  - `CheckCollisions()`: Resolves collisions.

### 9.4. MockServer
- **Responsibilities:** Implement `IServer`, manage state, validate moves, check wins.
- **Key Methods:**
  - `MakeMove(int playerId, Vector2 strikerPosition, Vector2 strikerForce)`: Processes turn.
  - `CheckWinCondition()`: Detects a winner.

---

## 10. Additional Rules (Optional)

- **Pink Coin Rule:** Must pot at least one color coin before potting the pink coin.
- **Turn Continuation:** Potting a coin grants an extra turn.

---

## 11. Future Scalability

The `IServer` interface allows seamless transition to networked multiplayer by replacing `MockServer` with a `NetworkServer`, minimizing client-side changes.

---

## 12. Summary

This GDD details a 2-player carrom game in Unity with custom physics, a mock server, and pass-and-play mechanics. The game pits two players against each other to pot their coins and the pink coin on a square board with four central pots. Optional rules add strategic depth, and the modular design supports future network expansion.