using GanjoorRandomPoemService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace GanjoorRandomPoemService.Pages
{
    public class BeytPHPModel : PageModel
    {
        public string BlockToUpdate { get; set; }
        public Couplet[] Couplets { get; set; }
        public string PoetName { get; set; }
        public string PoemUrl { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            Response.ContentType = "text/javascript;charset=utf-8";

            BlockToUpdate = "";
            if (!string.IsNullOrEmpty(Request.Query["block"]))
            {
                BlockToUpdate = Request.Query["block"];
            }

            int poetId = 0;
            if (!string.IsNullOrEmpty(Request.Query["p"]))
            {
                poetId = int.Parse(Request.Query["p"]);
            }

            bool twoCouplets = false;
            if (!string.IsNullOrEmpty(Request.Query["a"]))
            {
                twoCouplets = Request.Query["a"] == "1";
            }

            var response = await Client.GetAsync($"{Configuration["APIRoot"]}/api/ganjoor/poem/random?poetId={poetId}");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync()));
            }
            var poem = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            if(poem == null)
            {
                //this sometimes happen, I do not know why but it worth a retry for fixing probable bugs
                //retry with no poetId!
                response = await Client.GetAsync($"{Configuration["APIRoot"]}/api/ganjoor/poem/random");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync()));
                }
                poem = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            }
            PoemUrl = $"https://ganjoor.net{poem.fullUrl}";
            PoetName = poem.fullTitle;
            PoetName = PoetName.Substring(0, PoetName.IndexOf("»")).Trim();
            List<Verse> verses = new List<Verse>();
            List<Couplet> couplets = new List<Couplet>();
            foreach (var item in poem.verses)
            {
                int coupletIndex = item.coupletIndex;
                var existing = couplets.FirstOrDefault(c => c.Index == coupletIndex);
                if (existing != null)
                {
                    existing.Verse2 = item.text;
                }
                else
                {
                    couplets.Add(new Couplet()
                    {
                        Index = coupletIndex,
                        Verse1 = item.text,
                    });
                }
            }

            Random r = new Random(DateTime.Now.Millisecond);
            if (twoCouplets && couplets.Count > 1)
            {
                int index = r.Next(0, couplets.Count - 2);
                Couplets = [couplets[index], couplets[index + 1]];
            }
            else
            {
                int index = r.Next(0, couplets.Count - 1);
                Couplets = [couplets[index]];
            }

            return Page();
        }

        protected readonly HttpClient Client;

        protected readonly IConfiguration Configuration;
        public BeytPHPModel(HttpClient httpClient, IConfiguration configuration)
        {
            Client = httpClient;
            Configuration = configuration;
        }
    }
}
