using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Cria o schema aguardando o postgres ficar disponível (retry para race condition no K8s)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "Todos" (
                    "Id"        SERIAL       PRIMARY KEY,
                    "Title"     TEXT         NOT NULL DEFAULT '',
                    "Done"      BOOLEAN      NOT NULL DEFAULT false,
                    "CreatedAt" TIMESTAMPTZ  NOT NULL DEFAULT NOW()
                )
                """);
            logger.LogInformation("Banco de dados pronto.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Tentativa {Attempt}/10 falhou: {Message}", attempt, ex.Message);
            if (attempt == 10) throw;
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
