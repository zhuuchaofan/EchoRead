# LexiFlow 项目规范 (Project Specification)

> **Version**: 1.0
> **Last Updated**: 2026-01-29

---

## 1. 技术栈 (Tech Stack)

| 层级          | 技术                  |  版本   | 备注             |
| :------------ | :-------------------- | :-----: | :--------------- |
| **Runtime**   | .NET                  | 9.0 LTS | 后端运行时       |
| **Framework** | ASP.NET Core          |   9.0   | Minimal API      |
| **ORM**       | Entity Framework Core |   9.0   | SQLite Provider  |
| **Database**  | SQLite                |   3.x   | WAL 模式         |
| **Queue**     | DotNext.Threading     |   5.x   | 持久化通道       |
| **Browser**   | Playwright            |   1.x   | 无头浏览器       |
| **AI**        | Google.GenAI          |   1.x   | Gemini 3.0 Flash |
| **Logging**   | Serilog               |   9.x   | 结构化日志       |
| **Frontend**  | Next.js               |  15.x   | App Router       |
| **Styling**   | Tailwind CSS          |   4.x   | Utility-first    |
| **PWA**       | Serwist               |   9.x   | Service Worker   |
| **Container** | Docker                |  24.x   | 多阶段构建       |

---

## 2. 项目结构 (Project Structure)

```
/Volumes/fanxiang/LexiFlow/
├── src/
│   ├── backend/
│   │   ├── LexiFlow.sln
│   │   └── LexiFlow.Api/
│   │       ├── Domain/              # 领域模型
│   │       │   ├── Ingestion/
│   │       │   ├── Deconstruction/
│   │       │   └── Archival/
│   │       ├── Infrastructure/      # 基础设施
│   │       │   ├── Data/
│   │       │   ├── Channels/
│   │       │   ├── Browser/
│   │       │   ├── Content/
│   │       │   └── AI/
│   │       ├── Services/            # 应用服务
│   │       ├── Endpoints/           # API 端点
│   │       └── Program.cs
│   └── frontend/
│       └── lexiflow-web/
│           ├── src/
│           │   ├── app/
│           │   ├── components/
│           │   └── lib/
│           └── public/
├── docs/                            # 项目文档
├── scripts/                         # 部署脚本
├── data/                            # 数据目录 (gitignore)
├── logs/                            # 日志目录 (gitignore)
├── secrets/                         # 密钥目录 (gitignore)
├── docker-compose.yml
├── .gitignore
├── .editorconfig
└── README.md
```

---

## 3. 命名规范 (Naming Conventions)

### 3.1 C# 后端

| 类型     | 规范        | 示例                                 |
| :------- | :---------- | :----------------------------------- |
| 类/接口  | PascalCase  | `IngestionService`, `IJobChannel`    |
| 方法     | PascalCase  | `SubmitAsync()`, `ProcessJobAsync()` |
| 属性     | PascalCase  | `Status`, `CreatedAt`                |
| 私有字段 | \_camelCase | `_logger`, `_channel`                |
| 参数     | camelCase   | `sourceUrl`, `cancellationToken`     |
| 常量     | UPPER_SNAKE | `MAX_RETRY_COUNT`                    |

### 3.2 TypeScript 前端

| 类型      | 规范        | 示例                                |
| :-------- | :---------- | :---------------------------------- |
| 组件      | PascalCase  | `SubmitForm.tsx`, `StatusBadge.tsx` |
| 函数      | camelCase   | `useSubmission()`, `submitUrl()`    |
| 变量      | camelCase   | `isLoading`, `submission`           |
| 类型/接口 | PascalCase  | `Submission`, `AnalysisResult`      |
| 常量      | UPPER_SNAKE | `API_BASE_URL`                      |

### 3.3 文件命名

| 类型       | 规范           | 示例                  |
| :--------- | :------------- | :-------------------- |
| C# 类文件  | PascalCase.cs  | `IngestionService.cs` |
| React 组件 | PascalCase.tsx | `SubmitForm.tsx`      |
| Hooks      | camelCase.ts   | `useSubmission.ts`    |
| 工具函数   | camelCase.ts   | `api.ts`              |
| 配置文件   | kebab-case     | `docker-compose.yml`  |

---

## 4. API 规范 (API Specification)

### 4.1 RESTful 端点

