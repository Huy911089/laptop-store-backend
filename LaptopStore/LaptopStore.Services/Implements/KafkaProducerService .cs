using Confluent.Kafka;
using LaptopStore.Services.Configurations;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly KafkaOptions _kafkaOptions;

        public KafkaProducerService(IOptions<KafkaOptions> kafkaOptions, ILogger<KafkaProducerService> logger) 
        {
            _logger = logger;
            // Trích xuất giá trị cấu hình từ IOptions wrapper
            _kafkaOptions = kafkaOptions.Value;
            try
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = _kafkaOptions.BootstrapServers,
                    ClientId = _kafkaOptions.ClientId,

                    Acks = Acks.All,
                    EnableIdempotence = true
                };
                _producer = new ProducerBuilder<string, string>(config).Build();
                // Log theo chuẩn format đã thống nhất
                _logger.LogInformation("[KafkaProducerService] : Khởi tạo Producer thành công cho Client: {ClientId}", _kafkaOptions.ClientId);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[KafkaProducerService] : Lỗi khi khởi tạo Kafka Producer.");
                throw; // Rethrow để ứng dụng biết cấu hình Kafka đang có vấn đề ngay lúc khởi động
            }

        }

        public void Dispose()
        {
            _logger.LogInformation("[KafkaProducerService] : Đang dọn dẹp Kafka Producer, đợi xả nốt các message còn tồn đọng...");

            // Cực kỳ quan trọng: Ép Producer đẩy hết các message đang chờ trong RAM lên Server trong tối đa 10 giây
            _producer.Flush(TimeSpan.FromSeconds(10));

            _producer.Dispose();
            _logger.LogInformation("[KafkaProducerService] : Đã đóng Kafka Producer an toàn.");
        }

        public async Task ProduceAsync<T>(string topic, string key, T message)
        {
            try 
            {
                // [KafkaProducerService] : Serialize object C# thành chuỗi JSON để Kafka có thể hiểu được.
                var payload = JsonSerializer.Serialize(message);

                var kafkaMessage = new Message<string, string>
                {
                    Key = key,
                    Value = payload
                };
                // Gọi API của Confluent.Kafka để đẩy message
                var result = await _producer.ProduceAsync(topic, kafkaMessage);
                _logger.LogInformation(
                    "[KafkaProducerService] : Đã gửi thành công message tới topic '{Topic}', partition {Partition}, offset {Offset}.",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);
            }
            catch(ProduceException<string,string> ex)
            {
                // Lỗi do phía Kafka trả về (ví dụ: message quá lớn, topic không tồn tại...)
                _logger.LogError(ex, "[KafkaProducerService] : Lỗi từ Kafka khi gửi message. Lý do: {Reason}", ex.Error.Reason);
                throw; // Ném lỗi ra ngoài để tầng API (Controller) bắt và trả về Http status code phù hợp
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống (ví dụ: rớt mạng, mất kết nối...)
                _logger.LogError(ex, "[KafkaProducerService] : Lỗi hệ thống không xác định khi gửi message tới Kafka.");
                throw;
            }
        }
    }
}
