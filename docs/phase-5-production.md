# Phase 5: 生产交付 (Production)

> **预估工时**: 2-3 小时
> **目标**: 容器化部署，配置零信任网络

---

## 1. Docker 容器化

### 1.1 后端 Dockerfile

```dockerfile
# src/backend/LexiFlow.Api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LexiFlow.Api.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 使用 Playwright 官方镜像
FROM mcr.microsoft.com/playwright/dotnet:v1.48.0-noble AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# 创建数据目录
RUN mkdir -p /app/data /app/logs
VOLUME ["/app/data", "/app/logs"]

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LexiFlow.Api.dll"]
```

### 1.2 前端 Dockerfile

```dockerfile
# src/frontend/lexiflow-web/Dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public

ENV PORT=3000
EXPOSE 3000

CMD ["node", "server.js"]
```

---

## 2. Docker Compose

### 2.1 docker-compose.yml

```yaml
version: "3.8"

services:
  backend:
    build:
      context: ./src/backend/LexiFlow.Api
    container_name: lexiflow-api
    restart: always
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/lexiflow.db
      - GOOGLE_APPLICATION_CREDENTIALS=/run/secrets/gcp-key
    secrets:
      - gcp-key
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    networks:
      - lexiflow-net

  frontend:
    build:
      context: ./src/frontend/lexiflow-web
    container_name: lexiflow-web
    restart: always
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:8080
    depends_on:
      - backend
    networks:
      - lexiflow-net

  cloudflared:
    image: cloudflare/cloudflared:latest
    container_name: lexiflow-tunnel
    restart: always
    command: tunnel run
    environment:
      - TUNNEL_TOKEN_FILE=/run/secrets/cf-tunnel-token
    secrets:
      - cf-tunnel-token
    depends_on:
      - frontend
    networks:
      - lexiflow-net

secrets:
  gcp-key:
    file: ./secrets/gcp-service-account.json
  cf-tunnel-token:
    file: ./secrets/cloudflare-tunnel-token.txt

networks:
  lexiflow-net:
    driver: bridge
```

---

## 3. Cloudflare Tunnel 配置

### 3.1 创建 Tunnel

```bash
# 在 Cloudflare Dashboard 创建 Tunnel 或使用 CLI
cloudflared tunnel create lexiflow
cloudflared tunnel route dns lexiflow lexiflow.yourdomain.com
```

### 3.2 Tunnel 配置文件

```yaml
# ~/.cloudflared/config.yml (仅供参考，实际使用 Token)
tunnel: <TUNNEL_ID>
credentials-file: /root/.cloudflared/<TUNNEL_ID>.json

ingress:
  - hostname: lexiflow.yourdomain.com
    service: http://frontend:3000
  - hostname: api.lexiflow.yourdomain.com
    service: http://backend:8080
  - service: http_status:404
```

### 3.3 Zero Trust Access Policy

1. 在 Cloudflare Dashboard → Zero Trust → Access → Applications
2. 创建应用，配置规则：
   - **Policy**: 仅允许特定 GitHub/Google 账号
   - **Session Duration**: 24 小时

---

## 4. 部署脚本

### 4.1 scripts/deploy.sh

```bash
#!/bin/bash
set -e

echo "🚀 Starting LexiFlow deployment..."

# 1. 拉取最新代码
git pull origin main

# 2. 构建镜像
docker compose build --no-cache

# 3. 停止旧容器
docker compose down

# 4. 启动新容器
docker compose up -d

# 5. 健康检查
echo "⏳ Waiting for health check..."
sleep 10
curl -f http://localhost:8080/health || exit 1

echo "✅ Deployment complete!"
docker compose ps
```

---

## 5. 密钥管理清单

| 密钥                | 位置                                  | 用途            |
| :------------------ | :------------------------------------ | :-------------- |
| GCP Service Account | `secrets/gcp-service-account.json`    | Gemini API 认证 |
| Cloudflare Token    | `secrets/cloudflare-tunnel-token.txt` | Tunnel 连接     |

> [!CAUTION]
> 确保 `secrets/` 目录已添加到 `.gitignore`！

---

## 6. 验证清单 (Verification Checklist)

- [ ] `docker compose build` 成功
- [ ] `docker compose up -d` 所有容器运行
- [ ] 本地访问 `http://localhost:8080/health` 返回 200
- [ ] Cloudflare Tunnel 正常连接
- [ ] 通过外网域名访问前端
- [ ] Zero Trust 认证拦截未授权用户

---

## 7. 产出物 (Deliverables)

| 文件                                   | 描述                  |
| :------------------------------------- | :-------------------- |
| `src/backend/LexiFlow.Api/Dockerfile`  | 后端 Dockerfile       |
| `src/frontend/lexiflow-web/Dockerfile` | 前端 Dockerfile       |
| `docker-compose.yml`                   | 编排文件              |
| `scripts/deploy.sh`                    | 部署脚本              |
| `.gitignore`                           | 包含 secrets 排除规则 |
