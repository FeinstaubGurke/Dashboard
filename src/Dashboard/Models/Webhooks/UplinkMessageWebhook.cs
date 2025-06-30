namespace Dashboard.Models.Webhooks
{
    public class UplinkMessageWebhook<T> : TheThingsNetworkWebhookBase
    {
        public UplinkMessage<T> UplinkMessage { get; set; }
    }
}
