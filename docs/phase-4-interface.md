# Phase 4: 用户界面 (The Interface)

> **预估工时**: 4-6 小时
> **目标**: 构建 Next.js 15 PWA 前端，实现提交与状态追踪

---

## 1. 项目初始化

### 1.1 创建 Next.js 15 项目

```bash
cd src/frontend
npx -y create-next-app@latest lexiflow-web \
  --typescript \
  --tailwind \
  --eslint \
  --app \
  --src-dir \
  --import-alias "@/*" \
  --turbopack
```

### 1.2 安装依赖

```bash
cd lexiflow-web
npm install swr @serwist/next lucide-react
npm install -D @serwist/build
```

---

## 2. 目录结构

```
src/frontend/lexiflow-web/
├── src/
│   ├── app/
│   │   ├── layout.tsx
│   │   ├── page.tsx              # 首页（提交入口）
│   │   ├── submissions/
│   │   │   └── [id]/page.tsx     # 任务详情页
│   │   └── api/                  # BFF 代理（可选）
│   ├── components/
│   │   ├── ui/                   # 基础组件
│   │   ├── SubmitForm.tsx
│   │   ├── SubmissionCard.tsx
│   │   └── StatusBadge.tsx
│   ├── lib/
│   │   ├── api.ts                # API 客户端
│   │   └── hooks/
│   │       └── useSubmission.ts  # SWR Hook
│   └── sw.ts                     # Service Worker
├── public/
│   ├── manifest.json
│   └── icons/
└── next.config.ts
```

---

## 3. 核心组件

### 3.1 SubmitForm

```tsx
// components/SubmitForm.tsx
"use client";

import { useState } from "react";
import { submitUrl } from "@/lib/api";

export function SubmitForm() {
  const [url, setUrl] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      const result = await submitUrl(url);
      // 跳转到详情页或显示成功提示
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex gap-2">
      <input
        type="url"
        value={url}
        onChange={(e) => setUrl(e.target.value)}
        placeholder="输入 URL..."
        className="flex-1 px-4 py-2 rounded-lg border"
        required
      />
      <button
        type="submit"
        disabled={isSubmitting}
        className="px-6 py-2 bg-blue-600 text-white rounded-lg"
      >
        {isSubmitting ? "提交中..." : "解构"}
      </button>
    </form>
  );
}
```

### 3.2 SWR 轮询 Hook

```tsx
// lib/hooks/useSubmission.ts
import useSWR from "swr";
import { fetcher } from "@/lib/api";

export function useSubmission(id: string) {
  const { data, error, isLoading } = useSWR(
    `/api/v1/submissions/${id}`,
    fetcher,
    {
      refreshInterval: 2000, // 2秒轮询
      revalidateOnFocus: true,
      // 任务完成后停止轮询
      isPaused: () => data?.status === "Completed" || data?.status === "Failed",
    },
  );

  return { submission: data, error, isLoading };
}
```

---

## 4. PWA 配置 (Serwist)

### 4.1 next.config.ts

```typescript
import withSerwistInit from "@serwist/next";

const withSerwist = withSerwistInit({
  swSrc: "src/sw.ts",
  swDest: "public/sw.js",
});

export default withSerwist({
  // Next.js 配置
});
```

### 4.2 manifest.json

```json
{
  "name": "LexiFlow",
  "short_name": "LexiFlow",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#0f172a",
  "theme_color": "#3b82f6",
  "icons": [
    { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

---

## 5. 设计规范

### 5.1 颜色系统

| 用途       | 颜色                  |
| :--------- | :-------------------- |
| Primary    | `#3b82f6` (Blue-500)  |
| Background | `#0f172a` (Slate-900) |
| Surface    | `#1e293b` (Slate-800) |
| Text       | `#f1f5f9` (Slate-100) |
| Success    | `#22c55e` (Green-500) |
| Error      | `#ef4444` (Red-500)   |

### 5.2 状态徽章

- `Queued` → 灰色
- `Fetching` / `Cleaning` / `Analyzing` → 蓝色（脉冲动画）
- `Completed` → 绿色
- `Failed` → 红色

---

## 6. 验证清单 (Verification Checklist)

- [ ] 首页正确渲染，表单可提交
- [ ] 提交后跳转到详情页
- [ ] 详情页状态实时更新（SWR 轮询）
- [ ] 任务完成后停止轮询
- [ ] PWA 可安装（桌面/移动端）
- [ ] 离线访问显示缓存页面

---

## 7. 产出物 (Deliverables)

| 文件                            | 描述          |
| :------------------------------ | :------------ |
| `app/page.tsx`                  | 首页          |
| `app/submissions/[id]/page.tsx` | 任务详情页    |
| `components/SubmitForm.tsx`     | 提交表单      |
| `components/SubmissionCard.tsx` | 任务卡片      |
| `lib/hooks/useSubmission.ts`    | SWR 轮询 Hook |
| `public/manifest.json`          | PWA 清单      |
