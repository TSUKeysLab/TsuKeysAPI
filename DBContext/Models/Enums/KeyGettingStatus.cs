namespace tsuKeysAPIProject.DBContext.Models.Enums
{
    using System.Text.Json.Serialization;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KeyGettingStatus
    {
        AllKeys,
        AvailableKeys
    }
}
