using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;

public partial class StudentBinder_BiweeklyBehaviorGraph : System.Web.UI.Page
{
    clsData objData = null;
    clsSession sess = null;
    ClsTemplateSession ObjTempSess = null;
    private int m_currentPageIndex;
    private IList<Stream> m_streams;
    protected void Page_Load(object sender, EventArgs e)
    {
        RV_Behavior.AsyncRendering = false;
        RV_Behavior.SizeToReportContent = true;
        RV_Behavior.ZoomMode = ZoomMode.PageWidth;

        sess = (clsSession)Session["UserSession"];
        if (sess == null)
        {
            Response.Redirect("Error.aspx?Error=Your session has expired. Please log-in again");
        }
        else
        {
            bool flag = clsGeneral.PageIdentification(sess.perPage);
            if (flag == false)
            {
                Response.Redirect("Error.aspx?Error=You are not authorized to access this Page.Contact Program Administrator");
            }
        }
        if (!IsPostBack)
        {
            FILLBEHAVIOR();
            sess = (clsSession)Session["UserSession"];
            ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
            ClassDatatable objClassData = new ClassDatatable();
            if (Request.QueryString["studid"] != null)
            {
                int pageid = Convert.ToInt32(Request.QueryString["pageid"].ToString());
                int studid = Convert.ToInt32(Request.QueryString["studid"].ToString());
                sess.StudentId = studid;
                ObjTempSess.LessonPlanId = pageid;
                chkbxevents.Checked = true;
                chkbxIOA.Checked = true;
                chkmedication.Checked = false;
                //chkrate.Checked = true;
                chkrate.Checked = false;
                chkreptrend.Checked = true;
                txtEdate.Text = DateTime.Now.Date.AddDays(1).ToString("MM/dd/yyyy").Replace("-", "/");
                txtSdate.Text = DateTime.Now.Date.AddDays(-91).ToString("MM/dd/yyyy").Replace("-", "/");
                //rbtnClassType.SelectedValue = objClassData.GetClassType(sess.Classid);
                RV_Behavior.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "loadWait();", true);
                LoadGraph();                
            }
        }
    }

    [Serializable]
    public class CustomReportCredentials : Microsoft.Reporting.WebForms.IReportServerCredentials
    {
        // From: http://community.discountasp.net/default.aspx?f=14&m=15967
        // local variable for network credential.
        private string _UserName;
        private string _PassWord;
        private string _DomainName;
        public CustomReportCredentials(string UserName, string PassWord, string DomainName)
        {
            _UserName = UserName;
            _PassWord = PassWord;
            _DomainName = DomainName;
        }
        public System.Security.Principal.WindowsIdentity ImpersonationUser
        {
            get
            {
                return null;  // not use ImpersonationUser
            }
        }
        public ICredentials NetworkCredentials
        {
            get
            {
                // use NetworkCredentials
                return new NetworkCredential(_UserName, _PassWord, _DomainName);
            }
        }
        public bool GetFormsCredentials(out Cookie authCookie, out string user,
            out string password, out string authority)
        {

            // not use FormsCredentials unless you have implements a custom autentication.
            authCookie = null;
            user = password = authority = null;
            return false;
        }
    }
    private bool Validate()
    {
        bool result = true;
        if (txtSdate.Text == "")
        {
            result = false;
            tdMsg.InnerHtml = clsGeneral.warningMsg("Please Enter Start Date");
            return result;
        }
        else if (txtEdate.Text == "")
        {
            result = false;
            tdMsg.InnerHtml = clsGeneral.warningMsg("Please Enter End Date");
            return result;
        }
        else if (txtSdate.Text != "" && txtEdate.Text != "")
        {
            DateTime dtst = new DateTime();
            DateTime dted = new DateTime();
            dtst = DateTime.ParseExact(txtSdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            dted = DateTime.ParseExact(txtEdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            if (dtst > dted)
            {
                result = false;
                tdMsg.InnerHtml = clsGeneral.warningMsg("Start date is must before the End date");
                return result;
            }

        }        
        return result;

    }

    private void FILLBEHAVIOR()
    {
        string strQuery="";
        sess = (clsSession)Session["UserSession"];
        objData = new clsData();
        //string StrQuery = "SELECT MeasurementId as Id, Behaviour as Name FROM BehaviourDetails WHERE ActiveInd = 'A' AND StudentId=" + sess.StudentId + "";
        if (CheckBox1.Checked == true && CheckBox2.Checked == false)
        {
           strQuery = "SELECT  Bdet.MeasurementId  as Id,Bdet.Behaviour as Name " +
                               "FROM BehaviourDetails Bdet LEFT JOIN BehaviourLPRel BlpRel on Bdet.MeasurementId = BlpRel.MeasurementId " +
                                "WHERE Bdet.StudentId = " + sess.StudentId + " AND (Bdet.ActiveInd = 'A') order by Bdet.CreatedOn";
            
        }
        else if (CheckBox1.Checked == false && CheckBox2.Checked == true)
        {
            strQuery = "SELECT  Bdet.MeasurementId  as Id,Bdet.Behaviour as Name " +
                               "FROM BehaviourDetails Bdet LEFT JOIN BehaviourLPRel BlpRel on Bdet.MeasurementId = BlpRel.MeasurementId " +
                                "WHERE Bdet.StudentId = " + sess.StudentId + " AND (Bdet.ActiveInd = 'N') order by Bdet.CreatedOn";
            
        }
        else if (CheckBox1.Checked == true && CheckBox2.Checked == true)
        {
            strQuery = "SELECT  Bdet.MeasurementId  as Id,Bdet.Behaviour as Name " +
                              "FROM BehaviourDetails Bdet LEFT JOIN BehaviourLPRel BlpRel on Bdet.MeasurementId = BlpRel.MeasurementId " +
                               "WHERE Bdet.StudentId = " + sess.StudentId + " AND (Bdet.ActiveInd = 'N' OR Bdet.ActiveInd = 'A') order by Bdet.CreatedOn";

        }
        else
        {
            strQuery = "SELECT  Bdet.MeasurementId  as Id,Bdet.Behaviour as Name " +
                              "FROM BehaviourDetails Bdet LEFT JOIN BehaviourLPRel BlpRel on Bdet.MeasurementId = BlpRel.MeasurementId " +
                               "WHERE Bdet.StudentId = " + sess.StudentId + " AND (Bdet.ActiveInd = 'N' AND Bdet.ActiveInd = 'A') order by Bdet.CreatedOn";

        }
       



         objData.ReturnDropDownclinical(strQuery, ddlBehavior);
        if (ddlBehavior.Items.Count > 1)
        {
            ddlBehavior.Items.RemoveAt(0);
            ddlBehavior.Items.Insert(0, new ListItem("---------------All Behavior---------------", "0"));
        }
        //else
        //{
        //    ddlBehavior.Items.Insert(0, new ListItem("---------------All Beha---------------", "0"));
        //}
    }
    protected void btnsubmit_Click(object sender, EventArgs e)
    {
        try
        {
            LoadGraph();

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void LoadGraph()
    {
        if (Validate() == true)
        {
            objData = new clsData();
            int studid = Convert.ToInt32(Request.QueryString["studid"].ToString());
            string StudentName = "";
            string strQuery = "SELECT StudentLname+','+StudentFname AS StudentName FROM Student WHERE StudentId=" + studid;
            DataTable dt = objData.ReturnDataTable(strQuery, false);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    StudentName = dt.Rows[0]["StudentName"].ToString();
                }
            }

            tdMsg.InnerHtml = "";
            RV_Behavior.Visible = true;
            objData = new clsData();
            //objData.ExecuteSp();
            sess = (clsSession)Session["UserSession"];
            DateTime dtst = new DateTime();
            DateTime dted = new DateTime();
            dtst = DateTime.ParseExact(txtSdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            dted = DateTime.ParseExact(txtEdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            string StartDate = dtst.ToString("yyyy-MM-dd");
            string enddate = dted.ToString("yyyy-MM-dd");
            string behavior = ddlBehavior.SelectedItem.Value;
            bool chkbox1 = CheckBox1.Checked;
            bool chkbox2 = CheckBox2.Checked;
            string Event = "";
            int IncludeRateGraph;
            if (chkbox1 == true && chkbox2 == true && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
             {
                  string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                                  "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND (ActiveInd='A' OR ActiveInd='N')) LP FOR XML PATH('')),1,1,'')";
                  behavior = Convert.ToString(objData.FetchValue(StrQuery));
             }
            else if (chkbox1 == true && chkbox2 == false && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
            {
                string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                              "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A' ) LP FOR XML PATH('')),1,1,'')";
                behavior = Convert.ToString(objData.FetchValue(StrQuery));
            }
            else if (chkbox1 == false && chkbox2 == true && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
            {
                string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                              "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='N' ) LP FOR XML PATH('')),1,1,'')";
                behavior = Convert.ToString(objData.FetchValue(StrQuery));
            }
            //if (Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
            //{
            //    string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
            //                  "StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A') LP FOR XML PATH('')),1,1,'')";
            //    behavior = Convert.ToString(objData.FetchValue(StrQuery));
            //}
           
            
            if (behavior == "")
            {
                RV_Behavior.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "alert('No Data Available For Graph Plotting');", true);
                return;
            }
            Session["ClinicalReport"] = behavior;

            string TrendType = "NotNeed";
            if (Convert.ToBoolean(chkreptrend.Checked))
            {
                TrendType = "Quarter";
            }
            string Events = "None,";
            if (chkbxevents.Checked == true)
            {
                Events = "Major,Minor,Arrow";
            }
            else
            {
                if (chkbxmajor.Checked == true)
                {
                    Events += "Major,";
                }
                if (chkbxminor.Checked == true)
                {
                    Events += "Minor,";
                }
                if (chkbxarrow.Checked == true)
                {
                    Events += "Arrow";
                }
            }
            if (chkrate.Checked == true)
            {
                IncludeRateGraph = 1;
            }
            else
            {
                IncludeRateGraph = 0;
            }
            RV_Behavior.ServerReport.ReportServerCredentials = new CustomReportCredentials(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], ConfigurationManager.AppSettings["Domain"]);

            if (HttpContext.Current.Request.UserAgent.ToLower().Contains("ipad"))
            {
                if (rbtnIncidentalRegular.SelectedValue == "Regular")
                {
                    RV_Behavior.ServerReport.ReportPath = ConfigurationManager.AppSettings["DupClinical"];
                }
                else
                {
                    RV_Behavior.ServerReport.ReportPath = ConfigurationManager.AppSettings["IncidentalDupClinical"];
                }                
            }
            else
            {
                if (rbtnIncidentalRegular.SelectedValue == "Regular")
                {
                    RV_Behavior.ServerReport.ReportPath = ConfigurationManager.AppSettings["Clinical"];
                }
                else
                {
                    RV_Behavior.ServerReport.ReportPath = ConfigurationManager.AppSettings["IncidentalClinical"];
                }                
            }
            
            RV_Behavior.ShowParameterPrompts = false;

            ReportParameter[] parameter = new ReportParameter[12];
            parameter[0] = new ReportParameter("StartDate", StartDate);
            parameter[1] = new ReportParameter("ENDDate", enddate);
            parameter[2] = new ReportParameter("Behavior", behavior);
            parameter[3] = new ReportParameter("Studentid", studid.ToString());
            parameter[4] = new ReportParameter("SchoolId", sess.SchoolId.ToString());
            parameter[5] = new ReportParameter("Events", Events);
            parameter[6] = new ReportParameter("IncludeIOA", Convert.ToBoolean(chkbxIOA.Checked).ToString());
            parameter[7] = new ReportParameter("IncludeMedication", Convert.ToBoolean(chkmedication.Checked).ToString());
            parameter[8] = new ReportParameter("Trendtype", TrendType);
            parameter[9] = new ReportParameter("ClassType", rbtnClassType.SelectedValue);
            parameter[10] = new ReportParameter("IncludeRateGraph", IncludeRateGraph.ToString());
            parameter[11] = new ReportParameter("StudentName",  StudentName.Split(',')[0]+ ", " + StudentName.Split(',')[1]);
            
            this.RV_Behavior.ServerReport.SetParameters(parameter);

            RV_Behavior.ServerReport.Refresh();
        }
        else
        {
            RV_Behavior.Visible = false;
        }
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        try
        {
            if (Convert.ToString(Session["ClinicalReport"]) != "")
            {
                objData = new clsData();
                int studid = Convert.ToInt32(Request.QueryString["studid"].ToString());
                string StudentName = "";
                string strQuery = "SELECT StudentLname+','+StudentFname AS StudentName FROM Student WHERE StudentId=" + studid;
                DataTable dt = objData.ReturnDataTable(strQuery, false);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        StudentName = dt.Rows[0]["StudentName"].ToString();
                    }
                }
                string behavior = ddlBehavior.SelectedItem.Value;
                bool chkbox1 = CheckBox1.Checked;
                bool chkbox2 = CheckBox2.Checked;
                string Event = "";
                int IncludeRateGraph;
                if (chkbox1 == true && chkbox2 == true && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
                {
                    string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                                    "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND (ActiveInd='A' OR ActiveInd='N')) LP FOR XML PATH('')),1,1,'')";
                    behavior = Convert.ToString(objData.FetchValue(StrQuery));
                }
                else if (chkbox1 == true && chkbox2 == false && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
                {
                    string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                                  "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A' ) LP FOR XML PATH('')),1,1,'')";
                    behavior = Convert.ToString(objData.FetchValue(StrQuery));
                }
                else if (chkbox1 == false && chkbox2 == true && Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
                {
                    string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                                  "StudentId=" + studid + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='N' ) LP FOR XML PATH('')),1,1,'')";
                    behavior = Convert.ToString(objData.FetchValue(StrQuery));
                }
                //if (Convert.ToInt32(ddlBehavior.SelectedItem.Value) == 0)
                //{
                //    string StrQuery = "SELECT STUFF((SELECT ','+CONVERT(VARCHAR, MeasurementId)  FROM (SELECT DISTINCT MeasurementId FROM BehaviourDetails WHERE " +
                //                  "StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A') LP FOR XML PATH('')),1,1,'')";
                //    behavior = Convert.ToString(objData.FetchValue(StrQuery));
                //}

                //string ReportXML = "SELECT [Name],CAST(CAST([Content] AS VARBINARY(MAX)) AS XML) AS reportXML FROM [Catalog] WHERE type = 2";
                //DataTable dt = ObjData.ReturnDataTable(ReportXML, false);
                sess = (clsSession)Session["UserSession"];
                //string behavior = Convert.ToString(Session["ClinicalReport"]);
                string TrendType = "NotNeed";
                //int IncludeRateGraph;
                if (Convert.ToBoolean(chkreptrend.Checked))
                {
                    TrendType = "Quarter";
                }
                string Events = "None,";
                if (chkbxevents.Checked == true)
                {
                    Events = "Major,Minor,Arrow";
                }
                else
                {
                    if (chkbxmajor.Checked == true)
                    {
                        Events += "Major,";
                    }
                    if (chkbxminor.Checked == true)
                    {
                        Events += "Minor,";
                    }
                    if (chkbxarrow.Checked == true)
                    {
                        Events += "Arrow";
                    }
                }
                if (chkrate.Checked == true)
                {
                    IncludeRateGraph = 1;
                }
                else
                {
                    IncludeRateGraph = 0;
                }
                ReportViewer ClinicalReport = new ReportViewer();                
                DateTime dtst = new DateTime();
                DateTime dted = new DateTime();
                dtst = DateTime.ParseExact(txtSdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                dted = DateTime.ParseExact(txtEdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                string StartDate = dtst.ToString("yyyy-MM-dd");
                string enddate = dted.ToString("yyyy-MM-dd");
                ClinicalReport.ProcessingMode = ProcessingMode.Remote;
                ClinicalReport.ServerReport.ReportServerCredentials = new CustomReportCredentials(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], ConfigurationManager.AppSettings["Domain"]);
                if (rbtnIncidentalRegular.SelectedValue == "Regular")
                {
                    ClinicalReport.ServerReport.ReportPath = ConfigurationManager.AppSettings["ExportClinical"];
                }
                else
                {
                    ClinicalReport.ServerReport.ReportPath = ConfigurationManager.AppSettings["IncidentalExportClinical"];
                }                
                ClinicalReport.ServerReport.ReportServerUrl = new Uri(ConfigurationManager.AppSettings["ReportUrl"]);
                ClinicalReport.ServerReport.Refresh();

                ReportParameter[] parameter = new ReportParameter[12];
                parameter[0] = new ReportParameter("StartDate", StartDate);
                parameter[1] = new ReportParameter("ENDDate", enddate);
                parameter[2] = new ReportParameter("Behavior", behavior);
                parameter[3] = new ReportParameter("Studentid", studid.ToString());
                parameter[4] = new ReportParameter("SchoolId", sess.SchoolId.ToString());
                parameter[5] = new ReportParameter("Events", Events);
                parameter[6] = new ReportParameter("IncludeIOA", Convert.ToBoolean(chkbxIOA.Checked).ToString());
                parameter[7] = new ReportParameter("IncludeMedication", Convert.ToBoolean(chkmedication.Checked).ToString());
                parameter[8] = new ReportParameter("Trendtype", TrendType);                                      //Parameter Value
                parameter[9] = new ReportParameter("ClassType", rbtnClassType.SelectedValue);
                parameter[10] = new ReportParameter("IncludeRateGraph", IncludeRateGraph.ToString());
                parameter[11] = new ReportParameter("StudentName", StudentName.Split(',')[0] + ", " + StudentName.Split(',')[1]);
                ClinicalReport.ServerReport.SetParameters(parameter);

                Warning[] warnings;
                string[] streamids;
                string mimeType, encoding, extension, deviceInfo;

                if (Convert.ToBoolean(chkmedication.Checked).ToString().ToLower() == "true")
                {
                    deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
                }
                else
                {
                   // deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth><MarginTop>1.5in</MarginTop></DeviceInfo>";
                    deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth><MarginTop>0.5cm</MarginTop></DeviceInfo>";
                }
                //deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";

             //   if (Convert.ToBoolean(chkrate.Checked).ToString().ToLower() == "true")
           //     {
                 //   deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
               // }
               // else
               // {
               //     deviceInfo = "<DeviceInfo><PageHeight>5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
                //}
                 byte[] bytes = ClinicalReport.ServerReport.Render("PDF", deviceInfo, out mimeType, out encoding, out extension, out streamids, out warnings);


                 string outputPath = Server.MapPath("~\\StudentBinder\\Reports\\" + StudentName + "_ClinicalReport.pdf");
                                

                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                }

                Session["PdfPath"] = outputPath;
                tdMsgExport.InnerHtml = clsGeneral.sucessMsg("Export Successfully Created...");
                hdnExport.Value = "true";
                ClientScript.RegisterStartupScript(GetType(), "", "DownloadPopup();", true);

                //Response.Buffer = true;
                //Response.Clear();
                //Response.ContentType = mimeType;
                //Response.AddHeader("content-disposition", "attachment; filename=" + sess.StudentName + "_ClinicalReport.pdf");                
                //Response.BinaryWrite(bytes); // create the file
                //Response.Flush(); // send it to the client to download                
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void btnDownload_Click(object sender, EventArgs e)
    {
        try
        {
            string FileName = Session["PdfPath"].ToString();
            WebClient req = new WebClient();
            HttpResponse response = HttpContext.Current.Response;
            response.Clear();
            response.ClearContent();
            response.ClearHeaders();
            response.Buffer = true;
            response.AddHeader("Content-Disposition", "attachment;filename=\"" + FileName + "\"");
            byte[] data = req.DownloadData(FileName);
            response.BinaryWrite(data);
            ClientScript.RegisterStartupScript(GetType(), "", "HideWait();", true);
            response.End();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void btnPrint_Click(object sender, EventArgs e)
    {
        try
        {
            ExportReport();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected string GetAutoPrintJs()
    {
        var script = new StringBuilder();
        script.Append("var pp = getPrintParams();");
        script.Append("pp.interactive= pp.constants.interactionLevel.full;");
        script.Append("print(pp);"); return script.ToString();
    }

    private void ExportReport()
    {
        if (Convert.ToString(Session["ClinicalReport"]) != "")
        {            
            objData = new clsData();
            //string ReportXML = "SELECT [Name],CAST(CAST([Content] AS VARBINARY(MAX)) AS XML) AS reportXML FROM [Catalog] WHERE type = 2";
            //DataTable dt = ObjData.ReturnDataTable(ReportXML, false);
            sess = (clsSession)Session["UserSession"];
            string behavior = Convert.ToString(Session["ClinicalReport"]);
            string TrendType = "NotNeed";
            int IncludeRateGraph;
            if (Convert.ToBoolean(chkreptrend.Checked))
            {
                TrendType = "Quarter";
            }
            string Events = "None,";
            if (chkbxevents.Checked == true)
            {
                Events = "Major,Minor,Arrow";
            }
            else
            {
                if (chkbxmajor.Checked == true)
                {
                    Events += "Major,";
                }
                if (chkbxminor.Checked == true)
                {
                    Events += "Minor,";
                }
                if (chkbxarrow.Checked == true)
                {
                    Events += "Arrow";
                }
            }
            if (chkrate.Checked == true)
            {
                IncludeRateGraph = 1;
            }
            else
            {
                IncludeRateGraph = 0;
            }
            ReportViewer ClinicalReport = new ReportViewer();
            DateTime dtst = new DateTime();
            DateTime dted = new DateTime();
            dtst = DateTime.ParseExact(txtSdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            dted = DateTime.ParseExact(txtEdate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            string StartDate = dtst.ToString("yyyy-MM-dd");
            string enddate = dted.ToString("yyyy-MM-dd");
            ClinicalReport.ProcessingMode = ProcessingMode.Remote;
            ClinicalReport.ServerReport.ReportServerCredentials = new CustomReportCredentials(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"], ConfigurationManager.AppSettings["Domain"]);
            if (rbtnIncidentalRegular.SelectedValue == "Regular")
            {
                ClinicalReport.ServerReport.ReportPath = ConfigurationManager.AppSettings["DupClinical"];
            }
            else
            {
                ClinicalReport.ServerReport.ReportPath = ConfigurationManager.AppSettings["IncidentalDupClinical"];
            }        
            
            ClinicalReport.ServerReport.ReportServerUrl = new Uri(ConfigurationManager.AppSettings["ReportUrl"]);
            ClinicalReport.ServerReport.Refresh();

            ReportParameter[] parameter = new ReportParameter[11];
            parameter[0] = new ReportParameter("StartDate", StartDate);
            parameter[1] = new ReportParameter("ENDDate", enddate);
            parameter[2] = new ReportParameter("Behavior", behavior);
            parameter[3] = new ReportParameter("Studentid", sess.StudentId.ToString());
            parameter[4] = new ReportParameter("SchoolId", sess.SchoolId.ToString());
            parameter[5] = new ReportParameter("Events", Events);
            parameter[6] = new ReportParameter("IncludeIOA", Convert.ToBoolean(chkbxIOA.Checked).ToString());
            parameter[7] = new ReportParameter("IncludeMedication", Convert.ToBoolean(chkmedication.Checked).ToString());
            parameter[8] = new ReportParameter("Trendtype", TrendType);                                      //Parameter Value
            parameter[9] = new ReportParameter("ClassType", rbtnClassType.SelectedValue);
            parameter[10] = new ReportParameter("IncludeRateGraph", IncludeRateGraph.ToString());
            ClinicalReport.ServerReport.SetParameters(parameter);
            Warning[] warnings;
            string[] streamids;
            string mimeType, encoding, extension, deviceInfo;

            //deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
           // if (Convert.ToBoolean(chkrate.Checked).ToString().ToLower() == "true")
            //{
               // deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
            //}
            //else
            //{
                deviceInfo = "<DeviceInfo><PageHeight>8.5in</PageHeight><PageWidth>11in</PageWidth></DeviceInfo>";
           // }

            byte[] bytes = ClinicalReport.ServerReport.Render("PDF", deviceInfo, out mimeType, out encoding, out extension, out streamids, out warnings);
            string outputPath = Server.MapPath("~\\StudentBinder\\Reports\\" + sess.StudentName + "_ClinicalReport.pdf");
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }

            string PdfPath =  sess.StudentName + "_ClinicalReport.pdf";

             
            Response.Write(string.Format("<script>window.open('{0}','_blank');</script>", "PrintReport.aspx?file=" + PdfPath));
            
        }   
    }
    protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
   {
       //if (CheckBox1.Checked == true)
       //    CheckBox1.Checked = false;
       //else if (CheckBox1.Checked == false)
       //    CheckBox1.Checked = true;
        FILLBEHAVIOR();
    }
    protected void CheckBox2_CheckedChanged(object sender, EventArgs e)
    {

        //if (CheckBox2.Checked == true)
        //    CheckBox2.Checked = false;
        //else if (CheckBox2.Checked == false)
        //    CheckBox2.Checked = true;
        FILLBEHAVIOR();
    }

}