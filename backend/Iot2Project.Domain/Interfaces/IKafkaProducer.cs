using System.Threading.Tasks;

namespace Iot2Project.Domain.Interfaces
{
    public interface IKafkaProducer
    {
        Task PublishAsync(string topic, string message);
    }
}
