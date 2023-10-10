namespace TodoApi
{
    public class UserDTO
    {

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }


        public UserDTO() { }

        public UserDTO(User user) => 
        (UserId, UserName, Password) = (user.UserId, user.UserName, user.Password);
    }
}
