using Application;
using Infrastructure;
using WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // API can read and write Enum values as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

// Register Application layer services (validators, application services)
builder.Services.AddApplication();

// Register Infrastructure layer services (DbContext, IDateTimeProvider, Repositories, HttpClients with Polly)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictCorsPolicy", policy =>
    {
        // Only localhost is allowed
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("StrictCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
