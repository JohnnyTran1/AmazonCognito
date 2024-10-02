using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models.Account
{
    public class AccountResponse
    {
        public bool HasError { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }

        public AccountResponse()
        {
            HasError = false;
        }
    }
}