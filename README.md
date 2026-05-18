# 生息守护：VR赋能应急救护一体化平台

Unity HDRP VR急救培训系统 | Unity 2022.3.62f3 | HDRP

## 项目简介

基于VR技术的沉浸式急救技能培训平台，核心功能：
- 心肺复苏（CPR）训练与自动量化考评
- 自动体外除颤器（AED）使用训练
- VR手柄 + 手机相机双端协同动作捕捉与评分
- 基于RAG架构的AI急救知识问答
- 动态病程推演与个性化学情画像

## 技术栈

| 模块 | 技术 |
|------|------|
| VR客户端 | Unity 2022.3 + HDRP + XR Interaction Toolkit |
| 后端服务 | Python Flask + WebSocket |
| 数据库 | MySQL + Milvus向量数据库 |
| Web前端 | Vue3 + ECharts |
| 微信小程序 | MediaPipe姿态识别 + WebRTC |
| AI | RAG架构（Sentence-Transformer + LLM API） |

## 当前开发进度（2026-05-19）

### 阶段零：基础设施 ✅
- 服务接口：ILoginService、IVideoProvider、IScoreRepository
- 本地Mock实现（后端未就绪期间的占位方案）
- ServiceLocator单例 + UIManager面板管理 + GameState状态机

### 阶段一：UI面板框架 ✅
- 8个Panel脚本（均含自动绑定）：LoginPanel、HomeMenuPanel、SettingsPanel、HelpPanel、StudyVideoPanel、SceneSelectPanel、SkillSelectPanel、ScoreReportPanel
- 独立UI场景：`Assets/Scenes/UI/Login Scene.unity`
- 视频播放：支持进度条拖拽跳转
- 字体：LiberationSans SDF，所有文字英文

### 阶段二：InputManager + 游戏UI（待开发）
- InputManager输入抽象层
- TrainingHUD、ErrorPopup、RealtimeScorePanel
- 按压检测与评分逻辑

### 阶段三：VR扩展 ⏳
### 阶段四：后端切换 ⏳

## 文件夹结构

```
Assets/
├── Scenes/
│   ├── Subway/Demonstration.unity    ← 地铁站场景
│   └── UI/Login Scene.unity         ← 独立UI场景
├── Scripts/
│   ├── UI/          ← 8个Panel + UIManager
│   ├── Service/     ← 接口 + 本地实现 + ServiceLocator
│   └── Game/        ← GameState + GameStateMachine
├── Subway/
│   ├── Models/      ← 地铁站.fbx模型
│   ├── Materials/   ← 地铁站材质
│   ├── Prefabs/     ← 地铁站预制体
│   ├── Textures/    ← 地铁站贴图
│   ├── Meshes/      ← 地铁站网格
│   └── Lighting/    ← HDRP光照烘焙数据
├── Prefabs/
│   ├── Common/      ← 通用预制体
│   └── UI/          ← UI预制体
└── StreamingAssets/Videos/  ← 导学视频
```

新场景（医院、灾区等）只需在 `Scenes/` 和 `Assets/` 下新建对应文件夹即可。

## 本地Mock架构

后端接口未就绪，通过接口抽象 + 本地实现占位：

| 数据 | 接口 | 本地实现 | 存储位置 |
|------|------|---------|---------|
| 账号 | ILoginService | LocalLoginService | PersistentDataPath/accounts.json |
| 视频 | IVideoProvider | LocalVideoProvider | StreamingAssets/Videos/ |
| 成绩 | IScoreRepository | LocalScoreRepository | PersistentDataPath/Scores/*.json |

后端就绪后只需在 ServiceLocator.Awake() 中替换实现类即可，业务代码不动。

## 开发规范

- UI面板脚本：`[功能]Panel.cs`，管理类：`[功能]Manager.cs`（单例）
- 输入通过InputManager统一管理，场景切换通过UIManager.SwitchState
- 不要在Update中做昂贵运算，使用协程或固定更新
- HDRP设置不被意外覆盖（颜色空间、Render Scale）
- 功能开发在对应分支进行，提交前确保编译无错误

## 分支

- 主分支：`main`
- 默认分支：`main`
