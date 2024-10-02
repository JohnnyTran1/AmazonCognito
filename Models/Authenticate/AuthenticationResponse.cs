using Amazon.CognitoIdentityProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonCognito.Models.Authenticate
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool HasError { get; set; }
        public string Message { get; set; }
        public ChallengeNameType ChallengeNameType { get; set; }
        public string SessionID { get; set; }

        public Token Token { get; set; }
        public AuthenticationResponse()
        {
            HasError = false;
            IsAuthenticated = false;
        }

    }
}