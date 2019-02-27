using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed partial class TomlSettings
    {
        internal interface IExpSettingsBuilder
        {
            void EnableExperimentalFeature(ExperimentalFeature feature, bool enable);
        }
    }
}
