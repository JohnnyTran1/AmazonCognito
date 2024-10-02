<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="AmazonCognito.ResetPassword" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reset Password</title>
    <link href="../Library/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <script src="../Library/bootstrap/js/bootstrap.min.js"></script>
</head>
<body>
    <div class="container">
        <div style="margin-top: 10%">
            <section class="pb-4">
                <div class="bg-white border rounded-5">
                    <h1>Reset Password</h1>
                    <section class="w-100 p-4 d-flex justify-content-center pb-4">
                        <form runat="server" style="width: 22rem;">
                            <!-- New password input -->
                            <div class="form-outline mb-4">
                                <input runat="server" type="password" id="txtNewPassword" class="form-control" />
                                <label class="form-label" for="txtNewPassword">New Password*</label>
                            </div>
                            <!-- Repeat New Password input -->
                            <div class="form-outline mb-4">
                                <input runat="server" type="password" id="txtNewPasswordRepeat" class="form-control" />
                                <label class="form-label" for="txtNewPasswordRepeat">Repeat Password*</label>
                            </div>

                            <!-- Error Message Div -->
                            <div runat="server" id="alertMessageError" class="alert alert-danger" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblError"></asp:Label>
                            </div>
                            <!-- Success Message Div -->
                            <div runat="server" id="alertMessageSuccess" class="alert alert-danger" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblSuccess"></asp:Label>
                            </div>

                            <!-- Submit button -->
                            <asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary btn-block mb-4" Style="width: 100%" Text="Sign in" OnClick="btnSubmit_Click" />
                        </form>
                    </section>
                </div>
            </section>
        </div>
    </div>
</body>
</html>