| 方法   | 路径                       | 描述         | 响应码  |
| :----- | :------------------------- | :----------- | :-----: |
| `POST` | `/api/v1/submissions`      | 提交 URL     |   202   |
| `GET`  | `/api/v1/submissions/{id}` | 查询状态     | 200/404 |
| `GET`  | `/api/v1/submissions`      | 列表查询     |   200   |
| `GET`  | `/api/v1/entries/{id}`     | 获取解构结果 | 200/404 |
| `GET`  | `/health`                  | 健康检查     |   200   |

### 4.2 请求/响应格式

```json
// POST /api/v1/submissions
// Request
{ "url": "https://example.com/article" }

// Response (202 Accepted)
{
  "id": "uuid",
  "status": "Queued",
  "createdAt": "2026-01-29T10:00:00Z"
}
```

### 4.3 错误响应

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid URL format",
  "traceId": "00-abc123..."
}
```

---

## 5. 数据库规范 (Database Specification)

### 5.1 SQLite Pragma 配置

```sql
PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA foreign_keys = ON;
PRAGMA busy_timeout = 5000;
```

### 5.2 表结构

```sql
-- Submissions 表
CREATE TABLE Submissions (
    Id TEXT PRIMARY KEY,
    SourceUrl TEXT NOT NULL,
    Status INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    ProcessedAt TEXT NULL
);

-- DeconstructionJobs 表
CREATE TABLE DeconstructionJobs (
    Id TEXT PRIMARY KEY,
    SubmissionId TEXT NOT NULL,
    Status INTEGER NOT NULL,
    RawContent TEXT NULL,
    CleanedMarkdown TEXT NULL,
    AnalysisJson TEXT NULL,
    RetryCount INTEGER DEFAULT 0,
    LastError TEXT NULL,
    FOREIGN KEY (SubmissionId) REFERENCES Submissions(Id)
);

-- KnowledgeEntries 表
CREATE TABLE KnowledgeEntries (
    Id TEXT PRIMARY KEY,
    JobId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Summary TEXT NOT NULL,
    DeepLogic TEXT NULL,
    Tags TEXT NULL,  -- JSON Array
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (JobId) REFERENCES DeconstructionJobs(Id)
);
```

---

## 6. 日志规范 (Logging Specification)

### 6.1 日志级别

| 级别          | 用途           |
| :------------ | :------------- |
| `Debug`       | 开发调试信息   |
| `Information` | 正常业务流程   |
| `Warning`     | 可恢复的异常   |
| `Error`       | 需要关注的错误 |
| `Fatal`       | 系统崩溃       |

### 6.2 必须记录的事件

- 应用启动/关闭
- HTTP 请求（路径、状态码、耗时）
- 任务状态变更（JobId, OldStatus → NewStatus）
- 外部 API 调用（Gemini, 目标网站）
- 异常和重试

### 6.3 日志格式

```
[2026-01-29 10:00:00.123 +09:00] [INF] [JobProcessor] Job {JobId} transitioned from {OldStatus} to {NewStatus}
```

---

## 7. Git 规范 (Git Conventions)

### 7.1 分支策略

| 分支        | 用途         |
| :---------- | :----------- |
| `main`      | 生产就绪代码 |
| `develop`   | 开发集成分支 |
| `feature/*` | 功能开发     |
| `fix/*`     | Bug 修复     |

### 7.2 Commit Message

```
<type>(<scope>): <description>

[optional body]
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Examples**:

- `feat(api): add submission endpoint`
- `fix(browser): resolve memory leak in context disposal`
- `docs: update phase-1 plan`

---

## 8. 安全规范 (Security Specification)

### 8.1 敏感信息管理

| 信息      | 存储位置             | 注入方式       |
| :-------- | :------------------- | :------------- |
| GCP 凭证  | `secrets/gcp-*.json` | Docker Secret  |
| CF Token  | `secrets/cf-*.txt`   | Docker Secret  |
| DB 连接串 | 环境变量             | Docker Compose |

### 8.2 .gitignore 必须包含

```
# Secrets
secrets/
*.json.secret

# Data
data/
logs/

# Environment
.env
.env.local
```

### 8.3 网络安全

- **零入站端口**: 使用 Cloudflare Tunnel
- **身份验证**: Cloudflare Access (GitHub/Google OAuth)
- **HTTPS**: 由 Cloudflare 终止 TLS

---

## 9. 性能基准 (Performance Benchmarks)

| 指标                 | 目标值            |
| :------------------- | :---------------- |
| API 响应时间 (P95)   | < 100ms           |
| 单任务处理时间       | < 60s             |
| 内存占用 (Idle)      | < 256MB           |
| 内存占用 (100 Tasks) | < 512MB           |
| 浏览器轮换阈值       | 100 任务或 6 小时 |
