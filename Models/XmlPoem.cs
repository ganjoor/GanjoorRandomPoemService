namespace GanjoorRandomPoemService.Models
{
    public class XmlPoem
    {
        public string PoetName { get; set; }
        public string PoemUrl { get; set; }
        public Couplet[] Couplets { get; set; }
    }
}
