# Carrom Game Implementation Plan

This document provides a step-by-step guide to implement the base version of a 2-player pass-and-play carrom game in Unity. The game features a square board with four central pots, 9 white coins, 9 black coins, 1 pink coin, a striker, custom physics, and a mock server module within Unity. Each step includes a specific instruction and a test to ensure correct implementation.

## Step 1: Create the Game Board
- **Instruction**: In Unity, create a new scene named "CarromGame". Add a square game object (1x1 unit) centered at (0, 0) to represent the board. Add four small circular game objects (e.g., 0.1 unit diameter) at the center of each side (positions: (0, 0.5), (0, -0.5), (0.5, 0), (-0.5, 0)) to represent the pots. Apply distinct colors or materials to the board and pots.
- **Test**: Run the scene and visually confirm that the square board and four pots are correctly positioned and distinguishable.

## Step 2: Place Coins on the Board
- **Instruction**: Create three prefabs: white coin, black coin, and pink coin (each a small circle, e.g., 0.05 unit diameter). In a `GameManager` script, instantiate 9 white coins, 9 black coins, and 1 pink coin. Arrange them in a circular pattern at the board’s center (0, 0), with the pink coin in the middle and white and black coins alternating around it in a tight circle (e.g., radius 0.15 units).
- **Test**: Start the game and visually verify that 9 white, 9 black, and 1 pink coin are positioned correctly in the center.

## Step 3: Implement Striker Positioning
- **Instruction**: Create a striker prefab (e.g., a circle, 0.07 unit diameter). In the `GameManager`, allow the current player to drag the striker along their baseline using mouse input. For Player 1, restrict movement along the bottom edge (y = -0.5, x between -0.5 and 0.5). For Player 2, restrict movement along the top edge (y = 0.5, x between -0.5 and 0.5). Use a `currentPlayer` variable (1 or 2) to determine the baseline.
- **Test**: Switch between Player 1 and Player 2, drag the striker, and ensure it moves only along the correct baseline within the specified bounds.

## Step 4: Implement Striker Flicking
- **Instruction**: In the `GameManager`, enable the player to flick the striker. When the striker is clicked, allow dragging the mouse backward to set direction and force. On release, calculate the force vector (e.g., drag distance * constant) and apply it to the striker to initiate movement.
- **Test**: Position the striker, drag the mouse back and release, and observe if the striker moves across the board in the expected direction.

## Step 5: Implement Basic Physics
- **Instruction**: Use the custom physics module to manage movement and collisions. Add colliders and rigidbodies to the striker and all coins. Ensure the physics handles collisions between the striker and coins, and between coins, with appropriate momentum transfer and friction.
- **Test**: Flick the striker into a group of coins and confirm that collisions occur realistically, with coins moving based on impact.

## Step 6: Implement Potting Mechanism
- **Instruction**: In the physics module, define the four pot areas as trigger zones (e.g., circles at the pot positions). When a coin’s collider enters a pot’s trigger zone, remove the coin from the scene and update the `GameManager` to track which player potted it (based on coin type and current turn).
- **Test**: Flick a coin into a pot and verify that it disappears from the board and the game state updates accordingly.

## Step 7: Implement Turn Switching
- **Instruction**: In the `GameManager`, after a player flicks the striker and the physics simulation ends (see Step 11), switch the `currentPlayer` value (e.g., from 1 to 2 or 2 to 1). Reset the striker to the new player’s baseline.
- **Test**: Complete a turn by flicking the striker, wait for all movement to stop, and confirm that the turn switches to the other player with the striker repositioned.

## Step 8: Implement Win Condition Check
- **Instruction**: In the `GameManager`, after each turn, check if the current player has potted all 9 of their coins (white for Player 1, black for Player 2). If true, set a flag (e.g., `canWin`). On their next turn, if they pot the pink coin and `canWin` is true, declare them the winner and end the game.
- **Test**: Simulate a game where a player pots all 9 of their coins, then pots the pink coin on the next turn, and verify that they are declared the winner.

## Step 9: Implement Foul for Premature Pink Potting
- **Instruction**: In the `GameManager`, if the pink coin is potted and the current player has not potted all 9 of their coins, return the pink coin to the center of the board (position (0, 0)) and keep it in play.
- **Test**: Pot the pink coin before potting all 9 of your coins and confirm that it reappears at the board’s center.

## Step 10: Implement Mock Server Module
- **Instruction**: Create a `MockServer` class to manage game state (e.g., coin positions, potted coins, current turn, win conditions). The `GameManager` (client) must send move requests (e.g., striker flick) to the `MockServer`, which validates and updates the state, then notifies the client of changes.
- **Test**: Flick the striker and attempt to modify the game state directly in the client; ensure changes only occur via the `MockServer`.

## Step 11: Implement Simulation End Detection
- **Instruction**: In the custom physics module, check every frame if all objects (striker and coins) have velocities below a threshold (e.g., 0.01 units/sec). If true, signal the `GameManager` that the simulation has ended to proceed with turn switching.
- **Test**: Flick the striker, wait until all movement stops, and confirm that the game automatically proceeds to the next turn.

## Step 12: Restrict Input to Current Player
- **Instruction**: In the `GameManager`, disable striker positioning and flicking input when it’s not the player’s turn (e.g., check `currentPlayer` against an input source identifier). Only enable input for the current player.
- **Test**: During Player 2’s turn, attempt to move the striker as Player 1 and verify that no action occurs; repeat for Player 2 during Player 1’s turn.

## Step 13: Implement Basic UI
- **Instruction**: Add UI text elements to the scene: one showing the number of white coins left (for Player 1), one showing black coins left (for Player 2), one indicating the pink coin’s status (on board or potted), and one displaying the current player’s turn (e.g., “Player 1’s Turn”). Update these in the `GameManager` after each turn.
- **Test**: Pot some coins during gameplay and confirm that the UI updates to reflect the correct number of coins left and the current turn.

---

This plan ensures a functional base carrom game with core mechanics, tested incrementally for reliability. Advanced features like additional fouls or networked play can be added later.