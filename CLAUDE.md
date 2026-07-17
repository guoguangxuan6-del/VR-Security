# CLAUDE.md

本文件是本项目的行动准则，所有开发工作必须遵循。

---

## 项目基本信息

**生息守护：VR赋能应急救护一体化平台** | Unity 2022.3.62f3 | HDRP | VR急救培训

---

## 开发文件索引

| 文件 | 内容 | 何时查阅 |
|------|------|---------|
| `UI指导.md` | UI架构、面板清单、开发步骤、颜色/字体规范、Mock服务层 | 任何UI相关开发 |
| `一阶段优化意见.md` | 触发流程重构、代码架构优化、UX改进（已实施） | 优化参考 |
| `一阶段测试流程.md` | 完整 UI 测试清单（9类 40+ 项） | Play 测试前必读 |
| `LLM准则.txt` | LLM行为准则完整版 | 参考 |

---

## LLM行为准则（强制）

1. **Coding前先思考** — 不确定时主动提问，存在多种解释时列出方案，不沉默选一个
2. **简洁优先** — 最少代码解决问题，不做推测性实现，不抽象单次使用的代码
3. **精准改动** — 只碰必须改的，匹配现有风格，你的改动造成的孤儿代码自己清理
4. **目标驱动** — 将任务转为可验证目标，多步骤任务简述计划+检查点

检验标准：**每一行改动都能直接追溯到用户请求。**

---

## 项目开发规范

### Unity/C#

- Panel 脚本：`[功能]Panel.cs` | 管理类：`[功能]Manager.cs`（单例）
- 输入抽象：`InputManager` → `KeyboardInput` / `VRInput`
- 场景切换：`UIManager.SwitchState(GameState.XXX)`
- 不在 Update 中做昂贵运算；高频率逻辑用协程或固定更新
- HDRP 设置不被意外覆盖（颜色空间、Render Scale）

### 服务接口约束（强制）

- 业务代码只依赖接口（`ILoginService` / `IVideoProvider` / `IScoreRepository`），不直接引用实现类
- 所有数据交互必须经过接口，禁止绕过接口直接读写文件
- 视频文件放 `StreamingAssets`（只读），账号成绩放 `PersistentDataPath`（可读写）
- 后端就绪后替换 `ServiceLocator.Awake()` 中 3 行实例化代码即可

### 当前 UI 架构（2026-05-24 更新）

**GameState 枚举：**
```csharp
Home, Lobby, SceneSelect, SkillSelect, StudyVideo, Training,
Paused, ScoreReport, Settings, Help, Login(弹窗), Register(弹窗)
```

**面板清单（11个 + 1基类）：**
| 脚本 | GameObject | 类型 |
|------|-----------|------|
| HomePanel | HomePanel | FullScreen（启动入口） |
| HomeMenuPanel | LobbyPanel | FullScreen（大厅，原HomeMenuPanel重命名） |
| SettingsPanel | SettingsPanel | FullScreen（含Apply/Cancel） |
| HelpPanel | HelpPanel | FullScreen |
| StudyVideoPanel | StudyVideoPanel | FullScreen |
| SceneSelectPanel | SceneSelectPanel | FullScreen |
| SkillSelectPanel | SkillSelectPanel | FullScreen |
| ScoreReportPanel | ScoreReportPanel | FullScreen |
| TrainingPlaceholderPanel | TrainingPlaceholderPanel | FullScreen（占位） |
| LoginPanel | LoginPanel | Popup（弹窗） |
| RegisterPanel | RegisterPanel | Popup（弹窗） |
| BasePanel | — | 抽象基类（OnEnter/OnExit/OnBack） |

**UIManager 能力：**
- `SwitchState(GameState)` — 全屏切换（压栈 + 生命周期 + Fade动画）
- `GoBack()` — 出栈返回
- `ShowPopup(GameState)` / `HidePopup()` — 弹窗叠加（Scale+Fade动画）
- `GameStateMachine` 已删除，状态追踪由 UIManager 统一管理
- ESC 快捷键：优先关弹窗，否则 GoBack

**BasePanel 生命周期：** `OnEnter(object data)` / `OnExit()` / `OnBack()`，所有 Panel 均继承。

**AutoBindEditor：** Inspector 按钮，按命名约定（camelCase字段→PascalCase子对象）自动填充 `[SerializeField]` 字段。

**面板过渡动画：** FullScreen Fade 0.25s / Popup Scale+Fade 0.2s 打开 0.15s 关闭，CanvasGroup+协程实现。

**Settings Apply/Cancel：** 有Apply/Cancel按钮时预览不持久化，确认后写PlayerPrefs，取消恢复原值。无按钮时兼容旧版直接写入。

### 分支管理

- 默认分支 `main`，功能开发在对应分支，不在 main 上直接开发
- 提交前确保 Unity 能正常编译
