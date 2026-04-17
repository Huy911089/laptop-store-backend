namespace LaptopStore.Services.Configurations;

// [JwtSettings] : Class cấu hình typed options để tránh hard-code key JWT và tránh đọc IConfiguration rời rạc trong service.
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    // [JwtSettings] : Key bí mật dùng để ký token, phải đủ dài để an toàn.
    public string Key { get; set; } = string.Empty;
    // [JwtSettings] : Issuer là server phát hành token.
    public string Issuer { get; set; } = string.Empty;
    // [JwtSettings] : Audience mà token phục vụ.
    public string Audience { get; set; } = string.Empty ;
    // [JwtSettings] : Số phút sống của access token.
    public int AccessTokenExpireMinutes { get; set; }

    // [JwtSettings] : Số ngày sống của refresh token.
    public int RefreshTokenExpireDays { get; set; }
}
