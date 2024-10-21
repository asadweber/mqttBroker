using MQTTnet;
using MQTTnet.Server;
using System.Text;
namespace BrokerService
{


    public class MqttBrokerService
    {
        private MqttServer _mqttServer;

        public async Task StartMqttBrokerAsync()
        {
            // Configure the MQTT server options
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()  // Default non-encrypted endpoint
                .WithPersistentSessions(true)
                .WithDefaultEndpointPort(1883)
                ;

            // Create the MQTT server instance
            _mqttServer = new MqttFactory().CreateMqttServer(optionsBuilder.Build());

            // Handle client connection Validation
            _mqttServer.ValidatingConnectionAsync += async e =>
                 {
                     // Perform connection validation here
                     if (!IsValidClient(e.ClientId, e.UserName, e.Password))
                     {
                         Console.WriteLine($"Client {e.ClientId} failed to connect: Invalid credentials.");
                         e.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
                         return;
                     }
                     await Task.CompletedTask;
                 };

            // Handle client connection
            _mqttServer.ClientConnectedAsync += async e =>
            {
                Console.WriteLine($"Client {e.ClientId} connected.");
                await Task.CompletedTask;
            };

            // Handle client disconnection
            _mqttServer.ClientDisconnectedAsync += async e =>
            {
                Console.WriteLine($"Client {e.ClientId} disconnected.");
                await Task.CompletedTask;
            };

            // Handle subscription interception
            _mqttServer.InterceptingSubscriptionAsync += async e =>
            {
                e.TopicFilter.RetainAsPublished = true;
                Console.WriteLine($"Client {e.ClientId} is attempting to subscribe to {e.TopicFilter.Topic}.");
                e.ProcessSubscription = true;  // Accept subscription
                Console.WriteLine($"Subscription to {e.TopicFilter.Topic} was accepted.");
                

                await SendMessageToClient(e.ClientId, e.TopicFilter.Topic, "Message From Server");
                await Task.CompletedTask;
            };

            //Handle incoming messages
            //_mqttServer.InterceptingPublishAsync += async e =>
            //{
            //    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            //    Console.WriteLine($"Broker:Internal Received message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {message}");
            //    // Optionally, you can route or forward messages here
            //    await Task.CompletedTask;
            //};

            // Start the MQTT server
            await _mqttServer.StartAsync();
            Console.WriteLine("MQTT Broker started at port 1883.");
        }

        public async Task SendMessageToClient(string clientId,string topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
            .Build();
            var mqttApplicationMessage = new InjectedMqttApplicationMessage(mqttMessage)
            {
                SenderClientId = clientId
            };

            // Send message to the specific client
            await _mqttServer.InjectApplicationMessage(mqttApplicationMessage);
            Console.WriteLine($"Broker:Message published to client {clientId} on topic {topic}: {message}");
           await Task.CompletedTask;
        }

        private bool IsValidClient(string clientId, string username, string password)
        {
            // Replace this with your actual validation logic
            return username == "mqttuser" && password == "mqttpass";
        }

        public async Task StopMqttBrokerAsync()
        {
            if (_mqttServer != null)
            {
                await _mqttServer.StopAsync();
                Console.WriteLine("MQTT Broker stopped.");
            }
        }
    }

}
