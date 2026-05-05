using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LaptopStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestKafkaController : ControllerBase
{
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly ILogger<TestKafkaController> _logger;

    public TestKafkaController(
        IKafkaProducerService kafkaProducerService,
        ILogger<TestKafkaController> logger)
    {
        _kafkaProducerService = kafkaProducerService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessageToKafka([FromBody] object payload)
    {
        try
        {
            _logger.LogInformation("[TestKafkaController] : Bắt đầu gửi message test lên Kafka.");

            // Gọi service để đẩy data. "test-topic" là tên topic trên Kafka, "test-key" là khóa phân cụm.
            await _kafkaProducerService.ProduceAsync("test-topic", "test-key", payload);

            // Trả về đúng format chuẩn đã thống nhất
            return Ok(new
            {
                status = 200,
                message = "Gửi message lên Kafka thành công",
                data = payload
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestKafkaController] : Lỗi khi gửi message.");

            // Tránh thiếu try-catch và đảm bảo response lỗi cũng chuẩn format
            return StatusCode(500, new
            {
                status = 500,
                message = "Có lỗi xảy ra khi giao tiếp với Kafka Server",
                data = (object)null // Data trả về null khi lỗi
            });
        }
    }
}