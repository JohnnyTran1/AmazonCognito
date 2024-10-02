<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="AmazonCognito.ForgotPassword" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Forgot Password</title>
    <link href="../Library/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <script src="../Library/bootstrap/js/bootstrap.min.js"></script>
    <style type="text/css">
        .form-gap {
            padding-top: 10px;
        }
    </style>

    <%--    <script type="text/javascript">
        function ShowVerificationModal(title, body) {
            var myModal = new bootstrap.Modal(document.getElementById('verificationModal'), {
                keyboard: false
            })
            myModal.show();
        }

        $("#verificationModal").on("hidden.bs.modal", function () {
            document.getElementById("divVerificationModelSuccess").hidden = true;;
        });
    </script>--%>
</head>
<body>
    <div class="container d-flex flex-column">
        <form runat="server">
            <div class="row align-items-center justify-content-center min-vh-100">
                <div class="col-12 col-md-8 col-lg-4">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <div class="mb-4">
                                <div runat="server" id="alertMessage" class="alert alert-success" role="alert" visible="false">
                                    <asp:Label runat="server" ID="lblSuccess"></asp:Label>
                                </div>
                                <div runat="server" id="alertMessageError" class="alert alert-danger" role="alert" visible="false">
                                    <asp:Label runat="server" ID="lblError"></asp:Label>
                                </div>
                            </div>
                            <asp:Panel runat="server" ID="panelResetEmail">

                                <div class="mb-4">
                                    <h5>Forgot Password?</h5>
                                    <p class="mb-2">
                                        Enter your registered email address to reset the password
                                    </p>
                                </div>


                                <div class="mb-3">
                                    <label for="txtEmail" class="form-label">Email</label>
                                    <input runat="server" type="email" id="txtEmail" class="form-control" name="email" placeholder="Enter Your Email" />
                                </div>
                                <div class="mb-3 d-grid">
                                    <asp:Button runat="server" ID="btnReset" CssClass="btn btn-primary" Text="Reset Password" OnClick="btnReset_Click" />
                                </div>
                                <div class="text-center">
                                    <span>Don't have an account? <a href="SignUp.aspx">sign up</a></span>
                                    <hr />
                                    <span>I remember my password! <a href="../Login.aspx">login</a></span>
                                </div>
                            </asp:Panel>
                            <asp:Panel runat="server" ID="panelNewPassword">
                                <div class="mb-4">
                                    <h5>Create a new password.</h5>
                                    <p class="mb-2">
                                        Enter your verification code and new password
                                    </p>
                                </div>
                                <div class="mb-3">
                                    <label for="txtVerificationCode" class="form-label">Verification Code</label>
                                    <input runat="server" type="number" id="txtVerificationCode" class="form-control" name="verificationCode" placeholder="Enter Your Code" />
                                </div>
                                <div class="mb-3">
                                    <label for="txtNewPassword" class="form-label">New Password</label>
                                    <input runat="server" type="password" id="txtNewPassword" class="form-control" name="newPassword" placeholder="Enter Your Password" />
                                </div>
                                <div class="mb-3">
                                    <label for="txtRepeatNewPassword" class="form-label">Repeat New Password</label>
                                    <input runat="server" type="password" id="txtRepeatNewPassword" class="form-control" name="newRepeatPassword" placeholder="Repeat Password" />
                                </div>
                                <div class="mb-3 d-grid">
                                    <asp:Button runat="server" ID="btnSubmit" CssClass="btn btn-primary" Text="Submit" OnClick="btnVerifySubmit_Click" />
                                    <hr />
                                    <asp:Button runat="server" ID="btnResendCode" CssClass="btn btn-danger" Text="Resend Code" OnClick="btnResendCode_Click" />
                                    <br />
                                    <asp:Button runat="server" ID="btnCancel" CssClass="btn btn-secondary" Text="Cancel" OnClick="btnCancel_Click" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>





            <%-- <div class="modal fade" id="verificationModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Verification Code</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p>To verify your account please enter the code sent to the provided email address.</p>
                            <asp:TextBox runat="server" ID="txtCode" CssClass="form-control" TextMode="Number" />
                            <br />
                            <div runat="server" id="divVerificationModal" class="alert alert-danger" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblVerificationModalError"></asp:Label>
                            </div>
                            <div runat="server" id="divVerificationModelSuccess" class="alert alert-success" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblVerificationModalSuccess"></asp:Label>
                            </div>
                        </div>
                        <div class="modal-footer justify-content-between">
                            <asp:Button runat="server" ID="btnResendCode" CssClass="btn btn-danger" Text="Resend Code" OnClick="btnResendCode_Click" />
                            <div>
                                <asp:Button runat="server" ID="btnVerifySubmit" CssClass="btn btn-primary" Text="Submit" OnClick="btnVerifySubmit_Click" />
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>--%>
        </form>
    </div>
</body>
</html>
