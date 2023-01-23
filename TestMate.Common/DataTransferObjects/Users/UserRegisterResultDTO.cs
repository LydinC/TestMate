namespace TestMate.Common.DataTransferObjects.Users
{
    public class UserRegisterResultDTO
    {
        public string Username { get; set; }


        public UserRegisterResultDTO(string username)
        {
            Username = username;
        }
    }
}
