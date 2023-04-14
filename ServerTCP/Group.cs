using System;
namespace ServerTCP
{
    public class Group
    {
        private User _admin { get; set; }
        public string _adminName { get; set; }
        public List<User> _members = new List<User>();

        public Group(User admin, string adminName)
        {
            _admin = admin;
            _adminName = adminName;
        }
    }
}

