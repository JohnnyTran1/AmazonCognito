<%@ Page Title="" Language="C#" MasterPageFile="~/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="AmazonCognito.Dashboard" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- DataTables CSS -->
    <link href="Library/datatables/css/jquery.dataTables.min.css" rel="stylesheet" />
    <link href="Library/datatables/css/responsive.bootstrap5.min.css" rel="stylesheet" />

    <!-- DataTables JS -->
    <script src="Library/datatables/js/jquery.dataTables.min.js"></script>
    <script src="Library/datatables/js/dataTables.responsive.min.js"></script>
    <script src="Library/datatables/js/responsive.bootstrap5.min.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#grid1').DataTable({
                dom: "<'row'<'col-sm-6'B>>" +
                    "<'row table'<'col-sm-6'l><'col-sm-6'f>><br/>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-6'i><'col-sm-6'p>>",
            });
        });
    </script>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <div class="row" style="margin-top: 10px;">
            <div class="col-lg-12">
                <asp:Label runat="server" ID="lblWelcome" Font-Bold="true" Font-Size="XX-Large" />
            </div>
            <div class="col">
            </div>
            <div class="col" style="margin-top: 5px;">
            </div>
        </div>
        <div class="row" style="margin-top: 5em;">
            <h4>Current User's Attributes</h4>
            <div class="panel panel-default">
                <div class="panel-body">
                    <asp:GridView runat="server" ID="grid1" CssClass="table table-striped dt-responsive nowrap table-responsive" AutoGenerateColumns="true" Visible="true" GridLines="Horizontal" ClientIDMode="Static" />
                </div>
            </div>
        </div>
        <br />
        <br />
    </div>
</asp:Content>
