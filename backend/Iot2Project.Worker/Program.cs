using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;

using Iot2Project.Domain.Ports;
using Iot2Project.Application.Interfaces;

using Iot2Project.Infrastructure.Messaging.Mqtt;
using Iot2Project.Infrastructure.Persistence.Context;
using Iot2Project.Infrastructure.Persistence.Repositories;
using Iot2Project.Worker.Kafka;
using Iot2Project.Application.Configuration;
using Iot2Project.Application.Messaging;
using Iot2Project.Worker.Services;
using MQTTnet.Client;
using MQTTnet;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        services.Configure<TopicRoutingOptions>(context.Configuration.GetSection("TopicRouting"));

        services.AddSingleton<IDbConnectionFactory, PgsqlConnectionFactory>();
        services.AddScoped<IDbConnection>(provider =>
            provider.GetRequiredService<IDbConnectionFactory>().CreateConnection());
        services.AddScoped<IDeviceDataRepository, DeviceDataRepository>();

        // MQTT dependencies
        services.AddSingleton<IMqttClient>(_ => new MqttFactory().CreateMqttClient());
        services.AddScoped<IMqttForwarder, MqttForwarder>();
        services.AddHostedService<MqttSubscriberService>();

        // Kafka consumer
        services.AddHostedService<KafkaDeviceDataConsumer>();
    })

    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(); // habilita logs no console
    })
    .Build();

await host.RunAsync();
