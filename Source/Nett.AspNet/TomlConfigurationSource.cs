using Microsoft.Extensions.Configuration;

namespace Nett.AspNet
{
    public class TomlConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new TomlConfigurationProvider(this);
        }
    }
}
