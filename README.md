# LexiFlow

> **生产级个人知识解构系统** - 将网页内容转化为结构化洞察

## 📖 项目概述

LexiFlow (The Deconstructor) 是一个基于 DDD 架构的个人微服务，能够自动抓取网页内容，通过 AI 进行深度分析，并将结果归档为可检索的知识库。

## 🛠️ 技术栈

- **Backend**: .NET 9 (LTS) + ASP.NET Core Minimal API
- **Database**: SQLite (WAL Mode)
- **Queue**: DotNext Persistent Channels
- **Browser**: Playwright for .NET
- **AI**: Gemini 3.0 Flash
- **Frontend**: Next.js 15 + Tailwind CSS 4 + PWA
- **Deployment**: Docker + Cloudflare Tunnel

## 📁 项目结构

```
LexiFlow/
├── src/backend/          # .NET 后端
├── src/frontend/         # Next.js 前端
├── docs/                 # 项目文档
├── scripts/              # 部署脚本
└── docker-compose.yml
```

## 🚀 快速开始

```bash
# 1. 克隆项目
git clone <repo-url>
cd LexiFlow

# 2. 启动后端
cd src/backend/LexiFlow.Api
dotnet run

# 3. 启动前端
cd src/frontend/lexiflow-web
npm install && npm run dev
```

## 📚 文档

- [Phase 1: 坚实地基](docs/phase-1-foundation.md)
- [Phase 2: 核心引擎](docs/phase-2-engine.md)
- [Phase 3: 智能工坊](docs/phase-3-workshop.md)
- [Phase 4: 用户界面](docs/phase-4-interface.md)
- [Phase 5: 生产交付](docs/phase-5-production.md)
- [项目规范](docs/project-specification.md)

## 📄 License

MIT
