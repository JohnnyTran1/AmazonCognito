using Amazon.CognitoIdentityProvider.Model;
using AmazonCognito.Models;
using AmazonCognito.Models.Authenticate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace AmazonCognito.Account
{
    public partial class ProfilePage : System.Web.UI.Page
    {
        private ToastNotification notification = new ToastNotification();

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                if (Session["User_Object"] != null)
                {
                    User userObj = (User)Session["User_Object"];
                    PopulateForm(userObj);
                }
                else
                {
                    //Display error banner
                    notification = new ToastNotification
                    {
                        Heading = "Error",
                        Text = $"An error occurred while retrieving your information. Please try again later.",
                        Icon = ToastIcon.error,
                        HideAfter = 5,
                        AutoHide = false
                    };
                    (this.Master as AdminMaster).DisplayToastNotification(notification);
                }
            }
        }

        protected void PopulateForm(User user)
        {
            //Populate Labels
            lblName.Text = user.Name;
            lblEmail.Text = user.Email;


            //Populate Textbox fields 
            hiddenFieldID.Value = user.Id;
            txtFirstName.Text = user.GivenName;
            txtMiddleName.Text = user.MiddleName;
            txtLastName.Text = user.FamilyName;
            txtEmail.Text = user.Email;
            txtNickName.Text = user.Nickname;
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            CognitoHelper _helper = new CognitoHelper();        
            Token token = (Token) Session["AuthenticationToken"];


            /*
             * A false return means no errors were found
             */ 
            if (!ValidateRequiredFields()) 
            {
                User user = new User
                {
                    Id = hiddenFieldID.Value,
                    Email = txtEmail.Text,
                    FamilyName = txtLastName.Text,
                    GivenName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Nickname = txtNickName.Text,
                    Name = $"{txtFirstName.Text} {txtLastName.Text}"
                };

                List<AttributeType> attributeList = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = user.Email },
                    new AttributeType { Name = "family_name", Value = user.FamilyName },
                    new AttributeType { Name = "given_name", Value = user.GivenName },
                    new AttributeType { Name = "middle_name", Value = user.MiddleName },
                    new AttributeType { Name = "nickname", Value = user.Nickname },
                    new AttributeType { Name = "name", Value = user.Name }
                };

                //Update Attributes
                CognitoHelper.CognitoResponse response = await _helper.UserUpdateAttributes(token.Access_Token, attributeList);

                if(response != null && !response.HasError)
                {
                    IdentityHelper.SignIn(user, token, HttpContext.Current.User.Identity.GetRole().ToArray());
                    PopulateForm(user);

                    //User account updated
                    notification = new ToastNotification
                    {
                        Heading = "Success",
                        Text = $"{user.Name}'s account was updated.",
                        Icon = ToastIcon.success,
                        HideAfter = 7,
                        AutoHide = true
                    };
                }
                else
                {
                    //User account failed to update
                    notification = new ToastNotification
                    {
                        Heading = "Error",
                        Text = $"{user.Name}'s account failed to update. <hr/> Exception: {response.Message}.",
                        Icon = ToastIcon.error,
                        AutoHide = false
                    };
                }                 
            } 
            else
            {
                //Error occurred with user input
                notification = new ToastNotification
                {
                    Heading = "Error",
                    Text = $"Please correct any errors and try to save again.",
                    Icon = ToastIcon.error,
                    HideAfter = 5,
                    AutoHide = false
                };
            }

            (this.Master as AdminMaster).DisplayToastNotification(notification);
        }

        private bool ValidateRequiredFields()
        {
            //Clear any previous errors
            divErrorAlert.Visible = false;
            lblErrorMessage.Text = string.Empty;

            //Build error message
            StringBuilder errorMessage = new StringBuilder();
            bool hasError = false;

            //First Name
            if(string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                errorMessage.AppendLine("- First name cannot be left blank <br/>");
                hasError = true;
            }

            //Skip middle because not everyone has a middle name

            //Last Name
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                errorMessage.AppendLine("- Last name cannot be left blank <br/>");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                errorMessage.AppendLine("- Email cannot be left blank <br/>");
                hasError = true;
            }
            else if(!Validation.ValidateAnyEmail(txtEmail.Text))
            {
                errorMessage.AppendLine("- Email is not a valid format <br/>");
                hasError = true;
            }


            if(hasError)
            {
                //Display error message
                divErrorAlert.Visible = true;
                lblErrorMessage.Text = "<b>Error:</b> Please review the following errors:<br/><br/>" + errorMessage.ToString();
            }

            return hasError;
        }
    }
}