using System;
using System.IO;
using System.Text;
using Nett.Tests.Util;

namespace Nett.Coma.Tests.TestData
{
    public class MergeThreeFilesScenario : IDisposable
    {
        private TestFileName machineFile;
        private TestFileName userFile;
        private TestFileName localFile;

        private IConfigSource machineSource;
        private IConfigSource userSource;
        private IConfigSource localSource;

        public IConfigSource MachineSource => this.machineSource;

        public IConfigSource UserSource => this.userSource;

        public IConfigSource LocalSource => this.localSource;


        public TestFileName LocalFile => this.localFile;

        public class Cfg
        {
            public Item A { get; set; }

            public Item B { get; set; }

            public Item C { get; set; }

            public Item D { get; set; }

            public class Item
            {
                public string ItemVal { get; set; } = string.Empty;
            }
        }

        public static MergeThreeFilesScenario Setup(string testName)
        {
            var scen = new MergeThreeFilesScenario();
            scen.Init(testName);
            return scen;
        }

        public static string GetToml(params (char k, string v)[] kvp)
        {
            var sb = new StringBuilder();

            foreach (var i in kvp)
            {
                sb.AppendLine($"{i.k} = {{ {nameof(Cfg.Item.ItemVal)} = \"{i.v}\" }}");
            }

            return sb.ToString();
        }

        public Config<Cfg> Load()
        {
            return Config.CreateAs()
                .MappedToType(() => new Cfg())
                .StoredAs(store => store
                    .File(this.machineFile).AccessedBySource("machine", out this.machineSource).MergeWith(
                        store.File(this.userFile).AccessedBySource("user", out this.userSource).MergeWith(
                            store.File(this.localFile).AccessedBySource("local", out this.localSource))))
                .Initialize();
        }

        public MergeThreeFilesScenario MachineFileRemoved()
        {
            this.machineFile.Dispose();
            return this;
        }

        public MergeThreeFilesScenario UserFileRemoved()
        {
            this.userFile.Dispose();
            return this;
        }

        public MergeThreeFilesScenario LocalFileRemoved()
        {
            this.localFile.Dispose();
            return this;
        }
        private void Init(string testName)
        {
            this.machineFile = TestFileName.Create("machine", Toml.FileExtension, testName);
            this.userFile = TestFileName.Create("user", Toml.FileExtension, testName);
            this.localFile = TestFileName.Create("local", Toml.FileExtension, testName);

            File.WriteAllText(this.machineFile, GetToml(('A', "machine"), ('B', "machine"), ('C', "machine")));
            File.WriteAllText(this.userFile, GetToml(('A', "user"), ('B', "user")));
            File.WriteAllText(this.localFile, GetToml(('C', "local"), ('D', "local")));
        }


        public void Dispose()
        {
            this.machineFile.Dispose();
            this.userFile.Dispose();
            this.localFile.Dispose();
        }
    }
}
