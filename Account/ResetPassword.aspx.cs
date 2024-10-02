using System;
using System.Collections.Generic;
using AmazonCognito.Models;
using AmazonCognito.Models.Authenticate;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace AmazonCognito
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewPassword.Value) && !string.IsNullOrWhiteSpace(txtNewPasswordRepeat.Value))
            {
                if (txtNewPassword.Value == txtNewPasswordRepeat.Value)
                {
                    if (Session["ResetPasswordUsername"] != null)
                    {
                        CognitoHelper _helper = new CognitoHelper();

                        string username = Session["ResetPasswordUsername"].ToString();
                        string session = Session["ResetPasswordUserSession"].ToString();
                        string newPassword = txtNewPassword.Value;

                        var response = await _helper.NewPasswordRequiredUpdate(username, session, newPassword);

                        if (response != null && (!response.HasError && response.IsAuthenticated))
                        {
                           //User user = await _helper.GetUserDetails(response.AccessToken);
                            Session.Remove("ResetPasswordUsername");
                           // SignIn(user, response.AccessToken, new string[] { "Admin"});
                           // Response.Redirect("Dashboard.aspx");
                            //lblSuccess.Text = "Password has been reset. Please return to the login page and sign in";
                            //alertMessageSuccess.Visible = true;
                            //alertMessageError.Visible = false;


                            //Or
                            Response.Redirect("../Login.aspx?reset=true");


                        }
                    }
                    else
                    {
                        alertMessageSuccess.Visible = false;
                        alertMessageError.Visible = true;
                        lblError.Text = "User could not be determined";
                    }
                }
                else
                {
                    alertMessageSuccess.Visible = false;
                    alertMessageError.Visible = true;
                    lblError.Text = "Passwords do not match";
                }
            }
            else
            {
                alertMessageSuccess.Visible = false;
                alertMessageError.Visible = true;
                lblError.Text = "All fields are required";
            }
        }

        private void SignIn(User user, string token, params string[] roles)
        {
            Session["Authenticated"] = true;
            Session["User_Object"] = user;
            Session["User_Id"] = user.Id;
            Session["User_Name"] = user.Name;
            Session["User_FamilyName"] = user.FamilyName;
            Session["User_GivenName"] = user.GivenName;
            Session["User_Email"] = user.Email;
            Session["User_Roles"] = roles;
            Session["AuthenticationToken"] = token;
        }
    }
}