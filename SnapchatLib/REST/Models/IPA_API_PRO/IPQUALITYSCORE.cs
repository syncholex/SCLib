public class IPQUALITYSCORE
{
    public bool success { get; set; }
    public string message { get; set; }
    public int fraud_score { get; set; }
    public string country_code { get; set; }
    public string region { get; set; }
    public string city { get; set; }
    public string ISP { get; set; }
    public int ASN { get; set; }
    public string organization { get; set; }
    public bool is_crawler { get; set; }
    public string timezone { get; set; }
    public bool mobile { get; set; }
    public string host { get; set; }
    public bool proxy { get; set; }
    public bool vpn { get; set; }
    public bool tor { get; set; }
    public bool active_vpn { get; set; }
    public bool active_tor { get; set; }
    public bool recent_abuse { get; set; }
    public bool bot_status { get; set; }
    public string connection_type { get; set; }
    public string abuse_velocity { get; set; }
    public string zip_code { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string request_id { get; set; }
}