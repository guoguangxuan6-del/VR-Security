# 概览：生息守护 VR 平台 · 完美 UI 解决办法

> 完整多专家会诊报告见 `studio/UI完美解决方案.md`（已 present）。

**三大痛点 → 已查实根因（零虚构）**
- **UI 太丑**：默认 `LiberationSans SDF` 未换思源黑体；设计系统六色未在代码落地，无集中主题资产。
- **排版错误**：`Login Scene` 无 `CanvasScaler`（YAML 0 次）、单 Overlay Canvas、硬编码像素；`UIManager.AutoBind` 用 `GameObject.Find`+`transform.Find` 硬编码名 → 改名静默失效。
- **VR 单调/眩晕**：`CPRTraining` Canvas=0、XR Rig=0、相机 `Untagged`→`Camera.main==null`；🔴 更致命：**全工程无 XR Loader 配置 → 头显不初始化**（P0 硬阻塞）。

**解决办法核心**
- 桌面：Canvas Scaler(1920×1080, Match 0.5) + 分层 Canvas(Base/HUD/Popup) + 锚点/Layout Group/Content Size Fitter + 删硬编码 Find（改 `[SerializeField]`+`OnValidate`） + `DontDestroyOnLoad` 保活 11 面板。
- VR：全 **WorldSpace Canvas**（禁 Overlay）+ `XRUIInputModule` + `TrackedDeviceGraphicRaycaster` + 运行期 `RegisterCanvas`；**XR 相机 Tag 设 `MainCamera`**；UI 与游戏共用 `InputManager`/`TrainingInput` 语义链。
- 渲染：克隆 `HDRP_VR_Quest`（MSAA4 + 关 RT/SSR/SSGI/SSS/体积 + 动态分辨率 + FFR L2-3 + Vulkan）；后处理只留 Tonemapping/固定曝光/弱 Bloom/校色，**斩 MotionBlur/DoF/色差**；色彩缓冲**保持 RGBA16F**（勿换 R11G11B10，Adreno 不支持 MSAA resolve）；VR 世界 UI 用 **HDRP Unlit/Emissive** 材质 + 独立 Layer 避后处理污染。
- 契约：UI↔Gameplay 四支柱已双向锁定（含 B.5 设备→语义→UI 全链路），可并行进入实现期。

**已完成（2026-07-23）**
- ✅ 11 面板导航系统（NavigateTo/GoBack/ResetTo），UIManager 单一状态源
- ✅ HomePanel（未登录入口）+ HomeMenuPanel（已登录大厅）+ UserInfoBar
- ✅ InputField 字体修复（Deng SDF）+ 光标隐藏（alpha=0 + CustomCaretColor 启用）
- ✅ 技能选择 → 场景选择 → 地铁站场景跳转链路
- ✅ 淡入淡出动效（CanvasGroup.alpha）+ Z-order 遮挡修复
- ✅ Editor 破坏性工具清理（Migrator/Builder/NPCSimulator 全删）
- ✅ LoginPanel/RegisterPanel `setOnClickListener` → `onClick.AddListener`
- ✅ 后端 API 服务层 async/await 接入（ApiClient / ApiLogin / ApiVideo / ApiScore / ApiAvatar）
- ✅ TerminalCanvas `planeDistance=100` → `0`（修复密码框点击无响应）
- ✅ TMP Settings 默认字体 GUID 修正（原 GUID 指向不存在的资产）
- ✅ VRVirtualKeyboardHelper 条件防护（`#if !ENABLE_VR`）

**待办 TODO**
- [ ] Play mode 完整测试（登录 → 技能训练 → 场景选择 → 地铁站跳转）
- [ ] 场景手动保存（Ctrl+S）
- [ ] Demonstration.unity 加入 Build Settings（如需打包）
- [ ] UIManager DontDestroyOnLoad（DB-3 已决，代码未加）
- [ ] StudyVideo RenderTexture 释放（D7 待整改）
- [ ] 明文密码（D3b）后端切换前必改（S级）
- [ ] 50 个 Unreal 着色器枚举命名空间修复（main 基线固有）
- [ ] GameStateMachine prefab 缺失脚本恢复（main 基线固有）
