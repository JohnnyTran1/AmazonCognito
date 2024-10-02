<%@ Page Title="" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ProfilePage.aspx.cs" Inherits="AmazonCognito.Account.ProfilePage" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField runat="server" ID="hiddenFieldID" />
    <div class="container rounded shadow-lg bg-light mt-5 mb-5">
        <div class="row">
            <div class="col-md-3 border-right">
                <div class="d-flex flex-column align-items-center text-center p-3 py-5">
                    <img class="rounded-circle mt-5" width="150px" src="https://st3.depositphotos.com/15648834/17930/v/600/depositphotos_179308454-stock-illustration-unknown-person-silhouette-glasses-profile.jpg"><asp:Label runat="server" ID="lblName" CssClass="font-weight-bold"></asp:Label><asp:Label runat="server" ID="lblEmail" CssClass="text-black-50"></asp:Label><span> </span>
                </div>
            </div>

            <div class="col-md-5 border-right">
                <div class="p-3 py-5">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h4 class="text-right">Profile Information</h4>
                    </div>
                    <div class="row mt-2">
                        <div class="col-md-4">
                            <label class="labels">First Name</label><asp:TextBox runat="server" ID="txtFirstName" CssClass="form-control" placeholder="First Name" />
                        </div>
                        <div class="col-md-4">
                            <label class="labels">Middle Name</label><asp:TextBox runat="server" ID="txtMiddleName" CssClass="form-control" placeholder="Middle Name" />
                        </div>
                        <div class="col-md-4">
                            <label class="labels">Last Name</label><asp:TextBox runat="server" ID="txtLastName" CssClass="form-control" placeholder="Last Name" />
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-md-12">
                            <label class="labels">Email</label><asp:TextBox runat="server" ID="txtEmail" CssClass="form-control" placeholder="Email" />
                        </div>
                        <div class="col-md-12">
                            <label class="labels">Nickname</label><asp:TextBox runat="server" ID="txtNickName" CssClass="form-control" placeholder="Nickname" />
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-md-6">
                            <label class="labels">Country</label><input type="text" class="form-control" placeholder="DISPLAY ONLY" value="" readonly>
                        </div>
                        <div class="col-md-6">
                            <label class="labels">State/Region</label><input type="text" class="form-control" value="" placeholder="DISPLAY ONLY" readonly>
                        </div>
                    </div>
                    <div class="mt-5 text-center">
                        <asp:Button runat="server" ID="btnSave" CssClass="btn btn-primary profile-button" Text="Save Profile" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="p-3 py-5">
                    <div runat="server" id="divErrorAlert" class="alert alert-danger" role="alert" visible="false">
                        <asp:Label runat="server" ID="lblErrorMessage"></asp:Label>
                    </div>
                    <%-- <div class="d-flex justify-content-between align-items-center experience"><span>Edit Experience</span><span class="border px-3 p-1 add-experience"><i class="fa fa-plus"></i>&nbsp;Experience</span></div>
                    <br>
                    <div class="col-md-12">
                        <label class="labels">Experience in Designing</label><input type="text" class="form-control" placeholder="experience" value=""></div>
                    <br>
                    <div class="col-md-12">
                        <label class="labels">Additional Details</label><input type="text" class="form-control" placeholder="additional details" value=""></div>--%>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
