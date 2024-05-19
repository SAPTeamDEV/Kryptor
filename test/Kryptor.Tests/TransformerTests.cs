using System.Text;

namespace SAPTeam.Kryptor.Tests
{
    public class TransformerTests
    {
        private const string GenericaBase64Sample = "WdALRwtr0ScN6LcmVnG36eOCJOhaDZBW8VBmvk3Ncc6xalm01XJuU2d/KR/Nb1eJnAOhO6ydW1MJE6gpijgwik+S7oUTOTmGkukclNpPwy23YYatfCKGQy8ExlJqnpbhLDx82/R/3sKuiId0Tr+xTR8MeVRaLiBXtk2K++REAcpyKpFlur3NTdov7DneVP42mwDy8NEHh2De21bCFSxfjjpu8b9oShJSshITcTJI8fTrj9nJSNJf/bnVBdoLnpWJLf9E6XrQCaSDJ/toVn6u6YE1+IZChr03UU1M9rwfmih5CpEtioSTJUGjeRSSysZ9vtpi0/QlizxYWjjcAFWalxkj5KCefCC8O4kWO7q4GlqSj/ax+e197ByNshbd5X9tIxwOOD55eR7u3PzzHVJWBOVCO3jcH/z6mqow+QAEAlEygBCOT3WaAhGBDZm3WJEORfDYaQ7UduKdH+fdG0KTYEFf1CmLrEWyfS/bj1mbdPH7m343AcMfI00VeaY00aeSssVcaVEBgLAYYwbg3LUMR/y3iQw0wlXr04zpTu6wYQcrV8TinlMQWLvpfVKaa+sehkoRMYWMe+d2t998K5z2KZkMDDDHmnoAKj2Mh6hTNT4cT9nDnKFNQoZKOzsYixBSWyy9jeo5RgSZ3J5biGgTaHtnj+yQ6nLKojXsXGXCE4M9eNwZ30WFf8Y5OnXXewuN4mjnjv/NHbw/rRuyXzt8LZzplvdmU6CA4ZBcKknFMUUtIlpXPieTW54NP36TCfEJ49ZpubSq/u6i1lU4HnSaxDsqx1NabQjggESgRSZrmWLktWH+2t1HFMiwR7lxLU2AmBL/UXGZOV4JNf765qS8AE9J6nqkxq7tXm/lpz5Lyq5Vu27mqnpGFpGFxOsJnrBoNKR9oGvxUCN2qkgADzATh3zsFwQVCQQzTQBvBBdrMHaEQo54C1tZidbbdPgs+vWI4yDZxhYBE3BVH8r3MVaNPDGIP0f/pLnb/tGPo+zBczdVkLhjblL2j/jYMCFlyierio67JEXhjeQ8KWeUVvmpUjtp37g7IDyHHduU3NJi9McbFQ1NqnsSOJwvh+qhJvxn2yhdEwsjT6PTwGdAhF4H2MDFVVH3zh8r1ptCZtglRZIwyYX09jJSqLumxov38m3ijVa6GBQs5t6s2ByeBHDSaCWkJe65PwUSv4Ir53tzQku5FmoTx4cKYmlHxIgaB17WXWwcpoVA9oRFZhMViUAMKopENHhasZTJecPSYdNEo0CnNOJ1lNtuWnsaH0DMU9thAgvbvyqV9DKBXmoeL6Kyq/KH/A9sLQutUYRICUGISwbGZz0PjGuCoVkxAMjEt21KjLGdJLmB3ccEMoDRxmKXS0KNPVbGI+UJCVGtK1pa+UnaR46o0X1ofyrG5p0p/0mBRUmXROWUeVWgjJ97Uc2aO4CjXYOF+w7Oxxt7vb1fSXA9b2THlhR4RlXHOSB5YNul1bw79IbiIrB27BqS5RXBPHR+nnaqxFk4OLlHL0O2cKNi1+NytYpHq8BrWrQUIKq0jDJOW8n0Sd3G63zsoVqLkQbFrA/eRXqdFPTUhOIb5ko5UxxmBnpXrvgrE0jtddfThQr1Zdasog1BAPhNxpPbuMsoTCUZP9f+78KjMLSAclx+/+J4PR0KM5bhR6P3ghOYCg92zCfG1qZ0XfE7Tv5vq7ZEj/AfElMbuC2p67b8bc/u4jNLK+DljubICxyrK+ixNehZfqd40i+VN5o93z89YtCwSQbjKY3K9wC+2A8w1tj6teocU9ghqIzp4ijoSi8qoH/Oww2+H+XaGNWXU0f3IziiHLRU0cRohi+9b/cHS6g0IaBPyAuNdzgzVnuhEUaYdzIitIrULsHItk+PzoSBkMy2+zPULQFrcLPW7QtK9wN+lbXQHCDBYzfs6TSTeibJRGEM26V79o7X7A1sVFe4g1sNq30G2RglX7CwcfJtaoTgLza0/yP36shN8jH5hq3KROF4qib7a4df1cOTmMsKSadr9xksS1rXqBVviFSvktVBZoLKP1yvAFB2hGyde4NmZXMcFbhqRdfaVZE9aTHPB5+cHoak8Uv79h80ZIbN5V3ylEsGErr7B5ncfTv+aNuzrpBVBhXO3MfvN+opZu0W/sbnWuHJq62kFR4KDHOwnfIBcSOz/YGqhPOm7r69AAQEw5XnWZqnB4tjua3H2RVMo8Yo0lwDn+1EfuoBq+6JAmFrTFbfI3G4q921DjjcieI88TrLKN2Ysmqs9Fa3aHbPYjb0PQVil9BZUFJqudA1xCj4EmjuQgurOGCFBBeyywEBhVbWKdIesN2FY086QEh1hnD83MHt3pcfwyUCY82RRttAkyocEKwsv03mU3+j548LGWihDY01RKPbBYEnqCVJrW6CkbqsWICrebeaLqfrR5ffIthnnb2BrmBpWDOoerH9Hk2mV6ITz4QswEoq0NIiAIcbRtkwQW9k2GCMZRYkw4EUjjrv9HxZ7XqyxlItg3Hi+CvBKYqewWGsynwIUNAuS8VrV0KfN9GhF3Y7QIKhXBtYJue+CaNHHhAW2pzHiZTzqY513+jHUop6WEFON8M/fbLjfy0zoj74/5a/vC91z1ZtDeGVtUvT1MjC/vMdmuYk/pbv278s4ZPIUkiUFk46HrGN4RZ9tN11cWgMZobEDZmJ8q61Kfv3xDg9hdf3hoLC4SHW4UIliLJOLnkoRfKlJ/CiT5psMO2AoWwstXLXiZdkKya7dKQ11CiNuo1nlTmrMS6wzUBTPjELJIu9iUs3Jvo8fz9ZvkxKb9H0ZOijfDk7VQsgFDViZuyE+VZz0HKrCmLulJVWFIBMz+D42sNmMFfed5lkAXWF5c5YOlSfM8W/OkQdFFBv8zYtFfHkmyIwVhNYhrxsd2NtCJjjGov1uES1nGbp+1CPVCywqpD90/HIBp3QjnUfgxYM0LIby7VUQ1FwTVPguWP2BW4DIs6aVx0EBwnAiQ7bUadlv3VxK2Jl1H6fqIuQt4fLh6dipuVl6uyXGVlMFqKnX/nzn90WFnig7t8OhIMbIMUiXDWCwcxr2D/WtwiK5stSB+e0FmTS6zmPL7gVZCDepkIwqUQA2mCwgkMELDqmwxytaCWrGdNlONHC+woFo28jPzyC6YFrWHPlCnTEiJeBp29cMXMQigxxtBrOXuXikQDOX3lJ6oGDFWuR2CK2EMwn/pF1NpSDUmO5bAG5975gP9lUMlVHnC5S6V8907RozcGDKZbPz2olK2vxfF9OReaif8J+QB/gi2OyUpyTF0rGuoEqdk65liLcqbwPItnsyRXdv4HAIQUF/PqGjgJE/fJJfUMSvLc/u1pmTUb4T4pxhm0tK1GNkwIKE6uw+XU/ozZbNvAdqI9Zbjz4IfDzxxdczf0Ekzgh0NTGgQmmSKojWY9Diklxmh8Py3fJg3oTnB1iK5F/dlEb4bbqcareax2rc2tFQ3Gh34ecCb74Jma056HVm02wt8xL00OZ9YYS8vuSz5h8g44s9zzhCyQzroCkrW6lsIULyy1lgc4j8C7LWw24H6Hh4Tx29h95PDJuWjbxSgpk/KLFLluJBoOibJ5YDl2XvSODMkOGhtdZ2Fppooh8ZTlR0BuOnrLl5mPYpU1J+hb0RkF4uJkduO3/+LSoHgzLxUvx6ry8AGJWTksNo8b/jVlQBVdvk1Lf4NbMC4GMGBk+hzqOFFYLJyp6PJQFIzt/BJB5dJRHVaIffVAwKsXkaGCG6DAl54/951BXb3FIGagpPuuFxFKvTCCIs+t+W3Wbal10ZWzkozjF9/4NJ26qiE9v/mHdSA2myW7ZqKuXnQFQgVklyS7Kjxh5u3KNbztt4RnwYWlbwHuf5a4ndf7gfNBjjv5t1yQJLINp2bk2uDNwP/WqYAL59eh1zXkH8d+qfR+wJe+MkzSZkpRtbQyEtNxY4Ct/GL1JLkWdPrlpXsqxQuv/cTbQ7wnUcT3hnajrHzr5krJKe6Wmz3gWNwsB/M0Uc21UUS1SgZxWw9V/RWtes4K29yxyESv8mgYRdGi2UiKgKaCOmghJU8VEO//Jl7JFYiY8l+mmaXO4dB5APvBYgaimKOdrzjwnZpT57Yy0GGDfum/Ka8ErWuUqdj5tR0rOyAhenMvmsy+porR1nX3ZqV6nWfmPD83UjzrH/OJy7+HxkgiP79QpJk4Xsep8fly29tZ/XoqesKRMfCCr5WePvfup6WsJsK2aAlMsnS33/K3ZyToGz50IHdU5VC0fKMA5pXO3uYyyOzqOA3LunSMBSh6a8Vi3JfXYHNzukZ3zb0xkTMqjNHjd8FOg3U7Obp2uhODOv1BOM+1N8KFHbMgJIUrJ5gBWkQbVGuXJ+NWRRAy1BEUFuY36c+D6UC4t9ZrwrTeJAiww+CMxHHD7lKP67Wdhp3y6+JfTlriT4iCKl5L9L4gUjsVfxlQZkEuPy7en+SePyWnNyTadeEGR/oxrEGDUsJXZgBNPyI86wK0SH0grr4klFVOWq3yBcAEKuNIkG5Rckn4liOb2ilfD9RztEKAD0e6SxkMjze7SMr5ROUXDq/m+Cvx7fHda/+q6T7yHOjIpuML7kn92AQ7zcpmh8WTkam/aYbBQBdXvG3Pr/PfJ8nFQ8PsbPO0qBq212rFGuziKDQKZvLDwKZYhq9uHyV3e2da8aCR3x1MNWJH9oX9ayqxODGvhlWjlTOUM4ZoYy3pbywEFxAPp7w3ClXZLAFXsTtsBMR7NMcrvJkZHGGhjXAE7E8EaGzU6BiqsuidxdVbv+WoxiDa8j/iA70ilVoL7+V/Y0B4rr2NMryqODhLNwxxi/lvmureZJf6H7Dp52frdAYGRLut7DQH1OrXC/gJMJX2WvVq0Qdy8VLfJw/z6e9+hHgVrhkDNp7Xq+KR1LSIYnh6AnvPCkKIm7O2Jh4+T1NiddviS2BVpBI6rUEyqhjS5kl5gw06Vk+ARSEbpkhxFb5AMPbdXAVPhVTWKNZwcSAF2s9tpjPqh0BvRq0GWCFJ7jZOdFzGRptLWm6Y8NXhRnxNQv74vYODiW4gPIolf3putccmXgDQ7xc0pB7bFLWe+cYPO+2oS5FZn3ymurJgrVY1iRRcjRABOGbzvvRJVxnhgt/J4dV4KFBDU0n8j0c03tWPl2d1cNzlJ+fHY6q8d2QVVihqsVm2D44v1DeOWpdrfxVbSp8uXL9QkRs1D+4H+OdqSmVjCnWpJ/X5U+1Gbcjd2BDjJKSmsefXy1ea6qtngK+Eq4SgrQKp4VJGuXwtc5HlIoroxgTcvddyS45MYR+8rJQlaLx+qSPSxHeLYdKZaPWa2/tDi4LwmoZIqkotrP70Dkg7Jb3rz7Sb2H4CtKwIsqpFIfMPM5IA50wqIwBG+zcjEXPMDH6kn7ZvHeE3zUrzqRei845F9aNQWlkvmChBa9wrBfSFMZX0xQy/UpndLo3APEY9na/LIpvqjRP1psWttSEMrMuNN8U8LuQ==";
        private const string GenericaBase64Sample2 = "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";
        private const string GenericaBase64Sample3 = "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";

