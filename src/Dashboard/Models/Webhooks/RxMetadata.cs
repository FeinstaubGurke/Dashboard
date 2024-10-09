namespace Dashboard.Models.Webhooks
{
    public class RxMetadata
    {
        public GatewayIds GatewayIds { get; set; }
        public DateTime? Time { get; set; }
        public long Timestamp { get; set; }
        public int Rssi { get; set; }
        public int ChannelRssi { get; set; }
        public float Snr { get; set; }
        public Location? Location { get; set; }
        public string UplinkToken { get; set; }
        public int ChannelIndex { get; set; }
        public string ReceivedAt { get; set; }
    }
}
