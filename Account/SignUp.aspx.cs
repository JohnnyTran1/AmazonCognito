using AmazonCognito.Models;
using AmazonCognito.Models.Account;
using System;
using System.Text;

namespace AmazonCognito
{
    public partial class SignUp : System.Web.UI.Page
    {
        private readonly CognitoHelper _helper = new CognitoHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                registerCheck.Checked = false;
            }
        }

        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateUserInput())
            {
                if (registerCheck.Checked)
                {
                    if (registerPassword.Value == registerRepeatPassword.Value)
                    {
                        CognitoHelper.CognitoResponse response = await _helper.SignUpUser(registerEmail.Value, registerPassword.Value, registerEmail.Value, registerFirstName.Value, registerLastName.Value);

                        if (response != null && !response.HasError)
                        {
                            alertMessageError.Visible = false;
                            alertMessage.Visible = true;
                            lblSignupSuccess.Text = "<b>Success</b>: You have successfully created an account. Please click the button to verify your email address.";
                            Session["EmailToBeConfirmed"] = registerEmail.Value;
                            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowVerificationModal();", true);
                            ClearFields();
                        }
                        else
                        {
                            alertMessage.Visible = false;
                            alertMessageError.Visible = true;
                            lblSignupError.Text = response.Message;
                        }
                    }
                    else
                    {
                        alertMessage.Visible = false;
                        alertMessageError.Visible = true;
                        lblSignupError.Text = "<b>Error:</b>: Password do not match.";
                    }
                }
                else
                {
                    alertMessageError.Visible = true;
                    lblSignupError.Text = "Agreement has not be accepted.";
                }
            }
            else
            {
                alertMessage.Visible = false;
                alertMessageError.Visible = true;
            }
        }

        private bool ValidateUserInput()
        {
            bool hasError = false;
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.Append("<b>Error: Please review the following issues:<br/><br/>");

            if (string.IsNullOrWhiteSpace(registerFirstName.Value))
            {
                errorMessage.Append("- First Name is required<br/>");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(registerLastName.Value))
            {
                errorMessage.Append("- Last Name is required<br/>");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(registerEmail.Value))
            {
                errorMessage.Append("- Email is required<br/>");
                hasError = true;
            }
            else if (!Validation.ValidateAnyEmail(registerEmail.Value))
            {
                errorMessage.Append("- Email format is invalid<br/>");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(registerPassword.Value))
            {
                errorMessage.Append("- Password is required<br/>");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(registerRepeatPassword.Value))
            {
                errorMessage.Append("- Repeat Password is required");
                hasError = true;
            }

            if (hasError)
            {
                lblSignupError.Text = errorMessage.ToString();
            }

            return hasError;
        }

        private void ClearFields()
        {
            registerFirstName.Value = String.Empty;
            registerLastName.Value = String.Empty;
            registerEmail.Value = String.Empty;
            registerPassword.Value = String.Empty;
            registerRepeatPassword.Value = String.Empty;
            registerCheck.Checked = false;
        }

        protected void btnLoginPage_Click(object sender, EventArgs e)
        {
            Response.Redirect("../Login.aspx");
        }

        protected async void btnVerifySubmit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCode.Text))
            {
                string userProvidedCode = txtCode.Text;

                if (int.TryParse(userProvidedCode, out _))
                {
                    txtCode.Text = string.Empty;
                    string email = Session["EmailToBeConfirmed"].ToString();

                    CognitoHelper.CognitoResponse response = await _helper.ConfirmSignup(email, userProvidedCode);

                    if(response != null && response.HasError == false)
                    {
                        lblSignupSuccess.Text = "<b>Success</b>: You have successfully confirmed your account. Please go back to the login page to sign-in.";
                        Session.Remove("EmailToBeConfirmed");

                        divVerificationModal.Visible = false;
                        lblVerificationModalError.Text = string.Empty;
                    }
                    else
                    {
                        divVerificationModal.Visible = true;
                        lblVerificationModalError.Text = "The account verification failed. Please try again later.";
                    }
                }
                else
                {
                    divVerificationModal.Visible = true;
                    lblVerificationModalError.Text = "The code field must be a integer.";
                }
            }
            else
            {
                divVerificationModal.Visible = true;
                lblVerificationModalError.Text = "The code field is required.";
            }
        }

        protected async void btnResendCode_Click(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowVerificationModal();", true);
            divVerificationModelSuccess.Visible = false;
            divVerificationModal.Visible = false;

            if (Session["EmailToBeConfirmed"] != null)
            {
                string email = Session["EmailToBeConfirmed"].ToString();

                if (!string.IsNullOrWhiteSpace(email))
                {
                    CognitoHelper.CognitoResponse response = await _helper.ResendVerificationCode(email);

                    if(response != null && response.HasError == false)
                    {
                        divVerificationModelSuccess.Visible = true;
                        lblVerificationModalSuccess.Text = "Code has been resent";
                    }
                    else
                    {
                        lblVerificationModalError.Text = "Code could not be resent.";
                        divVerificationModal.Visible = true;
                    }
                }
                else
                {
                    lblVerificationModalError.Text = "Email could not be determined.";
                    divVerificationModal.Visible = true;
                }
            }
            else
            {
                lblVerificationModalError.Text = "Email could not be determined.";
                divVerificationModal.Visible = true;
            }
        }
    }
}