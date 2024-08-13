using System;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Generators;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreGenerateSessionHost : CliSessionHost
    {
        private KeyStoreGenerator Generator;
        private int Size;
        private TransformerToken Token;
        private int Margin;
        private string Output;

        public KeyStoreGenerateSessionHost(GlobalOptions globalOptions, KeyStoreGenerator generator, int size, TransformerToken token, int magin, string output) : base(globalOptions)
        {
            Generator = generator;
            Size = size > 0 ? size : KeyStore.GetRandomOddNumber();
            Token = token;
            Margin = magin;
            Output = output;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStoreLoadSession ksLoader;

            if (Token.IsValid())
            {
                ITranformer tranformer = Transformers.GetTranformer(Token);
                string kSize = Margin > 0 ? $"{Token.KeySize}+{Margin}" : $"{Token.KeySize}";
                Log($"Generating keystore with {kSize} keys using {tranformer.GetType().Name}");
                ksLoader = new KeyStoreTokenLoadSession(true, Token, Margin);
            }
            else
            {
                if (!Quiet && Generator == KeyStoreGenerator.EntroX)
                {
                    CollectEntropy();
                }

                string kSize = Margin > 0 ? $"{Size}+{Margin}" : $"{Size}";
                Log($"Generating keystore with {kSize} keys using {Generator}");
                ksLoader = new KeyStoreRandomLoadSession(true, Generator, Size, Margin);
            }

            var fileWriter = new FileSaveSession(Output, null);
            ksLoader.ContinueWith(fileWriter);

            NewSession(ksLoader);
            NewSession(fileWriter);

            ShowProgressMonitored(true).Wait();
        }

        public void CollectEntropy()
        {
            var crng = new CryptoRandom();
            List<(DateTime start, DateTime end, string data)> entropy = new List<(DateTime start, DateTime end, string data)>();

            Console.WriteLine();
            Console.WriteLine("Collecting entropy");

            Console.WriteLine("Please enter some random characters. more character and more randomness improves the security of your keystore.");
            var initTime = DateTime.Now;
            var initEnt = Console.ReadLine();
            var initEndTime = DateTime.Now;
            entropy.Add((initTime, initEndTime, initEnt));

            int i = 0;
            int count = crng.Next(1, 4);
            while (i < count)
            {
                Console.Write("Please enter more character: ");
                var sTime = DateTime.Now;
                var data = Console.ReadLine();
                var eTime = DateTime.Now;
                entropy.Add((sTime, eTime, data));

                i++;
            }

            int target = crng.Next(10, 40);
            while (target > 0)
            {
                var shuffledChars = entropy.SelectMany(x => x.data)
                                           .OrderBy(x => crng.Next())
                                           .SelectMany(x => BitConverter.GetBytes(x))
                                           .OrderBy(x => crng.Next())
                                           .ToArray();

                var shuffledNumbers = entropy.SelectMany(x => BitConverter.GetBytes((x.end - x.start).Ticks))
                                           .OrderBy(x => crng.Next())
                                           .ToArray();

                var shuffledArray = shuffledChars.Concat(shuffledNumbers)
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