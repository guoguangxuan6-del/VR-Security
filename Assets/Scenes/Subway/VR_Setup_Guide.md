# Subway场景VR设置与修复指南

本指南针对已重置为 17 号纯净静态版本的 **VR-Security** 项目，为你提供在地铁站场景 **[Demonstration.unity](file:///D:/Unity/VR-Security/Assets/Scenes/Subway/Demonstration.unity)** 中配置 VR 相机、实现手柄移动以及修复 HDRP 材质丢失的实战步骤。

---

## 🛠 一、 场景核心 Hierarchy 结构要求

为了避免运行时动态 SetParent 导致的骨骼丢失与动画报错，所有的相机与手部追踪组件必须在场景中以**静态父子级关系**提前摆放好。

请确保你的 Hierarchy（层级）视图呈现如下结构：
```
VRPlayer (挂载 CharacterController + VRPlayerRig 脚本)
└── OVRCameraRig (挂载 OVRAnchorBinder + HDRPMaterialFixer 脚本)
      └── TrackingSpace
            ├── LeftEyeAnchor
            ├── CenterEyeAnchor (Camera)
            ├── RightEyeAnchor
            ├── LeftHandAnchor
            │     └── OVRCustomHandPrefab_L (左手模型预制体，直接拖入此处作为子级)
            └── RightHandAnchor
                  └── OVRCustomHandPrefab_R (右手模型预制体，直接拖入此处作为子级)
```

---

## 🚀 三步配置法

### 第一步：打开地铁场景并禁用旧相机
1. 在 Project 窗口双击打开 `Assets/Scenes/Subway/Demonstration.unity` 场景。
2. 在 Hierarchy 中找到旧的主摄像机（如有名为 `Main Camera` 的物体），将其**右键删除**。

### 第二步：挂载物理移动控制器
1. 在 Hierarchy 窗口空白处右键选择 **`Create Empty`** 新建空物体，重命名为 **`VRPlayer`**。
2. 将场景已有的 **`OVRCameraRig`** 拖入 `VRPlayer` 下方作为其子物体。
3. 选中 `VRPlayer` 物体，在右侧 Inspector 中：
   * 点击 `Add Component`，添加 **`CharacterController`**（用于玩家物理体积与行走地面检测）。
   * 点击 `Add Component`，添加 **`VRPlayerRig`** 脚本。
   * 将子物体 `OVRCameraRig` 拖入 `VRPlayerRig` 的 **`Camera Rig`** 属性框中进行绑定。
   * *(注：此时你的左手柄摇杆已自动映射为手柄向导移动，右手柄摇杆已映射为防眩晕的 Snap Turn 瞬移偏转视角)*

### 第三步：修复手部隐形与洋红色材质丢失
由于 HDRP 高清管线与 Oculus SDK 默认的 Standard 材质不兼容，手部模型在运行时可能会隐形，且部分道具材质会报洋红色（粉红色）。
1. 选中场景中的 **`OVRCameraRig`** 物体，在其 Inspector 最底部点击 `Add Component`。
2. 搜索并添加 **`HDRPMaterialFixer`** 脚本。
   * *(注：游戏运行后，该脚本会自动遍历并将其下所有不兼容的材质热替换为 `HDRP/Lit` 或 `HDRP/Unlit`，使你的双手瞬间显现并拥有正常的肤色材质)*
3. 如果场景中还有其他道具（例如 `UIHelpers` 或特定的故障箱子）呈现粉红色：
   * 选中该物体，在其 Inspector 底部同样添加挂载 **`HDRPMaterialFixer`** 脚本即可。
