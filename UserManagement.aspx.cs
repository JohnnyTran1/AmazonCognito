using Amazon.CognitoIdentityProvider.Model;
using AmazonCognito.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace AmazonCognito
{
    public partial class UserManagement : System.Web.UI.Page
    {
        private readonly CognitoHelper _helper = new CognitoHelper();

        protected async void Page_Load(object sender, EventArgs e)
        {
            /*
             * Note: All content pages associated with AdminMaster.Master will have authentication and authorization checks performed
             * on the AdminMaster.cs page in the Page_Init event (approx. line 80).
             * 
             * The reason for this is so I only have to set it once and don't have to worry about the risk of these checks not getting put on every content page. Also,
             * if you take a look at the Master/Content Page LifeCycle you will see that the master page Init event is one of the first events to occur on the master/content 
             * page lifecycle. 
             */


            if (!IsPostBack)
            {
                CognitoHelper.CognitoResponse response = await GetAllUsers();

                if (response != null && !response.HasError)
                {
                    DisplayAllUsers(response.Content);
                }
            }

            //Needed for gridview & DataTables plugin
            if (gvUsers.Rows.Count > 0)
            {
                gvUsers.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        private async Task<CognitoHelper.CognitoResponse> GetAllUsers()
        {
            return await _helper.AdminGetAllUsers();
        }

        private void DisplayAllUsers(object list)
        {
            if (list != null)
            {
                gvUsers.DataSource = list;
                gvUsers.EmptyDataText = "No information available";
                gvUsers.DataBind();
            }
        }

        protected async void btnUsers_Click(object sender, EventArgs e)
        {
            CognitoHelper.CognitoResponse response = await _helper.AdminGetAllUsers();

            if (response != null && !response.HasError)
            {
                DisplayAllUsers(response.Content);
            }
        }

        protected async void chBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            ToastNotification notification;
            GridViewRow row = ((GridViewRow)((CheckBox)sender).NamingContainer);
            int index = row.RowIndex;

            if (index != -1)
            {
                string id = gvUsers.DataKeys[index].Values[0].ToString(); //This is how you can retrieve the ID column when it is not visible
                string username = gvUsers.Rows[index].Cells[1].Text;
                string name = gvUsers.Rows[index].Cells[2].Text;

                CheckBox cb1 = (CheckBox)gvUsers.Rows[index].FindControl("chBoxEnabled");

                if (cb1.Checked)
                {
                    CognitoHelper.CognitoResponse response = await _helper.AdminEnableUser(id);

                    if (response != null && !response.HasError)
                    {
                        //User was enabled
                        notification = new ToastNotification
                        {
                            Heading = "Success",
                            Text = $"{name}'s account was enabled.",
                            Icon = ToastIcon.success,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }
                    else
                    {
                        //User failed to be enabled
                        notification = new ToastNotification
                        {
                            Heading = "Error",
                            Text = $"{name}'s account failed to be enabled. <hr/> Exception: {response.Message}.",
                            Icon = ToastIcon.error,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }

                    (this.Master as AdminMaster).DisplayToastNotification(notification);
                }
                else
                {
                    CognitoHelper.CognitoResponse response = await _helper.AdminDisableUser(id);

                    if (response != null & !response.HasError)
                    {
                        //User was disabled
                        notification = new ToastNotification
                        {
                            Heading = "Success",
                            Text = $"{name}'s account was disabled.",
                            Icon = ToastIcon.success,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }
                    else
                    {
                        //User failed to be disabled
                        notification = new ToastNotification
                        {
                            Heading = "Error",
                            Text = $"{name}'s account failed to be disabled. <hr/> Exception: {response.Message}.",
                            Icon = ToastIcon.error,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }
                }
            }
            else
            {
                //Error
                notification = new ToastNotification
                {
                    Heading = "Error",
                    Text = "An unexpected error occurred. Please try again later.",
                    Icon = ToastIcon.error,
                    HideAfter = 7,
                    AutoHide = true
                };
            }

            //DisplayToastNotification(notification);

            (this.Master as AdminMaster).DisplayToastNotification(notification);
        }

        [WebMethod]
        public static object AddNewUser(string firstName, string lastName, string email)
        {
            JSONResponse jsonResponse;
            CognitoHelper _cognitoHelper = new CognitoHelper();


            try
            {
                if (IdentityHelper.IsAuthorized(new string[] { "Admin" }))
                {
                    if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(email))
                    {
                        CognitoHelper.CognitoResponse response = _cognitoHelper.AdminSignUpUser(email, email, firstName, lastName);

                        if (response != null && !response.HasError)
                        {

                            jsonResponse = new JSONResponse()
                            {
                                StatusCode = "200",
                                Message = "<b>Success</b>: You have successfully created an account. An message will be sent to the provide email address with a temporary password."
                            };

                            return jsonResponse;
                        }
                        else
                        {
                            jsonResponse = new JSONResponse()
                            {
                                StatusCode = response.Code.ToString(),
                                Message = response.Message
                            };

                            return jsonResponse;
                        }
                    }
                    else
                    {
                        //Bad Request
                        jsonResponse = new JSONResponse()
                        {
                            StatusCode = "400",
                            Message = "Please make sure all requried fields are correct"
                        };

                        return jsonResponse;
                    }

                }
                else
                {
                    //Not authorized
                    jsonResponse = new JSONResponse()
                    {
                        StatusCode = "403",
                        Message = "You are not authorized to perform this action"
                    };

                    return jsonResponse;
                }
            }
            catch (Exception)
            {
                jsonResponse = new JSONResponse()
                {
                    StatusCode = "500",
                    Message = "An unexpected error has occurred while processing your request."
                };

                return jsonResponse;
            }
        }

        public class JSONResponse
        {
            public object ContentObject { get; set; }
            public string Message { get; set; }
            public string StatusCode { get; set; }
        }

        protected async void btnVerify_Click(object sender, EventArgs e)
        {
            ToastNotification notification;
            List<AttributeType> attributeList = new List<AttributeType>();
            GridViewRow row = ((GridViewRow)((Button)sender).NamingContainer);
            int index = row.RowIndex;

            if (index != -1)
            {
                string id = gvUsers.DataKeys[index].Values[0].ToString(); //This is how you can retrieve the ID column when it is not visible
                string name = gvUsers.Rows[index].Cells[2].Text; //
                string emailVerified = gvUsers.DataKeys[index].Values[1].ToString(); //This is how you can retrieve the ID column when it is not visible

                if (bool.TryParse(emailVerified, out bool verified) && !verified)
                {
                    attributeList.Add(new AttributeType { Name = "email_verified", Value = "true" });

                    //Update Attributes
                    CognitoHelper.CognitoResponse response = await _helper.AdminUpdateUserAttributes(id, attributeList);

                    if (response != null && !response.HasError)
                    {
                        Button btn = (row.FindControl("btnVerify") as Button);
                        btn.Text = "Verified";
                        btn.Enabled = false;

                        //Email was set as verified
                        notification = new ToastNotification
                        {
                            Heading = "Success",
                            Text = $"{name}'s email was set as verified",
                            Icon = ToastIcon.success,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }
                    else
                    {
                        //Email failed to set as verified
                        notification = new ToastNotification
                        {
                            Heading = "Error",
                            Text = $"{name}'s email failed to be set as verified.  <hr/> Exception: {response.Message}.",
                            Icon = ToastIcon.error,
                            HideAfter = 7,
                            AutoHide = true
                        };
                    }

                    (this.Master as AdminMaster).DisplayToastNotification(notification);
                }
                else
                {
                    //User failed to be disabled
                    notification = new ToastNotification
                    {
                        Heading = "Error",
                        Text = $"{name}'s email is already marked as verified.",
                        Icon = ToastIcon.error,
                        HideAfter = 7,
                        AutoHide = true
                    };
                }
            }
            else
            {
                //Error
                notification = new ToastNotification
                {
                    Heading = "Error",
                    Text = "An unexpected error occurred. Please try again later.",
                    Icon = ToastIcon.error,
                    HideAfter = 7,
                    AutoHide = true
                };
            }

            //DisplayToastNotification(notification);

            (this.Master as AdminMaster).DisplayToastNotification(notification);
        }
    }
}