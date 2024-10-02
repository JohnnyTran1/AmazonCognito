using AmazonCognito.Models;
using AmazonCognito.Models.Authenticate;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace AmazonCognito
{
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        /*Security Variables - Preventing Cross-Site Forgery after successfully Login*/
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        /*Global CognitoHelper object */
        private readonly CognitoHelper _helper = new CognitoHelper();

        /*Loading events handled for added security*/
        protected void Page_Init(object sender, EventArgs e)
        {
            //First, check for the existence of the Anti-XSS cookie
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;

            //If the CSRF cookie is found, parse the token from the cookie.
            //Then, set the global page variable and view state user
            //key. The global variable will be used to validate that it matches 
            //in the view state form field in the Page.PreLoad method.
            if (requestCookie != null
                && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                //Set the global token variable so the cookie value can be
                //validated against the value in the view state form field in
                //the Page.PreLoad method.
                _antiXsrfTokenValue = requestCookie.Value;

                //Set the view state user key, which will be validated by the
                //framework during each request
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            //If the CSRF cookie is not found, then this is a new session.
            else
            {
                //Generate a new Anti-XSRF token
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");

                //Set the view state user key, which will be validated by the
                //framework during each request
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                //Create the non-persistent CSRF cookie
                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    //Set the HttpOnly property to prevent the cookie from
                    //being accessed by client side script
                    HttpOnly = true,

                    //Add the Anti-XSRF token to the cookie value
                    Value = _antiXsrfTokenValue
                };

                //If we are using SSL, the cookie should be set to secure to
                //prevent it from being sent over HTTP connections
                if (FormsAuthentication.RequireSSL &&
                    Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }

                //Add the CSRF cookie to the response
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;


            /*Check if the user is Authenticated before loading the page*/
            /*Makes it faster and more secured by preventing running further code*/
            if (IdentityHelper.IsAuthenticated() == false)
            {
                if (Session["UsedOauth"] != null && (bool)Session["UsedOauth"] == true)
                {
                    Session.Remove("UsedOauth");
                    Session.Abandon();
                    Response.Redirect("oauth.aspx");
                }
                else
                {
                    Session.Abandon();
                    Response.Redirect("Login.aspx");
                }
            }
            else if (Validation.IsTokenExpired((Token)Session["AuthenticationToken"]) == false)
            {
                if (Session["UsedOauth"] != null && (bool)Session["UsedOauth"] == true)
                {
                    Session.Remove("UsedOauth");
                    Session.Abandon();
                    Response.Redirect("oauth.aspx");
                }
                else
                {
                    Session.Abandon();
                    Response.Redirect("Login.aspx?msg=expired");
                }
            }
            /*Check if the user is allowed to access the page */
            else if (!IdentityHelper.IsAuthorized(new string[] { "Admin" }))
            {
                Response.Redirect("unauthorized.aspx", false);
            }

        }
        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            //During the initial page load, add the Anti-XSRF token and user
            //name to the ViewState
            if (!IsPostBack)
            {
                //Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;

                //If a user name is assigned, set the user name
                ViewState[AntiXsrfUserNameKey] =
                       Context.User.Identity.Name ?? String.Empty;
            }
            //During all subsequent post backs to the page, the token value from
            //the cookie should be validated against the token in the view state
            //form field. Additionally user name should be compared to the
            //authenticated users name
            else
            {
                //Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] !=
                         (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of " +
                                        "Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected async void btnLogout_Click(object sender, EventArgs e)
        {
            if (Session["AuthenticationToken"] != null)
            {
                if (Session["UsedOauth"] == null || (bool)Session["UsedOauth"] == false)
                {
                    Token token = (Token)Session["AuthenticationToken"];

                    CognitoHelper.CognitoResponse response = await _helper.SignOut(token.Access_Token);

                    if (response != null && !response.HasError)
                    {
                        Context.GetOwinContext().Authentication.SignOut();
                        Session.Abandon();
                        Session.Clear();
                        Response.Redirect("Login.aspx", false);
                    }
                    else
                    {
                        //Failed to signout throw error message.
                        ToastNotification notification = new ToastNotification
                        {
                            Heading = "Error",
                            Text = $"An Error occurred while attempting to signout. <hr/> Exception: {response.Message}.",
                            Icon = ToastIcon.error,
                            HideAfter = 7,
                            AutoHide = false
                        };

                        DisplayToastNotification(notification);
                    }
                }
                else
                {
                    var logoutUrl = $"{_helper.OauthDomainUrl}/logout?client_id={_helper.UserPoolClientAppId}&response_type=code&redirect_uri={_helper.OauthRedirectUri}oauth.aspx";
                    Session.Abandon();
                    Session.Clear();
                    Response.Redirect(logoutUrl, false);
                }             
            }
        }


        protected async void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            divUpdatePasswordModalError.Visible = false;
            lblUpdatePasswordModalError.Text = string.Empty;

            Token token = (Token)Session["AuthenticationToken"];

            if (token != null && !string.IsNullOrWhiteSpace(token.Access_Token))
            {
                if (!string.IsNullOrWhiteSpace(txtCurrentPassword.Value) && !string.IsNullOrWhiteSpace(txtCurrentPassword.Value) && !string.IsNullOrWhiteSpace(txtCurrentPassword.Value))
                {
                    if (txtNewPassword.Value == txtNewPasswordRepeat.Value)
                    {
                        CognitoHelper.CognitoResponse response = await _helper.UserChangePassword(token.Access_Token, txtCurrentPassword.Value, txtNewPassword.Value);

                        if(response != null && !response.HasError)
                        {
                            //Password changed worked
                            ToastNotification notification = new ToastNotification
                            {
                                Heading = "Success",
                                Text = "Your password has been updated.",
                                Icon = ToastIcon.success,
                                HideAfter = 7,
                                AutoHide = true
                            };

                            DisplayToastNotification(notification);

                            //Clear out the fields
                            txtNewPassword.Value = string.Empty;
                            txtNewPasswordRepeat.Value = string.Empty;
                            txtCurrentPassword.Value = string.Empty;    
                        }
                        else
                        {
                            //Password change failed
                            divUpdatePasswordModalError.Visible = true;
                            lblUpdatePasswordModalError.Text = "An unexpected error has occurred. Please try again later.";
                            DisplayUpdatePasswordModal();
                        }
                    }
                    else
                    {
                        //Password do not match
                        divUpdatePasswordModalError.Visible = true;
                        lblUpdatePasswordModalError.Text = "The new passwords do not match.";
                        DisplayUpdatePasswordModal();
                    }
                }
                else
                {
                    //One or more required fields is empty
                    divUpdatePasswordModalError.Visible = true;
                    lblUpdatePasswordModalError.Text = "Please make sure all fields are populated.";
                    DisplayUpdatePasswordModal();
                }
            }         
        }


        private void DisplayUpdatePasswordModal()
        {
            string script = "window.onload = function() { DisplayUpdatePasswordModal(); };";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenPasswordModal", script, true);
        }


        public void DisplayToastNotification(ToastNotification notification)
        {
            string script = "window.onload = function() { DisplayNotification('" + notification.Icon + "','" + notification.Heading + "','" + notification.Text.Replace("'", @"\'") + "','" + (notification.HideAfter * 1000) + "','" + notification.AutoHide + "'); };";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenModal", script, true);
        }
    }
}