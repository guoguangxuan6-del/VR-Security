# CPR DB 后端 API 文档

**服务地址**: `http://123.57.30.132:8080`  
**统一响应格式**: `{code, message, data}`  
**最新更新**: 2026-07-23

---

## 鉴权说明

| 符号 | 含义 |
|------|------|
| 🔓 | 公开接口，无需 token |
| 🔐 | 需在 Header 带 `Authorization: Bearer <token>` |

Token 通过登录接口获取，有效期 **24 小时**（86400000ms）。

**种子测试账号**: `testuser` / `Test@123456`

---

## 一、认证模块 — `/api/v1/auth`

### 1.1 注册 🔓

```
POST /api/v1/auth/register
Content-Type: application/json
```

**请求体**:
```json
{ "username": "string (必填)", "password": "string (必填)" }
```

**成功** (200):
```json
{
  "code": 200, "message": "success",
  "data": { "token": "eyJ...", "expiresAt": 1784836055118 }
}
```

**失败示例**:
```json
{"code":400, "message":"username already exists"}
{"code":400, "message":"username is required"}
```

### 1.2 登录 🔓

```
POST /api/v1/auth/login
Content-Type: application/json
```

请求体同注册。`expiresAt` 为 Unix 毫秒时间戳。

---

## 二、训练场景 — `/api/v1/scenes`

### 2.1 场景列表 🔓

```
GET /api/v1/scenes
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": [
    { "id": 1, "name": "成人 CPR 训练", "description": "标准成人胸外按压与人工呼吸训练场景",
      "type": "basic", "icon": "heart", "sortOrder": 1, "createdAt": "..." },
    { "id": 2, "name": "儿童 CPR 训练", "type": "basic", "icon": "child" },
    { "id": 3, "name": "AED 使用", "type": "basic", "icon": "zap" },
    { "id": 4, "name": "气道异物梗阻", "type": "advanced", "icon": "alert" },
    { "id": 5, "name": "综合考核", "type": "advanced", "icon": "star" }
  ]
}
```

`type`: `basic` | `advanced`  
`icon`: 前端图标标识名，按需映射。

---

## 三、知识库 — `/api/v1/knowledge`

### 3.1 知识列表 🔓

```
GET /api/v1/knowledge
GET /api/v1/knowledge?category=AED
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": [
    {
      "id": 1,
      "question": "心肺复苏（CPR）是什么？",
      "answer": "心肺复苏（CPR）是一种紧急急救技术，通过胸外按压和人工呼吸...",
      "category": "基础",
      "tags": "CPR,定义",
      "createdAt": "2026-07-23T07:22:27"
    }
  ]
}
```

**分类分布**（共 27 条）:

| 分类 | 条数 |
|------|:--:|
| 基础 | 8 |
| AED | 5 |
| 儿童CPR | 3 |
| 急救 | 6 |
| 常见问题 | 5 |

可选参数 `?category=` 按分类筛选。

---

## 四、智能问答 — `/api/v1/qa`

### 4.1 预设问题 🔓

```
GET /api/v1/qa/presets
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": {
    "presets": [
      "心肺复苏的正确步骤是什么？",
      "AED 如何使用？",
      "按压深度和频率应该是多少？",
      "如何判断患者是否需要心肺复苏？"
    ]
  }
}
```

### 4.2 提问 🔐

```
POST /api/v1/qa
Content-Type: application/json
Authorization: Bearer <token>
```

**请求体**:
```json
{
  "question": "string (必填)",
  "history": [
    { "role": "user", "content": "上一轮问题" },
    { "role": "assistant", "content": "上一轮回答" }
  ]
}
```

**响应** (200):
```json
{ "code": 200, "message": "success", "data": { "answer": "AI 回答内容..." } }
```

> ⚠️ QA API Key 未配置，返回 fallback `"AI 服务返回异常，请稍后重试。"`。

---

## 五、视频 — `/api/v1/videos`

### 5.1 获取视频 🔓

```
GET /api/v1/videos/{videoId}
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": { "videoId": "video1", "url": "https://...", "durationSeconds": 120 }
}
```

