using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models
{
    public class UserAdminInfo
    {
        public string Id { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public bool Enable { get; set; }
        public bool EmailVerified { get; set; }
    }
}