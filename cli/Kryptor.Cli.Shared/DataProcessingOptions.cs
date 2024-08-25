using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;

namespace SAPTeam.Kryptor.Cli
{
    public class DataProcessingOptions
    {
        public int BlockSize { get; set; }
        public string Provider { get; set; }
        public bool Continuous { get; set; }
        public bool RemoveHash { get; set; }
        public bool DynamicBlockProcessing { get; set; }
        public string KeyStore { get; set; }
        public string[] Files { get; set; }
    }

    public class DataProcessingOptionsBinder : BinderBase<DataProcessingOptions>
    {
        private readonly Option<int> blockSize;
        private readonly Option<string> provider;
        private readonly Option<bool> continuous;
        private readonly Option<bool> removeHash;
        private readonly Option<bool> dbp;
        private readonly Option<string> keyStore;
        private readonly Argument<IEnumerable<string>> files;

        public DataProcessingOptionsBinder(Option<int> blockSize, Option<string> provider, Option<bool> continuous, Option<bool> removeHash, Option<bool> dbp, Option<string> keyStore, Argument<IEnumerable<string>> files)
        {
            this.blockSize = blockSize;
            this.provider = provider;
            this.continuous = continuous;
            this.removeHash = removeHash;
            this.dbp = dbp;
            this.keyStore = keyStore;
            this.files = files;
        }

        protected override DataProcessingOptions GetBoundValue(BindingContext bindingContext)
        {
            return new DataProcessingOptions()
            {
                BlockSize = bindingContext.ParseResult.GetValueForOption(blockSize),
                Provider = bindingContext.ParseResult.GetValueForOption(provider),
                Continuous = bindingContext.ParseResult.GetValueForOption(continuous),
                RemoveHash = bindingContext.ParseResult.GetValueForOption(removeHash),
                DynamicBlockProcessing = bindingContext.ParseResult.GetValueForOption(dbp),
                KeyStore = bindingContext.ParseResult.GetValueForOption(keyStore),
                Files = bindingContext.ParseResult.GetValueForArgument(files).ToArray(),
            };
        }
    }
}