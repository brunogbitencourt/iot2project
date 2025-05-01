using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using System.Text;

public class Worker : BackgroundService
{
    private IMqttClient _mqttClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("test.mosquitto.org", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());
            Console.WriteLine($"Mensagem recebida no tópico '{topic}': {message}");
        };

        _mqttClient.ConnectedAsync += async e =>
        {
            Console.WriteLine("Conectado ao broker MQTT");
            await _mqttClient.SubscribeAsync("iot2/tanks/test");
        };

        await _mqttClient.ConnectAsync(options, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
