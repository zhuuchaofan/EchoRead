# Phase 1: 坚实地基 (The Foundation)

> **预估工时**: 2-3 小时
> **目标**: 建立项目骨架，打通数据库与日志基础设施

---

## 1. 项目结构初始化

### 1.1 Monorepo 目录结构

```
/Volumes/fanxiang/LexiFlow/
├── src/
│   ├── backend/
│   │   └── LexiFlow.Api/           # .NET 9 Minimal API
│   └── frontend/
│       └── lexiflow-web/           # Next.js 15 App
├── docs/                           # 项目文档
├── scripts/                        # 部署脚本
├── docker-compose.yml
├── .gitignore
├── .editorconfig
└── README.md
```

### 1.2 命令清单

```bash
# 1. 创建 Solution
dotnet new sln -n LexiFlow -o src/backend
dotnet new webapi -n LexiFlow.Api -o src/backend/LexiFlow.Api --use-minimal-apis
dotnet sln src/backend/LexiFlow.sln add src/backend/LexiFlow.Api

# 2. 安装核心 NuGet 包
cd src/backend/LexiFlow.Api
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.*
dotnet add package Serilog.AspNetCore --version 9.*
dotnet add package Serilog.Sinks.File --version 6.*
```

---

## 2. SQLite 生产级配置

### 2.1 Pragma 拦截器

创建 `Infrastructure/SqliteWalInterceptor.cs`：

- 在连接打开时执行 `PRAGMA journal_mode=WAL`
- 设置 `synchronous=NORMAL`
- 设置 `busy_timeout=5000`
- 启用 `foreign_keys=ON`

### 2.2 DbContext 配置

- 使用 `UseSqlite()` 并注册拦截器
- 数据库文件路径: `data/lexiflow.db`

---

## 3. Serilog 结构化日志

### 3.1 配置要点

```json
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName"]
  }
}
```

### 3.2 关键日志点

- 应用启动/关闭
- HTTP 请求（Serilog.RequestLogging）
- 数据库连接状态

---

## 4. 验证清单 (Verification Checklist)

- [ ] `dotnet build` 无错误
- [ ] `dotnet run` 启动后访问 `/health` 返回 200
- [ ] 数据库文件 `data/lexiflow.db` 自动创建
- [ ] 日志文件 `logs/log-*.txt` 正常写入
- [ ] SQLite `journal_mode` 确认为 `wal`

---

## 5. 产出物 (Deliverables)

| 文件                                     | 描述                            |
| :--------------------------------------- | :------------------------------ |
| `LexiFlow.Api.csproj`                    | 项目文件，包含所有依赖          |
| `Program.cs`                             | 应用入口，配置 Serilog、EF Core |
| `Infrastructure/AppDbContext.cs`         | 数据库上下文                    |
| `Infrastructure/SqliteWalInterceptor.cs` | WAL 模式拦截器                  |
| `appsettings.json`                       | 配置文件                        |
