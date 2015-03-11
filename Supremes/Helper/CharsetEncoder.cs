using System.Text;

namespace Supremes.Helper
{
    internal class CharsetEncoder
    {
        private readonly Encoder encoder;

        public CharsetEncoder(Encoding enc)
        {
            this.encoder = enc.GetEncoder();
            this.encoder.Fallback = EncoderFallback.ExceptionFallback;
        }

        public bool CanEncode(string input)
        {
            return CanEncode(input.ToCharArray());
        }

        public bool CanEncode(char c)
        {
            return CanEncode(new[] { c });
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
