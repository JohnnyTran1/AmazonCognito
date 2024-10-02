using AmazonCognito.Models;
using AmazonCognito.Models.Account;
using System;

namespace AmazonCognito
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private readonly CognitoHelper _helper = new CognitoHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                panelResetEmail.Visible = true;
                panelNewPassword.Visible = false;
            }
        }

        protected async void btnReset_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txtEmail.Value))
            {
                if(Validation.ValidateAnyEmail(txtEmail.Value))
                {
                    alertMessageError.Visible = false;
                    lblError.Text = string.Empty;

                    string email = txtEmail.Value;

                    var response = await _helper.ForgotPassword(email);

                    if (response != null && !response.HasError) {

                        Session["EmailResetToBeConfirmed"] = email;
                        alertMessage.Visible = true;
                        lblSuccess.Text = $"<b>Success:</b> A email was sent to the following address {response.Message}";
                        panelResetEmail.Visible = false;
                        panelNewPassword.Visible = true;
                    }
                    else
                    {
                        //Email is invalid format
                        alertMessageError.Visible = true;
                        lblError.Text = $"<b>Error:</b> {response.Message}";
                    }
                }
                else
                {
                    //Email is invalid format
                    alertMessageError.Visible = true;
                    lblError.Text = "<b>Error:</b> Email format is invalid";
                }
            }
            else
            {
                //Field blank
                alertMessageError.Visible = true;
                lblError.Text = "<b>Error:</b> Email is required";
            }
        }

        protected async void btnResendCode_Click(object sender, EventArgs e)
        {
            if (Session["EmailResetToBeConfirmed"] != null)
            {
                string email = Session["EmailResetToBeConfirmed"].ToString();

                if (!string.IsNullOrWhiteSpace(email))
                {
                    CognitoHelper.CognitoResponse response = await _helper.ResendVerificationCode(email);

                    if(response != null && !response.HasError)
                    {
                        alertMessage.Visible = true;
                        lblSuccess.Text = "Code has been resent";
                    }
                    else
                    {
                        alertMessageError.Visible = true;
                        lblError.Text = "Code could not be resent.";
                    }
                }
                else
                {
                    lblError.Text = "Email could not be determined.";
                    alertMessageError.Visible = true;
                }
            }
            else
            {
                lblError.Text = "Email could not be determined.";
                alertMessageError.Visible = true;
            }
        }

        protected async void btnVerifySubmit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtVerificationCode.Value) && !string.IsNullOrWhiteSpace(txtNewPassword.Value) && !string.IsNullOrWhiteSpace(txtRepeatNewPassword.Value))
            {
                if (txtNewPassword.Value == txtRepeatNewPassword.Value)
                {
                    if (Session["EmailResetToBeConfirmed"] != null)
                    {
                        string email = Session["EmailResetToBeConfirmed"].ToString();
                        string newPassword = txtNewPassword.Value;
                        string userProvidedCode = txtVerificationCode.Value;

                        if (int.TryParse(userProvidedCode, out _))
                        {
                            txtVerificationCode.Value = string.Empty;

                            CognitoHelper.CognitoResponse response = await _helper.ConfirmForgotPassword(email, userProvidedCode, newPassword);

                            if (response != null & !response.HasError)
                            {
                                Session.Remove("EmailResetToBeConfirmed");
                                Response.Redirect("../Login.aspx?passwordreset=true");
                            }
                            else
                            {
                                alertMessageError.Visible = true;
                                lblError.Text = "The account verification failed. Please try again later.";
                            }
                        }
                        else
                        {
                            alertMessageError.Visible = true;
                            lblError.Text = "The code field must be a integer.";
                        }
                    }
                    else
                    {
                        alertMessageError.Visible = true;
                        lblError.Text = "Email address could not be determined.";
                    }
                }
                else
                {
                    alertMessageError.Visible = true;
                    lblError.Text = "Passwords do not match";
                }
            }
            else
            {
                alertMessageError.Visible = true;
                lblError.Text = "All fields are required.";
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            txtEmail.Value= string.Empty;
            txtNewPassword.Value = string.Empty;
            txtRepeatNewPassword.Value = string.Empty; 
            txtVerificationCode.Value = "0";
            panelNewPassword.Visible = false;
            panelResetEmail.Visible = true;

            alertMessage.Visible = false;
            lblSuccess.Text = string.Empty;

            alertMessageError.Visible = false;
            lblError.Text = string.Empty;
        }
    }
}