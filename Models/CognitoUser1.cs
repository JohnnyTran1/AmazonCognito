using Amazon.CognitoIdentityProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon;
using Amazon.Runtime;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AmazonCognito.Models
{
    /*
     * DO NOT NEED THIS
     */
    public class CognitoUser1 : IdentityUser
    {
        public string Password { get; set; }
        public UserStatusType Status { get; set; }
    }
}