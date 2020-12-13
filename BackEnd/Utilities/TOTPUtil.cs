using System;
using OtpNet;
using System.Text;

namespace BackEnd.Ultilities
{
    public interface ITOTP
    {
        public string GenerateTOTP(string key);
        public bool IsValid();
        public bool VerifyToken(string token);
    }
    public class TOTPUtil :ITOTP
    {
        Totp totp;
        public string GenerateTOTP(string key)
        {
            totp = new Totp(Encoding.ASCII.GetBytes(key), step: int.Parse(new TimeSpan(0, 3, 0).TotalSeconds.ToString()));
            return totp.ComputeTotp();
        }
        public bool IsValid()
        {
            if (totp == null)
                return false;
            return totp.RemainingSeconds() >= 0;
        }

        public bool VerifyToken(string token)
        {
            long timeStepMatched;
            return totp.VerifyTotp(token, out timeStepMatched, null);
        }
    }
}
