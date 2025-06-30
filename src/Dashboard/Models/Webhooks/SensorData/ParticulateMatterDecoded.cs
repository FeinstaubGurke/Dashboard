using System.Text.Json.Serialization;

namespace Dashboard.Models.Webhooks.SensorData
{
    public class ParticulateMatterDecoded
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

        /// <summary>
        /// PM1 Micrograms per cubic meter
        /// </summary>
        [JsonPropertyName("mc_1p0")]
        public double? PM1 { get; set; }

        /// <summary>
        /// PM2.5 Micrograms per cubic meter
        /// </summary>
        [JsonPropertyName("mc_2p5")]
        public double? PM2_5 { get; set; }

        /// <summary>
        /// PM4 Micrograms per cubic meter
        /// </summary>
        [JsonPropertyName("mc_4p0")]
        public double? PM4 { get; set; }

        /// <summary>
        /// PM10 Micrograms per cubic meter
        /// </summary>
        [JsonPropertyName("mc_10p0")]
        public double? PM10 { get; set; }

        /// <summary>
        /// PM0.5 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("nc_0p5")]
        public double? ParticlesPerCubicCentimeterPM0_5 { get; set; }

        /// <summary>
        /// PM1 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("nc_1p0")]
        public double? ParticlesPerCubicCentimeterPM1 { get; set; }

        /// <summary>
        /// PM2.5 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("nc_2p5")]
        public double? ParticlesPerCubicCentimeterPM2_5 { get; set; }

        /// <summary>
        /// PM4 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("nc_4p0")]
        public double? ParticlesPerCubicCentimeterPM4 { get; set; }

        /// <summary>
        /// PM10 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("nc_10p0")]
        public double? ParticlesPerCubicCentimeterPM10 { get; set; }

        /// <summary>
        /// Typical particle size
        /// </summary>
        [JsonPropertyName("typical_particle_size")]
        public double? TypicalParticleSize { get; set; }
    }
}
