using System.Security.Cryptography;

using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates bytes array populated from specified seed, with fast speed designed for network transmissions.
    /// </summary>
    public class LiteEn : ITranformer
    {
        private readonly string _seed;
        private readonly byte[] _salt;

        private readonly int _sCount;
        private readonly SHA256 _sha256;
        private readonly SHA384 _sha384;
        private readonly SHA512 _sha512;

        /// <inheritdoc/>
        public event EventHandler<double> ProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteEn"/> class.
        /// </summary>
        /// <param name="seed">
        /// The seed to generate values.
        /// </param>
        /// <param name="salt">
        /// The additional value to alter the seed and outputs.
        /// </param>
        public LiteEn(byte[] seed, byte[] salt)
        {
            _sha256 = SHA256.Create();
            _sha384 = SHA384.Create();
            _sha512 = SHA512.Create();

            _seed = BitConverter.ToString(_sha512.ComputeHash(seed.Concat(salt).ToArray())).Replace("-", "");
            _salt = _sha384.ComputeHash(salt);
            _sCount = _seed.Length;
        }

        /// <inheritdoc/>
        public void Generate(byte[] buffer, int rotate)
        {
            double totalProgress = 0;

            if (rotate < 1)
            {
                rotate = Transformers.ToAbsInt32(_salt, (buffer.Length % 41) + _seed[28] + _salt[0]) % 10;
            }

            byte[] tl = new byte[5]
            {
                (byte)(buffer.Length * 6 % 256),
                (byte)Math.Abs((buffer.Length - _sCount) * rotate % 256),
                (byte)(buffer.Length * 124 % 256),
                (byte)((buffer.Length + rotate) * 75 % 128),
                (byte)(buffer.Length * 13 % 64),
            };

            byte[] tl2 = _sha384.ComputeHash(Transformers.Mix((buffer.Length + rotate) * _seed[17], tl, _salt));

            byte[] vf = _sha512.ComputeHash(tl).Concat(_sha256.ComputeHash(ChangeCase(tl2.Base64Encode(), _salt[23] + (buffer.Length % 14)).Base64EncodeToByte()).Base64EncodeToByte()).ToArray();
            byte[] vt = _sha256.ComputeHash(vf.Concat(_sha384.ComputeHash(tl2.Select(x => (byte)(x * 6 % 256)).ToArray())).ToArray());

            int i = 0;

            while (i < buffer.Length)
            {
                vt = _sha384.ComputeHash(_sha512.ComputeHash(vt));

                for (int j = 0; j < rotate; j++)
                {
                    vt = _sha256.ComputeHash(_sha384.ComputeHash(vt).Concat(_salt).ToArray());
                }

                Array.Copy(vt, 0, buffer, i, Math.Min(vt.Length, buffer.Length - i));

                totalProgress += (double)vt.Length / buffer.Length;
                ProgressChanged?.Invoke(this, totalProgress * 100);

                i += vt.Length;
            }
        }

        private string ChangeCase(string src, int seed)
        {
            int c = 0;

            char[] clone = src.ToArray();
            Transformers.Shuffle(clone, seed);

            return new string(clone.Select(x =>
            {
                return c++ % 2 == 0 ? x.ToString().ToUpper()[0] : x;
            }).ToArray());
        }
    }
}
