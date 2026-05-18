# UI 开发指导方案

> 本方案针对"生息守护：VR赋能应急救护一体化平台"，覆盖当前键盘模拟阶段与VR设备阶段。

---

## 一、架构概览

```
┌─────────────────────────────────────────────────────────┐
│                      UI 层（Canvas）                     │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ 主菜单UI │ │ 训练UI  │ │ 成绩UI  │ │ VR键盘UI │       │
│  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘       │
│       │           │           │           │             │
│  ┌────▼───────────▼───────────▼───────────▼────┐       │
│  │              UIManager (单例)                │       │
│  └────────────────────┬────────────────────────┘       │
│                       │                                  │
│  ┌────────────────────▼────────────────────────┐       │
│  │           InputManager (抽象输入层)          │       │
│  └────────────────────┬────────────────────────┘       │
│            ┌───────────┴───────────┐                    │
│     ┌──────▼──────┐        ┌──────▼──────┐             │
│     │ KeyboardInput│        │ VRInput     │             │
│     │  (当前)      │        │ (VR设备)    │             │
│     └─────────────┘        └─────────────┘             │
└─────────────────────────────────────────────────────────┘
```

---

## 二、输入抽象层设计

### 2.1 核心接口

所有输入通过 `InputManager` 单例统一管理，键盘模式和VR模式共用同一套API。

```csharp
// InputManager.cs — 核心输入抽象
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // ===== 当前阶段（键盘模式）=====
    // 移动：WASD
    public Vector3 GetMovement();
    // 视角：鼠标
    public Vector2 GetLook();
    // 交互：E键 / 左键点击
    public bool GetInteract();
    public bool GetInteractDown(); // 按下瞬间
    // 抓取：G键
    public bool GetGrab();
    public bool GetGrabDown();
    // 按压：C键（按住表示正在按压）
    public bool GetCompression();
    // 取消：ESC
    public bool GetCancel();
    // ===== VR阶段追加 =====

    // 当前是否为VR模式
    public bool IsVRMode { get; set; }

    // VR手柄位姿（VR阶段）
    public Transform GetLeftHandTransform();
    public Transform GetRightHandTransform();

    // VR模式下手柄按键
    public bool GetVRTrigger();
    public bool GetVRGrip();
}
```

### 2.2 键盘映射方案（当前阶段）

| 键盘按键 | 对应交互动作 | 说明 |
|---------|------------|------|
| W/A/S/D | 移动 | 第一人称移动 |
| 鼠标移动 | 视角转向 | 类似FPS |
| E | 交互 | 拾取/使用物品 |
| G | 抓取 | 抓取物品（按住） |
| C（按住） | 胸外按压 | 按住期间持续触发按压，松开停止 |
| C（短按） | AED放电 | 在AED流程中短按C触发放电 |
| Q | 下一操作提示 | 跳过当前语音指导步骤 |
| ESC | 取消/返回 | 取消当前操作/返回上级菜单 |
| Tab | 打开/关闭成绩面板 | 查看实时评分 |
| 1/2/3/4 | 快捷选择 | 主菜单快捷入口 |
| 空格 | 确认/继续 | 替代VR的"点击确认" |
| F | 切换第一人称/第三人称 | 调试用视角切换 |

---

## 三、UI 界面清单与开发步骤

### 3.1 界面清单

| ID | 界面名称 | 优先级 | 说明 |
|----|---------|--------|------|
| UI-01 | 主菜单（HomeMenu） | P0 | 首页，含导学视频、技能训练、设置、帮助入口 |
| UI-02 | 导学视频播放界面 | P1 | 播放CPR/AED教学视频 |
| UI-03 | 设置界面（Settings） | P1 | 音量调节、分辨率控制 |
| UI-04 | 帮助界面（Help） | P2 | 手柄/键位说明 |
| UI-05 | 场景选择界面 | P0 | 选择训练场景（地铁站/病房等） |
| UI-06 | 技能选择界面 | P0 | 选择CPR训练或AED训练 |
| UI-07 | 训练中HUD | P0 | 实时显示步骤提示、计时、评分 |
| UI-08 | 错误提示弹窗 | P0 | 红字弹窗 + 语音报错 |
| UI-09 | 成绩报告界面 | P0 | 训练结束后的完整报告 |
| UI-10 | 登录界面 | P1 | 账号登录（VR阶段需要全息键盘） |
| UI-11 | VR键盘（VirtualKeyboard） | P1 | VR环境中的虚拟键盘 |
| UI-12 | 实时评分悬浮面板 | P1 | Tab切换显示，包含深度/频率/顺序等指标 |

