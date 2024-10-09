namespace Dashboard.Models
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
    }
}
