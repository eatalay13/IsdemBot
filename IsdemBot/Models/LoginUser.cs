namespace IsdemBot.Models
{
    public class LoginUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public static LoginUser DefaultUser
        {
            get
            {
                var user = new LoginUser
                {
                    UserName = "43669784782",
                    Password = "BADE43*-"
                };

                return user;
            }
        }
    }
}
