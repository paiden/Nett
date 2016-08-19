using System;
using System.IO;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.TestData
{
    /// <summary>
    /// Simulate a git config system with different configuration scopes
    /// </summary>
    public sealed class GitScenario : IDisposable
    {
        public const string SystemDefaultContent = @"
[Core]
Symlinks = false
AutoClrf = true

[Core.Help]
Format = ""Web""";

        public const string UserDefaultContent = @"
[User]
Name = ""Test User""
EMail = ""test@user.com""";

        public const string RepoDefaultContent = @"
[Core]
Bare = false
IgnoreCase = true";

        private static readonly string MergedDefaultContent = RepoDefaultContent + Environment.NewLine
            + SystemDefaultContent.Replace("[Core]", "")
            + UserDefaultContent + Environment.NewLine;


        public static readonly GitConfig MergedDefault = Toml.ReadString<GitConfig>(MergedDefaultContent);

        public TestFileName SystemFile { get; }
        public TestFileName UserFile { get; }
        public TestFileName RepoFile { get; }

        private GitScenario(string testName)
        {
            this.SystemFile = TestFileName.Create(testName, "system", GitConfig.Extension);
            this.UserFile = TestFileName.Create(testName, "user", GitConfig.Extension);
            this.RepoFile = TestFileName.Create(testName, "repo", GitConfig.Extension);
        }

        public ComaConfig<GitConfig> CreateMergedFromDefaults() =>
           ComaConfig.CreateMerged(() => new GitConfig(), this.SystemFile, this.UserFile, this.RepoFile);

        public static GitScenario Setup(string testName)
        {
            var scenario = new GitScenario(testName);

            File.WriteAllText(scenario.SystemFile, SystemDefaultContent);
            File.WriteAllText(scenario.UserFile, UserDefaultContent);
            File.WriteAllText(scenario.RepoFile, RepoDefaultContent);

            return scenario;
        }

        public void Dispose()
        {
            this.SystemFile.Dispose();
            this.UserFile.Dispose();
            this.RepoFile.Dispose();
        }

        public sealed class GitConfig
        {
            public const string Extension = ".config";

            public CoreConfig Core { get; set; } = new CoreConfig();
            public UserConfig User { get; set; } = new UserConfig();

            public override bool Equals(object obj)
            {
                var g = (GitConfig)obj;
                return this.Core.Equals(g.Core) &&
                    this.User.Equals(g.User);
            }

            public override int GetHashCode() => base.GetHashCode();

            public sealed class CoreConfig
            {
                public bool Symlinks { get; set; } = false;
                public bool AutoClrf { get; set; } = true;
                public HelpConfig Help { get; set; } = new HelpConfig();
                public bool Bare { get; set; } = false;
                public bool IgnoreCase { get; set; } = false;

                public override bool Equals(object obj)
                {
                    CoreConfig cc = (CoreConfig)obj;

                    return this.Symlinks == cc.Symlinks
                        && this.AutoClrf == cc.AutoClrf
                        && this.Help.Equals(cc.Help)
                        && this.Bare == cc.Bare
                        && this.IgnoreCase == cc.IgnoreCase;
                }

                public override int GetHashCode() => base.GetHashCode();
            }

            public sealed class HelpConfig
            {
                public HelpFormat Format { get; set; } = HelpFormat.Man;

                public enum HelpFormat
                {
                    Man,
                    Info,
                    Web,
                }

                public override bool Equals(object obj)
                {
                    var hc = (HelpConfig)obj;
                    return this.Format == hc.Format;
                }

                public override int GetHashCode() => base.GetHashCode();
            }

            public sealed class UserConfig
            {
                public string Name { get; set; } = null;
                public string EMail { get; set; } = null;

                public override bool Equals(object obj)
                {
                    var uc = (UserConfig)obj;

                    return this.Name == uc.Name
                        && this.EMail == uc.EMail;
                }

                public override int GetHashCode() => base.GetHashCode();
            }
        }
    }
}
