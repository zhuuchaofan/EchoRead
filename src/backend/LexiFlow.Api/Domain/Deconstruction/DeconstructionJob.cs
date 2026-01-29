namespace LexiFlow.Api.Domain.Deconstruction;

/// <summary>
/// 解构任务状态枚举
/// </summary>
public enum JobStatus
{
    Queued = 0,
    Fetching = 1,
    Cleaning = 2,
    Analyzing = 3,
    Completed = 4,
    Failed = 5
}

/// <summary>
/// 解构任务聚合根
/// 跟踪从排队到完成的全生命周期状态
/// </summary>
public class DeconstructionJob
{
    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public JobStatus Status { get; private set; }
    public string? RawContent { get; private set; }
    public string? CleanedMarkdown { get; private set; }
    public string? AnalysisJson { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // EF Core 需要的无参构造函数
    private DeconstructionJob() { }

    public static DeconstructionJob Create(Guid submissionId)
    {
        return new DeconstructionJob
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Status = JobStatus.Queued,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void TransitionTo(JobStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == JobStatus.Completed || newStatus == JobStatus.Failed)
        {
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void SetRawContent(string content)
    {
        RawContent = content;
    }

    public void SetCleanedMarkdown(string markdown)
    {
        CleanedMarkdown = markdown;
    }

    public void SetAnalysisResult(string json)
    {
        AnalysisJson = json;
    }

    public void RecordError(string error)
    {
        LastError = error;
        RetryCount++;
    }
}
