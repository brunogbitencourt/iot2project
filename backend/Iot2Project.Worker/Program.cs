using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Iot2Project.Domain.Interfaces;
using Iot2Project.Infrastructure.Messaging.Mqtt;
using Iot2Project.Infrastructure.Messaging.Kafka;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Kafka producer (singleton)
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        // Forwarder: faz o bridge MQTT → Kafka
        services.AddSingleton<MqttMessageForwarder>();

        // Worker que consome MQTT e usa o forwarder
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();
