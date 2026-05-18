# CLAUDE.md

本文件是本项目的行动准则，所有开发工作必须遵循。

---

## 项目基本信息

**项目名称**：生息守护：VR赋能应急救护一体化平台
**项目类型**：Unity HDRP VR急救培训系统
**Unity版本**：2022.3.62f3
**渲染管线**：HDRP（High Definition Render Pipeline）
**目标平台**：VR一体机（APK）+ Web端 + 微信小程序端

### 项目简介

本项目是一款基于VR技术的沉浸式急救技能培训平台，核心功能包括：
- 心肺复苏（CPR）训练与自动量化考评
- 自动体外除颤器（AED）使用训练
- VR手柄 + 手机相机双端协同动作捕捉与评分
- 基于RAG架构的AI急救知识问答
- 动态病程推演与个性化学情画像

### 技术栈

| 模块 | 技术 |
|------|------|
| VR客户端 | Unity 2022.3 + HDRP + VRTK / XR Interaction Toolkit |
| 后端服务 | Python Flask + WebSocket |
| 数据库 | MySQL（双库：管理员/用户）+ Milvus向量数据库 |
| Web前端 | Vue3 + ECharts |
| 微信小程序 | MediaPipe姿态识别 + WebRTC |
| AI | RAG架构（Sentence-Transformer + LLM API） |

### 项目成员分工

| 角色 | 负责 |
|------|------|
| 队长 | Unity VR客户端开发、项目统筹 |
| Unity队员 | VR交互、VRTK开发 |
| AI队员 | Python后端、RAG引擎、病程推演、学情分析 |
| 前端队员 | Web前端开发、数据可视化 |
| 后端队员 | 云端服务器、API接口、数据库 |

---

## LLM行为准则（强制）

以下准则来自 `LLM准则.txt`，是所有开发工作的底层约束：

### 1. Coding前先思考

**不要假设。不要隐藏困惑。要主动提出权衡方案。**

实现前：
- 明确陈述你的假设。不确定时要主动提问
- 存在多种解释时，说明所有可能方案，不要沉默地选一个
- 存在更简单的方案时要指出，有必要时提出反对
- 有不明确之处要停下来，指出哪里不清楚并询问

### 2. 简洁优先

**最少代码解决当前问题。不做推测性实现。**

- 不做超出需求的功能
- 单次使用的代码不抽象
- 不添加"灵活性"或"可配置性"（未请求的不要加）
- 不为不可能发生的场景写错误处理
- 如果写了200行而可以50行解决，就重写

自问："一位高级工程师会觉得这太复杂了吗？"如果答案是是，就简化。

### 3. 精准改动

**只触碰必须改动的。清理自己造成的混乱。**

编辑现有代码时：
- 不"改进"相邻代码、注释或格式
- 不重构没坏的东西
- 匹配现有风格，即使你可能有不同偏好
- 发现无关死代码时：指出，不删除

你的改动造成孤儿代码时：
- 删除因你的改动而未使用的import/变量/函数
- 不删除既有死代码（除非被要求）

检验标准：**每一行改动都能直接追溯到用户请求。**

### 4. 目标驱动执行

**定义成功标准。循环验证直到完成。**

将任务转化为可验证的目标：
- "添加验证" → "先写无效输入的测试，再让它们通过"
- "修复bug" → "写能复现bug的测试，再让它通过"
- "重构X" → "确保前后测试都通过"

多步骤任务应简述计划：
```
1. [步骤] → 验证：[检查点]
2. [步骤] → 验证：[检查点]
3. [步骤] → 验证：[检查点]
```

强成功标准让你能独立循环。弱标准（"让它能工作"）需要不断确认。

---

## 项目特定开发规范

### Unity/C# 规范

- 所有UI面板脚本命名：`[功能]Panel.cs`
- 管理类脚本命名：`[功能]Manager.cs`（单例模式）
- 输入抽象：`InputManager` 统一管理键盘和VR输入，子类实现 `KeyboardInput` / `VRInput`
- 场景切换通过 `UIManager.SwitchState(GameState.XXX)` 管理
- 不要在Update中做昂贵的运算；按压检测等高频率逻辑使用协程或固定更新
- HDRP使用，请确保HDRP设置不被意外覆盖（尤其是颜色空间、Render Scale）

### UI 开发规范

- 详见 `UI指导.md`
- Prefab命名：`UI_[界面名]_[变体].prefab`
- 所有UI面板预设要能在键盘模式和VR模式下通用（通过InputManager抽象差异）

### 分支管理

- 默认分支：`main`
- 功能开发请在对应分支进行，**不要直接在main上开发**
- 提交前确保Unity能正常编译，无编译错误

### 目录结构（目标）

