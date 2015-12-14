using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Users
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string UserPrincipalName { get; set; }
        public string CanonicalName { get; set; }
        public string Uid { get; set; }
        public string DistinguishedName { get; set; }
        public UserAccountControl UserAccountControl { get; set; }
    }
}
