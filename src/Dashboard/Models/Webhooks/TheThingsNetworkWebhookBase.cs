﻿namespace Dashboard.Models.Webhooks
{
    public class TheThingsNetworkWebhookBase
    {
        public EndDeviceIds EndDeviceIds { get; set; }
        public string[] CorrelationIds { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
