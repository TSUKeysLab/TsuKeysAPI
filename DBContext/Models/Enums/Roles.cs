namespace tsuKeysAPIProject.DBContext.Models.Enums
{
    using System.Text.Json.Serialization;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Roles
    {
        User,
        Administrator,
        Teacher,
        DeanTeacher,
        Student,
        Dean
    }
}
