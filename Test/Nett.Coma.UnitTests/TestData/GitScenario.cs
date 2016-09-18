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
        public const string RepoDefaultContent = @"
[Core]
Bare = false
IgnoreCase = true";

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

        public static readonly GitConfig MergedDefault;
        private static readonly string MergedDefaultContent;

        static GitScenario()
        {
            MergedDefaultContent = RepoDefaultContent + Environment.NewLine
                    + SystemDefaultContent.Replace("[Core]", "")
            + UserDefaultContent + Environment.NewLine;

            MergedDefault = Toml.ReadString<GitConfig>(MergedDefaultContent);
        }

        private GitScenario(string testName)
        {
            this.SystemFile = TestFileName.Create(testName, "system", GitConfig.Extension);
            this.UserFile = TestFileName.Create(testName, "user", GitConfig.Extension);
            this.RepoFile = TestFileName.Create(testName, "repo", GitConfig.Extension);

            this.SystemFileSource = ConfigSource.CreateFileSource(this.SystemFile, SystemAlias);
            this.UserFileSource = ConfigSource.CreateFileSource(this.UserFile, UserAlias);
            this.RepoFileSource = ConfigSource.CreateFileSource(this.RepoFile, RepoAlias);
        }

        public string RepoAlias => "RepoAlias";
        public TestFileName RepoFile { get; }
        public IConfigSource RepoFileSource { get; private set; }
        public string SystemAlias => "System";
        public TestFileName SystemFile { get; }
        public IConfigSource SystemFileSource { get; private set; }
        public string UserAlias => "User";
        public TestFileName UserFile { get; }
        public IConfigSource UserFileSource { get; private set; }

        public static GitScenario Setup(string testName)
        {
            var scenario = new GitScenario(testName);

            File.WriteAllText(scenario.SystemFile, SystemDefaultContent);
            File.WriteAllText(scenario.UserFile, UserDefaultContent);
            File.WriteAllText(scenario.RepoFile, RepoDefaultContent);

            return scenario;
        }

        public Config<GitConfig> CreateMergedFromDefaults()
        {
            var source = ConfigSource.Merged(this.SystemFileSource, this.UserFileSource, this.RepoFileSource);
            return Config.Create(() => new GitConfig(), source);
        }

        // Default System settings stored in all scopes
        public GitScenario UseSystemDefaultContentForeachSource()
        {
            File.WriteAllText(this.SystemFile, SystemDefaultContent);
            File.WriteAllText(this.UserFile, SystemDefaultContent);
            File.WriteAllText(this.RepoFile, SystemDefaultContent);

            return this;
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
                public bool AutoClrf { get; set; } = true;
                public bool Bare { get; set; } = false;
                public HelpConfig Help { get; set; } = new HelpConfig();
                public bool IgnoreCase { get; set; } = false;
                public bool Symlinks { get; set; } = false;

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
                public enum HelpFormat
                {
                    Man,
                    Info,
                    Web,
                }

                public HelpFormat Format { get; set; } = HelpFormat.Man;

                public override bool Equals(object obj)
                {
                    var hc = (HelpConfig)obj;
                    return this.Format == hc.Format;
                }

                public override int GetHashCode() => base.GetHashCode();
            }

            public sealed class UserConfig
            {
                public string EMail { get; set; } = null;
                public string Name { get; set; } = null;

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
