[English Documentation](./README.md)

# DragonBones Unity 插件

DragonBones 骨骼动画 Unity 运行时插件，支持在 Unity 中播放和管理 DragonBones 骨骼动画。

## 功能特性

- ✅ 完整的骨骼动画播放支持
- ✅ 动画混合、分层播放
- ✅ 网格变形（Mesh Deformation）
- ✅ 反向动力学（IK）
- ✅ 边界框碰撞检测
- ✅ UGUI 支持
- ✅ 事件系统
- ✅ 换装、换肤支持
- ✅ 自定义着色器支持

## 安装

### 方式一：使用 UnityPackage（推荐）

1. 在 [Releases](https://github.com/你的用户名/DragonBonesUnity/releases) 页面下载最新的 `DragonBonesUnity.unitypackage`
2. 在 Unity 编辑器中双击 `.unitypackage` 文件导入
3. 完成！

### 方式二：源代码集成

1. 将 `Assets/DragonBones` 文件夹复制到你的 Unity 项目 Assets 目录下
2. 等待 Unity 编译完成

## 快速开始

### 1. 创建 Armature 对象

在 Unity 菜单中选择：
```
GameObject → DragonBones → Armature Object
```

或选中已导入的 DragonBones 数据文件（_ske.json），右键选择：
```
Create → DragonBones → Armature Object
```

### 2. 基本播放代码

```csharp
using DragonBones;
using UnityEngine;

public class DragonBonesDemo : MonoBehaviour
{
    private UnityArmatureComponent armature;

    void Start()
    {
        armature = GetComponent<UnityArmatureComponent>();
        
        // 播放动画
        armature.animation.Play("walk");
        
        // 循环播放
        armature.animation.Play("walk", 0);
    }

    void Update()
    {
        // 点击切换动画
        if (Input.GetMouseButtonDown(0))
        {
            armature.animation.FadeIn("jump", 0.2f);
        }
    }
}
```

## 常用 API

### UnityArmatureComponent

| 属性/方法 | 说明 |
|---------|------|
| `animation` | 动画控制器 |
| `armature` | 骨架对象 |
| `armatureName` | 骨架名称 |
| `unityData` | DragonBones 数据资源 |
| `sortingLayerName` | 排序层名称 |
| `sortingOrder` | 排序顺序 |
| `flipX` / `flipY` | X/Y 轴翻转 |
| `timeScale` | 动画播放速度 |

### 动画控制

```csharp
// 播放动画
armature.animation.Play("animationName");

// 淡入切换动画
armature.animation.FadeIn("animationName", 0.3f);

// 暂停
armature.animation.Stop();

// 播放次数（0 为无限循环）
armature.animation.Play("animationName", 0);

// 获取当前动画
var state = armature.animation.GetState("animationName");
state.timeScale = 0.5f; // 放慢此动画
```

### 动画事件

```csharp
void Start()
{
    armature = GetComponent<UnityArmatureComponent>();
    armature.AddDBEventListener(EventObject.COMPLETE, OnAnimationComplete);
    armature.AddDBEventListener(EventObject.FRAME_EVENT, OnFrameEvent);
}

void OnAnimationComplete(string type, EventObject eventObject)
{
    Debug.Log("动画播放完成: " + eventObject.animationState.name);
}

void OnFrameEvent(string type, EventObject eventObject)
{
    Debug.Log("触发帧事件: " + eventObject.name);
}
```

### 换装

```csharp
// 替换插槽显示
var slot = armature.armature.GetSlot("weapon_slot");
slot.displayIndex = 1;  // 切换到第 2 个显示对象

// 更换皮肤
var skinData = armature.armature.armatureData.GetSkin("hero_blue");
armature.armature.SwapSkin(skinData);
```

## 示例场景

插件包含以下示例场景（位于 `Assets/DragonBones/Demos/Scenes/`）：

| 场景 | 说明 |
|------|------|
| `1.HellowDragonBones.unity` | 基础动画播放 |
| `2.AnimationBase.unity` | 动画基础操作 |
| `3.DragonBonesEvent.unity` | 事件系统使用 |
| `4.AnimationLayer.unity` | 动画分层混合 |
| `5.BoneOffset.unity` | 骨骼偏移控制 |
| `6.InverseKinematics.unity` | IK 反向动力学 |
| `7.BoundingBox.unity` | 边界框碰撞 |
| `8.ReplaceSlotDisplay.unity` | 插槽显示替换 |
| `9.ReplaceSkin.unity` | 完整换装 |
| `10.ReplaceAnimation.unity` | 动画数据替换 |
| `11.CoreElement.unity` | 核心元素演示 |
| `UGUIDisplay.unity` | UGUI 中使用 |
| `Performance.unity` | 性能测试 |
| `LightShader.unity` | 自定义着色器 |

## 数据导入

### 使用 DragonBones 数据

1. 在 DragonBones Pro 编辑器中导出数据
2. 将导出的文件（`*_ske.json`, `*_tex.json`, `*_tex.png`）复制到 Unity 项目
3. 选中 `*_ske.json` 文件，右键菜单选择：
   ```
   Create → DragonBones → Create Unity Data
   ```
4. 会生成一个 `*_Data.asset` 文件，这是 Unity 可用的数据资源

### 注意事项

- 确保 DragonBones Pro 导出的数据版本与插件版本兼容
- 目前支持 DragonBones 5.x 格式的数据
- 版本不兼容时会自动弹出错误提示

## UGUI 中使用

创建 UGUI 模式的 Armature：

```
GameObject → DragonBones → Armature Object(UGUI)
```

或在代码中设置：

```csharp
armature.isUGUI = true;
```

## 性能优化建议

1. **减少 draw call**：启用 `closeCombineMeshes` 合并网格
2. **控制同屏数量**：大量角色时考虑对象池
3. **合理设置帧率**：通过 `timeScale` 调整动画速度
4. **卸载无用数据**：使用 `UnityFactory.factory.Clear()` 清理缓存

## 常见问题

### Q: 动画播放时材质丢失？
A: 确保正确生成了 `*_Data.asset` 文件，并且材质路径正确。重新执行 "Create Unity Data" 即可。

### Q: 导入后显示为紫色？
A: 材质着色器丢失。检查材质使用的 Shader 是否存在，或重新分配正确的 Shader。

### Q: 版本不兼容怎么办？
A: 插件会自动弹出版本不兼容提示。请用 DragonBones Pro 将数据转换为支持的版本。

### Q: 如何调整动画播放速度？
A: 
```csharp
armature.timeScale = 0.5f; // 整体放慢
// 或单独调整某个动画
var state = armature.animation.Play("walk");
state.timeScale = 0.8f;
```

## 技术支持

- [DragonBones 官方网站](http://www.loongbones.com/)

## License

MIT License - 详见 [LICENSE](LICENSE) 文件

---
