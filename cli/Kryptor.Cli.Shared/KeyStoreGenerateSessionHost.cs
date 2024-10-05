using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreGenerateSessionHost : CliSessionHost
    {
        private readonly KeyStoreGenerator Generator;
        private readonly int Size;
        private TransformerToken Token;
        private readonly int Margin;
        private readonly string Output;

        public KeyStoreGenerateSessionHost(GlobalOptions globalOptions, KeyStoreGenerator generator, int size, TransformerToken token, int margin, string output) : base(globalOptions)
        {
            Generator = generator;
            Size = size > 0 ? size : KeyStore.GetRandomOddNumber();
            Token = token;
            Margin = margin;
            Output = output;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStoreLoadSession ksLoader;

            if (Token.IsValid())
            {
                ITransformer tranformer = Transformers.GetTransformer(Token);
                string kSize = Margin > 0 ? $"{Token.KeySize.FormatWithCommas()}+{Margin.FormatWithCommas()}" : $"{Token.KeySize.FormatWithCommas()}";
                Log($"Generating keystore with {kSize} keys using {tranformer.GetType().Name}");
                ksLoader = new KeyStoreTokenLoadSession(true, Token, Margin);
            }
            else
            {
                if (!NoInteractions && Generator == KeyStoreGenerator.EntroX)
                {
                    CollectEntropy();
                }

                string kSize = Margin > 0 ? $"{Size.FormatWithCommas()}+{Margin.FormatWithCommas()}" : $"{Size.FormatWithCommas()}";
                Log($"Generating keystore with {kSize} keys using {Generator}");
                ksLoader = new KeyStoreRandomLoadSession(true, Generator, Size, Margin);
            }

            FileSaveSession fileWriter = new FileSaveSession(Output, null);
            ksLoader.ContinueWith(fileWriter);

            NewSession(ksLoader);
            NewSession(fileWriter);

            ShowProgressMonitored(true).Wait();
        }

        public static void CollectEntropy()
        {
            CryptoRandom crng = new CryptoRandom();
            List<(DateTime start, DateTime end, string data)> entropy = new List<(DateTime start, DateTime end, string data)>();

            Console.WriteLine();
            Console.WriteLine("Collecting entropy");

            Console.WriteLine("Please enter some random characters. more character and more randomness improves the security of your keystore.");
            DateTime initTime = DateTime.Now;
            string initEnt = Console.ReadLine();
            DateTime initEndTime = DateTime.Now;
            entropy.Add((initTime, initEndTime, initEnt));

            int i = 0;
            int count = crng.Next(1, 4);
            while (i < count)
            {
                Console.Write("Please enter more character: ");
                DateTime sTime = DateTime.Now;
                string data = Console.ReadLine();
                DateTime eTime = DateTime.Now;
                entropy.Add((sTime, eTime, data));

                i++;
            }

            int target = crng.Next(10, 40);
            while (target > 0)
            {
                byte[] shuffledChars = entropy.SelectMany(x => x.data)
                                           .OrderBy(x => crng.Next())
                                           .SelectMany(BitConverter.GetBytes)
                                           .OrderBy(x => crng.Next())
                                           .ToArray();

                byte[] shuffledNumbers = entropy.SelectMany(x => BitConverter.GetBytes((x.end - x.start).Ticks))
                                           .OrderBy(x => crng.Next())
                                           .ToArray();

                byte[] shuffledArray = shuffledChars.Concat(shuffledNumbers)
                                                 .Shuffle(crng)
                                                 .ToArray();

                EntroX.AddEntropy(shuffledArray.Take(crng.Next(Math.Min(1, shuffledArray.Length)))
                                                    .ToArray());

                target--;
            }

            Console.WriteLine();
        }
    }
}