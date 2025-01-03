using GanjoorRandomPoemService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace GanjoorRandomPoemService.Pages
{
    public class BeytXmlModel : PageModel
    {
        public XmlPoem[] Poems { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            Response.ContentType = "text/xml";
            Response.Headers.Append("Access-Control", "allow <*>");

            int reqNum = 1;
            if (!string.IsNullOrEmpty(Request.Query["n"]))
            {
                reqNum = int.Parse(Request.Query["n"]);
                if(reqNum < 1 || reqNum > 20)
                {
                    reqNum = 1;
                }
            }

            bool numberedItems = false;
            if (!string.IsNullOrEmpty(Request.Query["l"]))
            {
                numberedItems = Request.Query["l"] != "0";
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

            List<XmlPoem> list = new List<XmlPoem>();
            for (int i = 0; i < reqNum; i++)
            {
                
                var response = await Client.GetAsync($"{Configuration["APIRoot"]}/api/ganjoor/poem/random?poetId={poetId}");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync()));
                }
                var poem = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                if (poem == null)
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
                string poemUrl = $"https://ganjoor.net{poem.fullUrl}";
                string poetName = poem.fullTitle;
                poetName = poetName.Substring(0, poetName.IndexOf("»")).Trim();
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

                XmlPoem x = new XmlPoem()
                {
                    PoemUrl = poemUrl,
                    PoetName = poetName,
                };

                Random r = new Random(DateTime.Now.Millisecond);
                if (twoCouplets && couplets.Count > 1)
                {
                    int index = r.Next(0, couplets.Count - 2);
                    couplets[index].Index = 1;
                    couplets[index + 1].Index = 2;
                    x.Couplets = [couplets[index], couplets[index + 1]];
                }
                else
                {
                    int index = r.Next(0, couplets.Count - 1);
                    couplets[index].Index = 1;
                    x.Couplets = [couplets[index]];
                }
                list.Add(x);
                
            }

            Poems = list.ToArray();

            return Page();
        }

        protected readonly HttpClient Client;

        protected readonly IConfiguration Configuration;
        public BeytXmlModel(HttpClient httpClient, IConfiguration configuration)
        {
            Client = httpClient;
            Configuration = configuration;
        }

    }
}