---

## 四、后端Mock服务层

> 后端接口未就绪期间，通过接口抽象 + 本地实现的方式占位。后端就绪后只需替换实现类，业务代码不动。

### 4.1 接口定义

| 接口 | 职责 | 方法 |
|------|------|------|
| ILoginService | 账号管理 | Register(), Login(), IsLoggedIn, CurrentUser |
| IVideoProvider | 视频路径 | GetVideoPath(videoId), HasVideo(videoId) |
| IScoreRepository | 成绩存取 | SaveScore(), GetUserScores(), GetLatestScore() |

### 4.2 本地实现

| 接口 | 实现类 | 存储位置 |
|------|--------|---------|
| ILoginService | LocalLoginService | PersistentDataPath/accounts.json |
| IVideoProvider | LocalVideoProvider | StreamingAssets/Videos/ |
| IScoreRepository | LocalScoreRepository | PersistentDataPath/Scores/*.json |

### 4.3 统一入口

`ServiceLocator` 单例提供所有服务的访问入口，Awake中决定实例化哪个实现类。

---

## 五、开发步骤（分阶段）

> **设计原则**：
> 1. InputManager不作为一次性开发，按需分段实现
> 2. 每个小阶段完成后找用户确认，再进入下一阶段
> 3. 服务接口先定义，本地实现先写，后端就绪后替换

### ⚠️ 当前状态（2026-05-19）

**已完成：**
- 阶段零全部完成（接口+本地实现+ServiceLocator）
- 阶段一8个Panel脚本已完成（含AutoBind自动绑定）
- 独立UI场景 `Assets/Scenes/Login Scene.unity` 已创建并完成迁移
- 所有Panel布局已优化，居中锚定，层次清晰
- Inspector字段已通过编辑器脚本自动绑定
- 所有文字英文，字体使用 LiberationSans SDF

**已知问题：**
1. ❌ Training状态无对应面板（阶段二内容）

**下一步：在独立UI场景中执行 Step 1.9 整合测试**

---

### 文件夹结构（2026-05-19 整理后）✅

已按场景隔离方案整理完毕：
```
Assets/
├── Scenes/Subway/Demonstration.unity    ← 地铁站
├── Scenes/UI/Login Scene.unity         ← 独立UI
├── Subway/{Models,Materials,Prefabs,Textures,Meshes,Lighting}/
├── Prefabs/Common/ + Prefabs/UI/
└── Scripts/, StreamingAssets/, Fonts/ (共享)
```

---

### 阶段零：基础设施（前置✅）

#### Step 0.1 — 服务接口与本地实现 ✅
- 创建 `Assets/Scripts/Service/` 目录
- 定义三个接口：ILoginService、IVideoProvider、IScoreRepository
- 实现三个本地类：LocalLoginService、LocalVideoProvider、LocalScoreRepository
- 创建 ServiceLocator 单例
- 在 StreamingAssets/Videos/ 下放入测试视频文件
- **验证**：ServiceLocator.Instance 能正确获取三个服务实例
- **⏸ 确认点**：接口定义是否合理？方法是否够用？

### 阶段一：纯UI面板（无InputManager依赖）

所有交互通过Unity EventSystem原生支持（按钮点击、滑块拖拽等），ESC/Tab快捷键在UIManager中直接使用`Input.GetKeyDown`。

#### Step 1.1 — 登录界面（LoginPanel）UI-10 ✅
- 输入框：用户名 + 密码（用系统键盘）
- 登录按钮 → 调用 ILoginService.Login()
- 注册按钮 → 调用 ILoginService.Register()
- 离线模式（跳过登录直接进入主菜单）
- **验证**：注册→登录→进入主菜单流程通畅
- **⏸ 确认点**：登录UI布局和交互是否满意？

#### Step 1.2 — 主菜单（HomeMenu）UI-01 ✅
- 创建 Canvas + EventSystem
- 四个按钮：导学视频、技能训练、设置、帮助
- 键盘快捷键：1/2/3/4
- **验证**：四个按钮能正确跳转对应面板
- **⏸ 确认点**：主菜单布局和按钮样式是否满意？

#### Step 1.3 — 设置界面（Settings）UI-03 ✅
- 音量滑块：主音量、语音音量、背景音量
- 分辨率下拉（仅窗口模式有效）
- 数据重置按钮（清除本地成绩 → IScoreRepository）
- **验证**：滑块拖动有反馈，数据重置能清空本地文件
- **⏸ 确认点**：设置项是否完整？布局是否满意？

#### Step 1.4 — 帮助界面（Help）UI-04 ✅
- 图示键盘/手柄按键说明
- **验证**：按键说明与UI指导.md中的映射表一致
- **⏸ 确认点**：帮助内容是否清晰？

#### Step 1.5 — 导学视频（StudyVideo）UI-02 ✅
- 全屏播放VideoPlayer组件
- 视频路径通过 IVideoProvider.GetVideoPath() 获取
- 支持播放/暂停、进度条
- **验证**：能播放StreamingAssets/Videos/下的测试视频
- **⏸ 确认点**：视频播放控件是否满足需求？

#### Step 1.6 — 场景选择（SceneSelect）UI-05 ✅
- 卡片式UI，当前仅"地铁站"可用，其他锁定或灰显（占位）
- **验证**：选择地铁站后能跳转到技能选择
- **⏸ 确认点**：卡片布局和锁定状态显示是否满意？

#### Step 1.7 — 技能选择（SkillSelect）UI-06 ✅
- CPR训练 / AED训练 两个入口
- **验证**：选择技能后能跳转到训练（暂时跳转到主菜单占位）
- **⏸ 确认点**：技能选择的布局和交互是否满意？

#### Step 1.8 — 成绩报告（ScoreReport）UI-09 ✅
- 训练结束后显示完整报告
- 内容：总分、各步骤扣分明细、正确率、建议改进项
- 成绩数据通过 IScoreRepository 读取
- 按钮：重新训练 / 返回主菜单
- **验证**：面板能正常显示和关闭
- **⏸ 确认点**：报告的布局和数据展示是否满意？

#### Step 1.9 — 阶段一整合测试（待执行）
- 全流程导航测试：登录 → 主菜单 → 各子面板跳转 → 返回
- 确保所有UI状态正确切换，无死锁
- ✅ 前置条件已满足（视频可播放，进度条可拖拽）
- **⏸ 确认点**：整体流程是否通畅？有无需要调整的地方？

### ✅ 阶段一重构（已完成）

> UI已从地铁站场景中拆分。

#### Step R.1 — 创建独立UI场景 ✅
- 已创建 `Assets/Scenes/Login Scene.unity`
- UIManager + LoginCanvas + 8个Panel 已迁移
- Demonstation.unity 中地铁站模型保留

#### Step R.2 — 修复布局 ✅
- 所有8个Panel居中锚定，尺寸/间距已优化
- ScoreReportPanel 包含 DetailPanel 子面板
- HelpPanel 包含 KeyboardHelp/VRHelp 双模式子面板

#### Step R.3 — 文件夹结构 ⚠️
- Scenes/ 目录已创建
- 其余目录待后续整理

### 阶段二：InputManager基础层 + 游戏UI

#### Step 2.1 — InputManager.cs 核心输入方法
- 实现游戏交互所需的输入抽象：
  - `GetMovement()` — WASD移动
  - `GetLook()` — 鼠标视角
  - `GetInteract()` / `GetInteractDown()` — E键交互
  - `GetGrab()` / `GetGrabDown()` — G键抓取
  - `GetCompression()` — C键按压（按住持续触发）
  - `GetCancel()` — ESC取消
- CompressionDetector迁移为通过InputManager获取输入
- **验证**：WASD移动、鼠标转向、E键交互、C键按压均正常
- **⏸ 确认点**：输入响应是否灵敏？键位是否合理？

#### Step 2.2 — 训练中HUD（TrainingHUD）UI-07
- 左上角：当前步骤文字提示（"请检查患者呼吸"）
- 右上角：计时器（秒表）
- 底部中央：按压深度条 + 按压频率指示（数据来自CompressionDetector）
- **验证**：进入训练后HUD正确显示按压数据
- **⏸ 确认点**：HUD布局和信息密度是否合适？

#### Step 2.3 — 错误提示（ErrorPopup）UI-08
- 检测到错误操作时，从屏幕中央弹出红色文字提示
- 配合语音报错（AudioSource播放）
- 2秒后自动消失，或按ESC立即关闭
- **验证**：故意操作错误（如按压深度不足），验证弹窗出现
- **⏸ 确认点**：弹窗的样式和持续时间是否合适？

#### Step 2.4 — 实时评分（RealtimeScorePanel）UI-12
- Tab键切换显示/隐藏
- 包含深度、频率、顺序等指标
- **验证**：训练中Tab能切换评分面板
- **⏸ 确认点**：评分面板的布局和数据展示是否清晰？

#### Step 2.5 — 阶段二整合测试
- 全流程贯通测试：主菜单 → 场景选择 → 技能选择 → 训练 → 报错/纠正 → 结束 → 报告 → 返回主菜单
- 确保所有UI状态正确切换，无死锁
- **⏸ 确认点**：完整训练流程是否通畅？有无需要调整的地方？

### 阶段三：VR扩展（VR设备到位后）

#### Step 3.1 — InputManager VR方法扩展
- 在InputManager中追加VR输入方法：
  - `GetVRTrigger()` — 交互/确认
  - `GetVRGrip()` — 抓取
  - `GetLeftHandTransform()` / `GetRightHandTransform()` — 手柄位姿
- `InputManager.IsVRMode = true` 时自动切换到VR输入
- **验证**：连接VR后手柄控制生效
- **⏸ 确认点**：VR输入映射是否合理？

#### Step 3.2 — VR键盘（VirtualKeyboard）UI-11
- VR空间中渲染3D键盘模型
- 用手柄射线选择按键
- **验证**：VR中能用射线打字
- **⏸ 确认点**：键盘的按键大小和射线精度是否合适？

#### Step 3.3 — VRTK/XR Interaction Toolkit 集成
- 将键盘模式的Raycaster替换为XR射线交互
- 将EventSystem的鼠标输入替换为XR Controller输入
- **验证**：VR中能正常移动、交互所有UI
- **⏸ 确认点**：VR交互体验是否流畅？

#### Step 3.4 — 震动反馈
- 错误操作时手柄震动
- AED放电时手柄震动
- 按压节奏感震动反馈
- **验证**：各项操作触发对应震动
- **⏸ 确认点**：震动反馈是否提升了操作体验？

### 阶段四：后端切换（后端接口就绪后）

#### Step 4.1 — 实现ApiLoginService
- 调用后端HTTP接口实现登录/注册
- **验证**：能用后端账号登录
- **⏸ 确认点**：登录流程是否正常？

#### Step 4.2 — 实现StreamVideoProvider
- 返回流媒体URL替代本地路径
- **验证**：视频能从服务器流式播放
- **⏸ 确认点**：视频加载速度是否可接受？

#### Step 4.3 — 实现ApiScoreRepository
- 训练成绩上传到服务端
- 历史成绩从服务端拉取
- **验证**：成绩数据跨设备同步
- **⏸ 确认点**：数据同步是否正常？

#### Step 4.4 — ServiceLocator切换
- 将ServiceLocator中的本地实现替换为API实现
- **验证**：全流程使用后端服务跑通
- **⏸ 确认点**：切换后是否有遗漏或异常？

---

## 六、UI 组件规范

### 6.1 Canvas 设置
- **Render Mode**：Screen Space - Overlay（2D UI）或 Screen Space - Camera（需要深度检测时）
- **Reference Resolution**：1920 × 1080
- **Match**：Width 或 Height 根据目标设备调整

### 6.2 字体
- **中文主字体**：思源黑体（Noto Sans SC）或系统默认雅黑
- **数字/英文**：Roboto 或 Arial
- **评分数字**：等宽字体（Roboto Mono）

### 6.3 颜色规范
```
主色调（Primary）：#00B4D8（医疗蓝）
警告色（Warning）：#FF4444（错误红）
成功色（Success）：#44FF88（正确绿）
提示色（Hint）：   #FFD700（金黄）
背景色（Background）：#1A1A2E（深蓝黑）
面板背景：rgba(10, 10, 30, 0.85)（半透明）
文字色：#FFFFFF / #E0E0E0
```

### 6.4 命名规范
- Prefab名：`UI_[界面名]_[变体].prefab`
- 脚本名：`[界面名]Panel.cs`（对应一个界面）或 `[功能]Component.cs`（可复用组件）
- 场景内UI对象：`[PanelName]_[ElementName]`（如 `HomeMenu_BtnStudy`）

---

## 七、状态机与UI联动

### 7.1 游戏状态定义

```csharp
public enum GameState
{
    MainMenu,       // 主菜单
    SceneSelect,    // 场景选择
    SkillSelect,    // 技能选择
    StudyVideo,     // 导学视频
    Training,        // 训练中
    Paused,          // 暂停
    ScoreReport,     // 成绩报告
    Settings,        // 设置
    Help,            // 帮助
    Login,           // 登录
}
```

### 7.2 UIManager 状态切换

```csharp
// UIManager.cs
public void SwitchState(GameState newState)
{
    // 1. 隐藏所有UI面板
    HideAllPanels();

    // 2. 根据状态显示对应面板
    switch (newState)
    {
        case GameState.MainMenu:    homeMenu.SetActive(true); break;
        case GameState.Training:    trainingHUD.SetActive(true); break;
        // ...
    }

    // 3. 通知 GameStateMachine
    if (GameStateMachine.Instance != null)
        GameStateMachine.Instance.Transition(newState);
}
```

### 7.3 键盘快捷键全局监听

```csharp
// 在 UIManager.Update() 中
void Update()
{
    // ESC 全局处理
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        HandleEscape();
    }

    // Tab 切换评分面板
    if (Input.GetKeyDown(KeyCode.Tab))
    {
        realtimeScorePanel.Toggle();
    }
}
```

---

## 八、过渡阶段：键盘 ↔ VR 自动切换

```csharp
// InputManager.cs
void Awake()
{
    // 检测VR是否连接
#if UNITY_XR
    IsVRMode = XRDevice.isPresent;
#else
    IsVRMode = false;
#endif
}
```

- 当VR设备连接并启用时，自动切换到VR模式
- 断开VR时，自动回退到键盘模式
- UI不需重新加载，由 `InputManager` 屏蔽底层差异

---

## 九、开发检查清单

### 键盘阶段完成标准
- [ ] 所有UI界面能正常打开和关闭（所有Panel都向我确认完毕后再确定完成）
- [ ] WASD移动 + 鼠标视角正常
- [ ] E键交互：拾取/使用物品正常
- [ ] G键抓取：按住抓取、松开释放
- [ ] C键按压：按住期间持续触发，松开停止
- [ ] 完整训练流程（CPR或AED）能走通
- [ ] 错误检测和弹窗正常触发
- [ ] 成绩报告正确显示
- [ ] ESC返回上级菜单正常
- [ ] Tab切换评分面板正常

### VR阶段完成标准
- [ ] 手柄移动和视角控制正常
- [ ] 射线交互替代鼠标
- [ ] 抓取/按压手柄按键映射正确
- [ ] 全息键盘能正常输入
- [ ] 震动反馈正确触发
- [ ] 键盘模式回退机制正常
