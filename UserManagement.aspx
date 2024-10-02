<%@ Page Title="" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="AmazonCognito.UserManagement" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- DataTables CSS -->
    <link href="Library/datatables/css/jquery.dataTables.min.css" rel="stylesheet" />
    <link href="Library/datatables/css/responsive.bootstrap5.min.css" rel="stylesheet" />

    <!-- DataTables JS -->
    <script src="Library/datatables/js/jquery.dataTables.min.js"></script>
    <script src="Library/datatables/js/dataTables.responsive.min.js"></script>
    <script src="Library/datatables/js/responsive.bootstrap5.min.js"></script>

    <!-- DataTables Buttons -->
    <script src="Library/datatables/Buttons-2.3.3/js/dataTables.buttons.min.js"></script>
    <link href="Library/datatables/Buttons-2.3.3/css/buttons.dataTables.min.css" rel="stylesheet" />
    <script src="Library/datatables/Buttons-2.3.3/js/buttons.html5.min.js"></script>

    <!-- DataTable Button Dependencies for Excel & PDF -->
    <script src="Library/datatables/JSZip-2.5.0/jszip.min.js"></script>
    <!-- Excel -->
    <script src="Library/datatables/pdfmake-0.1.36/pdfmake.min.js"></script>
    <!-- PDF -->
    <script src="Library/datatables/pdfmake-0.1.36/vfs_fonts.js"></script>
    <!-- PDF -->

    <style>
        .table {
            margin-bottom: 10px;
            margin-top: 10px;
        }
    </style>

    <script type="text/javascript">

        $(document).ready(function () {
            $('#gvUsers').DataTable({
                dom: "<'row'<'col-sm-6'B>>" +
                    "<'row table'<'col-sm-6'l><'col-sm-6'f>><br/>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-6'i><'col-sm-6'p>>",
                buttons: [
                    'copy', 'csv',
                    {
                        extend: 'excel',
                        text: 'MS Excel'

                    },
                    {
                        extend: 'pdf',
                        text: 'Create PDF'
                    }
                ]
            });

            /*Clear the Add New User fields and any alert messages when the OffCanvas side bar closes*/
            const myOffcanvas = document.getElementById('offcanvasAddNewUser')
            myOffcanvas.addEventListener('hidden.bs.offcanvas', event => {
                $('#alertMessage').hide();
                ClearAddUserFields();
            });
        });


        function AddNewUser() {

            var obj = {
                firstName: document.getElementById("<%= registerFirstName.ClientID %>").value,
                lastName: document.getElementById("<%= registerLastName.ClientID %>").value,
                email: document.getElementById("<%= registerEmail.ClientID %>").value,
            };

            $.ajax({
                type: "POST",
                url: "<%=System.IO.Path.GetFileName(Request.Path)%>/AddNewUser",  //Get the current page
                data: JSON.stringify(obj),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (res) {

                    if (res !== null) {

                        if (res.d.StatusCode === "200") {

                            ClearAddUserFields();
                            DisplayAlert('success', res.d.Message);

                        } else {
                            DisplayAlert('danger', res.d.Message);
                        }
                    }
                    else {
                        DisplayAlert('danger',"<b>Error:</b> There was an error retrieving the data.");
                    }
                },
                error: function (response) {
                    DisplayAlert('danger', "<b>Error:</b> There was an error retrieving the data.");
                }
            });
        }

        function ClearAddUserFields() {
            document.getElementById("<%= registerFirstName.ClientID %>").value = "";
            document.getElementById("<%= registerLastName.ClientID %>").value = "";
            document.getElementById("<%= registerEmail.ClientID %>").value = "";
        }


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


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <br />
    <h1 class="text-center">User Management</h1>
    <div class="container">
        <div class="row" style="margin-top: 5em;">

            <div class="panel panel-default">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-lg-2 offset-lg-10 col-sm-12">
                            <button id="btnAddUser" class="btn btn-secondary" data-bs-toggle="offcanvas" data-bs-target="#offcanvasAddNewUser" onclick="return false" aria-controls="offcanvasAddNewUser" style="margin-bottom: 15px; width: 100%;"><i class="bi-person-plus-fill"></i>Add User</button>
                        </div>
                    </div>
                    <asp:GridView runat="server" ID="gvUsers" CssClass="table table-striped dt-responsive nowrap table-responsive" Caption="All Users" EmptyDataText="No Users On File" DataKeyNames="Id, EmailVerified" AutoGenerateColumns="false" Visible="true" GridLines="Horizontal" ClientIDMode="Static">
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <asp:BoundField DataField="GivenName" HeaderText="Given Name" />
                            <asp:BoundField DataField="Name" HeaderText="Name" />
                            <asp:BoundField DataField="FamilyName" HeaderText="Family Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="EmailVerified" Visible="false" />
                            <asp:TemplateField HeaderText="Email Verified" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Button runat="server" ID="btnVerify" CssClass="btn btn-secondary" OnClick="btnVerify_Click" Text='<%# Eval("EmailVerified").ToString().ToLower() == "true" ? "Verified" : "Verify" %>' Enabled='<%# Eval("EmailVerified").ToString().ToLower() == "true" ? false : true %>' ToolTip='<%# Eval("EmailVerified").ToString().ToLower() == "true" ? "Email is already verified" : "Mark email as verified" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Enable" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:CheckBox runat="server" ID='chBoxEnabled' Checked='<%# Eval("Enable") %>' OnCheckedChanged="chBoxEnabled_CheckedChanged" AutoPostBack="true" ToolTip='<%# Eval("Enable").ToString().ToLower() == "true" ? "User status is enabled" : "User status is disabled" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Off Canvas Admin Add User -->
        <div class="offcanvas offcanvas-start" tabindex="-1" id="offcanvasAddNewUser" aria-labelledby="offcanvasAddNewUserLabel">
            <div class="offcanvas-header">
                <h5 class="offcanvas-title" id="offcanvasAddNewUserLabel">Add a User</h5>
                <button type="button" class="btn-close text-reset" data-bs-dismiss="offcanvas" aria-label="Close"></button>
            </div>
            <div class="offcanvas-body">
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
                <div id="alertMessage" class="" role="alert" style="display: none">
                    <asp:Label runat="server" ID="lblMessage" ClientIDMode="Static"></asp:Label>
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>

                <!-- Submit button -->
                <input type="button" id="btnAddUserSubmit" class="btn btn-primary  btn-block mb-3" style="width: 100%" value="Submit" onclick="AddNewUser();" />
            </div>
        </div>


    </div>
</asp:Content>