种子数据: `video1`(120s)、`video2`(180s)。

---

## 六、成绩 — `/api/v1/scores`

### 6.1 提交成绩 🔐

```
POST /api/v1/scores
Content-Type: application/json
```

**请求体**:
```json
{
  "scene": "string (必填)",
  "skill": "string (必填)",
  "totalScore": 85.5 (必填),
  "compressionDepthAvg": 5.2,
  "compressionRateAvg": 110.0,
  "errorCount": 2,
  "stepDetails": "{\"steps\":[...]}"
}
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": {
    "id": 1, "username": "testuser", "scene": "成人CPR训练",
    "skill": "胸外按压", "totalScore": 85.5,
    "compressionDepthAvg": 5.2, "compressionRateAvg": 110.0,
    "errorCount": 2, "stepDetails": "...", "createdAt": "..."
  }
}
```

### 6.2 成绩列表 🔐

```
GET /api/v1/scores
GET /api/v1/scores?username=testuser
```

不带 `username` 返回当前用户成绩；带参数只能查自己，查他人返回 403。

### 6.3 最新成绩 🔐

```
GET /api/v1/scores/latest
GET /api/v1/scores/latest?username=testuser
```

权限规则同 6.2。返回单条 `ScoreDto` 或 `null`。

### 6.4 成绩统计 🔐

```
GET /api/v1/scores/stats
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": {
    "totalAttempts": 3,
    "averageScore": 85.2,
    "highestScore": 92.5,
    "lowestScore": 78.0,
    "scenesTrained": 3,
    "skillsTrained": 2,
    "recentScores": [
      { "id": 3, "scene": "儿童CPR训练", "totalScore": 78.0, ... },
      { "id": 2, "scene": "AED使用", "totalScore": 92.5, ... },
      { "id": 1, "scene": "成人CPR训练", "totalScore": 85.0, ... }
    ]
  }
}
```

`recentScores` 最多返回最近 5 条，按时间倒序。无数据时返回 `[]`。

---

## 七、学员 — `/api/v1/students`

### 7.1 学员列表 🔐

```
GET /api/v1/students
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": [
    {
      "id": 1, "name": "张三", "phone": "13800138000",
      "email": "zhangsan@example.com", "groupName": "2026夏季班",
      "certStatus": "certified", "trainedAt": "2026-07-20T10:00:00",
      "createdAt": "2026-07-20T10:00:00"
    }
  ]
}
```

`certStatus`: `certified` | `training` | `expired`  
⚠️ 无种子数据，返回 `[]`。

---

## 八、用户速查 — `/api/v1/user`

### 8.1 基本信息 🔐

```
GET /api/v1/user/info
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": { "id": 1, "username": "testuser", "createdAt": "..." }
}
```

> 如需完整个人信息（姓名、手机、班级、头像等），请使用下方的 `/api/v1/profile` 接口。

---

## 九、个人信息 — `/api/v1/profile`  ⭐新增

### 9.1 获取个人信息 🔐

```
GET /api/v1/profile
```

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": {
    "id": 1,
    "username": "testuser",
    "realName": "张三",
    "role": "student",
    "avatar": "/uploads/avatars/1_xxx.jpg",
    "gender": 1,
    "phone": "13800138000",
    "studentId": "2024001",
    "className": "护理2班",
    "createdAt": "2026-07-22T23:37:10"
  }
}
```

`gender`: 0=未知, 1=男, 2=女  
`role`: `"student"` | `"admin"`（默认 `"student"`）

### 9.2 更新个人信息 🔐

```
PUT /api/v1/profile
Content-Type: application/json
```

**请求体**（只传需要更新的字段）:
```json
{
  "realName": "张三",
  "gender": 1,
  "phone": "13800138000",
  "studentId": "2024001",
  "className": "护理2班"
}
```

**手机号格式**: `1[3-9]xxxxxxxxx`（11位中国大陆手机号）

**响应** (200):
```json
{ "code": 200, "message": "更新成功", "data": { /* 完整 profile */ } }
```

**错误**:
- `400`: 参数校验失败（如手机号格式不对）
- `409`: 手机号或学号已被其他用户使用

### 9.3 上传头像 🔐

```
POST /api/v1/profile/avatar
Content-Type: multipart/form-data
```

**表单参数**:

| 参数 | 类型 | 说明 |
|------|------|------|
| `file` | File | 图片文件（jpg / png / webp，≤ 2MB） |

**响应** (200):
```json
{
  "code": 200, "message": "上传成功",
  "data": { "avatar_url": "/uploads/avatars/1_1753234567.jpg" }
}
```

完整头像 URL = `http://123.57.30.132:8080` + `avatar_url`。

