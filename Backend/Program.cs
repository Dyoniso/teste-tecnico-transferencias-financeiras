using Backend.Data;
using Backend.Middlewares;
using Backend.Repositories;
using Backend.Repositories.Interfaces;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Services.Kafka;
using Backend.Services.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(
            builder.Configuration
                .GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

/*
 * Repositories
 */
builder.Services.AddScoped<
    IAccountRepository,
    AccountRepository>();

builder.Services.AddScoped<
    ITransferRepository,
    TransferRepository>();

builder.Services.AddScoped<
    ITransferAttemptRepository,
    TransferAttemptRepository>();

builder.Services.AddScoped<
    ITransferLimitRepository,
    TransferLimitRepository>();

builder.Services.AddScoped<
    IOutboxRepository,
    OutboxRepository>();

/*
 * Services
 */
builder.Services.AddScoped<
    IAccountService,
    AccountService>();

builder.Services.AddScoped<
    ITransferService,
    TransferService>();

builder.Services.AddScoped<
    ITransferLimitService,
    TransferLimitService>();

/*
 * Kafka
 */
builder.Services.AddSingleton<
    IKafkaProducer,
    KafkaProducer>();

/*
 * Workers
 */
builder.Services.AddHostedService<
    ScheduledTransferWorker>();

builder.Services.AddHostedService<
    OutboxPublisherWorker>();

builder.Services.AddHostedService<
    KafkaConsumerService>();

/*
 * Swagger
 */
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Transferências Financeiras API",
            Version = "v1",
            Description =
                "API REST para transferências financeiras imediatas e agendadas."
        });

    var xmlFile =
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

/*
 * Inicialização automática do banco.
 */
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    var logger =
        scope.ServiceProvider
            .GetRequiredService<
                ILogger<Program>>();

    const int maxRetries = 5;

    for (var attempt = 1;
         attempt <= maxRetries;
         attempt++)
    {
        try
        {
            logger.LogInformation(
                "Inicializando banco de dados.");

            /*
             * Se existirem migrations,
             * aplica normalmente.
             */
            var hasMigrations =
                (await dbContext.Database
                    .GetPendingMigrationsAsync())
                .Any();

            if (hasMigrations)
            {
                logger.LogInformation(
                    "Migrations encontradas. Aplicando migrations pendentes.");

                await dbContext.Database
                    .MigrateAsync();
            }
            else
            {
                /*
                 * Não existem migrations.
                 *
                 * Cria o banco e toda a estrutura
                 * diretamente a partir dos Models.
                 */
                logger.LogInformation(
                    "Nenhuma migration encontrada. Criando estrutura inicial do banco.");

                await dbContext.Database
                    .EnsureCreatedAsync();
            }

            logger.LogInformation(
                "Banco de dados inicializado com sucesso.");

            break;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Erro ao inicializar banco. Tentativa {Attempt}/{MaxRetries}.",
                attempt,
                maxRetries);

            if (attempt == maxRetries)
            {
                throw;
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5));
        }
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();