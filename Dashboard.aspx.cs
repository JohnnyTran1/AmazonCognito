using AmazonCognito.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;

namespace AmazonCognito
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private readonly CognitoHelper _helper = new CognitoHelper();

        protected void Page_Load(object sender, EventArgs e)
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
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["User_Object"] != null)
                {                  
                    User user = (User)Session["User_Object"];

                    lblWelcome.Text = $"Welcome, {user.Name}";

                    List<User> list = new List<User>();
                    list.Add(user);

                    DisplayGridview(list);
                }
            }

            //Needed for gridview & DataTables plugin
            if (grid1.Rows.Count > 0)
            {
                grid1.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        private void DisplayGridview<T>(ICollection<T> collection)
        {
            grid1.DataSource = collection;
            grid1.EmptyDataText = "No information available";
            grid1.DataBind();
        }


    }
}