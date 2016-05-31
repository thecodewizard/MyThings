using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyThings.Web.Startup))]
namespace MyThings.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
