using System.CommandLine;
using System.CommandLine.Binding;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingOptions
    {
        public int BlockSize { get; set; }
        public string Provider { get; set; }
        public string[] Parameters { get; set; }
        public bool Continuous { get; set; }
        public bool RemoveHash { get; set; }
        public bool DynamicBlockProcessing { get; set; }
        public string OutputPath { get; set; }
        public string KeyStore { get; set; }
        public string[] Files { get; set; }
    }

    public class DataProcessingOptionsBinder : BinderBase<DataProcessingOptions>
    {
        private readonly Option<int> blockSize;
        private readonly Option<string> provider;
        private readonly Option<IEnumerable<string>> parameters;
        private readonly Option<bool> continuous;
        private readonly Option<bool> removeHash;
        private readonly Option<bool> dbp;
        private readonly Option<string> outputPath;
        private readonly Option<string> keyStore;
        private readonly Argument<IEnumerable<string>> files;

        public DataProcessingOptionsBinder(Option<int> blockSize, Option<string> provider, Option<IEnumerable<string>> parameters, Option<bool> continuous, Option<bool> removeHash, Option<bool> dbp, Option<string> outputPath, Option<string> keyStore, Argument<IEnumerable<string>> files)
        {
            this.blockSize = blockSize;
            this.provider = provider;
            this.parameters = parameters;
            this.continuous = continuous;
            this.removeHash = removeHash;
            this.dbp = dbp;
            this.outputPath = outputPath;
            this.keyStore = keyStore;
            this.files = files;
        }

        protected override DataProcessingOptions GetBoundValue(BindingContext bindingContext)
        {
            return new DataProcessingOptions()
            {
                BlockSize = bindingContext.ParseResult.GetValueForOption(blockSize),
                Provider = bindingContext.ParseResult.GetValueForOption(provider),
                Parameters = bindingContext.ParseResult.GetValueForOption(parameters).ToArray(),
                Continuous = bindingContext.ParseResult.GetValueForOption(continuous),
                RemoveHash = bindingContext.ParseResult.GetValueForOption(removeHash),
                DynamicBlockProcessing = bindingContext.ParseResult.GetValueForOption(dbp),
                OutputPath = bindingContext.ParseResult.GetValueForOption(outputPath),
                KeyStore = bindingContext.ParseResult.GetValueForOption(keyStore),
                Files = bindingContext.ParseResult.GetValueForArgument(files).ToArray(),
            };
        }
    }
}