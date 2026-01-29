# Phase 2: 核心引擎 (The Engine)

> **预估工时**: 4-6 小时
> **目标**: 实现从 URL 提交到持久化队列的完整链路

---

## 1. DDD 领域模型设计

### 1.1 摄取上下文 (Ingestion Context)

```csharp
// Domain/Ingestion/Submission.cs
public class Submission
{
    public Guid Id { get; private set; }
    public SourceUrl SourceUrl { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
}

// Domain/Ingestion/SourceUrl.cs (Value Object)
public record SourceUrl
{
    public string Value { get; }
    public SourceUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format");
        Value = uri.ToString();
    }
}
```

### 1.2 解构上下文 (Deconstruction Context)

```csharp
// Domain/Deconstruction/DeconstructionJob.cs
public class DeconstructionJob
{
    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public JobStatus Status { get; private set; }
    public string? RawContent { get; private set; }
    public string? CleanedMarkdown { get; private set; }
    public AnalysisResult? Analysis { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
}

public enum JobStatus
{
    Queued,
    Fetching,
    Cleaning,
    Analyzing,
    Completed,
    Failed
}
```

---

## 2. DotNext 持久化通道集成

### 2.1 NuGet 包

```bash
dotnet add package DotNext.Threading --version 5.*
```

### 2.2 通道配置

```csharp
// Infrastructure/Channels/JobChannel.cs
public class JobChannel : IDisposable
{
    private readonly PersistentChannel<DeconstructionJob> _channel;

    public JobChannel(string dataPath)
    {
        var options = new PersistentChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            PartitionCapacity = 100  // 背压阈值
        };
        _channel = new PersistentChannel<DeconstructionJob>(
            Path.Combine(dataPath, "job-queue"),
            options
        );
    }

    public ValueTask EnqueueAsync(DeconstructionJob job, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(job, ct);

    public IAsyncEnumerable<DeconstructionJob> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);
}
```

---

## 3. Ingestion API 端点

### 3.1 Minimal API 设计

```csharp
// Endpoints/IngestionEndpoints.cs
public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/submissions");

        group.MapPost("/", async (SubmitUrlRequest request, IIngestionService service) =>
        {
            var result = await service.SubmitAsync(request.Url);
            return Results.Accepted($"/api/v1/submissions/{result.Id}", result);
        });

        group.MapGet("/{id:guid}", async (Guid id, IIngestionService service) =>
        {
            var submission = await service.GetByIdAsync(id);
            return submission is null ? Results.NotFound() : Results.Ok(submission);
        });
    }
}
```

---

## 4. 后台消费者服务

### 4.1 HostedService 骨架

```csharp
// Services/JobProcessorService.cs
public class JobProcessorService : BackgroundService
{
    private readonly JobChannel _channel;
    private readonly ILogger<JobProcessorService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobId} failed", job.Id);
                // 实现重试逻辑（Phase 3）
            }
        }
    }
}
```

---

## 5. 验证清单 (Verification Checklist)

- [ ] POST `/api/v1/submissions` 返回 `202 Accepted`
- [ ] 提交后任务进入持久化队列
- [ ] 模拟进程重启（Kill + Restart），队列中的任务恢复
- [ ] GET `/api/v1/submissions/{id}` 返回正确状态

---

## 6. 产出物 (Deliverables)

| 文件                                    | 描述                   |
| :-------------------------------------- | :--------------------- |
| `Domain/Ingestion/*.cs`                 | 摄取上下文领域模型     |
| `Domain/Deconstruction/*.cs`            | 解构上下文领域模型     |
| `Infrastructure/Channels/JobChannel.cs` | DotNext 持久化通道封装 |
| `Services/IngestionService.cs`          | 摄取服务               |
| `Services/JobProcessorService.cs`       | 后台处理服务骨架       |
| `Endpoints/IngestionEndpoints.cs`       | API 端点               |
