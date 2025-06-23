# Carrom Game Communication Design

## Game States
1. **Initialization**: Set up the board, place coins (9 white, 9 black, 1 pink), assign Player 1 to white and Player 2 to black.
2. **Player Turn**: Current player positions and flicks the striker.
3. **Physics Simulation**: Physics engine simulates movement and collisions.
4. **Validation and Update**: Server validates the move, updates state, checks fouls and win conditions.
5. **Turn Switch**: Switch to the next player after a valid move.
6. **Game End**: Ends when a player pots all 9 of their color coins and then the pink coin.

### Win Condition
- Pot all 9 assigned color coins, then pot the pink coin on a subsequent turn.

### Foul Rule
- Potting the pink coin before all 9 color coins: pink coin returns to center, turn switches.

## Communication Flow
- **Client** (Unity UI) sends move requests to **Server** (mock module).
- **Server** processes moves, simulates physics, updates state, and responds.
- **Client** updates UI or requests game state as needed.

## Message Codes

### 1. MakeMove
- **Player ID (int)**: 1 or 2
- **Striker Position (Vector2)**: Starting position
- **Striker Force (Vector2)**: Force and direction

### 2. MoveResult
- **Success (bool)**: Move validity
- **New Coin Positions (List<Coin>)**: Coins on board
  - `Type (enum: White, Black, Pink)`
  - `Position (Vector2)`
- **Potted Coins This Turn (List<CoinType>)**: Potted coin types
- **Foul (bool)**: True if pink potted prematurely
- **Next Turn (int)**: Next player ID
- **Game State (enum: Ongoing, Win)**: Game status
- **Winner (int, optional)**: Winner ID if won

### 3. GetGameState
- (No fields)

### 4. GameStateResponse
- **Coin Positions (List<Coin>)**: Coins on board
  - `Type (enum: White, Black, Pink)`
  - `Position (Vector2)`
- **Potted Coins Count Per Player (Dictionary<int, int>)**: Potted count per player
- **Current Turn (int)**: Current player ID
- **Game State (enum: Ongoing, Win)**: Game status
- **Winner (int, optional)**: Winner ID if won