namespace Dashboard.Clients.Models
{
    public class EndDevices
    {
        public Ids ids { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Attributes attributes { get; set; }
    }
}
