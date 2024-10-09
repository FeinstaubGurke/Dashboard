using Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<SensorStatusService>();

builder.Services.AddControllers().AddJsonOptions(configure =>
{
    //configure.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
