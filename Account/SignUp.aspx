<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SignUp.aspx.cs" Inherits="AmazonCognito.SignUp" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SignUp</title>
    <link href="../Library/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <script src="../Library/bootstrap/js/bootstrap.min.js"></script>

    <script type="text/javascript">
        function ShowVerificationModal(title, body) {
            var myModal = new bootstrap.Modal(document.getElementById('verificationModal'), {
                keyboard: false
            })
            myModal.show();
        }

        $("#verificationModal").on("hidden.bs.modal", function () {
            document.getElementById("divVerificationModelSuccess").hidden = true;;
        });
    </script>
</head>
<body>
    <div class="container">
        <form runat="server">
            <section class="pb-4">
                <div class="bg-white border rounded-5" style="margin-top: 10%">
                    <section class="w-100 p-4 d-flex justify-content-center pb-4">
                        <div style="width: 26rem;">
                            <h1 class="text-center">SignUp</h1>

                            <!-- First Name input -->
                            <div class="form-outline mb-4">
                                <label class="form-label" for="registerFirstName">First Name*</label>
                                <input runat="server" type="text" id="registerFirstName" class="form-control" />

                            </div>

                            <!-- Last Name input -->
                            <div class="form-outline mb-4">
                                <label class="form-label" for="registerLastName">Last Name*</label>
                                <input runat="server" type="text" id="registerLastName" class="form-control" />
                            </div>

                            <!-- Email input -->
                            <div class="form-outline mb-4">
                                <label class="form-label" for="registerEmail">Email*</label>
                                <input runat="server" type="email" id="registerEmail" class="form-control" />
                            </div>

                            <!-- Password input -->
                            <div class="form-outline mb-4">
                                <label class="form-label" for="registerPassword">Password*</label>
                                <input runat="server" type="password" id="registerPassword" class="form-control" />
                            </div>

                            <!-- Repeat Password input -->
                            <div class="form-outline mb-4">
                                <label class="form-label" for="registerRepeatPassword">Repeat Password*</label>
                                <input runat="server" type="password" id="registerRepeatPassword" class="form-control" />
                            </div>

                            <!-- Checkbox -->
                            <div class="form-check d-flex justify-content-center mb-4">
                                <input runat="server" class="form-check-input me-2" type="checkbox" value="" id="registerCheck" checked
                                    aria-describedby="registerCheckHelpText" />
                                <label class="form-check-label" for="registerCheck">
                                    I have read and agree to the terms
                                </label>
                            </div>

                            <div runat="server" id="alertMessage" class="alert alert-success" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblSignupSuccess"></asp:Label>
                            </div>
                            <div runat="server" id="alertMessageError" class="alert alert-danger" role="alert" visible="false">
                                <asp:Label runat="server" ID="lblSignupError"></asp:Label>
                            </div>
                            <!-- Submit button -->
                            <asp:Button runat="server" ID="btnSubmit" type="submit" class="btn btn-primary btn-block mb-3" Style="width: 100%" Text="Submit" OnClick="btnSubmit_Click" />

                            <hr />
                            <asp:Button runat="server" ID="btnLoginPage" CssClass="btn btn-secondary" Text="Back to Login" OnClick="btnLoginPage_Click" Width="100%" />
                            <button type='button' class='btn btn-success' data-bs-toggle='modal' data-bs-target='#verificationModal' style="width: 100%; margin-top: 25px;">Confirm Email</button>
                        </div>
                        <!-- Pills content -->
                    </section>
                </div>
            </section>

            <div class="modal fade" id="verificationModal" tabindex="-1">
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
            </div>
        </form>
    </div>
</body>
</html>
