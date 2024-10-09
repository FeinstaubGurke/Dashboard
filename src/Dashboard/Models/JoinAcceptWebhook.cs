namespace Dashboard.Models
{
    public class JoinAcceptWebhook
    {
        public EndDeviceIds EndDeviceIds { get; set; }
        public string[] CorrelationIds { get; set; }
        public string ReceivedAt { get; set; }
        public JoinAccept JoinAccept { get; set; }
    }
}
