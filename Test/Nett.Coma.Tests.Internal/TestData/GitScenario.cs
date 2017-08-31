using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Nett.Tests.Util;

namespace Nett.Coma.Tests.TestData
{
    /// <summary>
    /// Simulate a git config system with different configuration scopes
    /// </summary>
    [ExcludeFromCodeCoverage]
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
            this.SystemFile = TestFileName.Create("system", GitConfig.Extension, testName);
            this.UserFile = TestFileName.Create("user", GitConfig.Extension, testName);
            this.RepoFile = TestFileName.Create("repo", GitConfig.Extension, testName);
        }

        public string RepoSourceName => "RepoAlias";
        public TestFileName RepoFile { get; }
        //public IConfigSource RepoFileSource { get; private set; }
        public string SystemSourceName => "System";
        public TestFileName SystemFile { get; }
        //public IConfigSource SystemFileSource { get; private set; }
        public string UserSourceName => "User";
        public TestFileName UserFile { get; }
        //public IConfigSource UserFileSource { get; private set; }

        public static GitScenario Setup(string testName)
        {
            var scenario = new GitScenario(testName);

            File.WriteAllText(scenario.SystemFile, SystemDefaultContent);
            File.WriteAllText(scenario.UserFile, UserDefaultContent);
            File.WriteAllText(scenario.RepoFile, RepoDefaultContent);

            return scenario;
        }

        public Config<GitConfig> CreateMergedFromDefaultsWithExplicitFileStore()
        {
            var sys = new FileConfigStore(TomlSettings.DefaultInstance, this.SystemFile);
            var user = new FileConfigStore(TomlSettings.DefaultInstance, this.UserFile);
            var repo = new FileConfigStore(TomlSettings.DefaultInstance, this.RepoFile);

            return Config.CreateAs()
                .MappedToType(() => new GitConfig())
                .StoredAs(store => store
                    .File(this.SystemFile).AsSourceWithName(this.SystemSourceName)
                    .MergeWith().File(this.UserFile).AsSourceWithName(this.UserSourceName)
                    .MergeWith().File(this.RepoFile).AsSourceWithName(this.RepoSourceName))
                .Initialize();
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