        [Fact]
        public void TokenTest()
        {
            TransformerToken token1 = TransformerToken.Parse("Generica:secret:128");
            TransformerToken token2 = TransformerToken.Parse("generica:secret:128:salt");
            TransformerToken token3 = TransformerToken.Parse("generica:secret:128:salt-64");
            TransformerToken token4 = TransformerToken.Parse("generica:secret:128-64");
            TransformerToken token5 = TransformerToken.Parse("test:secret:128");

            Assert.Throws<ArgumentException>(() => TransformerToken.Parse("generica:secret:128:salt:64"));
            Assert.Throws<ArgumentException>(() => TransformerToken.Parse("generica:secret"));
            Assert.Throws<ArgumentException>(() => TransformerToken.Parse("generica"));

            Assert.Equal("generica", token1.TransformerName);
            Assert.Equal(Encoding.UTF8.GetBytes("secret"), token1.SecretKey);
            Assert.Equal(128, token1.KeySize);
            Assert.Null(token1.Salt);
            Assert.Equal(0, token1.Rotate);

            Assert.Equal("generica", token2.TransformerName);
            Assert.Equal(Encoding.UTF8.GetBytes("secret"), token2.SecretKey);
            Assert.Equal(128, token2.KeySize);
            Assert.Equal(Encoding.UTF8.GetBytes("salt"), token2.Salt);
            Assert.Equal(0, token2.Rotate);

            Assert.Equal("generica", token3.TransformerName);
            Assert.Equal(Encoding.UTF8.GetBytes("secret"), token3.SecretKey);
            Assert.Equal(128, token3.KeySize);
            Assert.Equal(Encoding.UTF8.GetBytes("salt"), token3.Salt);
            Assert.Equal(64, token3.Rotate);

            Assert.Equal("generica", token4.TransformerName);
            Assert.Equal(Encoding.UTF8.GetBytes("secret"), token4.SecretKey);
            Assert.Equal(128, token4.KeySize);
            Assert.Null(token4.Salt);
            Assert.Equal(64, token4.Rotate);

            Assert.Throws<ArgumentException>(() => Transformers.GetTranformer(token5));
        }

