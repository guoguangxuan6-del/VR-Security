# 项目进度 · Life Guard VR

> 最后更新：2026-07-23

## 当前分支

- **工作分支**：`feature/no-vr`（本机稳定编写沙盒，无 VR 包）
- **主线分支**：`main`（含 VR/XR 的团队主线备份）

## 今日完成（2026-07-23）

### 修复：TMP InputField 光标巨大 + 密码框点击无响应

| 问题 | 根因 | 修复 |
|------|------|------|
| 输入框光标覆盖整个高度 | `m_GlobalFontAsset: null` + TMP Settings 默认字体 GUID 指向不存在资产 | 修正 TMP Settings 默认字体 GUID；所有 InputField 分配 `Deng SDF` |
| 密码框点击无法输入 | `Canvas.planeDistance=100` 导致射线平面偏移，GraphicRaycaster 射线击不中 UI | `TerminalCanvas` `planeDistance` 改为 0 |
| 光标颜色未生效 | `m_CustomCaretColor: 0`（自定义光标颜色未启用） | 启用 `m_CustomCaretColor: 1` + `alpha=0` 隐藏光标 |

### 新增：后端 API 服务层（async/await）

将数据服务从本地 Mock 切换到真实后端 API（`http://123.57.30.132:8080`）。

**新增文件：**
- `Assets/Scripts/Service/ApiClient.cs` — HTTP 客户端基类（token 管理 + 信封解析）
- `Assets/Scripts/Service/ApiLoginService.cs` — 登录/注册 API
- `Assets/Scripts/Service/ApiVideoProvider.cs` — 视频服务 API
- `Assets/Scripts/Service/ApiScoreRepository.cs` — 成绩仓库 API
- `Assets/Scripts/Service/ApiAvatarService.cs` — 头像服务 API
- `Assets/Scripts/Service/Dto.cs` — 数据传输对象

**修改文件：**
- `ILoginService.cs` / `IScoreRepository.cs` / `IVideoProvider.cs` — 接口全异步化
- `LocalLoginService.cs` / `LocalScoreRepository.cs` / `LocalVideoProvider.cs` — 实现类适配
- `ScoreData.cs` — 增加 `stepDetailsJson` 字段
- `ServiceLocator.cs` — 用 Api 实现替换 Local 实现
- `LoginPanel.cs` / `RegisterPanel.cs` / `StudyVideoPanel.cs` / `ScoreReportPanel.cs` / `HomeMenuPanel.cs` / `SceneSelectPanel.cs` — UI 层适配 async

**保留文件（可回退）：**
- `LocalLoginService.cs` / `LocalScoreRepository.cs` / `LocalVideoProvider.cs`

### 关键决策

1. **服务接口全异步**：后端 HTTP 调用天然需要 async/await，所有三个服务接口改为返回 `Task`/`Task<T>`
2. **Local 实现保留**：`ServiceLocator.Awake()` 中切换 `new ApiXxx()` ↔ `new LocalXxx()` 即可回退
3. **Token 持久化**：JWT token 存入 `PlayerPrefs`，`IsTokenValid()` 检查过期
4. **统一错误处理**：`ParseResponse<T>` 在 HTTP/JSON 失败时返回 `code: -1` 信封，不抛异常

## 项目架构（当前）

### UI 层（11 面板 + 1 基类）

| 面板 | 类型 | 状态 |
|------|------|------|
| HomePanel | FullScreen（未登录入口） | ✅ 完成 |
| HomeMenuPanel | FullScreen（已登录大厅） | ✅ 完成 |
| LoginPanel | Popup（弹窗） | ✅ 完成 |
| RegisterPanel | Popup（弹窗） | ✅ 完成 |
| SettingsPanel | FullScreen | ✅ 完成 |
| HelpPanel | FullScreen | ✅ 完成 |
| StudyVideoPanel | FullScreen | ✅ 完成 |
| SceneSelectPanel | FullScreen | ✅ 完成 |
| SkillSelectPanel | FullScreen | ✅ 完成 |
| ScoreReportPanel | FullScreen | ✅ 完成 |
| TrainingPlaceholderPanel | FullScreen（占位） | ✅ 完成 |

### 导航系统（UIManager）

- **单一状态源**：`Stack<string> panelHistory`
- `NavigateTo(panelName)` — Push + ShowPanel
- `GoBack()` — Pop + ShowPanel（空栈退出终端）
- `ResetTo(panelName)` — Clear + ShowPanel（登录/退出时）

### 服务接口层

| 接口 | 本地实现 | API 实现 |
|------|---------|---------|
| `ILoginService` | `LocalLoginService` | `ApiLoginService` |
| `IVideoProvider` | `LocalVideoProvider` | `ApiVideoProvider` |
| `IScoreRepository` | `LocalScoreRepository` | `ApiScoreRepository` |
| `IAvatarService` | — | `ApiAvatarService` |

## 已知待办

| # | 问题 | 备注 |
|---|------|------|
| 1 | Play mode 完整测试 | 登录→技能→场景→跳转全流程 |
| 2 | 场景手动保存 | Ctrl+S 保存 Login Scene |
| 3 | `UIManager` 未 `DontDestroyOnLoad` | DB-3 已决，代码未加 |
| 4 | StudyVideo RenderTexture 未释放 | D7 待整改 |
| 5 | 明文密码（D3b） | 后端切换前必改（S级） |
| 6 | Demonstration.unity 不在 Build Settings | 编辑器可运行，打包需添加 |
| 7 | 50 个 Unreal 着色器编译失败 | main 基线固有，枚举命名空间过时 |

## 后端 API 端点（已接入）

| 端点 | 方法 | 说明 |
|------|------|------|
| `/api/v1/auth/login` | POST | 登录，返回 JWT |
| `/api/v1/auth/register` | POST | 注册 |
| `/api/v1/videos/{id}` | GET | 获取视频 URL |
| `/api/v1/scores` | POST | 提交成绩 |
| `/api/v1/scores/user/{id}` | GET | 获取用户成绩列表 |
| `/api/v1/scores/latest` | GET | 获取最新成绩 |
| `/api/v1/avatars` | GET | 获取头像列表 |
| `/api/v1/scenes` | GET | 获取场景列表（未接入 UI） |
| `/api/v1/knowledge` | GET | 获取知识库（未接入 UI） |
| `/api/v1/qa` | POST | 智能问答（需后端配置 AI Key） |
| `/api/v1/pose/detect` | POST | 姿态识别（训练评估） |

## 后续可扩展

- 场景列表动态加载（`/api/v1/scenes` → SceneSelectPanel）
- 知识库接入 HelpPanel
- 智能问答（需后端配置 AI API Key）
- 姿态识别用于训练动作评估
