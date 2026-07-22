# 明亮温馨医院走廊 · 灯光与色彩/材质情绪规范（HDRP / VR 目标）

> 适用范围：Unity HDRP 渲染管线；最终运行目标为 VR 设备。
> 读者：3D 建模师 / 场景美术。
> 性质：方向指引文档（美术规范），不含代码。
> 总目标：**明亮但不刺眼，温馨有安全感，规避传统冷白荧光灯管的"医院冷漠感"。**

---

## 0. 设计总纲（一句话给美术）

走「暖白日光 + 天然木色 + 医疗引导色」路线，画面通透、柔和、有呼吸感。
所有参数都要为 **VR 的视觉舒适度与性能**让路（见 §2.5、§4.5）。

- 室内统一偏**暖白（3500–4000K）**，不要冷白荧光。
- 主光来自**隐藏式灯带 + 筒灯**的间接光，而非裸灯管。
- **窗 + 柔化自然光**是温馨感的核心来源。
- 灯具自己要**自发光（Emissive）**，光源本体亮，而不是只靠 Light 组件。
- 材质**哑光优先**，金属/玻璃必须有反射来源（Reflection Probe）。

---

## 1. 整体色彩情绪板

### 1.1 主色（墙面 / 地面）

| 区域 | 颜色倾向 | Hex 近似 | 色彩心理意图 |
|---|---|---|---|
| 墙面暖涂料 | 米杏 / 奶油白，非纯白，略带暖调 | `#F2E9DC`（米杏）/ `#EDE4D3`（奶油白） | 柔和洁净而不冰冷，像家而非诊室 |
| 地面地胶（PVC/橡胶） | 中明度暖浅灰，带细微纹理 | `#C9C2B6`（暖浅灰）/ `#B7B0A4` | 中性承接，耐脏、安静，不抢视觉 |

- 墙面**不要纯白（#FFFFFF）**，纯白在 HDRP 下易被曝光推成死亮，且显冷硬。
- 地面明度低于墙面，形成"上亮下稳"的稳定感。

### 1.2 辅色（木饰 / 软装）

| 区域 | 颜色倾向 | Hex 近似 | 色彩心理意图 |
|---|---|---|---|
| 木饰面（墙裙/导视/座椅） | 浅橡 / 白蜡木暖调 | `#D8C3A5`（浅橡）/ `#C9AE84`（暖木） | 天然材料带来"被照料"的安全感，中和医疗感 |
| 软装（座椅布艺/帘） | 低饱和莫兰迪（暖灰绿/米褐） | `#A9B5A0`（灰绿）/ `#C0B2A1`（米褐） | 放松、温暖，避免高饱和刺眼 |

### 1.3 点缀色（医疗引导色）

| 颜色 | Hex 近似 | 用途 | 心理意图 |
|---|---|---|---|
| 医疗青绿 | `#4FB6A0`（清新青绿）/ `#2E8B79`（深医疗绿） | 导视牌、指示、与绿植呼应 | 专业可信 + 健康/生命联想，做"该往哪走"的引导 |
| 暖橙 | `#E8924A` / `#F0A55A` | 呼叫/状态/重点提示 | 温暖、活跃、提示性，少量制造焦点 |

> **点缀色面积控制在 5–10%**，只做"锚点"，避免变成主题乐园配色。

### 1.4 推荐色温区间

| 光源类别 | 色温 | 说明 |
|---|---|---|
| 环境 / 天光 | 3500–4000K | 暖白，不要 6500K 冷白 |
| 重点照明（导视/座椅） | 3000–3500K | 更暖，做焦点 |
| 自然采光（窗） | 5400–6500K | 日光白，但经窗纱柔化，与室内暖白形成"冷暖对话"而非冲突 |

**关键原则**：室内统一偏暖，窗光偏冷；用 Emissive 灯带与重点暖光做"暖色锚点"，平衡窗的冷光。

---

## 2. 灯光方案（HDRP）

### 2.1 环境光（Ambient / HDRI / 天光）

