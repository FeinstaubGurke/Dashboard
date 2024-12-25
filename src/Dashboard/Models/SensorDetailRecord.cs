using System.Text.Json.Serialization;

namespace Dashboard.Models
{
    public class SensorDetailRecord
    {
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// PM1 Micrograms per cubic meter
        /// </summary>
        public double PM1 { get; set; }

        /// <summary>
        /// PM2.5 Micrograms per cubic meter
        /// </summary>
        public double PM2_5 { get; set; }

        /// <summary>
        /// PM4 Micrograms per cubic meter
        /// </summary>
        public double PM4 { get; set; }

        /// <summary>
        /// PM10 Micrograms per cubic meter
        /// </summary>
        public double PM10 { get; set; }


        /// <summary>
        /// PM0.5 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("CcPM0_5")]
        public double ParticlesPerCubicCentimeterPM0_5 { get; set; }

        /// <summary>
        /// PM1 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("CcPM1")]
        public double ParticlesPerCubicCentimeterPM1 { get; set; }

        /// <summary>
        /// PM2.5 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("CcPM2_5")]
        public double ParticlesPerCubicCentimeterPM2_5 { get; set; }

        /// <summary>
        /// PM4 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("CcPM4")]
        public double ParticlesPerCubicCentimeterPM4 { get; set; }

        /// <summary>
        /// PM10 Particles per cubic centimeter
        /// </summary>
        [JsonPropertyName("CcPM10")]
        public double ParticlesPerCubicCentimeterPM10 { get; set; }

        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
