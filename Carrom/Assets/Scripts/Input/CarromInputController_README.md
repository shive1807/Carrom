# CarromInputController Usage Guide

## Overview
The `CarromInputController` script provides a complete input system for carrom pool games with the following features:

- ✅ **Drag left/right to aim** - Rotates striker around board center
- ✅ **Pull backward for power** - Slingshot-style power adjustment
- ✅ **Visual feedback** - Power line and aim line indicators
- ✅ **Cross-platform input** - Mouse (editor) and touch (mobile) support
- ✅ **Unity physics integration** - Works with both 2D and 3D Rigidbody components
- ✅ **State management** - Prevents input during striker movement
- ✅ **Debug support** - Visual debug lines and logging

## Setup Instructions

### 1. Basic Setup
1. Add the `CarromInputController` script to any GameObject in your scene
2. Create or assign a striker GameObject with a Transform component
3. The script will automatically add Rigidbody components if needed

### 2. Inspector Configuration

#### **Striker Configuration**
- `Striker`: Transform of your striker GameObject
- `Striker Rigidbody2D/3D`: Will be auto-assigned if not set
- `Use 2D Physics`: Toggle between 2D and 3D physics systems

#### **Board Configuration**
- `Board Center`: Transform at the center of your board (auto-created if null)
- `Board Radius`: Visual radius for debug circles (default: 2f)
- `Striker Distance From Center`: How far striker sits from center (default: 1.8f)

#### **Input Settings**
- `Aiming Sensitivity`: How much rotation per drag distance (default: 2f)
- `Max Power Distance`: Maximum backward drag distance (default: 1f)
- `Force Multiplier`: Multiplier for physics force (default: 10f)
- `Min Force Threshold`: Minimum power to fire (default: 0.1f)

#### **Visual Feedback**
- `Power Line Renderer`: LineRenderer for power indicator (auto-created)
- `Aim Line Renderer`: LineRenderer for aim direction (auto-created)
- `Power/Aim Line Color`: Colors for the visual indicators
- `Aim Line Length`: Length of aim indicator line (default: 3f)

#### **Debug Settings**
- `Enable Debug Lines`: Show debug visualizations in Scene view
- `Enable Debug Log`: Print debug information to console

## How to Use

### Basic Interaction
1. **Aiming**: Drag left or right anywhere on screen (except near striker)
2. **Power Adjustment**: Drag near the striker and pull backward
3. **Firing**: Release after power adjustment to fire the striker

### Input Modes
The script automatically detects input type:
- **Aiming Mode**: When drag starts away from striker
- **Power Mode**: When drag starts close to striker (within 0.5 units)

### Visual Feedback
- **Yellow Line**: Shows aim direction
- **Red/Yellow Line**: Shows power level (color changes with intensity)
- **Debug Lines**: Board boundary, striker-to-center line, etc.

## Integration with Existing Code

### Method 1: Replace Existing Input
```csharp
// Disable your existing input system
// Add CarromInputController to your striker GameObject
// Configure the inspector settings
```

### Method 2: Use Alongside Custom Physics
```csharp
public class CustomCarromManager : MonoBehaviour
{
    public CarromInputController inputController;
    
    void Start()
    {
        // You can access the input controller's state
        string currentState = inputController.GetCurrentState();
        float powerLevel = inputController.GetPowerLevel();
    }
    
    // Reset striker when needed
    public void ResetGame()
    {
        inputController.ResetStriker();
    }
}
```

## Public Methods

### `ResetStriker()`
Resets striker to initial position and stops all movement.

### `GetCurrentState()`
Returns current input state as string: "Idle", "Aiming", "PowerAdjusting", "Firing", "StrikerMoving"

### `GetPowerLevel()`
Returns current power level as float (0-1 range)

## Customization Tips

### Adjusting Feel
- **Increase `aimingSensitivity`** for faster aiming
- **Increase `forceMultiplier`** for more powerful shots
- **Adjust `maxPowerDistance`** to change power range

### Visual Customization
- Change `powerLineColor` and `aimLineColor` for different themes
- Modify LineRenderer properties in `SetupVisualFeedback()` method
- Add particle effects by extending the `FireStriker()` method

### Physics Integration
- The script works with both Unity's built-in physics and custom physics
- For custom physics, override the `FireStriker()` method
- State management prevents conflicts with ongoing simulations

## Troubleshooting

### Common Issues

1. **Striker doesn't move**
   - Check that Rigidbody component is assigned
   - Verify `forceMultiplier` is not too low
   - Ensure striker is not kinematic

2. **Aiming feels too sensitive/slow**
   - Adjust `aimingSensitivity` value
   - Check camera setup (orthographic vs perspective affects world coordinates)

3. **Power adjustment doesn't work**
   - Make sure you're dragging close to the striker
   - Check `maxPowerDistance` setting
   - Verify `minForceThreshold` is appropriate

4. **Visual lines don't appear**
   - LineRenderer components might not be assigned
   - Check that materials are set up correctly
   - Ensure LineRenderer GameObjects are active

### Debug Mode
Enable `enableDebugLines` and `enableDebugLog` to see:
- Board boundary circle (cyan)
- Striker-to-center line (green)
- Aim direction ray (yellow)
- Power direction ray (red)
- Console logging of all operations

## Performance Notes

- Script updates every frame but uses efficient calculations
- LineRenderers are only enabled when needed
- Debug lines only draw when `enableDebugLines` is true
- No garbage allocation during normal operation

## Example Scene Setup

1. Create empty GameObject "GameManager"
2. Add `CarromInputController` script
3. Create striker GameObject with SpriteRenderer/MeshRenderer
4. Assign striker to the script's `Striker` field
5. Press Play and test the input system

The script will handle all initialization automatically! 