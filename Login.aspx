<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="AmazonCognito.Login" Async="true" %>

<!DOCTYPE html lang="en">
<!-- OPTION 1: Create your own login, forgot password, reset password pages -->

<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <title>Login</title>

        <!-- Required meta tags -->
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

    <!-- Bootstrap -->
    <link href="Library/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <script src="Library/bootstrap/js/bootstrap.bundle.min.js"></script>
    <link href="Library/bootstrap/bootstrap-icons/font/bootstrap-icons.min.css" rel="stylesheet" />
    <script src="Library/jquery/jquery.min.js"></script>

    <!-- Custom stylesheet for login page -->
    <link href="Library/login.css" rel="stylesheet" />

    <script type="text/javascript">

        function DisplayAlert(alertType, message) {
            $('#alertMessage').hide();
            $('#lblMessage').html(message);
            $('#alertMessage').attr('class', 'alert alert-dismissible  alert-' + alertType);
            $('#alertMessage').show("size");

            if (alertType == 'success') {
                setTimeout(function () { $('#alertMessage').hide(); }, 7000);
            }
        }

    </script>
</head>
<body>
    <div class="container">
        <div style="margin-top: 10%">
            <section class="pb-4">
                <div class="bg-white border rounded-5">
                    <div class="col-md-8 offset-2" style="margin-top: 15px;">
                        <div runat="server" id="alertMessage" class="" role="alert" style="display:none">
                            <asp:Label runat="server" ID="lblMessage" ClientIDMode="Static"></asp:Label>
                            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                        </div>
                    </div>
                    <section class="w-100 p-4 d-flex justify-content-center pb-4">
                        <form runat="server" style="width: 22rem;">
                            <!-- Email input -->
                            <div class="form-floating mb-4">
                                <input runat="server" type="email" id="txtEmail" class="form-control" placeholder="Enter email" name="email" />
                                <label for="txtEmail">Email address</label>
                            </div>

                            <!-- Password input -->
                            <div class="form-floating mb-4">
                                <input runat="server" type="password" id="txtpassword" class="form-control" placeholder="Enter password" name="password" />
                                <label for="txtpassword">Password</label>
                            </div>

                            <!-- 2 column grid layout for inline styling -->
                            <div class="row mb-4">
                                <div class="col d-flex justify-content-center">
                                    <!-- Checkbox -->
                                    <div class="form-check">
                                        <asp:CheckBox runat="server" CssClass="" ID="chkRememberMe" AutoPostBack="false" />
                                        <label class="form-check-label" for="form2Example31">Remember me </label>
                                    </div>
                                </div>

                                <div class="col">
                                    <!-- Simple link -->
                                    <a href="Account/ForgotPassword.aspx">Forgot password?</a>
                                </div>
                            </div>

                            <!-- Error Message Div -->
                            <div runat="server" id="alertMessageError" class="alert alert-danger" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblError"></asp:Label>
                            </div>

                            <!-- Submit button -->
                            <asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary btn-block mb-4" Style="width: 100%" Text="Sign in" OnClick="btnSubmit_Click" />

                            <!-- Register buttons -->
                            <div class="text-center">
                                <p>Not a member? <a href="Account/SignUp.aspx">Register</a></p>
                            </div>
                            <div class="col-md-12 ">
                                <div class="login-or">
                                    <hr class="hr-or">
                                    <span class="span-or">or</span>
                                </div>
                            </div>
                            <div class="col-md-12 mb-3">
                                <p class="text-center">

                                    <a href="Oauth.aspx" class="btn btn-secondary">Use Cognito Oauth</a>
                                </p>
                            </div>
                        </form>
                    </section>
                </div>
            </section>
        </div>
    </div>
</body>
</html>
