using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlString : TomlValue<string>
    {
        public enum TypeOfString
        {
            Default,
            Normal,
            Literal,
            Multiline,
            MultilineLiteral,
        }

        private TypeOfString type = TypeOfString.Default;

        public TomlString(string value, TypeOfString type = TypeOfString.Default)
            : base(value)
        {
            this.type = type;
        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            if(this.type == TypeOfString.Default)
            {
                var toWrite = TomlString.EspaceString(this.Value);
                writer.WriteLine("{0}", toWrite);
            }
        }

        private static string EspaceString(string source)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(source), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}