- 用 HDRP 的 **Visual Environment + Physically Based Sky** 或 **HDRI Sky** 作为环境贴图来源；Ambient 由 Sky/HDRI 提供，**不要纯灰 Ambient**（纯灰会让画面发脏发冷）。
- 天空对间接光（Indirect Diffuse / Reflection）的贡献保持**中低**，避免发灰、过曝。
- 若用 HDRI，选"室内 / 暖白室内"类，**避免冷蓝天**。
- **曝光（Exposure）**：HDRP 走基于物理的曝光。走廊建议**固定曝光（Fixed）**，允许少量 Exposure Compensation（约 -0.3~0，给亮部留余量）。**VR 不宜用自动曝光跳变**（双目亮度不一致会眩晕）。

### 2.2 天花板主照明

**原则**：暖白但足够亮、均匀、无可见灯管。用"隐藏式灯带 + 筒灯"做面光源感，而非裸荧光灯。

- **间接光为主**：天花板凹槽灯带（Emissive + 少量 Area/Rect Light 或 Light Probe 间接），给墙面/天花板柔和洗光。
- **筒灯（Spot 小角度）**：沿走廊均匀布置，做主照度，色温 3500–4000K。
- **比例建议**：间接灯带 ≈ 50% + 筒灯 ≈ 40% + 重点/洗墙 ≈ 10%。

**HDRP Light 近似参数方向**（给美术的是"相对强弱方向"，最终以视觉亮度 + 曝光为准，不要死磕绝对数值；HDRP 下不同 Light 类型单位不同：Directional 用 lux，Point/Spot 用 candela，Area 用 nits/luminance）：

| 光源类型 | Light Type | Intensity 方向 | Color Temperature | Indirect Multiplier | 说明 |
|---|---|---|---|---|---|
| 天花板灯带（间接） | Area(Rect) / Emissive | 低–中 | 3800K | 1.0–1.5 | 柔和洗顶，靠 Indirect 提亮 |
| 筒灯 | Spot（窄角） | 中 | 3600K | 1.0 | 均匀主照度，避免过曝 |
| 环境补光 | Ambient（来自 Sky） | — | 4000K | — | 整体底光 |
| 窗自然光 | Directional（经窗洞） | 中–高（受窗面积限制） | 6000K | 1.5–2.0 | 软阴影，温馨主来源 |

### 2.3 自然采光（窗）

- 用 **Directional Light** 模拟阳光，经窗洞投入走廊；窗用真实玻璃（半透）+ 窗纱/磨砂，**柔化硬阴影**。
- 阴影：Shadow Map 中分辨率 + **软阴影（PCF）+ 接触阴影（Contact Shadows 开）**，避免硬黑影制造不安。
- 让阳光在地面/墙面投出**暖色光斑**，作为"自然、安全"的核心情绪锚。
- **VR**：Directional 阴影开销可控，优先保证。

### 2.4 关键区域重点照明 / 洗墙光

- **导视牌**：小角度 Spot 或正面 Area 补光，确保可读；色温偏暖 3200–3500K，让绿色导视更醒目。
- **绿植区**：上方 Point/Spot 洗光，让植物通透，暗示"生命/疗愈"。
- **座椅区**：暖橙低强度重点光（Emissive 指示灯 + 局部洗光），制造"可停留"的安心角落。
- **洗墙光**：墙面底部/顶部线形 Emissive 灯带，让墙面有层次、不空。

### 2.5 VR 灯光注意事项（重要）

- **性能**：光源数量克制。优先 Directional + 少量 Spot；大量 Point 灯在 VR 双目下开销翻倍。
- **避免过亮**：VR 近眼，过亮光源（尤其直视灯）易不适。灯本体用 Emissive 但亮度限制合理，配合 Bloom 柔化。
- **阴影**：Contact Shadows 开但分辨率适中，避免双目不一致。
- 建议渲染路径：**Single-Pass Instanced**，抗锯齿用 **MSAA（4x）**。

---

## 3. 材质表现方向（HDRP）

HDRP 材质以 **Lit** 为核心，通过 **Material Type**（Subsurface Scattering / Standard / Anisotropy / Iridescence / Translucent / Specular Color）切换高级层；另有 Fabric、Hair、Eye、Unlit 等专用 Shader。关键是 **Mask Map、粗糙度/金属度与高级层（Clear Coat、Subsurface/Transmission）**。

### 3.1 墙面暖涂料

