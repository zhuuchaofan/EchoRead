using Microsoft.EntityFrameworkCore;
using Serilog;
using LexiFlow.Api.Infrastructure.Data;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("LexiFlow API 正在启动...");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog
    builder.Host.UseSerilog();

    // 配置 SQLite + WAL 模式
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=data/lexiflow.db";
        
        options.UseSqlite(connectionString)
               .AddInterceptors(new SqliteWalInterceptor());
    });

    // 添加健康检查
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    // 配置 OpenAPI
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // 确保数据目录存在
    Directory.CreateDirectory("data");
    Directory.CreateDirectory("logs");

    // 确保数据库已创建
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Log.Information("数据库初始化完成");
    }

    // 配置中间件
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Serilog 请求日志
    app.UseSerilogRequestLogging();

    // 健康检查端点
    app.MapHealthChecks("/health");

    // API 端点
    app.MapGet("/", () => Results.Ok(new { Message = "LexiFlow API v1.0", Status = "Running" }));

    app.MapGet("/api/v1/status", () => Results.Ok(new
    {
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow
    }));

    Log.Information("LexiFlow API 已启动，监听端口: {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
