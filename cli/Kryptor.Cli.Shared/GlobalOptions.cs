using System.CommandLine;
using System.CommandLine.Binding;

namespace SAPTeam.Kryptor.Cli
{
    public class GlobalOptions
    {
        public bool Verbose { get; set; }
        public bool Quiet { get; set; }
        public bool NoColor { get; set; }
    }

    public class GlobalOptionsBinder : BinderBase<GlobalOptions>
    {
        private readonly Option<bool> verbose;
        private readonly Option<bool> quiet;
        private readonly Option<bool> noColor;

        public GlobalOptionsBinder(Option<bool> verbose, Option<bool> quiet, Option<bool> noColor)
        {
            this.verbose = verbose;
            this.quiet = quiet;
            this.noColor = noColor;
        }

        protected override GlobalOptions GetBoundValue(BindingContext bindingContext)
        {
            return new GlobalOptions()
            {
                Verbose = bindingContext.ParseResult.GetValueForOption(verbose),
                Quiet = bindingContext.ParseResult.GetValueForOption(quiet),
                NoColor = bindingContext.ParseResult.GetValueForOption(noColor),
            };
        }
    }
}