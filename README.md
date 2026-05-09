[中文文档](./README-zh.md)

# DragonBones Unity Plugin

DragonBones skeletal animation runtime for Unity, supporting playback and management of DragonBones skeletal animations.

## Features

- ✅ Full skeletal animation playback support
- ✅ Animation blending and layer playback
- ✅ Mesh Deformation
- ✅ Inverse Kinematics (IK)
- ✅ Bounding box collision detection
- ✅ UGUI support
- ✅ Event system
- ✅ Skin and slot replacement support
- ✅ Custom shader support

## Installation

### Method 1: Using UnityPackage (Recommended)

1. Download the latest `DragonBonesUnity.unitypackage` from the [Releases](https://github.com/your-username/DragonBonesUnity/releases) page
2. Double-click the `.unitypackage` file in Unity Editor to import
3. Done!

### Method 2: Source Code Integration

1. Copy the `Assets/DragonBones` folder into your Unity project's Assets directory
2. Wait for Unity to finish compiling

## Quick Start

### 1. Create Armature Object

From Unity menu:
```
GameObject → DragonBones → Armature Object
```

Or select an imported DragonBones data file (`*_ske.json`), right-click and select:
```
Create → DragonBones → Armature Object
```

### 2. Basic Playback Code

```csharp
using DragonBones;
using UnityEngine;

public class DragonBonesDemo : MonoBehaviour
{
    private UnityArmatureComponent armature;

    void Start()
    {
        armature = GetComponent<UnityArmatureComponent>();
        
        // Play animation
        armature.animation.Play("walk");
        
        // Loop infinitely
        armature.animation.Play("walk", 0);
    }

    void Update()
    {
        // Switch animation on click
        if (Input.GetMouseButtonDown(0))
        {
            armature.animation.FadeIn("jump", 0.2f);
        }
    }
}
```

## Common API

### UnityArmatureComponent

| Property/Method | Description |
|---------|------|
| `animation` | Animation controller |
| `armature` | Armature object |
| `armatureName` | Armature name |
| `unityData` | DragonBones data asset |
| `sortingLayerName` | Sorting layer name |
| `sortingOrder` | Sorting order |
| `flipX` / `flipY` | X/Y axis flip |
| `timeScale` | Animation playback speed |

### Animation Control

```csharp
// Play animation
armature.animation.Play("animationName");

// Cross-fade to animation
armature.animation.FadeIn("animationName", 0.3f);

// Pause
armature.animation.Stop();

// Play count (0 for infinite loop)
armature.animation.Play("animationName", 0);

// Get current animation state
var state = armature.animation.GetState("animationName");
state.timeScale = 0.5f; // Slow down this animation
```

### Animation Events

```csharp
void Start()
{
    armature = GetComponent<UnityArmatureComponent>();
    armature.AddDBEventListener(EventObject.COMPLETE, OnAnimationComplete);
    armature.AddDBEventListener(EventObject.FRAME_EVENT, OnFrameEvent);
}

void OnAnimationComplete(string type, EventObject eventObject)
{
    Debug.Log("Animation complete: " + eventObject.animationState.name);
}

void OnFrameEvent(string type, EventObject eventObject)
{
    Debug.Log("Frame event triggered: " + eventObject.name);
}
```

### Skin Replacement

```csharp
// Replace slot display
var slot = armature.armature.GetSlot("weapon_slot");
slot.displayIndex = 1;  // Switch to 2nd display object

// Change skin
var skinData = armature.armature.armatureData.GetSkin("hero_blue");
armature.armature.SwapSkin(skinData);
```

## Demo Scenes

Plugin includes the following demo scenes (located in `Assets/DragonBones/Demos/Scenes/`):

| Scene | Description |
|------|------|
| `1.HellowDragonBones.unity` | Basic animation playback |
| `2.AnimationBase.unity` | Animation basic operations |
| `3.DragonBonesEvent.unity` | Event system usage |
| `4.AnimationLayer.unity` | Animation layer blending |
| `5.BoneOffset.unity` | Bone offset control |
| `6.InverseKinematics.unity` | IK Inverse Kinematics |
| `7.BoundingBox.unity` | Bounding box collision |
| `8.ReplaceSlotDisplay.unity` | Slot display replacement |
| `9.ReplaceSkin.unity` | Complete skin replacement |
| `10.ReplaceAnimation.unity` | Animation data replacement |
| `11.CoreElement.unity` | Core elements demo |
| `UGUIDisplay.unity` | Usage in UGUI |
| `Performance.unity` | Performance test |
| `LightShader.unity` | Custom shader demo |

## Data Import

### Using DragonBones Data

1. Export data from DragonBones Pro editor
2. Copy exported files (`*_ske.json`, `*_tex.json`, `*_tex.png`) into Unity project
3. Select the `*_ske.json` file, right-click menu select:
   ```
   Create → DragonBones → Create Unity Data
   ```
4. A `*_Data.asset` file will be generated - this is the Unity-compatible data resource

### Notes

- Ensure DragonBones Pro exported data version is compatible with the plugin
- Currently supports DragonBones 5.x format data
- Version incompatibility will automatically show an error dialog

## Usage in UGUI

Create UGUI mode Armature:

```
GameObject → DragonBones → Armature Object(UGUI)
```

Or set in code:

```csharp
armature.isUGUI = true;
```

## Performance Optimization Tips

1. **Reduce draw calls**: Enable `closeCombineMeshes` to merge meshes
2. **Control on-screen count**: Consider object pooling for many characters
3. **Adjust frame rate**: Use `timeScale` to adjust animation speed
4. **Unload unused data**: Use `UnityFactory.factory.Clear()` to clean cache

## FAQ

### Q: Material missing when animation plays?
A: Ensure `*_Data.asset` file was generated correctly and material paths are correct. Re-run "Create Unity Data".

### Q: Purple/magenta color after import?
A: Material shader missing. Check if the shader used by the material exists, or reassign the correct shader.

### Q: Version incompatibility?
A: Plugin will automatically show version incompatibility dialog. Convert the data to a supported version using DragonBones Pro.

### Q: How to adjust animation speed?
A: 
```csharp
armature.timeScale = 0.5f; // Slow down globally
// Or adjust single animation
var state = armature.animation.Play("walk");
state.timeScale = 0.8f;
```

## Support

- [DragonBones Official Website](http://www.loongbones.app/)

## License

MIT License - See [LICENSE](LICENSE) file for details

---