        [Fact]
        public void GenericaDeterministicTest()
        {
            byte[] data = Convert.FromBase64String(GenericaBase64Sample);

            TransformerToken token = new()
            {
                TransformerName = "generica",
                SecretKey = Encoding.UTF8.GetBytes("test"),
                KeySize = 128,
                Salt = Encoding.UTF8.GetBytes("salt"),
                Rotate = 64,
            };

            ITranformer tranformer = Transformers.GetTranformer(token);
            byte[] buffer = new byte[token.KeySize * 32];
            tranformer.Generate(buffer, token.Rotate);
            Assert.Equal(data, buffer);

            byte[] data2 = Convert.FromBase64String(GenericaBase64Sample2);

            TransformerToken token2 = new()
            {
                TransformerName = "generica",
                SecretKey = Encoding.UTF8.GetBytes("test"),
                KeySize = 128,
            };

            ITranformer tranformer2 = Transformers.GetTranformer(token2);
            byte[] buffer2 = new byte[token2.KeySize * 32];
            tranformer2.Generate(buffer2, token2.Rotate);
            Assert.Equal(data2, buffer2);

            byte[] data3 = Convert.FromBase64String(GenericaBase64Sample3);

            TransformerToken token3 = new()
            {
                TransformerName = "generica",
                SecretKey = Encoding.UTF8.GetBytes("test"),
                KeySize = 128,
                Rotate = 64,
            };

            ITranformer tranformer3 = Transformers.GetTranformer(token3);
            byte[] buffer3 = new byte[token3.KeySize * 32];
            tranformer3.Generate(buffer3, token3.Rotate);
            Assert.Equal(data3, buffer3);
        }

