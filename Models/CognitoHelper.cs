using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace AmazonCognito.Models
{
    public class CognitoHelper
    {
        private readonly string _userPoolId = ConfigurationManager.AppSettings["User_Pool_Id"];
        private readonly string _userPoolClientAppId = ConfigurationManager.AppSettings["User_Pool_Client_Id"];
        private readonly string _userPoolClientAppSecret = ConfigurationManager.AppSettings["User_Pool_Client_Secret"];
        private readonly AmazonCognitoIdentityProviderClient _provider;
        private readonly CognitoUserPool _cognitoUserPool;
        private CognitoResponse cognitoResponse;
        private readonly Amazon.RegionEndpoint region = Amazon.RegionEndpoint.USEast1;
        private List<JWT.Keys> _keyList;

        /*
         * Below is only needed if you are going to use the AWS Login Page (Oauth).
         */
        private string _oauthDomainUrl = ConfigurationManager.AppSettings["User_Pool_Oauth_Domain"];
        private string _oauthCallbackRedirectUri = ConfigurationManager.AppSettings["User_Pool_Oauth_Redirect_Uri"];
        private string _oauthSignoutUri = ConfigurationManager.AppSettings["User_Pool_Oauth_Signout_Uri"];

        public CognitoHelper()
        {
            /* 
             * I recommend setting up a new service account (New user in AWS IAM) with limited privileges/permissions. These keys will be stored in AwsSettings.config 
             * EX. Add new user and check "Access key- Programmatic access", uncheck "Password - AWS Management Console Access".
             * Only give policy permission to "AmazonCognitoPowerUser" and "AmazonCognitoReadOnly". 
             * NOTE: You may need additional permissions dependent on the requirements of your project, but typically those two policies will be enough.
             */
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(ConfigurationManager.AppSettings["Aws_Access_Key"], ConfigurationManager.AppSettings["Aws_Access_Key_Secret"]);
            _provider = new AmazonCognitoIdentityProviderClient(awsCredentials, region);
            _cognitoUserPool = new CognitoUserPool(_userPoolId, _userPoolClientAppId, _provider);
        }

        /*Used for Oauth*/
        internal string UserPoolClientAppId
        {
            get { return _userPoolClientAppId; }
        }

        internal string UserPoolClientAppSecret
        {
            get { return _userPoolClientAppSecret; }
        }

        internal string OauthDomainUrl
        {
            get { return _oauthDomainUrl; }
        }

        internal string OauthRedirectUri
        {
            get { return _oauthCallbackRedirectUri; }
        }

        internal string OauthSignoutRedirectUri
        {
            get { return _oauthSignoutUri; }
        }

        internal Task<List<JWT.Keys>> GetPublicKeys
        {
            get { return GetAWSPublicKey(); }
        }

        private async Task<List<JWT.Keys>> GetAWSPublicKey()
        {
            string url = $"https://cognito-idp.{region.SystemName}.amazonaws.com/{_userPoolId}/.well-known/jwks.json";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return  _keyList = JWT.Keys.DecodePublicKeys(content);
            }
        }


        internal void GetUserPoolClient()
        {
            DescribeUserPoolClientRequest describeUserPoolClientRequest = new DescribeUserPoolClientRequest
            {
                ClientId = _userPoolClientAppId,
                UserPoolId = _userPoolId
            };

            DescribeUserPoolClientResponse describeUserPoolResponse = _provider.DescribeUserPoolClient(describeUserPoolClientRequest);

            if (describeUserPoolResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                _oauthSignoutUri = describeUserPoolResponse.UserPoolClient.LogoutURLs.FirstOrDefault().ToString();
                _oauthCallbackRedirectUri = describeUserPoolResponse.UserPoolClient.CallbackURLs.FirstOrDefault().ToString();
            }
        }


        /// <summary>
        /// Credentials will be validated and return an access token if valid.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>AuthenticationResponse</returns>
        internal async Task<Authenticate.AuthenticationResponse> ValidateUserCredentials(string username, string password)
        {
            Authenticate.AuthenticationResponse authenticationResponse = new Authenticate.AuthenticationResponse();

            try
            {
                CognitoUser user = new CognitoUser(username, this._userPoolClientAppId, _cognitoUserPool, _provider, this._userPoolClientAppSecret);

                InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
                {
                    Password = password
                };

                AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(authResponse.ChallengeName) && authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    authenticationResponse.IsAuthenticated = false;
                    authenticationResponse.HasError = true;
                    authenticationResponse.ChallengeNameType = ChallengeNameType.NEW_PASSWORD_REQUIRED;
                    authenticationResponse.SessionID = authResponse.SessionID;
                    authenticationResponse.Message = "A new password is required.";
                    return authenticationResponse;
                }
                if (authResponse.AuthenticationResult != null)
                {
                    Authenticate.Token token = new Authenticate.Token
                    {
                        Access_Token = authResponse.AuthenticationResult.AccessToken,
                        Id_Token = authResponse.AuthenticationResult.IdToken,
                        Refresh_Token = authResponse.AuthenticationResult.RefreshToken,
                        Expires_In = authResponse.AuthenticationResult.ExpiresIn,
                        IssueDate = DateTime.Now
                    };

                    authenticationResponse.IsAuthenticated = true;
                    authenticationResponse.AccessToken = authResponse.AuthenticationResult.AccessToken;
                    authenticationResponse.Token = token;
                    return authenticationResponse;
                }
                else
                {
                    throw new Exception("An unexpected error has occurred. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                authenticationResponse.HasError = true;
                authenticationResponse.Message = ex.Message;
                return authenticationResponse;
            }

        }

        /// <summary>
        /// Sign out user by terminating access token
        /// </summary>
        /// <param name="accessToken">User's access token</param>
        /// <returns>Boolean</returns>
        internal async Task<CognitoResponse> SignOut(string accessToken)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                var userRequest = new GlobalSignOutRequest
                {
                    AccessToken = accessToken
                };

                GlobalSignOutResponse globalSignOutResponse = await _provider.GlobalSignOutAsync(userRequest);

                if (globalSignOutResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)globalSignOutResponse.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = globalSignOutResponse.ResponseMetadata.ToString();
                }
                else
                {
                    cognitoResponse.Code = (int)globalSignOutResponse.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = globalSignOutResponse.ResponseMetadata.ToString();
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// Get attributes associated with the user
        /// </summary>
        /// <param name="token">User Access Token</param>
        /// <returns>User</returns>
        internal async Task<User> GetUserDetails(string token)
        {
            try
            {
                var userRequest = new GetUserRequest
                {
                    AccessToken = token
                };

                GetUserResponse getUserResponse = await _provider.GetUserAsync(userRequest);


                if (getUserResponse != null)
                {
                    List<AttributeType> attributeList = getUserResponse.UserAttributes;

                    User user = new User
                    {
                        Id = attributeList.Exists(x => x.Name == "sub") ? attributeList.Find(x => x.Name == "sub").Value.ToString() : string.Empty,
                        Address = attributeList.Exists(x => x.Name == "address") ? attributeList.Find(x => x.Name == "address").Value.ToString() : string.Empty,
                        BirthDate = attributeList.Exists(x => x.Name == "birthdate") ? attributeList.Find(x => x.Name == "birthdate").Value.ToString() : string.Empty,
                        Email = attributeList.Exists(x => x.Name == "email") ? attributeList.Find(x => x.Name == "email").Value.ToString() : string.Empty,
                        FamilyName = attributeList.Exists(x => x.Name == "family_name") ? attributeList.Find(x => x.Name == "family_name").Value.ToString() : string.Empty,
                        Gender = attributeList.Exists(x => x.Name == "gender") ? attributeList.Find(x => x.Name == "gender").Value.ToString() : string.Empty,
                        GivenName = attributeList.Exists(x => x.Name == "given_name") ? attributeList.Find(x => x.Name == "given_name").Value.ToString() : string.Empty,
                        Locale = attributeList.Exists(x => x.Name == "locale") ? attributeList.Find(x => x.Name == "locale").Value.ToString() : string.Empty,
                        MiddleName = attributeList.Exists(x => x.Name == "middle_name") ? attributeList.Find(x => x.Name == "middle_name").Value.ToString() : string.Empty,
                        Name = attributeList.Exists(x => x.Name == "name") ? attributeList.Find(x => x.Name == "name").Value.ToString() : string.Empty,
                        Nickname = attributeList.Exists(x => x.Name == "nickname") ? attributeList.Find(x => x.Name == "nickname").Value.ToString() : string.Empty,
                        PhoneNumber = attributeList.Exists(x => x.Name == "phone_number") ? attributeList.Find(x => x.Name == "phone_number").Value.ToString() : string.Empty,
                        Picture = attributeList.Exists(x => x.Name == "picture") ? attributeList.Find(x => x.Name == "picture").Value.ToString() : string.Empty,
                        PreferredUsername = attributeList.Exists(x => x.Name == "preferred_username") ? attributeList.Find(x => x.Name == "preferred_username").Value.ToString() : string.Empty,
                        Profile = attributeList.Exists(x => x.Name == "profile") ? attributeList.Find(x => x.Name == "profile").Value.ToString() : string.Empty,
                        UpdatedAt = attributeList.Exists(x => x.Name == "updated_at") ? attributeList.Find(x => x.Name == "updated_at").Value.ToString() : string.Empty,
                        Website = attributeList.Exists(x => x.Name == "website") ? attributeList.Find(x => x.Name == "website").Value.ToString() : string.Empty,
                        ZoneInfo = attributeList.Exists(x => x.Name == "zoneinfo") ? attributeList.Find(x => x.Name == "zoneinfo").Value.ToString() : string.Empty
                    };

                    return user;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// User accounts that have a challenge type "NEW_PASSWORD_REQUIRED" will need to update their password. 
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="sessionToken">Session token</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        internal async Task<Authenticate.AuthenticationResponse> NewPasswordRequiredUpdate(string username, string sessionToken, string newPassword)
        {
            Authenticate.AuthenticationResponse authenticationResponse = new Authenticate.AuthenticationResponse();

            try
            {
                CognitoUser user = new CognitoUser(username, this._userPoolClientAppId, _cognitoUserPool, _provider, _userPoolClientAppSecret);

                RespondToNewPasswordRequiredRequest respondToNewPasswordRequiredRequest = new RespondToNewPasswordRequiredRequest
                {
                    NewPassword = newPassword,
                    SessionID = sessionToken
                };


                AuthFlowResponse authFlowResponse = await user.RespondToNewPasswordRequiredAsync(respondToNewPasswordRequiredRequest);

                if (!string.IsNullOrWhiteSpace(authFlowResponse.ChallengeName) && authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    authenticationResponse.IsAuthenticated = false;
                    authenticationResponse.HasError = true;
                    authenticationResponse.ChallengeNameType = ChallengeNameType.NEW_PASSWORD_REQUIRED;
                    authenticationResponse.SessionID = authFlowResponse.SessionID;
                    authenticationResponse.Message = "A new password is required.";
                    return authenticationResponse;
                }
                if (authFlowResponse.AuthenticationResult != null)
                {
                    authenticationResponse.IsAuthenticated = true;
                    authenticationResponse.AccessToken = authFlowResponse.AuthenticationResult.AccessToken;
                    return authenticationResponse;
                }
                else
                {
                    throw new Exception("An unexpected error has occurred. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                authenticationResponse.HasError = true;
                authenticationResponse.Message = ex.Message;
                return authenticationResponse;
            }
        }


        #region User signup
        /// <summary>
        /// A user can self-register for an account. A verification code will be sent to the provided email.
        /// Note: The provided input parameters may change depending on how your user pool is configured.
        /// https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        /// </summary>
        /// <param name="username">Username or Email</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email</param>
        /// <param name="firstName">First Name</param>
        /// <param name="LastName">Last Name</param>
        /// <returns></returns>
        internal async Task<CognitoResponse> SignUpUser(string username, string password, string email, string firstName, string LastName)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                SignUpRequest signUpRequest = new SignUpRequest();

                signUpRequest.ClientId = _userPoolClientAppId;
                signUpRequest.SecretHash = GetUserPoolSecretHash(username, _userPoolClientAppId, _userPoolClientAppSecret);
                signUpRequest.Username = username;
                signUpRequest.Password = password;

                //End-User's full name in displayable form including all name parts, possibly including titles and suffixes,
                //ordered according to the End-User's locale and preferences.
                AttributeType attributeType = new AttributeType();
                attributeType.Name = "name";
                attributeType.Value = firstName + " " + LastName;
                signUpRequest.UserAttributes.Add(attributeType);

                //End-User's preferred e-mail address.
                AttributeType attributeType1 = new AttributeType();
                attributeType1.Name = "email";
                attributeType1.Value = email;
                signUpRequest.UserAttributes.Add(attributeType1);

                //Given name(s) or first name(s) of the End-User. 
                AttributeType attributeType2 = new AttributeType();
                attributeType2.Name = "given_name";
                attributeType2.Value = firstName;
                signUpRequest.UserAttributes.Add(attributeType2);

                //Surname(s) or last name(s) of the End-User. 
                AttributeType attributeType3 = new AttributeType();
                attributeType3.Name = "family_name";
                attributeType3.Value = LastName;
                signUpRequest.UserAttributes.Add(attributeType3);

                SignUpResponse result = await _provider.SignUpAsync(signUpRequest);

                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Message = "User account was created.";
                    cognitoResponse.Code = (int)result.HttpStatusCode;
                    cognitoResponse.HasError = false;
                }
                else
                {
                    cognitoResponse.Message = "User failed to create.";
                    cognitoResponse.Code = (int)result.HttpStatusCode;
                    cognitoResponse.HasError = true;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }

        }

        /// <summary>
        /// Confirm email address from a self-register account by verifying code sent to the newly created user's email account.
        /// </summary>
        /// <param name="username">Username or Email</param>
        /// <param name="code">Verification Code</param>
        /// <returns>Bool</returns>
        internal async Task<CognitoResponse> ConfirmSignup(string username, string code)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                CognitoUser user = new CognitoUser(username, this._userPoolClientAppId, _cognitoUserPool, _provider);

                ConfirmSignUpRequest confirmSignUpRequest = new ConfirmSignUpRequest
                {
                    Username = username,
                    ConfirmationCode = code,
                    ClientId = this._userPoolClientAppId,
                    SecretHash = GetUserPoolSecretHash(username, _userPoolClientAppId, _userPoolClientAppSecret)
                };

                ConfirmSignUpResponse confirmSignUpResult = await _provider.ConfirmSignUpAsync(confirmSignUpRequest);

                if (confirmSignUpResult != null && confirmSignUpResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)confirmSignUpResult.HttpStatusCode;
                    cognitoResponse.Message = "User account confirmed";
                    cognitoResponse.HasError = false;
                }
                else
                {
                    cognitoResponse.Code = (int)confirmSignUpResult.HttpStatusCode;
                    cognitoResponse.Message = "User confirmation failed";
                    cognitoResponse.HasError = true;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        #endregion


        #region Forgot Password Flow
        /// <summary>
        /// Initiate the forgot password work flow. A verification code will be sent to the verified email on record.
        /// </summary>
        /// <param name="username">Username or Email</param>
        /// <returns></returns>
        internal async Task<CognitoResponse> ForgotPassword(string username)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                ForgotPasswordRequest forgotPasswordRequest = new ForgotPasswordRequest
                {
                    ClientId = _userPoolClientAppId,
                    Username = username,
                    SecretHash = GetUserPoolSecretHash(username, _userPoolClientAppId, _userPoolClientAppSecret)
                };

                ForgotPasswordResponse response = await _provider.ForgotPasswordAsync(forgotPasswordRequest);

                if (response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.Message = response.CodeDeliveryDetails.Destination;
                    cognitoResponse.HasError = false;
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = true;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// Reset forgotten password
        /// </summary>
        /// <param name="username">Username or Email</param>
        /// <param name="code">Verification code</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        internal async Task<CognitoResponse> ConfirmForgotPassword(string username, string code, string newPassword)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                ConfirmForgotPasswordRequest confirmForgotPasswordRequest = new ConfirmForgotPasswordRequest
                {
                    ClientId = _userPoolClientAppId,
                    Username = username,
                    ConfirmationCode = code,
                    SecretHash = GetUserPoolSecretHash(username, _userPoolClientAppId, _userPoolClientAppSecret),
                    Password = newPassword
                };

                ConfirmForgotPasswordResponse response = await _provider.ConfirmForgotPasswordAsync(confirmForgotPasswordRequest);

                if (response != null && response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.Message = response.ResponseMetadata.RequestId;
                    cognitoResponse.HasError = false;
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.Message = "Confirm forgot password failed.";
                    cognitoResponse.HasError = true;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        #endregion


        #region Admin Features
        /// <summary>
        /// Admin signup a user.
        /// Requires AWS credentials to be used setup in _provider.
        /// </summary>
        /// <param name="username">Username or email</param>
        /// <param name="email">Email</param>
        /// <param name="firstName">First Name</param>
        /// <param name="LastName">Last Name</param>
        /// <returns></returns>
        internal CognitoResponse AdminSignUpUser(string username, string email, string firstName, string LastName)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                AdminCreateUserRequest signUpRequest = new AdminCreateUserRequest
                {
                    UserPoolId = this._userPoolId,
                    DesiredDeliveryMediums = new List<string> { "EMAIL" },
                    Username = username,
                    //UserAttributes = new List<AttributeType>() { new AttributeType { Name = "email_verified", Value = "true" } }
                };

                //End-User's full name in displayable form including all name parts, possibly including titles and suffixes,
                //ordered according to the End-User's locale and preferences.
                AttributeType attributeType = new AttributeType();
                attributeType.Name = "name";
                attributeType.Value = firstName + " " + LastName;
                signUpRequest.UserAttributes.Add(attributeType);

                //End-User's preferred e-mail address.
                AttributeType attributeType1 = new AttributeType();
                attributeType1.Name = "email";
                attributeType1.Value = email;
                signUpRequest.UserAttributes.Add(attributeType1);

                //Given name(s) or first name(s) of the End-User. 
                AttributeType attributeType2 = new AttributeType();
                attributeType2.Name = "given_name";
                attributeType2.Value = firstName;
                signUpRequest.UserAttributes.Add(attributeType2);

                //Surname(s) or last name(s) of the End-User. 
                AttributeType attributeType3 = new AttributeType();
                attributeType3.Name = "family_name";
                attributeType3.Value = LastName;
                signUpRequest.UserAttributes.Add(attributeType3);

                //End-User's preferred nickname. 
                AttributeType attributeType4 = new AttributeType();
                attributeType4.Name = "nickname";
                attributeType4.Value = "Ghost";
                signUpRequest.UserAttributes.Add(attributeType4);

                AdminCreateUserResponse result = _provider.AdminCreateUser(signUpRequest);

                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)result.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = result.User.UserStatus;
                }
                else
                {
                    cognitoResponse.Code = (int)result.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = "Failed to signup user";
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// Disable a user in the user pool
        /// </summary>
        /// <param name="usernameDisabled">Username to be disabled</param>
        /// <returns>Boolean</returns>
        internal async Task<CognitoResponse> AdminDisableUser(string usernameDisabled)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                AdminDisableUserRequest disableUserRequest = new AdminDisableUserRequest
                {
                    Username = usernameDisabled,
                    UserPoolId = _userPoolId
                };

                AdminDisableUserResponse response = await _provider.AdminDisableUserAsync(disableUserRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = "User disabled";
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = "failed";
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// Enable a user in the user pool
        /// </summary>
        /// <param name="usernameEnable">Username to be enabled</param>
        /// <returns>Boolean</returns>
        internal async Task<CognitoResponse> AdminEnableUser(string usernameEnable)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                AdminEnableUserRequest enableUserRequest = new AdminEnableUserRequest
                {
                    Username = usernameEnable,
                    UserPoolId = _userPoolId
                };

                AdminEnableUserResponse response = await _provider.AdminEnableUserAsync(enableUserRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = "User enabled";
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = "failed";
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }


        internal async Task<CognitoResponse> AdminUpdateUserAttributes(string username, List<AttributeType> attributeList)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                AdminUpdateUserAttributesRequest adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
                {
                    Username = username,
                    UserAttributes = attributeList,
                    UserPoolId = _userPoolId
                };

                AdminUpdateUserAttributesResponse response = await _provider.AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = "User attributes updated";
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = "User attribute failed to update";
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        internal async Task<CognitoResponse> AdminGetAllUsers()
        {
            cognitoResponse = new CognitoResponse();
            List<UserAdminInfo> listOfUsers = new List<UserAdminInfo>();
            try
            {
                ListUsersRequest listUsersRequest = new ListUsersRequest
                {
                    UserPoolId = _userPoolId
                };

                ListUsersResponse response = await _provider.ListUsersAsync(listUsersRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = false;

                    List<UserType> userType = response.Users;

                    foreach (UserType item in userType)
                    {

                        UserAdminInfo user = new UserAdminInfo
                        {
                            Id = item.Attributes.Exists(x => x.Name == "sub") ? item.Attributes.Find(x => x.Name == "sub").Value.ToString() : string.Empty,
                            Email = item.Attributes.Exists(x => x.Name == "email") ? item.Attributes.Find(x => x.Name == "email").Value.ToString() : string.Empty,
                            Name = item.Attributes.Exists(x => x.Name == "name") ? item.Attributes.Find(x => x.Name == "name").Value.ToString() : string.Empty,
                            GivenName = item.Attributes.Exists(x => x.Name == "given_name") ? item.Attributes.Find(x => x.Name == "given_name").Value.ToString() : string.Empty,
                            FamilyName = item.Attributes.Exists(x => x.Name == "family_name") ? item.Attributes.Find(x => x.Name == "family_name").Value.ToString() : string.Empty,
                            EmailVerified = item.Attributes.Exists(x => x.Name == "email_verified") ? Convert.ToBoolean(item.Attributes.Find(x => x.Name == "email_verified").Value.ToString()) : false,
                            Status = item.UserStatus,
                            Enable = item.Enabled
                        };

                        listOfUsers.Add(user);
                    }

                    cognitoResponse.Content = listOfUsers;
                }
                else
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = true;
                    cognitoResponse.Message = "Failed to retrieve users.";
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// An administrator can change the password of a user's account in the user pool.
        /// </summary>
        /// <param name="username">Username or Email of user whose password is to be changed</param>
        /// <param name="newpassword">New password</param>
        /// <param name="permanent">Will the new password be temporary or permanent</param>
        /// <returns>CognitoResponse</returns>
        internal async Task<CognitoResponse> AdminSetUserPassword(string username, string newpassword, bool permanent)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                AdminSetUserPasswordRequest adminSetUserPasswordRequest = new AdminSetUserPasswordRequest
                {
                    Password = newpassword,
                    Username = username,
                    Permanent = permanent,
                    UserPoolId = _userPoolId
                };

                AdminSetUserPasswordResponse response = await _provider.AdminSetUserPasswordAsync(adminSetUserPasswordRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.HasError = false;
                    cognitoResponse.Message = response.ResponseMetadata.ToString();
                }
                else
                {
                    cognitoResponse.HasError = true;
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                    cognitoResponse.Message = response.ResponseMetadata.ToString();
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }
        #endregion


        /// <summary>
        /// Resend verification code to email address on file for the user.
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>CognitoResponse</returns>
        internal async Task<CognitoResponse> ResendVerificationCode(string username)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                CognitoUser user = new CognitoUser(username, this._userPoolClientAppId, _cognitoUserPool, _provider, this._userPoolClientAppSecret);
                await user.ResendConfirmationCodeAsync();

                cognitoResponse.Code = (int)HttpStatusCode.OK;
                cognitoResponse.Message = "Success";

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// A user who has been issued an access token can update their password.
        /// </summary>
        /// <param name="accessToken">User's access token</param>
        /// <param name="oldPassword">User's current password</param>
        /// <param name="newPassword">User's new password</param>
        /// <returns>CognitoResponse</returns>
        internal async Task<CognitoResponse> UserChangePassword(string accessToken, string oldPassword, string newPassword)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                ChangePasswordRequest changePassRequest = new ChangePasswordRequest()
                {
                    PreviousPassword = oldPassword,
                    ProposedPassword = newPassword,
                    AccessToken = accessToken
                };

                ChangePasswordResponse response = await _provider.ChangePasswordAsync(changePassRequest);


                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                }
                else
                {
                    cognitoResponse.HasError = true;
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }

        /// <summary>
        /// A user who has been issued an access token can update their attributes
        /// </summary>
        /// <param name="accessToken">User's</param>
        /// <param name="attributeList">List of AttributeType objects</param>
        /// <returns>CognitoResponse</returns>
        internal async Task<CognitoResponse> UserUpdateAttributes(string accessToken, List<AttributeType> attributeList)
        {
            cognitoResponse = new CognitoResponse();

            try
            {
                UpdateUserAttributesRequest updateUserAttributesRequest = new UpdateUserAttributesRequest
                {
                    AccessToken = accessToken,
                    UserAttributes = attributeList
                };

                UpdateUserAttributesResponse response = await _provider.UpdateUserAttributesAsync(updateUserAttributesRequest);

                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                }
                else
                {
                    cognitoResponse.HasError = true;
                    cognitoResponse.Code = (int)response.HttpStatusCode;
                }

                return cognitoResponse;
            }
            catch (Exception ex)
            {
                cognitoResponse.HasError = true;
                cognitoResponse.Code = (int)ErrorHelper.GetHttpErrorCode(ex.InnerException);
                cognitoResponse.Message = ex.Message;

                return cognitoResponse;
            }
        }


        /// <summary>
        ///  Computes the secret hash for the user pool using the corresponding userID, clientID, and client secret
        /// </summary>
        /// <param name="userID">The current userID</param>
        /// <param name="clientID">The clientID for the client being used</param>
        /// <param name="clientSecret">The client secret of the corresponding clientID</param>
        /// <returns>Returns the secret hash for the user pool using the corresponding 
        /// userID, clientID, and client secret</returns>
        private static string GetUserPoolSecretHash(string userID, string clientID, string clientSecret)
        {
            string message = userID + clientID;
            byte[] keyBytes = Encoding.UTF8.GetBytes(clientSecret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            HMACSHA256 hmacSha256 = new HMACSHA256(keyBytes);
            byte[] hashMessage = hmacSha256.ComputeHash(messageBytes);

            return Convert.ToBase64String(hashMessage);
        }


        public class CognitoResponse
        {
            public CognitoResponse()
            {
                HasError = false;
            }

            public bool HasError { get; set; }
            public string Message { get; set; }
            public int Code { get; set; }

            public object Content { get; set; }

            public static implicit operator Task<object>(CognitoResponse v)
            {
                throw new NotImplementedException();
            }
        }

    }


}