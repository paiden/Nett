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

        private IConfigSource repoSource;
        public IConfigSource RepoSource => this.repoSource;

        public TestFileName RepoFile { get; }
        public IConfigSource SystemSource => this.systemSource;
        private IConfigSource systemSource;
        public TestFileName SystemFile { get; }
        //public IConfigSource SystemFileSource { get; private set; }
        private IConfigSource userSource;
        public IConfigSource UserSource => this.userSource;
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

        public Config<GitConfig> CreateMergedFromDefaults()
        {
            return Config.CreateAs()
                .MappedToType(() => new GitConfig())
                .StoredAs(store =>
                    store.File(this.SystemFile).AccessedBySource("sys", out this.systemSource).MergeWith(
                        store.File(this.UserFile).AccessedBySource("user", out this.userSource).MergeWith(
                            store.File(this.RepoFile).AccessedBySource("repo", out this.repoSource))))
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
