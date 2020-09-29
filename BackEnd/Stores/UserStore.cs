using BackEnd.Models;
using BackEnd.Ultilities;

namespace BackEnd.Stores
{
    public interface IUserStore
    {
        public bool IsResetPasswordTokenVerified { get; set; }
        public bool TokenIdVerified { get; set; }
        public string GenerateTokenAndSave(string key);
        public bool IsTokenValid(string token);
    }

    public class UserStore : IUserStore
    {
        public bool IsResetPasswordTokenVerified { get; set; } = false;
        public bool TokenIdVerified { get; set; } = false;

        private readonly ITOTP _tOTPUtil;

        public UserStore(ITOTP tOTPUtil)
        {
            _tOTPUtil = tOTPUtil;
        }
        public string GenerateTokenAndSave(string key)
        {
            return _tOTPUtil.GenerateTOTP(key);
        }

        public bool IsTokenValid(string token)
        {
            var result = _tOTPUtil.IsValid() && _tOTPUtil.VerifyToken(token);
            if (result)
                IsResetPasswordTokenVerified = true;
        return result;
        }
    }
}
