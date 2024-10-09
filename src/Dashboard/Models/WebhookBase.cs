namespace Dashboard.Models
{
    public class WebhookBase
    {
        public EndDeviceIds EndDeviceIds { get; set; }
        public string[] CorrelationIds { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
