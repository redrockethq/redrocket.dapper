namespace RedRocket.Dapper.Core.Tests.Models
{
    [Table("User")]
    public interface IUser
    {
        [PrimaryKey("User_Id")]
        int UserId { get; set; }

        [Column("First_Name")]
        string FirstName { get; set; }
    }
}
