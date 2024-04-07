// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System.Collections.Generic;

public class RegistrationSuggestedFriendOrdering
{
    public string suggestion_subtext { get; set; }
    public string suggestion_subtext_lowercase { get; set; }
    public string suggestion_token { get; set; }
    public string userId { get; set; }
}

public class find_friends_reg
{
    public string content_suggestion_page_position { get; set; }
    public bool is_trimmed { get; set; }
    public int last_address_book_updated_date { get; set; }
    public List<object> nonSnapchatterResult { get; set; }
    public string recently_active_text { get; set; }
    public List<RegistrationSuggestedFriendOrdering> registration_suggested_friend_ordering { get; set; }
    public List<object> results { get; set; }
    public List<SuggestedFriendResultsV2> suggested_friend_results_v2 { get; set; }
}

public class SuggestedFriendResultsV2
{
    public string bitmoji_avatar_id { get; set; }
    public string bitmoji_background_id { get; set; }
    public string bitmoji_scene_id { get; set; }
    public string bitmoji_selfie_id { get; set; }
    public string display_name { get; set; }
    public bool is_recently_active { get; set; }
    public string mutable_username { get; set; }
    public string snap_pro_id { get; set; }
    public string snapshot_metadata_string { get; set; }
    public string userId { get; set; }
    public string username { get; set; }
}

