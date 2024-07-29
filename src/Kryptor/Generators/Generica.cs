using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using MoreLinq;
using MoreLinq.Extensions;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Generators
{
    /// <summary>
    /// Generates bytes array populated from specified seed.
    /// </summary>
    public class Generica : ITranformer
    {
        private readonly string _seed;
        private readonly byte[] _salt;

        private readonly int _sCount;
        private readonly SHA256 _sha256;
        private readonly SHA384 _sha384;
        private readonly SHA512 _sha512;

        /// <inheritdoc/>
        public event Action<double> OnProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generica"/> class.
        /// </summary>
        /// <param name="seed">
        /// The seed to generate values.
        /// </param>
        /// <param name="salt">
        /// The additional value to alter the seed and outputs.
        /// </param>
        public Generica(byte[] seed, byte[] salt)
        {
            _sha256 = SHA256.Create();
            _sha384 = SHA384.Create();
            _sha512 = SHA512.Create();

            _seed = BitConverter.ToString(_sha512.ComputeHash(Transformers.Mix(Transformers.ToInt32(seed, seed[12 % seed.Length] * salt[3 % salt.Length]), seed, salt).ToArray())).Replace("-", "");
            _salt = _sha384.ComputeHash(salt);
            _sCount = _seed.Length;
        }

        /// <inheritdoc/>
        public void Generate(byte[] buffer, int rotate)
        {
            double totalProgress = 0;

            if (rotate < 1)
            {
                rotate = Transformers.ToAbsInt32(_salt, buffer.Length % 41 + _seed[28] + _salt[0]) % 100;
            }

            byte[] tl = new byte[5]
            {
                (byte)(buffer.Length * 6 % 256),
                (byte)Math.Abs((buffer.Length - _sCount) * rotate % 256),
                (byte)(buffer.Length * 124 % 256),
                (byte)((buffer.Length + rotate) * 75 % 128),
                (byte)(buffer.Length * 13 % 64),
            };

            tl = _sha384.ComputeHash(Transformers.Mix((buffer.Length + rotate) * _seed[6], tl, _salt));

            byte[] hashes = new byte[3][]
            {
                _sha512.ComputeHash(Encode(MoreEnumerable.Repeat(ChangeCase(_seed, _salt[26] * _salt[7] + buffer.Length % 13), Math.Max(buffer.Length % 10, 1)).ToArray())),
                _sha384.ComputeHash(Encode(new string(_seed.Chunk(_sCount / 2).Last()).PadRight(_sCount * 2, Convert.ToString(_sha512.ComputeHash(tl)).Replace("-", "")[5]).PadLeft(_sCount * 5, Convert.ToString(_sha384.ComputeHash(tl.Base64EncodeToByte())).Replace("-", "")[6]))),
                _sha256.ComputeHash(Encode(ChangeCase(Convert.ToBase64String(Encode(_seed)), _seed[19] + buffer.Length % 7)))
            }.SelectMany(x => x).OrderBy(x => x * 9 % 24).ToArray();

            byte[] vm = _sha384.ComputeHash(hashes.Select(x => (byte)(x * 7 % 256)).ToArray());
            byte[] vf = _sha512.ComputeHash(hashes).Concat(_sha256.ComputeHash(ChangeCase(tl.Base64Encode(), _salt[39] + buffer.Length % 11).Base64EncodeToByte()).Base64EncodeToByte()).ToArray();
            byte[] vt = _sha256.ComputeHash(vf.Concat(_sha384.ComputeHash(hashes.Select(x => (byte)((x * 11 / 4 * 6 + 5) % 256)).ToArray())).ToArray());

            int i = 0;

            while (i < buffer.Length)
            {
                vt = _sha384.ComputeHash(_sha512.ComputeHash(vt)
                                                .Concat(_sha384.ComputeHash(new byte[] { (byte)(Math.Abs(buffer.Length - i) % 256) }))
                                                .Concat(vf.Take(Math.Abs(buffer.Length - i) % 64))
                                                .Concat(vm.Take((i + 2) * 3 % 48))
                                                .ToArray());

                for (int j = 0; j < rotate; j++)
                {
                    vt = _sha256.ComputeHash(Transformers.Mix(i > 0 && j > 0 ? i * j + _seed[43] + _seed[9] + _salt[34] : _seed[15] * rotate + buffer.Length, _sha384.ComputeHash(vt), _salt).ToArray());
                }

                Array.Copy(vt, 0, buffer, i, Math.Min(vt.Length, buffer.Length - i));

                totalProgress += vt.Length / buffer.Length;
                OnProgress?.Invoke(totalProgress * 100);

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

        private static byte[] Encode(char[] src)
        {
            return Encoding.UTF8.GetBytes(src);
        }

        private static byte[] Encode(string src)
        {
            return Encoding.UTF8.GetBytes(src);
        }
    }
}
