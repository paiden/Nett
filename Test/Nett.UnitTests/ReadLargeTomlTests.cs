using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Nett.UnitTests
{
    [ExcludeFromCodeCoverage]
    public class ReadLargeTomlTests
    {
        [Fact]
        public void ReadLargeToml01()
        {
            string toml = @"
## Test Script

TestExecutionSource = ""$(TestExecutionSource)""
TestSuiteName = ""$(TestSuiteName)""
LocalTestRunBaseDir = ""$(LocalTestRunBaseDir)""

[Runtime]
RuntimeType = ""Resource""

[[Resources]]
Type = ""Mandatory""
SourceDirectory = ""$(InstallDir)\bin""
TargetDirectory = ""runtime""
Files = [""A.dll"", ""B.dll"", ""C""]

[[Resources]]
Type = ""Mandatory""
SourceDirectory = ""$(BinDir)""
TargetDirectory = "".""
Files = [""$(TS).exe"", ""$(TS).e1"", ""$(TestSuiteName).e2""]

[[Resources]]
Type = ""Optional""
SourceDirectory = ""$(BinDir)""
TargetDirectory = "".""
Files = [""$(TS).exe.config"", ""*.xls"", "" *.xlsx"", "" *.xlsx""]

[Publish]
Type = ""NetShare""
ShareLocation = ""\\server001\\TestData\\share""
PublishLocation = ""\\server001\\TestData\\results""

";

            var read = Toml.ReadString<Root>(toml);

            Assert.NotNull(read);
            Assert.NotNull(read.Resources);
            Assert.Equal(3, read.Resources.Count);
        }

        private class Root
        {
            public Runtime Runtime { get; set; }
            public List<Resource> Resources { get; set; }
        }

        private class Runtime
        {
            public string RuntimeType { get; set; }
        }

        private class Publish
        {
            public string Type { get; set; }
            public string ShareLocation { get; set; }
            public string PublishLocation { get; set; }
        }

        private class Resource
        {
            public string Type { get; set; }
            public string SourceDirectory { get; set; }
            public string TargetDirectory { get; set; }
            public List<string> Files { get; set; }
        }
    }
}
