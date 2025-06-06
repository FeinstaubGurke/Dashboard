using Dashboard.Clients;
using Dashboard.Jobs;
using Dashboard.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache();

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<TheThingsStackClient>();
builder.Services.AddScoped<IObjectStorageService, S3ObjectStorageService>();
builder.Services.AddScoped<IDataAggregationService, DataAggregationService>();

builder.Services.AddQuartz(q => {
    q.InterruptJobsOnShutdown = true;
    q.InterruptJobsOnShutdownWithWait = true;

    q.AddJob<SensorInactivityCheckJob>(opts => opts.WithIdentity(JobKeys.SensorInactivityCheck));
    q.AddJob<AggregateSensorDataJob>(opts => opts.WithIdentity(JobKeys.AggregateSensorData));

    q.AddTrigger(opts => opts
        .ForJob(JobKeys.SensorInactivityCheck)
        .WithIdentity($"{nameof(SensorInactivityCheckJob)}-trigger")
        .StartAt(new DateTimeOffset(DateTime.UtcNow.AddSeconds(30)))
        .WithSimpleSchedule(o => o.WithIntervalInMinutes(30).RepeatForever())
    );

    q.AddTrigger(opts => opts
        .ForJob(JobKeys.AggregateSensorData)
        .WithIdentity($"{nameof(AggregateSensorDataJob)}-trigger")
        .StartAt(DateBuilder.TodayAt(0, 10, 0))
        .WithSimpleSchedule(o => o.WithIntervalInHours(24).RepeatForever())
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseOutputCache();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
