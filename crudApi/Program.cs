using crudApi;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Caching;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<HttpClient>(new HttpClient());
builder.Services.AddSingleton<CachedMemory>(new CachedMemory());


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
        policy =>
        {                          
                policy.WithOrigins(
                    "https://localhost/",
                    "https://www.memoglobal.com",
                    "https://www.google.com")
                .WithMethods("POST","GET","PUT","DELETE");
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(allowedOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
