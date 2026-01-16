using Microsoft.AspNetCore.Identity;

namespace AcmeWeeklyReportTest.Helpers
{
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<string> _hasher = new();

        // 產生 Hash（存進資料庫）
        public static string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        // 驗證密碼（登入時用）
        public static bool Verify(string hashedPassword, string inputPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                null!, hashedPassword, inputPassword);

            return result == PasswordVerificationResult.Success;
        }
    }
}
