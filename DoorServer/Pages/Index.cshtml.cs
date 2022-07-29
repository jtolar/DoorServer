using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoorServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        //private readonly RloginHostedService rloginHostedService;

        public IndexModel(ILogger<IndexModel> logger)
        {
            this.logger = logger;
            //rloginHostedService = hostedService;
        }

        public void OnGet()
        {
            logger.LogInformation("Index Page Load.");
        }
    }
}