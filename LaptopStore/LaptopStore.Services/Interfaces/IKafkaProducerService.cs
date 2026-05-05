using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Interfaces
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync<T>(string topic, string key, T message);
    }
}
