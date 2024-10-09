using System.Text.Json.Serialization;

namespace Dashboard.Models.Webhooks
{
    public class Decoded
    {
        public string BootloaderVersion { get; set; }
        public string FirmwareVersion { get; set; }
        public int HardwareVersion { get; set; }
        public string Sensormodus { get; set; }
        public string TxReason { get; set; }
        public int UplinkZyklus { get; set; }
        public float Voltage { get; set; }

        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        
        [JsonPropertyName("mc_1p0")]
        public double? PM1 { get; set; }

        [JsonPropertyName("mc_2p5")]
        public double? PM2_5 { get; set; }
    }
}
