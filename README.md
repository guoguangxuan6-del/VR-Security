# 🏥 生息守护 · Life Guard

**VR 应急救护一体化培训平台**

生息守护是一款基于 Unity HDRP + VR 的急救技能培训系统，提供沉浸式的 CPR、AED、创伤处置等急救训练场景。

---

## 🎯 项目定位

- **目标用户**：医学院学生、社区急救志愿者、企业安全员
- **运行平台**：Meta Quest 3（VR）+ 桌面端（开发调试）
- **渲染管线**：HDRP（VR 专用配置，MSAA4 / RGBA16F / FFR）
- **核心功能**：急救技能训练、实时评分、成绩管理、视频教学

---

## 🚀 快速开始

### 系统要求

| 组件 | 最低版本 |
|------|---------|
| Unity | 2022.3.62f3 LTS |
| Render Pipeline | HDRP 14.x |
| XR Plugin | Oculus XR Plugin 4.5.4 + OpenXR 1.14.3 |

### 开发分支

| 分支 | 用途 |
|------|------|
| `main` | 团队主线（含 VR 包，在 VR 能力机上构建运行） |
| `feature/no-vr` | 本机工作分支（移除 VR/XR 包，键鼠编写/调试） |

### 项目结构

```
Assets/
├── Scenes/UI/           # UI 场景（Login、StudyVideo、ScoreReport 等）
├── Scripts/
│   ├── UI/              # 11 面板 + UIManager + BasePanel
│   ├── Service/         # 服务接口层（ILoginService / ApiLoginService 等）
│   ├── Input/           # 输入抽象（KeyboardInput / VRInput / InputManager）
│   └── Gameplay/        # 训练场景游戏逻辑
├── Prefabs/UI/          # UI Prefab 模板
├── Fonts/               # SDF 字体资产（Deng SDF）
├── StreamingAssets/     # 视频/资源文件
└── Shaders/             # Unreal 导出 + 自定义 Shader
```

---

## 🔧 UI 系统

### 11 个面板 + 导航

| 面板 | 类型 | 入口 |
|------|------|------|
| HomePanel | FullScreen | 启动入口（未登录） |
| HomeMenuPanel | FullScreen | 大厅（已登录） |
| LoginPanel | Popup | 登录弹窗 |
| RegisterPanel | Popup | 注册弹窗 |
| SettingsPanel | FullScreen | 设置 |
| HelpPanel | FullScreen | 帮助/知识库 |
| StudyVideoPanel | FullScreen | 视频教学 |
| SceneSelectPanel | FullScreen | 场景选择（卡片式） |
| SkillSelectPanel | FullScreen | 技能选择（CPR/AED） |
| ScoreReportPanel | FullScreen | 成绩报告 |
| TrainingPlaceholderPanel | FullScreen | 训练占位 |

### 导航系统

- **单一状态源**：`UIManager` + `Stack<string> panelHistory`
- `NavigateTo(panelName)` — Push + ShowPanel
- `GoBack()` — Pop + ShowPanel
- `ResetTo(panelName)` — Clear + ShowPanel

### 面板切换

- FullScreen：CanvasGroup.alpha 淡入淡出（0.25s）
- Popup：Scale + Fade（打开 0.2s / 关闭 0.15s）
- Z-order 修复：先隐藏所有面板再显示新面板

---

## 📡 后端 API

| 端点 | 说明 |
|------|------|
| `/api/v1/auth/login` | 登录，返回 JWT |
| `/api/v1/auth/register` | 注册 |
| `/api/v1/videos/{id}` | 获取视频 URL |
| `/api/v1/scores` | 提交成绩 |
| `/api/v1/scores/user/{id}` | 获取用户成绩列表 |
| `/api/v1/scores/latest` | 获取最新成绩 |
| `/api/v1/avatars` | 获取头像列表 |
| `/api/v1/scenes` | 获取场景列表 |
| `/api/v1/knowledge` | 获取知识库 |
| `/api/v1/qa` | 智能问答（需配置 AI Key） |
| `/api/v1/pose/detect` | 姿态识别（训练评估） |

### 服务接口

```csharp
// ServiceLocator.Awake() 中切换
new ApiLoginService()  ↔ new LocalLoginService()  // 登录
new ApiVideoProvider() ↔ new LocalVideoProvider() // 视频
new ApiScoreRepository() ↔ new LocalScoreRepository() // 成绩
```

---

## 📝 开发规范

详见 [`CLAUDE.md`](./CLAUDE.md)。

核心原则：
- 业务代码只依赖接口，不直接引用实现类
- 所有数据交互必须经过接口
- 不在 Update 中做昂贵运算
- HDRP 设置不被意外覆盖
- 最少代码解决问题，不做推测性实现

---

## 📋 项目进度

详见 [`overview.md`](./overview.md) 和 [`studio/TODO.md`](./studio/TODO.md)。

---

## 📄 文档索引

| 文件 | 内容 |
|------|------|
| `CLAUDE.md` | 项目开发规范、架构、行为准则 |
| `UI指导.md` | UI 架构、面板清单、设计系统、Mock 服务层 |
| `overview.md` | 项目进度总览（后端接入 + 今日完成） |
| `studio/overview.md` | 工作室概览（多专家会诊结论） |
| `studio/TODO.md` | 待办事项 + 阻塞项 |
| `studio/UI完美解决方案.md` | 完整 UI 多专家解决方案 |
| `studio/VR-UI编写与转换规范.md` | VR UI 编写 → VR 运行转换规范 |

---

## 🤝 团队

| 角色 | 职责 |
|------|------|
| 殷启鸣（主理人） | 编排调度、架构决策、生命周期管控 |
| 高见远（架构师） | 模块边界、解耦、C# 脚本、代码评审 |
| 涂光影（图形） | 渲染管线、材质、光照、Shader |
| 武衡（玩法） | 玩法逻辑、物理、动画、导航 |
| 尤琪（UI） | UGUI/UI Toolkit 界面与交互 |
| 严过关（性能/QA） | 性能红线、Profiler、测试与回归 |
| 包万成（构建） | 场景/Prefab/资源、构建与发布 |

---

## 📜 许可

内部项目，保留所有权利。
