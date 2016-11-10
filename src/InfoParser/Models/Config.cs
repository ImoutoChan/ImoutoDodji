namespace InfoParser.Models
{
    public class ExhentaiConfiguration
    {
        public int ipb_member_id { get; set; }
        
        public string ipb_pass_hash { get; set; }
    }

    public class Configuration
    {
        public ExhentaiConfiguration ExhentaiConfiguration { get; set; } = new ExhentaiConfiguration();

        //public ParserType[] ParserOrder { get; set; } = new []{ ParserType.}
    }
}