- Shader：**Lit**，Material Type = Standard（无金属）。
- 参数：Metallic 0；Smoothness **0.2–0.4**（哑光微光泽）；Albedo 取米杏 Hex。
- Clear Coat：**关**。Subsurface：**关**（墙面用普通漫反射）。
- 细节：用 **Detail Map** 加细微噪点，避免大面死板。

### 3.2 地面地胶

- Lit，Standard；Metallic 0；Smoothness **0.35–0.55**（略可擦洗的光泽，非镜面）；带法线/细微划痕。
- 反射：靠 **Reflection Probe / Sky** 提供；VR 性能起见 **SSR（屏幕空间反射）关或低**，用反射探针代替。
- **避免高 Smoothness** 造成眩光斑点。

### 3.3 木饰面

- Lit，Standard；Metallic 0；Smoothness **0.4–0.6**；Albedo 浅橡；**可加 Clear Coat 0.2–0.4**（清漆木感）让木纹通透。
- 法线/粗糙度贴图体现木纹走向。

### 3.4 玻璃（窗 / 隔断）

- Shader：**Lit，Surface Type = Transparent**，启用 **Refraction（IOR ≈ 1.5）** 与 Transmission；或用 Material Type = Translucent。
- 参数：Metallic 0；Smoothness **0.9+**（清玻）/ **0.6–0.7 + 法线扰动**（磨砂窗，柔化视界）。
- 反射来源：**Reflection Probe / Sky**；窗外景用背景 HDRI 或实际窗外几何体。
- 半透磨砂窗降低 Smoothness 并加法线扰动，柔化走廊视界。

### 3.5 金属扶手 / 医疗设施金属件

- Lit，Standard；Metallic **1.0**（不锈钢 0.9）；Smoothness **0.6–0.85**；Albedo 近灰/不锈钢色。
- 反射：必须靠 **Reflection Probe / SSR** 才有质感，否则发死黑。
- VR 中金属高光别过曝，配合曝光控制。

### 3.6 布料 / 软装 / 植物

- 布艺：用 **Fabric** Shader 或 Lit + 低 Smoothness（0.3–0.5）。
- 植物叶片：**Lit，Material Type = Subsurface Scattering 或 Translucent**，开启 **Transmission**，让叶脉透光、更"活"。
- 软装低饱和，避免高光塑料感。

### 3.7 灯具轻微自发光（Emissive）

- 灯罩/灯带本体用 **Emissive**（而非只放 Light），让光源"自己亮"。
- 参数：Emissive Color 暖白（与灯色温一致，约 `#FFE6C8`）；HDRP 用 HDR 颜色 + **Emissive Intensity** 控制亮度，**避免爆白**。
- 配合 Bloom 让灯带边缘柔光晕开，是"温馨"的关键。
- 做法：**Emissive Mesh + 轻量 Light**（或直接 Emissive + Light Probe 间接），减少实时 Light 数量 —— **VR 友好**。

---

## 4. 后处理 Volume 建议（HDRP）

HDRP 用 **Volume 框架**（Volume Profile + Volume 组件）。走廊放一个 **Global Volume**，关键区域可叠加 **Local Volume**。

### 4.1 Tone Mapping（色调映射）

- 模式：**ACES**（电影感、高光不过曝）但整体**偏亮**——通过 Exposure / Color Adjustments 提亮中间调。
- 或 HDRP 的 **Neutral**（更保真）。要"明亮温馨"建议 **ACES + 提亮**。
- 目标：高光不死白，暗部不死黑，整体通透。

### 4.2 Bloom（泛光）

- 开启，**轻微**：Threshold 高（≈0.8–1.0）、Intensity 低（0.3–0.6）、Scatter 小。
- 作用：灯带/窗光的柔光晕，制造"光感温馨"。
- **VR**：可开但强度低，避免双目光晕不一致/眩晕。

### 4.3 Color Grading（色彩分级）偏暖

- **White Balance**：Temperature 偏暖（+10~+20），Tint 略偏暖/品红。
- **Color Adjustments**：Post Exposure +0.2~+0.4（提亮），Contrast 略降（更柔和），Saturation 略升（木色/绿更鲜活但不过饱和）。
- **Lift/Gamma/Gain 或 Shadows/Midtones/Highlights**：中间调加一点暖橙；阴影**避免纯蓝**（去冷）。
- **Split Toning**：阴影带一点暖，高光带一点青绿，呼应医疗色。

