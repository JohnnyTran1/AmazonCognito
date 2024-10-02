using AmazonCognito.Models;
using AmazonCognito.Models.Authenticate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AmazonCognito
{
    public partial class Oauth : System.Web.UI.Page
    {
        protected string Parameters;
        private readonly CognitoHelper _cognitoHelper = new CognitoHelper();
        protected async void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                var CognitoUrl = $"{_cognitoHelper.OauthDomainUrl}/login?client_id={_cognitoHelper.UserPoolClientAppId}&response_type=code&scope=email+openid+profile+aws.cognito.signin.user.admin&redirect_uri={_cognitoHelper.OauthRedirectUri}oauth.aspx";
                Session["loginWith"] = "cognito";
                Response.Redirect(CognitoUrl, false);
            }


            if ((Session.Contents.Count > 0) && (Session["loginWith"] != null) && (Session["loginWith"].ToString() == "cognito"))
            {
                try
                {
                    var url = Request.Url.Query;
                    if (url != "")
                    {
                        string queryString = url.ToString();
                        char[] delimiterChars = { '=' };
                        string[] words = queryString.Split(delimiterChars);
                        string code = words[1];

                        if (code != null)
                        {
                            Token token = GetAccessToken(code);
                            if (token != null)
                            {
                                User user = await _cognitoHelper.GetUserDetails(token.Access_Token);

                                if (user != null)
                                {
                                    //Used to know where to return the user
                                    Session["UsedOauth"] = true;

                                    //Hard coded in role, but this information would be retrieved from the database and dependent on requirements of your project
                                    var roles = new[] { "Admin" };

                                    /*
                                     * This SignIn method just takes the user information and sets it to Session objects  
                                     * that can but used throughout the application.
                                     */
                                    IdentityHelper.SignIn(user, token, roles);
                                    Session.Remove("loginWith");
                                    Response.Redirect("Dashboard.aspx", false);
                                }
                                else
                                {
                                    //Display error message
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        private Token GetAccessToken(string code)
        {
            try
            {
                //get the access token 
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"{_cognitoHelper.OauthDomainUrl}/oauth2/token");
                webRequest.Method = "POST";

                var clientId = _cognitoHelper.UserPoolClientAppId;
                var clientSecret = _cognitoHelper.UserPoolClientAppSecret;

                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                               .GetBytes(clientId + ":" + clientSecret));
                webRequest.Headers.Add("Authorization", "Basic " + encoded);


                String parameters = $"code={code}&client_id={_cognitoHelper.UserPoolClientAppId}&redirect_uri={_cognitoHelper.OauthRedirectUri}oauth.aspx&grant_type=authorization_code";
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;
                Stream postStream = webRequest.GetRequestStream();
                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                WebResponse response = webRequest.GetResponse();
                postStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(postStream);
                string responseFromServer = reader.ReadToEnd();

                Token token = JsonConvert.DeserializeObject<Token>(responseFromServer);

                if (token != null)
                {
                    return token;
                }
                else
                    throw new Exception("token is null");
            }
            catch (Exception)
            {
                return null;
            }
        }

        /*
         * Replaced this method with the one in IdentityHelper class
         */ 
        //private void SignIn(User user, Token token, params string[] roles)
        //{
        //    Session["Authenticated"] = true;
        //    Session["User_Object"] = user;
        //    Session["User_Id"] = user.Id;
        //    Session["User_Name"] = user.Name;
        //    Session["User_FamilyName"] = user.FamilyName;
        //    Session["User_GivenName"] = user.GivenName;
        //    Session["User_Email"] = user.Email;
        //    Session["User_Roles"] = roles;
        //    Session["AuthenticationToken"] = token;
        //    Session["UsedOauth"] = true;
        //}
    }
}