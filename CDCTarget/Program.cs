using CDCTarget.Data;
using MessageBus;
using Microsoft.EntityFrameworkCore;
using ServiceMessageBus;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the SQLite Database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Register the Message Bus
builder.Services.AddSingleton<IMessageBus, InMemoryMessageBus>();

// Register the Architecture Modules
builder.Services.AddSingleton<CDCTarget.Modules.Logger>();
builder.Services.AddSingleton<CDCTarget.Modules.Processor>();
builder.Services.AddSingleton<CDCTarget.Modules.ThroughputDetector>();
builder.Services.AddSingleton<CDCTarget.Modules.EventManager>();

// Register the Service Layer
builder.Services.AddSingleton<CDCTarget.Services.IServices.IEventBindingService, CDCTarget.Services.EventBindingService>();

// 3. Add API Controller support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // For testing API endpoints via UI

var app = builder.Build();

// 4. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();


// Initialize the Event Binding Service on startup
using (var scope = app.Services.CreateScope())
{
    var bindingService = scope.ServiceProvider.GetRequiredService<CDCTarget.Services.IServices.IEventBindingService>();
    bindingService.InitializeBindings();
}

app.Run();