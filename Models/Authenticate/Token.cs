using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models.Authenticate
{
    public class Token
    {
        public string Id_Token { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public DateTime IssueDate { get; set; }
        public int Expires_In { get; set; }
    }
}