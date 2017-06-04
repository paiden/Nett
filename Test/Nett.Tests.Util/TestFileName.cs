using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Nett.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public sealed class TestFileName : IDisposable
    {
        private readonly string fileName;

        private TestFileName(string fileName)
        {
            this.fileName = fileName;
        }

        public static implicit operator string(TestFileName fn) => fn.fileName;

        public static TestFileName Create(string name, string extension, [CallerMemberName]string test = null)
        {
            var fn = $"{test}_{name}".TestRunUniqueName(extension);
            Directory.CreateDirectory(Path.GetDirectoryName(fn));
            return new TestFileName(fn);
        }

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
