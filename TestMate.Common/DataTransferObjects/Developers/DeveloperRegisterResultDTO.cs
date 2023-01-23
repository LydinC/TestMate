namespace TestMate.Common.DataTransferObjects.Developers
{
    public class DeveloperRegisterResultDTO
    {
        public string Username { get; set; }


        public DeveloperRegisterResultDTO(string username)
        {
            Username = username;
        }
    }
}
