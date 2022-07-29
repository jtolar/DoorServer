using DoorServer.TcpServers.HostedServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoorServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly RloginHostedService rloginHostedService;

        public IndexModel(ILogger<IndexModel> logger, RloginHostedService rloginHostedService)
        {
            this.logger = logger;
            this.rloginHostedService = rloginHostedService;
        }

        public void OnGet()
        { 
        }
    }
}