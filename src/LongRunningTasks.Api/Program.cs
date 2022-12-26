using Hangfire;
using LongRunningTasks.Application;
using LongRunningTasks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// For using backgroud workers as Windows Services
// builder.Host.UseWindowsService(); 
    
// Add services to the container.
//builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.AddApplication();
builder.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard();

app.UseAuthorization();

app.MapControllers();

app.Run();
