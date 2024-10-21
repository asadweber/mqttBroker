using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;
namespace BrokerService.Broker
{


    public class MqttBrokerService
    {
        private MqttServer _mqttServer;

        public async Task StartMqttBrokerAsync()
        {

            // Configure the MQTT server options
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()  // Default non-encrypted endpoint
                .WithDefaultEndpointPort(1883)
                ;

            // Create the MQTT server instance
            var factory = new MqttFactory();
            _mqttServer = factory.CreateMqttServer(optionsBuilder.Build());

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
                Console.WriteLine($"Client {e.ClientId} is attempting to subscribe to {e.TopicFilter.Topic}.");
                e.ProcessSubscription = true;  // Accept subscription

                await SendMessageToClient(e.ClientId, e.TopicFilter.Topic, "Sent Message to Client");

                await Task.CompletedTask;
            };

            //Handle incoming messages
            //_mqttServer.InterceptingPublishAsync += async e =>
            //{
            //    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            //    Console.WriteLine($"Broker:Internal Received message from {e.ClientId} on topic {e.ApplicationMessage.Topic}: {message}");
            //    // Optionally, you can route or forward messages here
            //    e.ProcessPublish = true;
            //    await Task.CompletedTask;
            //};

            // Start the MQTT server
            await _mqttServer.StartAsync();
            Console.WriteLine("MQTT Broker started at port 1883.");
        }

        public async Task SendMessageToClient(string clientId, string topic, string message)
        {
            // Publish a message to a topic after 5 seconds
            var mqttApplicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic) // Topic name
                .WithPayload(Encoding.UTF8.GetBytes(message)) // Message content
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // QoS
                .WithRetainFlag(false)
                .Build();


            var data = new InjectedMqttApplicationMessage(mqttApplicationMessage)
            {
                SenderClientId = clientId
            };

            // Publish the message
            await _mqttServer.InjectApplicationMessage(data);
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
