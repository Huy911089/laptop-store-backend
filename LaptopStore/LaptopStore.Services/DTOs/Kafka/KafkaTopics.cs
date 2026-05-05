using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Kafka
{
    public class KafkaTopics
    {
        public const string OrderCreated = "order-created";
        public const string OrderStatusChanged = "order-status-changed";
    }
}