**错误**:
- `400`: 文件格式不支持或超过大小限制

---

## 十、姿态识别 — `/api/v1/pose`

### 10.1 姿态检测 🔐

```
POST /api/v1/pose/detect
Content-Type: multipart/form-data
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `image` | File | 图片文件 |

**响应** (200):
```json
{
  "code": 200, "message": "success",
  "data": { "angles": [/* AngleAnalysis[] */], "landmarks": [/* PoseLandmark[] */] }
}
```

---

## 附录

### 通用错误码

| code | 含义 |
|------|------|
| 200 | 成功 |
| 400 | 请求参数错误 |
| 401 | 未认证 / token 无效或过期 |
| 403 | 无权限访问 |
| 404 | 资源不存在 |
| 405 | HTTP 方法不允许 |
| 409 | 数据冲突（如手机号/学号已存在） |
| 415 | Content-Type 不支持（请用 application/json） |
| 500 | 服务器内部错误 |

### 统一响应结构

```typescript
interface ApiResponse<T> {
  code: number;
  message: string;
  data: T | null;
}
```

### 前端调用示例

```javascript
const BASE = 'http://123.57.30.132:8080';

// 登录
const { data: { token } } = await fetch(`${BASE}/api/v1/auth/login`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username: 'testuser', password: 'Test@123456' })
}).then(r => r.json());

// 获取个人资料
const { data: profile } = await fetch(`${BASE}/api/v1/profile`, {
  headers: { 'Authorization': `Bearer ${token}` }
}).then(r => r.json());

// 更新个人资料
await fetch(`${BASE}/api/v1/profile`, {
  method: 'PUT',
  headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
  body: JSON.stringify({ realName: '李四', phone: '13900001111' })
});

// 上传头像
const formData = new FormData();
formData.append('file', fileInput.files[0]);
const { data: { avatar_url } } = await fetch(`${BASE}/api/v1/profile/avatar`, {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
}).then(r => r.json());
```

### 完整接口速查表

| 模块 | 方法 | 路径 | 鉴权 |
|------|------|------|:--:|
| 认证 | `POST` | `/api/v1/auth/register` | 🔓 |
| 认证 | `POST` | `/api/v1/auth/login` | 🔓 |
| 场景 | `GET` | `/api/v1/scenes` | 🔓 |
| 知识库 | `GET` | `/api/v1/knowledge` | 🔓 |
| 问答 | `GET` | `/api/v1/qa/presets` | 🔓 |
| 问答 | `POST` | `/api/v1/qa` | 🔐 |
| 视频 | `GET` | `/api/v1/videos/{videoId}` | 🔓 |
| 成绩 | `POST` | `/api/v1/scores` | 🔐 |
| 成绩 | `GET` | `/api/v1/scores` | 🔐 |
| 成绩 | `GET` | `/api/v1/scores/latest` | 🔐 |
| 成绩 | `GET` | `/api/v1/scores/stats` | 🔐 |
| 学员 | `GET` | `/api/v1/students` | 🔐 |
| 用户 | `GET` | `/api/v1/user/info` | 🔐 |
| 个人信息 | `GET` | `/api/v1/profile` | 🔐 |
| 个人信息 | `PUT` | `/api/v1/profile` | 🔐 |
| 个人信息 | `POST` | `/api/v1/profile/avatar` | 🔐 |
| 姿态 | `POST` | `/api/v1/pose/detect` | 🔐 |
