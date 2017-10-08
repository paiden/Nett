using Newtonsoft.Json;

namespace Nett.PerfTests
{
    /// <summary>
    /// Simplified parser that tries to do real parser like processing tasks. This simplified parsing
    /// is used to establish a base line for performance checks. This is a very simple first version...
    /// that is missing quite a few aspects from the real parser... but sould be good enought for a
    /// first try.
    /// Missing:
    /// - Dictionary Lookup
    /// - List traversal lookup
    /// - Collection allocations
    /// - Small collection copy
    /// - Reflection based property mapping
    /// </summary>
    internal sealed class BaselineParser
    {
        public T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
