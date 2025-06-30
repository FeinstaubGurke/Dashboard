namespace Dashboard.Models.Webhooks
{
    public class UplinkMessage<T>
    {
        public string SessionKeyId { get; set; }
        public int f_port { get; set; }
        public int f_cnt { get; set; }
        public string FrmPayload { get; set; }
        public DecodedPayload<T>? DecodedPayload { get; set; }
        public RxMetadata[] RxMetadata { get; set; }
        public Settings Settings { get; set; }
        public string ReceivedAt { get; set; }
        public string ConsumedAirtime { get; set; }
        public NetworkIds NetworkIds { get; set; }
    }
}
