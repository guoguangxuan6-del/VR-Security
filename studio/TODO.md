# TODO · Life Guard VR

> 最后更新：2026-07-23

---

## 🔴 当前阻塞

| # | 问题 | 原因 | 下一步 |
|---|------|------|--------|
| 1 | Play mode 完整测试 | 导航重构 + API 服务层改动 | 完整走一遍登录→技能→场景→跳转流程 |
| 2 | 场景手动保存 | 编辑器改动未 Ctrl+S | 用户保存场景 |

---

## 🟡 待修复（已知）

| # | 问题 | 位置 | 备注 |
|---|------|------|------|
| 3 | 50 个 Unreal 着色器编译失败 | `Assets/Shaders/*.shader` | main 基线固有，枚举命名空间过时 |
| 4 | `GameStateMachine 2.prefab` 缺失脚本 | `Assets/Prefabs/UI/` | main 基线固有 |
| 5 | `UIManager` 未 `DontDestroyOnLoad` | `Assets/Scripts/UI/UIManager.cs` | DB-3 已决，代码未加 |
| 6 | Demonstration.unity 不在 Build Settings | `ProjectSettings/EditorBuildSettings.asset` | 编辑器可运行，打包需添加 |
| 7 | StudyVideo RenderTexture 未释放 | `Assets/Scripts/UI/StudyVideoPanel.cs` | D7 待整改 |
| 8 | 明文密码（D3b） | `Assets/Scripts/Service/LocalLoginService.cs` | 后端切换前必改（S级） |

---

## 🟢 近期可推进

| # | 内容 | 依赖 |
|---|------|------|
| 9 | UIManager DontDestroyOnLoad（P5S-4.1） | 无 |
| 10 | CompressionDetector 深度映射（P5S-1.1） | D2 决策 |
| 11 | ScoreCalculator 统一函数（P5S-3.1） | Epic 1 |
| 12 | TrainingHUD（P5S-2.1） | Epic 1 |
| 13 | ErrorPopup（P5S-2.2） | P5S-1.1 |
| 14 | RealtimeScorePanel（P5S-2.3） | P5S-3.1 |

---

## 🔵 后端集成（2026-07-23 已接入）

| # | 内容 | 状态 |
|---|------|------|
| 15 | API 服务层 async/await 接入 `http://123.57.30.132:8080` | ✅ 完成，待 Play Mode 测试 |
| 16 | 登录/注册/视频/成绩 API | ✅ 完成 |
| 17 | 头像服务 API | ✅ 完成 |
| 18 | 场景列表动态加载 | 后端就绪，待接入 UI |
| 19 | 知识库 HelpPanel 接入 | 后端就绪，待接入 UI |
| 20 | 智能问答 | 需后端配置 AI API Key |
| 21 | 姿态识别训练评估 | 后端就绪，待接入 UI |

---

## ✅ 已完成（2026-07-23）

- [x] 11 面板导航系统（NavigateTo/GoBack/ResetTo）
- [x] HomePanel + HomeMenuPanel + UserInfoBar
- [x] InputField 字体修复（Deng SDF）+ 光标隐藏（alpha=0 + CustomCaretColor）
- [x] 技能→场景→地铁站跳转链路
- [x] 淡入淡出动效 + Z-order 遮挡修复
- [x] Editor 破坏性工具清理（Migrator/Builder/NPCSimulator 全删）
- [x] LoginPanel/RegisterPanel `setOnClickListener` → `onClick.AddListener`
- [x] 后端 API 服务层（ApiClient / ApiLogin / ApiVideo / ApiScore / ApiAvatar / Dto）
- [x] 所有接口 async/await 化 + Local 实现保留
- [x] TerminalCanvas `planeDistance=100` → `0`（修复密码框点击）
- [x] TMP Settings 默认字体 GUID 修正
- [x] VRVirtualKeyboardHelper 条件防护（#if !ENABLE_VR）