### 4.4 其他建议

- **Vignette**：轻微（强度 0.2–0.3），聚焦走廊纵深；VR 中别太重（周边是真实视野）。
- **Fog / Volumetric Fog**：极轻体积雾增加空气感与纵深；VR 中控制密度（性能 + 避免糊）。
- **Ambient Occlusion**：开（屏幕空间 / HZB AO），强化转角/家具接触处，增加"踏实感"。

### 4.5 VR 后处理注意（重要）

**关闭 / 避免**（会引发 VR 眩晕或双目问题）：

- ❌ Depth of Field（辐辏-调节冲突 → 眩晕）
- ❌ Motion Blur（VR 禁用）
- ❌ Chromatic Aberration（边缘色散在 VR 中易不适）
- ❌ Lens Distortion（禁用）

**建议保留**：

- ✅ Tonemapping、轻度 Bloom、Color Grading、White Balance、Ambient Occlusion、轻微 Vignette、极轻 Fog。
- ✅ 抗锯齿：**MSAA（4x）**，不用 TAA（双目 + TAA 易鬼影）。
- ✅ 后处理栈要轻，单 pass 优先。

---

## 5. 给建模师的贴图 / 通道备注

### 5.1 需要自发光贴图（Emissive Map）的道具 / 区域

- **所有灯具**（灯罩、灯带、筒灯面）：Emissive 贴图，控制发亮形状/区域（灯带用条状 emissive，避免整片爆亮）。
- **医疗指示灯**（呼叫 / 疏散 / 设备状态）：小面积强 emissive，颜色用引导色（绿 / 橙）。
- **导视牌背光**：导视文字/图标用 emissive，确保可读且自带光感。
- ⚠️ Emissive 贴图要匹配发光几何，避免发光溢出到不该亮的面。

### 5.2 AO / 接触阴影

- 烘焙 **AO 贴图（Ambient Occlusion Map）** 到墙面转角、家具底部、天花板凹槽——HDRP 的 AO 通道读取它，增加体积感与"踏实"。
- **接触阴影（Contact Shadows）** 在引擎层开；建模阶段可在贴图里预烘焙小接触暗部，双保险。
- 家具 / 绿植 / 座椅与地面接触处**务必有 AO**，否则"飘"。

### 5.3 玻璃 / 金属的环境反射来源

- 布景时确保场景有 **Reflection Probe**（走廊两端 + 中段至少各一个，或按区域布置），金属/玻璃才能反射环境而非死黑。
- **窗外景**：提供窗外 HDRI 或实际窗外几何体，玻璃才有内容可透/可反。
- 玻璃/金属的法线/粗糙度贴图要准，否则反射失真。
- 不锈钢扶手：静态布景用 **Baked Probe** 省性能（VR 友好）。

### 5.4 通用贴图规范

- 所有材质提供完整 PBR 通道：**Albedo / Normal / Mask Map（R=Metallic, G=AO, B=DetailMask, A=Smoothness）/ Height（可选）/ Emissive（灯具类）**。
- 分辨率：墙面/地面 1k–2k（按面积），道具 512–1k，**避免过度（VR 带宽）**。
- 色彩空间：**Albedo / Emissive 用 sRGB**；**Mask / Normal / Height 用 Linear**。
- **避免纯黑 Albedo**（吸光死黑），用深灰 + 环境光。

### 5.5 UV 与接缝

- 大面积墙面/地面用合理 UV 平铺，或考虑 World/Trim UV 思路，避免拉伸。
- 灯带 / 导视等需要清晰 emissive 形状的区域，UV 要与发光几何对齐。

---

## 附：一句话校验清单（美术自检）

- [ ] 室内无冷白荧光灯管，色温统一 **3500–4000K 暖白**
- [ ] 主光来自**隐藏灯带 + 筒灯**，间接光为主
- [ ] 有窗，有**柔化自然光斑**
- [ ] 灯具自身 **Emissive** 发亮
- [ ] 材质**哑光优先**，金属/玻璃有**反射探针**
- [ ] 后处理：**ACES 偏亮 + 轻 Bloom + 暖白平衡 + AO**，**无 DoF / 动态模糊**
- [ ] VR：**光源数克制、MSAA、无眩晕后处理**