        [Fact]
        public void RotateDeterministicTest()
        {
            int[] array = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var emptyCollection = Enumerable.Empty<int>();

            int[] result1 = (int[])array.Clone();
            int[] result2 = (int[])array.Clone();
            int[] result3 = (int[])array.Clone();
            int[] result4 = emptyCollection.ToArray();

            int[] expect1 = { 8, 9, 1, 2, 3, 4, 5, 6, 7 };
            int[] expect2 = { 3, 4, 5, 6, 7, 8, 9, 1, 2 };

            Transformers.Rotate(result1, 2);
            Transformers.Rotate(result2, -2);
            Transformers.Rotate(result3, 0);
            Transformers.Rotate(result4, 3);

            Assert.Equal(expect1, result1);
            Assert.Equal(expect2, result2);
            Assert.Equal(array, result3);

            Assert.Empty(result4);
        }

        [Fact]
        public void PickDeterministicTest()
        {
            var collection = new List<int> { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            int[] expect = { 50, 70, 20 };

            var result1 = Transformers.Pick(collection, 3, seed: 123);
            var result2 = Transformers.Pick(collection, 3, seed: 124);

            Assert.NotEqual(collection, result1); // Shuffled
            Assert.Equal(expect, result1);
            Assert.NotEqual(expect, result2);
            Assert.Equal(3, result1.Count());
        }

        [Fact]
        public void MixDeterministicTest()
        {
            var collection1 = new List<string> { "A", "B", "C" };
            var collection2 = new List<string> { "X", "Y", "Z" };

            string[] expect = { "Z", "Y", "B", "X", "C", "A" };

            var result1 = Transformers.Mix(seed: 456, collection1, collection2);
            var result2 = Transformers.Mix(seed: 457, collection1, collection2);

            Assert.NotEqual(collection1.Concat(collection2), result1); // Shuffled
            Assert.Equal(expect, result1);
            Assert.NotEqual(expect, result2);
            Assert.Equal(6, result1.Length);
        }

        [Fact]
        public void BitConverterDeterministicTest()
        {
            byte[] array = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

            int expect1 = 0xC010E04;
            long expect2 = 0x8070D0B0C010E04;

            int result1 = Transformers.ToInt32(array, 1212);
            int result2 = Transformers.ToInt32(array, 1213);
            long result3 = Transformers.ToInt64(array, 1212);
            long result4 = Transformers.ToInt64(array, 1213);

            Assert.NotEqual(result1, result3);
            Assert.Equal(expect1, result1);
            Assert.NotEqual(expect1, result2);
            Assert.Equal(expect2, result3);
            Assert.NotEqual(expect2, result4);
        }
    }
}
