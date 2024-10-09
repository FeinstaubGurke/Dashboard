namespace Dashboard.Models
{
    public class UplinkMessageWebhook
    {
        public EndDeviceIds EndDeviceIds { get; set; }
        public string[] CorrelationIds { get; set; }
        public DateTime ReceivedAt { get; set; }
        public UplinkMessage UplinkMessage { get; set; }
    }

    public class UplinkMessage
    {
        public string SessionKeyId { get; set; }
        public int f_port { get; set; }
        public int f_cnt { get; set; }
        public string FrmPayload { get; set; }
        public DecodedPayload DecodedPayload { get; set; }
        public RxMetadata[] RxMetadata { get; set; }
        public Settings Settings { get; set; }
        public string ReceivedAt { get; set; }
        public string ConsumedAirtime { get; set; }
        public NetworkIds NetworkIds { get; set; }
    }

    public class DecodedPayload
    {
        public Decoded Decoded { get; set; }
    }

    public class Decoded
    {
        public string BootloaderVersion { get; set; }
        public string firmware_version { get; set; }
        public int hardware_version { get; set; }
        public string sensormodus { get; set; }
        public string tx_reason { get; set; }
        public int uplink_zyklus { get; set; }
        public float voltage { get; set; }
    }

    public class Settings
    {
        public DataRate DataRate { get; set; }
        public string Frequency { get; set; }
        public long Timestamp { get; set; }
        public DateTime Time { get; set; }
    }

    public class DataRate
    {
        public Lora Lora { get; set; }
    }

    public class Lora
    {
        public int Bandwidth { get; set; }
        public int SpreadingFactor { get; set; }
        public string CodingRate { get; set; }
    }

    public class NetworkIds
    {
        public string NetId { get; set; }
        public string NsId { get; set; }
        public string TenantId { get; set; }
        public string ClusterId { get; set; }
        public string ClusterAddress { get; set; }
    }

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

    public class GatewayIds
    {
        public string GatewayId { get; set; }
        public string Eui { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Altitude { get; set; }
        public string Source { get; set; }
    }
}
