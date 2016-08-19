using System;
using System.Diagnostics;
using System.IO;

namespace Nett.UnitTests.Util
{
    public sealed class TestFileName : IDisposable
    {
        private readonly string fileName;

        private TestFileName(string fileName)
        {
            this.fileName = fileName;
        }

        public static implicit operator string(TestFileName fn) => fn.fileName;

        public static TestFileName Create(string test, string name, string extension) =>
            new TestFileName($"{test}_{name}".TestRunUniqueName(extension));

        public void Dispose() => TryDeleteFile(this.fileName);

        private static void TryDeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Failed to cleanup file:" + exc.ToString());
            }
        }
    }
}
