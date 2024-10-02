using AmazonCognito.Models;
using System;
using System.Web.Security.AntiXss;

namespace AmazonCognito
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string message = string.Empty;

                if (Request.QueryString["passwordreset"] != null)
                {
                    string passwordRest = Request.QueryString["passwordreset"];

                    if (bool.TryParse(passwordRest, out _))
                    {
                        alertMessage.Visible = true;
                        message = "<b>Success:</b> Your password has successfully be reset. Please login.";

                        Request.QueryString.Clear();
                        DisplayAlertMessage("success", message);
                    }
                }
                else if (Request.QueryString["msg"] != null)
                {
                    message = Request.QueryString["msg"];

                    if (message.Equals("expired"))
                    {
                        message = "<b>Warning:</b> Your access token expired. Please login again.";
                        DisplayAlertMessage("success", message);
                    }
                }

                //If cookie found then set email address and check remember me
                if (Request.Cookies["UserName"] != null)
                {
                    txtEmail.Value = AntiXssEncoder.HtmlEncode(Request.Cookies["UserName"].Value, false);
                    chkRememberMe.Checked = true;
                }
            }
        }

        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            CognitoHelper _helper = new CognitoHelper();
            string username = txtEmail.Value;
            string password = txtpassword.Value;

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                //This is for the Remember me Cookie functionality. It will only remember the email address.
                if (chkRememberMe.Checked)
                {
                    Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(10);
                    Response.Cookies["UserName"].Value = AntiXssEncoder.HtmlEncode(txtEmail.Value.Trim(), false);
                    Response.Cookies["UserName"].Secure = true;
                }
                else
                {
                    Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(-1);
                }

                var response = await _helper.ValidateUserCredentials(username, password);

                if (response != null && (!response.HasError && response.IsAuthenticated))
                {
                    //Validate this token is from AWS
                    Validation.ValidateSignatureOfAccessToken(response.AccessToken, await _helper.GetPublicKeys);
                    Validation.IsTokenExpired(response.AccessToken);

                    var user = await _helper.GetUserDetails(response.AccessToken);

                    if (user != null)
                    {
                        /*
                         * Hard coded in roles, but this information would be retrieved from the database.
                         * This may also be optional as it depends on requirements of your project
                         */
                        var roles = new[] { "Admin" };


                        /*
                         * This SignIn method just takes the user information and sets it to Session objects  
                         * that can but used throughout the application.
                         */
                        IdentityHelper.SignIn(user, response.Token, roles);

                        //Redirect to Dashboard
                        Response.Redirect("Dashboard.aspx", false);
                    }
                    else
                    {
                        //Display error message
                        alertMessageError.Visible = true;
                        lblError.Text = "An unexpected error has occurred. Please try again later.";
                    }
                }
                else if (response.ChallengeNameType == Amazon.CognitoIdentityProvider.ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    Session["ResetPasswordUsername"] = username;
                    Session["ResetPasswordUserSession"] = response.SessionID;
                    Response.Redirect("Account/ResetPassword.aspx", false);
                }
                else
                {
                    //Display error message
                    alertMessageError.Visible = true;
                    lblError.Text = response.Message;
                }
            }
            else
            {
                //Display error message
                alertMessageError.Visible = true;
                lblError.Text = "Email Address and/or Password cannot be blank";
            }
        }

        private void DisplayAlertMessage(string alertType, string message)
        {
            string script = "window.onload = function() { DisplayAlert('" + alertType + "','" + message + "'); };";

            ClientScript.RegisterStartupScript(this.GetType(), "OpenModal", script, true);
        }

    }
}