using CommandLine;
using Signature.Core;

namespace Nett.Signer
{
    public sealed class Options
    {
        [Option('i', "input", DefaultValue = "", HelpText = "Input NuGet package", Required = true)]
        public string Input { get; set; }

        [Option('o', "output", DefaultValue = "", HelpText = "Output NuGet package name", Required = true)]
        public string Output { get; set; }

        [Option('p', "spid", DefaultValue = "", HelpText = "Strong Named package ID", Required = true)]
        public string StrongNamedPackageId { get; set; }

        [Option('k', "keyfile", DefaultValue = "", HelpText = "Key file (snk/pfx)", Required = true)]
        public string KeyFile { get; set; }
    }

    public sealed class Program
    {
        static void Main(string[] args)
        {
            var o = CommandLine.Parser.Default.ParseArguments<Options>(args).Value;
            var signer = new PackageSigner();

            signer.SignPackage(o.Input, o.Output, o.KeyFile, "", o.StrongNamedPackageId);
        }
    }
}
