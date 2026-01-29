namespace LexiFlow.Api.Domain.Ingestion;

/// <summary>
/// 提交状态枚举
/// </summary>
public enum SubmissionStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// 提交聚合根
/// 代表用户提交的一次 URL 解构请求
/// </summary>
public class Submission
{
    public Guid Id { get; private set; }
    public string SourceUrl { get; private set; } = string.Empty;
    public SubmissionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // EF Core 需要的无参构造函数
    private Submission() { }

    public static Submission Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL format", nameof(url));
        }

        return new Submission
        {
            Id = Guid.NewGuid(),
            SourceUrl = uri.ToString(),
            Status = SubmissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        Status = SubmissionStatus.Processing;
    }

    public void MarkAsCompleted()
    {
        Status = SubmissionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = SubmissionStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }
}
