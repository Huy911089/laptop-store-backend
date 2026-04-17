using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Repositories.Entities
{
    public class RefreshToken
    {
        // [RefreshToken] : Khóa chính của bảng refresh token.
        public Guid Id { get; set; } = Guid.NewGuid();

        // [RefreshToken] : Token random thật sự được cấp cho client để xin access token mới.
        public string Token { get; set; } = string.Empty;

        // [RefreshToken] : Liên kết refresh token này thuộc về user nào.
        public Guid UserId { get; set; }

        // [RefreshToken] : Thời điểm token được tạo.
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // [RefreshToken] : Thời điểm hết hạn của refresh token.
        public DateTime ExpiresAtUtc { get; set; }

        // [RefreshToken] : Nếu token bị thu hồi thì đánh dấu thời gian revoke.
        public DateTime? RevokedAtUtc { get; set; }

        // [RefreshToken] : Lý do revoke để sau này debug hoặc audit.
        public string? ReplacedByToken { get; set; }

        // [RefreshToken] : Navigation property để EF Core join sang bảng User.
        public User User { get; set; } = null!;
    }
}
