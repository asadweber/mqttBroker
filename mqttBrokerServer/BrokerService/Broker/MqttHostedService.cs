namespace BrokerService.Broker
{
    public class MqttHostedService : IHostedService
    {
        private readonly MqttBrokerService _mqttBrokerService;

        public MqttHostedService(MqttBrokerService mqttBrokerService)
        {
            _mqttBrokerService = mqttBrokerService;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mqttBrokerService.StartMqttBrokerAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _mqttBrokerService.StopMqttBrokerAsync();
        }
    }
}
