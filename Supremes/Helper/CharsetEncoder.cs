using System.Text;

namespace Supremes.Helper
{
    internal class CharsetEncoder
    {
        private readonly Encoder encoder;

        public CharsetEncoder(Encoding enc)
        {
            this.encoder = enc.GetEncoder();
#if NET45
            this.encoder.Fallback = EncoderFallback.ExceptionFallback;
#endif
        }

        public bool CanEncode(char[] chars)
        {
            try
            {
                encoder.GetByteCount(chars, 0, chars.Length, true);
                return true;
            }
            catch (EncoderFallbackException)
            {
                return false;
            }
        }
    }
}
