namespace Dashboard.Models
{
    public class Rootobject
    {
        public End_Device_Ids end_device_ids { get; set; }
        public string[] correlation_ids { get; set; }
        public string received_at { get; set; }
        public Join_Accept join_accept { get; set; }
    }

    public class End_Device_Ids
    {
        public string device_id { get; set; }
        public Application_Ids application_ids { get; set; }
        public string dev_eui { get; set; }
        public string join_eui { get; set; }
        public string dev_addr { get; set; }
    }

    public class Application_Ids
    {
        public string application_id { get; set; }
    }

    public class Join_Accept
    {
        public string session_key_id { get; set; }
        public string received_at { get; set; }
    }
}
