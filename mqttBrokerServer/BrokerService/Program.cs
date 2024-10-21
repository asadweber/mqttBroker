using BrokerService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<MqttClientService>();

// Register the MqttBrokerService as a singleton
builder.Services.AddSingleton<MqttBrokerService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Start the MQTT broker when the application starts
var mqttBrokerService = app.Services.GetRequiredService<MqttBrokerService>();
mqttBrokerService.StartMqttBrokerAsync().GetAwaiter().GetResult();

// Stop the MQTT broker when the application shuts down
app.Lifetime.ApplicationStopping.Register(() =>
{
    mqttBrokerService.StopMqttBrokerAsync().GetAwaiter().GetResult();
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
