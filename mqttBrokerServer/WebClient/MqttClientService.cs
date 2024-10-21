
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
namespace BrokerService
{


    public class MqttClientService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IModel _rabbitChannel;
        private readonly string _rabbitQueueName = "mqtt-to-rabbitmq";

        public MqttClientService(IConfiguration configuration)
        {
            // MQTT setup
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // RabbitMQ setup
            var rabbitFactory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:Host"],
                Port = int.Parse(configuration["RabbitMQ:Port"]),
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"]
            };

            var rabbitConnection = rabbitFactory.CreateConnection();
            _rabbitChannel = rabbitConnection.CreateModel();
            _rabbitChannel.QueueDeclare(queue: _rabbitQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            

        }

        public async Task StartBridgeAsync(SubscriptionRequest request)
        {
          
            
            // Connect to the MQTT broker
            var mqttOptions = new MqttClientOptionsBuilder()
           .WithClientId(Guid.NewGuid().ToString())
           .WithTcpServer("localhost", 1883) // Address of the MQTT broker
           .WithCredentials("mqttuser", "mqttpass") // Username and password
           .WithCleanSession() // Clean session option
           .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)           
           .Build();
            
           if(!_mqttClient.IsConnected)
            await  _mqttClient.ConnectAsync(mqttOptions);

            //Unbind
            _mqttClient.ApplicationMessageReceivedAsync -= ApplicationMessageReceived();

            //Receiver From Broker 
            _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceived();


            // Subscribe to the MQTT topic
            await _mqttClient.SubscribeAsync(request.Topic);

        }


        private Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceived()
        {
            return async e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Received message from MQTT Broker: {message} for Client {e.ClientId}");

                // Forward message to RabbitMQ
                //PublishToRabbitMq(message);
                // Since it's async, we return a completed task
                StopBridge();
                await Task.CompletedTask;
            };
        }

        private void PublishToRabbitMq(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _rabbitChannel.BasicPublish(exchange: "", routingKey: _rabbitQueueName, basicProperties: null, body: body);
            Console.WriteLine($"Message forwarded to RabbitMQ: {message}");
        }

        public void StopBridge()
        {
            _mqttClient.DisconnectAsync();
            _rabbitChannel.Close();
        }
    }

}
