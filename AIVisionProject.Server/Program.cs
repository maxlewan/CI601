using AIVisionProject.Server;
using AIVisionProject.Server.Services;
using Azure.Identity;
using Microsoft.Extensions.Azure;
var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddAzureKeyVault(Constants.keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AccountStorageConnection:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AccountStorageConnection:queue"]!, preferMsi: true);
});
builder.Services.AddScoped<IStorageService, BlobService>();
builder.Services.AddScoped<IVideoAnalysisService, VideoIndexerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder => builder.WithOrigins("https://localhost:5173") // Add your frontend URL here
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowOrigin");
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
