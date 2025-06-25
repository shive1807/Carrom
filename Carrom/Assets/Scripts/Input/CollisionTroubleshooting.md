# Carrom Collision Troubleshooting Guide

## Common Collision Issues & Fixes

### 1. **Coins Not Colliding With Each Other**

#### **Root Causes:**
- ❌ **Missing Physics Material** - No bounce or proper friction
- ❌ **Wrong Collision Detection** - Using discrete instead of continuous
- ❌ **Z-Position Mismatch** - Objects at different Z levels in 2D physics
- ❌ **Overlapping Objects** - Objects start inside each other
- ❌ **Incorrect Layer Settings** - Physics collision matrix blocking collisions
- ❌ **High-Speed Tunneling** - Fast objects pass through each other

#### **Fixes Applied:**
✅ **Added PhysicsMaterial2D** with proper friction (0.3f) and bounciness (0.7f)  
✅ **Set Collision Detection** to `CollisionDetectionMode2D.Continuous`  
✅ **Ensured Z=0** for all 2D physics objects  
✅ **Added Collision Testing** function to diagnose issues  
✅ **Configured High-Speed Physics** - Improved timestep and solver iterations  
✅ **Disabled Rigidbody Sleep** - Prevents objects from sleeping during collisions  

### 2. **How to Test Collisions**

#### **In-Game Testing:**
1. **Press `C` key** during play to run collision diagnostics
2. **Press `H` key** to test high-speed collisions specifically
3. **Check Console** for detailed collision setup information
4. **Look for warnings** about overlapping objects or tunneling

#### **What the Test Checks:**
- All Rigidbody2D objects and their properties
- CircleCollider2D setup and actual radius calculations
- Physics material assignments
- Object spacing and overlap detection
- Physics2D global settings

### 3. **High-Speed Collision Issues (Tunneling)**

#### **Problem:** Objects pass through each other at high speeds
This happens when objects move so fast they completely skip over each other in a single physics frame.

#### **Solutions Applied:**
```csharp
// Reduced physics timestep for more frequent updates
Time.fixedDeltaTime = 0.008f; // ~125Hz instead of 50Hz

// Increased solver iterations for better accuracy
Physics2D.velocityIterations = 12; // Default: 8
Physics2D.positionIterations = 6;  // Default: 3

// Prevented objects from sleeping during collisions
rb2D.sleepMode = RigidbodySleepMode2D.NeverSleep;

// Used continuous collision detection
rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
```

#### **Testing High-Speed Collisions:**
- **Press `H` key** to run automated high-speed collision test
- Two coins will be launched at each other at 15 units/sec
- Watch to see if they bounce or pass through each other
- Check console for detailed collision information

### 4. **Manual Troubleshooting Steps**

#### **Step 1: Verify Basic Setup**
```csharp
// Each coin should have:
Rigidbody2D rb2D = coin.GetComponent<Rigidbody2D>();
CircleCollider2D collider = coin.GetComponent<CircleCollider2D>();

// Check these settings:
rb2D.gravityScale = 0;  // No gravity for top-down view
rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
collider.sharedMaterial = physicsMaterial;  // Not null
```

#### **Step 2: Check Object Positioning**
- All objects must be at **Z = 0** for 2D physics
- Objects should **not overlap** when created
- Minimum distance = sum of both object radii

#### **Step 3: Verify Physics Material**
```csharp
PhysicsMaterial2D material = new PhysicsMaterial2D("CarromPhysics");
material.friction = 0.3f;     // Some friction to prevent infinite sliding
material.bounciness = 0.7f;   // Good bounce for carrom coins
```

#### **Step 4: Check Unity Physics Settings**
- Open **Edit → Project Settings → Physics 2D**
- Verify **Collision Matrix** allows collisions between your layers
- Check **Gravity** is set to (0, 0) for top-down games

### 4. **Advanced Debugging**

#### **Enable Visual Debugging:**
```csharp
// In Scene view, enable:
// - Physics Debugger (Window → Analysis → Physics Debugger)
// - Gizmos for colliders
// - 2D Physics visualization
```

#### **Console Debugging:**
```csharp
void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log($"{gameObject.name} collided with {collision.gameObject.name}");
}
```

### 5. **Performance Considerations**

#### **Collision Detection Modes:**
- **Discrete**: Fast but can miss fast-moving collisions
- **Continuous**: Slower but catches all collisions ✅ (Used)
- **ContinuousDynamic**: For fast-moving objects only

#### **Rigidbody Settings:**
```csharp
rb2D.linearDamping = 2f;   // Slows down over time
rb2D.angularDamping = 5f;  // Stops spinning over time
```

### 6. **Common Unity Issues**

#### **Layer Collision Matrix:**
1. Go to **Edit → Project Settings → Physics 2D**
2. Check **Layer Collision Matrix** at bottom
3. Ensure your object layers can collide with each other

#### **Time Scale:**
- If `Time.timeScale = 0`, physics won't update
- Check with `Debug.Log($"Time scale: {Time.timeScale}");`

#### **Solver Settings:**
- **Position Iterations**: Higher = more accurate (default: 3)
- **Velocity Iterations**: Higher = more stable (default: 8)

### 7. **Testing Checklist**

Before reporting collision issues, verify:

- [ ] All objects have Rigidbody2D components
- [ ] All objects have CircleCollider2D components  
- [ ] Physics material is assigned and not null
- [ ] Objects are positioned at Z = 0
- [ ] No objects are overlapping at start
- [ ] Collision detection mode is set to Continuous
- [ ] Layer collision matrix allows collisions
- [ ] Time.timeScale > 0
- [ ] Gravity is disabled (gravityScale = 0)

### 8. **Quick Fix Commands**

Run collision test: **Press `C` key** in play mode  
Test high-speed collisions: **Press `H` key** in play mode

Reset all physics objects:
```csharp
foreach(var rb in FindObjectsOfType<Rigidbody2D>())
{
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;
}
```

### 9. **Still Having Issues?**

If collisions still don't work after following this guide:

1. **Enable Physics Debugger** (Window → Analysis → Physics Debugger)
2. **Check Console** for any physics-related errors
3. **Test with simpler setup** (just 2 objects)
4. **Try 3D physics** instead of 2D as alternative
5. **Check Unity version compatibility**

Remember: The collision test function (`C` key) provides detailed diagnostics to help identify the exact issue! 