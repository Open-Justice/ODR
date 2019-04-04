using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClickNClaim.WebPortal.Startup))]
namespace ClickNClaim.WebPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
