using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Nett.AspNet.SystemTests.Controllers
{
    [Produces("application/json")]
    [Route("api/Home")]
    public class HomeController : Controller
    {
        private readonly TestOptions options;
        private readonly MySubOptions subOptions;

        public HomeController(IOptions<TestOptions> options, IOptions<MySubOptions> subOptions)
        {
            this.options = options.Value;
            this.subOptions = subOptions.Value;
        }

        public string Get()
        {
            return $"Option1={this.options.Option1};Option2={this.options.Option2};" +
                $"SubOption1={this.subOptions.SubOption1};SubOption2={this.subOptions.SubOption2}";
        }
    }
}