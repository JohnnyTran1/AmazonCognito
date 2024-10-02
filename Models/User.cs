using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }
        public string FamilyName { get; set; }
        public string Gender { get; set; }
        public string GivenName { get; set; }
        public string Locale { get; set; }
        public string MiddleName { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string PhoneNumber { get; set; }
        public object Picture { get; set; }
        public string PreferredUsername { get; set; }
        public string Profile { get; set; }
        public string ZoneInfo { get; set; }
        public string UpdatedAt { get; set; }
        public string Website { get; set; }


        
    }
}