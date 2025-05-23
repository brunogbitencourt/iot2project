using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;

using Iot2Project.Domain.Interfaces;
using Iot2Project.Application.Interfaces;

using Iot2Project.Infrastructure.Messaging.Mqtt;
using Iot2Project.Infrastructure.Messaging.Kafka;
using Iot2Project.Infrastructure.Persistence.Context;
using Iot2Project.Infrastructure.Persistence.Repositories;
using Iot2Project.Worker.Kafka;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Kafka: produtor de mensagens (singleton porque mantém conexão com o broker)
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        // Encaminha mensagem do MQTT para o Kafka
        services.AddSingleton<MqttMessageForwarder>();

        // Fábrica de conexões com PostgreSQL
        services.AddSingleton<IDbConnectionFactory, PgsqlConnectionFactory>();

        // Cria uma nova conexão por escopo (útil para repositórios)
        services.AddScoped<IDbConnection>(provider =>
            provider.GetRequiredService<IDbConnectionFactory>().CreateConnection());

        // Repositório para salvar dados de dispositivos (implementado via Dapper)
        services.AddScoped<IDeviceDataRepository, DeviceDataRepository>();

        // Worker principal que consome mensagens do MQTT e aciona o forwarder
        services.AddHostedService<Worker>();
        services.AddHostedService<KafkaDeviceDataConsumer>();


    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(); // habilita logs no console
    })
    .Build();

await host.RunAsync();