```
Assets/
├── Scripts/
│   ├── UI/           # UI面板脚本
│   ├── Input/        # 输入抽象层
│   ├── Game/         # 游戏逻辑（状态机、计分）
│   ├── Audio/        # 音频管理
│   ├── Service/      # 服务接口与本地实现
│   │   ├── ILoginService.cs
│   │   ├── IVideoProvider.cs
│   │   ├── IScoreRepository.cs
│   │   ├── LocalLoginService.cs
│   │   ├── LocalVideoProvider.cs
│   │   ├── LocalScoreRepository.cs
│   │   └── ServiceLocator.cs
│   └── [其他模块]/    # 随着项目推进扩展
├── Prefabs/
│   ├── Environment/  # 环境模型
│   ├── Props/        # 可交互道具
│   └── UI/           # UI预设
├── Scenes/
│   ├── Subway/       # 地铁站场景
│   ├── Hospital/     # 医院场景
│   └── [其他]/       # 其他场景
├── Materials/        # 材质
├── Audio/            # 音效、语音
├── StreamingAssets/
│   └── Videos/       # 导学视频（随APK打包）
└── Textures/         # 纹理贴图
```

### 本地Mock架构（后端未就绪期间）

> 后端接口开发完成后，只需替换实现类，业务代码不动。

| 数据 | 接口 | 本地实现 | 存储位置 | 后端替换 |
|------|------|---------|---------|---------|
| 账号 | ILoginService | LocalLoginService | PersistentDataPath/accounts.json | ApiLoginService |
| 视频 | IVideoProvider | LocalVideoProvider | StreamingAssets/Videos/ | StreamVideoProvider |
| 成绩 | IScoreRepository | LocalScoreRepository | PersistentDataPath/Scores/*.json | ApiScoreRepository |

**切换方式**：`ServiceLocator.Awake()` 中实例化对应实现类，后端就绪后改3行代码即可。

**约束**：
- 业务代码只依赖接口（ILoginService等），不直接引用本地实现类
- 所有后端数据交互必须经过接口，禁止绕过接口直接读写文件
- 视频文件放StreamingAssets（随APK打包，只读）
- 账号和成绩放PersistentDataPath（可读写，跨平台）


## 有效准则的衡量标准

- diff中的不必要改动减少
- 因过度复杂导致的返工减少
- 澄清性问题出现在实现之前，而非错误之后
- 现在以及后续阶段都同步更新CLAUDE.md

---

## 当前开发阶段（2026-05-19 更新）

> **开发策略**：InputManager不作为一次性开发，按需分段实现。先完成纯UI面板，再逐步构建输入抽象层。每个小阶段完成后找用户确认。

---

### 已完成

**阶段零：基础设施（完成）**
- 服务接口：ILoginService、IVideoProvider、IScoreRepository
- 本地实现（LocalLoginService、LocalVideoProvider、LocalScoreRepository）
- ServiceLocator单例 + UIManager + GameState枚举 + GameStateMachine

**阶段一：UI面板脚本 + 场景建设（完成）**
- 8个Panel脚本（均含 AutoBind 自动绑定）：LoginPanel、HomeMenuPanel、SettingsPanel、HelpPanel、StudyVideoPanel、SceneSelectPanel、SkillSelectPanel、ScoreReportPanel
- UIManager.SwitchState() 管理所有面板显隐
- 独立 UI 场景：`Assets/Scenes/Login Scene.unity`
- 所有 Panel 居中锚定，布局已优化
- 所有文字为英文，字体使用 LiberationSans SDF
- Inspector 字段均通过编辑器脚本自动绑定，无 None 引用

**阶段一验证通过项：**
- 视频播放：cprdemo.mp4 正常播放，进度条拖拽跳转正常
- 文件命名：`StreamingAssets/Videos/cprdemo.mp4`

---

### 已知问题

1. **Training 状态无对应面板**（阶段二内容）
2. **不确定登录后触发逻辑是否符合预期**（登录成功 → SwitchState 跳转 HomeMenuPanel，还是应该只显示 Panel 不跳场景？）

---

### 当前文件夹结构（2026-05-19 整理后）

```
Assets/
├── Scenes/
│   ├── Subway/
│   │   └── Demonstration.unity   ← 地铁站场景
│   └── UI/
│       └── Login Scene.unity     ← 独立UI场景
├── Scripts/
│   ├── UI/           # 8个Panel脚本 + UIManager
│   ├── Service/      # 接口 + 本地实现 + ServiceLocator + ScoreData
│   └── Game/         # GameState + GameStateMachine
├── Subway/
│   ├── Models/       # 8个 .fbx 模型
│   ├── Materials/    # 地铁材质
│   ├── Prefabs/      # 87个地铁预制体
│   ├── Textures/     # 149张贴图
│   ├── Meshes/       # 187个网格
│   └── Lighting/     # HDRP光照烘焙数据
├── Prefabs/
│   ├── Common/       # 通用预制体 (Cube)
│   └── UI/           # UI预制体
├── StreamingAssets/Videos/
│   └── cprdemo.mp4   ← H.264 baseline MP4, 播放及拖拽正常
├── Fonts/            # LiberationSans SDF (来自 TMP Resources)
└── (Unity系统目录: Engine, HDRPDefaultResources, TextMesh Pro, XR, Shaders, Editor)
```

---

### 待开发（按顺序）

**阶段一收尾：**
- [ ] Step 1.9 整合测试（在 Login Scene 中验证完整流程）

**阶段二（游戏）：InputManager + 游戏UI**
- Step 2.1 InputManager → 2.2 TrainingHUD → 2.3 ErrorPopup → 2.4 RealtimeScorePanel → 2.5 整合测试

**阶段三（VR）：VR扩展** ⏳ VR设备到位后

**阶段四（后端）：后端切换** ⏳ 后端接口就绪后
