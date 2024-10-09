namespace Dashboard.Models.Webhooks
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Altitude { get; set; }
        public string Source { get; set; }
    }
}
