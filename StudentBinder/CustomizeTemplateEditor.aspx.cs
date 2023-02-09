﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Collections;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Globalization;

public partial class StudentBinder_CustomizeTemplateEditor : System.Web.UI.Page
{
    static clsData Dataobj = null;
    clsSession sess;
    clsLessons oLessons;
    ClsTemplateSession ObjTempSess;
    clsMathToSamples objMatch = null;
    clsData objData = null;
    DataClass oData = null;
    static bool Disable = false;
    svc_lessonManagement.LessonManagementClient lp = new svc_lessonManagement.LessonManagementClient();
    svc_lessonManagement.clsRequest req = new svc_lessonManagement.clsRequest();
    static int DistractInc = 1;    

    protected void Page_Load(object sender, EventArgs e)
    {
        divMessage.InnerHtml = "";
        sess = (clsSession)Session["UserSession"];

        ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
        //rbtnPromptLevel.Visible = false;
        //rdoRandomMoveover.Visible = false;
        //chkTotalRandom.Visible = false;
        //rbtnMatchToSampleExpressive.Visible = false;

        //lblPromptLevel.Visible = false;

        tdReadMsg.Visible = true;
        if (sess == null)
        {
            Response.Redirect("Error.aspx?Error=Your session has expired. Please log-in again");
        }
        else
        {
            //HiddenField1.Value = sess.UserName;
            bool flag = clsGeneral.PageIdentification(sess.perPage);
            if (flag == false)
            {
                Response.Redirect("Error.aspx?Error=You are not authorized to access this Page.Contact Program Administrator");
            }
        }

        //   if (ObjTempSess == null) return;

        //ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
        //scriptManager.RegisterPostBackControl(this.grdFile);
        if (!IsPostBack)
        {
            if (Request.QueryString["admin"] != null && Request.QueryString["admin"] == "true")
            {
                Fillinsertdata();
                FillGoalData();
                FillApprovedLessonDataAdmin();
                lstCompletePrompts.Items.Clear();
                objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt' ORDER BY sortOrder ", lstCompletePrompts);
                panelAdminTemp.Visible = false;
                BtnAddNewLessonPlan.Visible = false;
                mBxContainer.Visible = false;
                MainDiv.Style.Add("width", "100%");
                idLessonType.Visible = false;
                BtnAddNewLessonPlan.Visible = false;
                panelContent.Visible = false;
                panelButtons.Visible = false;
                panelGoalAdmin.Visible = true;
                panelGoal.Visible = false;
                panelButtonsAdmin.Visible = true;
                DilogAproveLP.Visible = false;
                BtnUpdateLessonPlan.Text = "Save";
                ApprovalNoteVisibilityAdmin();
                LoadLPforSorting();
                if (Request.QueryString["DatabankMode"] == "OpenOrEdit")
                    lnkApprovedLessonsAdmin_Click(sender, e);
                else
                    ClientScript.RegisterStartupScript(this.GetType(), "", "LoadAdminTemplateView(0);", true);
                cbx_deletesess.Visible = false;
            }
            else
            {
                LoadData();
                lblloadAlert.Visible = true;
                btndoc.Visible = false;
                BtnSubmit.Visible = false;
                BtnPreview.Visible = false;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                BtnUpdateLessonPlan.Visible = false;
                if (Request.QueryString["vLessonId"] != null)             //Return from visual lesson list
                {
                    int currentLessonId = Convert.ToInt32(Request.QueryString["vLessonId"]);
                    ViewState["HeaderId"] = currentLessonId;
                    LoadTemplateData(currentLessonId);                                                 //load the selected visual lessonData

                    // BtnVTSavePrompt.Visible = true;

                    setWritePermissions(false);

                    if (Request.QueryString["copy"] != null)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "timerStart();", true);
                    }



                }
                else
                {


                    //  BtnVTSavePrompt.Visible = false;
                }
            }

            hideAllOptions();
        }

        if (CopyCheck())//Enable/Disable copy checkbox
        {
            chkCpyStdtTemplate.Enabled = true;
        }
        else
        {
            chkCpyStdtTemplate.Enabled = false;
        }

        BtnExportTemplate.Attributes.Add("onClick", "return false;");

        //drpTasklist_SelectedIndexChanged1(sender, e);
    }

    public bool DeletePermission()
    {
        bool visibleResult = false;
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string strSql = " SELECT RGP.ReadInd,RGP.WriteInd FROM RoleGroupPerm RGP  INNER JOIN UserRoleGroup URG ON RGP.RoleGroupId = URG.RoleGroupId INNER JOIN  [Object] O ";
        strSql += "ON RGP.ObjectId = O.ObjectId WHERE    URG.SchoolId =  '" + sess.SchoolId + "' AND  URG.UserId = '" + sess.LoginId + "'  and O.ObjectUrl='Delete LessonPlan'";
        DataTable Dt = objData.ReturnDataTable(strSql, false);
        Boolean Read = false;
        Boolean Write = false;
        try
        {
            if (Dt != null)
            {
                if (Dt.Rows.Count > 0)
                {
                    foreach (DataRow Dr in Dt.Rows)
                    {
                        Read = Convert.ToBoolean(Dr["ReadInd"]);
                        Write = Convert.ToBoolean(Dr["WriteInd"]);

                        if (Write == true) break;
                    }
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

        if (Write == true)
        {
            visibleResult = true;
        }
        else
        {
            visibleResult = false;
        }

        return visibleResult;
    }

    public void fillParentSetData()
    {
        int headerId = 0;
        // fill the Parent set in step tab
        try
        {
            if (ViewState["HeaderId"] != null)
            {
                headerId = Convert.ToInt32(ViewState["HeaderId"]);
            }
            lblSelParentSets.Text = "Not Assigned to any Sets";
            ClearStepData();
            FillSetDrpList();
            BtnUpdateStep.Visible = false;
            btnAddStepDetails.Visible = true;

            chkEnd.Enabled = true;
            chkEnd.Checked = true;
            ddlSortOrder.Enabled = false;

            lblAddorUpdateStep.Text = "Add Step";
            if (IsDiscrete(headerId) == true)
            {
                BtnUpdateStep.Visible = false;
                btnAddStepDetails.Visible = false;
            }
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddStep();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    private void LoadData()
    {
        tdReadMsg.InnerHtml = "";
        bool visible = false;
        Fillinsertdata();
        FillData();                                      // Fill the goal type assigned for the student
        FillApprovedLessonData();
        FillCompltdLessonPlans();
        FillRejectedLessons();
        FillMaintenanceLessonData();
        FillInactiveLessonData();
        ButtonVisibility(visible);
        LoadLPforSorting();
    }

    private void setWritePermissions(bool createNew)
    {
        clsGeneral.PageReadAndWrite(sess.LoginId, sess.SchoolId, out Disable);
        if (Disable == true)
        {
            btnFromReject.Visible = false;
            btnFromRejectDup.Visible = false;
            BtnCopyTemplate.Visible = false;
            BtnSubmit.Visible = false;
            BtnExportTemplate.Visible = false;
            ButtonVisibility(false);
            DatalistVisibility(false);
            if (createNew == true) BtnCopyTemplate.Visible = false;
        }
        else
        {
            int headerId = 0;
            if (ViewState["HeaderId"] != null)
            {
                headerId = Convert.ToInt32(ViewState["HeaderId"]);
            }
            if (createNew == true) BtnCopyTemplate.Visible = true;
            GetStatus(headerId);
        }
    }


    private void setApprovePermission()
    {
        clsGeneral.ApprovePermissions(sess.LoginId, sess.SchoolId, out Disable);
        if (Disable == true)
        {
            BtnApproval.Visible = false;

            BtnReject.Visible = false;

            btnCommentLessonInfo.Visible = false;
            btncommentLessonProcedure.Visible = false;
            btncommentPrompt.Visible = false;
            btncommentset.Visible = false;
            btncommentStep.Visible = false;
            btnCommentTypeofInstr.Visible = false;
            btnMeasurementSystems.Visible = false;
        }
        else
        {
            BtnApproval.Visible = true;
    
            BtnReject.Visible = true;

            btnCommentLessonInfo.Visible = true;
            btncommentLessonProcedure.Visible = true;
            btncommentPrompt.Visible = true;
            btncommentset.Visible = true;
            btncommentStep.Visible = true;
            btnCommentTypeofInstr.Visible = true;
            btnMeasurementSystems.Visible = true;
        }
    }


    private void Fillinsertdata()
    {
        try
        {
            //stdLevel.Visible = false;
            objData = new clsData();
            objData.ReturnDropDown("Select LookupId as Id , LookupCode as Name from dbo.LookUp where LookupType='Datasheet-Teaching Procedures' and ActiveInd='A'", drpTeachingProc);
            //objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='Datasheet-Prompt Procedures'", ddlPromptProcedure);
            objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='Datasheet-Prompt Procedures' ORDER BY case when LookupName = 'Progressive Time Delay' then 1 when LookupName = 'Constant Time Delay' then 2 else 3 end asc", ddlPromptProcedure);
            objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='Teaching Format'", drp_teachingFormat);




            // objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt'", lstCompletePrompts);

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void ButtonVisibility(bool visibility)
    {
        int temppId = 0;
        if (visibility == false)
        {

            lblLessonType.Visible = false;                // Visual  lesson plan link
            idOption.Visible = false;

            BtnUpdate.Visible = false;
            BtnAddMeasure.Visible = false;
            btnAddSet.Visible = false;
            btnAddSetDetails.Visible = false;
            //setPanel.Visible = false;
            if (Hdfsavemeasure.Value != "")
            {
                setPanel.Style.Add("display", "Block");
                stepPanel.Style.Add("display", "Block");   
            }
            else
            {
                setPanel.Style.Add("display", "None");
                stepPanel.Style.Add("display", "None");
            }
            btnAddPrompt.Visible = false;
            btnAddStepCriteria.Visible = false;
            btnAddSetCriteria.Visible = false;
            BtnAddStep.Visible = false;
            btnAddStepDetails.Visible = false;
            //stepPanel.Visible = false;
            BtnAddSort.Visible = false;            
            BtnSavePrompt.Visible = false;
            BtnAddPromptSelctd.Visible = false;
            BtnAddAllPrompt.Visible = false;
            BtnRemvePrmptSelctd.Visible = false;
            BtnRemoveAllPrmpt.Visible = false;
            btUpdateLessonProc.Visible = false;
            ddlPromptProcedure.Enabled = false;
            lstCompletePrompts.Visible = true;
            lstSelectedPrompts.Visible = true;
            lstCompletePrompts.Enabled = false;
            lstSelectedPrompts.Enabled = false;

            lblMeasureStart.Visible = false;
            lblSetStart.Visible = false;
            lblSetCriStart.Visible = false;
            lblStepStart.Visible = false;
            lblStepCriStart.Visible = false;
            lblPromptCriStart.Visible = false;

            lblVisibilityFalse();

        }
        else
        {

            lblLessonType.Visible = true;                // Visual  lesson plan link
            idOption.Visible = true;

            if (ViewState["HeaderId"] != null)
            {
                int headerId = Convert.ToInt32(ViewState["HeaderId"]);
                VisualPromptDisable(headerId);
            }


            BtnUpdate.Visible = true;
            BtnAddMeasure.Visible = true;
            //Hdfsavemeasure.Value = "";
            btnAddSet.Visible = true;
            btnAddSetDetails.Visible = true;
            //setPanel.Visible = true;
            setPanel.Style.Add("display", "Block");
            btnAddSetCriteria.Visible = true;
            BtnAddStep.Visible = true;
            btnAddStepDetails.Visible = true;
            //stepPanel.Visible = true;
            stepPanel.Style.Add("display", "Block");
            BtnAddSort.Visible = true;
            btnAddStepCriteria.Visible = true;
            BtnSavePrompt.Visible = true;
            btnAddPrompt.Visible = true;
            BtnAddPromptSelctd.Visible = true;
            BtnAddAllPrompt.Visible = true;
            BtnRemvePrmptSelctd.Visible = true;
            BtnRemoveAllPrmpt.Visible = true;
            btUpdateLessonProc.Visible = true;
            lstCompletePrompts.Visible = true;
            lstSelectedPrompts.Visible = true;
            lblSelctPrompt.Visible = true;
            ddlPromptProcedure.Enabled = true;
            lstCompletePrompts.Enabled = true;
            lstSelectedPrompts.Enabled = true;

            //lblMeasureStart.Visible = true;
            //lblSetStart.Visible = true;
            //lblSetCriStart.Visible = true;
            //lblStepStart.Visible = true;
            //lblStepCriStart.Visible = true;
            //lblPromptCriStart.Visible = true;

            lblVisibility(temppId);

        }
    }


    protected void VisualPromptDisable(int headerId)
    {
        string selctQuerry = "";
        objData = new clsData();

        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
        object objVt = objData.FetchValue(selctQuerry);

        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                BtnVTSavePrompt.Visible = true;
            }
            else
            {
                BtnVTSavePrompt.Visible = false;
            }
        }
        else BtnVTSavePrompt.Visible = false;
    }


    protected void lblVisibility(int TemplateId)
    {
        if (Request.QueryString["admin"] == null)
        {
            if (dlMeasureData.Items.Count > 0) lblMeasureStart.Visible = false;
            else lblMeasureStart.Visible = true;
            if (dlSetDetails.Items.Count > 0) lblSetStart.Visible = false;
            else lblSetStart.Visible = false;
            if (dlSetCriteria.Items.Count > 0) lblSetCriStart.Visible = false;
            else lblSetCriStart.Visible = true;

            if (dlStepDetails.Items.Count > 0) lblStepStart.Visible = false;
            else lblStepStart.Visible = true;

            if (dlStepCriteria.Items.Count > 0) lblStepCriStart.Visible = false;
            else lblStepCriStart.Visible = true;

            if (dlPromptCriteria.Items.Count > 0) lblPromptCriStart.Visible = false;
            else lblPromptCriStart.Visible = true;


            if (TemplateId > 0)
            {
                lblEnableIfDiscrete(TemplateId);
                lblEnableIfVisualTool(TemplateId);
            }
        }
    }


    protected void lblEnableIfDiscrete(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        string chainType = "";
        int teachId = 0;
        string teachName = "";
        object objTeach = null;
        try
        {
            strQuerry = "SELECT SkillType,TeachingProcId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    chainType = dtNew.Rows[0]["SkillType"].ToString();
                    try
                    {
                        teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                    }
                    catch
                    {
                        teachId = 0;
                    }
                    if (teachId > 0)
                    {
                        try
                        {
                            strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                            objTeach = objData.FetchValue(strQuerry);
                            if (objTeach != null)
                            {
                                teachName = objTeach.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    if (chainType == "Discrete")
                    {

                        lblStepStart.Visible = false;
                        lblStepCriStart.Visible = false;
                    }
                    else
                    {
                        if (teachName == "Total Task")
                        {
                            if (dlStepDetails.Items.Count > 0) lblStepStart.Visible = false;
                            else lblStepStart.Visible = true;
                            lblStepCriStart.Visible = false;
                        }
                        else
                        {
                            if (dlStepDetails.Items.Count > 0) lblStepStart.Visible = false;
                            else lblStepStart.Visible = true;
                            if (dlStepCriteria.Items.Count > 0) lblStepCriStart.Visible = false;
                            else lblStepCriStart.Visible = true;
                        }

                    }
                }
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }



    protected void lblEnableIfVisualTool(int TemplateId)
    {
        objData = new clsData();
        string selctQuerry = "";
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
        object objVt = objData.FetchValue(selctQuerry);

        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                lblSetStart.Visible = false;
                lblStepStart.Visible = false;
                lblStepCriStart.Visible = false;

                // ControlsDisableOnVisual(false);
            }
            else
            {
                if (dlSetDetails.Items.Count > 0) lblSetStart.Visible = false;
                else lblSetStart.Visible = false;
                if (dlSetCriteria.Items.Count > 0) lblSetCriStart.Visible = false;
                else lblSetCriStart.Visible = true;

                if (dlStepDetails.Items.Count > 0) lblStepStart.Visible = false;
                else lblStepStart.Visible = true;

                if (dlStepCriteria.Items.Count > 0) lblStepCriStart.Visible = false;
                else lblStepCriStart.Visible = true;

                lblEnableIfDiscrete(TemplateId);

                //   ControlsDisableOnVisual(true);
                // IsDiscrete(TemplateId);
            }
        }
        else
        {
            if (dlSetDetails.Items.Count > 0) lblSetStart.Visible = false;
            else lblSetStart.Visible = false;
            if (dlSetCriteria.Items.Count > 0) lblSetCriStart.Visible = false;
            else lblSetCriStart.Visible = true;

            if (dlStepDetails.Items.Count > 0) lblStepStart.Visible = false;
            else lblStepStart.Visible = true;

            if (dlStepCriteria.Items.Count > 0) lblStepCriStart.Visible = false;
            else lblStepCriStart.Visible = true;

            lblEnableIfDiscrete(TemplateId);
        }


    }



    #region OVERLOADING FUNCTIONS
    clsData oData_ov = null;
    ClsTemplateSession oTemp = new ClsTemplateSession();
    clsSession oSession = null;

    protected void RadioButtonListSets_SelectedIndexChanged(object sender, EventArgs e)
    {
        oData_ov = new clsData();
        hdnRadBtnSet.Value = RadioButtonListSets.SelectedValue.ToString();
        Session["iCurrentSetId"] = RadioButtonListSets.SelectedValue.ToString();
        loadStepOverrid(Convert.ToInt32(RadioButtonListSets.SelectedValue.ToString()));
    }

    protected void RadioButtonListSteps_SelectedIndexChanged(object sender, EventArgs e)
    {
        oData_ov = new clsData();
        //hdnRadBtnStep.Value = Convert.ToString(oData_ov.FetchValue("SELECT SortOrder FROM DSTempStep WHERE DSTempStepId=" + RadioButtonListSteps.SelectedValue));
        //Session["iCurrentStep"] = oData_ov.FetchValue("SELECT SortOrder FROM DSTempStep WHERE DSTempStepId=" + RadioButtonListSteps.SelectedValue);
        string chainType = oData_ov.FetchValue("select chaintype from DSTempStep as dsts join DSTempHdr as dsth on dsts.DSTempHdrId = dsth.DSTempHdrId where dsts.dstempstepid = " + RadioButtonListSteps.SelectedValue).ToString();

        //if (chainType == "Backward chain")
        //{
            Session["iCurrentStep"] = RadioButtonListSteps.SelectedIndex + 1;
        //}
        //else
        //{
        //    Session["iCurrentStep"] = oData_ov.FetchValue("SELECT SortOrder FROM DSTempStep WHERE DSTempStepId=" + RadioButtonListSteps.SelectedValue);
        //}

    }
    protected void RadioButtonListPrompts_SelectedIndexChanged(object sender, EventArgs e)
    {
        oData_ov = new clsData();
        hdnRadBtnPrompt.Value = Convert.ToString(RadioButtonListPrompts.SelectedValue);
        Session["sCurrentPrompt"] = oData_ov.FetchValue("SELECT PromptId FROM DSTempPrompt WHERE DSTempPromptId=" + RadioButtonListPrompts.SelectedValue);
    }

    protected void RadioButtonListSets_tt_SelectedIndexChanged(object sender, EventArgs e)
    {
        hdnRadBtnSet.Value = RadioButtonListSets_tt.SelectedValue.ToString();
        loadStepOverrid(Convert.ToInt32(RadioButtonListSets_tt.SelectedValue.ToString()));
        Session["iCurrentSetId"] = RadioButtonListSets_tt.SelectedValue.ToString();
    }

    public void loadSetsOverride(int prevTempId, int NewTempId)
    {
        //Clearing Override Div for showing Previous lessons Steps for in MTS issue [7-May-2020] -- Start
        RadioButtonListSteps.Items.Clear();
        rptr_listStep.DataSource = null;
        rptr_listStep.DataBind();
        //Clearing Override Div for showing Previous lessons Steps for in MTS issue [7-May-2020] -- End
        oData_ov = new clsData();
        //if (Convert.ToInt32(oData_ov.FetchValue("SELECT COUNT(*) FROM StdtSessionHdr WHERE (SessionStatusCd='D' OR SessionStatusCd='S') AND DSTempHdrId=" + prevTempId)) > 0)
		//if (Convert.ToInt32(oData_ov.FetchValue("SELECT COUNT(*) FROM StdtDSStat WHERE DSTempHdrId=" + prevTempId)) > 0)
        // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 - Start
        if (prevTempId == 0)
        {
            Session["Selection"] = "0";  // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
            prevTempId = NewTempId;
            lblsetrev.Text = "No prior Set";
            lblsteprev.Text = "No Prior Step";
            lblpromptrev.Text = "No prior Prompt"; 
        }
        else
        {
            Session["Selection"] = "1"; // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
            Session["sortOrder"] =Convert.ToInt32 (oData_ov.FetchValue("SELECT NextStepId from StdtDSStat  where DSTempHdrId =" + prevTempId)); // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
            DataTable DTNames = oData_ov.ReturnDataTable("SELECT NextSetId,(SELECT SetCd FROM DSTempSet WHERE DSTempSetId= NextSetId) SetName,(SELECT TOP 1 CASE WHEN StepCd IS NULL THEN StepName ELSE StepCd END FROM DSTempStep WHERE DSTempSetId=NextSetId AND SortOrder= NextStepId) StepName,(SELECT LookupName FROM LookUp WHERE LookupId= NextPromptId) PromptName FROM StdtDSStat WHERE DSTempHdrId=" + prevTempId, false);
            lblsetrev.Text = Convert.ToString(DTNames.Rows[0]["SetName"]);
            lblsteprev.Text = Convert.ToString(DTNames.Rows[0]["StepName"]);
            lblpromptrev.Text = Convert.ToString(DTNames.Rows[0]["PromptName"]);
            Session["oldSetId"] = Convert.ToString(DTNames.Rows[0]["NextSetId"]);
            Session["PrevStep"] = lblsteprev.Text; // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
            Session["PrevPrompt"] = lblpromptrev.Text; // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
        }
        // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 - End
            Session["OldTempId"] = prevTempId;
            string Type = "SELECT ChainType,TotalTaskType FROM DSTempHdr where DSTempHdrId=" + NewTempId;
            DataTable dataT = oData_ov.ReturnDataTable(Type, false);

            string type = dataT.Rows[0]["ChainType"].ToString();
            string totalTasktype = dataT.Rows[0]["TotalTaskType"].ToString();

            if (totalTasktype == "1")
            {
                totalTaskOverride.Visible = true;
                normalOverride.Visible = false;
            }
            if (totalTasktype == "0" || totalTasktype == "")
            {
                totalTaskOverride.Visible = false;
                normalOverride.Visible = true;
            }

            int iCurrentSetId = 0;

            SqlDataReader reader = null;
            string strQry = " SELECT Hdr.SkillType, ISNULL(MAX(NextSetId),0) NextSetId,ISNULL(MAX(NextSetNmbr),0) NextSetNmbr " +
                     " ,ISNULL(MAX(NextStepId),0)NextStepId,ISNULL(MAX(NextPromptId),0)NextPromptId" +
                     " FROM DSTempHdr Hdr LEFT JOIN StdtDSStat Stat  ON Hdr.DSTempHdrId = Stat.DSTempHdrId " +
                      " WHERE Hdr.DSTempHdrId= " + prevTempId + " GROUP BY Hdr.SkillType ";

            reader = oData_ov.ReturnDataReader(strQry, false);
            if (reader.Read())
            {
                Session["iCurrentSetId"] = oData_ov.FetchValue("SELECT DSTempSetId FROM DSTempSet WHERE DSTempHdrId=" + NewTempId + " AND SetCd=(SELECT SetCd FROM DSTempSet WHERE DSTempHdrId=" + prevTempId + " AND DSTempSetId=" + Convert.ToInt32(reader["NextSetId"]) + ") AND SetName=(SELECT SetName FROM DSTempSet WHERE DSTempHdrId=" + prevTempId + " AND DSTempSetId=" + Convert.ToInt32(reader["NextSetId"]) + ")");
                Session["iCurrentStep"] = Convert.ToInt32(reader["NextStepId"]);
                string sCurrentPrompt = reader["NextPromptId"].ToString();
                if (!String.IsNullOrEmpty(sCurrentPrompt))
                    Session["sCurrentPrompt"] = Convert.ToInt32(sCurrentPrompt);
            }
            reader.Close();
            if (Session["iCurrentSetId"] != null && Convert.ToString(Session["iCurrentSetId"]) != "")
            {

                iCurrentSetId = Convert.ToInt32(Session["iCurrentSetId"]);
                //hdnRadBtnSet.Value = iCurrentSetId.ToString();
            }
            oTemp = (ClsTemplateSession)Session["BiweeklySession"];
            if (oTemp != null)
            {
                oSession = (clsSession)HttpContext.Current.Session["UserSession"];
                if (oSession != null)
                {

                    string sqlStr = "select DSTempSetId,SetCd,SetName,SortOrder from DSTempSet where DSTempHdrId=" + NewTempId + " AND ActiveInd = 'A' order by SortOrder";
                    DataSet ds = oData_ov.ReturnDataSet(sqlStr, false);
                    int n = ds.Tables[0].Rows.Count; // Approval Popup Issue New Lesson Creation [6 - jul -2020] - Dev 1 
                    RadioButtonListSets.DataSource = ds;
                    RadioButtonListSets.DataTextField = "SetCd";
                    RadioButtonListSets.DataValueField = "DSTempSetId";
                    RadioButtonListSets.DataBind();
                    // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 -- Start
                    if (RadioButtonListSets.Items.Count != 0)
                    {
                        if (prevTempId != NewTempId)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                if (ds.Tables[0].Rows[i]["SetCd"].ToString() == lblsetrev.Text)
                                    RadioButtonListSets.Items[i].Selected = true;
                            }
                        }
                        else
                        {
                            RadioButtonListSets.Items[0].Selected = true;
                        }
                    }
                    // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 -- End
                    RadioButtonListSets_tt.DataSource = ds;
                    RadioButtonListSets_tt.DataTextField = "SetCd";
                    RadioButtonListSets_tt.DataValueField = "DSTempSetId";
                    RadioButtonListSets_tt.DataBind();
                    // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 -- Start
                    if (RadioButtonListSets.Items.Count != 0)
                    {
                        if (prevTempId != NewTempId)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                String s = ds.Tables[0].Rows[i]["SetCd"].ToString();
                                if (s == lblsetrev.Text)
                                    RadioButtonListSets.Items[i].Selected = true;
                            }
                        }
                        else
                            RadioButtonListSets.Items[0].Selected = true;
                    }
                    // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1 -- End
                    rptr_ListSets.DataSource = ds;
                    rptr_ListSets.DataBind();



                }
            }
            if (iCurrentSetId > 0)
            {
                for (int i = 0; i < RadioButtonListSets.Items.Count; i++)
                {
                    if (Convert.ToInt32(RadioButtonListSets.Items[i].Value) == iCurrentSetId)
                    {
                        //RadioButtonListSets.Items[i].Selected = true;
                        //RadioButtonListSets_tt.Items[i].Selected = true;
                        loadStepOverrid(iCurrentSetId);
                    }
                }

            }
            // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
            if (RadioButtonListSets.Items.Count != 0)
            {
                if (prevTempId == NewTempId)
                {
                    Session["iCurrentSetId"] = RadioButtonListSets.SelectedValue.ToString();
                    loadStepOverrid(Convert.ToInt32(RadioButtonListSets.SelectedValue.ToString()));
                    Session["iCurrentStep"] = 1;
                }
                // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
                loadPromptOverrid(NewTempId);
            }
    }

    private int getPrevVersion()
    {
        sess = (clsSession)Session["UserSession"];
        oData_ov = new clsData();
        int prevCount = 0; // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
        int i = 0; // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
        string query = "select dstemphdrid from DSTempHdr where lessonplanid = (SELECT lessonplanid FROM DSTempHdr where dstemphdrid = " + Convert.ToInt32(ViewState["HeaderId"]) + ") and studentid = " + sess.StudentId + " order by dstemphdrid desc";  // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
        DataTable dt = oData_ov.ReturnDataTable(query, false);

        prevCount = Convert.ToInt32(oData_ov.FetchValue("select Count(dstemphdrid) from DSTempHdr where lessonplanid = (SELECT lessonplanid FROM DSTempHdr where dstemphdrid = " + Convert.ToInt32(ViewState["HeaderId"]) + ") and studentid = " + sess.StudentId)); // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
        if (dt.Rows.Count > 1)
        {
            // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  - Start
            for (i = 1; i < prevCount; i++)
            {
                int prevTempId = Convert.ToInt32(dt.Rows[i]["dstemphdrid"].ToString());
                if (Convert.ToInt32(oData_ov.FetchValue("SELECT COUNT(*) FROM StdtDSStat WHERE DSTempHdrId=" + prevTempId)) > 0)
                    return prevTempId;

            }
            // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  - End
        }

        return 0;
    }

    private void loadPromptOverrid(int NewTempId)
    {
        int iCurrentPrompt = 0;
        string sqlStr = "";
        string type = "";
        //if (Session["sCurrentPrompt"] != null && Convert.ToString(Session["sCurrentPrompt"]) != "")
        //{
        //    iCurrentPrompt = Convert.ToInt32(Session["sCurrentPrompt"]);
        //    hdnRadBtnPrompt.Value = iCurrentPrompt.ToString();
        //}
        oData_ov = new clsData();        
        //oTemp = (ClsTemplateSession)Session["BiweeklySession"];
        if (oTemp != null)
        {
            sqlStr = "SELECT lk.LookupName FROM DSTempHdr hd inner join lookup lk on lk.LookupId=hd.PromptTypeId where DSTempHdrId=" + NewTempId;
            type = oData_ov.FetchValue(sqlStr).ToString();
            oSession = (clsSession)HttpContext.Current.Session["UserSession"];
            if (oSession != null)
            {

                if ((type == "Least-to-Most") || (type == "Graduated Guidance"))
                {
                    sqlStr = "SELECT LU.LookupId as Id,LU.LookupName as Name,DS.DSTempPromptid FROM LookUp LU INNER JOIN DSTempPrompt DS ON DS.PromptId=LU.LookupId WHERE" +
                                            " DS.ActiveInd='A' AND DS.DSTempHdrId=" + NewTempId + " ORDER BY SortOrder DESC";
                }
                else
                {
                    sqlStr = "SELECT LU.LookupId as Id,LU.LookupName as Name,DS.DSTempPromptid FROM LookUp LU INNER JOIN DSTempPrompt DS ON DS.PromptId=LU.LookupId WHERE" +
                                " DS.ActiveInd='A' AND DS.DSTempHdrId=" + NewTempId + " ORDER BY SortOrder ";
                }
                //sqlStr = "select DP.DSTempPromptid,LK.LookupName,DP.PromptOrder from DSTempPrompt DP join LookUp LK on LK.LookupId=DP.PromptId where DSTempHdrId=" + oTemp.TemplateId + " order by DP.PromptOrder";
                DataSet ds = oData_ov.ReturnDataSet(sqlStr, false);
                RadioButtonListPrompts.DataSource = ds;
                RadioButtonListPrompts.DataTextField = "Name";
                RadioButtonListPrompts.DataValueField = "DSTempPromptid";
                RadioButtonListPrompts.DataBind();
                // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
				if(ds.Tables[0].Rows.Count > 0)
                {
                    if (Session["Selection"] == "1")
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (ds.Tables[0].Rows[i]["Name"].ToString() == Session["Prevprompt"].ToString())
                                RadioButtonListPrompts.Items[i].Selected = true;
                        }
                    }
                    else
                    {
                        RadioButtonListPrompts.Items[0].Selected = true;
                        Session["sCurrentPrompt"] = oData_ov.FetchValue("SELECT PromptId FROM DSTempPrompt WHERE DSTempPromptId=" + Convert.ToInt32(RadioButtonListPrompts.SelectedValue.ToString()));
                    }
				}
                // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1


            }
        }
        //
        if (iCurrentPrompt > 0)
        {
            sqlStr = "select DP.DSTempPromptid from DSTempPrompt DP join LookUp LK on LK.LookupId=DP.PromptId where DSTempHdrId=" + NewTempId + ""
                + " and LK.LookupId=" + iCurrentPrompt + " order by DP.PromptOrder";
            iCurrentPrompt = Convert.ToInt32(oData_ov.FetchValue(sqlStr));
            //for (int i = 0; i < RadioButtonListPrompts.Items.Count; i++)
            //{
            //    if (Convert.ToInt32(RadioButtonListPrompts.Items[i].Value) == iCurrentPrompt)
            //    {
            //        RadioButtonListPrompts.Items[i].Selected = true;

            //    }
            //}
        }

    }

    private void loadStepOverrid(int setid)
    {
        int iCurrentStep = 0;
        //if (Session["iCurrentStep"] != null && Session["iCurrentStep"] != "")
        //{
        //    iCurrentStep = Convert.ToInt32(Session["iCurrentStep"]);
        //hdnRadBtnStep.Value = iCurrentStep.ToString();
        //}
        oData_ov = new clsData();
        oTemp = (ClsTemplateSession)Session["BiweeklySession"];
        oTemp.TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        string type = "";
        if (oTemp != null)
        {
            oSession = (clsSession)HttpContext.Current.Session["UserSession"];
            if (oSession != null)
            {
                string sqlStr = "SELECT ChainType,TotalTaskType FROM DSTempHdr where DSTempHdrId=" + oTemp.TemplateId;
                DataTable dataT = oData_ov.ReturnDataTable(sqlStr, false);

                type = dataT.Rows[0]["ChainType"].ToString();
                string totalTasktype = dataT.Rows[0]["TotalTaskType"].ToString();
                //DataTable DToldTempData = null;

                if (totalTasktype != "1")
                {
                    sqlStr = "select DSTempHdrId,DSTempStepId,StepCd,StepName,SortOrder from DSTempStep where DSTempHdrId=" + oTemp.TemplateId + " and DSTempSetId=" + setid + " and ActiveInd = 'A' AND IsDynamic=0 order by SortOrder";

                }
                else
                {
                    //sqlStr = "select dsts.DSTempHdrId,dsts.DSTempStepId,dsts.StepCd,dsts.StepName,dsts.SortOrder, sdsss.PromptId from DSTempStep as dsts LEFT JOIN StdtDSStepStat sdsss on dsts.DSTempStepId = sdsss.DSTempStepId where dsts.DSTempHdrId=" + Session["OldTempId"] + " and dsts.DSTempSetId=" + Session["oldSetId"] + " and dsts.ActiveInd = 'A' AND IsDynamic=0 order by dsts.SortOrder";
                    //DToldTempData = oData_ov.ReturnDataTable(sqlStr, false);
                    sqlStr = "select dsts.DSTempHdrId,dsts.DSTempStepId,dsts.StepCd,dsts.StepName,dsts.SortOrder, sdsss.PromptId from DSTempStep as dsts LEFT JOIN StdtDSStepStat sdsss on dsts.DSTempStepId = sdsss.DSTempStepId where dsts.DSTempHdrId=" + oTemp.TemplateId + " and dsts.DSTempSetId=" + setid + " and dsts.ActiveInd = 'A' AND IsDynamic=0 order by dsts.SortOrder";

                }
                if (type == "Backward chain")
                {
                    if (totalTasktype != "1")
                    {

                        sqlStr = "SELECT [DSTempHdrId],[DSTempStepId],[StepCd],[StepName],RANK() OVER(ORDER BY SortOrder ASC) as StepId  FROM [dbo].[DSTempStep] " +
                                 "WHERE DSTempHdrId=" + oTemp.TemplateId + " AND DsTempSetId=" + setid + " AND ActiveInd='A' AND IsDynamic=0 ORDER BY [SortOrder] DESC";

                    }
                    else
                    {
                        //sqlStr = "SELECT dsts.DSTempHdrId,dsts.DSTempStepId,dsts.StepCd,dsts.StepName,RANK() OVER(ORDER BY dsts.SortOrder ASC) as StepId,dsts.SortOrder, sdsss.PromptId " +
                        //         "FROM [DSTempStep] as dsts LEFT JOIN StdtDSStepStat sdsss ON dsts.DSTempStepId = sdsss.DSTempStepId " +
                        //         "WHERE dsts.DSTempHdrId= " + Session["OldTempId"] + " AND dsts.DsTempSetId=" + Session["oldSetId"] + " AND dsts.ActiveInd='A' AND IsDynamic=0 ORDER BY dsts.SortOrder DESC";
                        //DToldTempData = oData_ov.ReturnDataTable(sqlStr, false);

                        sqlStr = "SELECT dsts.DSTempHdrId,dsts.DSTempStepId,dsts.StepCd,dsts.StepName,RANK() OVER(ORDER BY dsts.SortOrder ASC) as StepId,dsts.SortOrder, sdsss.PromptId " +
                                 "FROM [DSTempStep] as dsts LEFT JOIN StdtDSStepStat sdsss ON dsts.DSTempStepId = sdsss.DSTempStepId " +
                                 "WHERE dsts.DSTempHdrId= " + oTemp.TemplateId + " AND dsts.DsTempSetId=" + setid + " AND dsts.ActiveInd='A' AND IsDynamic=0 ORDER BY dsts.SortOrder DESC";
                    }
                }
                DataTable dt = oData_ov.ReturnDataTable(sqlStr, false);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["StepCd"].ToString() != "" && dr["StepCd"].ToString() != null)
                    {
                        if (totalTasktype != "1")
                        {
                            //Code added for approval popup issue [29-06-2020] dev 1 start
                            if (Session["Selection"] == "1")
                            {
                                if (type == "Backward chain")
                                {
                                    int sOrder = Convert.ToInt32(Session["sortOrder"]);
                                    int n = dt.Rows.Count;
                                    int s = (sOrder - 1);
                                    lblsteprev.Text = dt.Rows[(sOrder - 1)]["StepCd"].ToString();
                                }
                                Session["PrevStep"] = lblsteprev.Text;
                            }
                            //END  
                            RadioButtonListSteps.DataSource = dt;
                            RadioButtonListSteps.DataTextField = "StepCd";
                            RadioButtonListSteps.DataValueField = "DSTempStepId";
                            RadioButtonListSteps.DataBind();
                            // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  
							if (dt.Rows.Count > 0)
                                if (Session["Selection"] == "1")
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {

                                        if (dt.Rows[i]["StepCd"].ToString() == Session["PrevStep"].ToString())
                                            RadioButtonListSteps.Items[i].Selected = true;
                                    }
                                }
                                else
								    RadioButtonListSteps.Items[0].Selected = true;
                            // Approval Popup Issue New Lesson Creation [18 - jun -2020] - Dev 1  r
                        }
                        else
                        {

                            //foreach (DataRow row in DToldTempData.Rows)
                            //{
                            //    if (!DBNull.Value.Equals(row["PromptId"]))
                            //    {
                            //        if (Convert.ToString(row["StepCd"]) == Convert.ToString(dr["StepCd"]) && Convert.ToString(row["SortOrder"]) == Convert.ToString(dr["SortOrder"]))
                            //        {
                            //            dr["PromptId"] = row["PromptId"];
                            //        }
                            //    }
                            //}


                            rptr_listStep.DataSource = dt;
                            rptr_listStep.DataBind();
                            Session["OldTempId"] = null;
                            Session["oldSetId"] = null;
                        }

                    }
                }
                if (dt.Rows.Count == 0)
                {
                    RadioButtonListSteps.Items.Clear();
                    rptr_listStep.DataSource = null;
                    rptr_listStep.DataBind();

                }
            }
        }

        if (iCurrentStep > 0)
        {
            if (type == "Backward chain")
            {
                iCurrentStep = (RadioButtonListSteps.Items.Count - iCurrentStep) + 1;
            }
            string sqlStr = "select DSTempStepId from DSTempStep where DSTempHdrId=" + oTemp.TemplateId + " "
                + "and DSTempSetId=" + setid + " and SortOrder=" + iCurrentStep + " AND IsDynamic=0 order by SortOrder";
            iCurrentStep = Convert.ToInt32(oData_ov.FetchValue(sqlStr));
            //for (int i = 0; i < RadioButtonListSteps.Items.Count; i++)
            //{
            //    if (Convert.ToInt32(RadioButtonListSteps.Items[i].Value) == iCurrentStep)
            //    {
            //        RadioButtonListSteps.Items[i].Selected = true;

            //    }
            //}

        }
        //else
        //{
        //    if (hdnRadBtnStep.Value == "")
        //    {
        //        if (RadioButtonListSteps.Items.Count > 0)
        //        {
        //            RadioButtonListSteps.Items[0].Selected = true;
        //        }
        //    }
        //}


    }

    protected void rptr_listStep_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            DropDownList promptList = e.Item.FindControl("stepDDL") as DropDownList;
            HiddenField hdn_dsTempHdrId = e.Item.FindControl("hdn_dsTempHdrId") as HiddenField;
            HiddenField hdn_promptIdValue = e.Item.FindControl("hdn_stepDDLValue") as HiddenField;

            System.Data.DataSet ds = new System.Data.DataSet();
            ds = getPromptList(hdn_dsTempHdrId.Value);

            promptList.DataSource = ds.Tables[0];
            promptList.DataTextField = "LookupName";
            promptList.DataValueField = "PromptId";

            promptList.DataBind();

            promptList.Items.Insert(0, new ListItem("--- Select ---", ""));

            promptList.SelectedValue = hdn_promptIdValue.Value;
        }
    }

    protected void btnOverride_Click(object sender, EventArgs e)
    {
        /// FUNCTION WHEN CLICKED OVERRIDE
    }

    public System.Data.DataSet getPromptList(string dsTempHdrId)
    {

        string sqlStr = "";
        string type = "";
        DataSet ds = new DataSet();
        ds = null;
        oData_ov = new clsData();
        //oTemp = (ClsTemplateSession)Session["BiweeklySession"];
        if (oTemp != null)
        {
            sqlStr = "SELECT lk.LookupName FROM DSTempHdr hd inner join lookup lk on lk.LookupId=hd.PromptTypeId where DSTempHdrId=" + oTemp.TemplateId;
            type = oData_ov.FetchValue(sqlStr).ToString();
            oSession = (clsSession)HttpContext.Current.Session["UserSession"];
            if (oSession != null)
            {


                if ((type == "Least-to-Most")||(type=="Graduated Guidance"))
                {
                    sqlStr = "SELECT dstp.DSTempPromptId, dstp.DSTempHdrId, dstp.PromptId, dstp.PromptOrder, lu.LookupName FROM DSTempPrompt as dstp join [LookUp] as lu on dstp.PromptId = lu.LookupId WHERE (dstp.DSTempHdrId = " + dsTempHdrId + " AND dstp.ActiveInd = 'A') ORDER BY SortOrder DESC";

                }
                else
                {
                    sqlStr = "SELECT dstp.DSTempPromptId, dstp.DSTempHdrId, dstp.PromptId, dstp.PromptOrder, lu.LookupName FROM DSTempPrompt as dstp join [LookUp] as lu on dstp.PromptId = lu.LookupId WHERE (dstp.DSTempHdrId = " + dsTempHdrId + " AND dstp.ActiveInd = 'A') ORDER BY SortOrder";

                }
                ds = oData_ov.ReturnDataSet(sqlStr, false);


            }
        }

        return ds;

    }

    protected void btn_sets_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        loadStepOverrid(Convert.ToInt32(btn.ToolTip));
    }

    #endregion




    protected void lblVisibilityFalse()
    {
        lblMeasureStart.Visible = false;
        lblSetStart.Visible = false;
        lblSetCriStart.Visible = false;
        lblStepStart.Visible = false;
        lblStepCriStart.Visible = false;
        lblPromptCriStart.Visible = false;

    }

    protected void FillApprovedLessonData()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string selctQuerry = "";

        try
        {
            if (sess != null)
            {

                selctQuerry = "SELECT  Name,GoalId,ID  " +
                               "    ,StatusId,LookupName,LessonOrder FROM   " +
                                        " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                                    " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                                       " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                                           " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                                                        " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                                                        "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                                                     "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName = 'Approved'" +
                                                  "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";


                DataTable dtList = objData.ReturnDataTable(selctQuerry, false);
                dlApprovedLessons.DataSource = dtList;
                dlApprovedLessons.DataBind();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void FillMaintenanceLessonData()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string selctQuerry = "";

        try
        {
            if (sess != null)
            {

                selctQuerry = "SELECT  Name,GoalId,ID  " +
                               "    ,StatusId,LookupName,LessonOrder FROM   " +
                                        " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                                    " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                                       " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                                           " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                                                        " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                                                        "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                                                     "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName = 'Maintenance'" +
                                                  "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";


                DataTable dtList = objData.ReturnDataTable(selctQuerry, false);
                dlMaintenanceLp.DataSource = dtList;
                dlMaintenanceLp.DataBind();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void FillInactiveLessonData()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string selctQuerry = "";

        try
        {
            if (sess != null)
            {

                selctQuerry = "SELECT  Name,GoalId,ID  " +
                               "    ,StatusId,LookupName,LessonOrder FROM   " +
                                        " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                                    " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                                       " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                                           " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                                                        " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                                                        "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                                                     "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName = 'Inactive'" +
                                                  "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";


                DataTable dtList = objData.ReturnDataTable(selctQuerry, false);
                dlInactiveLp.DataSource = dtList;
                dlInactiveLp.DataBind();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void FillRejectedLessons()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string selctQuerry = "";

        try
        {
            if (sess != null)
            {

                selctQuerry = "SELECT  Name,GoalId,ID  " +
                               "    ,StatusId,LookupName,LessonOrder FROM   " +
                                        " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                                    " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                                       " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                                           " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                                                        " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                                                        "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                                                     "  StdtLP.ActiveInd='A' AND LU.LookupName = 'Expired'" +
                                                  "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";


                DataTable dtList = objData.ReturnDataTable(selctQuerry, false);
                dlRejectedLp.DataSource = dtList;
                dlRejectedLp.DataBind();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    private void FillData()
    {
        objData = new clsData();
        oData = new DataClass();
        oLessons = new clsLessons();
        sess = (clsSession)Session["UserSession"];
        string selectQuerry = "";
        try
        {
            if (sess != null)
            {

                selectQuerry = "SELECT  Name,GoalId,ID  " +
                                   "    ,StatusId,LookupName,LessonOrder FROM   " +
                                   " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                                   " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                                   " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                                   " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                                   " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                                   "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                                   "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName<>'Approved' AND LU.LookupName = 'In Progress' " +
                                   "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";


                DataTable dtList = objData.ReturnDataTable(selectQuerry, false);
                dlLP.DataSource = null;
                dlLP.DataBind();

                dlLP.DataSource = dtList;                                               // Fill the subtype lesson for te specified goal.
                dlLP.DataBind();
                //BtnMaintenance.Visible = false;
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    private void FillCompltdLessonPlans()
    {
        objData = new clsData();
        oData = new DataClass();
        oLessons = new clsLessons();
        sess = (clsSession)Session["UserSession"];
        string selectQuerry = "";
        try
        {
            if (sess != null)
            {



                selectQuerry = "SELECT  Name,GoalId,ID  " +
                             "    ,StatusId,LookupName,LessonOrder FROM   " +
                             " (SELECT DTmp.DSTemplateName+' '+isnull(DTmp.VerNbr,'') as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID   " +
                             " ,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder,ROW_NUMBER() over (partition by DTmp.DSTempHdrId order by MAX(StdtLP.GoalId) DESC) frow " +
                             " FROM (StdtLessonPlan StdtLP INNER JOIN  " +
                             " (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId) " +
                             " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId)   " +
                             "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND  " +
                             "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName = 'Pending Approval'" +
                             "  GROUP BY DTmp.DSTemplateName,DTmp.VerNbr,StdtLP.GoalId, DTmp.DSTempHdrId,DTmp.StatusId,LU.LookupName,DTmp.LessonOrder) data WHERE frow=1 order by LessonOrder";

                DataTable dtList = objData.ReturnDataTable(selectQuerry, false);
                dlCompltdLessonPlans.DataSource = dtList;                                               // Fill the subtype lesson for te specified goal.
                dlCompltdLessonPlans.DataBind();
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    private void fillStepAndSet(int TempId)
    {
        string strQuery = "";
        string strBinderSet = "";
        string strBinderStep = "";
        DataTable Dt = new DataTable();
        sess = (clsSession)Session["UserSession"];

        objData = new clsData();
        // strQuery = "select StepName,StepCd from dbo.DSTempStep where DSTempHdrId = " + TempId + " AND ActiveInd = 'A'";

        strQuery = "SELECT  DSTempParentStepId,SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,SortOrder,SetIds,SetNames FROM DSTempParentStep"
                       + " WHERE DSTempHdrId = " + TempId + " And ActiveInd = 'A' ORDER BY DSTempSetId,SortOrder";

        strBinderStep = "<ul>";
        Dt = objData.ReturnDataTable(strQuery, false);

        if (Dt != null)
        {
            foreach (DataRow Dr in Dt.Rows)
            {
                strBinderStep += "<li>" + Dr["StepCd"].ToString() + "-" + Dr["StepName"].ToString() + "</li>";
            }
        }
        tdStep.InnerHtml = "<b>Step Description(s):</b><img src='images/toolTipQMark.png' style='height:14px;width:14px' title='Automatically lists all the STEPS. If you need to change or add STEPS, go back to the STEP tab'/>" + strBinderStep.ToString() + "</ul>";



        strQuery = "select SetName,SetCd from dbo.DSTempSet where DSTempHdrId = " + TempId + " AND ActiveInd = 'A'";
        strBinderSet = "<ul>";
        Dt = objData.ReturnDataTable(strQuery, false);

        if (Dt != null)
        {
            foreach (DataRow Dr in Dt.Rows)
            {
                strBinderSet += "<li>" + Dr["SetCd"].ToString() + "-" + Dr["SetName"].ToString() + "</li>";
            }
        }

        tdSet.InnerHtml = "<b>Set Description(s):</b><img src='images/toolTipQMark.png' style='height:14px;width:14px' title='Automatically lists all the SETS. If you need to change or add SETS, go back to the SET tab'/>" + strBinderSet.ToString() + "</ul>";


    }

    //protected void rdolistDatasheet_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    objData = new clsData();
    //    if (rdolistDatasheet.SelectedValue == "0")
    //    {
    //        showtrailid.Visible = true;
    //        txtNoofTrail.Visible = true;
    //        taskAnalysis.Visible = false;
    //        drpTasklist.Visible = false;
    //        txtNoofTrail.Text = "";
    //    }
    //    else
    //    {
    //        showtrailid.Visible = false;
    //        txtNoofTrail.Visible = false;
    //        taskAnalysis.Visible = true;BtnPreview_Click
    //        drpTasklist.Visible = true;
    //        drpTasklist.SelectedIndex = 0;
    //    }
    //}


    protected void chkDiscrete_CheckedChanged(object sender, System.EventArgs e)
    {
        objData = new clsData();
        if (chkDiscrete.Checked == true)
        {
            showMatchToSampleDrop();
            showtrailid.Visible = true;
            txtNoofTrail.Visible = true;
            taskAnalysis.Visible = false;
            drpTasklist.Visible = false;
            txtNoofTrail.Text = "";
        }
        else
        {
            showMatchToSampleDrop();
            //showtrailid.Visible = false;
            //txtNoofTrail.Visible = false;
            taskAnalysis.Visible = true;
            drpTasklist.Visible = true;
            drpTasklist.SelectedIndex = 0;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }


    protected void GetSetData(int headerId)
    {
        objData = new clsData();
        string selQuerry = "";
        try
        {
            selQuerry = "SELECT DSTempSetId, SetCd, SetName,SortOrder FROm DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A' ORDER BY SortOrder";
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            DataTable dtUpdated = new DataTable();
            dtUpdated = dtList.Clone();
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtList.Rows)
                    {
                        foreach (DataColumn col in dtList.Columns)
                        {
                            if (col.ColumnName == "SetCd")
                            {

                                dr[col] = dr[col].ToString().Replace("&qt;", "'");
                                dr[col] = dr[col].ToString().Replace("?s", "'s");
                                dr[col] = dr[col].ToString().Replace("&lqt", "‘");
                                dr[col] = dr[col].ToString().Replace("?", "");
                                dr[col] = dr[col].ToString().Replace("&rqt", "’");
                            }
                            if (col.ColumnName == "SetName")
                            {
                                dr[col] = dr[col].ToString().Replace("&qt;", "'");
                                dr[col] = dr[col].ToString().Replace("?s", "'s");
                                dr[col] = dr[col].ToString().Replace("&lqt", "‘");
                                dr[col] = dr[col].ToString().Replace("?", "");
                                dr[col] = dr[col].ToString().Replace("&rqt", "’");
                            }
                        }
                        dtUpdated.ImportRow(dr);
                    }
                    dlSetDetails.DataSource = dtUpdated;
                    dlSetDetails.DataBind();
                }
                else
                {
                    dlSetDetails.DataSource = dtUpdated;
                    dlSetDetails.DataBind();
                }
            }
            lblVisibility(headerId);
            fillStepAndSet(headerId);
            ClearSetData();
            BtnUpdateSetDetails.Visible = false;
            btnAddSetDetails.Visible = true;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void GetStepData(int headerId)
    {
        objData = new clsData();
        string selQuerry = "";
        string objProcId="";
        object objTeach = "";
        try
        {
            string strQuerry = "SELECT ISNULL(TeachingProcId,-1) FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            if (headerId != 0 && headerId != -1)
            {
                objProcId = objData.FetchValue(strQuerry).ToString();
            }

            if (objProcId != "-1" || Convert.ToInt32(objProcId) > 0)
            {
                if (objProcId != "")
                {
                    strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + objProcId;
                    objTeach = objData.FetchValue(strQuerry);
                    if (objTeach != null)
                    {
                        if (objTeach.ToString() != "Match-to-Sample")
                        {
                            checkStepSortOrder(headerId);
                        }
                    }
                }
            } 

            selQuerry = "SELECT  DSTempParentStepId,SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,SortOrder,SetIds,SetNames FROM DSTempParentStep"
                        + " WHERE DSTempHdrId = " + headerId + " And ActiveInd = 'A' ORDER BY SortOrder";
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            DataTable dtUpdated = new DataTable();
            dtUpdated = dtList.Clone();
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtList.Rows)
                    {
                        foreach (DataColumn col in dtList.Columns)
                        {
                            if (col.ColumnName == "StepCd")
                            {

                                dr[col] = dr[col].ToString().Replace("&qt;", "'");
                                dr[col] = dr[col].ToString().Replace("?s", "'s");
                                dr[col] = dr[col].ToString().Replace("?", "");
                                dr[col] = dr[col].ToString().Replace("&lqt", "‘");
                                dr[col] = dr[col].ToString().Replace("&rqt", "’");
                            }
                            if (col.ColumnName == "StepName")
                            {
                                dr[col] = dr[col].ToString().Replace("&qt;", "'");
                                dr[col] = dr[col].ToString().Replace("?s", "'s");
                                dr[col] = dr[col].ToString().Replace("?", "");
                                dr[col] = dr[col].ToString().Replace("&lqt", "‘");
                                dr[col] = dr[col].ToString().Replace("&rqt", "’");
                            }
                            if (col.ColumnName == "SetNames")
                            {

                                dr[col] = dr[col].ToString().Replace("&qt;", "'");
                                dr[col] = dr[col].ToString().Replace("?s", "'s");
                                dr[col] = dr[col].ToString().Replace("?", "");
                                dr[col] = dr[col].ToString().Replace("&lqt", "‘");
                                dr[col] = dr[col].ToString().Replace("&rqt", "’");
                            }
                        }
                        dtUpdated.ImportRow(dr);
                    }
                    dlStepDetails.DataSource = dtList;
                    dlStepDetails.DataBind();
                }
                else
                {
                    dlStepDetails.DataSource = dtList;
                    dlStepDetails.DataBind();
                }

                int totStepCount = dtList.Rows.Count;

                DataTable dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("Value");

                for (int i = 1; i <= totStepCount; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["Id"] = i;
                    dr["Value"] = i;

                    dt.Rows.Add(dr);
                }

                ddlSortOrder.DataSource = dt;
                ddlSortOrder.DataValueField = "Id";
                ddlSortOrder.DataTextField = "Value";
                ddlSortOrder.DataBind();


            }


            //fill the indes ddl


            lblVisibility(headerId);
            fillStepAndSet(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void checkStepSortOrder(int headerId)
    {
        objData = new clsData();
        string selQuerry = "";
        string updateQry = "";
        int serial = 0;
        int sortOrder = 0;
        try
        {
            selQuerry = "SELECT DSTempParentStepId, ROW_NUMBER() Over (Order by DSTempHdrId, SortOrder) As [Sln],SortOrder FROM DSTempParentStep WHERE DSTempHdrId = "
                + headerId + " and activeInd = 'A' ORDER BY DSTempHdrId, SortOrder";
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList.Rows.Count > 0)
            {
                foreach (DataRow dr in dtList.Rows)
                {
                    serial = Convert.ToInt32(dr["Sln"].ToString());
                    sortOrder = Convert.ToInt32(dr["SortOrder"].ToString());
                    if (serial != sortOrder)
                    {
                        updateQry = "UPDATE DSTempParentStep SET SortOrder=" + serial + " WHERE DSTempParentStepId=" + Convert.ToInt32(dr["DSTempParentStepId"].ToString());
                        objData.Execute(updateQry);

                        //updateQry = "UPDATE DSTempStep SET SortOrder=" + serial + " WHERE DSTempParentStepId=" + Convert.ToInt32(dr["DSTempParentStepId"].ToString());
                        //objData.Execute(updateQry);
                    }
                }
                /// UPDATE SORTORDER OF DSTempStep
                /// 
                updateQry = "update DSTempStep  set SortOrder=(select SortOrder from DSTempParentStep where DSTempParentStep.DSTempParentStepId=DSTempStep.DSTempParentStepId) where ActiveInd='A' and DSTempHdrId=" + headerId;
                objData.Execute(updateQry);
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }

    }


    protected void GetSetCriteriaData(int headerId)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];

        try
        {

            if (sess != null)
            {
                string selQuerry = "Select DSR.DSTempRuleId,DSCol.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,Dscol.ColName,  " +
                                                  "  DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance,DSR.TotCorrInstance,DSR.ConsequetiveInd,DSR.IOAReqInd," +
                                                        "  DSR.MultiTeacherReqInd,DSR.IsNA,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd from DSTempRule DSR Inner Join " + //--- [New Criteria] May 2020 ---//
                                                          "   DSTempSetCol DSCol ON    DSR.DSTempSetColId=DSCol.DSTempSetColId  Where DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + " And " +
                                                          "  DSCol.DSTempHdrId=" + headerId + " And DSR.RuleType='SET' AND DSR.CriteriaType<>'MODIFICATION' ";

                DataTable dtList = objData.ReturnDataTable(selQuerry, false);


                selQuerry = "  Select DSR.DSTempRuleId,DSR.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,' ' as ColName,    DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance, " +
                             " DSR.TotCorrInstance,DSR.ConsequetiveInd,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DSR.IOAReqInd,  DSR.MultiTeacherReqInd from DSTempRule DSR  Where DSR.RuleType ='SET' And  DSR.CriteriaType='MODIFICATION' And  DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + "   And DSTempHdrId=" + headerId + " "; //--- [New Criteria] May 2020 ---//

                DataTable dtListMod = objData.ReturnDataTable(selQuerry, false);

                if (dtListMod != null)
                {
                    foreach (DataRow dr in dtListMod.Rows)
                    {
                        dtList.Rows.Add(dr.ItemArray);
                    }
                }

                if (dtList != null)
                {
                    if (dtList.Rows.Count > 0)
                    {

                        dlSetCriteria.DataSource = dtList;
                        dlSetCriteria.DataBind();
                    }
                    else
                    {
                        dlSetCriteria.DataSource = dtList;
                        dlSetCriteria.DataBind();
                    }
                }
            }
            lblVisibility(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void GetStepCriteriaData(int headerId)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        int setColId = 0;
        try
        {
            if (sess != null)
            {
                string selQuerry = "Select DSR.DSTempRuleId,DSCol.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,Dscol.ColName,  " +
                                                  "  DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance,DSR.TotCorrInstance,DSR.ConsequetiveInd,DSR.IOAReqInd," +
                                                        "  DSR.MultiTeacherReqInd,DSR.IsNA,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd from DSTempRule DSR Inner Join " + //--- [New Criteria] May 2020 ---//
                                                          "   DSTempSetCol DSCol ON    DSR.DSTempSetColId=DSCol.DSTempSetColId  Where DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + " And " +
                                                          "  DSCol.DSTempHdrId=" + headerId + " And DSR.RuleType='STEP' AND DSR.CriteriaType<>'MODIFICATION'";

                DataTable dtList = objData.ReturnDataTable(selQuerry, false);

                selQuerry = "  Select DSR.DSTempRuleId,DSR.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,' ' as ColName,    DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance, " +
                               " DSR.TotCorrInstance,DSR.ConsequetiveInd,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DSR.IOAReqInd,  DSR.MultiTeacherReqInd from DSTempRule DSR  Where DSR.RuleType ='STEP' And  DSR.CriteriaType='MODIFICATION' And  DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + "   And DSTempHdrId=" + headerId + " "; //--- [New Criteria] May 2020 ---//

                DataTable dtListMod = objData.ReturnDataTable(selQuerry, false);

                if (dtListMod != null)
                {
                    foreach (DataRow dr in dtListMod.Rows)
                    {
                        dtList.Rows.Add(dr.ItemArray);
                    }
                }

                if (dtList != null)
                {
                    if (dtList.Rows.Count > 0)
                    {

                        dlStepCriteria.DataSource = dtList;
                        dlStepCriteria.DataBind();
                    }
                    else
                    {
                        dlStepCriteria.DataSource = dtList;
                        dlStepCriteria.DataBind();
                    }
                }
            }

            lblVisibility(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void GetPromptCriteriaData(int headerId)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        int setColId = 0;

        try
        {
            if (sess != null)
            {
                string selQuerry = "Select DSR.DSTempRuleId,DSCol.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,Dscol.ColName,  " +
                                                  "  DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance,DSR.TotCorrInstance,DSR.ConsequetiveInd,DSR.IOAReqInd," +
                                                        "  DSR.MultiTeacherReqInd,DSR.IsNA,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd from DSTempRule DSR Inner Join " + //--- [New Criteria] May 2020 ---//
                                                          "   DSTempSetCol DSCol ON    DSR.DSTempSetColId=DSCol.DSTempSetColId  Where DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + " And " +
                                                          "  DSCol.DSTempHdrId=" + headerId + " And DSR.RuleType='PROMPT' AND DSR.CriteriaType<>'MODIFICATION' ";

                DataTable dtList = objData.ReturnDataTable(selQuerry, false);

                selQuerry = "  Select DSR.DSTempRuleId,DSR.DSTempHdrId,DSR.DSTempSetColId,DSR.DSTempSetColCalcId,DSR.RuleType,' ' as ColName,    DSR.CriteriaType,DSR.ScoreReq,DSR.TotalInstance, " +
                                " DSR.TotCorrInstance,DSR.ConsequetiveInd,ISNULL(DSR.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DSR.IOAReqInd,  DSR.MultiTeacherReqInd from DSTempRule DSR  Where DSR.RuleType ='PROMPT' And  DSR.CriteriaType='MODIFICATION' And  DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + "   And DSTempHdrId=" + headerId + " "; //--- [New Criteria] May 2020 ---//

                DataTable dtListMod = objData.ReturnDataTable(selQuerry, false);

                if (dtListMod != null)
                {
                    foreach (DataRow dr in dtListMod.Rows)
                    {
                        dtList.Rows.Add(dr.ItemArray);
                    }
                }
                if (dtList != null)
                {
                    if (dtList.Rows.Count > 0)
                    {

                        dlPromptCriteria.DataSource = dtList;
                        dlPromptCriteria.DataBind();
                    }
                    else
                    {
                        dlPromptCriteria.DataSource = dtList;
                        dlPromptCriteria.DataBind();
                    }
                }
            }
            lblVisibility(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void dlStepDetails_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        Label lblParentSet = (Label)e.Item.FindControl("lblParntSet");
        Label lblDesc = (Label)e.Item.FindControl("lblStepDesc");
        Button btnEdit = (Button)e.Item.FindControl("btnEditStep");
        Button btnDelt = (Button)e.Item.FindControl("btnRemoveStep");
        //ImageButton imgUp = (ImageButton)e.Item.FindControl("imgUp");
        //ImageButton imgDown = (ImageButton)e.Item.FindControl("imgDown");
        int headerId = 0;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (IsDiscrete(headerId) == true)
        {
            btnEdit.Visible = false;
            btnDelt.Visible = false;
            //imgUp.Visible = false;
            //imgDown.Visible = false;
        }
        string setNames = "";
        DataSet ds = new DataSet();
        string cmdstr = "SELECT DSTempSetId AS ID, SetCd As Name FROM DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
        DataTable dt = objData.ReturnDataTableDropDown(cmdstr, false);

        if (dt != null && dt.Rows.Count > 0)
        {

            foreach (DataRow item in dt.Rows)
            {

                setNames += item["Name"] + ",";


            }
        }
        if (lblParentSet.Text != "")
        {
            lblParentSet.Text = lblParentSet.Text.Substring(0, (lblParentSet.Text.Length - 1));
            string setnames = "SELECT STUFF((SELECT SetCd +',' FROM DSTempSet WHERE DSTempSetId IN (" + lblParentSet.Text + ") AND ActiveInd = 'A' FOR XML PATH('')),1,0,'')";
            lblParentSet.Text = Convert.ToString(objData.FetchValue(setnames));
        }

        if (lblParentSet.Text == "" && setNames == "")
        {
            lblParentSet.Text = "Not Assigned to any Sets";
        }
        if (lblParentSet.Text == setNames)
        {
            lblParentSet.Text = "All Sets";
        }
        if (lblParentSet.Text == "")
        {
            lblParentSet.Text = "Not Assigned to any Sets";
        }
        if (lblParentSet.Text != "")
        {
            if (lblParentSet.Text.ToString().Length > 40)
            {
                lblParentSet.Text = lblParentSet.Text.ToString().Substring(0, 40) + "........";
            }
        }

        if (lblDesc.Text.ToString() != "")
        {
            if (lblDesc.Text.ToString().Length > 30)
            {
                //lblDesc.Text = lblDesc.Text.ToString().Substring(0, 30) + "........";
            }
        }

    }

    protected void GetMeasureData(int headerId)
    {
        objData = new clsData();
        string selQuerry = "";
        try
        {
            selQuerry = "SELECT DSTempSetColId, ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,MisTrialDesc,CalcuType,CalcuData FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    dlMeasureData.DataSource = dtList;
                    dlMeasureData.DataBind();
                }
                else
                {
                    dlMeasureData.DataSource = dtList;
                    dlMeasureData.DataBind();
                }
            }

            lblVisibility(headerId);


            string IdQuery = "SELECT DSTempSetColId FROM DSTempSetCol WHERE ColName = 'Column3' AND ColTypeCd = 'Prompt' AND ActiveInd = 'A' AND DSTempHdrId = " + headerId;

            DataTable dt = objData.ReturnDataTable(IdQuery, false);

            if (dt.Rows.Count > 0)
            {
                BtnVTSavePrompt.Text = "Delete Prompt";
            }
            else
            {
                BtnVTSavePrompt.Text = "Add Prompt";
            }


        }
        catch (Exception Ex)
        {
            throw Ex;
        }


    }

    protected void dlLP_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        //BtnMaintenance.Visible = false;        
        objData = new clsData();
        LinkButton lnkLesson = (LinkButton)e.Item.FindControl("lnkLessonPlan");
        try
        {
            if (lnkLesson.Text.ToString() != null)
            {
                if (lnkLesson.Text.ToString().Length > 26)
                {
                    lnkLesson.Text = lnkLesson.Text.ToString().Substring(0, 26) + "......";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void dlCompltdLessonPlans_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkLesson = (LinkButton)e.Item.FindControl("lnkCompltdLessonPlan");
        try
        {
            if (lnkLesson.Text.ToString() != null)
            {
                if (lnkLesson.Text.ToString().Length > 26)
                {
                    lnkLesson.Text = lnkLesson.Text.ToString().Substring(0, 26) + "......";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void dlApprovedLessons_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkApprvdLess = (LinkButton)e.Item.FindControl("lnkApprovedLessons");
        try
        {
            if (lnkApprvdLess.Text.ToString() != null)
            {
                if (lnkApprvdLess.Text.ToString().Length > 26)
                {
                    lnkApprvdLess.Text = lnkApprvdLess.Text.ToString().Substring(0, 26) + ".....";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void dlRejectedLp_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkApprvdLess = (LinkButton)e.Item.FindControl("lnkRejectedLp");
        try
        {
            if (lnkApprvdLess.Text.ToString() != null)
            {
                if (lnkApprvdLess.Text.ToString().Length > 26)
                {
                    lnkApprvdLess.Text = lnkApprvdLess.Text.ToString().Substring(0, 26) + ".....";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void dlMaintenanceLp_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkApprvdLess = (LinkButton)e.Item.FindControl("lnkMaintenanceLp");
        try
        {
            if (lnkApprvdLess.Text.ToString() != null)
            {
                if (lnkApprvdLess.Text.ToString().Length > 26)
                {
                    lnkApprvdLess.Text = lnkApprvdLess.Text.ToString().Substring(0, 26) + ".....";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void dlInactiveLp_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkApprvdLess = (LinkButton)e.Item.FindControl("lnkInactiveLp");
        try
        {
            if (lnkApprvdLess.Text.ToString() != null)
            {
                if (lnkApprvdLess.Text.ToString().Length > 26)
                {
                    lnkApprvdLess.Text = lnkApprvdLess.Text.ToString().Substring(0, 26) + ".....";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void dlSetDetails_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        Label lblDesc = (Label)e.Item.FindControl("lblSetDesc");
        Button BtnEdit = (Button)e.Item.FindControl("btnEditSet");
        Button btnDelt = (Button)e.Item.FindControl("btnRemoveSet");
        ImageButton imgUp = (ImageButton)e.Item.FindControl("imgUp");
        ImageButton imgDown = (ImageButton)e.Item.FindControl("imgDown");
        int headerId = 0;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        try
        {

            if (lblDesc.Text.ToString() != "")
            {
                if (lblDesc.Text.ToString().Length > 100)
                {
                    lblDesc.Text = lblDesc.Text.ToString().Substring(0, 100) + "........";
                }
            }



        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void dlMeasureData_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        string selQuerry = "";
        string misDesc = "";
        bool IncldeMistrial = false;
        HiddenField hiddenFld = (HiddenField)e.Item.FindControl("hdnColId");
        Literal ltData = (Literal)e.Item.FindControl("ltMeasureCaegory");
        Button BtnEdit = (Button)e.Item.FindControl("btnEditMeasure");
        Button btnDelt = (Button)e.Item.FindControl("BtnRemove");
        Label MisTrial = (Label)e.Item.FindControl("lblMistrialDesc");
        string selectQuerry = "";
        int headerId = 0;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            selQuerry = "SELECT IncMisTrialInd, MisTrialDesc FROM DSTempSetCol WHERE DSTempSetColId = " + hiddenFld.Value;
            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    IncldeMistrial = Convert.ToBoolean(dtNew.Rows[0]["IncMisTrialInd"]);
                    misDesc = dtNew.Rows[0]["MisTrialDesc"].ToString();

                    if (IncldeMistrial == true)
                    {
                        MisTrial.Text = "Included," + misDesc.ToString() + "";
                    }
                    else
                    {
                        MisTrial.Text = "Not Included," + misDesc.ToString() + "";
                    }
                }
                else
                {
                    MisTrial.Text = "Not Included Mistrial";
                }
            }
            else
            {
                MisTrial.Text = "Not Included Mistrial";
            }
            selectQuerry = "SELECT CalcType FROM DSTempSetColCalc WHERE DSTempSetColId = " + hiddenFld.Value;
            DataTable dtList = objData.ReturnDataTable(selectQuerry, false);
            if (dtList != null)
            {

                if (dtList.Rows.Count > 0)
                {
                    ltData.Text = "<table>";
                    for (int i = 0; i < dtList.Rows.Count; i++)
                    {

                        ltData.Text += "<tr><td><p>" + dtList.Rows[i]["CalcType"].ToString() + "</p></td></tr>";


                    }
                    ltData.Text += "</table>";
                }
                else
                {
                    ltData.Text = "No Data";
                }
            }
            else
            {
                ltData.Text = "No Data";
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void dlStepCriteria_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        HiddenField hdnSetRuleId = (HiddenField)e.Item.FindControl("hdnStepCritVal");
        Label lblCriteriaDef = (Label)e.Item.FindControl("lblCriteriaDefStep");
        Label lblCritType = (Label)e.Item.FindControl("lblCriteriaTypeStep");
        Button btnEdit = (Button)e.Item.FindControl("BtnEditStepCriteria");
        Button btnDelt = (Button)e.Item.FindControl("BtnRemoveStepCriteria");
        int scoreReq = 0;
        int totalInstance = 0;
        int totalCorrInstance = 0;
        bool consecutveSess = false;
        bool consecutveAvg = false; //--- [New Criteria] May 2020 ---//
        bool isMultitchr = false;
        bool isIoReq = false;
        bool isNAChk = false;
        string colName = "";
        string measureType = "";
        string selQuerry = "";
        string cmpleteData = "";
        int headerId = 0;
        string criteriaType = "";

        string colType = "";
        string modCom = "";
        string modScore = "";
        int moveUpstat = 1;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (IsDiscrete(headerId) == true)
        {
            btnEdit.Visible = false;
            btnDelt.Visible = false;
        }
        try
        {

            selQuerry = "SELECT DsRule.DSTempSetColId,DsRule.DSTempSetColCalcId,DsRule.RuleType,DsRule.CriteriaType,DsRule.ScoreReq,DsRule.TotalInstance," +
                                "  DsRule.TotCorrInstance,DsRule.ModificationComment,DsRule.ModificationRule,DsRule.ConsequetiveInd,ISNULL(DsRule.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DsRule.MultiTeacherReqInd,DsRule.IOAReqInd,DsCol.ColName,DsColCalc.CalcType,DsCol.ColTypeCd,DsRule.IsNA,DsCol.Moveupstat FROM DSTempRule DsRule LEFT JOIN DSTempSetCol DsCol " + //--- [New Criteria] May 2020 ---//
                                    "  ON DsRule.DSTempSetColId = DsCol.DSTempSetColId LEFT JOIN DSTempSetColCalc DsColCalc ON DsRule.DSTempSetColCalcId = DsColCalc.DSTempSetColCalcId WHERE DSTempRuleId = " + hdnSetRuleId.Value;

            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {

                modCom = Convert.ToString(dtNew.Rows[0]["ModificationComment"]);
                if (modCom == "")
                {
                    modScore = Convert.ToString(dtNew.Rows[0]["ModificationRule"]);
                }


                if (dtNew.Rows.Count > 0)
                {
                    lblCritType.Text = dtNew.Rows[0]["CriteriaType"].ToString();
                    scoreReq = Convert.ToInt32(dtNew.Rows[0]["ScoreReq"]);
                    totalInstance = Convert.ToInt32(dtNew.Rows[0]["TotalInstance"]);
                    totalCorrInstance = Convert.ToInt32(dtNew.Rows[0]["TotCorrInstance"]);
                    consecutveSess = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveInd"]);
                    consecutveAvg = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveAvgInd"]); //--- [New Criteria] May 2020 ---//
                    isMultitchr = Convert.ToBoolean(dtNew.Rows[0]["MultiTeacherReqInd"]);
                    isIoReq = Convert.ToBoolean(dtNew.Rows[0]["IOAReqInd"]);
                    colName = dtNew.Rows[0]["ColName"].ToString();
                    measureType = dtNew.Rows[0]["CalcType"].ToString();
                    criteriaType = lblCritType.Text;
                    colType = dtNew.Rows[0]["ColTypeCd"].ToString();
                    isNAChk = Convert.ToBoolean(dtNew.Rows[0]["IsNA"]);
                    if (dtNew.Rows[0]["MoveUpstat"].ToString() != "")
                        moveUpstat = Convert.ToInt16(dtNew.Rows[0]["MoveUpstat"]);

                    if (criteriaType != "MODIFICATION")
                    {
                        string measure1 = measureType.Substring(0, 1);
                        string measure2 = measureType.Substring(1, measureType.Length - 1);
                        if (measure1 == "%")
                        {
                            measureType = "% " + measure2;
                        }
                    }
                    if (criteriaType == "MOVE UP")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)//Moveupstat check added in every section below for list 5 Task #9 11-02-2021(change in Set moveup and move down in frequency and duration)
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Step.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Step.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + " Consecutive Average" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Step.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//
                            
                        }
                        else
                        {
                            cmpleteData = "NA";
                        }

                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MOVE DOWN")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Step.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Step.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                        cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Step.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//

                        }
                        else
                        {
                            cmpleteData = "NA";
                        }

                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MODIFICATION")
                    {
                        if (consecutveSess == false)
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }
                        else
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }

                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }

                    try
                    {
                        selQuerry = "UPDATE DSTempRule SET CriteriaDetails = '" + clsGeneral.convertQuotes(cmpleteData.ToString()) + "' WHERE DSTempRuleId = " + hdnSetRuleId.Value;
                        objData.Execute(selQuerry);
                    }
                    catch
                    {
                    }
                    cmpleteData = "";
                    if (criteriaType == "MODIFICATION") setModificationVisibility(false);
                }
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void dlSetCriteria_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        HiddenField hdnSetRuleId = (HiddenField)e.Item.FindControl("hdnSetCritVal");
        Label lblCriteriaDef = (Label)e.Item.FindControl("lblCriteriaDef");
        Label lblCritType = (Label)e.Item.FindControl("lblCriteriaType");
        Button BtnEdit = (Button)e.Item.FindControl("btnEditMeasure");
        Button btnDelt = (Button)e.Item.FindControl("BtnRemove");
        int headerId = 0;
        int scoreReq = 0;
        int totalInstance = 0;
        int totalCorrInstance = 0;
        bool consecutveSess = false;
        bool consecutveAvg = false; //--- [New Criteria] May 2020 ---//
        bool isMultitchr = false;
        bool isIoReq = false;
        bool isNAChk = false;
        string colName = "";
        string measureType = "";
        string selQuerry = "";
        string cmpleteData = "";
        string criteriaType = "";
        string colType = "";
        string modCom = "";
        string modScore = "";
        int moveUpStat = 1;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            selQuerry = "SELECT DsRule.DSTempSetColId,DsRule.DSTempSetColCalcId,DsRule.RuleType,DsRule.CriteriaType,DsRule.ScoreReq,DsRule.TotalInstance," +
                                "  DsRule.TotCorrInstance,DsRule.ModificationComment,DsRule.ModificationRule,DsRule.ConsequetiveInd,ISNULL(DsRule.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DsRule.MultiTeacherReqInd,DsRule.IOAReqInd,DsCol.ColName,DsColCalc.CalcType,DsCol.ColTypeCd,DsRule.IsNA,DsCol.MoveUpstat FROM DSTempRule DsRule LEFT JOIN DSTempSetCol DsCol " + //--- [New Criteria] May 2020 ---//
                                    "  ON DsRule.DSTempSetColId = DsCol.DSTempSetColId LEFT JOIN DSTempSetColCalc DsColCalc ON DsRule.DSTempSetColCalcId = DsColCalc.DSTempSetColCalcId WHERE DSTempRuleId = " + hdnSetRuleId.Value;

            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    modCom = Convert.ToString(dtNew.Rows[0]["ModificationComment"]);
                    if (modCom == "")
                    {
                        modScore = Convert.ToString(dtNew.Rows[0]["ModificationRule"]);
                    }


                    lblCritType.Text = dtNew.Rows[0]["CriteriaType"].ToString();
                    scoreReq = Convert.ToInt32(dtNew.Rows[0]["ScoreReq"]);
                    totalInstance = Convert.ToInt32(dtNew.Rows[0]["TotalInstance"]);
                    totalCorrInstance = Convert.ToInt32(dtNew.Rows[0]["TotCorrInstance"]);
                    consecutveSess = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveInd"]);
                    consecutveAvg = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveAvgInd"]); //--- [New Criteria] May 2020 ---//
                    isMultitchr = Convert.ToBoolean(dtNew.Rows[0]["MultiTeacherReqInd"]);
                    isIoReq = Convert.ToBoolean(dtNew.Rows[0]["IOAReqInd"]);
                    colName = dtNew.Rows[0]["ColName"].ToString();
                    measureType = dtNew.Rows[0]["CalcType"].ToString();
                    criteriaType = lblCritType.Text;
                    colType = dtNew.Rows[0]["ColTypeCd"].ToString();
                    isNAChk = Convert.ToBoolean(dtNew.Rows[0]["IsNA"]);
                    if (dtNew.Rows[0]["MoveUpstat"].ToString() != "")
                        moveUpStat = Convert.ToInt16(dtNew.Rows[0]["MoveUpstat"]);

                    if (criteriaType != "MODIFICATION")
                    {
                        string measure1 = measureType.Substring(0, 1);
                        string measure2 = measureType.Substring(1, measureType.Length - 1);
                        if (measure1 == "%")
                        {
                            measureType = "% " + measure2;
                        }
                    }
                    if (criteriaType == "MOVE UP")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)//Moveupstat comparison for List 5 task #9 11-02-2021
                                       cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Set.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)
                                       cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";

                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Set.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)
                                       cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";

                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Set.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//
                        }
                        else
                        {
                            cmpleteData = "NA";
                        }

                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MOVE DOWN")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Set.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Set.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpStat==1)
                                        cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Set.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//
                        }
                        else
                        {
                            cmpleteData = "NA";
                        }
                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MODIFICATION")
                    {
                        if (consecutveSess == false)
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }
                        else
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }

                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }


                    try
                    {
                        selQuerry = "UPDATE DSTempRule SET CriteriaDetails = '" + clsGeneral.convertQuotes(cmpleteData.ToString()) + "' WHERE DSTempRuleId = " + hdnSetRuleId.Value;
                        objData.Execute(selQuerry);
                    }
                    catch
                    {
                    }

                    cmpleteData = "";
                    if (criteriaType == "MODIFICATION") setModificationVisibility(false);
                }
            }


        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void dlPromptCriteria_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        HiddenField hdnSetRuleId = (HiddenField)e.Item.FindControl("hdnPromptCritVal");
        Label lblCriteriaDef = (Label)e.Item.FindControl("lblCriteriaDefPrompt");
        Label lblCritType = (Label)e.Item.FindControl("lblCriteriaTypePrompt");
        int scoreReq = 0;
        int totalInstance = 0;
        int totalCorrInstance = 0;
        bool consecutveSess = false;
        bool consecutveAvg = false; //--- [New Criteria] May 2020 ---//
        bool isMultitchr = false;
        bool isIoReq = false;
        bool isNAChk = false;
        string colName = "";
        string measureType = "";
        string selQuerry = "";
        string cmpleteData = "";
        Button BtnEdit = (Button)e.Item.FindControl("btnEditMeasure");
        Button btnDelt = (Button)e.Item.FindControl("BtnRemove");
        int headerId = 0;
        string criteriaType = "";

        string colType = "";

        string modCom = "";
        string modScore = "";
        int moveUpstat = 1;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        try
        {
            selQuerry = "SELECT DsRule.DSTempSetColId,DsRule.DSTempSetColCalcId,DsRule.RuleType,DsRule.CriteriaType,DsRule.ScoreReq,DsRule.TotalInstance," +
                                "  DsRule.TotCorrInstance,DsRule.ModificationComment,DsRule.ModificationRule,DsRule.ConsequetiveInd,ISNULL(DsRule.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DsRule.MultiTeacherReqInd,DsRule.IOAReqInd,DsCol.ColName,DsColCalc.CalcType,DsCol.ColTypeCd,DsRule.IsNA,DsCol.MoveUpstat FROM DSTempRule DsRule LEFT JOIN DSTempSetCol DsCol " + //--- [New Criteria] May 2020 ---//
                                    "  ON DsRule.DSTempSetColId = DsCol.DSTempSetColId LEFT JOIN DSTempSetColCalc DsColCalc ON DsRule.DSTempSetColCalcId = DsColCalc.DSTempSetColCalcId WHERE DSTempRuleId = " + hdnSetRuleId.Value;

            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {

                if (dtNew.Rows.Count > 0)
                {
                    modCom = Convert.ToString(dtNew.Rows[0]["ModificationComment"]);
                    if (modCom == "")
                    {
                        modScore = Convert.ToString(dtNew.Rows[0]["ModificationRule"]);
                    }

                    lblCritType.Text = dtNew.Rows[0]["CriteriaType"].ToString();
                    scoreReq = Convert.ToInt32(dtNew.Rows[0]["ScoreReq"]);
                    totalInstance = Convert.ToInt32(dtNew.Rows[0]["TotalInstance"]);
                    totalCorrInstance = Convert.ToInt32(dtNew.Rows[0]["TotCorrInstance"]);
                    consecutveSess = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveInd"]);
                    consecutveAvg = Convert.ToBoolean(dtNew.Rows[0]["ConsequetiveAvgInd"]);
                    isMultitchr = Convert.ToBoolean(dtNew.Rows[0]["MultiTeacherReqInd"]);
                    isIoReq = Convert.ToBoolean(dtNew.Rows[0]["IOAReqInd"]);
                    colName = dtNew.Rows[0]["ColName"].ToString();
                    measureType = dtNew.Rows[0]["CalcType"].ToString();
                    criteriaType = lblCritType.Text;
                    colType = dtNew.Rows[0]["ColTypeCd"].ToString();
                    isNAChk = Convert.ToBoolean(dtNew.Rows[0]["IsNA"]);
                    if (dtNew.Rows[0]["MoveUpstat"].ToString() != "")
                        moveUpstat = Convert.ToInt16(dtNew.Rows[0]["MoveUpstat"]);

                    if (criteriaType != "MODIFICATION")
                    {
                        string measure1 = measureType.Substring(0, 1);
                        string measure2 = measureType.Substring(1, measureType.Length - 1);
                        if (measure1 == "%")
                        {
                            measureType = "% " + measure2;
                        }
                    }
                    if (criteriaType == "MOVE UP")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)//Moveupstat comparison added for List 5 Task #9
                                       cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Prompt.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Prompt.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                        cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + "  for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Atleast " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Prompt.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//
                        }
                        else
                        {
                            cmpleteData = "NA";
                        }
                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MOVE DOWN")
                    {
                        if (!isNAChk)
                        {
                            if (consecutveSess == false)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " in " + totalCorrInstance + " out of " + totalInstance + " Sessions";
                                }
                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ".IOA should be completed before advancing a Prompt.";
                                }
                            }
                            else
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "More than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + "" + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Prompt.";
                                }
                            }

                            //--- [New Criteria] May 2020 - (Start) ---//
                            if (consecutveAvg == true)
                            {
                                if (colType == "Duration" || colType == "Frequency")
                                {
                                    if(moveUpstat==1)
                                       cmpleteData = "More than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                    else
                                        cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }
                                else
                                {
                                    cmpleteData = "Less than " + scoreReq + " Consecutive Average " + measureType + " for " + colName + " for " + totalInstance + " Consecutive Sessions";
                                }

                                if (isMultitchr == true)
                                {
                                    cmpleteData += " With atleast 2 different staff";
                                }
                                if (isIoReq == true)
                                {
                                    cmpleteData += ". IOA should be completed before advancing a Prompt.";
                                }
                            }
                            //--- [New Criteria] May 2020 - (End) ---//
                        }
                        else
                        {
                            cmpleteData = "NA";
                        }
                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    else if (criteriaType == "MODIFICATION")
                    {
                        if (consecutveSess == false)
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }
                        else
                        {
                            if (modCom == "")
                            {
                                cmpleteData = "Atleast " + modScore + " MOVE DOWNS Required ";
                            }
                            else
                            {
                                cmpleteData = modCom;
                            }
                        }


                        lblCriteriaDef.Text = cmpleteData.ToString();
                    }
                    try
                    {
                        selQuerry = "UPDATE DSTempRule SET CriteriaDetails = '" + clsGeneral.convertQuotes(cmpleteData.ToString()) + "' WHERE DSTempRuleId = " + hdnSetRuleId.Value;
                        objData.Execute(selQuerry);
                    }
                    catch
                    {
                    }
                    cmpleteData = "";
                    if (criteriaType == "MODIFICATION") setModificationVisibility(false);
                }
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void FindType(int headerId)
    {
        objData = new clsData();
        object objVal = null;
        int isValue = 0;
        string selctQuerry = "SELECT IsVisualTool FROM DSTempHdr WHERE DSTempHdrId = " + headerId + "";
        objVal = objData.FetchValue(selctQuerry);
        if (objVal != null)
        {
            isValue = Convert.ToInt32(objVal);
        }
        if (isValue != 0)
        {
            lblLessonType.Text = "VisualTool";
            btnconvrtLessonPlan.Text = "Convert To NonVisualTool";

        }
        else
        {
            lblLessonType.Text = "Non-VisualTool";
            btnconvrtLessonPlan.Text = "Convert To VisualTool";
            //  imgApproveStatus.Visible = false;
        }


    }



    protected void EnableEdit(int tempId)
    {
        objData = new clsData();
        string selctQuerry;
        string selctIsVT;
        object checkVT;
        int isVt = 0;
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + tempId;
        checkVT = objData.FetchValue(selctQuerry);
        if (tempId == 0)
        {
            btneditMode.Visible = false;
            btnreasignNewLsn.Visible = false;
        }
        else
        {
            if (checkVT.ToString() != "")
            {
                isVt = Convert.ToInt32(checkVT);
                selctIsVT = "SELECT IsST_Edit FROM LE_Lesson WHERE LessonId = " + isVt;
                int studentAssgn = Convert.ToInt32(objData.FetchValue(selctIsVT));

                if ((isVt > 0) && (studentAssgn == 1))
                {
                    btneditMode.Visible = true;
                    btnreasignNewLsn.Visible = true;

                    // lnkEditVT.Visible = true;
                    //  imgReasign.Visible = true;
                }
                else if (isVt > 0)
                {
                    btnreasignNewLsn.Visible = true;
                }
                else
                {
                    btneditMode.Visible = false;
                    btnreasignNewLsn.Visible = false;
                    //  lnkEditVT.Visible = false;
                    //  imgReasign.Visible = false;
                }
            }
            else
            {
                btneditMode.Visible = false;
                btnreasignNewLsn.Visible = false;
                // lnkEditVT.Visible = false;
                // imgReasign.Visible = false;
            }
        }

    }



    protected void LoadTemplateData(int headerId)
    {
        lblloadAlert.Visible = false;

        MainDiv.Visible = true;                      // Right Main Div Visiblity option set true.
        IsDiscrete(headerId);                       // Check the template is discrete
        lblDataFill(headerId);                      // Fill Current LessonName
        LessonInfo(headerId);                       // Fill the lessonPlanDetails.
        GetMeasureData(headerId);                   // Fill the measure Data 
        FillTypeOfInstruction(headerId);           // Fill Type of Instruction Data
        GetSetData(headerId);                      // Fill the Sets Data
        GetStepData(headerId);                    // Fill the Steps Data
        GetSetCriteriaData(headerId);             // Fill the Set Criteria Data
        GetStepCriteriaData(headerId);           // Fill the Step Criteria Data
        GetPromptCriteriaData(headerId);        // Fill the Prompt Criteria Data
        GetPromptProcedureList(headerId);      // Fill the prompt Procedure...
        FillDropPrompt(headerId);             // Fill dropdownlist prompt procedure...
        GetLessonProcData(headerId);           // Fill LessonProcedureData
        fillStepAndSet(headerId);

        FindType(headerId);                       //Check Is Visual Lesson or not
        EnableEdit(headerId);

        GetStatus(headerId);                    // Button Permissions
        //  SetLessonProcedure(headerId);                       // Fill Lesson Procedure Data
    }



    protected void lnkLessonPlan_Click(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
        lessonEDate.Enabled = true;
        lessonSDate.Enabled = true;
        lessonSDate.Text = "";
        lessonEDate.Text = "";        Hdfsavemeasure.Value = "";
        textBoxDisableEnable(true);
        btnrejectedNotes.Visible = false;
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        btndoc.Style.Add("display", "Block");
        BtnSubmit.Visible = true;
        BtnPreview.Visible = true;
        BtnMaintenance.Visible = false;
        BtnInactive.Visible = false;
        BtnActive.Visible = false;
        btnDelLp.Visible = true;
        btndoc.Visible = true;
        BtnUpdateLessonPlan.Visible = true;
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        //lessonSDate.Enabled = true;
        //lessonEDate.Enabled = true;
        // bool visibility = false;
        LinkButton lbLesson = (LinkButton)sender;
        try
        {
            int headerId = Convert.ToInt32(lbLesson.CommandArgument);
            ViewState["HeaderId"] = headerId;
            fillParentSetData();
            LoadTemplateData(headerId);


            setWritePermissions(false);
            FillDocSmall(headerId);
            VisibleApprovalNotesPR();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
        ClearSetData();
        BtnUpdateSetDetails.Visible = false;
        btnAddSetDetails.Visible = true;

        setOptions(true);
    }



    protected void lnkCompltdLessonPlan_Click(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
        lessonSDate.Text = "";
        lessonEDate.Text = "";        Hdfsavemeasure.Value = "1";
        textBoxDisableEnable(false);
        btnrejectedNotes.Visible = false;
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        btndoc.Style.Add("display", "None");
        BtnApproval.Visible = true;
        BtnInactive.Visible = false;
        BtnActive.Visible = false;
        BtnReject.Visible = true;
        btnDelLp.Visible = true;
        BtnPreview.Visible = true;
        lessonSDate.Enabled = false;
        lessonEDate.Enabled = false;
        BtnUpdateLessonPlan.Visible = false;
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        int headerId = 0;
        LinkButton lbLesson = (LinkButton)sender;
        try
        {
            headerId = Convert.ToInt32(lbLesson.CommandArgument);

            ViewState["HeaderId"] = headerId;
            oTemp.TemplateId = headerId;

            LoadTemplateData(headerId);

            setWritePermissions(false);
            VisibleApprovalNotes(true);

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        setApprovePermission();
        drpTasklist_SelectedIndexChanged1(sender, e);

        int prevTempId = getPrevVersion();
        BtnApproval_hdn.Visible = false;
        BtnApproval.Visible = true;
        loadSetsOverride(prevTempId, headerId);         
    }
    

    protected void lnkApprovedLessons_Click(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
        lessonSDate.Text = "";
        lessonEDate.Text = "";
        objData = new clsData();
        Hdfsavemeasure.Value = "1";
        textBoxDisableEnable(false);
        btnrejectedNotes.Visible = false;
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        BtnInactive.Visible = true;
        BtnMaintenance.Visible = true;
        BtnPreview.Visible = true;
        BtnActive.Visible = false;
        btndoc.Style.Add("display", "None");
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        btnDelLp.Visible = true;
        BtnUpdateLessonPlan.Visible = false;
        lessonSDate.Enabled = false;
        lessonEDate.Enabled = false;
        // bool visibility = false;
        LinkButton lnkApprvdless = (LinkButton)sender;
        try
        {
            int headerId = Convert.ToInt32(lnkApprvdless.CommandArgument);
            hdDefaultName.Value = Convert.ToString(objData.FetchValue("SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId='" + headerId + "'"));

            Session["GoalID_Approved"] = objData.FetchValue("SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + headerId + "')");
            ViewState["HeaderId"] = headerId;

            LoadTemplateData(headerId);

            CheckAsigned(headerId);

            setWritePermissions(true);
            VisibleApprovalNotes(false);
            VisibleApprovalNote();

            fn_setChkTotalTask(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
        
    }

    private void textBoxDisableEnable(bool status)
    {
        txtmajorset.Enabled = status;
        txtminorset.Enabled = status;
        txtBaseline.Enabled = status;
        txtobjective.Enabled = status;
        txtGenProce.Enabled = status;
        txtSpecStandrd.Enabled = status;
        txtPreSkills.Enabled = status;
        txtLessonPlanGoal.Enabled = status;
        txtFramework.Enabled = status;
        txtMaterials.Enabled = status;
        txtTeacherDo.Enabled = status;
        txtStudentDo.Enabled = status;
        txtConsequenceDO.Enabled = status;
        txtSDInstruction.Enabled = status;
        txtResponseOutcome.Enabled = status;
        txtReinforcementProc.Enabled = status;
        txtIncorrectResponse.Enabled = status;
        txtCorrectionProcedure.Enabled = status;
        txtMistrial.Enabled = status;
        txtMistrialProcedure.Enabled = status;
        txtSpecEntrypoint.Enabled = status;
        drpTeachingProc.Enabled = status;
        drp_teachingFormat.Enabled = status;
        chkDiscrete.Enabled = status;
        drpTasklist.Enabled = status;
        
    }

    protected void lnkRejectedLp_Click(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
       lessonSDate.Text = "";
       lessonEDate.Text = "";
       lessonSDate.Enabled = false;
       lessonEDate.Enabled = false; 
       Hdfsavemeasure.Value = "1";
        textBoxDisableEnable(false);
        btnrejectedNotes.Visible = true;
        btndoc.Style.Add("display", "None");
        btnDelLp.Visible = true;
        BtnSubmit.Visible = true;
        BtnActive.Visible = false;
        BtnInactive.Visible = false;
        BtnUpdateLessonPlan.Visible = false;
        BtnPreview.Visible = false;
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        // bool visibility = false;
        LinkButton lnkApprvdless = (LinkButton)sender;
        try
        {
            int headerId = Convert.ToInt32(lnkApprvdless.CommandArgument);
            ViewState["HeaderId"] = headerId;

            LoadTemplateData(headerId);

            // CheckAsigned(headerId);

            setWritePermissions(true);

            btnFromReject.Visible = GetRejectedLPsInInProgressStatus(headerId);
            if (btnFromReject.Visible == false) btnFromRejectDup.Visible = true;
            else btnFromRejectDup.Visible = false;
            VisibleApprovalNotes(false);
            VisibleApprovalNote();


        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    private bool GetRejectedLPsInInProgressStatus(int headerId)
    {
        objData = new clsData();
        bool Status = false;
        string Query = "SELECT COUNT(*) FROM DSTempHdr WHERE LessonPlanId=(SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + headerId + "') AND StudentId=(SELECT StudentId FROM DSTempHdr WHERE DSTempHdrId='" + headerId + "') AND StatusId=(SELECT [LookupId] FROM [dbo].[LookUp] WHERE [LookupType]='TemplateStatus' AND [LookupName]='In Progress')";
        object CountStatus = objData.FetchValue(Query);
        if (Convert.ToInt32(CountStatus) == 0) Status = true;
        return Status;
    }

    protected void lnkMaintenanceLp_Click(object sender, EventArgs e)
    {

    }

    protected void LessonInfo(int templateId)
    {
        objData = new clsData();
        string strQuery = "";
        string strQuery2 = "";
        object objId = null;
        int lessonPlanId = 0;
        strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        objId = objData.FetchValue(strQuery);
        if (objId != null)
        {
            lessonPlanId = Convert.ToInt32(objId);
        }
        try
        {
            strQuery = "SELECT DSTemplateName,FrameandStrand,LessonPlanGoal,SpecStandard,NoofTimesTried,NoofTimesTriedPer,SpecEntryPoint,PreReq,Materials,deletessn,(Select CONVERT(VARCHAR, DsTempHdr.LessonSDate, 101)) AS LessonSDate,(Select CONVERT(VARCHAR, DsTempHdr.LessonEDate, 101)) AS LessonEDate FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
            DataTable dtNew = objData.ReturnDataTable(strQuery, false);
            strQuery = "Select EffStartDate from StDtLessonPlan inner join StDtIEP on StDtLessonPlan.StDtIEPId=StDtIEP.StDtIEPId where StDtLessonPlan.LessonPlanId = " + lessonPlanId + " and StDtLessonPlan.StudentId = " + sess.StudentId + " order by StDtIEP.StdtIEPId Desc ";
            //DataTable dtNew1 = objData.ReturnDataTable(strQuery, false);
            //strQuery = "Select EffStartDate from StDtLessonPlan inner join StDtIEP_PE on StDtLessonPlan.StDtIEPId=StDtIEP_PE.StDtIEP_PEId where StDtLessonPlan.LessonPlanId = " + lessonPlanId + " and StDtLessonPlan.StudentId = " + sess.StudentId + " order by StDtIEP_PE.StdtIEP_PEId Desc ";
            //DataTable dtNew2 = objData.ReturnDataTable(strQuery, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    txtLessonName.Text = dtNew.Rows[0]["DSTemplateName"].ToString();
                    txtNoofTimesTried.Text = dtNew.Rows[0]["NoofTimesTried"].ToString();
                    string NoofTimesTriedPer = dtNew.Rows[0]["NoofTimesTriedPer"].ToString();
                    if(NoofTimesTriedPer == "")
                        noofTimesTriedPer.SelectedIndex = 0;
                    else
                        noofTimesTriedPer.SelectedValue = NoofTimesTriedPer;
                    txtLessonPlanGoal.Text = dtNew.Rows[0]["LessonPlanGoal"].ToString();
                    txtFramework.Text = dtNew.Rows[0]["FrameandStrand"].ToString();
                    txtSpecStandrd.Text = dtNew.Rows[0]["SpecStandard"].ToString();
                    txtSpecEntrypoint.Text = dtNew.Rows[0]["SpecEntryPoint"].ToString();
                    txtPreSkills.Text = dtNew.Rows[0]["PreReq"].ToString();
                    txtMaterials.Text = dtNew.Rows[0]["Materials"].ToString();
                    string del = dtNew.Rows[0]["deletessn"].ToString();

                        lessonSDate.Text = dtNew.Rows[0]["LessonSDate"].ToString();
                        lessonEDate.Text = dtNew.Rows[0]["LessonEDate"].ToString();


                    if (del == "true")
                    {
                        cbx_deletesess.Checked = Convert.ToBoolean("true");
                    }
                    else
                    {
                        cbx_deletesess.Checked = Convert.ToBoolean("false");
                    }

                }
                else
                {
                    txtLessonName.Text = "";
                    txtNoofTimesTried.Text = "";
                    noofTimesTriedPer.SelectedIndex = 0;
                    txtFramework.Text = "";
                    txtSpecStandrd.Text = "";
                    txtSpecEntrypoint.Text = "";
                    txtPreSkills.Text = "";
                    txtMaterials.Text = "";
                    txtLessonPlanGoal.Text = "";
                }
            }

            strQuery = "SELECT ApprNoteLessonInfo,ApprNoteTypeInstruction,ApprNoteMeasurement,ApprNoteSet,ApprNoteStep,ApprNotePrompt,ApprNoteLessonProc FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
            DataTable dtApprove = objData.ReturnDataTable(strQuery, false);

            if (dtApprove != null)
            {
                if (dtApprove.Rows.Count > 0)
                {
                    txtCommentLessonInfo.Text = dtApprove.Rows[0]["ApprNoteLessonInfo"].ToString();
                    txtCommentTypeofInstr.Text = dtApprove.Rows[0]["ApprNoteTypeInstruction"].ToString();
                    txtMeasurementSystems.Text = dtApprove.Rows[0]["ApprNoteMeasurement"].ToString();
                    txtcommentset.Text = dtApprove.Rows[0]["ApprNoteSet"].ToString();
                    txtcommentStep.Text = dtApprove.Rows[0]["ApprNoteStep"].ToString();
                    txtcommentPrompt.Text = dtApprove.Rows[0]["ApprNotePrompt"].ToString();
                    txtcommentLessonProcedure.Text = dtApprove.Rows[0]["ApprNoteLessonProc"].ToString();
                }
                else
                {
                    txtCommentLessonInfo.Text = "";
                    txtCommentTypeofInstr.Text = "";
                    txtMeasurementSystems.Text = "";
                    txtcommentset.Text = "";
                    txtcommentStep.Text = "";
                    txtcommentPrompt.Text = "";
                    txtcommentLessonProcedure.Text = "";
                }
            }

            string goalNames = "";
            strQuery2 = "select Goal.GoalCode from Goal inner join GoalLPRel on Goal.GoalId=GoalLPRel.GoalId inner join LessonPlan on LessonPlan.LessonPlanId=GoalLPRel.LessonPlanId where LessonPlan.LessonPlanId=" + lessonPlanId;

            DataTable Dt2 = objData.ReturnDataTable(strQuery2, false);
            if (Dt2 != null)
            {
                if (Dt2.Rows.Count > 0)
                {
                    for (int i = 0; i < Dt2.Rows.Count; i++)
                    {
                        goalNames += Dt2.Rows[i]["GoalCode"].ToString() + ",";
                    }
                    lblGoalName.Text = goalNames.TrimEnd(',');
                }
                else
                {
                    lblGoalName.Text = "";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void LessonInfoAdmin(int templateId)
    {
        objData = new clsData();
        string strQuery = "";
        string strQuery2 = "";
        object objId = null;
        int lessonPlanId = 0;
        strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        objId = objData.FetchValue(strQuery);
        if (objId != null)
        {
            lessonPlanId = Convert.ToInt32(objId);
        }
        try
        {
            strQuery = "SELECT LessonPlanName,FrameandStrand,SpecStandard,SpecEntryPoint,PreReq,Materials,(Select CONVERT(VARCHAR, LessonSDate, 101)) AS LessonSDate,(Select CONVERT(VARCHAR, LessonEDate, 101)) AS LessonEDate FROM LessonPlan WHERE LessonPlanId = " + lessonPlanId;
            string strQy = "SELECT NoofTimesTried,NoofTimesTriedPer FROM DSTempHdr WHERE LessonPlanId = " + lessonPlanId + " AND StudentId IS NULL";
            DataTable dtNew = objData.ReturnDataTable(strQuery, false);
            DataTable NoofTimesTried = objData.ReturnDataTable(strQy, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    txtLessonName.Text = dtNew.Rows[0]["LessonPlanName"].ToString();
                    txtNoofTimesTried.Text = dtNew.Rows[0]["NoofTimesTried"].ToString();
                    noofTimesTriedPer.SelectedValue = dtNew.Rows[0]["NoofTimesTriedPer"].ToString();
                    txtFramework.Text = dtNew.Rows[0]["FrameandStrand"].ToString();
                    txtSpecStandrd.Text = dtNew.Rows[0]["SpecStandard"].ToString();
                    txtSpecEntrypoint.Text = dtNew.Rows[0]["SpecEntryPoint"].ToString();
                    txtPreSkills.Text = dtNew.Rows[0]["PreReq"].ToString();
                    txtMaterials.Text = dtNew.Rows[0]["Materials"].ToString();
                    lessonSDate.Text = dtNew.Rows[0]["LessonSDate"].ToString();
                    lessonEDate.Text = dtNew.Rows[0]["LessonEDate"].ToString();
                }
                else
                {
                    txtLessonName.Text = "";
                    txtNoofTimesTried.Text = "";
                    noofTimesTriedPer.SelectedIndex = 0;
                    txtFramework.Text = "";
                    txtSpecStandrd.Text = "";
                    txtSpecEntrypoint.Text = "";
                    txtPreSkills.Text = "";
                    txtMaterials.Text = "";
                    lessonSDate.Text = "";
                    lessonEDate.Text = "";

                }
            }
            strQuery = "SELECT LessonPlanGoal FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
            object lessonGoal = objData.FetchValue(strQuery);

            if (lessonGoal != null)
            {
                txtLessonPlanGoal.Text = lessonGoal.ToString();
            }
            else
                txtLessonPlanGoal.Text = "";

            strQuery = "SELECT ApprNoteLessonInfo,ApprNoteTypeInstruction,ApprNoteMeasurement,ApprNoteSet,ApprNoteStep,ApprNotePrompt,ApprNoteLessonProc FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
            DataTable dtApprove = objData.ReturnDataTable(strQuery, false);

            if (dtApprove != null)
            {
                if (dtApprove.Rows.Count > 0)
                {
                    txtCommentLessonInfo.Text = dtApprove.Rows[0]["ApprNoteLessonInfo"].ToString();
                    txtCommentTypeofInstr.Text = dtApprove.Rows[0]["ApprNoteTypeInstruction"].ToString();
                    txtMeasurementSystems.Text = dtApprove.Rows[0]["ApprNoteMeasurement"].ToString();
                    txtcommentset.Text = dtApprove.Rows[0]["ApprNoteSet"].ToString();
                    txtcommentStep.Text = dtApprove.Rows[0]["ApprNoteStep"].ToString();
                    txtcommentPrompt.Text = dtApprove.Rows[0]["ApprNotePrompt"].ToString();
                    txtcommentLessonProcedure.Text = dtApprove.Rows[0]["ApprNoteLessonProc"].ToString();
                }
                else
                {
                    txtCommentLessonInfo.Text = "";
                    txtCommentTypeofInstr.Text = "";
                    txtMeasurementSystems.Text = "";
                    txtcommentset.Text = "";
                    txtcommentStep.Text = "";
                    txtcommentPrompt.Text = "";
                    txtcommentLessonProcedure.Text = "";
                }
            }

            string goalNames = "";
            strQuery2 = "select Goal.GoalCode from Goal inner join GoalLPRel on Goal.GoalId=GoalLPRel.GoalId inner join LessonPlan on LessonPlan.LessonPlanId=GoalLPRel.LessonPlanId where LessonPlan.LessonPlanId=" + lessonPlanId;

            DataTable Dt2 = objData.ReturnDataTable(strQuery2, false);
            if (Dt2 != null)
            {
                if (Dt2.Rows.Count > 0)
                {
                    for (int i = 0; i < Dt2.Rows.Count; i++)
                    {
                        goalNames += Dt2.Rows[i]["GoalCode"].ToString() + ",";
                    }
                    lblGoalName.Text = goalNames.TrimEnd(',');
                }
                else
                {
                    lblGoalName.Text = "";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void CheckAsigned(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        int LessonPlanId = 0;
        int count = 0;
        int statusExpId = 0;
        int statusDelId = 0;
        int statusInacId = 0;
        object objLessonId = null;
        object objcount = null;
        sess = (clsSession)Session["UserSession"];

        if (sess != null)
        {
            try
            {
                statusExpId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired' "));
                statusDelId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Deleted' "));
                statusInacId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive' "));
                strQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
                objLessonId = objData.FetchValue(strQuerry);
                if (objLessonId != null)
                {
                    LessonPlanId = Convert.ToInt32(objLessonId);
                }

                //strQuerry = "SELECT COUNT(DSTempHdrId) FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and LessonPlanId=" + LessonPlanId + " AND StatusId not in(" + statusExpId + "," + statusDelId + "," + statusInacId + ")";
                //objcount = objData.FetchValue(strQuerry);
                //if (objcount != null)
                //{
                //    count = Convert.ToInt32(objcount);
                //}
                //if (count > 1)
                //{
                //    BtnCopyTemplate.Visible = false;

                //}
                //else
                //{
                BtnCopyTemplate.Visible = true;

                //}


            }
            catch (Exception Ex)
            {

            }
        }

    }


    protected void lblDataFill(int headerID)
    {
        objData = new clsData();
        try
        {
            string strQuerry = "SELECT DSTemplateName+' '+isnull(VerNbr,'') as DSTemplateName FROM DSTempHdr WHERE DSTempHdrId = " + headerID;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    string lessonName = dtNew.Rows[0]["DSTemplateName"].ToString();
                    lblCaptnLesson.Text = "Currently Working On   :  ";
                    lblcurrntLessonName.ToolTip = lessonName;
                    lblcurrntLessonName.Text = (lessonName.Length > 50) ? (lessonName.Substring(0, 45) + "...") : lessonName;
                    //  lblLessonNameSet.Text = lessonName;
                    // lblLessonNameStep.Text = lessonName;
                    //   lblLessonNamePrompt.Text = lessonName;
                    // lblLessonNameProcedure.Text = lessonName;

                }
                else
                {
                    lblCaptnLesson.Text = "";
                    lblcurrntLessonName.Text = "";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected bool IsDiscrete(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        string chainType = "";
        int teachId = 0;
        string teachName = "";
        object objTeach = null;
        try
        {
            strQuerry = "SELECT SkillType,TeachingProcId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    chainType = dtNew.Rows[0]["SkillType"].ToString();
                    try
                    {
                        teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                    }
                    catch
                    {
                        teachId = 0;
                    }
                    if (teachId > 0)
                    {
                        try
                        {
                            strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                            objTeach = objData.FetchValue(strQuerry);
                            if (objTeach != null)
                            {
                                teachName = objTeach.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    if (chainType == "Discrete")
                    {
                        BtnAddStep.Visible = false;
                        btnAddStepDetails.Visible = false;
                        stepPanel.Style.Add("display", "None");
                        BtnAddSort.Visible = false;
                        btnAddStepCriteria.Visible = false;
                        HdfAddstep.Value = "1";
                        dlStepDetails.Visible = false;
                        dlStepCriteria.Visible = false;
                        lblStepStart.Visible = false;
                        lblStepCriStart.Visible = false;
                        if (teachName == "Match-to-Sample")
                        {
                            lblMatchtoSamples.Visible = true;
                            txtMatcSamples.Visible = true;
                            BtnAddSamples.Visible = true;
                            MatchSampleTooltip.Visible = true;
                            lstMatchSamples.Visible = true;
                            Disitem.Visible = false;
                            btnDeltSamples.Visible = true;
                            chk_distractor.Visible = true;
                            distractortooltip.Visible = true;
                        }
                        else
                        {
                            lblMatchtoSamples.Visible = false;
                            txtMatcSamples.Visible = false;
                            BtnAddSamples.Visible = false;
                            MatchSampleTooltip.Visible = false;
                            lstMatchSamples.Visible = false;
                            btnDeltSamples.Visible = false;
                            chk_distractor.Visible = false;
                            distractortooltip.Visible = false;
                        }
                        return true;
                    }
                    else
                    {
                        if (teachName == "Total Task")
                        {
                            lblMatchtoSamples.Visible = false;
                            txtMatcSamples.Visible = false;
                            BtnAddSamples.Visible = false;
                            MatchSampleTooltip.Visible = false;
                            lstMatchSamples.Visible = false;
                            btnDeltSamples.Visible = false;
                            BtnAddStep.Visible = true;
                            //stepPanel.Visible = true;
                            stepPanel.Style.Add("display", "Block");
                            btnAddStepDetails.Visible = true;
                            BtnAddSort.Visible = true;
                            btnAddStepCriteria.Visible = false;
                            HdfAddstep.Value = "1";
                            dlStepDetails.Visible = true;
                            dlStepCriteria.Visible = true;
                            lblVisibility(headerId);
                        }
                        else
                        {

                            lblMatchtoSamples.Visible = false;
                            txtMatcSamples.Visible = false;
                            BtnAddSamples.Visible = false;
                            MatchSampleTooltip.Visible = false;
                            lstMatchSamples.Visible = false;
                            btnDeltSamples.Visible = false;
                            BtnAddStep.Visible = true;
                            //stepPanel.Visible = true;
                            stepPanel.Style.Add("display", "Block");
                            btnAddStepDetails.Visible = true;
                            BtnAddSort.Visible = true;
                            btnAddStepCriteria.Visible = true;
                            dlStepDetails.Visible = true;
                            dlStepCriteria.Visible = true;
                            lblVisibility(headerId);               // Check whether alert label to show or not
                        }
                        return false;
                    }
                }
            }
            return false;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected bool IsMatchToSample(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        string chainType = "";
        int teachId = 0;
        string teachName = "";
        bool valid = false;
        object objTeach = null;
        try
        {
            strQuerry = "SELECT SkillType,TeachingProcId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    chainType = dtNew.Rows[0]["SkillType"].ToString();
                    try
                    {
                        teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                    }
                    catch
                    {
                        teachId = 0;
                    }
                    if (teachId > 0)
                    {
                        try
                        {
                            //strQuerry = "SELECT LookupName FROM LookUp WHERE LookupId = " + teachId;
                            strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                            objTeach = objData.FetchValue(strQuerry);
                            if (objTeach != null)
                            {
                                teachName = objTeach.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    if (chainType == "Discrete")
                    {
                        if (teachName == "Match-to-Sample")
                        {

                            valid = true;

                        }
                        else
                        {

                            valid = false;
                        }

                    }
                    else
                    {

                        valid = false;
                    }
                }
                else
                {
                    valid = false;
                }
            }
            else
            {
                valid = false;
            }
            return valid;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }




    protected void GetLessonProcData(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        try
        {
            strQuerry = "SELECT LessonDefInst,StudentReadCrita,StudCorrRespDef,StudIncorrRespDef,ReinforcementProc,CorrectionProc,Mistrial,MistrialResponse,TeacherPrepare,StudentPrepare,StudResponse FROM " +
                                " DSTempHdr WHERE DSTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    txtSDInstruction.Text = dtNew.Rows[0]["LessonDefInst"].ToString();
                    txtResponseOutcome.Text = dtNew.Rows[0]["StudentReadCrita"].ToString();
                    //txtCorrectResponse.Text = dtNew.Rows[0]["StudCorrRespDef"].ToString();
                    txtIncorrectResponse.Text = dtNew.Rows[0]["StudIncorrRespDef"].ToString();
                    txtReinforcementProc.Text = dtNew.Rows[0]["ReinforcementProc"].ToString();
                    txtCorrectionProcedure.Text = dtNew.Rows[0]["CorrectionProc"].ToString();
                    txtMistrial.Text = dtNew.Rows[0]["Mistrial"].ToString();
                    txtMistrialProcedure.Text = dtNew.Rows[0]["MistrialResponse"].ToString();
                    txtTeacherDo.Text = dtNew.Rows[0]["TeacherPrepare"].ToString();
                    txtStudentDo.Text = dtNew.Rows[0]["StudentPrepare"].ToString();
                    txtConsequenceDO.Text = dtNew.Rows[0]["StudResponse"].ToString();

                }
                else
                {
                    txtSDInstruction.Text = "";
                    txtResponseOutcome.Text = "";
                    txtIncorrectResponse.Text = "";
                    txtReinforcementProc.Text = "";
                    txtCorrectionProcedure.Text = "";
                    txtMistrial.Text = "";
                    txtMistrialProcedure.Text = "";
                    txtTeacherDo.Text = "";
                    txtStudentDo.Text = "";
                    txtConsequenceDO.Text = "";



                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void GetPromptProcedureList(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        try
        {
            strQuerry = "SELECT dsprompt.PromptId As ID,lukUp.LookupName As Name FROM DSTempPrompt dsprompt LEFT JOIN [LookUp] lukUp ON dsprompt.PromptId = lukUp.LookupId WHERE DSTempHdrId = '" + headerId + "' ORDER BY lukUp.SortOrder";
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    lstSelectedPrompts.DataSource = dtNew;
                    lstSelectedPrompts.DataTextField = "Name";
                    lstSelectedPrompts.DataValueField = "ID";
                    lstSelectedPrompts.DataBind();
                    lstCompletePrompts.Items.Clear();
                    objData.ReturnListBox("SELECT LookupId As ID,LookupName As Name FROM [LookUp] WHERE LookupType = 'DSTempPrompt' AND  LookupId NOT IN (Select PromptId FROM DSTempPrompt WHERE DSTempHdrId = " + headerId + ") ORDER BY SortOrder", lstCompletePrompts);

                }
                else
                {
                    lstSelectedPrompts.Items.Clear();
                    objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt' ORDER BY SortOrder", lstCompletePrompts);
                }
            }
            else
            {
                lstSelectedPrompts.Items.Clear();
                objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt' ORDER BY SortOrder", lstCompletePrompts);
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void FillDropPrompt(int headerId)
    {
        objData = new clsData();
        string naValue = "";
        string selQuerry = "";
        try
        {

            selQuerry = "SELECT LookupId FROM LookUp WHERE LookupType = 'Datasheet-Prompt Procedures' AND LookupName = 'NA'";
            object val = objData.FetchValue(selQuerry);

            if (val != null) naValue = val.ToString();

            selQuerry = "SELECT PromptTypeId FROM DSTempHdr WHERE DsTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    try
                    {
                        ddlPromptProcedure.SelectedValue = dtNew.Rows[0]["PromptTypeId"].ToString();
                        if (naValue != "")
                        {
                            if (ddlPromptProcedure.SelectedValue == naValue.ToString())
                            {
                                SelectedPromptTool.Visible = false;
                                CompletePromptTool.Visible = false;
                                lstCompletePrompts.Visible = false;
                                lstSelectedPrompts.Visible = false;
                                BtnAddPromptSelctd.Visible = false;
                                BtnAddAllPrompt.Visible = false;
                                BtnRemvePrmptSelctd.Visible = false;
                                BtnRemoveAllPrmpt.Visible = false;
                                lblSelctPrompt.Visible = false;
                            }
                            else
                            {
                                SelectedPromptTool.Visible = true;
                                CompletePromptTool.Visible = true;
                                lstCompletePrompts.Visible = true;
                                lstSelectedPrompts.Visible = true;
                                BtnAddPromptSelctd.Visible = true;
                                BtnAddAllPrompt.Visible = true;
                                BtnRemvePrmptSelctd.Visible = true;
                                BtnRemoveAllPrmpt.Visible = true;
                                lblSelctPrompt.Visible = true;
                            }
                        }
                    }
                    catch
                    {
                        ddlPromptProcedure.SelectedIndex = 0;
                    }
                }
                else
                {
                    lstCompletePrompts.Items.Clear();
                    lstSelectedPrompts.Items.Clear();
                    ddlPromptProcedure.SelectedIndex = 0;
                }

            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }
    //protected void BtnLessonInfo_Click(object sender, EventArgs e)
    //{
    //    objData = new clsData();
    //    DataTable dtNew = new DataTable();
    //    string strQuery = "";
    //    int headerId = 0;
    //    int lessonPlanId = 0;
    //    sess = (clsSession)Session["UserSession"];
    //    if (sess != null)
    //    {
    //        if (ViewState["HeaderId"] != null)
    //        {
    //            try
    //            {
    //                headerId = Convert.ToInt32(ViewState["HeaderId"]);
    //                strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
    //                dtNew = objData.ReturnDataTable(strQuery, false);
    //                if (dtNew != null)
    //                {
    //                    if (dtNew.Rows.Count > 0)
    //                    {
    //                        lessonPlanId = Convert.ToInt32(dtNew.Rows[0]["LessonPlanId"]);
    //                    }
    //                }
    //            }
    //            catch
    //            {
    //            }
    //        }
    //    }
    //}
    protected void BtnUpdate_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string skilltype = string.Empty;
        int teachProcId = 0;
        int noofTrial = 0;
        int tTaskType = 0;
        string StrNoTril;
        string taskAnalysis = string.Empty;
        string taskOther = string.Empty;
        string strQuery = "";
        int headerId = 0;
        //string drpValue = drpTeachingProc.SelectedItem.Text;
        string MatchToSample = "";
        string MTSRecOrExp = "";
        string totalTask = "";
        string drpTProcTxt = "";
        string drpTProcId = "";
        ClearSetData();
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {           
            string txtCommentTypeofInstrP = txtCommentTypeofInstr.Text.Trim().Replace("'", "''");
            string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteTypeInstruction='" + txtCommentTypeofInstrP + "' WHERE DSTempHdrId='" + headerId + "'";
            objData.Execute(UpdateAppr);

            if (chkDiscrete.Checked == true)
            {
                skilltype = "Discrete";
            }
            else
            {
                skilltype = "Chained";
                txtNoofTrail.Text = "";
            }
            try
            {
                drpTProcTxt = drpTeachingProc.SelectedItem.Text.Trim();
                drpTProcId = drpTeachingProc.SelectedItem.Value;
            }
            catch { }
            string query1 = "";
            string drpValue1 = "";
            query1 = "select LookupDesc from LookUp where LookupId='" + drpTProcId + "'";
            DataTable dt1 = objData.ReturnDataTable(query1, false);
            if (dt1.Rows.Count > 0)
            {
                drpValue1 = dt1.Rows[0]["LookupDesc"].ToString();
                if (drpValue1 == "Match-to-Sample")
                {
                    if (rbtnMatchToSampleExpressive.SelectedItem.Text.Trim() == "Receptive")
                    {
                        if (rdoRandomMoveover.SelectedItem.Text.Trim() == "Randomized")
                        {
                            MatchToSample = "Randomized";
                        }
                        else
                        {
                            MatchToSample = "Move Over Items";
                        }
                        MTSRecOrExp = "Receptive";
                    }
                    else
                    {
                        MatchToSample = "MTSisExpressive";
                        MTSRecOrExp = "Expressive";
                    }

                }

                else
                    MatchToSample = "";
                if (drpValue1 == "Total Task")
                {
                    if (chkTotalRandom.Checked == true)
                    {
                        totalTask = "Randomized";
                    }
                    else
                        totalTask = "";

                }
                else
                    totalTask = "";
            }
            try
            {
                noofTrial = Convert.ToInt32(txtNoofTrail.Text.Trim());
                StrNoTril = noofTrial.ToString();
            }
            catch
            {
                noofTrial = 0;
                StrNoTril = "''";
            }

            if (drpTasklist.SelectedValue != "0" && chkDiscrete.Checked == false)
            {
                if (drpTasklist.SelectedItem.Text == "Other")
                {
                    taskAnalysis = "Total Task";
                    taskOther = "1";
                }
                else
                {
                    taskAnalysis = drpTasklist.SelectedItem.Text;
                    taskOther = "0";
                }
            }
            try
            {
                teachProcId = Convert.ToInt32(drpTeachingProc.SelectedValue);
            }
            catch
            {
                teachProcId = 0;
            }
            try
            {
                if (drpValue1 == "Total Task" && chkDiscrete.Checked == false && chk_stepBystep.Checked == true && (drpTasklist.SelectedItem.Text == "Total Task" || drpTasklist.SelectedItem.Text == "Other"))
                {
                    tTaskType = 1;
                }
                else if (drpValue1 == "Total Task" && chkDiscrete.Checked == false && chk_stepBystep.Checked == false && (drpTasklist.SelectedItem.Text == "Total Task" || drpTasklist.SelectedItem.Text == "Other"))
                {
                    tTaskType = 0;
                }
            }
            catch
            {
                tTaskType = 0;
            }

            if (ValidateTeach() == true)
            {
                try
                {

                    if (skilltype == "Discrete")
                    {
                        //strQuery = "Select LookupId from LookUp where LookupName = 'Match-to-Sample'";
                        //DataTable dt = new DataTable();
                        //dt = objData.ReturnDataTable(strQuery, false);
                        //string MTS_id = ""; // match to sample id
                        //if (dt.Rows.Count > 0)
                        //{

                        //    MTS_id = dt.Rows[0]["LookupId"].ToString();
                        //}
                        ////pramod

                        //fn_isMatchToSample(drpTeachingProc.SelectedValue);

                        if (fn_isMatchToSample(drpTeachingProc.SelectedValue))
                        {
                            strQuery = "update DSTempHdr set TeachingProcId=" + teachProcId + ",SkillType='" + skilltype + "',TotalTaskFormat='" + totalTask + "',MatchToSampleType='" + MatchToSample + "',MatchToSampleRecOrExp='" + MTSRecOrExp + "',NbrOfTrials=" + StrNoTril + ",ChainType='" + taskAnalysis + "'," +
                        "MajorSetting='" + clsGeneral.convertQuotes(txtmajorset.Text.Trim()) + "',MinorSetting='" + clsGeneral.convertQuotes(txtminorset.Text.Trim()) + "',Baseline='" + clsGeneral.convertQuotes(txtBaseline.Text.Trim()) + "',Objective='" + clsGeneral.convertQuotes(txtobjective.Text.Trim()) + "',GeneralProcedure='" + clsGeneral.convertQuotes(txtGenProce.Text.Trim()) + "',ModifiedBy=" + sess.LoginId + ",ModifiedOn=GETDATE() where DSTempHdrId =" + headerId + "";
                            objData.Execute(strQuery);
                            IsDiscrete(headerId);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertSuccessMsg();", true);

                        }
                        else
                        {
                            if (CheckAnyCriteria(headerId) == true)
                            {
                                strQuery = "update DSTempHdr set TeachingProcId=" + teachProcId + ",SkillType='" + skilltype + "',TotalTaskFormat='" + totalTask + "',MatchToSampleType='" + MatchToSample + "',MatchToSampleRecOrExp='" + MTSRecOrExp + "',NbrOfTrials=" + StrNoTril + ",ChainType='" + taskAnalysis + "'," +
                             "MajorSetting='" + clsGeneral.convertQuotes(txtmajorset.Text.Trim()) + "',MinorSetting='" + clsGeneral.convertQuotes(txtminorset.Text.Trim()) + "',Baseline='" + clsGeneral.convertQuotes(txtBaseline.Text.Trim()) + "',Objective='" + clsGeneral.convertQuotes(txtobjective.Text.Trim()) + "',GeneralProcedure='" + clsGeneral.convertQuotes(txtGenProce.Text.Trim()) + "',ModifiedBy=" + sess.LoginId + ",ModifiedOn=GETDATE() where DSTempHdrId =" + headerId + "";
                                objData.Execute(strQuery);
                                IsDiscrete(headerId);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertSuccessMsg();", true);
                            }
                            else if (checkIsVTool() == true)
                            {
                                strQuery = "update DSTempHdr set TeachingProcId=" + teachProcId + ",SkillType='" + skilltype + "',TotalTaskFormat='" + totalTask + "',MatchToSampleType='" + MatchToSample + "',MatchToSampleRecOrExp='" + MTSRecOrExp + "',NbrOfTrials=" + StrNoTril + ",ChainType='" + taskAnalysis + "'," +
                            "MajorSetting='" + clsGeneral.convertQuotes(txtmajorset.Text.Trim()) + "',MinorSetting='" + clsGeneral.convertQuotes(txtminorset.Text.Trim()) + "',Baseline='" + clsGeneral.convertQuotes(txtBaseline.Text.Trim()) + "',Objective='" + clsGeneral.convertQuotes(txtobjective.Text.Trim()) + "',GeneralProcedure='" + clsGeneral.convertQuotes(txtGenProce.Text.Trim()) + "',ModifiedBy=" + sess.LoginId + ",ModifiedOn=GETDATE() where DSTempHdrId =" + headerId + "";
                                objData.Execute(strQuery);
                                IsDiscrete(headerId);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertSuccessMsg();", true);
                            }
                            else
                            {
                                FillTypeOfInstruction(headerId);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertConvertDisc();", true);
                            }
                        }

                    }
                    else
                    {

                        if (checkIsTotalTask(headerId) == true)
                        {
                            strQuery = "update DSTempHdr set TeachingProcId=" + teachProcId + ",SkillType='" + skilltype + "',TotalTaskFormat='" + totalTask + "',TotalTaskType='" + tTaskType + "',TaskOther='" + taskOther + "',MatchToSampleType='" + MatchToSample + "',MatchToSampleRecOrExp='" + MTSRecOrExp + "',NbrOfTrials=" + StrNoTril + ",ChainType='" + taskAnalysis + "'," +
                                "MajorSetting='" + clsGeneral.convertQuotes(txtmajorset.Text.Trim()) + "',MinorSetting='" + clsGeneral.convertQuotes(txtminorset.Text.Trim()) + "',Baseline='" + clsGeneral.convertQuotes(txtBaseline.Text.Trim()) + "',Objective='" + clsGeneral.convertQuotes(txtobjective.Text.Trim()) + "',GeneralProcedure='" + clsGeneral.convertQuotes(txtGenProce.Text.Trim()) + "',ModifiedBy=" + sess.LoginId + ",ModifiedOn=GETDATE() where DSTempHdrId =" + headerId + "";
                            objData.Execute(strQuery);

                            IsDiscrete(headerId);                // Check updated template is chained or discrete.
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertSuccessMsg();", true);
                        }
                        else
                        {
                            FillTypeOfInstruction(headerId);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertConvertTotalTask();", true);
                        }
                    }

                    GetStatus(headerId);


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            //ash
            if (skilltype == "Discrete")
            {
                if (IsMatchToSample(headerId) == true)
                {
                    sess = (clsSession)Session["UserSession"];
                    if (ViewState["HeaderId"] != null)
                    {
                        headerId = Convert.ToInt32(ViewState["HeaderId"]);
                    }

                    string sqlQuery = "select DSTempSetId,samples,DistractorSamples from DSTempSet where DSTempHdrId=" + headerId;

                    DataTable dt = new DataTable();
                    dt = objData.ReturnDataTable(sqlQuery, false);

                    foreach (DataRow dr in dt.Rows)
                    {
                        int setId = Convert.ToInt32(dr["DSTempSetId"]);
                        if (setId != null)
                        {
                            if (setId != 0)
                            {
                                string samples = dr["samples"].ToString();
                                int length = samples.Length;
                                string distractorsamples = dr["DistractorSamples"].ToString();
                                int dislength = distractorsamples.Length;
                                string matchSelctdNew = "";
                                if (length > 0)
                                {
                                    matchSelctdNew = samples.ToString().Substring(0, length - 1);
                                }

                                if (IsMatchToSample(headerId) == true)
                                {
                                    if (matchSelctdNew != "")
                                    {
                                        if (matchSelctdNew != "")
                                        {
                                            MatchSampDef(headerId, matchSelctdNew, setId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //ash
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        lblSelParentSets.Text = "Not Assigned to any Sets";
        ClearStepData();
        FillSetDrpList();
        BtnUpdateStep.Visible = false;
        btnAddStepDetails.Visible = true;

        chkEnd.Enabled = true;
        chkEnd.Checked = true;
        ddlSortOrder.Enabled = false;

        lblAddorUpdateStep.Text = "Add Step";

        FillSetDrpList();
        ClearSetData();
        BtnUpdateSetDetails.Visible = false;
        btnAddSetDetails.Visible = true;
        lblAddOrUpdateSet.Text = "Add Set";

        drpTasklist_SelectedIndexChanged1(sender, e);
        fn_setChkTotalTask(headerId);
    }

    private bool fn_isMatchToSample(string teachingProcId)
    {
        objData = new clsData();
        string strQuery = "Select LookupId from LookUp where LookupDesc = 'Match-to-Sample'";
        DataTable dt = new DataTable();
        dt = objData.ReturnDataTable(strQuery, false);
        int dt_count = dt.Rows.Count;
        if (dt_count > 0)
        {
            for (int i = 0; i < dt_count; i++)
            {
                if (dt.Rows[i]["LookupId"].ToString() == teachingProcId)
                    return true;
            }
        }

        return false;
    }



    private bool checkIsTotalTask(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        int countCriteria = 0;
        bool valid = false;
        object objVal = null;
        int columnId = 0;
        string teachName = "";
        object objTeach = null;
        //strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + drpTeachingProc.SelectedValue;  //---Commmented for Fixing Errorlog production log 2020
        //objTeach = objData.FetchValue(strQuerry);  //---Commmented for Fixing Errorlog production log 2020

        string drpTeachingValue = drpTeachingProc.SelectedValue;
        if (!String.IsNullOrEmpty(drpTeachingValue))
        {
            strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + drpTeachingValue;
            objTeach = objData.FetchValue(strQuerry);
        }

        if (objTeach != null)
        {
            teachName = objTeach.ToString();
        }

        if ((teachName != "Total Task") || (drpTasklist.Text != "Total Task"))
            return true;

        DataTable dtNew = new DataTable();
        strQuerry = "SELECT DSTempSetColId FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
        dtNew = objData.ReturnDataTable(strQuerry, false);
        if (dtNew != null)
        {
            if (dtNew.Rows.Count > 0)
            {
                for (int i = 0; i < dtNew.Rows.Count; i++)
                {
                    columnId = Convert.ToInt32(dtNew.Rows[i]["DSTempSetColId"]);
                    strQuerry = "SELECT COUNT(DSTempRuleId) FROM DSTempRule WHERE DSTempSetColId = " + columnId + " AND RuleType = 'STEP' AND ActiveInd = 'A'";
                    objVal = objData.FetchValue(strQuerry);
                    if (objVal != null)
                    {
                        countCriteria = Convert.ToInt32(objVal);
                    }
                    if (countCriteria > 0)
                    {
                        return false;
                    }
                    else
                    {
                        valid = true;
                    }
                }
            }
            else
            {
                valid = true;
            }
        }
        else
        {
            valid = true;
        }

        return valid;

    }






    private bool CheckAnyCriteria(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        int countStep = 0;
        int countCriteria = 0;
        bool valid = false;
        object objVal = null;
        object objCount = null;
        int columnId = 0;
        DataTable dtNew = new DataTable();
        strQuerry = "SELECT COUNT(DSTempStepId) FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A' AND IsDynamic=0";
        objCount = objData.FetchValue(strQuerry);
        if (objCount != null)
        {
            countStep = Convert.ToInt32(objCount);
        }
        if (countStep == 0)
        {
            strQuerry = "SELECT DSTempSetColId FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    for (int i = 0; i < dtNew.Rows.Count; i++)
                    {
                        columnId = Convert.ToInt32(dtNew.Rows[i]["DSTempSetColId"]);
                        strQuerry = "SELECT COUNT(DSTempRuleId) FROM DSTempRule WHERE DSTempSetColId = " + columnId + " AND RuleType = 'STEP' AND ActiveInd = 'A'";
                        objVal = objData.FetchValue(strQuerry);
                        if (objVal != null)
                        {
                            countCriteria = Convert.ToInt32(objVal);
                        }
                        if (countCriteria > 0)
                        {
                            return false;
                        }
                        else
                        {
                            valid = true;
                        }
                    }
                }
                else
                {
                    valid = true;
                }
            }
            else
            {
                valid = true;
            }

        }
        else
        {
            valid = false;
        }
        return valid;
    }


    protected bool ValidateTeach()
    {
        objData = new clsData();
        if (drpTeachingProc.SelectedValue == "0")
        {
            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "ValidateTeachingProc();", true);
            return false;
        }

        if (chkDiscrete.Checked == true)
        {
            if (txtNoofTrail.Text == "")
            {
                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "ValidateNoofTrials();", true);
                return false;
            }
        }
        else if (chkDiscrete.Checked == false)
        {
            if (drpTasklist.SelectedValue == "0")
            {
                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "ValidateDrpclass();", true);
                return false;
            }
        }
        return true;
    }

    protected void ClearTypeInstruction()
    {
        setTeachingDDls("Clear All", "0");
        // rdolistDatasheet.SelectedValue = "1";
        drpTasklist.SelectedIndex = 0;
        txtmajorset.Text = "";
        txtminorset.Text = "";
        txtobjective.Text = "";
        txtBaseline.Text = "";
        chkDiscrete.Checked = false;

    }
    private void setTeachingDDls(string teachingType, string id)
    {
        switch (teachingType)
        {
            case "Teaching Method":

                /// CLEAR BOTH THE DROPDOWNLISTS
                /// 
                drpTeachingProc.Items.Clear();
                drp_teachingFormat.Items.Clear();

                /// GET LOOKUP DETAILS USING ID (TEACHING METHOD ID)
                /// 
                objData = new clsData();
                string query = "select LookupName,LookupDesc,ParentLookupId from LookUp where LookupId='" + id + "'";
                DataTable dt = objData.ReturnDataTable(query, false);

                /// FILL TEACHING FORMAT
                /// 
                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='Teaching Format'", drp_teachingFormat);

                /// SET THE TEACHING FORMAT TO SELECTED
                /// 
                drp_teachingFormat.SelectedValue = dt.Rows[0]["ParentLookupId"].ToString();

                /// FILL THE TEACHING METHOD WITH RESPECT TO TEACHING FORMAT
                /// 
                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where ParentLookupId=" + dt.Rows[0]["ParentLookupId"].ToString(), drpTeachingProc);

                /// SET THE SELECTED TEACHING METHOD
                /// 
                drpTeachingProc.SelectedValue = id.ToString();

                break;

            case "Teaching Format":

                /// CLEAR BOTH THE DROPDOWNLISTS
                /// 
                drpTeachingProc.Items.Clear();
                drp_teachingFormat.Items.Clear();

                /// FILL TEACHING FORMAT
                /// 
                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LooupType='Teaching Format'", drp_teachingFormat);

                /// SET THE TEACHING FORMAT TO SELECTED
                /// 
                drp_teachingFormat.SelectedValue = id.ToString();

                /// FILL THE TEACHING METHOD WITH RESPECT TO TEACHING FORMAT
                /// 
                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where ParentLookupId=" + id.ToString(), drpTeachingProc);

                break;

            case "Clear All":

                /// CLEAR BOTH THE DROPDOWNLISTS
                /// 
                drpTeachingProc.Items.Clear();
                drp_teachingFormat.Items.Clear();

                /// FILL TEACHING FORMAT
                /// 
                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='Teaching Format'", drp_teachingFormat);

                /// FILL THE TEACHING METHOD WITH RESPECT TO TEACHING FORMAT
                /// 
                drpTeachingProc.Items.Add(new ListItem("--- Select Teaching Format ---", "0"));

                break;

        }
    }

    protected void FillTypeOfInstruction(int headerId)
    {
        objData = new clsData();
        string skillType = "";
        string chainType = "";
        string taskOth = "";
        string totalTask = "";
        string promptLevelRadio = "";
        int noofTrials = 0;
        string teachingProcId = "";
        string MatchToSample = "";
        string MTSRecOrExp = "";
        //if (ViewState["HeaderId"] != null)
        //{
        //    headerId = Convert.ToInt32(ViewState["HeaderId"]);
        //}
        try
        {
            /// RESET THE TEACHING DROPDOWN LISTS
            /// 
            // setTeachingDDls("Clear All", 0);

            objData.ReturnDropDown("Select LookupId as Id , LookupCode as Name from dbo.LookUp where LookupType='Datasheet-Teaching Procedures' and ActiveInd='A'", drpTeachingProc);

            string selQuerry = "SELECT TeachingProcId, SkillType,MatchToSampleType,MatchToSampleRecOrExp,NbrOfTrials,ChainType,TotalTaskFormat,TotalTaskType,TaskOther,MajorSetting,MinorSetting,Baseline,Objective,GeneralProcedure FROm DSTempHdr WHERE DSTempHdrId = " + headerId;

            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    teachingProcId = dtList.Rows[0]["TeachingProcId"].ToString();
                    if (teachingProcId.Length>0 && teachingProcId != null)
                    {
                        try
                        {
                            //drpTeachingProc.SelectedValue = teachingProcId;
                            setTeachingDDls("Teaching Method", teachingProcId);
                        }
                        catch
                        {
                            //drpTeachingProc.SelectedValue = "0";
                            setTeachingDDls("Clear All", "0");
                        }
                    }
                    else
                    {
                        if(drp_teachingFormat.SelectedValue !=null)
                            if (drp_teachingFormat.SelectedValue !="0")
                                objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where ParentLookupId=" + drp_teachingFormat.SelectedValue.ToString(), drpTeachingProc);
                            else
                                setTeachingDDls("Clear All", "0");
                    }

                    string drpTProcTxt = drpTeachingProc.SelectedItem.Text;
                    string drpTProcId = drpTeachingProc.SelectedItem.Value;
                    string query1 = "";
                    query1 = "select LookupName,LookupDesc from LookUp where LookupId='" + drpTProcId + "'";
                    DataTable dt1 = objData.ReturnDataTable(query1, false);

                    if (dt1.Rows.Count > 0)
                    {
                        string drpValue2 = dt1.Rows[0]["LookupDesc"].ToString();
                        if (drpValue2 == "Match-to-Sample")
                        {
                            rbtnMatchToSampleExpressive.Visible = true;
                        }
                        else
                        {
                            //  rbtnMatchToSampleExpressive.Visible = false;
                        }
                    }

                    skillType = dtList.Rows[0]["SkillType"].ToString();
                    MatchToSample = dtList.Rows[0]["MatchToSampleType"].ToString();
                    MTSRecOrExp = dtList.Rows[0]["MatchToSampleRecOrExp"].ToString();
                    totalTask = dtList.Rows[0]["TotalTaskFormat"].ToString();
                    if (skillType == "Chained")
                    {
                        if (MTSRecOrExp == "Expressive")
                        {
                            rbtnMatchToSampleExpressive.SelectedValue = "1";
                            rdoRandomMoveover.Visible = false;
                        }
                        else
                        {
                            rbtnMatchToSampleExpressive.SelectedValue = "0";
                            //  rdolistDatasheet.SelectedValue = "1";
                            if (MatchToSample == "Randomized")
                            {
                                rdoRandomMoveover.Visible = true;
                                rdoRandomMoveover.SelectedValue = "1";
                            }
                            else if (MatchToSample == "Move Over Items")
                            {
                                rdoRandomMoveover.Visible = true;
                                rdoRandomMoveover.SelectedValue = "0";
                            }
                            else
                                rdoRandomMoveover.Visible = false;
                        }
                        //Randomize total task

                        if (totalTask == "Randomized")
                        {
                            chkTotalRandom.Visible = true;
                            chkTotalRandom.Checked = true;
                        }

                        else
                        {
                           // chkTotalRandom.Visible = false;
                            chkTotalRandom.Checked = false;
                        }
                        chkDiscrete.Checked = false;
                        //showtrailid.Visible = false;
                        // txtNoofTrail.Visible = false;
                        taskAnalysis.Visible = true;
                        drpTasklist.Visible = true;

                        taskOth = dtList.Rows[0]["TaskOther"].ToString();
                        if (taskOth == "1")
                        {
                            chainType = "Other";
                        }
                        else
                        {
                            chainType = dtList.Rows[0]["ChainType"].ToString();
                        }
                        drpTasklist.SelectedValue = chainType.ToString();

                        rbtnPromptLevel.Visible = false;
                        // lblPromptLevel.Visible = false;
                        //string drpTProcTxt = drpTeachingProc.SelectedItem.Text;
                        //string drpTProcId = drpTeachingProc.SelectedItem.Value;
                        //string query1 = "";
                        //query1 = "select LookupName from LookUp where LookupCode='" + drpTProcTxt + "' and LookupId='" + drpTProcId + "'";
                        //DataTable dt1 = objData.ReturnDataTable(query1, false);
                        if (dt1.Rows.Count > 0)
                        {
                            string drpValue1 = dt1.Rows[0]["LookupDesc"].ToString();
                            if ((drpTasklist.SelectedIndex == 3 || drpTasklist.SelectedIndex == 4) && drpValue1 == "Total Task")
                            {
                                rbtnPromptLevel.Visible = true;
                                //lblPromptLevel.Visible = true;
                            }
                        }

                        //promptLevelRadio = dtList.Rows[0]["TotalTaskType"].ToString();
                        //if (promptLevelRadio == "0")
                        //{
                        //    rbtnPromptLevel.SelectedValue = "0";
                        //}
                        //else if (promptLevelRadio == "1")
                        //{
                        //    rbtnPromptLevel.SelectedValue = "1";
                        //}
                        //else
                        //{
                        //    rbtnPromptLevel.SelectedValue = "1";
                        //}
                    }
                    else if (skillType == "Discrete")
                    {
                        if (MTSRecOrExp == "Expressive")
                        {
                            rbtnMatchToSampleExpressive.SelectedValue = "1";
                            rdoRandomMoveover.Visible = false;
                        }
                        else
                        {
                            rbtnMatchToSampleExpressive.SelectedValue = "0";
                            // rdolistDatasheet.SelectedValue = "0";
                            if (MatchToSample == "Randomized")
                            {
                                rdoRandomMoveover.Visible = true;
                                rdoRandomMoveover.SelectedValue = "1";
                            }
                            else if (MatchToSample == "Move Over Items")
                            {
                                rdoRandomMoveover.Visible = true;
                                rdoRandomMoveover.SelectedValue = "0";
                            }
                            else
                                rdoRandomMoveover.Visible = false;
                        }

                        //Randomize total task
                        if (totalTask == "Randomized")
                        {
                            chkTotalRandom.Visible = true;
                            chkTotalRandom.Checked = true;
                        }

                        else
                        {
                            //chkTotalRandom.Visible = false;
                            chkTotalRandom.Checked = false;
                        }
                        chkDiscrete.Checked = true;
                        txtNoofTrail.Visible = true;
                        showtrailid.Visible = true;
                        drpTasklist.Visible = false;
                        noofTrials = Convert.ToInt32(dtList.Rows[0]["NbrOfTrials"]);
                        txtNoofTrail.Text = noofTrials.ToString();
                    }
                    else
                    {
                        rdoRandomMoveover.Visible = false;
                       // chkTotalRandom.Visible = false;
                        //showtrailid.Visible = false;
                        //txtNoofTrail.Visible = false;
                        taskAnalysis.Visible = true;
                        drpTasklist.Visible = true;
                        //   rdolistDatasheet.SelectedValue = "1";
                        chkDiscrete.Checked = false;
                        drpTasklist.SelectedIndex = 0;
                    }

                    txtmajorset.Text = dtList.Rows[0]["MajorSetting"].ToString();
                    txtminorset.Text = dtList.Rows[0]["MinorSetting"].ToString();
                    txtBaseline.Text = dtList.Rows[0]["Baseline"].ToString();
                    txtobjective.Text = dtList.Rows[0]["Objective"].ToString();
                    txtGenProce.Text = dtList.Rows[0]["GeneralProcedure"].ToString();
                }
                else
                {
                    txtmajorset.Text = "";
                    txtminorset.Text = "";
                    txtobjective.Text = "";
                    txtBaseline.Text = "";
                    txtGenProce.Text = "";
                    drpTasklist.SelectedIndex = 0;
                    chkDiscrete.Checked = false;
                    txtNoofTrail.Text = "";

                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }
    protected void btnAddSetDetails_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        object objCount = null;
        int headerId = 0;
        int findCount = 0;
        int newCount = 0;
        string insertQuerry = "";
        string setMatch = "";
        string matchSelctd = "";
        string DistractorsetMatch = "";
        string matchDistractors = "";
        int SetId = 0;
        int length = 0;
        int Dislength = 0;
        int DistractCount = 0;
        try
        {
            if (sess != null)
            {

                if (ViewState["HeaderId"] != null)
                {
                    headerId = Convert.ToInt32(ViewState["HeaderId"]);
                }

                if (IsDiscrete(headerId) == true)
                {
                    string[] arryMatchValue = new string[lstMatchSamples.Items.Count];
                    if (lstMatchSamples.Items.Count > 0)
                    {
                        for (int i = 0; i < lstMatchSamples.Items.Count; i++)
                        {
                            arryMatchValue[i] = lstMatchSamples.Items[i].Value.ToString();
                        }
                        for (int arryInt = 0; arryInt < lstMatchSamples.Items.Count; arryInt++)
                        {
                            setMatch += arryMatchValue[arryInt].ToString() + ",";
                        }
                        length = setMatch.Length;
                        matchSelctd = setMatch.ToString().Substring(0, length - 1);

                        //}


                        string[] DistractarryMatchValue = new string[Disitem.Items.Count];
                        if (Disitem.Items.Count > 0)
                        {
                            DistractCount = Disitem.Items.Count;
                            for (int i = 0; i < Disitem.Items.Count; i++)
                            {
                                DistractarryMatchValue[i] = Disitem.Items[i].Value.ToString();
                    }
                            for (int arryInt = 0; arryInt < Disitem.Items.Count; arryInt++)
                            {
                                DistractorsetMatch += DistractarryMatchValue[arryInt].ToString() + ",";
                            }
                            Dislength = DistractorsetMatch.Length;
                            matchDistractors = DistractorsetMatch.ToString().Substring(0, Dislength - 1);

                }
                    }

                }
                string selQuerry = "SELECT COUNT(*) FROM DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                objCount = objData.FetchValue(selQuerry);
                if (objCount != null)
                {
                    findCount = Convert.ToInt32(objCount);
                }
                newCount = findCount + 1;
                try
                {
                    insertQuerry = "Insert Into DSTempSet(SchoolId,DSTempHdrId,SetCd,SetName,Samples,SortOrder,ActiveInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,DistractorSamples,DistractorSamplesCount) ";
                    insertQuerry += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtBoxAddSet.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtSetDescription.Text.Trim()) + "','" + clsGeneral.convertQuotes(setMatch) + "'," + newCount + ",'A'," + sess.LoginId + ",getdate()," + sess.LoginId + " ,getdate(),'" + clsGeneral.convertQuotes(DistractorsetMatch) + "'," + DistractCount + ") ";

                    SetId = objData.ExecuteWithScope(insertQuerry);


                    if (IsMatchToSample(headerId) == true)
                    {
                        if (matchSelctd != null)
                        {
                            MatchSampDef(headerId, matchSelctd, SetId);                 // Match to Sample code 
                        }
                    }

                    // ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseSetPopup();", true);
                    ClearSetData();

                    fillParentSetData();

                    GetSetData(headerId);
                    showMatchToSampleDrop();
                    GetStepData(headerId);
                    txtBoxAddSet.Focus();
                }
                catch (Exception Ex)
                {
                    string error = Ex.Message;
                    tdMsgSet.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }



    protected void MatchSampDef(int TId, string setMatch, int setId)
    {
        objData = new clsData();
        objMatch = new clsMathToSamples();
        clsMathToSamples.Step[] steps;
        // setMatch = setMatch.Remove(0, 1);
        int NoOfTrials = 0;
        string strQuerry = "";
        string deltQuerry = "";
        string samples = "";
        sess = (clsSession)Session["UserSession"];
        string[] setArray = setMatch.Split(',');
        string drpValue = rdoRandomMoveover.SelectedItem.Text.Trim();
        int order = 0;
        try
        {
            if (TId > 0)
            {
                NoOfTrials = Convert.ToInt32(objData.FetchValue("Select NbrofTrials from DSTempHdr where NbrofTrials <> '' AND SkillType = 'Discrete' AND DSTempHdrId=" + TId + ""));

                if (NoOfTrials != 0)
                {
                    deltQuerry = "UPDATE DSTempStep SET ActiveInd = 'D' WHERE DSTempSetId =" + setId + " AND IsDynamic=0";
                    objData.Execute(deltQuerry);
                    if (drpValue == "Randomized")
                    {
                        steps = clsMathToSamples.FormSteps(setArray, NoOfTrials);
                    }
                    else
                        steps = clsMathToSamples.FormStepsInOrder(setArray, NoOfTrials);

                    foreach (clsMathToSamples.Step step in steps)
                    {
                        if (step != null)
                        {
                            order++;
                            samples = step.TrialText + " ";

                            strQuerry = "Insert into DSTempStep (SchoolId,DSTempHdrId,DSTempSetId,StepName,SortOrder,ActiveInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) " +
                            "Values(" + sess.SchoolId + "," + TId + "," + setId + ",'" + clsGeneral.convertQuotes(samples) + "'," + order + ",'A'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate()) ";
                            objData.Execute(strQuerry);
                        }
                    }
                }

            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected clsMathToSamples.Step[] MatchSampDef2(int TId, string setMatch, int setId)
    {
        objData = new clsData();
        objMatch = new clsMathToSamples();
        clsMathToSamples.Step[] steps;
        int NoOfTrials = 0;
        sess = (clsSession)Session["UserSession"];
        string[] setArray = setMatch.Split(',');
        string drpValue = rdoRandomMoveover.SelectedItem.Text.Trim();
        try
        {
            if (TId > 0)
            {
                NoOfTrials = Convert.ToInt32(objData.FetchValue("Select NbrofTrials from DSTempHdr where NbrofTrials <> '' AND SkillType = 'Discrete' AND DSTempHdrId=" + TId + ""));
                if (NoOfTrials != 0)
                {
                    if (drpValue == "Randomized")
                    {
                        steps = clsMathToSamples.FormSteps(setArray, NoOfTrials);
                        return steps;
                    }
                    else
                    {
                        steps = clsMathToSamples.FormStepsInOrder(setArray, NoOfTrials);
                        return steps;
                    }
                }
            }
            return null;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void btnAddStepDetails_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        string strQuery = "";
        int parentSetId = 0;
        int findCount = 0;
        int newCount = 0;
        int stepId = 0;
        int parentStepId = 0;
        int count = 0;
        string setIds = "";
        string setNames = "";
        object objCount = null;
        sess = (clsSession)Session["UserSession"];
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        try
        {
            if (sess != null)
            {
                List<String> CountryID_list = new List<string>();
                //List<String> AllCountryID_list = new List<string>();
                List<String> CountryName_list = new List<string>();

                foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
                {
                    //AllCountryID_list.Add(item.Value); // Set Ids
                    if (item.Selected)
                    {
                        CountryID_list.Add(item.Value); // Set Ids
                        CountryName_list.Add(item.Text); // Set Names
                        setIds += item.Value + ",";
                        setNames += item.Text + ",";
                        count++;
                        parentSetId = Convert.ToInt32(item.Value);
                    }
                }



                //string selQuery = "SELECT ISNULL(MAX(SortOrder),0) FROM DSTempParentStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                //objCount = objData.FetchValue(selQuery);
                //int maxCount = Convert.ToInt32(objCount) + 1;
                //strQuery = "INSERT INTO DSTempParentStep(SchoolId,DSTempHdrId,StepCd,StepName,SortOrder,SetIds,SetNames,ActiveInd,CreatedBy,CreatedOn)";
                //strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "',";
                //strQuery += "'" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + maxCount + ",'" + setIds + "','" + setNames + "','A',";
                //strQuery += "" + sess.LoginId + ",getdate())";

                int selectedText = 0;
                if (chkEnd.Checked == true)
                {

                    string selQuery = "SELECT ISNULL(MAX(SortOrder),0) FROM DSTempParentStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                    objCount = objData.FetchValue(selQuery);
                    selectedText = Convert.ToInt32(objCount) + 1;
                }
                else
                {
                    selectedText = Convert.ToInt32(ddlSortOrder.SelectedValue);

                    string selectQry = "SELECT DSTempParentStepId, SortOrder FROM  DSTempParentStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                    DataTable dtList = objData.ReturnDataTable(selectQry, false);
                    var SelectedSteps = dtList.AsEnumerable().Where(x => x.Field<int>("SortOrder") >= selectedText).ToList();
                    foreach (var sel in SelectedSteps)
                    {
                        string updateQuerry = "UPDATE DSTempParentStep SET SortOrder = " + (Convert.ToInt32(sel["SortOrder"]) + 1) + " WHERE DSTempParentStepId= " + sel["DSTempParentStepId"].ToString();
                        objData.Execute(updateQuerry);
                    }
                }


                //string selQuery1 = "SELECT DSTempParentSetId FROM DSTempParentStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                //objCount = objData.FetchValue(selQuery1);
                //UpdateCompleteSteponParentSet(Convert.ToInt32(objCount), headerId);


                strQuery = "INSERT INTO DSTempParentStep(SchoolId,DSTempHdrId,StepCd,StepName,SortOrder,SetIds,SetNames,ActiveInd,CreatedBy,CreatedOn)";
                strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "',";
                strQuery += "'" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + selectedText + ",'" + setIds + "','" +clsGeneral.convertQuotes(setNames) + "','A',";
                strQuery += "" + sess.LoginId + ",getdate())";










                parentStepId = objData.ExecuteWithScope(strQuery);
                if (count > 1)
                {
                    //if(CountryID_list.Count<=0)
                    //    CountryID_list=AllCountryID_list;

                    for (int index = 0; index < CountryID_list.Count; index++)
                    {
                        //---------------------------------

                        //string selQuerry = "SELECT ISNULL(MAX(SortOrder),0) FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A' AND IsDynamic=0" +
                        //    " AND DSTempSetId = " + Convert.ToInt32(CountryID_list[index]);
                        //objCount = objData.FetchValue(selQuerry);
                        //if (objCount != null)
                        //{
                        //    findCount = Convert.ToInt32(objCount);
                        //}
                        //newCount = findCount + 1;

                        //try
                        //{
                        //    strQuery = "Insert Into DSTempStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,DSTempParentStepId,SortOrder,ActiveInd,CreatedBy,CreatedOn) ";
                        //    strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "','"
                        //        + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + Convert.ToInt32(CountryID_list[index]) + "," + parentStepId + ","
                        //        + newCount + ",'A'," + sess.LoginId + ",getdate())";
                        //    stepId = objData.ExecuteWithScope(strQuery);
                        //} 
                        //catch (Exception Ex)
                        //{
                        //    string error = Ex.Message;
                        //    tdMsg.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        //}



                        //string selQuery2 = "SELECT DSTempSetId FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
                        //objCount = objData.FetchValue(selQuery2);


                        UpdateSortOrder(Convert.ToInt32(CountryID_list[index]), headerId, selectedText);
                        try
                        {
                            strQuery = "Insert Into DSTempStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,DSTempParentStepId,SortOrder,ActiveInd,CreatedBy,CreatedOn) ";
                            strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "','"
                                + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + Convert.ToInt32(CountryID_list[index]) + "," + parentStepId + ","
                                + selectedText + ",'A'," + sess.LoginId + ",getdate())";
                            stepId = objData.ExecuteWithScope(strQuery);
                        }

                        catch (Exception Ex)
                        {
                            string error = Ex.Message;
                            tdMsg.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }



                        //-----------------------------------------




                    }

                    btnAddStepDetails.Visible = true;
                    BtnUpdateStep.Visible = false;

                    chkEnd.Enabled = true;
                    chkEnd.Checked = true;
                    ddlSortOrder.Enabled = false;

                    lblAddorUpdateStep.Text = "Add Step";
                    ClearStepData();

                    // ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseStepPopup();", true);
                    GetStepData(headerId);
                }
                else
                {
                    //------------------------------------

                    //string selQuerry = "SELECT ISNULL(MAX(SortOrder),0) FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A' AND IsDynamic=0 AND DSTempSetId = " + Convert.ToInt32(parentSetId);
                    //objCount = objData.FetchValue(selQuerry);
                    //if (objCount != null)
                    //{
                    //    findCount = Convert.ToInt32(objCount);
                    //}

                    //newCount = findCount + 1;

                    try
                    {
                        strQuery = "Insert Into DSTempStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,DSTempParentStepId,SortOrder,ActiveInd,CreatedBy,CreatedOn) ";
                        strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + Convert.ToInt32(parentSetId) + "," + parentStepId + "," + selectedText + ",'A'," + sess.LoginId + ",getdate())";
                        stepId = objData.ExecuteWithScope(strQuery);
                        ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseStepPopup();", true);
                        GetStepData(headerId);

                    }
                    //--------------------------------



                    catch (Exception Ex)
                    {
                        string error = Ex.Message;
                        tdMsg.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                    }

                }
                ClearStepData();
            }
            showMatchToSampleDrop();
            ddchkCountry.Focus();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void BtnAddSetDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        string insertQuerry = "";
        string criteriaType = "";
        int ioaReq = 0;
        int multiTeach = 0;
        int consctveSess = 0;
        int consctveAvg = 0;  //--- [New Criteria] May 2020 ---//
        int columnValue = 0;
        int measureValue = 0;
        string requiredScore = "";
        string totalInstance = "";
        string totalcorrectInstance = "";
        string numbrSessions = "";
        int isNAChk = 0;        
        sess = (clsSession)Session["UserSession"];

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ValidateCheck() == true)
        {

            try
            {
                if (sess != null)
                {
                    if (rbtnIoaReq.SelectedValue == "1") ioaReq = 1;
                    else ioaReq = 0;
                    if (rbtnMultitchr.SelectedValue == "1") multiTeach = 1;
                    else multiTeach = 0;
                    if (rbtnConsectiveSes.SelectedValue == "1") consctveSess = 1;
                    else consctveSess = 0;
                    //--- [New Criteria] May 2020 - (Start) ---//
                    if (rbtnConsectiveAvg.SelectedValue == "1") consctveAvg = 1;
                    else consctveAvg = 0;
                    //--- [New Criteria] May 2020 - (End) ---//
                    if (ddlCriteriaType.SelectedIndex != 0) criteriaType = ddlCriteriaType.SelectedValue.ToString();
                    if (ddlTempColumn.SelectedIndex != 0)
                        try
                        {
                            columnValue = Convert.ToInt32(ddlTempColumn.SelectedValue);
                        }
                        catch
                        {
                            columnValue = 0;
                        }
                    if (ddlTempMeasure.SelectedIndex != 0)
                        try
                        {
                            measureValue = Convert.ToInt32(ddlTempMeasure.SelectedValue);
                        }
                        catch
                        {
                            measureValue = 0;
                        }
                    requiredScore = clsGeneral.convertQuotes(txtRequiredScore.Text.Trim());
                    numbrSessions = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());
                    if (consctveSess == 1 || consctveAvg == 1) totalInstance = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());  //--- [New Criteria] May 2020 ---//
                    else
                    {
                        totalInstance = clsGeneral.convertQuotes(txtIns2.Text.Trim());
                        totalcorrectInstance = clsGeneral.convertQuotes(txtIns1.Text.Trim());
                    }

                    if (requiredScore == "") requiredScore = "0";
                    if (chkNACriteria.Checked == true)
                    {
                        isNAChk = 1;
                    }
                    insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                                + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'SET','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                    int index = objData.Execute(insertQuerry);

                    if (chkCpyStepCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'STEP','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ")  ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    if (chkCpyPromptCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'PROMPT','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ")  ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);
                    GetSetCriteriaData(headerId);
                    GetStepCriteriaData(headerId);
                    GetPromptCriteriaData(headerId);
                }
                showMatchToSampleDrop();
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                throw Ex;
            }
        }
    }


    protected bool ValidateCheck()
    {                

        if (ddlCriteriaType.SelectedIndex != 3)
        {
            objData = new clsData();
            if (ddlCriteriaType.SelectedValue == "0")
            {                
                tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Select Criteria Type");
                ddlCriteriaType.Focus();
                return false;
            }
            if (ddlTempColumn.SelectedValue == "0" || ddlTempColumn.SelectedValue == "")
            {                
                tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Select Column Type");
                ddlTempColumn.Focus();
                return false;
            }
            if (ddlTempMeasure.SelectedValue == "0" || ddlTempMeasure.SelectedValue == "")
            {                
                tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Select Measure Type");
                ddlTempMeasure.Focus();
                return false;
            }
            if (!chkNACriteria.Checked)
            {
                if (txtRequiredScore.Text == "")
                {                    
                    tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Enter Required Score");
                    txtRequiredScore.Focus();
                    return false;
                }
                if ((rbtnConsectiveSes.SelectedValue == "0") && (rbtnConsectiveAvg.SelectedValue == "0") /*//--- [New Criteria] May 2020 ---//*/)
                {
                    if (txtIns2.Text == "")
                    {                        
                        tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Enter Total  Instance Value");
                        txtIns2.Focus();
                        return false;
                    }
                    if (txtIns1.Text == "")
                    {                        
                        tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Enter Total Correct Instance Value");
                        txtIns1.Focus();
                        return false;
                    }
                }
                else if (rbtnConsectiveSes.SelectedValue == "1" || rbtnConsectiveAvg.SelectedValue == "1" /*//--- [New Criteria] May 2020 ---//*/)
                {
                    if (txtNumbrSessions.Text == "")
                    {                        
                        tdMsgCriteria.InnerHtml = clsGeneral.warningMsg("Please Define No of Sessions");
                        txtNumbrSessions.Focus();
                        return false;

                    }
                }
            }
        }

        return true;
    }





    protected void BtnAddStepDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        string insertQuerry = "";
        string criteriaType = "";
        int ioaReq = 0;
        int multiTeach = 0;
        int consctveSess = 0;
        int consctveAvg = 0;  //--- [New Criteria] May 2020 ---//
        int columnValue = 0;
        int measureValue = 0;
        string requiredScore = "";
        string totalInstance = "";
        string totalcorrectInstance = "";
        string numbrSessions = "";
        int isNAChk = 0;        
        sess = (clsSession)Session["UserSession"];

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ValidateCheck() == true)
        {

            try
            {
                if (sess != null)
                {


                    if (rbtnIoaReq.SelectedValue == "1") ioaReq = 1;
                    else ioaReq = 0;
                    if (rbtnMultitchr.SelectedValue == "1") multiTeach = 1;
                    else multiTeach = 0;
                    if (rbtnConsectiveSes.SelectedValue == "1") consctveSess = 1;
                    else consctveSess = 0;
                    //--- [New Criteria] May 2020 - (Start) ---//
                    if (rbtnConsectiveAvg.SelectedValue == "1") consctveAvg = 1;
                    else consctveAvg = 0;
                    //--- [New Criteria] May 2020 - (End) ---//
                    if (ddlCriteriaType.SelectedIndex != 0) criteriaType = ddlCriteriaType.SelectedValue.ToString();
                    if (ddlTempColumn.SelectedIndex != 0)
                        try
                        {
                            columnValue = Convert.ToInt32(ddlTempColumn.SelectedValue);
                        }
                        catch
                        {
                            columnValue = 0;
                        }
                    if (ddlTempMeasure.SelectedIndex != 0)
                        try
                        {
                            measureValue = Convert.ToInt32(ddlTempMeasure.SelectedValue);
                        }
                        catch
                        {
                            measureValue = 0;
                        }
                    requiredScore = clsGeneral.convertQuotes(txtRequiredScore.Text.Trim());
                    numbrSessions = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());
                    if (consctveSess == 1 || consctveAvg == 1) totalInstance = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());  //--- [New Criteria] May 2020 ---//
                    else
                    {
                        totalInstance = clsGeneral.convertQuotes(txtIns2.Text.Trim());
                        totalcorrectInstance = clsGeneral.convertQuotes(txtIns1.Text.Trim());
                    }
                    if (requiredScore == "") requiredScore = "0";
                    if (chkNACriteria.Checked == true)
                    {
                        isNAChk = 1;
                    }                    
                    insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                                + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'STEP','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                    int index = objData.Execute(insertQuerry);

                    if (chkCpySetCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'SET','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    if (chkCpyPromptCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'PROMPT','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);                    
                    GetStepCriteriaData(headerId);
                    GetSetCriteriaData(headerId);
                    GetPromptCriteriaData(headerId);
                }
                showMatchToSampleDrop();

            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                throw Ex;
            }
        }
    }

    protected void BtnAddPromptDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        string insertQuerry = "";
        string criteriaType = "";
        int ioaReq = 0;
        int multiTeach = 0;
        int consctveSess = 0;
        int consctveAvg = 0;  //--- [New Criteria] May 2020 ---//
        int columnValue = 0;
        int measureValue = 0;
        string requiredScore = "";
        string totalInstance = "";
        string totalcorrectInstance = "";
        string numbrSessions = "";
        int isNAChk = 0;                
        sess = (clsSession)Session["UserSession"];

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ValidateCheck() == true)
        {

            try
            {
                if (sess != null)
                {
                    if (rbtnIoaReq.SelectedValue == "1") ioaReq = 1;
                    else ioaReq = 0;
                    if (rbtnMultitchr.SelectedValue == "1") multiTeach = 1;
                    else multiTeach = 0;
                    if (rbtnConsectiveSes.SelectedValue == "1") consctveSess = 1;
                    else consctveSess = 0;
                    //--- [New Criteria] May 2020 - (Start) ---//
                    if (rbtnConsectiveAvg.SelectedValue == "1") consctveAvg = 1;
                    else consctveAvg = 0;
                    //--- [New Criteria] May 2020 - (End) ---//
                    if (ddlCriteriaType.SelectedIndex != 0) criteriaType = ddlCriteriaType.SelectedValue.ToString();
                    if (ddlTempColumn.SelectedIndex != 0)
                        try
                        {
                            columnValue = Convert.ToInt32(ddlTempColumn.SelectedValue);
                        }
                        catch
                        {
                            columnValue = 0;
                        }
                    if (ddlTempMeasure.SelectedIndex != 0)
                        try
                        {
                            measureValue = Convert.ToInt32(ddlTempMeasure.SelectedValue);
                        }
                        catch
                        {
                            measureValue = 0;
                        }
                    requiredScore = clsGeneral.convertQuotes(txtRequiredScore.Text.Trim());
                    numbrSessions = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());
                    if (consctveSess == 1 || consctveAvg == 1) totalInstance = clsGeneral.convertQuotes(txtNumbrSessions.Text.Trim());  //--- [New Criteria] May 2020 ---//
                    else
                    {
                        totalInstance = clsGeneral.convertQuotes(txtIns2.Text.Trim());
                        totalcorrectInstance = clsGeneral.convertQuotes(txtIns1.Text.Trim());
                    }
                    if (requiredScore == "") requiredScore = "0";
                    if (chkNACriteria.Checked == true)
                    {
                        isNAChk = 1;
                    }
                    insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                                + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'PROMPT','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                    int index = objData.Execute(insertQuerry);

                    if (chkCpySetCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'SET','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    if (chkCpyStepCri.Checked == true)
                    {
                        insertQuerry = "Insert Into DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,IOAReqInd,MultiTeacherReqInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IsComment,ModificationComment,ModificationRule,IsNA,ConsequetiveAvgInd) " //--- [New Criteria] May 2020 ---//
                            + "Values(" + headerId + "," + sess.SchoolId + "," + columnValue + "," + measureValue + ",'STEP','" + criteriaType + "'  ," + requiredScore + ",'" + totalInstance + "','" + totalcorrectInstance + "', " + consctveSess + "," + ioaReq + "," + multiTeach + ", " + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + IsComments.Value + ",'" + txtModComments.Text + "','" + txtModNo.Text + "'," + isNAChk + "," + consctveAvg + ") ;"; //--- [New Criteria] May 2020 ---//
                        objData.Execute(insertQuerry);
                    }

                    ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);
                    GetPromptCriteriaData(headerId);
                    GetSetCriteriaData(headerId);
                    GetStepCriteriaData(headerId);
                }
                showMatchToSampleDrop();
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                throw Ex;
            }
        }

    }

    protected void btnEditSet_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        try
        {
            Button btnEdit = (Button)sender;
            int setId = Convert.ToInt32(btnEdit.CommandArgument);
            ViewState["EditSetId"] = setId;
            if (Hdfsavemeasure.Value != "")
            {
                viewSetData();
                
            }
            else
            {
                BtnUpdateSetDetails.Visible = true;
                ClearSetData();
            }
            btnAddSetDetails.Visible = false;
            lblAddOrUpdateSet.Text = "Update Set";
            EditSetData(setId);
            txtBoxAddSet.Focus();
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddSet();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    protected void viewSetData()
    {
        lstMatchSamples.Items.Clear();
        BtnUpdateSetDetails.Visible = false;
        txtBoxAddSet.ReadOnly = true;
        txtSetDescription.ReadOnly = true;
        txtMatcSamples.ReadOnly = true;
        lstMatchSamples.Enabled = false;
        Disitem.Enabled = false;
        chk_distractor.Enabled = false;        
    }

    protected void btnEditStep_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        try
        {
            Button btnEditStep = (Button)sender;
            int stepId = Convert.ToInt32(btnEditStep.CommandArgument);
            ViewState["EditStepId"] = stepId;
            chkEnd.Checked = false;
            chkEnd.Enabled = false;
            ddlSortOrder.Enabled = true;
            if (Hdfsavemeasure.Value != "")
                viewStepdata();
            else
            {
                BtnUpdateStep.Visible = true;
                ClearStepData();
            }
            btnAddStepDetails.Visible = false;
            chkEnd.Enabled = false;
            chkEnd.Checked = false;
            ddlSortOrder.Enabled = true;

            lblAddorUpdateStep.Text = "Update Step";
            
            FillSetDrpList();
            EditStepData(stepId);
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddStep();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    protected void viewStepdata()
    {
        BtnUpdateStep.Visible = false;
        ddchkCountry.Enabled = false;
        txtStepName.ReadOnly = true;
        txtStepDesc.ReadOnly = true;
    }

    protected void BtnEditSetCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        try
        {
            Button btnEditCriteria = (Button)sender;
            int criteriaId = Convert.ToInt32(btnEditCriteria.CommandArgument);
            ViewState["EditSetCriteria"] = criteriaId;
            BtnAddSetDCriteria.Visible = false;
            BtnAddStepDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = false;
            BtnUpdateStepDCriteria.Visible = false;
            BtnUpdatePromptDCriteria.Visible = false;            
            if (Hdfsavemeasure.Value != "")
                viewCriteria();
            else
            {
                BtnUpdateSetDCriteria.Visible = true;
                ClearCriteriaData();
            }
            lblCopyTo.Visible = false;
            chkCpySetCri.Visible = false;
            chkCpyStepCri.Visible = false;
            chkCpyPromptCri.Visible = false;
            lblCpySetCri.Visible = false;
            lblCpyStepCri.Visible = false;
            lblCpyPromptCri.Visible = false;
            
            FillDropMeasure();
            EditSetCriteria(criteriaId);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void viewCriteria()
    {
        BtnUpdateSetDCriteria.Visible = false;
        ddlCriteriaType.Enabled = false;
        rbtnIoaReq.Enabled = false;
        ddlTempColumn.Enabled = false;
        ddlTempMeasure.Enabled = false;
        txtRequiredScore.ReadOnly = true;
        chkNACriteria.Enabled = false;
        rbtnMultitchr.Enabled = false;
        rbtnConsectiveSes.Enabled = false;
        rbtnConsectiveAvg.Enabled = false;
        txtNumbrSessions.ReadOnly = true;
        txtIns1.ReadOnly = true;
        txtIns2.ReadOnly = true;

    }
    protected void BtnEditStepCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        try
        {
            Button btnEditCriteria = (Button)sender;
            int criteriaId = Convert.ToInt32(btnEditCriteria.CommandArgument);
            ViewState["EditStepCriteria"] = criteriaId;
            BtnAddSetDCriteria.Visible = false;
            BtnAddStepDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = false;            
            if (Hdfsavemeasure.Value != "")
            {
                BtnUpdateStepDCriteria.Visible = false;
                viewCriteria();
            }
            else
            {
                BtnUpdateStepDCriteria.Visible = true;
                ClearCriteriaData();
            }
            BtnUpdatePromptDCriteria.Visible = false;
            BtnUpdateSetDCriteria.Visible = false;

            lblCopyTo.Visible = false;
            chkCpySetCri.Visible = false;
            chkCpyStepCri.Visible = false;
            chkCpyPromptCri.Visible = false;
            lblCpySetCri.Visible = false;
            lblCpyStepCri.Visible = false;
            lblCpyPromptCri.Visible = false;

            
            FillDropMeasure();
            EditSetCriteria(criteriaId);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void BtnEditPromptCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        try
        {
            Button btnEditCriteria = (Button)sender;
            int criteriaId = Convert.ToInt32(btnEditCriteria.CommandArgument);
            ViewState["EditPromptCriteria"] = criteriaId;
            BtnAddSetDCriteria.Visible = false;
            BtnAddStepDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = false;
            BtnUpdateStepDCriteria.Visible = false;
            BtnUpdateSetDCriteria.Visible = false;            
            if (Hdfsavemeasure.Value != "")
            {
                BtnUpdatePromptDCriteria.Visible = false;
                viewCriteria();
            }
            else
            {
                BtnUpdatePromptDCriteria.Visible = true;
                ClearCriteriaData();
            }
            lblCopyTo.Visible = false;
            chkCpySetCri.Visible = false;
            chkCpyStepCri.Visible = false;
            chkCpyPromptCri.Visible = false;
            lblCpySetCri.Visible = false;
            lblCpyStepCri.Visible = false;
            lblCpyPromptCri.Visible = false;

            
            FillDropMeasure();
            EditSetCriteria(criteriaId);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }






    protected void EditSetData(int setId)
    {
        objData = new clsData();
        string matchData = "";
        string itemData = "";
        string distarctormatchData = "";
        string disitemData = "";
        //string[] arryMatchValue = new string[100];
        try
        {
            string selQuerry = "SELECT SetCd,SetName,Samples,DistractorSamples FROm DSTempSet WHERE DSTempSetId = " + setId;
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    txtBoxAddSet.Text = dtList.Rows[0]["SetCd"].ToString();
                    txtSetDescription.Text = dtList.Rows[0]["SetName"].ToString();
                    matchData = dtList.Rows[0]["Samples"].ToString();
                    distarctormatchData = dtList.Rows[0]["DistractorSamples"].ToString();
                    if (matchData != null)
                    {
                        string[] arryMatchValue = new string[matchData.Length];
                        arryMatchValue = matchData.Split(',');

                        for (int i = 0; i < arryMatchValue.Length - 1; i++)
                        {

                            itemData = arryMatchValue[i].ToString();
                            ListItem item = new ListItem();
                            item.Text = itemData;
                            item.Value = itemData;
                            lstMatchSamples.Items.Add(item);
                        }

                    }

                    if (distarctormatchData != null)
                    {
                        string[] DistractorarryMatchValue = new string[distarctormatchData.Length];
                        DistractorarryMatchValue = distarctormatchData.Split(',');

                        for (int i = 0; i < DistractorarryMatchValue.Length - 1; i++)
                        {

                            disitemData = DistractorarryMatchValue[i].ToString();
                            ListItem item2 = new ListItem();
                            item2.Text = disitemData;
                            item2.Value = disitemData;
                            Disitem.Items.Add(item2);
                        }

                    }

                    //  txtMatcSamples.Text = dtList.Rows[0]["Samples"].ToString();


                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void EditStepData(int stepId)
    {
        objData = new clsData();
        string[] tempIds;
        string setParentId = "";
        lblSelParentSets.Text = "";
        int flag = 0;
        List<String> CountryID_list = new List<string>();
        List<String> CountryName_list = new List<string>();
        try
        {
            string selQuerry = "SELECT DSTempSetId,StepCd,StepName,SetIds,SortOrder FROM DSTempParentStep WHERE ActiveInd='A' AND DSTempParentStepId = " + stepId;
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    txtStepName.Text = dtList.Rows[0]["StepCd"].ToString();
                    txtStepDesc.Text = dtList.Rows[0]["StepName"].ToString();

                    ddlSortOrder.SelectedValue = dtList.Rows[0]["SortOrder"].ToString();

                    setParentId = dtList.Rows[0]["SetIds"].ToString();
                    if (setParentId != "")
                    {
                        tempIds = setParentId.Split(',');
                        foreach (var items in tempIds)
                        {
                            foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
                            {
                                if (item.Value == items)
                                {
                                    item.Selected = true;
                                }
                            }
                        }
                    }
                    foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
                    {

                        if (item.Selected == true)
                        {
                            string text = "";
                            if (item.Text != "")
                            {
                                if (item.Text.ToString().Length > 80)
                                {
                                    text = item.Text.ToString().Substring(0, 80) + "........";
                                }
                                else
                                {
                                    text = item.Text;
                                }
                            }

                            lblSelParentSets.Text += "- " + text + "</br>";
                            flag++;
                        }
                    }
                    if (flag == ddchkCountry.Items.Count)
                    {
                        lblSelParentSets.Text = "All Sets";
                    }
                    else if (flag == 0)
                    {
                        lblSelParentSets.Text = "Not Assigned to any Sets";
                    }
                    flag = 0;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void EditSetCriteria(int criteriaId)
    {
        objData = new clsData();
        string criteriaType = "";
        int scoreReq = 0;
        int totalInstance = 0;
        int totalCorrInstance = 0;
        bool consecutveSess = false;
        bool consecutveAvg = false; //--- [New Criteria] May 2020 ---//
        bool isMultitchr = false;
        bool isIoReq = false;
        string colName = "";
        string measureType = "";
        string selQuerry = "";
        int columnId = 0;
        int measureId = 0;
        string ruleType = "";
        //  string cmpleteData = "";         

        try
        {

            selQuerry = "SELECT DsRule.IsComment,DsRule.ModificationComment,DsRule.ModificationRule, DsRule.DSTempRuleId,DsRule.DSTempSetColId,DsRule.DSTempSetColCalcId,DsRule.RuleType,DsRule.CriteriaType,DsRule.ScoreReq,DsRule.TotalInstance, " +
                               "  DsRule.TotCorrInstance,DsRule.ConsequetiveInd,ISNULL(DsRule.ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,DsRule.MultiTeacherReqInd,DsRule.IOAReqInd,DsCol.ColName,DsColCalc.CalcType,DsRule.IsNA FROM DSTempRule DsRule LEFT JOIN DSTempSetCol DsCol " + //--- [New Criteria] May 2020 ---//
                               "  ON DsRule.DSTempSetColId = DsCol.DSTempSetColId LEFT JOIN DSTempSetColCalc DsColCalc ON DsRule.DSTempSetColCalcId = DsColCalc.DSTempSetColCalcId WHERE DSTempRuleId = " + criteriaId;

            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    criteriaType = dtList.Rows[0]["CriteriaType"].ToString();
                    scoreReq = Convert.ToInt32(dtList.Rows[0]["ScoreReq"]);
                    totalInstance = Convert.ToInt32(dtList.Rows[0]["TotalInstance"]);
                    totalCorrInstance = Convert.ToInt32(dtList.Rows[0]["TotCorrInstance"]);
                    consecutveSess = Convert.ToBoolean(dtList.Rows[0]["ConsequetiveInd"]);
                    consecutveAvg = Convert.ToBoolean(dtList.Rows[0]["ConsequetiveAvgInd"]); //--- [New Criteria] May 2020 ---//
                    isMultitchr = Convert.ToBoolean(dtList.Rows[0]["MultiTeacherReqInd"]);
                    isIoReq = Convert.ToBoolean(dtList.Rows[0]["IOAReqInd"]);
                    colName = dtList.Rows[0]["ColName"].ToString();
                    measureType = dtList.Rows[0]["CalcType"].ToString();
                    columnId = Convert.ToInt32(dtList.Rows[0]["DSTempSetColId"]);
                    measureId = Convert.ToInt32(dtList.Rows[0]["DSTempSetColCalcId"]);
                    ruleType = dtList.Rows[0]["RuleType"].ToString();
                    if (Convert.ToBoolean(dtList.Rows[0]["IsNA"]) == true)
                    {
                        chkNACriteria.Checked = true;
                    }
                    else
                        chkNACriteria.Checked = false;
                    bool IsComment = false;
                    if (dtList.Rows[0]["IsComment"] != "") IsComment = Convert.ToBoolean(dtList.Rows[0]["IsComment"]);
                    if (IsComment == true)
                    {
                        IsComments.Value = "1";
                        chkIsComments.Checked = true;
                        chkScore.Checked = false;
                        txtModNo.Text = "";
                        txtModComments.Text = dtList.Rows[0]["ModificationComment"].ToString();
                        txtModComments.Enabled = true;

                    }
                    else
                    {
                        IsComments.Value = "0";
                        chkIsComments.Checked = false;
                        chkScore.Checked = true;
                        txtModComments.Text = "";
                        txtModNo.Text = dtList.Rows[0]["ModificationRule"].ToString();
                        txtModNo.Enabled = true;

                    }


                    if (criteriaType == "MODIFICATION")
                    {
                        setModificationVisibility(false);
                        trModification.Style.Add("visibility", "visible");
                        trModification1.Style.Add("visibility", "visible");
                    }
                    else
                    {
                        setModificationVisibility(true);
                        trModification.Style.Add("visibility", "hidden");
                        trModification1.Style.Add("visibility", "hidden");
                    }


                    if (ruleType == "SET")
                    {
                        lblCriteriaName.Text = "Add Set";
                    }
                    else if (ruleType == "STEP")
                    {
                        lblCriteriaName.Text = "Add Step";
                    }
                    else if (ruleType == "PROMPT")
                    {
                        lblCriteriaName.Text = "Add Prompt";
                    }
                    try
                    {
                        ddlCriteriaType.SelectedValue = criteriaType;
                    }
                    catch
                    {
                        ddlCriteriaType.SelectedValue = "0";
                    }

                    if (isIoReq == true) rbtnIoaReq.SelectedValue = "1";
                    else rbtnIoaReq.SelectedValue = "0";
                    if (isMultitchr == true) rbtnMultitchr.SelectedValue = "1";
                    else rbtnMultitchr.SelectedValue = "0";
                    if (consecutveSess == true) rbtnConsectiveSes.SelectedValue = "1";
                    else rbtnConsectiveSes.SelectedValue = "0";
                    if (consecutveAvg == true) rbtnConsectiveAvg.SelectedValue = "1"; //--- [New Criteria] May 2020 ---//
                    else rbtnConsectiveAvg.SelectedValue = "0"; //--- [New Criteria] May 2020 ---//
                    try
                    {
                        ddlTempColumn.SelectedValue = columnId.ToString();
                    }
                    catch
                    {
                        ddlTempColumn.SelectedIndex = 0;
                    }
                    FillMeasureDrop(columnId);
                    try
                    {
                        ddlTempMeasure.SelectedValue = measureId.ToString();
                    }
                    catch
                    {
                        ddlTempMeasure.SelectedIndex = 0;
                    }
                    if (Convert.ToBoolean(dtList.Rows[0]["IsNA"]) == true)
                    {
                        txtRequiredScore.Text = scoreReq == 0 ? "" : scoreReq.ToString();
                    }
                    else
                        txtRequiredScore.Text = scoreReq.ToString();

                    if (consecutveSess == true || consecutveAvg == true) //--- [New Criteria] May 2020 ---//
                    {
                        txtNumbrSessions.Text = totalInstance == 0 ? "" : totalInstance.ToString();
                    }
                    else
                    {
                        txtIns2.Text = totalInstance == 0 ? "" : totalInstance.ToString();
                        txtIns1.Text = totalCorrInstance == 0 ? "" : totalCorrInstance.ToString();
                    }


                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void FillMeasureDrop(int columnId)
    {
        objData = new clsData();
        try
        {
            string selQuerry = " SELECT DSTempSetColCalcId As Id, CalcType As Name  FROM DSTempSetColCalc WHERE  DSTempSetColId = " + columnId;/*CalcType <> '%Accuracy at Training Step' AND CalcType <>'%Accuracy at Previously Learned Steps' AND */
            //string selQuerry = " SELECT DSTempSetColCalcId As Id, CalcType As Name  FROM DSTempSetColCalc WHERE  DSTempSetColId = '" + columnId + "' and CalcType != '%Accuracy at Training Step' and CalcType ! = '%Accuracy at Previously Learned Steps'";
            objData.ReturnDropDownForMeasureCriteria(selQuerry, ddlTempMeasure);
            if (ddlTempMeasure.Items.Count == 0)
            {
                ddlTempMeasure.Items.Insert(0, new ListItem("------------Select Column------------", "0"));
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void btnAddSet_Click(object sender, EventArgs e)
    {
        ClearSetData();
        BtnUpdateSetDetails.Visible = false;
        btnAddSetDetails.Visible = true;
        lblAddOrUpdateSet.Text = "Add Set";
        txtBoxAddSet.Focus();
        //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddSet();", true);
        drpTasklist_SelectedIndexChanged1(sender, e);
    }





    protected void BtnAddStep_Click(object sender, EventArgs e)
    {
        try
        {
            lblSelParentSets.Text = "Not Assigned to any Sets";
            ClearStepData();
            FillSetDrpList();
            BtnUpdateStep.Visible = false;
            chkEnd.Enabled = true;
            chkEnd.Checked = true;
            ddlSortOrder.Enabled = false;
            btnAddStepDetails.Visible = true;
            lblAddorUpdateStep.Text = "Add Step";
            ddchkCountry.Focus();
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddStep();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }




    protected void BtnAddSort_Click(object sender, EventArgs e)
    {
        try
        {


            // ClearStepData();
            FillSetSortDrpList();
            dlistStepSorting.Visible = false;
            BtnUpdateSortorder.Visible = false;
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddSort();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }




    protected void FillSetSortDrpList()
    {
        objData = new clsData();
        string strQuerry = "";
        int headerId = 0;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {


            strQuerry = "SELECT DSTempSetId AS ID, SetCd As Name FROM DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            objData.ReturnDropDownForSortParentSet(strQuerry, ddlparentSetlist);
            // DataTable dttemp = objData.ReturnDataTable(strQuerry, false);
            //if ((dttemp != null) && (dttemp.Rows.Count == 0))
            //{
            //    ddlparentSetlist.DataSource = dttemp;
            //    ddlparentSetlist.DataBind();              

            //}

            if (ddlparentSetlist.Items.Count == 0)
            {
                ddlparentSetlist.Items.Insert(0, new ListItem(".............................SELECT...........................", "0"));
                //lstSetData.Items.Insert(0, new ListItem("No Set Created", "0"));
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void ddlparentSetlist_SelectedIndexChanged(object sender, EventArgs e)
    {
        objData = new clsData();
        string strQuerry = "";
        int setID = 0;
        if (ddlparentSetlist.SelectedValue != "0")
        {


            setID = Convert.ToInt32(ddlparentSetlist.SelectedValue);
            ViewState["currentParentSetId"] = setID;

            dlistStepSorting.Visible = true;
            BtnUpdateSortorder.Visible = true;

            DisplayStepData(setID);


        }
        else
        {
            dlistStepSorting.Visible = false;
            BtnUpdateSortorder.Visible = false;

            //ddlStepList.Items.Clear();
            //ddlStepList.Items.Insert(0, new ListItem(".............................SELECT...........................", "0"));
        }
    }


    protected void DisplayStepData(int setId)
    {
        objData = new clsData();
        string selQuerry = "";
        try
        {

            selQuerry = "SELECT DSTempStepId ,StepCd,StepName FROM DSTempStep WHERE DSTempSetId = " + setId + " AND ActiveInd='A' AND IsDynamic=0";
            DataTable dtList = objData.ReturnDataTable(selQuerry, false);
            if (dtList != null)
            {
                if (dtList.Rows.Count > 0)
                {
                    dlistStepSorting.DataSource = dtList;
                    dlistStepSorting.DataBind();
                    BtnUpdateSortorder.Visible = true;
                }
                else
                {
                    dlistStepSorting.DataSource = dtList;
                    dlistStepSorting.DataBind();
                    BtnUpdateSortorder.Visible = false;
                }
            }
            else BtnUpdateSortorder.Visible = false;

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void dlistStepSorting_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        int currentParentSetId = 0;
        int currentStepId = 0;
        string strQuerry = "";
        int count = 0;
        int countValue = 0;
        int currentSortOrder = 0;
        DropDownList drpSort = (DropDownList)e.Item.FindControl("ddlSortingstep");
        HiddenField hidenStepId = (HiddenField)e.Item.FindControl("hdnStepID");
        Label lblStepName = (Label)e.Item.FindControl("lblStepName");
        Label lblStepDesc = (Label)e.Item.FindControl("lblStepDesc");
        currentParentSetId = Convert.ToInt32(ViewState["currentParentSetId"]);

        currentStepId = Convert.ToInt32(hidenStepId.Value);

        strQuerry = "SELECT COUNT(DSTempStepId) FROM DSTempStep WHERE DSTempSetId = " + currentParentSetId + " AND ActiveInd='A' AND IsDynamic=0";
        countValue = Convert.ToInt32(objData.FetchValue(strQuerry));


        strQuerry = "SELECT SortOrder FROM DSTempStep WHERE DSTempStepId = " + currentStepId + " AND IsDynamic=0 ";
        currentSortOrder = Convert.ToInt32(objData.FetchValue(strQuerry));
        if (lblStepName.Text != "")
        {
            if (lblStepName.Text.ToString().Length > 30)
            {
                lblStepName.Text = lblStepName.Text.ToString().Substring(0, 30) + "........";
            }
        }

        if (lblStepDesc.Text.ToString() != "")
        {
            if (lblStepDesc.Text.ToString().Length > 40)
            {
                lblStepDesc.Text = lblStepDesc.Text.ToString().Substring(0, 40) + "........";
            }
        }
        for (int i = 0; i < countValue; i++)
        {
            count++;
            drpSort.Items.Add(new ListItem { Text = count.ToString(), Value = count.ToString() });
        }
        drpSort.SelectedValue = currentSortOrder.ToString();




        //Label lblDesc = (Label)e.Item.FindControl("lblSetDesc");
        //Button BtnEdit = (Button)e.Item.FindControl("btnEditSet");
        //Button btnDelt = (Button)e.Item.FindControl("btnRemoveSet");
        //int headerId = 0;
        //if (ViewState["HeaderId"] != null)
        //{
        //    headerId = Convert.ToInt32(ViewState["HeaderId"]);
        //}

        //try
        //{

        //    if (lblDesc.Text.ToString() != "")
        //    {
        //        if (lblDesc.Text.ToString().Length > 100)
        //        {
        //            lblDesc.Text = lblDesc.Text.ToString().Substring(0, 100) + "........";
        //        }
        //    }



        //}
        //catch (Exception Ex)
        //{
        //    throw Ex;
        //}
    }
    private bool HasDuplicates(string[] arrayList)
    {
        List<string> vals = new List<string>();
        bool returnValue = false;
        foreach (string s in arrayList)
        {
            if (vals.Contains(s))
            {
                returnValue = true;
                break;
            }
            vals.Add(s);
        }


        return returnValue;
    }

    protected void BtnUpdateSortorder_Click(object sender, EventArgs e)
    {
        int currentsortId = 0;
        string strQuerry = "";
        objData = new clsData();
        int setId = 0;
        int stepId = 0;

        setId = Convert.ToInt32(ViewState["currentParentSetId"]);
        strQuerry = "SELECT COUNT(DSTempStepId) FROM DSTempStep WHERE DSTempSetId = " + setId + " AND ActiveInd='A' AND IsDynamic=0";
        int count = Convert.ToInt32(objData.FetchValue(strQuerry));


        int i = 0;
        string[] ar = new string[count];





        foreach (DataListItem diGoal in dlistStepSorting.Items)
        {
            DropDownList drpSortId = (DropDownList)diGoal.FindControl("ddlSortingstep");
            stepId = Convert.ToInt32(drpSortId.SelectedValue);
            ar[i] = stepId.ToString();
            i++;
        }


        if (HasDuplicates(ar) == false)
        {
            foreach (DataListItem diGoal in dlistStepSorting.Items)
            {
                HiddenField currentStepId = (HiddenField)diGoal.FindControl("hdnStepID");
                DropDownList drpSortId = (DropDownList)diGoal.FindControl("ddlSortingstep");
                stepId = Convert.ToInt32(currentStepId.Value);
                currentsortId = Convert.ToInt32(drpSortId.SelectedValue);
                strQuerry = "Update DSTempStep Set SortOrder=" + currentsortId + " Where DSTempStepId =" + stepId + " And DSTempSetId=" + setId + " AND IsDynamic=0";
                objData.Execute(strQuerry);

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "CloseSortingPopup();", true);
            }
        }
        else
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "alert('Duplicates in Sort order');", true);
            DisplayStepData(setId);
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }



    protected void btnAddSetCriteria_Click(object sender, EventArgs e)
    {
        try
        {
            if (Hdfsavemeasure.Value != "")
                BtnAddSetDCriteria.Visible = false;
            else
                BtnAddSetDCriteria.Visible = true;

            Session["measuretype"] = "set";
            ClearCriteriaData();
            FillDropMeasure();                          //Fill dropdown for measure in criteria
            CheckSetCopyCriteria();                      //show the set/step/prompt copy criteria button
            lblCopyTo.Visible = true;
            BtnAddStepDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = false;
            BtnUpdateSetDCriteria.Visible = false;
            BtnUpdateStepDCriteria.Visible = false;
            BtnUpdatePromptDCriteria.Visible = false;
            lblCriteriaName.Text = "Set Criteria";
            setModificationVisibility(true);
            trModification.Style.Add("visibility", "hidden");
            trModification1.Style.Add("visibility", "hidden");
            chkNACriteria.Visible = false;
            ApplyAllDisableVisual();            

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void ApplyAllDisableVisual()
    {
        int headerId = 0;
        objData = new clsData();
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        string selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
        object objVt = objData.FetchValue(selctQuerry);

        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                chkCpyStepCri.Enabled = false;
                chkCpyPromptCri.Enabled = false;
                chkCpySetCri.Enabled = false;
            }

        }
    }

    protected bool checkIsVTool()
    {
        int headerId = 0;
        bool valid = false;
        objData = new clsData();
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        string selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
        object objVt = objData.FetchValue(selctQuerry);

        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                valid = true;
            }
            else valid = false;

        }
        return valid;
    }


    protected void CheckSetCopyCriteria()
    {
        int headerId = 0;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            if (FindCriteriaType(headerId) == false)
            {
                chkCpyStepCri.Visible = false;
                lblCpyStepCri.Visible = false;
                chkCpySetCri.Visible = false;
                lblCpySetCri.Visible = false;
                chkCpyPromptCri.Visible = true;
                lblCpyPromptCri.Visible = true;
            }
            else
            {
                chkCpyStepCri.Visible = true;
                lblCpyStepCri.Visible = true;
                chkCpyPromptCri.Visible = true;
                lblCpyPromptCri.Visible = true;
                chkCpySetCri.Visible = false;
                lblCpySetCri.Visible = false;

            }
        }
        catch
        {
        }



    }

    protected void CheckStepCopyCriteria()
    {
        chkCpyStepCri.Visible = false;
        lblCpyStepCri.Visible = false;
        chkCpyPromptCri.Visible = true;
        lblCpyPromptCri.Visible = true;
        chkCpySetCri.Visible = true;
        lblCpySetCri.Visible = true;
    }


    protected void CheckStepPromptCriteria()
    {
        int headerId = 0;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            if (FindCriteriaType(headerId) == false)
            {
                chkCpyStepCri.Visible = false;
                lblCpyStepCri.Visible = false;
                chkCpySetCri.Visible = true;
                lblCpySetCri.Visible = true;
                chkCpyPromptCri.Visible = false;
                lblCpyPromptCri.Visible = false;
            }
            else
            {
                chkCpyStepCri.Visible = true;
                lblCpyStepCri.Visible = true;
                chkCpyPromptCri.Visible = false;
                lblCpyPromptCri.Visible = false;
                chkCpySetCri.Visible = true;
                lblCpySetCri.Visible = true;

            }
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }








    protected bool FindCriteriaType(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        string chainType = "";
        int teachId = 0;
        string teachName = "";
        string chainMode = "";
        object objTeach = null;
        bool retType = false;
        try
        {
            strQuerry = "SELECT SkillType,TeachingProcId,ChainType FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    chainType = dtNew.Rows[0]["SkillType"].ToString();
                    chainMode = dtNew.Rows[0]["ChainType"].ToString();

                    try
                    {
                        teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                    }
                    catch
                    {
                        teachId = 0;
                    }
                    if (teachId > 0)
                    {
                        try
                        {
                            strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                            objTeach = objData.FetchValue(strQuerry);
                            if (objTeach != null)
                            {
                                teachName = objTeach.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    if (chainType == "Discrete")
                    {

                        retType = false;
                    }
                    else
                    {
                        if ((teachName == "Total Task") && (chainMode == "Total Task"))
                        {

                            retType = false;
                        }
                        else
                        {

                            retType = true;
                        }

                    }
                }
            }
            return retType;
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void btnAddStepCriteria_Click(object sender, EventArgs e)
    {
        try
        {
            if (Hdfsavemeasure.Value != "")
                BtnAddStepDCriteria.Visible = false;
            else
                BtnAddStepDCriteria.Visible = true;
            Session["measuretype"] = "step";
            ClearCriteriaData();
            FillDropMeasure();                          //Fill dropdown for measure in criteria
            CheckStepCopyCriteria();
            lblCopyTo.Visible = true;
            BtnAddSetDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = false;
            BtnUpdateSetDCriteria.Visible = false;
            BtnUpdateStepDCriteria.Visible = false;
            BtnUpdatePromptDCriteria.Visible = false;
            lblCriteriaName.Text = "Step Criteria";
            setModificationVisibility(true);
            trModification.Style.Add("visibility", "hidden");
            trModification1.Style.Add("visibility", "hidden");
            chkNACriteria.Checked = false;            

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void btnAddPrompt_Click(object sender, EventArgs e)
    {
        try
        {
            Session["measuretype"] = "prompt";
            ClearCriteriaData();
            FillDropMeasure();                          //Fill dropdown for measure in criteria
            CheckStepPromptCriteria();
            lblCopyTo.Visible = true;
            BtnAddSetDCriteria.Visible = false;
            BtnAddPromptDCriteria.Visible = true;
            BtnAddStepDCriteria.Visible = false;
            BtnUpdateSetDCriteria.Visible = false;
            BtnUpdateStepDCriteria.Visible = false;
            BtnUpdatePromptDCriteria.Visible = false;
            lblCriteriaName.Text = "Prompt Criteria";
            setModificationVisibility(true);
            trModification.Style.Add("visibility", "hidden");
            trModification1.Style.Add("visibility", "hidden");
            chkNACriteria.Checked = false;            


            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AddCriteriaPopup();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void ddlTempColumn_SelectedIndexChanged(object sender, EventArgs e)
    {
        objData = new clsData();
        int selectdColumnId = 0;
        string selQuerry = "";
        try
        {
            if (ddlTempColumn.SelectedIndex != 0)
            {
                selectdColumnId = Convert.ToInt32(ddlTempColumn.SelectedValue);
                //if (Session["measuretype"].ToString() == "set")
                //{
                    selQuerry = "Select DSTempSetColCalcId as Id,CalcType as Name from DSTempSetColCalc where CalcType <> '%Accuracy at Training Step' AND CalcType <>'%Accuracy at Previously Learned Steps' AND DSTempSetColId= " + selectdColumnId;
                //}
                //else
                //{
                //    selQuerry = "Select DSTempSetColCalcId as Id,CalcType as Name from DSTempSetColCalc where DSTempSetColId= " + selectdColumnId;
                //}
                objData.ReturnDropDownForMeasureCriteria(selQuerry, ddlTempMeasure);
                DataTable dttemp = objData.ReturnDataTable(selQuerry, false);
                if ((dttemp != null) && (dttemp.Rows.Count == 0))
                {
                    ddlTempMeasure.DataSource = dttemp;
                    ddlTempMeasure.DataBind();
                }

                if (ddlTempMeasure.Items.Count == 0)
                {
                    ddlTempMeasure.Items.Insert(0, new ListItem("---------------Select Measure--------------", "0"));
                }
            }

            else
            {
                ddlTempMeasure.Items.Clear();
                ddlTempMeasure.Items.Insert(0, new ListItem("---------------Select Measure--------------", "0"));
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void FillDropMeasure()
    {
        objData = new clsData();
        string selQuerry = "";
        int headerId = 0;

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {

            selQuerry = "SELECT DSTempSetColId As Id,ColName As Name FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            objData.ReturnDropDownForCriteria(selQuerry, ddlTempColumn);
            DataTable dttemp = objData.ReturnDataTable(selQuerry, false);
            if ((dttemp != null) && (dttemp.Rows.Count == 0))
            {
                ddlTempColumn.DataSource = dttemp;
                ddlTempColumn.DataBind();
            }

            if (ddlTempColumn.Items.Count == 0)
            {
                ddlTempColumn.Items.Insert(0, new ListItem(".................Select Column..............", "0"));
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }


    }


    protected void FillSetDrpList()
    {
        objData = new clsData();
        string strQuerry = "";
        int headerId = 0;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            DataSet ds = new DataSet();
            string cmdstr = "SELECT DSTempSetId AS ID, SetCd As Name FROM DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            DataTable dt = objData.ReturnDataTableDropDown(cmdstr, false);

            ddchkCountry.DataSource = dt;
            ddchkCountry.DataTextField = "Name";
            ddchkCountry.DataValueField = "ID";
            ddchkCountry.DataBind();


            //strQuerry = "SELECT DSTempSetId AS ID, SetCd As Name FROM DSTempSet WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A'";
            //objData.ReturnDropDownForStep(strQuerry, ddlSetData);
            //DataTable dttemp = objData.ReturnDataTable(strQuerry, false);
            //if ((dttemp != null) && (dttemp.Rows.Count == 0))
            //{
            //    ddlSetData.DataSource = dttemp;
            //    ddlSetData.DataBind();
            //    //lstSetData.DataSource = dttemp;
            //    //lstSetData.DataBind();

            //}

            //if (ddlSetData.Items.Count == 0)
            //{
            //    ddlSetData.Items.Insert(0, new ListItem(".............................ALL...........................", "0"));
            //    //lstSetData.Items.Insert(0, new ListItem("No Set Created", "0"));
            //}
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }



    protected void BtnUpdateSetDetails_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string updateQuerry = "";
        int editSetId = 0;
        int headerId = 0;
        string setMatch = "";
        int length = 0;
        string matchSelctd = "";

        string DistractorsetMatch = "";        
        int Dislength = 0;
        string matchDistractors = "";
        int DistractCount = 0;

        sess = (clsSession)Session["UserSession"];
        if (ViewState["EditSetId"] != null)
        {
            editSetId = Convert.ToInt32(ViewState["EditSetId"]);
        }
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (sess != null)
        {

            if (lstMatchSamples.Items.Count > 0)
            {
                string[] arryMatchValue = new string[lstMatchSamples.Items.Count];
                updateQuerry = "UPDATE DSTempSet SET Samples = '' WHERE DSTempSetId = " + editSetId;
                int index = objData.Execute(updateQuerry);

                for (int i = 0; i < lstMatchSamples.Items.Count; i++)
                {
                    arryMatchValue[i] = lstMatchSamples.Items[i].Value.ToString();
                }
                for (int arryInt = 0; arryInt < lstMatchSamples.Items.Count; arryInt++)
                {
                    setMatch += arryMatchValue[arryInt].ToString() + ",";
                }

                length = setMatch.Length;
                matchSelctd = setMatch.ToString().Substring(0, length - 1);

                string[] DistractarryMatchValue = new string[Disitem.Items.Count];
                if (Disitem.Items.Count > 0)
                {
                    DistractCount = Disitem.Items.Count;
                    for (int i = 0; i < Disitem.Items.Count; i++)
                    {
                        DistractarryMatchValue[i] = Disitem.Items[i].Value.ToString();
            }
                    for (int arryInt = 0; arryInt < Disitem.Items.Count; arryInt++)
                    {
                        DistractorsetMatch += DistractarryMatchValue[arryInt].ToString() + ",";
                    }
                    Dislength = DistractorsetMatch.Length;
                    matchDistractors = DistractorsetMatch.ToString().Substring(0, Dislength - 1);

                }
            }

            try
            {
                updateQuerry = "UPDATE DSTempSet SET SetCd = '" + clsGeneral.convertQuotes(txtBoxAddSet.Text.Trim()) + "', SetName = '" + clsGeneral.convertQuotes(txtSetDescription.Text.Trim()) + "',Samples='" + clsGeneral.convertQuotes(setMatch.Trim()) + "',DistractorSamples='" + clsGeneral.convertQuotes(DistractorsetMatch.Trim()) + "', DistractorSamplesCount=" + DistractCount + ",ModifiedBy = " + sess.LoginId + ",ModifiedOn = GetDate()  WHERE DSTempSetId = " + editSetId;
                int index = objData.Execute(updateQuerry);

                if (IsMatchToSample(headerId) == true)
                {
                    if (matchSelctd != null)
                    {
                        MatchSampDef(headerId, matchSelctd, editSetId);                 // Match to Sample code 
                    }
                }


                ClearSetData();
                BtnUpdateSetDetails.Visible = false;
                btnAddSetDetails.Visible = true;
                lblAddOrUpdateSet.Text = "Add Set";

                fillParentSetData();


                //ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseSetPopup();", true);
                showMatchToSampleDrop();

                GetSetData(headerId);
                GetStepData(headerId);
                txtBoxAddSet.Focus();
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;
                tdMsgSet.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                throw Ex;
            }
        }

    }

    protected void BtnUpdateStep_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string updateQuerry = "";
        string strQurry = "";
        int editStepId = 0;
        int headerId = 0;
        int parentSetId = 0;
        object objParentStep = null;
        string objParents = "";
        int stepParentId = 0;
        string[] tempSteps;
        int countlsInt = 0;
        object objCount = 0;
        int findCount = 0;
        string setIds = "";
        string setNames = "";
        sess = (clsSession)Session["UserSession"];

        if (ViewState["EditStepId"] != null)
        {
            editStepId = Convert.ToInt32(ViewState["EditStepId"]);
        }
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }



        List<String> CountryID_list = new List<string>();
        List<String> CountryName_list = new List<string>();
        List<String> CountryID_Removed = new List<string>();

        //bool bAllSets = true;
        foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
        {
            if (item.Selected)
            {
                CountryID_list.Add(item.Value);
                CountryName_list.Add(item.Text);
                setIds += item.Value + ",";
                setNames += item.Text + ",";
                //bAllSets = false;
            }
            else
            {
                CountryID_Removed.Add(item.Value);
            }

        }


        try
        {
            if (sess != null)
            {
                //string ParentStepId = "";
                //strQurry = "SELECT DSTempParentStepId FROM DSTempParentStep WHERE ActiveInd='A' AND DSTempParentStepId= " + editStepId;
                //object obj = objData.FetchValue(strQurry); if (obj != null) ParentStepId = obj.ToString();
                // tempSteps = objParents.Split(',');
                //  foreach (var item in tempSteps)
                // {
                //    if (item != "")
                // {


                /// SELECT THE CURRENT AND NEW SORTORDER
                /// 
                strQurry = "SELECT SortOrder FROM DSTempParentStep WHERE DSTempParentStepId=" + editStepId + "AND ActiveInd = 'A'";
                objParentStep = objData.FetchValue(strQurry);
                int currentSortOrder = 0;
                if (objParentStep != null)
                {
                    currentSortOrder = Convert.ToInt32(objParentStep.ToString());
                }
                int newSortOrder = Convert.ToInt32(ddlSortOrder.SelectedValue);

                /// UPDATE PARENTSTEP TABLE WITH THE STEP DETAILS AND SORTORDER
                /// 
                updateQuerry = "Update DSTempParentStep Set SetIds='" + setIds + "',SetNames='" + setNames + "',StepCd='" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "',StepName='" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "' where DSTempParentStepId=" + editStepId;
                int indexs = objData.Execute(updateQuerry);



                foreach (System.Web.UI.WebControls.ListItem items in ddchkCountry.Items)
                {

                    strQurry = "SELECT DSTempStepId FROM DSTempStep WHERE DSTempSetId = " + items.Value + " AND DSTempParentStepId=" + editStepId + " AND IsDynamic=0 AND ActiveInd = 'A'";
                    objParentStep = objData.FetchValue(strQurry);

                    if (objParentStep != null)
                    {
                        if (items.Selected)
                        {
                            updateQuerry = "UPDATE DSTempStep SET ActiveInd='A',StepCd='" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "',StepName='" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "',"
                                + " ModifiedBy = " + sess.LoginId + ",ModifiedOn = GETDATE() WHERE DSTempStepId = " + objParentStep.ToString() + " AND IsDynamic=0";
                            int index = objData.Execute(updateQuerry);
                        }
                        else
                        {
                            updateQuerry = "DELETE FROM DSTempStep WHERE DSTempStepId = " + objParentStep.ToString() + " AND IsDynamic=0";
                            int index = objData.Execute(updateQuerry);
                            //updateQuerry = "UPDATE DSTempStep SET ActiveInd='D', ModifiedBy = " + sess.LoginId + ",ModifiedOn = GETDATE() WHERE DSTempStepId = " + objParentStep.ToString() + " AND IsDynamic=0";
                            //int index = objData.Execute(updateQuerry);

                        }
                    }
                    else
                    {
                        if (items.Selected)
                        {
                            //----------------------

                            //string selQuerry = "SELECT ISNULL(MAX(SortOrder),0) FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND ActiveInd = 'A' AND IsDynamic=0 AND DSTempSetId = " + Convert.ToInt32(items.Value);
                            //objCount = objData.FetchValue(selQuerry);
                            //if (objCount != null)
                            //{
                            //    findCount = Convert.ToInt32(objCount);
                            //}

                            //int newCount = findCount + 1;

                            //------------------------------

                            try
                            {
                                string strQuery = "Insert Into DSTempStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,DSTempParentStepId,SortOrder,ActiveInd,CreatedBy,CreatedOn) ";
                                strQuery += " Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtStepName.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtStepDesc.Text.Trim()) + "'," + Convert.ToInt32(items.Value) + "," + editStepId + "," + currentSortOrder + ",'A'," + sess.LoginId + ",getdate())";
                                int stepId = objData.ExecuteWithScope(strQuery);


                            }
                            catch (Exception Ex)
                            {
                                string error = Ex.Message;
                                tdMsg.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                            }
                        }
                    }
                    // UpdateCompleteSteponSet(Convert.ToInt32(items.Value), headerId);
                }


                if (currentSortOrder < newSortOrder)
                {
                    ///UPDATE SORTORDER OF PARENTSTEP
                    ///
                    strQurry = "SELECT  DSTempParentStepId,SortOrder FROM DSTempParentStep"
                        + " WHERE DSTempHdrId=" + headerId + "AND ActiveInd = 'A' AND SortOrder >" + currentSortOrder + " AND SortOrder <=" + newSortOrder;
                    DataTable dtList = objData.ReturnDataTable(strQurry, false);

                    for (int i = 0; i < dtList.Rows.Count; i++)
                    {
                        int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) - 1;
                        strQurry = "UPDATE DSTempParentStep SET SortOrder = " + sortOderToChange + " WHERE DSTempParentStepId = " + Convert.ToInt32(dtList.Rows[i]["DSTempParentStepId"].ToString());
                        objData.Execute(strQurry);
                    }
                    strQurry = "UPDATE DSTempParentStep SET SortOrder = " + newSortOrder + " WHERE DSTempParentStepId = " + editStepId;
                    objData.Execute(strQurry);


                    ///UPDATE SORTORDER OF DSTempStep
                    ///
                    strQurry = "SELECT  DSTempStepId,SortOrder FROM DSTempStep"
                        + " WHERE DSTempHdrId=" + headerId + "AND ActiveInd = 'A' AND SortOrder >" + currentSortOrder + " AND SortOrder <=" + newSortOrder;
                    dtList = objData.ReturnDataTable(strQurry, false);

                    for (int i = 0; i < dtList.Rows.Count; i++)
                    {
                        int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) - 1;
                        strQurry = "UPDATE DSTempStep SET SortOrder = " + sortOderToChange + " WHERE DSTempStepId = " + Convert.ToInt32(dtList.Rows[i]["DSTempStepId"].ToString());
                        objData.Execute(strQurry);
                    }
                    strQurry = "UPDATE DSTempStep SET SortOrder = " + newSortOrder + " WHERE DSTempParentStepId = " + editStepId;
                    objData.Execute(strQurry);
                }

                else if (currentSortOrder > newSortOrder)
                {
                    ///UPDATE SORTORDER OF PARENTSTEP
                    ///
                    strQurry = "SELECT  DSTempParentStepId,SortOrder FROM DSTempParentStep"
                        + " WHERE DSTempHdrId=" + headerId + "AND ActiveInd = 'A' AND SortOrder <" + currentSortOrder + " AND SortOrder >=" + newSortOrder;
                    DataTable dtList = objData.ReturnDataTable(strQurry, false);

                    for (int i = 0; i < dtList.Rows.Count; i++)
                    {
                        int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) + 1;
                        strQurry = "UPDATE DSTempParentStep SET SortOrder = " + sortOderToChange + " WHERE DSTempParentStepId = "
                            + Convert.ToInt32(dtList.Rows[i]["DSTempParentStepId"].ToString());
                        objData.Execute(strQurry);
                    }
                    strQurry = "UPDATE DSTempParentStep SET SortOrder = " + newSortOrder + " WHERE DSTempParentStepId = " + editStepId;
                    objData.Execute(strQurry);


                    ///UPDATE SORTORDER OF DSTempStep
                    ///
                    strQurry = "SELECT  DSTempStepId,SortOrder FROM DSTempStep"
                        + " WHERE DSTempHdrId=" + headerId + "AND ActiveInd = 'A' AND SortOrder <" + currentSortOrder + " AND SortOrder >=" + newSortOrder;
                    dtList = objData.ReturnDataTable(strQurry, false);

                    for (int i = 0; i < dtList.Rows.Count; i++)
                    {
                        int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) + 1;
                        strQurry = "UPDATE DSTempStep SET SortOrder = " + sortOderToChange + " WHERE DSTempStepId = "
                            + Convert.ToInt32(dtList.Rows[i]["DSTempStepId"].ToString());
                        objData.Execute(strQurry);
                    }
                    strQurry = "UPDATE DSTempStep SET SortOrder = " + newSortOrder + " WHERE DSTempParentStepId = " + editStepId;
                    objData.Execute(strQurry);
                }








                btnAddStepDetails.Visible = true;
                BtnUpdateStep.Visible = false;
                chkEnd.Enabled = true;
                chkEnd.Checked = true;
                ddlSortOrder.Enabled = false;
                lblAddorUpdateStep.Text = "Add Step";
                ClearStepData();

                //ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseStepPopup();", true);
                GetStepData(headerId);
            }
            showMatchToSampleDrop();
            ddchkCountry.Focus();
        }
        catch (Exception Ex)
        {
            string error = Ex.Message;
            tdMsg.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
            throw Ex;
        }

    }





    protected void UpdateCompleteSteponSet(int parentSetId, int headerId)
    {
        string strQuerry = "";
        string updateQuerry = "";
        objData = new clsData();
        strQuerry = "SELECT DSTempStepId,DSTempSetId,SortOrder,DSTempHdrId,RANK() OVER (ORDER BY SortOrder) As SortRank FROM DSTempStep WHERE " +
                                 "   DSTempHdrId = " + headerId + " AND DSTempSetId =" + parentSetId + " AND IsDynamic=0 AND ActiveInd='A'";
        DataTable dtList = objData.ReturnDataTable(strQuerry, false);
        if (dtList != null)
        {
            if (dtList.Rows.Count > 0)
            {

                foreach (DataRow row in dtList.Rows)
                {
                    updateQuerry = "UPDATE DsTempStep SET SortOrder = " + row["SortRank"].ToString() + " WHERE DSTempStepId= " + row["DSTempStepId"].ToString() + " AND IsDynamic=0";
                    objData.Execute(updateQuerry);
                }

            }
        }
    }

    protected void UpdateSortOrder(int parentSetId, int headerId, int selectedText)
    {
        string strQuerry = "";
        string updateQuerry = "";
        objData = new clsData();

        try
        {
            //-----------------------------

            //strQuerry = "SELECT  DSTempStepId,SortOrder FROM DSTempStep"
            //               + " WHERE DSTempHdrId=" + headerId + "AND ActiveInd = 'A' AND SortOrder >=" + selectedText;
            //DataTable dtList = objData.ReturnDataTable(strQuerry, false);

            //for (int i = 0; i < dtList.Rows.Count; i++)
            //{
            //    int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) + 1;
            //    strQuerry = "UPDATE DSTempStep SET SortOrder = " + sortOderToChange + " WHERE DSTempStepId = "
            //        + Convert.ToInt32(dtList.Rows[i]["DSTempStepId"].ToString());
            //    objData.Execute(strQuerry);
            //}

            strQuerry = "update DSTempStep  set SortOrder=(select SortOrder from DSTempParentStep where DSTempParentStep.DSTempParentStepId=DSTempStep.DSTempParentStepId) where ActiveInd='A' and DSTempHdrId=" + headerId;
            objData.Execute(strQuerry);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        //-----------------------------
    }

    protected void UpdateCompleteSteponParentSet(int parentSetId, int headerId)
    {
        string strQuerry = "";
        string updateQuerry = "";
        objData = new clsData();
        strQuerry = "SELECT DSTempStepId,DSTempSetId,SortOrder,DSTempHdrId,RANK() OVER (ORDER BY SortOrder) As SortRank FROM DSTempStep WHERE " +
                                 "   DSTempHdrId = " + headerId + " AND DSTempParentStepId =" + parentSetId + " AND IsDynamic=0";

        DataTable dtList = objData.ReturnDataTable(strQuerry, false);
        if (dtList != null)
        {
            if (dtList.Rows.Count > 0)
            {
                foreach (DataRow row in dtList.Rows)
                {
                    updateQuerry = "UPDATE DsTempStep SET SortOrder = " + row["SortRank"].ToString() + " WHERE DSTempStepId= " + row["DSTempStepId"].ToString() + " AND IsDynamic=0";
                    objData.Execute(updateQuerry);
                }

            }
        }
    }


    protected void BtnUpdateSetDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string updateQuerry = "";
        int setCriteriaId = 0;
        int headerId = 0;
        string totCorrectResp = "";
        string totalResp = "";
        int isNAChk = 0;        
        sess = (clsSession)Session["UserSession"];
        //  int consctveSess = 0;
        //  string totalInstance = "";

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ViewState["EditSetCriteria"] != null)
        {
            setCriteriaId = Convert.ToInt32(ViewState["EditSetCriteria"]);
        }
        try
        {
            if (sess != null)
            {
                if (rbtnConsectiveSes.SelectedValue == "0" && rbtnConsectiveAvg.SelectedValue == "0") //--- [New Criteria] May 2020 ---//
                {
                    totCorrectResp = txtIns2.Text.ToString();
                    totalResp = txtIns1.Text.ToString();
                }
                else
                {
                    totCorrectResp = txtNumbrSessions.Text.ToString();
                }

                if (ValidateCheck() == true)
                {

                    try
                    {
                        if (chkNACriteria.Checked == true)
                        {
                            isNAChk = 1;
                        }                 
                        updateQuerry = "UPDATE DSTempRule SET CriteriaType = '" + ddlCriteriaType.SelectedValue.ToString() + "', IOAReqInd = '" + rbtnIoaReq.SelectedValue.ToString() + "', MultiTeacherReqInd = '" + rbtnMultitchr.SelectedValue.ToString() + "',DSTempSetColId = '" + ddlTempColumn.SelectedValue.ToString() + "',DSTempSetColCalcId = '" + ddlTempMeasure.SelectedValue.ToString() + "',ScoreReq = '" + clsGeneral.convertQuotes(txtRequiredScore.Text.Trim()) + "',ConsequetiveInd = '" + rbtnConsectiveSes.SelectedValue.ToString() + "',ConsequetiveAvgInd = '" + rbtnConsectiveAvg.SelectedValue.ToString() + "',TotCorrInstance = '" + totalResp + "',TotalInstance = '" + totCorrectResp + "',ModifiedBy = " + sess.LoginId + ", ModifiedOn = GETDATE(),IsComment=" + IsComments.Value + ",ModificationComment='" + txtModComments.Text + "',ModificationRule='" + txtModNo.Text + "',IsNA=" + isNAChk + " WHERE DSTempRuleId = " + setCriteriaId; //--- [New Criteria] May 2020 ---//

                        int index = objData.Execute(updateQuerry);
                        ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);
                        GetSetCriteriaData(headerId);

                    }
                    catch (Exception Ex)
                    {
                        string error = Ex.Message;
                        tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                    }
                }
            }
            showMatchToSampleDrop();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void BtnUpdateStepDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string updateQuerry = "";
        int setCriteriaId = 0;
        int headerId = 0;
        string totCorrectResp = "";
        string totalResp = "";
        int isNAChk = 0;        
        sess = (clsSession)Session["UserSession"];
        //  int consctveSess = 0;
        //  string totalInstance = "";

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ViewState["EditStepCriteria"] != null)
        {
            setCriteriaId = Convert.ToInt32(ViewState["EditStepCriteria"]);
        }
        try
        {
            if (sess != null)
            {
                if (rbtnConsectiveSes.SelectedValue == "0" && rbtnConsectiveAvg.SelectedValue == "0") //--- [New Criteria] May 2020 ---//
                {
                    totCorrectResp = txtIns2.Text.ToString();
                    totalResp = txtIns1.Text.ToString();
                }
                else
                {
                    totCorrectResp = txtNumbrSessions.Text.ToString();
                }

                if (ValidateCheck() == true)
                {

                    try
                    {
                        if (chkNACriteria.Checked == true)
                        {
                            isNAChk = 1;
                        }                                                 
                        updateQuerry = "UPDATE DSTempRule SET CriteriaType = '" + ddlCriteriaType.SelectedValue.ToString() + "', IOAReqInd = '" + rbtnIoaReq.SelectedValue.ToString() + "', MultiTeacherReqInd = '" + rbtnMultitchr.SelectedValue.ToString() + "',DSTempSetColId = '" + ddlTempColumn.SelectedValue.ToString() + "',DSTempSetColCalcId = '" + ddlTempMeasure.SelectedValue.ToString() + "',ScoreReq = '" + clsGeneral.convertQuotes(txtRequiredScore.Text.Trim()) + "',ConsequetiveInd = '" + rbtnConsectiveSes.SelectedValue.ToString() + "',ConsequetiveAvgInd = '" + rbtnConsectiveAvg.SelectedValue.ToString() + "',TotCorrInstance = '" + totalResp + "',TotalInstance = '" + totCorrectResp + "',ModifiedBy = " + sess.LoginId + ", ModifiedOn = GETDATE(),IsComment=" + IsComments.Value + ",ModificationComment='" + txtModComments.Text + "',ModificationRule='" + txtModNo.Text + "',IsNA=" + isNAChk + " WHERE DSTempRuleId = " + setCriteriaId; //--- [New Criteria] May 2020 ---//
                        int index = objData.Execute(updateQuerry);
                        ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);
                        GetStepCriteriaData(headerId);
                                          
                    }
                    catch (Exception Ex)
                    {
                        string error = Ex.Message;
                        tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                    }
                }
            }
            showMatchToSampleDrop();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void BtnUpdatePromptDCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string updateQuerry = "";
        int setCriteriaId = 0;
        int headerId = 0;
        string totCorrectResp = "";
        string totalResp = "";
        int isNAChk = 0;        
        sess = (clsSession)Session["UserSession"];
        //  int consctveSess = 0;
        //  string totalInstance = "";

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        if (ViewState["EditPromptCriteria"] != null)
        {
            setCriteriaId = Convert.ToInt32(ViewState["EditPromptCriteria"]);
        }
        try
        {
            if (sess != null)
            {
                if (rbtnConsectiveSes.SelectedValue == "0" && rbtnConsectiveAvg.SelectedValue == "0") //--- [New Criteria] May 2020 ---//
                {
                    totCorrectResp = txtIns2.Text.ToString();
                    totalResp = txtIns1.Text.ToString();
                }
                else
                {
                    totCorrectResp = txtNumbrSessions.Text.ToString();
                }

                if (ValidateCheck() == true)
                {

                    try
                    {
                        if (chkNACriteria.Checked == true)
                        {
                            isNAChk = 1;
                        }                   
                        updateQuerry = "UPDATE DSTempRule SET CriteriaType = '" + ddlCriteriaType.SelectedValue.ToString() + "', IOAReqInd = '" + rbtnIoaReq.SelectedValue.ToString() + "', MultiTeacherReqInd = '" + rbtnMultitchr.SelectedValue.ToString() + "',DSTempSetColId = '" + ddlTempColumn.SelectedValue.ToString() + "',DSTempSetColCalcId = '" + ddlTempMeasure.SelectedValue.ToString() + "',ScoreReq = '" + clsGeneral.convertQuotes(txtRequiredScore.Text.Trim()) + "',ConsequetiveInd = '" + rbtnConsectiveSes.SelectedValue.ToString() + "',ConsequetiveAvgInd = '" + rbtnConsectiveAvg.SelectedValue.ToString() + "',TotCorrInstance = '" + totalResp + "',TotalInstance = '" + totCorrectResp + "',ModifiedBy = " + sess.LoginId + ", ModifiedOn = GETDATE(),IsComment=" + IsComments.Value + ",ModificationComment='" + txtModComments.Text + "',ModificationRule='" + txtModNo.Text + "',IsNA=" + isNAChk + " WHERE DSTempRuleId = " + setCriteriaId; //--- [New Criteria] May 2020 ---//
                        int index = objData.Execute(updateQuerry);
                        ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "CloseCriteriaPopup();", true);
                        GetPromptCriteriaData(headerId);

                    }
                    catch (Exception Ex)
                    {
                        string error = Ex.Message;
                        tdMsgCriteria.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                    }
                }
            }
            showMatchToSampleDrop();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void btnRemoveSet_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int setId = 0;
        int headerId = 0;
        Button BtnDelt = (Button)sender;
        DataTable dtNew = new DataTable();
        setId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            //New code - check the selected lesson is match to sample or not

            //selQuerry = "SELECT SkillType,TeachingProcId FROM DSTempHdr WHERE DSTempHdrId = " + headerId;
            //dtNew = objData.ReturnDataTable(selQuerry, false);
            //if (dtNew != null)
            //{
            //    if (dtNew.Rows.Count > 0)
            //    {
            //        chainType = dtNew.Rows[0]["SkillType"].ToString();
            //        try
            //        {
            //            teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
            //        }
            //        catch
            //        {
            //            teachId = 0;
            //        }
            //        if (teachId > 0)
            //        {
            //            try
            //            {
            //                selQuerry = "SELECT LookupName FROM LookUp WHERE LookupId = " + teachId;
            //                objTeach = objData.FetchValue(selQuerry);
            //                if (objTeach != null)
            //                {
            //                    teachName = objTeach.ToString();
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                throw ex;
            //            }
            //        }
            //    }
            //}
            ////End



            //// Check the selected set has any child step.

            //if (chainType == "Discrete" && teachName == "Match-to-Sample")
            //{
            //    //deltQuerry = "UPDATE DSTempSet SET ActiveInd = 'D' WHERE DSTempSetId = " + setId;
            //    deltQuerry = "DELETE DSTempSet WHERE DSTempSetId = " + setId;
            //    int index = objData.Execute(deltQuerry);
            //}
            //else
            //{
            //    selQuerry = "SELECT DSTempStepId FROM DSTempStep WHERE DSTempHdrId = " + headerId + " AND DSTempSetId = " + setId + " AND ActiveInd = 'A' AND IsDynamic=0";
            //    dtNew = objData.ReturnDataTable(selQuerry, false);

            //    if (dtNew != null)
            //    {
            //        if (dtNew.Rows.Count > 0)
            //        {
            //            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "AlertNotDelete();", true);
            //        }
            //        else
            //        {
            //            //deltQuerry = "UPDATE DSTempSet SET ActiveInd = 'D' WHERE DSTempSetId = " + setId;
            //            deltQuerry = "DELETE DSTempSet WHERE DSTempSetId = " + setId;
            //            int index = objData.Execute(deltQuerry);


            //        }
            //    }
            //    else
            //    {
            //        //deltQuerry = "UPDATE DSTempSet SET ActiveInd = 'D' WHERE DSTempSetId = " + setId;
            //        deltQuerry = "DELETE DSTempSet WHERE DSTempSetId = " + setId;
            //        int index = objData.Execute(deltQuerry);


            //    }
            //}

            clsAssignLessonPlan ObjDeleteLP = new clsAssignLessonPlan();
            ObjDeleteLP.Delete_Reorder_setStep(setId, "set");
            fillParentSetData();
            GetSetData(headerId);
            GetStepData(headerId);
            txtBoxAddSet.Focus();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    protected void btnRemoveStep_Click(object sender, EventArgs e)
    {
        BtnAddStep_Click(sender, e); // To Hide Update Button on Edit - Update is in Progress -- Dev 2 [10-Jul-2020]
        objData = new clsData();
        int stepId = 0;
        int headerId = 0;
        //string deltQuerry = "";
        string strQurry = "";
        string objParents = null;
        Button BtnDelt = (Button)sender;
        //string[] tempSteps;
        stepId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            /// GET THE CURRENT SORTORDER OF THE STEP TO DELETE
            strQurry = "SELECT SortOrder from  DSTempParentStep WHERE DSTempParentStepId = " + stepId + "AND ActiveInd = 'A'";
            int currentSortOrder = Convert.ToInt32(objData.FetchValue(strQurry).ToString());


            /// DELETE STEPS FROM ALL SETS IN DSTEMPSTEP TABLE
            /// 
            strQurry = "DELETE FROM DSTempStep WHERE DSTempParentStepId= " + stepId;
            int index = objData.Execute(strQurry);

            /// DELETE STEP FROM THE PARENT TABLE IE. DSTEMPPARENTSTEP
            /// 
            strQurry = "UPDATE DSTempParentStep SET ActiveInd = 'D' WHERE DSTempParentStepId = " + stepId;
            index = objData.Execute(strQurry);

            /// REORDER PARENTSTEP TABLE
            /// 
            strQurry = "SELECT  DSTempParentStepId,SortOrder FROM DSTempParentStep"
                        + " WHERE DSTempHdrId = " + headerId + " And ActiveInd = 'A' AND SortOrder >" + currentSortOrder;
            DataTable dtList = objData.ReturnDataTable(strQurry, false);

            for (int i = 0; i < dtList.Rows.Count; i++)
            {
                int sortOderToChange = Convert.ToInt32(dtList.Rows[i]["SortOrder"].ToString()) - 1;
                strQurry = "UPDATE DSTempParentStep SET SortOrder = " + sortOderToChange + " WHERE DSTempParentStepId = " + Convert.ToInt32(dtList.Rows[i]["DSTempParentStepId"].ToString());
                objData.Execute(strQurry);

                strQurry = "UPDATE DSTempStep SET SortOrder = " + sortOderToChange + " WHERE DSTempParentStepId = " + Convert.ToInt32(dtList.Rows[i]["DSTempParentStepId"].ToString());
                objData.Execute(strQurry);
            }


            #region OLD CODE {COMMENTED}
            // ------------------- OLD CODE ------------------

            //strQurry = "SELECT DSTempStepId FROM DSTempStep WHERE ActiveInd='A' AND DSTempParentStepId= " + stepId;
            //objParents = objData.FetchValue(strQurry).ToString();
            //tempSteps = objParents.Split(',');
            //foreach (var item in tempSteps)
            //{
            //    if (item != "")
            //    {
            //        //deltQuerry = "UPDATE DSTempStep SET ActiveInd = 'D' WHERE DSTempSetId = " + item + " AND DSTempParentStepId=" + stepId;
            //        deltQuerry = "DELETE DSTempStep WHERE DSTempSetId = " + item + " AND DSTempParentStepId=" + stepId +" AND IsDynamic=0";
            //        int index = objData.Execute(deltQuerry);



            //        string sel = "SELECT * FROM DSTempStep WHERE  DSTempSetId = " + item + " AND DSTempHdrId=" + headerId + " AND ActiveInd='A' AND SortOrder>(SELECT SortOrder FROM DSTempStep WHERE DSTempSetId = " + item + " AND DSTempParentStepId=" + stepId + " AND IsDynamic=0) AND IsDynamic=0";
            //        DataTable dtSteps = objData.ReturnDataTable(sel, false);
            //        if (dtSteps != null)
            //        {
            //            foreach (DataRow dr in dtSteps.Rows)
            //            {
            //                deltQuerry = "UPDATE DSTempStep SET SortOrder=(SortOrder-1) WHERE DSTempStepId=" + dr["DSTempStepId"].ToString()+" AND IsDynamic=0";
            //                objData.Execute(deltQuerry);
            //            }
            //        }
            //    }

            //}
            //deltQuerry = "UPDATE DSTempParentStep SET ActiveInd = 'D' WHERE DSTempParentStepId = " + stepId;
            //int indexs = objData.Execute(deltQuerry);


            //clsAssignLessonPlan ObjDeleteLP = new clsAssignLessonPlan();
            //ObjDeleteLP.Delete_Reorder_setStep(Convert.ToInt32(objParents), "step");



            //// --------------------------------


            //string strQuerry = "SELECT DSTempParentStepId,DSTempSetId,SortOrder,DSTempHdrId,RANK() OVER (ORDER BY SortOrder) As SortRank FROM DSTempParentStep WHERE " +
            //    "   DSTempHdrId = " + headerId + " AND ActiveInd='A'";
            //DataTable dtList = objData.ReturnDataTable(strQuerry, false);
            //if (dtList.Rows.Count > 0)
            //{
            //    foreach (DataRow row in dtList.Rows)
            //    {
            //        string updateQry = "UPDATE DSTempParentStep SET SortOrder = " + row["SortRank"] + " WHERE DSTempParentStepId= " + row["DSTempParentStepId"].ToString();
            //        objData.Execute(updateQry);
            //    }
            //}


            ////---------------------------------

            #endregion


            GetStepData(headerId);
            ddchkCountry.Focus();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }


    protected void BtnRemoveSetCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        int setCriteriaId = 0;
        Button BtnDelt = (Button)sender;
        setCriteriaId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            string strQuerry = "UPDATE DSTempRule SET ActiveInd = 'D' WHERE DSTempRuleId = " + setCriteriaId;
            int index = objData.Execute(strQuerry);
            GetSetCriteriaData(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }


    protected void BtnRemoveStepCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        int setCriteriaId = 0;
        Button BtnDelt = (Button)sender;
        setCriteriaId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            string strQuerry = "UPDATE DSTempRule SET ActiveInd = 'D' WHERE DSTempRuleId = " + setCriteriaId;
            int index = objData.Execute(strQuerry);
            GetStepCriteriaData(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void BtnRemovePromptCriteria_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        int setCriteriaId = 0;
        Button BtnDelt = (Button)sender;
        setCriteriaId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            string strQuerry = "UPDATE DSTempRule SET ActiveInd = 'D' WHERE DSTempRuleId = " + setCriteriaId;
            int index = objData.Execute(strQuerry);
            GetPromptCriteriaData(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }




    protected void ddlColumnType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlColumnType.SelectedValue == "+/-")
        {
            PlusMinusDiv.Visible = true;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }
        else if (ddlColumnType.SelectedValue == "Prompt")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = true;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }

        else if (ddlColumnType.SelectedValue == "Text")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = true;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }
        else if (ddlColumnType.SelectedValue == "Duration")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = true;
            FrequencyDiv.Visible = false;
        }
        else if (ddlColumnType.SelectedValue == "Frequency")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = true;
        }
    }

    protected void BtnAddMeasure_Click(object sender, EventArgs e)
    {
        aoTb.Style.Add("display", "none");
        aoTb2.Style.Add("display", "none");
        promptAOTb.Style.Add("display", "none");
        promptAOTb2.Style.Add("display", "none");
        opdiv1.Style.Add("display", "none");
        opdiv1.Style.Add("display", "none");
        textAOTb.Style.Add("display", "none");
        durationAOTb.Style.Add("display", "none");
        frequencyAOTb.Style.Add("display", "none");
        BtnSaveMeasure.Visible = true;
        BtnUpdateMeasure.Visible = false;
        PlusMinusDiv.Visible = true;
        promptDiv.Visible = false;
        TextDiv.Visible = false;
        DurationDiv.Visible = false;
        FrequencyDiv.Visible = false;
        chkCurrentPrompt.Checked = true;
        ViewState["New"] = true;
        FillAllPrompts();             // Fill all prompts when selecting prompt column type
        ClearData();
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "EditMeasurePopup();", true);

    }

    protected void FillAllPrompts()
    {
        objData = new clsData();
        objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt' order by SortOrder", ddlPromptList);

    }

    protected void ClearData()
    {
        txtColumnName.Text = "";
        MoveupOpt.SelectedValue = "1";
        MoveupOpt1.SelectedValue = "1";
        ddlColumnType.SelectedValue = "+/-";
        rdbplusMinus.SelectedValue = "+";
        txtplusCorrectResponse.Text = "";
        txtPlusIncorrectResp.Text = "";
        chkplusIncludeMistrial.Checked = false;
        txtplusIncludeMistrial.Text = "";
        chkplusAccuracy.Checked = false;
        chkplusAccuracy.Enabled = true;
        chkplustotalcorrect.Checked = false;
        chkplustotalcorrect.Enabled = true;
        chkplustotalIncorrect.Checked = false;
        chkplustotalIncorrect.Enabled = true;
        txtExCurrentStep.Text = "";

        txtpluslearnedStep.Text = "";
        txtplusAccuracy.Text = "";
        txtplustotalcorrect.Text = "";
        txtPlustotalIncorrect.Text = "";
        chkplusindependent.Checked = false;
        chkplusindependent.Enabled = true;
        chkplusindependentForAll.Checked = false;
        chkplusindependentForAll.Enabled = true;
        txtplusIndependentForAll.Text = "";
        txtplusIndependent.Text = "";
        chkpluslearnedStep.Checked = false;
        chkpluslearnedStep.Enabled = true;

        chkCurrentStep.Checked = false;
        chkCurrentStep.Enabled = true;

        txtpluslearnedStep.Text = "";
        chkPlusPromptPerc.Checked = false;
        chkPlusPromptPerc.Enabled = true;
        txtPlusPromptPerc.Text = "";

        //   chkCurrentPrompt.Checked = false;
        txtpromptSelectPrompt.Text = "";
        txtPromptIncrctResp.Text = "";
        txtPromptIncMisTrial.Text = "";
        chkPromptInclMisTrial.Checked = false;
        chkPrompPercAccuracy.Checked = false;
        chkPrompPercAccuracy.Enabled = true;
        chkPromptPercPrompt.Checked = false;
        chkPromptPercPrompt.Enabled = true;
        txtPromptpecPrompt.Text = "";
        chkPercIndependent.Checked = false;
        chkPercIndependent.Enabled = true;
        chkPercIndependentForAll.Checked = false;
        chkPercIndependentForAll.Enabled = true;
        txtPromptIndependent.Text = "";
        txtPromptAccuracy.Text = "";
        chkPromptAccExcluseCrntStep.Checked = false;
        chkPromptAccExcluseCrntStep.Enabled = true;
        chkPromptAccLearnedStep.Checked = false;
        chkPromptAccLearnedStep.Enabled = true;
        txtPromptAccExcluseCrntStep.Text = "";
        txtPromptAccLearnedStep.Text = "";

        txtTextCrctResponse.Text = "";
        txtTextInCrctResp.Text = "";
        chkTxtIncMisTrial.Checked = false;
        chkTxtIncMisTrial.Enabled = true;
        txtTxtIncMisTrial.Text = "";
        chkTxtNa.Checked = false;
        chkTxtNa.Enabled = true;
        txtTxtNA.Text = "";
        chkTextCustomize.Checked = false;
        chkTextCustomize.Enabled = true;
        txtTxtCustomize.Text = "";
        txtCalcuType.Text = "";

        txtDurCorrectResponse.Text = "";
        txtDurIncrctResp.Text = "";
        chkDurIncludeMistrial.Checked = false;
        txtDurInclMisTrial.Text = "";
        chkDurAverage.Checked = false;
        chkDurAverage.Enabled = true;
        chkDurTotalDur.Checked = false;
        chkDurTotalDur.Enabled = true;
        txtFreqCorrectResponse.Text = "";
        txtfreqIncrctResp.Text = "";
        chkFreqIncludeMistrial.Checked = false;
        txtFreqIncludeMistrial.Text = "";
        chkFrequency.Checked = false;
        chkFrequency.Enabled = true;
        txtFrequency.Text = "";
        ddlColumnType.Enabled = true;
        MoveupOpt1.Enabled = true;
        MoveupOpt.Enabled = true;
        txtDurAverage.Text = "";
        txtDurTotalDuration.Text = "";
        divBtn.Visible = false;

        tdMsgMeasure.InnerHtml = "";

        chkplusAccuracyIIG.Checked = false;
        chkPlusPromptPercIIG.Checked = false;
        chkplusindependentIIG.Checked = false;
        chkplusindependentForAllIIG.Checked = false;
        chkpluslearnedStepIIG.Checked = false;
        chkCurrentStepIIG.Checked = false;
        chkplustotalcorrectIIG.Checked = false;
        chkplustotalIncorrectIIG.Checked = false;

        chkPrompPercAccuracyIIG.Checked = false;
        chkPromptPercPromptIIG.Checked = false;
        chkPercIndependentIIG.Checked = false;
        chkPercIndependentForAllIIG.Checked = false;
        chkPromptAccLearnedStepIIG.Checked = false;
        chkPromptAccExcluseCrntStepIIG.Checked = false;

        chkTxtNaIIG.Checked = false;
        chkTextCustomizeIIG.Checked = false;

        chkDurAverageIIG.Checked = false;
        chkDurTotalDurIIG.Checked = false;

        chkFrequencyIIG.Checked = false;
        //
        txtColumnName.ReadOnly = false;
        txtplusCorrectResponse.ReadOnly = false;
        txtPlusIncorrectResp.ReadOnly = false;
        txtplusAccuracy.ReadOnly = false;
        txtPlusPromptPerc.ReadOnly = false;
        txtplusIndependent.ReadOnly = false;
        txtplusIndependentForAll.ReadOnly = false;
        txtplustotalcorrect.ReadOnly = false;
        txtPlustotalIncorrect.ReadOnly = false;
        txtpluslearnedStep.ReadOnly = false;
        txtExCurrentStep.ReadOnly = false;
        txtpromptSelectPrompt.ReadOnly = false;
        txtPromptIncrctResp.ReadOnly = false;
        txtPromptAccuracy.ReadOnly = false;
        txtPromptpecPrompt.ReadOnly = false;
        txtPromptIndependent.ReadOnly = false;
        txtPromptIndependentForAll.ReadOnly = false;
        txtPromptAccLearnedStep.ReadOnly = false;
        txtPromptAccExcluseCrntStep.ReadOnly = false;
        //text
        txtTxtNA.ReadOnly = false;
        txtTxtCustomize.ReadOnly = false;
        txtCalcuType.ReadOnly = false;
        txtTextCrctResponse.ReadOnly = false;
        txtTextInCrctResp.ReadOnly = false;
        //duration
        txtDurCorrectResponse.ReadOnly = false;
        txtDurIncrctResp.ReadOnly = false;
        txtDurAverage.ReadOnly = false;
        txtDurTotalDuration.ReadOnly = false;
        //frequency
        txtfreqIncrctResp.ReadOnly = false;
        txtfreqIncrctResp.ReadOnly = false;
        txtFrequency.ReadOnly = false;

    }
    protected void ClearSetData()
    {
        txtBoxAddSet.Text = "";
        txtSetDescription.Text = "";
        tdMsgSet.InnerHtml = "";
        lstMatchSamples.Items.Clear();
        Disitem.Items.Clear();
        txtMatcSamples.Text = "";
        txtBoxAddSet.ReadOnly = false;
        txtSetDescription.ReadOnly = false;
        txtMatcSamples.ReadOnly = false;
        lstMatchSamples.Enabled = true;
        DistractInc = 1;
        chk_distractor.Checked = false;
        chk_distractor.Enabled = true;
        Disitem.Enabled = true;
    }

    protected void ClearStepData()
    {
        foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
        {
            item.Selected = false;
        }
        lblSelParentSets.Text = "Not Assigned to any Sets";
        txtStepName.Text = "";
        txtStepDesc.Text = "";
        tdMsgStep.InnerHtml = "";
        ddchkCountry.Enabled = true;
        txtStepName.ReadOnly = false;
        txtStepDesc.ReadOnly = false;

    }

    protected void ClearCriteriaData()
    {
        ddlCriteriaType.SelectedIndex = 0;
        //for (int i = 0; i < rbtnIoaReq.Items.Count; i++)
        //{
        //    rbtnIoaReq.Items[i].Selected = false;
        //}

        //for (int i = 0; i < rbtnMultitchr.Items.Count; i++)
        //{
        //    rbtnMultitchr.Items[i].Selected = false;
        //}
        //ddlTempColumn.SelectedValue = "0";
        //for (int i = 0; i < rbtnConsectiveSes.Items.Count; i++)
        //{
        //    rbtnConsectiveSes.Items[i].Selected = false;
        //}

        rbtnIoaReq.SelectedValue = "0";
        rbtnMultitchr.SelectedValue = "0";
        rbtnConsectiveSes.SelectedValue = "0";
        rbtnConsectiveAvg.SelectedValue = "0"; //--- [New Criteria] May 2020 ---//
        ddlTempMeasure.SelectedValue = "0";
        txtNumbrSessions.Text = "";
        txtRequiredScore.Text = "";
        txtIns1.Text = "";
        txtIns2.Text = "";
        txtModNo.Text = "";
        txtModComments.Enabled = true;
        txtModComments.Text = "";
        chkIsComments.Checked = true;
        chkNACriteria.Checked = false;
        chkScore.Checked = false;
        txtIns2.ReadOnly = false;
        txtIns1.ReadOnly = false;
        tdMsgCriteria.InnerHtml = "";
        txtModNo.Enabled = false;
        chkCpyPromptCri.Checked = false;
        chkCpySetCri.Checked = false;
        chkCpyStepCri.Checked = false;
        //ddlCriteriaType.Enabled = true;
        txtRequiredScore.ReadOnly = false;
        txtNumbrSessions.ReadOnly = false;
        txtIns1.ReadOnly = false;
        txtIns2.ReadOnly = false;
        ddlCriteriaType.Enabled = true;

    }




    protected void BtnSaveMeasure_Click(object sender, EventArgs e)
    {

        objData = new clsData();
        SqlTransaction Transs = null;
        SqlConnection con = objData.Open();
        string insertQuerry = "";
        string selectQry = "";
        int returnId = 0;
        int headerId = 0;
        int inclMistrial = 0;
        int index = 0;
        int calcuTypeVal = 0;
        int iiGraph = 0;
        if (rbtnCalcuType.SelectedValue == "1")
        {
            calcuTypeVal = 1;
        }
        else if (rbtnCalcuType.SelectedValue == "0")
        {
            calcuTypeVal = 0;
        }
        sess = (clsSession)Session["UserSession"];
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            if (sess != null)
            {
                if (!objData.IFExists("select DSTempSetColId from DSTempSetCol where ColName='" + clsGeneral.convertQuotes(txtColumnName.Text) + "' AND DSTempHdrId=" + headerId + " AND ActiveInd = 'A'"))
                {
                    if (ddlColumnType.SelectedValue == "+/-")
                    {
                        clsData.blnTrans = true;
                        Transs = con.BeginTransaction();
                        try
                        {
                            if (chkplusIncludeMistrial.Checked == true)
                            {
                                inclMistrial = 1;
                            }
                            insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc " +
                                  ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,MoveUpstat) " +
                                  "Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "','" + Convert.ToString(ddlColumnType.SelectedValue) + "' ,'" + Convert.ToString(rdbplusMinus.SelectedValue) + "','" + clsGeneral.convertQuotes(txtplusCorrectResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtPlusIncorrectResp.Text.Trim()) + "'," + inclMistrial + ",'" + clsGeneral.convertQuotes(txtplusIncludeMistrial.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + 1 + ") ";
                            returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                            if (chkplusAccuracy.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkplusAccuracyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkplusAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtplusAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkPlusPromptPerc.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPlusPromptPercIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkPlusPromptPerc.Text) + "','" + clsGeneral.convertQuotes(txtPlusPromptPerc.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            if (chkplusindependent.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkplusindependentIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkplusindependent.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkplusindependentForAll.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkplusindependentForAllIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkplusindependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            if (chkpluslearnedStep.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkpluslearnedStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtpluslearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkCurrentStep.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkCurrentStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtExCurrentStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            if (chkplustotalcorrect.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkplustotalcorrectIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkplustotalcorrect.Text) + "','" + clsGeneral.convertQuotes(txtplustotalcorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            if (chkplustotalIncorrect.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkplustotalIncorrectIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkplustotalIncorrect.Text) + "','" + clsGeneral.convertQuotes(txtPlustotalIncorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            objData.CommitTransation(Transs, con);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                            GetMeasureData(headerId);
                            //  SetLessonProcedure(headerId);

                        }
                        catch (Exception Ex)
                        {
                            objData.RollBackTransation(Transs, con);
                            string error = Ex.Message;
                            tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }
                    }
                    else if (ddlColumnType.SelectedValue == "Prompt")
                    {
                        int currentPrompt = 0;

                        clsData.blnTrans = true;
                        Transs = con.BeginTransaction();

                        try
                        {
                            if (chkPromptInclMisTrial.Checked == true)
                            {
                                inclMistrial = 1;
                            }
                            if (chkCurrentPrompt.Checked == false)
                            {
                                if (ddlPromptList.SelectedIndex > 0)
                                {
                                    currentPrompt = Convert.ToInt32(ddlPromptList.SelectedValue);
                                }
                                else
                                {
                                    currentPrompt = 0;
                                }
                            }


                            insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc " +
                                   ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,MoveUpstat) " +
                                   "Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "','" + Convert.ToString(ddlColumnType.SelectedValue) + "' ," + currentPrompt + ",'" + clsGeneral.convertQuotes(txtpromptSelectPrompt.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtPromptIncrctResp.Text.Trim()) + "'," + inclMistrial + ",'" + clsGeneral.convertQuotes(txtPromptIncMisTrial.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + 1 + ") ";
                            returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                            if (chkPrompPercAccuracy.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPrompPercAccuracyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                               "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkPrompPercAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            if (chkPromptPercPrompt.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPromptPercPromptIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkPromptPercPrompt.Text) + "','" + clsGeneral.convertQuotes(txtPromptpecPrompt.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkPercIndependent.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPercIndependentIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkPercIndependent.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkPercIndependentForAll.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPercIndependentForAllIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkPercIndependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkPromptAccLearnedStep.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPromptAccLearnedStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccLearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkPromptAccExcluseCrntStep.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkPromptAccExcluseCrntStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccExcluseCrntStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            objData.CommitTransation(Transs, con);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                            GetMeasureData(headerId);
                            // SetLessonProcedure(headerId);
                        }
                        catch (Exception Ex)
                        {
                            objData.RollBackTransation(Transs, con);
                            string error = Ex.Message;
                            tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }

                    }
                    else if (ddlColumnType.SelectedValue == "Text")
                    {
                        clsData.blnTrans = true;
                        Transs = con.BeginTransaction();

                        try
                        {
                            if (chkTxtIncMisTrial.Checked == true)
                            {
                                inclMistrial = 1;
                            }

                            insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc,CalcuType,CalcuData " +
                                   ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,MoveUpstat) " +
                                   "Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "','" + Convert.ToString(ddlColumnType.SelectedValue) + "' ,'" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtTextInCrctResp.Text.Trim()) + "'," + inclMistrial + ",'" + clsGeneral.convertQuotes(txtTxtIncMisTrial.Text.Trim()) + "'," + calcuTypeVal + ",'" + clsGeneral.convertQuotes(txtCalcuType.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + 1 + ") ";
                            returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                            if (chkTxtNa.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkTxtNaIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkTxtNa.Text) + "','" + clsGeneral.convertQuotes(txtTxtNA.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkTextCustomize.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkTextCustomizeIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcFormula,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkTextCustomize.Text) + "','" + clsGeneral.convertQuotes(txtTxtCustomize.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            objData.CommitTransation(Transs, con);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                            GetMeasureData(headerId);
                            // SetLessonProcedure(headerId);
                        }
                        catch (Exception Ex)
                        {
                            objData.RollBackTransation(Transs, con);
                            string error = Ex.Message;
                            tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }
                    }
                    else if (ddlColumnType.SelectedValue == "Duration")
                    {
                        clsData.blnTrans = true;
                        Transs = con.BeginTransaction();

                        try
                        {
                            if (chkDurIncludeMistrial.Checked == true)
                            {
                                inclMistrial = 1;
                            }

                            insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc " +
                                   ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,MoveUpstat) " +
                                   "Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "','" + Convert.ToString(ddlColumnType.SelectedValue) + "' ,'" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtDurIncrctResp.Text.Trim()) + "'," + inclMistrial + ",'" + clsGeneral.convertQuotes(txtDurInclMisTrial.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A',"+Convert.ToInt16(MoveupOpt1.SelectedValue)+") ";
                            returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                            if (chkDurAverage.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkDurAverageIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkDurAverage.Text) + "','" + clsGeneral.convertQuotes(txtDurAverage.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }
                            if (chkDurTotalDur.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkDurTotalDurIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                              "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkDurTotalDur.Text) + "','" + clsGeneral.convertQuotes(txtDurTotalDuration.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            objData.CommitTransation(Transs, con);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                            GetMeasureData(headerId);
                            // SetLessonProcedure(headerId);
                        }
                        catch (Exception Ex)
                        {
                            objData.RollBackTransation(Transs, con);
                            string error = Ex.Message;
                            tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }
                    }
                    else if (ddlColumnType.SelectedValue == "Frequency")
                    {
                        clsData.blnTrans = true;
                        Transs = con.BeginTransaction();

                        try
                        {
                            if (chkFreqIncludeMistrial.Checked == true)
                            {
                                inclMistrial = 1;
                            }

                            insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc " +
                                   ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,MoveUpstat) " +
                                   "Values(" + sess.SchoolId + "," + headerId + ",'" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "','" + Convert.ToString(ddlColumnType.SelectedValue) + "' ,'" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtfreqIncrctResp.Text.Trim()) + "'," + inclMistrial + ",'" + clsGeneral.convertQuotes(txtFreqIncludeMistrial.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A',"+Convert.ToInt16(MoveupOpt.SelectedValue)+") ";
                            returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                            if (chkFrequency.Checked == true)
                            {
                                iiGraph = 0;
                                if (chkFrequencyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                "Values(" + sess.SchoolId + "," + returnId + ",'" + Convert.ToString(chkFrequency.Text) + "','" + clsGeneral.convertQuotes(txtFrequency.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                            }

                            objData.CommitTransation(Transs, con);
                            ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                            GetMeasureData(headerId);
                            //  SetLessonProcedure(headerId);
                        }
                        catch (Exception Ex)
                        {
                            objData.RollBackTransation(Transs, con);
                            string error = Ex.Message;
                            tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
                        }
                    }
                    showMatchToSampleDrop();
                }
                else
                {
                    tdMsgMeasure.InnerHtml = clsGeneral.warningMsg("Same column name exist for the template");
                    txtColumnName.Focus();
                }

            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void BtnUpdateMeasure_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int columnId = 0;
        string updateQuerry = "";
        string updateQuerryIIG = "";
        string insertQuerry = "";
        string deltQuerry = "";
        SqlTransaction Transs = null;
        int returnId = 0;
        int inclMistrial = 0;
        int index = 0;
        int headerId = 0;
        string measureValue = "";
        int iiGraph = 0;
        int MoveUpstat=1;
        sess = (clsSession)Session["UserSession"];
        if (ViewState["EditValue"] != null)
        {
            columnId = Convert.ToInt32(ViewState["EditValue"]);
        }
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        int calcuTypeVal = 0;
        if (rbtnCalcuType.SelectedValue == "1")
        {
            calcuTypeVal = 1;
        }
        else if (rbtnCalcuType.SelectedValue == "0")
        {
            calcuTypeVal = 0;
        }

        try
        {
            if (sess != null)
            {
                if (!objData.IFExists("select DSTempSetColId from DSTempSetCol where ColName='" + clsGeneral.convertQuotes(txtColumnName.Text) + "' AND DSTempHdrId=" + headerId + " AND DSTempSetColId <> " + columnId + " AND ActiveInd = 'A'"))
                {
                    string selQuerry = "SELECT DSTempRuleId FROM DSTempRule WHERE DSTempSetColId = " + columnId + " AND ActiveInd = 'A'";        // Check the currrent column assigned to any criteria. If so, it will ignore the updation.
                    DataTable dtCheckData = objData.ReturnDataTable(selQuerry, false);

                    SqlConnection con = objData.Open();
                    clsData.blnTrans = true;
                    Transs = con.BeginTransaction();
                    if (ddlColumnType.SelectedValue == "Frequency" || ddlColumnType.SelectedValue == "Duration")
                        MoveUpstat = Convert.ToInt16( MoveupOpt.SelectedValue);
                    if (ddlColumnType.SelectedValue == "Duration")
                        MoveUpstat = Convert.ToInt16(MoveupOpt1.SelectedValue);
                    updateQuerry = "UPDATE DSTempSetCol SET ColName = '" + clsGeneral.convertQuotes(txtColumnName.Text.Trim()) + "', ColTypeCd = '" + Convert.ToString(ddlColumnType.SelectedValue) + "',ModifiedBy = " + sess.LoginId + ",ModifiedOn = getdate(),MoveUpstat="+MoveUpstat+" WHERE DSTempSetColId = " + columnId;
                    returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));

                    if (dtCheckData != null && dtCheckData.Rows.Count > 0)
                    {

                        if (ddlColumnType.SelectedValue == "+/-")
                        {
                            try
                            {
                                if (chkplusIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + Convert.ToString(rdbplusMinus.SelectedValue) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtplusCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtPlusIncorrectResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtplusIncludeMistrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));

                                iiGraph = 0;
                                if (chkplusAccuracyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkplusAccuracy.Enabled == true)
                                {
                                    measureValue = "%Accuracy";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                                    if (chkplusAccuracy.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtplusAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + " where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPlusPromptPercIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                    //update iigraph
                                }
                                if (chkPlusPromptPerc.Enabled == true)
                                {
                                    measureValue = "%Prompted";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                                    if (chkPlusPromptPerc.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPlusPromptPerc.Text) + "','" + clsGeneral.convertQuotes(txtPlusPromptPerc.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Prompted";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkplusindependentIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkplusindependent.Enabled == true)
                                {
                                    measureValue = "%Independent";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkplusindependent.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusindependent.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Independent";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkplusindependentForAllIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkplusindependentForAll.Enabled == true)
                                {
                                    measureValue = "%Independent of All Steps";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkplusindependentForAll.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusindependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Independent of All Steps";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkpluslearnedStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkpluslearnedStep.Enabled == true)
                                {
                                    measureValue = "%Accuracy at Training Step";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkpluslearnedStep.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtpluslearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy at Training Step";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkCurrentStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkCurrentStep.Enabled == true)
                                {
                                    measureValue = "%Accuracy at Previously Learned Steps";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkCurrentStep.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtExCurrentStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy at Previously Learned Steps";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }

                                iiGraph = 0;
                                if (chkplustotalcorrectIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkplustotalcorrect.Enabled == true)
                                {
                                    measureValue = "Total Correct";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                                    if (chkplustotalcorrect.Checked == true)
                                    {
                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplustotalcorrect.Text) + "','" + clsGeneral.convertQuotes(txtplustotalcorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Total Correct";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }

                                iiGraph = 0;
                                if (chkplustotalIncorrectIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkplustotalIncorrect.Enabled == true)
                                {
                                    measureValue = "Total Incorrect";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                                    if (chkplustotalIncorrect.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplustotalIncorrect.Text) + "','" + clsGeneral.convertQuotes(txtPlustotalIncorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Total Incorrect";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                // SetLessonProcedure(headerId);

                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }


                        else if (ddlColumnType.SelectedValue == "Prompt")
                        {
                            int currentPrompt = 0;
                            try
                            {
                                if (chkPromptInclMisTrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }
                                if (chkCurrentPrompt.Checked == false)
                                {
                                    if (ddlPromptList.SelectedIndex > 0)
                                    {
                                        currentPrompt = Convert.ToInt32(ddlPromptList.SelectedValue);
                                    }
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = " + currentPrompt + " ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtpromptSelectPrompt.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtPromptIncrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtPromptIncMisTrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));
                                iiGraph = 0;
                                if (chkPrompPercAccuracyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPrompPercAccuracy.Enabled == true)
                                {
                                    measureValue = "%Accuracy";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                                    if (chkPrompPercAccuracy.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                       "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPrompPercAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPromptPercPromptIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPromptPercPrompt.Enabled == true)
                                {
                                    measureValue = "%Prompted";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkPromptPercPrompt.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPromptPercPrompt.Text) + "','" + clsGeneral.convertQuotes(txtPromptpecPrompt.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Prompted";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPercIndependentIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPercIndependent.Enabled == true)
                                {
                                    measureValue = "%Independent";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkPercIndependent.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                      "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPercIndependent.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Independent";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPercIndependentForAllIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPercIndependentForAll.Enabled == true)
                                {
                                    measureValue = "%Independent of All Steps";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkPercIndependentForAll.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                      "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPercIndependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Independent of All Steps";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPromptAccLearnedStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPromptAccLearnedStep.Enabled == true)
                                {
                                    measureValue = "%Accuracy at Training Step";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkPromptAccLearnedStep.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtpluslearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy at Training Step";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkPromptAccExcluseCrntStepIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkPromptAccExcluseCrntStep.Enabled == true)
                                {
                                    measureValue = "%Accuracy at Previously Learned Steps";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkPromptAccExcluseCrntStep.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtExCurrentStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "%Accuracy at Previously Learned Steps";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //  SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }

                        }
                        else if (ddlColumnType.SelectedValue == "Text")
                        {

                            try
                            {
                                if (chkTxtIncMisTrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }


                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtTextInCrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtTxtIncMisTrial.Text.Trim()) + "', CalcuType=" + calcuTypeVal + ", CalcuData='" + clsGeneral.convertQuotes(txtCalcuType.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));
                                iiGraph = 0;
                                if (chkTxtNaIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkTxtNa.Enabled == true)
                                {
                                    measureValue = "NA";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkTxtNa.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkTxtNa.Text) + "','" + clsGeneral.convertQuotes(txtTxtNA.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "NA";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkTextCustomizeIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkTextCustomize.Enabled == true)
                                {
                                    measureValue = "Customize";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkTextCustomize.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcFormula,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                      "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkTextCustomize.Text) + "','" + clsGeneral.convertQuotes(txtTxtCustomize.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Customize";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                // SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                        else if (ddlColumnType.SelectedValue == "Duration")
                        {

                            try
                            {
                                if (chkDurIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtDurIncrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtDurInclMisTrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));
                                iiGraph = 0;
                                if (chkDurAverageIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkDurAverage.Enabled == true)
                                {
                                    measureValue = "Avg Duration";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkDurAverage.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkDurAverage.Text) + "','" + clsGeneral.convertQuotes(txtDurAverage.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Avg Duration";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                iiGraph = 0;
                                if (chkDurTotalDurIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkDurTotalDur.Enabled == true)
                                {
                                    measureValue = "Total Duration";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkDurTotalDur.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                      "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkDurTotalDur.Text) + "','" + clsGeneral.convertQuotes(txtDurTotalDuration.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Total Duration";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                // SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                        else if (ddlColumnType.SelectedValue == "Frequency")
                        {

                            try
                            {
                                if (chkFreqIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtfreqIncrctResp.Text.Trim()) + "',IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtFreqIncludeMistrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));
                                iiGraph = 0;
                                if (chkFrequencyIIG.Checked == true)
                                {
                                    iiGraph = 1;
                                }
                                if (chkFrequency.Enabled == true)
                                {
                                    measureValue = "Frequency";
                                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureValue + "' And ActiveInd = 'A'";
                                    index = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));
                                    if (chkFrequency.Checked == true)
                                    {

                                        insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                        "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkFrequency.Text) + "','" + clsGeneral.convertQuotes(txtFrequency.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                        index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                    }
                                }
                                else
                                {
                                    measureValue = "Frequency";
                                    updateQuerryIIG = "update DSTempSetColCalc set IncludeInGraph=" + iiGraph + "where DSTempSetColId=" + columnId + " and CalcType='" + measureValue + "' and ActiveInd='A'";
                                    objData.Execute(updateQuerryIIG);
                                }
                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //  SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                    }
                    else
                    {

                        deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId;
                        returnId = Convert.ToInt32(objData.ExecuteWithTrans(deltQuerry, con, Transs));

                        if (ddlColumnType.SelectedValue == "+/-")
                        {
                            try
                            {
                                if (chkplusIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + Convert.ToString(rdbplusMinus.SelectedValue) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtplusCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtPlusIncorrectResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtplusIncludeMistrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));

                                if (chkplusAccuracy.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkplusAccuracyIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtplusAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkPlusPromptPerc.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPlusPromptPercIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPlusPromptPerc.Text) + "','" + clsGeneral.convertQuotes(txtPlusPromptPerc.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkplusindependent.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkplusindependentIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusindependent.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkplusindependentForAll.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkplusindependentForAllIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplusindependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtplusIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                if (chkpluslearnedStep.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkpluslearnedStepIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtpluslearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkCurrentStep.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkCurrentStepIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtExCurrentStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                if (chkplustotalcorrect.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkplustotalcorrectIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplustotalcorrect.Text) + "','" + clsGeneral.convertQuotes(txtplustotalcorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                if (chkplustotalIncorrect.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkplustotalIncorrectIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkplustotalIncorrect.Text) + "','" + clsGeneral.convertQuotes(txtPlustotalIncorrect.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //  SetLessonProcedure(headerId);

                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                        else if (ddlColumnType.SelectedValue == "Prompt")
                        {
                            int currentPrompt = 0;
                            try
                            {
                                if (chkPromptInclMisTrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }
                                if (chkCurrentPrompt.Checked == false)
                                {
                                    if (ddlPromptList.SelectedIndex > 0)
                                    {
                                        currentPrompt = Convert.ToInt32(ddlPromptList.SelectedValue);
                                    }
                                    else
                                    {
                                        currentPrompt = 0;
                                    }
                                }


                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = " + currentPrompt + " ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtpromptSelectPrompt.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtPromptIncrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtPromptIncMisTrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));


                                if (chkPrompPercAccuracy.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPrompPercAccuracyIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                   "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPrompPercAccuracy.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccuracy.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                if (chkPromptPercPrompt.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPromptPercPromptIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPromptPercPrompt.Text) + "','" + clsGeneral.convertQuotes(txtPromptpecPrompt.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkPercIndependent.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPercIndependentIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                  "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPercIndependent.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependent.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkPercIndependentForAll.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPercIndependentForAllIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                  "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkPercIndependentForAll.Text) + "','" + clsGeneral.convertQuotes(txtPromptIndependentForAll.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkPromptAccLearnedStep.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPromptAccLearnedStepIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkpluslearnedStep.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccLearnedStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkPromptAccExcluseCrntStep.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkPromptAccExcluseCrntStepIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                  "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkCurrentStep.Text) + "','" + clsGeneral.convertQuotes(txtPromptAccExcluseCrntStep.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //   SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }

                        }
                        else if (ddlColumnType.SelectedValue == "Text")
                        {

                            try
                            {
                                if (chkTxtIncMisTrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }


                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtTextCrctResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtTextInCrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtTxtIncMisTrial.Text.Trim()) + "', CalcuType=" + calcuTypeVal + ", CalcuData='" + clsGeneral.convertQuotes(txtCalcuType.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));


                                if (chkTxtNa.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkTxtNaIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkTxtNa.Text) + "','" + clsGeneral.convertQuotes(txtTxtNA.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkTextCustomize.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkTextCustomizeIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcFormula,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                  "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkTextCustomize.Text) + "','" + clsGeneral.convertQuotes(txtTxtCustomize.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //   SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                        else if (ddlColumnType.SelectedValue == "Duration")
                        {

                            try
                            {
                                if (chkDurIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtDurCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtDurIncrctResp.Text.Trim()) + "' ,IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtDurInclMisTrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));


                                if (chkDurAverage.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkDurAverageIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkDurAverage.Text) + "','" + clsGeneral.convertQuotes(txtDurAverage.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }
                                if (chkDurTotalDur.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkDurTotalDurIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                  "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkDurTotalDur.Text) + "','" + clsGeneral.convertQuotes(txtDurTotalDuration.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                // SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                        else if (ddlColumnType.SelectedValue == "Frequency")
                        {

                            try
                            {
                                if (chkFreqIncludeMistrial.Checked == true)
                                {
                                    inclMistrial = 1;
                                }

                                updateQuerry = "UPDATE DSTempSetCol SET CorrResp = '" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "' ,CorrRespDesc = '" + clsGeneral.convertQuotes(txtFreqCorrectResponse.Text.Trim()) + "',InCorrRespDesc = '" + clsGeneral.convertQuotes(txtfreqIncrctResp.Text.Trim()) + "',IncMisTrialInd = " + inclMistrial + ", MisTrialDesc = '" + clsGeneral.convertQuotes(txtFreqIncludeMistrial.Text.Trim()) + "' WHERE DSTempSetColId = " + columnId;
                                returnId = Convert.ToInt32(objData.ExecuteWithTrans(updateQuerry, con, Transs));


                                if (chkFrequency.Checked == true)
                                {
                                    iiGraph = 0;
                                    if (chkFrequencyIIG.Checked == true)
                                    {
                                        iiGraph = 1;
                                    }
                                    insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd,IncludeInGraph) " +
                                                    "Values(" + sess.SchoolId + "," + columnId + ",'" + Convert.ToString(chkFrequency.Text) + "','" + clsGeneral.convertQuotes(txtFrequency.Text.Trim()) + "'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A'," + iiGraph + ") ";
                                    index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));
                                }

                                objData.CommitTransation(Transs, con);
                                ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "CloseMeasure();", true);
                                GetMeasureData(headerId);
                                //SetLessonProcedure(headerId);
                            }
                            catch (Exception Ex)
                            {
                                objData.RollBackTransation(Transs, con);
                                string error = Ex.Message;
                                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Updation Failed! <br> '" + error + "' ");
                            }
                        }
                    }
                }
                else
                {
                    tdMsgMeasure.InnerHtml = clsGeneral.warningMsg("Same column name exist for the template");
                    txtColumnName.Focus();
                }
                showMatchToSampleDrop();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        showMatchToSampleDrop();
    }


    //protected void DeltSelctdMeasure(int columnId, string measureData)
    //{
    //    objData = new clsData();
    //    string deltQuerry = "";
    //    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND CalcType = '" + measureData + "' And ActiveInd = 'A'";
    //    objData.Execute(deltQuerry);
    //}

    protected void BtnRemove_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int columnId = 0;
        int index = 0;
        int headerId = 0;
        string deltQuerry = "";
        Button BtnDelt = (Button)sender;
        columnId = Convert.ToInt32(BtnDelt.CommandArgument);
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {

            string selQuerry = "SELECT DSTempRuleId FROM DSTempRule WHERE DSTempSetColId = " + columnId + " AND ActiveInd = 'A'";
            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    ScriptManager.RegisterClientScriptBlock(UpdatePanel1, UpdatePanel1.GetType(), "", "AlertNotDelete();", true);
                }
                else
                {

                    deltQuerry = "Update DSTempSetCol SET ActiveInd = 'D'  WHERE DSTempSetColId = " + columnId;
                    index = objData.Execute(deltQuerry);
                    deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId;
                    index = objData.Execute(deltQuerry);
                }
            }
            else
            {
                deltQuerry = "Update DSTempSetCol SET ActiveInd = 'D'  WHERE DSTempSetColId = " + columnId;
                index = objData.Execute(deltQuerry);
                deltQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId;
                index = objData.Execute(deltQuerry);
            }

            GetMeasureData(headerId);   // Bind the DataList Measure Details
            //  SetLessonProcedure(headerId);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }


    }


    protected void btnEditMeasure_Click(object sender, EventArgs e)
    {
        int columnId = 0;
        try
        {
            Button btnEdit = (Button)sender;
            columnId = Convert.ToInt32(btnEdit.CommandArgument);
            ViewState["EditValue"] = columnId;
            BtnSaveMeasure.Visible = false;
            if (Hdfsavemeasure.Value != "")
            {
                viewMeasures();
            }
            else
            {
                BtnUpdateMeasure.Visible = true;
                ClearData();
            }
            EditMeasureData(columnId);
            ViewState["New"] = false;
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "EditMeasurePopup();", true);
            rbtnCalcuType_SelectedIndexChanged(sender, e);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void viewMeasures()
    {
        BtnUpdateMeasure.Visible = false;
        txtColumnName.ReadOnly = true;
        rdbplusMinus.Enabled = false;
        txtplusCorrectResponse.ReadOnly = true;
        txtPlusIncorrectResp.ReadOnly = true;
        chkplusAccuracy.Enabled = false;
        txtplusAccuracy.ReadOnly = true;
        chkplusAccuracyIIG.Enabled = false;
        chkPlusPromptPerc.Enabled = false;
        txtPlusPromptPerc.ReadOnly = true;
        chkPlusPromptPercIIG.Enabled = false;
        chkplusindependent.Enabled = false;
        txtplusIndependent.ReadOnly = true;
        chkplusindependentIIG.Enabled = false;
        chkplusindependentForAll.Enabled = false;
        txtplusIndependentForAll.ReadOnly = true;
        chkplusindependentForAllIIG.Enabled = false;
        chkplustotalcorrect.Enabled = false;
        txtplustotalcorrect.ReadOnly = true;
        chkplustotalcorrectIIG.Enabled = false;
        chkplustotalIncorrect.Enabled = false;
        txtPlustotalIncorrect.ReadOnly = true;
        chkplustotalIncorrectIIG.Enabled = false;
        chkpluslearnedStep.Enabled = false;
        txtpluslearnedStep.ReadOnly = true;
        chkpluslearnedStepIIG.Enabled = false;
        chkCurrentStep.Enabled = false;
        txtExCurrentStep.ReadOnly = true;
        chkCurrentStepIIG.Enabled = false;

        chkCurrentPrompt.Enabled = false;
        ddlPromptList.Enabled = false;
        txtpromptSelectPrompt.ReadOnly = true;
        txtPromptIncrctResp.ReadOnly = true;
        chkPrompPercAccuracy.Enabled = false;
        txtPromptAccuracy.ReadOnly = true;
        chkPrompPercAccuracyIIG.Enabled = false;
        chkPromptPercPrompt.Enabled = false;
        txtPromptpecPrompt.ReadOnly = true;
        chkPromptPercPromptIIG.Enabled = false;
        chkPercIndependent.Enabled = false;
        chkPercIndependentIIG.Enabled = false;
        txtPromptIndependent.ReadOnly = true;
        chkPercIndependentForAll.Enabled = false;
        chkPercIndependentForAllIIG.Enabled = false;
        txtPromptIndependentForAll.ReadOnly = true;
        chkPromptAccLearnedStep.Enabled = false;
        txtPromptAccLearnedStep.ReadOnly = true;
        chkPromptAccLearnedStepIIG.Enabled = false;
        chkPromptAccExcluseCrntStep.Enabled = false;
        txtPromptAccExcluseCrntStep.ReadOnly = true;
        chkPromptAccExcluseCrntStepIIG.Enabled = false;

        chkTxtNa.Enabled = false;
        txtTxtNA.ReadOnly = true;
        chkTxtNaIIG.Enabled = false;
        chkTextCustomize.Enabled = false;
        txtTxtCustomize.ReadOnly = true;
        chkTextCustomizeIIG.Enabled = false;
        rbtnCalcuType.Enabled = false;
        txtCalcuType.ReadOnly = true;
        txtTextCrctResponse.ReadOnly = true;
        txtTextInCrctResp.ReadOnly = true;

        txtDurCorrectResponse.ReadOnly = true;
        txtDurIncrctResp.ReadOnly = true;
        chkDurAverage.Enabled = false;
        txtDurAverage.ReadOnly = true;
        chkDurAverageIIG.Enabled = false;
        chkDurTotalDur.Enabled = false;
        txtDurTotalDuration.ReadOnly = true;
        chkDurTotalDurIIG.Enabled = false;

        txtFreqCorrectResponse.ReadOnly = true;
        txtfreqIncrctResp.ReadOnly = true;
        chkFrequency.Enabled = false;
        txtFrequency.ReadOnly = true;
        chkFrequencyIIG.Enabled = false;
    }

    protected void EditMeasureData(int columnId)
    {
        objData = new clsData();
        string selQuerry = "";
        string columnType = "";
        string calcType = "";
        string calcDesc = "";
        string calcFormula = "";
        int inclMistrial = 0;
        string mistrialDesc = "";
        string crctResponse = "";
        string crctResponseDesc = "";
        string incorrctResp = "";
        int colCalcId = 0;
        int calcuTypeVal = 0;
        string calcuData = "";
        bool isIIGraph = false;
        int MoveupStat = 1;
        aoTb.Style.Add("display", "none");
        aoTb2.Style.Add("display", "none");
        promptAOTb.Style.Add("display", "none");
        promptAOTb2.Style.Add("display", "none");
        opdiv1.Style.Add("display", "none");
        opdiv1.Style.Add("display", "none");
        textAOTb.Style.Add("display", "none");
        durationAOTb.Style.Add("display", "none");
        frequencyAOTb.Style.Add("display", "none");
        try
        {

            selQuerry = "SELECT ColName,ColTypeCd,CorrResp,CorrRespDesc,InCorrRespDesc,IncMisTrialInd,MisTrialDesc,CalcuType,CalcuData,MoveUpstat FROM DSTempSetCol WHERE DSTempSetColId = " + columnId + " AND ActiveInd = 'A'";
            DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
            if (dtNew != null)
            {
                if (dtNew.Rows.Count > 0)
                {
                    txtColumnName.Text = dtNew.Rows[0]["ColName"].ToString();
                    inclMistrial = Convert.ToInt32(dtNew.Rows[0]["IncMisTrialInd"]);
                    mistrialDesc = dtNew.Rows[0]["MisTrialDesc"].ToString();
                    crctResponse = dtNew.Rows[0]["CorrResp"].ToString();
                    crctResponseDesc = dtNew.Rows[0]["CorrRespDesc"].ToString();
                    incorrctResp = dtNew.Rows[0]["InCorrRespDesc"].ToString();
                    if (dtNew.Rows[0]["MoveUpstat"].ToString() != "")
                        MoveupStat = Convert.ToInt16(dtNew.Rows[0]["MoveUpstat"]);
                    if (crctResponseDesc != "" || incorrctResp != "")
                    {
                        aoTb.Style.Add("display", "block");
                    }
                    columnType = dtNew.Rows[0]["ColTypeCd"].ToString();
                    try
                    {
                        calcuTypeVal = Convert.ToInt32(dtNew.Rows[0]["CalcuType"]);
                    }
                    catch
                    {
                        calcuTypeVal = 1;
                    }
                    calcuData = dtNew.Rows[0]["CalcuData"].ToString();
                    txtCalcuType.Text = calcuData.ToString();
                    rbtnCalcuType.SelectedIndex = calcuTypeVal;
                    ddlColumnType.SelectedValue = columnType;
                    DivMeasureVisibility(columnType);
                    selQuerry = "SELECT DSTempSetColCalcId,CalcType,CalcRptLabel,CalcFormula,IncludeInGraph FROM DSTempSetColCalc WHERE DSTempSetColId = " + columnId + " AND ActiveInd = 'A'";
                    DataTable dtList = objData.ReturnDataTable(selQuerry, false);

                    if (dtList != null)
                    {
                        if (dtList.Rows.Count > 0)
                        {
                            if (columnType == "+/-")
                            {
                                if (crctResponse == "+")
                                {
                                    rdbplusMinus.SelectedValue = "+";
                                }
                                else rdbplusMinus.SelectedValue = "-";
                                txtplusCorrectResponse.Text = crctResponseDesc.ToString();
                                txtPlusIncorrectResp.Text = incorrctResp.ToString();

                                if (inclMistrial == 1)
                                {
                                    chkplusIncludeMistrial.Checked = true;
                                    txtplusIncludeMistrial.Text = mistrialDesc;
                                }
                                for (int i = 0; i < dtList.Rows.Count; i++)
                                {
                                    calcType = dtList.Rows[i]["CalcType"].ToString();
                                    calcDesc = dtList.Rows[i]["CalcRptLabel"].ToString();
                                    colCalcId = Convert.ToInt32(dtList.Rows[i]["DSTempSetColCalcId"]);

                                    if (dtList.Rows[i]["IncludeInGraph"] is bool)
                                    {
                                        isIIGraph = Convert.ToBoolean(dtList.Rows[i]["IncludeInGraph"]);
                                    }

                                    if (calcType == "%Accuracy")
                                    {
                                        chkplusAccuracy.Checked = true;
                                        txtplusAccuracy.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkplusAccuracy.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkplusAccuracyIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Prompted")
                                    {
                                        chkPlusPromptPerc.Checked = true;
                                        txtPlusPromptPerc.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPlusPromptPerc.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPlusPromptPercIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Independent")
                                    {
                                        chkplusindependent.Checked = true;
                                        txtplusIndependent.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkplusindependent.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkplusindependentIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Independent of All Steps")
                                    {
                                        chkplusindependentForAll.Checked = true;
                                        txtplusIndependentForAll.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkplusindependentForAll.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkplusindependentForAllIIG.Checked = true;
                                        }
                                    }

                                    if (calcType == "%Accuracy at Training Step")
                                    {
                                        chkpluslearnedStep.Checked = true;
                                        txtpluslearnedStep.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkpluslearnedStep.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkpluslearnedStepIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Accuracy at Previously Learned Steps")
                                    {
                                        chkCurrentStep.Checked = true;
                                        txtExCurrentStep.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkCurrentStep.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkCurrentStepIIG.Checked = true;
                                        }
                                    }
                                    if (chkplusindependentForAll.Checked == true || chkpluslearnedStep.Checked == true || chkCurrentStep.Checked == true)
                                    {
                                        aoTb2.Style.Add("display", "block");
                                        opdiv1.Style.Add("display", "block");
                                    }

                                    if (calcType == "Total Correct")
                                    {
                                        chkplustotalcorrect.Checked = true;
                                        txtplustotalcorrect.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkplustotalcorrect.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkplustotalcorrectIIG.Checked = true;
                                        }
                                    }

                                    if (calcType == "Total Incorrect")
                                    {
                                        chkplustotalIncorrect.Checked = true;
                                        txtPlustotalIncorrect.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkplustotalIncorrect.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkplustotalIncorrectIIG.Checked = true;
                                        }
                                    }



                                }
                            }

                            else if (columnType == "Prompt")
                            {
                                FillAllPrompts();
                                int promptcorrectResp = Convert.ToInt32(crctResponse);
                                if (inclMistrial == 1)
                                {
                                    chkPromptInclMisTrial.Checked = true;
                                    txtPromptIncMisTrial.Text = mistrialDesc;
                                }
                                if (promptcorrectResp > 0)
                                {
                                    chkCurrentPrompt.Checked = false;
                                    divCurrentPrompt.Visible = true;
                                    ddlPromptList.SelectedValue = promptcorrectResp.ToString();
                                }
                                else
                                {
                                    chkCurrentPrompt.Checked = true;
                                    divCurrentPrompt.Visible = false;
                                }
                                txtpromptSelectPrompt.Text = crctResponseDesc;
                                txtPromptIncrctResp.Text = incorrctResp.ToString();
                                if (crctResponseDesc != "" || incorrctResp != "")
                                {
                                    promptAOTb.Style.Add("display", "block");
                                }
                                for (int i = 0; i < dtList.Rows.Count; i++)
                                {
                                    calcType = dtList.Rows[i]["CalcType"].ToString();
                                    calcDesc = dtList.Rows[i]["CalcRptLabel"].ToString();
                                    colCalcId = Convert.ToInt32(dtList.Rows[i]["DSTempSetColCalcId"]);
                                    if (dtList.Rows[i]["IncludeInGraph"] is bool)
                                    {
                                        isIIGraph = Convert.ToBoolean(dtList.Rows[i]["IncludeInGraph"]);
                                    }
                                    if (calcType == "%Accuracy")
                                    {
                                        chkPrompPercAccuracy.Checked = true;
                                        txtPromptAccuracy.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPrompPercAccuracy.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPrompPercAccuracyIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Prompted")
                                    {
                                        chkPromptPercPrompt.Checked = true;
                                        txtPromptpecPrompt.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPromptPercPrompt.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPromptPercPromptIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Independent")
                                    {
                                        chkPercIndependent.Checked = true;
                                        txtPromptIndependent.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPercIndependent.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPercIndependentIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Independent of All Steps")
                                    {
                                        chkPercIndependentForAll.Checked = true;
                                        txtPromptIndependentForAll.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPercIndependentForAll.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPercIndependentForAllIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Accuracy at Training Step")
                                    {
                                        chkPromptAccLearnedStep.Checked = true;
                                        txtPromptAccLearnedStep.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPromptAccLearnedStep.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPromptAccLearnedStepIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "%Accuracy at Previously Learned Steps")
                                    {
                                        chkPromptAccExcluseCrntStep.Checked = true;
                                        txtPromptAccExcluseCrntStep.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkPromptAccExcluseCrntStep.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkPromptAccExcluseCrntStepIIG.Checked = true;
                                        }
                                    }
                                    if (chkPercIndependentForAll.Checked == true || chkPromptAccLearnedStep.Checked == true || chkPromptAccExcluseCrntStep.Checked == true)
                                    {
                                        promptAOTb2.Style.Add("display", "block");
                                        opdiv2.Style.Add("display", "block");
                                    }
                                }

                            }
                            else if (columnType == "Text")
                            {
                                txtTextCrctResponse.Text = crctResponseDesc.ToString();
                                txtTextInCrctResp.Text = incorrctResp.ToString();
                                if (txtTextCrctResponse.Text != "" || txtTextInCrctResp.Text != "")
                                {
                                    textAOTb.Style.Add("display", "block");
                                }

                                if (inclMistrial == 1)
                                {
                                    chkTxtIncMisTrial.Checked = true;
                                    txtTxtIncMisTrial.Text = mistrialDesc;
                                }

                                for (int i = 0; i < dtList.Rows.Count; i++)
                                {
                                    calcType = dtList.Rows[i]["CalcType"].ToString();
                                    calcDesc = dtList.Rows[i]["CalcRptLabel"].ToString();
                                    calcFormula = dtList.Rows[i]["CalcFormula"].ToString();
                                    colCalcId = Convert.ToInt32(dtList.Rows[i]["DSTempSetColCalcId"]);
                                    if (dtList.Rows[i]["IncludeInGraph"] is bool)
                                    {
                                        isIIGraph = Convert.ToBoolean(dtList.Rows[i]["IncludeInGraph"]);
                                    }
                                    if (calcType == "NA")
                                    {
                                        chkTxtNa.Checked = true;
                                        txtTxtNA.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkTxtNa.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkTxtNaIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "Customize")
                                    {
                                        chkTextCustomize.Checked = true;
                                        txtTxtCustomize.Text = calcFormula.ToString();
                                        //txtCalcuType.Text = calcFormula.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkTextCustomize.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkTextCustomizeIIG.Checked = true;
                                        }
                                    }
                                }
                            }

                            else if (columnType == "Duration")
                            {
                                MoveupOpt1.SelectedValue = MoveupStat.ToString();
                                txtDurCorrectResponse.Text = crctResponseDesc.ToString();
                                txtDurIncrctResp.Text = incorrctResp.ToString();
                                if (txtDurCorrectResponse.Text != "" || txtDurIncrctResp.Text != "")
                                {
                                    durationAOTb.Style.Add("display", "block");
                                }
                                if (inclMistrial == 1)
                                {
                                    chkDurIncludeMistrial.Checked = true;
                                    txtDurInclMisTrial.Text = mistrialDesc;
                                }
                                for (int i = 0; i < dtList.Rows.Count; i++)
                                {
                                    calcType = dtList.Rows[i]["CalcType"].ToString();
                                    calcDesc = dtList.Rows[i]["CalcRptLabel"].ToString();
                                    colCalcId = Convert.ToInt32(dtList.Rows[i]["DSTempSetColCalcId"]);
                                    if (dtList.Rows[i]["IncludeInGraph"] is bool)
                                    {
                                        isIIGraph = Convert.ToBoolean(dtList.Rows[i]["IncludeInGraph"]);
                                    }
                                    if (calcType == "Avg Duration")
                                    {
                                        chkDurAverage.Checked = true;
                                        txtDurAverage.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkDurAverage.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkDurAverageIIG.Checked = true;
                                        }
                                    }
                                    if (calcType == "Total Duration")
                                    {
                                        chkDurTotalDur.Checked = true;
                                        txtDurTotalDuration.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkDurTotalDur.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkDurTotalDurIIG.Checked = true;
                                        }
                                    }

                                }
                            }

                            else if (columnType == "Frequency")
                            {
                                MoveupOpt.SelectedValue = MoveupStat.ToString();
                                txtFreqCorrectResponse.Text = crctResponseDesc.ToString();
                                txtfreqIncrctResp.Text = incorrctResp.ToString();
                                if (txtFreqCorrectResponse.Text != "" || txtfreqIncrctResp.Text != "")
                                {
                                    frequencyAOTb.Style.Add("display", "block");
                                }
                                if (inclMistrial == 1)
                                {
                                    chkFreqIncludeMistrial.Checked = true;
                                    txtFreqIncludeMistrial.Text = mistrialDesc;
                                }

                                for (int i = 0; i < dtList.Rows.Count; i++)
                                {
                                    calcType = dtList.Rows[i]["CalcType"].ToString();
                                    calcDesc = dtList.Rows[i]["CalcRptLabel"].ToString();
                                    colCalcId = Convert.ToInt32(dtList.Rows[i]["DSTempSetColCalcId"]);
                                    if (dtList.Rows[i]["IncludeInGraph"] is bool)
                                    {
                                        isIIGraph = Convert.ToBoolean(dtList.Rows[i]["IncludeInGraph"]);
                                    }
                                    if (calcType == "Frequency")
                                    {
                                        chkFrequency.Checked = true;
                                        txtFrequency.Text = calcDesc.ToString();
                                        if (IsMeasureAssigned(columnId, colCalcId) == true)
                                        {
                                            chkFrequency.Enabled = false;
                                        }
                                        if (isIIGraph == true)
                                        {
                                            chkFrequencyIIG.Checked = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }




    protected bool IsMeasureAssigned(int colId, int measureId)
    {
        string selQuerry = "SELECT DSTempRuleId FROM DSTempRule WHERE DSTempSetColId = " + colId + " AND DSTempSetColCalcId = " + measureId + "   AND ActiveInd = 'A'";
        DataTable dtNew = objData.ReturnDataTable(selQuerry, false);
        if (dtNew != null && dtNew.Rows.Count > 0)
        {
            ddlColumnType.Enabled = false;
            MoveupOpt.Enabled = false;
            MoveupOpt1.Enabled = false;
            return true;
        }
        else
        {
            return false;
        }
    }


    protected void DivMeasureVisibility(string colType)
    {
        if (colType == "+/-")
        {
            PlusMinusDiv.Visible = true;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }
        else if (colType == "Prompt")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = true;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }

        else if (colType == "Text")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = true;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = false;
        }
        else if (colType == "Duration")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = true;
            FrequencyDiv.Visible = false;
        }
        else if (colType == "Frequency")
        {
            PlusMinusDiv.Visible = false;
            promptDiv.Visible = false;
            TextDiv.Visible = false;
            DurationDiv.Visible = false;
            FrequencyDiv.Visible = true;
        }
    }



    protected void BtnAddPromptSelctd_Click(object sender, EventArgs e)
    {
        try
        {
            if (ddlPromptProcedure.SelectedIndex == 0)
            {
                //tdMsg.InnerHtml = clsGeneral.warningMsg("Please Select Prompt Procedure");
                //ddlPromptProcedure.Focus();

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSelectPrompt();", true);
                return;
            }
            else
            {
                if (lstCompletePrompts.SelectedIndex > -1)
                {
                    string _value = lstCompletePrompts.SelectedItem.Value; //Gets the value of  items in list.
                    string _text = lstCompletePrompts.SelectedItem.Text;  // Gets the Text of items in the list.  
                    ListItem item = new ListItem(); //create a list item
                    item.Text = _text;               //Assign the values to list item   
                    item.Value = _value;
                    lstSelectedPrompts.Items.Add(item); //Add the list item to the selected list of prompts   
                    lstCompletePrompts.Items.Remove(item); //Remove the details from AllPrompts list   
                    //  tdMsg.InnerHtml = "";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    protected void BtnAddAllPrompt_Click(object sender, EventArgs e)
    {
        try
        {
            if (ddlPromptProcedure.SelectedIndex == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSelectPrompt();", true);
                // tdMsg.InnerHtml = clsGeneral.warningMsg("Please Select Prompt Procedure");
                // ddlPromptProcedure.Focus();
                return;
            }
            else
            {
                int _count = lstCompletePrompts.Items.Count;
                if (_count != 0)
                {
                    for (int i = 0; i < _count; i++)
                    {
                        ListItem item = new ListItem();
                        item.Text = lstCompletePrompts.Items[i].Text;
                        item.Value = lstCompletePrompts.Items[i].Value;
                        //Add the item to selected Role list
                        lstSelectedPrompts.Items.Add(item);
                        // tdMsg.InnerHtml = "";
                    }

                }
            }
            lstCompletePrompts.Items.Clear();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void BtnRemvePrmptSelctd_Click(object sender, EventArgs e)
    {
        try
        {
            if (ddlPromptProcedure.SelectedIndex == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSelectPrompt();", true);
                // tdMsg.InnerHtml = clsGeneral.warningMsg("Please Select Prompt Procedure");
                //  ddlPromptProcedure.Focus();
                return;
            }
            else
            {

                if (lstSelectedPrompts.SelectedIndex > -1)
                {
                    string _value = lstSelectedPrompts.SelectedItem.Value; //Gets the value of items in list.
                    string _text = lstSelectedPrompts.SelectedItem.Text;  // Gets the Text of items in the list.  
                    ListItem item = new ListItem(); //create a list item
                    item.Text = _text;               //Assign the values to list item   
                    item.Value = _value;
                    lstSelectedPrompts.Items.Remove(item); //Remove from the selected list
                    lstCompletePrompts.Items.Add(item); //Add in the PromptAll list 
                    //  tdMsg.InnerHtml = "";

                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void BtnRemoveAllPrmpt_Click(object sender, EventArgs e)
    {
        try
        {
            if (ddlPromptProcedure.SelectedIndex == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSelectPrompt();", true);
                // tdMsg.InnerHtml = clsGeneral.warningMsg("Please Select Prompt Procedure");
                // ddlPromptProcedure.Focus();
                return;
            }
            else
            {
                int _count = lstSelectedPrompts.Items.Count;
                if (_count != 0)
                {
                    for (int i = 0; i < _count; i++)
                    {
                        ListItem item = new ListItem();
                        item.Text = lstSelectedPrompts.Items[i].Text;
                        item.Value = lstSelectedPrompts.Items[i].Value;
                        lstCompletePrompts.Items.Add(item);
                        //   tdMsg.InnerHtml = "";
                    }
                }
            }
            lstSelectedPrompts.Items.Clear();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void ddlPromptProcedure_SelectedIndexChanged(object sender, EventArgs e)
    {
        objData = new clsData();
        string selQuerry = "";
        string naValue = "";
        object objVal = null;
        try
        {
            selQuerry = "SELECT LookupId FROM LookUp WHERE LookupType = 'Datasheet-Prompt Procedures' AND LookupName = 'NA'";
            objVal = objData.FetchValue(selQuerry);
            if (objVal != null)
            {
                naValue = objVal.ToString();
            }

            if (ddlPromptProcedure.SelectedValue == naValue.ToString())
            {
                SelectedPromptTool.Visible = false;
                CompletePromptTool.Visible = false;
                lstCompletePrompts.Visible = false;
                lstSelectedPrompts.Visible = false;
                BtnAddPromptSelctd.Visible = false;
                BtnAddAllPrompt.Visible = false;
                BtnRemvePrmptSelctd.Visible = false;
                BtnRemoveAllPrmpt.Visible = false;
                lblSelctPrompt.Visible = false;
            }
            else
            {
                SelectedPromptTool.Visible = true;
                CompletePromptTool.Visible = true;
                lstCompletePrompts.Visible = true;
                lstSelectedPrompts.Visible = true;
                BtnAddPromptSelctd.Visible = true;
                BtnAddAllPrompt.Visible = true;
                BtnRemvePrmptSelctd.Visible = true;
                BtnRemoveAllPrmpt.Visible = true;
                lblSelctPrompt.Visible = true;
            }
        }
        catch
        {
        }



    }

    protected void BtnSavePrompt_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int headerId = 0;
        int count = 0;
        string strQuerry = "";
        string naValue = "";
        int index = 0;
        object objVal = null;
        object objnew = null;
        object objIfexist = null;
        int ifexists = 0;
        sess = (clsSession)Session["UserSession"];
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            try
            {
                strQuerry = "SELECT LookupId FROM LookUp WHERE LookupType = 'Datasheet-Prompt Procedures' AND LookupName = 'NA'";
                objVal = objData.FetchValue(strQuerry);
                if (objVal != null)
                {
                    naValue = objVal.ToString();
                }
            }
            catch
            {
            }

            if (ddlPromptProcedure.SelectedIndex == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSelectPrompt();", true);
                return;
            }

            else
            {
                saveChkStepByStep(headerId);

                int promptProcedure = Convert.ToInt32(ddlPromptProcedure.SelectedValue);
                if (lstSelectedPrompts.Items.Count == 0)
                {
                    ddlPromptProcedure.SelectedValue = naValue;
                    promptProcedure = Convert.ToInt32(naValue);

                }
                try
                {

                    if (ddlPromptProcedure.SelectedValue == naValue.ToString())                            // If selected prompt procedure is NA, then the complete asigned promptes of that headerId will delete.
                    {
                        strQuerry = "SELECT COUNT(DSTempSetColId) FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ColTypeCd = 'Prompt' AND ActiveInd = 'A'";
                        objnew = objData.FetchValue(strQuerry);
                        if (objnew != null)
                        {
                            count = Convert.ToInt32(objnew);
                        }
                        ddlPromptProcedure_SelectedIndexChanged(sender, e);
                        if (count == 0)
                        {
                            strQuerry = "UPDATE DSTempHdr SET PromptTypeId = " + promptProcedure + " WHERE DSTempHdrId =" + headerId;
                            index = objData.Execute(strQuerry);
                            string del = "DELETE FROM DSTempPrompt WHERE DSTempHdrId = " + headerId;
                            objData.Execute(del);
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
                            return;
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertPromptValid();", true);
                            return;
                        }
                    }
                }
                catch
                {
                }

                strQuerry = "UPDATE DSTempHdr SET PromptTypeId = " + promptProcedure + " WHERE DSTempHdrId =" + headerId;
                index = objData.Execute(strQuerry);
                //for (int j = 0; j < lstCompletePrompts.Items.Count; j++)
                //{
                //    string promptId = lstCompletePrompts.Items[j].Value.ToString();
                //    try
                //    {
                //        string del = "DELETE FROM DSTempPrompt WHERE DSTempHdrId = " + headerId + " AND PromptId = " + promptId;
                //        objData.Execute(del);
                //    }
                //    catch (Exception ex)
                //    {
                //    }
                //}

                string delquery = "DELETE FROM DSTempPrompt WHERE DSTempHdrId = " + headerId;
                objData.Execute(delquery);
                int arraycnt = 0;
                string Promptid = "";
                foreach (ListItem li in lstSelectedPrompts.Items)
                {
                    Promptid += li.Value + ",";
                    arraycnt++;
                }
                Promptid = Promptid.TrimEnd(',');
                DataTable dtPrompt = objData.ReturnDataTable("SELECT [LookupId] FROM [dbo].[LookUp] WHERE [LookupId] IN (" + Promptid + ") ORDER BY [SortOrder] DESC", false);
                if (dtPrompt != null)
                {
                    for (int i = 0; i < dtPrompt.Rows.Count; i++)
                    {
                        ifexists = 0;
                        string selctPromptId = dtPrompt.Rows[i]["LookupId"].ToString();

                        try
                        {
                            //string sel = "SELECT DSTempPromptId FROM DSTempPrompt WHERE DSTempHdrId = " + headerId + " AND PromptId = " + selctPromptId;
                            //objIfexist = objData.FetchValue(sel);
                            //if (objIfexist != null)
                            //{
                            //    ifexists = Convert.ToInt32(objIfexist);
                            //}
                            //if (ifexists <= 0)
                            //{

                            string strQuery = "INSERT INTO DSTempPrompt(DSTempHdrId,PromptId,PromptOrder,ActiveInd,CreatedBy,CreatedOn) Values(" + headerId + "," + selctPromptId + "," + i + ",'A'," + sess.LoginId + ",GetDate())";
                            int promptId = objData.Execute(strQuery);

                            //    tdMsg.InnerHtml = clsGeneral.sucessMsg("Prompt Procedure Added Successfully");

                            //}

                        }

                        catch (SqlException ex)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertFailedMsg();", true);
                            // tdMsg.InnerHtml = clsGeneral.failedMsg("Class Insertion For User Failed");
                        }


                    }
                }
            }
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    private void saveChkStepByStep(int headerId)
    {
        objData = new clsData();
        string strQuerry = "";
        if (chk_stepBystep.Visible == true)
        {
            if (chk_stepBystep.Checked == true)
            {
                strQuerry = "UPDATE DSTempHdr SET TotalTaskType = 1 WHERE DSTempHdrId =" + headerId;
            }
            else
            {
                strQuerry = "UPDATE DSTempHdr SET TotalTaskType = 0 WHERE DSTempHdrId =" + headerId;
            }

            objData.Execute(strQuerry);
        }
    }



    protected void BtnAddSamples_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string textValue = "";
        //textValue = clsGeneral.convertQuotes(txtMatcSamples.Text.Trim());
        textValue = txtMatcSamples.Text.Trim();
            ListItem item = new ListItem();
        DistractInc = 1;

        Dictionary<string, string> DistractIndex = new Dictionary<string, string>();
        foreach (ListItem listBoxItem in Disitem.Items)
        {
            if (!DistractIndex.ContainsKey(listBoxItem.Text) && !DistractIndex.ContainsKey(listBoxItem.Value))
                DistractIndex.Add(listBoxItem.Text, listBoxItem.Value);
        }

        string Disexist = "";

        if (textValue != "" && (chk_distractor.Checked == true || chk_distractor.Checked == false))
        {
            if (chk_distractor.Checked == true)
            {
                for (int j = 1; j <= Disitem.Items.Count; j++)
                {
                    Disexist = "D" + DistractInc;
                    for (var i = 0; i < Disitem.Items.Count; i++)
                    {
                        if (Disitem.Items[i].ToString().ToLower().Contains(Disexist.ToLower()))
                        {                                
                            DistractInc++;
                            Disexist = "D" + DistractInc;
                        }
                    }                    
                }
                textValue = "D" + DistractInc + "-" + textValue;
            }
            item.Text = textValue;
            item.Value = textValue;
            lstMatchSamples.Items.Add(item);
            if (chk_distractor.Checked == true)
            {                
                Disitem.Items.Add(item);
            }
            txtMatcSamples.Text = "";
            txtMatcSamples.Focus();
            DistractInc++;
        }
        else if (textValue == "" && chk_distractor.Checked == true)
        {
            if (chk_distractor.Checked == true)
            {
                for (int j = 1; j <= Disitem.Items.Count; j++)
                {
                    Disexist = "D" + DistractInc;
                    for (var i = 0; i < Disitem.Items.Count; i++)
                    { 
                        if (Disitem.Items[i].ToString().ToLower().Contains(Disexist.ToLower()))
                        {
                            DistractInc++;
                            Disexist = "D" + DistractInc;
    }
                    }
                }
                textValue = "D" + DistractInc;   
            }
            item.Text = textValue;
            item.Value = textValue;
            lstMatchSamples.Items.Add(item);
            if (chk_distractor.Checked == true)
            {
                Disitem.Items.Add(item);
            }
            txtMatcSamples.Text = "";
            txtMatcSamples.Focus();
            DistractInc++;
        }
    }


    protected void btnDeltSamples_Click(object sender, EventArgs e)
    {
        objData = new clsData();

        if (lstMatchSamples.SelectedIndex > -1)
        {
            string value = lstMatchSamples.SelectedItem.Value;
            string text = lstMatchSamples.SelectedItem.Text;
            ListItem item = new ListItem(); //create a list item
            item.Text = text;               //Assign the values to list item   
            item.Value = value;
            if(Disitem.Items.Contains(item))
            {
                Disitem.Items.Remove(item);
            }
            lstMatchSamples.Items.Remove(item); //Remove from the selected list
        }


    }



    protected void rbtnConsectiveSes_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rbtnConsectiveSes.SelectedValue == "1")
        {
            txtIns2.ReadOnly = true;
            txtIns1.ReadOnly = true;
            txtNumbrSessions.ReadOnly = false;
            txtIns2.Text = "";
            txtIns1.Text = "";
            rbtnConsectiveAvg.Items[0].Selected = false; //--- [New Criteria] May 2020 ---//
            rbtnConsectiveAvg.Items[1].Selected = true; //--- [New Criteria] May 2020 ---//
        }
        else if (rbtnConsectiveSes.SelectedValue == "0")
        {
            txtNumbrSessions.ReadOnly = true;
            txtIns2.ReadOnly = false;
            txtIns1.ReadOnly = false;
            txtNumbrSessions.Text = "";
            rbtnConsectiveAvg.Items[0].Selected = false; //--- [New Criteria] May 2020 ---//
            rbtnConsectiveAvg.Items[1].Selected = true; //--- [New Criteria] May 2020 ---//

        }

    }

    //--- [New Criteria] May 2020 - (Start) ---//
    protected void rbtnConsectiveAvg_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rbtnConsectiveAvg.SelectedValue == "1")
        {
            txtIns2.ReadOnly = true;
            txtIns1.ReadOnly = true;
            txtNumbrSessions.ReadOnly = false;
            txtIns2.Text = "";
            txtIns1.Text = "";
            rbtnConsectiveSes.Items[0].Selected = false;
            rbtnConsectiveSes.Items[1].Selected = true;
        }
        else if (rbtnConsectiveAvg.SelectedValue == "0")
        {
            txtNumbrSessions.ReadOnly = true;
            txtIns2.ReadOnly = false;
            txtIns1.ReadOnly = false;
            txtNumbrSessions.Text = "";
            rbtnConsectiveSes.Items[0].Selected = false;
            rbtnConsectiveSes.Items[1].Selected = true;
        }

    }
    //--- [New Criteria] May 2020 - (End) ---//

    protected void btUpdateLessonProc_Click(object sender, EventArgs e)
    {
        this.ClientScript.RegisterOnSubmitStatement(this.GetType(), "EscapeField", "EscapeField();");
        objData = new clsData();
        int headerId = 0;
        string updateQuerry = "";

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        try
        {
            
            string txtcommentLessonProcedureP = txtcommentLessonProcedure.Text.Trim().Replace("'", "''");

            
            string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteLessonProc='" + txtcommentLessonProcedureP + "' WHERE DSTempHdrId='" + headerId + "'";
            objData.Execute(UpdateAppr);

            updateQuerry = "UPDATE DSTempHdr SET LessonDefInst = '" + clsGeneral.convertQuotes(txtSDInstruction.Text.Trim())
                + "',StudentReadCrita = '" + clsGeneral.convertQuotes(txtResponseOutcome.Text.Trim()) + "', " +
                            "ReinforcementProc = '" + clsGeneral.convertQuotes(txtReinforcementProc.Text.Trim())
                            + "',CorrectionProc = '" + clsGeneral.convertQuotes(txtCorrectionProcedure.Text.Trim())
                            + "',StudCorrRespDef = '" + clsGeneral.convertQuotes(txtResponseOutcome.Text.Trim())
                            + "',MistrialResponse = '" + clsGeneral.convertQuotes(txtMistrialProcedure.Text.Trim())
                            + "',StudIncorrRespDef = '" + clsGeneral.convertQuotes(txtIncorrectResponse.Text.Trim()) + "' " +
                           " ,Mistrial = '" + clsGeneral.convertQuotes(txtMistrial.Text) + "', TeacherPrepare = '" + clsGeneral.convertQuotes(txtTeacherDo.Text) + "',StudentPrepare = '" + clsGeneral.convertQuotes(txtStudentDo.Text) + "',StudResponse = '" + clsGeneral.convertQuotes(txtConsequenceDO.Text) + "' WHERE DSTempHdrId = " + headerId;

            int index = objData.Execute(updateQuerry);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
        }


        catch (Exception Ex)
        {
            string error = Ex.Message;
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertFailedMsg();", true);
            throw Ex;
        }
        showMatchToSampleDrop();
        drpTasklist_SelectedIndexChanged1(sender, e);
    }


    private void GetStatus(int TemplateId)
    {
        bool visibility = false;
        objData = new clsData();
        int tempStatusId = 0;
        try
        {
            int progresStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='In Progress' "));
            object objTempstat = objData.FetchValue("SELECT  [StatusId] FROM DSTempHdr WHERE DSTempHdrId= " + TemplateId + " ");

            if (objTempstat != null)
            {
                if (objTempstat.ToString() != "")
                {
                    tempStatusId = Convert.ToInt16(objTempstat);
                }

            }

            int AppStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));

            int PendStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Pending Approval' "));
            int expireStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired' "));
            int MaintStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance' "));
            int InctStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive' "));
            if (tempStatusId == AppStatusId)
            {
                BtnCopyTemplate.Visible = true;
                BtnExportTemplate.Visible = true;
                BtnMaintenance.Visible = true;
                BtnMakeRegular.Visible = false;
                BtnActive.Visible = false;
                BtnInactive.Visible = true;
                BtnSubmit.Visible = false;
                BtnApproval.Visible = false;
                BtnReject.Visible = false;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                btnDelLp.Visible = true;
                btnrejectedNotes.Visible = false;
                visibility = false;

                ButtonVisibility(visibility);
                DatalistVisibility(visibility);
                CheckAsigned(TemplateId);              // Check Is any other lesson plan waiting for approval.

            }
            else if (tempStatusId == progresStatusId)
            {
                BtnCopyTemplate.Visible = false;
                BtnExportTemplate.Visible = false;
                BtnMaintenance.Visible = false;
                BtnMakeRegular.Visible = false;
                BtnActive.Visible = false;
                BtnInactive.Visible = false;
                BtnSubmit.Visible = true;
                btnDelLp.Visible = true;
                btndoc.Style.Add("display", "Block");
                BtnApproval.Visible = false;
                BtnReject.Visible = false;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                btnrejectedNotes.Visible = false;
                visibility = true;

                ButtonVisibility(visibility);
                DatalistVisibility(visibility);
                try
                {
                    FillDropPrompt(TemplateId);                              // Prompt Procedure NA Check. 
                    IsDiscrete(TemplateId);                                  // Check the current Lesson plan is discrete or not        
                    IsVisualTool(TemplateId);                                //Check current lesson is visual and asigning the control feature enable or disable

                }
                catch (Exception Ex)
                {
                    throw Ex;
                }

            }
            else if (tempStatusId == PendStatusId)
            {
                BtnCopyTemplate.Visible = false;
                BtnMaintenance.Visible = false;
                BtnExportTemplate.Visible = false;
                BtnMakeRegular.Visible = false;
                BtnActive.Visible = false;
                BtnInactive.Visible = false;
                BtnSubmit.Visible = false;
                BtnApproval.Visible = true;
                BtnReject.Visible = true;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                btnDelLp.Visible = true;
                btnrejectedNotes.Visible = false;
                visibility = false;

                ButtonVisibility(visibility);
                DatalistVisibility(visibility);
            }
            else if (tempStatusId == expireStatusId)
            {
                BtnCopyTemplate.Visible = false;
                BtnMaintenance.Visible = false;
                BtnExportTemplate.Visible = false;
                BtnMakeRegular.Visible = false;
                BtnActive.Visible = false;
                BtnInactive.Visible = false;
                BtnSubmit.Visible = false;//
                BtnApproval.Visible = false;
                BtnReject.Visible = false;
                btnFromReject.Visible = GetRejectedLPsInInProgressStatus(TemplateId);//
                if (btnFromReject.Visible == false) btnFromRejectDup.Visible = true;
                else btnFromRejectDup.Visible = false;
                btnDelLp.Visible = true;
                btnrejectedNotes.Visible = true;
                visibility = false;
                ButtonVisibility(visibility);
                DatalistVisibility(visibility);
            }
            else if (tempStatusId == MaintStatusId)
            {
                BtnCopyTemplate.Visible = false;
                BtnMakeRegular.Visible = true;
                BtnExportTemplate.Visible = true;
                BtnSubmit.Visible = false;
                BtnActive.Visible = false;
                BtnInactive.Visible = false;
                btnDelLp.Visible = false;
                btnrejectedNotes.Visible = false;
                BtnApproval.Visible = false;
                BtnReject.Visible = false;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                BtnMaintenance.Visible = false;
                visibility = false;
                ButtonVisibility(visibility);
                DatalistVisibility(visibility);
            }
            else if (tempStatusId == InctStatusId)
            {
                BtnCopyTemplate.Visible = false;
                BtnMakeRegular.Visible = false;
                BtnExportTemplate.Visible = true;
                BtnSubmit.Visible = false;
                BtnActive.Visible = true;
                BtnInactive.Visible = false;
                btnDelLp.Visible = true;
                btnrejectedNotes.Visible = false;
                BtnApproval.Visible = false;
                BtnReject.Visible = false;
                btnFromReject.Visible = false;
                btnFromRejectDup.Visible = false;
                BtnMaintenance.Visible = false;
                visibility = false;
                ButtonVisibility(visibility);
                DatalistVisibility(visibility);

            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void IsVisualTool(int TemplateId)
    {
        objData = new clsData();
        string selctQuerry = "";
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
        object objVt = objData.FetchValue(selctQuerry);

        if (Convert.ToString(objVt) != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                ControlsDisableOnVisual(false);
            }
            else
            {
                ControlsDisableOnVisual(true);
                IsDiscrete(TemplateId);
            }
        }
        else
        {
            ControlsDisableOnVisual(true);
            IsDiscrete(TemplateId);
        }


    }

    protected void ControlsDisableOnVisual(bool valid)
    {
        objData = new clsData();

        if (valid == false)
        {


            lblMeasureStart.Visible = false;
            BtnAddMeasure.Visible = false;
            btnAddSet.Visible = false;
            btnAddSetDetails.Visible = false;
            //setPanel.Visible = false;
            setPanel.Style.Add("display", "None");
            dlSetDetails.Visible = false;
            BtnAddStep.Visible = false;
            btnAddStepDetails.Visible = false;
            //stepPanel.Visible = false;
            stepPanel.Style.Add("display", "None");
            BtnAddSort.Visible = false;
            lblSetStart.Visible = false;


            btnAddStepCriteria.Visible = false;
            dlStepDetails.Visible = false;
            dlStepCriteria.Visible = false;
            lblStepStart.Visible = false;
            lblStepCriStart.Visible = false;

            foreach (DataListItem item in dlMeasureData.Items)
            {
                Button btnEdit = (Button)item.FindControl("btnEditMeasure");
                Button btnDelt = (Button)item.FindControl("BtnRemove");
                if (valid == false)
                {
                    if (btnEdit != null)
                    {
                        btnEdit.Visible = false;
                    }
                    if (btnDelt != null)
                    {
                        btnDelt.Visible = false;
                    }
                }
                else
                {
                    btnDelt.Visible = true;
                    btnEdit.Visible = true;
                }
            }
        }

        else
        {



            lblMeasureStart.Visible = true;
            BtnAddMeasure.Visible = true;

            btnAddSet.Visible = true;
            btnAddSetDetails.Visible = true;
            //setPanel.Visible = true;
            setPanel.Style.Add("display", "Block");
            dlSetDetails.Visible = true;
            BtnAddStep.Visible = true;
            btnAddStepDetails.Visible = true;
            //stepPanel.Visible = true;
            stepPanel.Style.Add("display", "Block");
            BtnAddSort.Visible = true;
            BtnAddSort.Visible = true;
            lblSetStart.Visible = false;


            btnAddStepCriteria.Visible = true;
            dlStepDetails.Visible = true;
            dlStepCriteria.Visible = true;
            lblStepStart.Visible = true;
            lblStepCriStart.Visible = true;
        }
    }


    protected void DatalistVisibility(bool valid)
    {
        objData = new clsData();
        foreach (DataListItem item in dlMeasureData.Items)
        {
            Button btnEdit = (Button)item.FindControl("btnEditMeasure");
            Button btnDelt = (Button)item.FindControl("BtnRemove");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details" ;
                    }
                    else
                        btnEdit.Visible = false;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
            }
            else
            {
                btnDelt.Visible = true;
                btnEdit.Visible = true;
            }
        }

        foreach (DataListItem item in dlSetDetails.Items)
        {
            Button btnEdit = (Button)item.FindControl("btnEditSet");
            Button btnDelt = (Button)item.FindControl("btnRemoveSet");
            ImageButton imgUp = (ImageButton)item.FindControl("imgUp");
            ImageButton imgDown = (ImageButton)item.FindControl("imgDown");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details";
                    }
                    else
                        btnEdit.Visible = false;
                    
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
                if (imgUp != null)
                {
                    imgUp.Visible = false;
                }
                if (imgDown != null)
                {
                    imgDown.Visible = false;
                }
            }
            else
            {
                if (btnEdit != null)
                {
                    btnEdit.Visible = true;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = true;
                }
            }
        }

        foreach (DataListItem item in dlSetCriteria.Items)
        {
            Button btnEdit = (Button)item.FindControl("BtnEditSetCriteria");
            Button btnDelt = (Button)item.FindControl("BtnRemoveSetCriteria");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details";
                    }
                    else
                        btnEdit.Visible = false;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
            }
            else
            {
                if (btnEdit != null)
                {
                    btnEdit.Visible = true;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = true;
                }
            }
        }
        foreach (DataListItem item in dlStepDetails.Items)
        {
            Button btnEdit = (Button)item.FindControl("btnEditStep");
            Button btnDelt = (Button)item.FindControl("btnRemoveStep");
            //ImageButton imgUp = (ImageButton)item.FindControl("imgUp");
            //ImageButton imgDown = (ImageButton)item.FindControl("imgDown");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details";
                    }
                    else
                        btnEdit.Visible = false;

                    
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
                //if (imgUp != null)
                //{
                //    imgUp.Visible = false;
                //}
                //if (imgDown != null)
                //{
                //    imgDown.Visible = false;
                //}
            }
            else
            {
                if (btnEdit != null)
                {
                    btnEdit.Visible = true;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = true;
                }
                //if (imgUp != null)
                //{
                //    imgUp.Visible = true;
                //}
                //if (imgDown != null)
                //{
                //    imgDown.Visible = true;
                //}
            }
        }

        foreach (DataListItem item in dlStepCriteria.Items)
        {
            Button btnEdit = (Button)item.FindControl("BtnEditStepCriteria");
            Button btnDelt = (Button)item.FindControl("BtnRemoveStepCriteria");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details";
                    }
                    else
                        btnEdit.Visible = false;

                    
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
            }
            else
            {
                if (btnEdit != null)
                {
                    btnEdit.Visible = true;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = true;
                }
            }
        }

        foreach (DataListItem item in dlPromptCriteria.Items)
        {
            Button btnEdit = (Button)item.FindControl("BtnEditPromptCriteria");
            Button btnDelt = (Button)item.FindControl("BtnRemovePromptCriteria");
            if (valid == false)
            {
                if (btnEdit != null)
                {
                    if (Hdfsavemeasure.Value != "")
                    {
                        btnEdit.Visible = true;
                        btnEdit.Text = "View Details";
                    }
                    else
                        btnEdit.Visible = false;
                    
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = false;
                }
            }
            else
            {
                if (btnEdit != null)
                {
                    btnEdit.Visible = true;
                }
                if (btnDelt != null)
                {
                    btnDelt.Visible = true;
                }
            }
        }

    }

    protected int ReturnNewVLessonId(int templateId)
    {
        objData = new clsData();
        oData = new DataClass();
        int studId = sess.StudentId;
        int newVisualLessonId = 0;
        string selctQuerry = "";
        string studentName = "";
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        object objVt = objData.FetchValue(selctQuerry);
        if (objVt != null)
        {

            if (objVt.ToString() != "")
            {
                string selctStudentName = "SELECT StudentLname + StudentFname As StudentName FROm Student WHERE StudentId = " + studId;
                DataTable dtNew = objData.ReturnDataTable(selctStudentName, false);
                if (dtNew.Rows.Count > 0)
                {
                    studentName = dtNew.Rows[0]["StudentName"].ToString();

                }
                int vtId = Convert.ToInt32(objVt);
                if (vtId > 0)
                {
                    try
                    {
                        int isStEdit = 1;
                        int isCcEdit = 0;
                        string selctSpQuerry = "sp_copyLessonPlan";     // Stored Procedure call for duplicate Lessonplan
                        //int newLessonId = oData.Execute_SpCopyLesson(selctSpQuerry, vtId, isStEdit, isCcEdit, studentName);
                        int newLessonId = oData.Execute_SpCopyLesson(selctSpQuerry, vtId, isStEdit, isCcEdit);

                        if (newLessonId > 0)
                        {
                            string selctLp = "SELECT MAX(LessonId) FROM LE_Lesson";
                            newVisualLessonId = Convert.ToInt32(objData.FetchValue(selctLp));
                        }

                    }
                    catch (Exception Ex)
                    {
                        throw Ex;
                    }
                }
            }
        }

        return newVisualLessonId;
    }


    protected void BtnCopyTemplate_Approve(object sender, EventArgs e)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        if (ViewState["HeaderId"] != null)
        {
            int apprvdLessonId = Convert.ToInt32(objData.FetchValue("SELECT COUNT(*) FROM DSTempHdr WHERE LessonPlanId= (SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId=" + ViewState["HeaderId"] + " ) AND StudentId=" + sess.StudentId + " AND StatusId IN (SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatuS' AND (LookupName='In Progress' OR LookupName='Pending Approval'))"));
            if (apprvdLessonId == 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){$('#overlay').fadeIn('slow', function () { $('#DilogAproveLP').animate({ top: '5%' },{duration: 'slow',easing: 'linear'}) }); $('#close_x').click(function () {$('#dialog').animate({ top: '-300%' }, function () {$('#overlay').fadeOut('slow');});});});", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){$('#overlay').fadeIn('slow', function () { $('#DivAlertAPPLP').animate({ top: '5%' },{duration: 'slow',easing: 'linear'}) }); });", true);
            }

        }

    }

    protected void BtnCopyTemplate_Click(object sender, EventArgs e)             // Submit Template
    {
        objData = new clsData();
        int apprvdLessonId = 0;
        int visualLessonId = 0;
        if (ViewState["HeaderId"] != null)
        {
            apprvdLessonId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        tdReadMsg.InnerHtml = "";
        sess = (clsSession)Session["UserSession"];
        clsAssignLessonPlan AssignLP = new clsAssignLessonPlan();
        visualLessonId = ReturnNewVLessonId(apprvdLessonId);                // Function to take the copy of new visual lessonplan and pass the new lessonplanId.

        try
        {
            if (sess != null)
            {

                int tempid = AssignLP.CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId);      // Set the approved template to expired and copied the new template
                ViewState["HeaderId"] = tempid;                    //Set new headerId to load
                FillData();                                      // Fill the goal type assigned for the student
                FillApprovedLessonData();
                FillRejectedLessons();

                IsDiscrete(tempid);                       // Check the template is discrete
                lblDataFill(tempid);                      // Fill Current LessonName
                GetMeasureData(tempid);                   // Fill the measure Data 
                FillTypeOfInstruction(tempid);                   // Fill Type of Instruction Data
                GetSetData(tempid);                      // Fill the Sets Data
                GetStepData(tempid);                    // Fill the Steps Data
                GetSetCriteriaData(tempid);             // Fill the Set Criteria Data
                GetStepCriteriaData(tempid);           // Fill the Step Criteria Data
                GetPromptCriteriaData(tempid);        // Fill the Prompt Criteria Data
                GetPromptProcedureList(tempid);      // Fill the prompt Procedure...
                FillDropPrompt(tempid);             // Fill dropdownlist prompt procedure...
                GetLessonProcData(tempid);           // Fill LessonProcedureData
                GetStatus(tempid);                    // Button Permissions
                CreateDocument(apprvdLessonId, tempid);                 //Copy documents associated with template
                BtnPreview.Visible = true;
                btndoc.Style.Add("display", "Block");
                BtnUpdateLessonPlan.Visible = true;
                fillParentSetData();
                VisibleApprovalNotes(false);
                VisibleApprovalNote();
                textBoxDisableEnable(true);

            }


        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    private void VisibleApprovalNotes(bool Status)
    {
        bool ReadStatus = false;
        if (Status == false) ReadStatus = true;
        lblCommentLessonInfo.Visible = Status;
        LessoninfoApptooltip.Visible = Status;
        txtCommentLessonInfo.Visible = Status;
        btnCommentLessonInfo.Visible = Status;
        txtCommentLessonInfo.ReadOnly = ReadStatus;

        lblCommentTypeofInstr.Visible = Status;
        TypeInstructTooltip.Visible = Status;
        txtCommentTypeofInstr.Visible = Status;
        btnCommentTypeofInstr.Visible = Status;
        txtCommentTypeofInstr.ReadOnly = ReadStatus;

        lblMeasurementSystems.Visible = Status;
        measurementsystemtooltip.Visible = Status;
        txtMeasurementSystems.Visible = Status;
        btnMeasurementSystems.Visible = Status;
        txtMeasurementSystems.ReadOnly = ReadStatus;

        lblcommentset.Visible = Status;
        commentSetTooltip.Visible = Status;
        commentSetTooltip.Visible = Status;
        txtcommentset.Visible = Status;
        btncommentset.Visible = Status;
        txtcommentset.ReadOnly = ReadStatus;

        lblcommentStep.Visible = Status;
        commentStepTooltip.Visible = Status;
        txtcommentStep.Visible = Status;
        btncommentStep.Visible = Status;
        txtcommentStep.ReadOnly = ReadStatus;

        lblcommentPrompt.Visible = Status;
        commentPromptTooltp.Visible = Status;
        txtcommentPrompt.Visible = Status;
        btncommentPrompt.Visible = Status;
        txtcommentPrompt.ReadOnly = ReadStatus;

        lblcommentLessonProcedure.Visible = Status;
        commentProceduretooltp.Visible = Status;
        txtcommentLessonProcedure.Visible = Status;
        btncommentLessonProcedure.Visible = Status;
        txtcommentLessonProcedure.ReadOnly = ReadStatus;

    }

    private void VisibleApprovalNote()
    {
        lblCommentLessonInfo.Visible = true;
        LessoninfoApptooltip.Visible = true;
        txtCommentLessonInfo.Visible = true;

        lblCommentTypeofInstr.Visible = true;
        TypeInstructTooltip.Visible = true;
        txtCommentTypeofInstr.Visible = true;

        lblMeasurementSystems.Visible = true;
        measurementsystemtooltip.Visible = true;
        txtMeasurementSystems.Visible = true;

        lblcommentset.Visible = true;
        commentSetTooltip.Visible = true;
        txtcommentset.Visible = true;

        lblcommentStep.Visible = true;
        commentStepTooltip.Visible = true;
        txtcommentStep.Visible = true;

        lblcommentPrompt.Visible = true;
        commentPromptTooltp.Visible = true;
        txtcommentPrompt.Visible = true;

        lblcommentLessonProcedure.Visible = true;
        commentProceduretooltp.Visible = true;
        txtcommentLessonProcedure.Visible = true;

    }
    private void VisibleApprovalNotesPR()
    {

        lblCommentLessonInfo.Visible = true;
        LessoninfoApptooltip.Visible = true;
        txtCommentLessonInfo.Visible = true;
        btnCommentLessonInfo.Visible = false;
        txtCommentLessonInfo.ReadOnly = false;

        lblCommentTypeofInstr.Visible = true;
        TypeInstructTooltip.Visible = true;
        txtCommentTypeofInstr.Visible = true;
        btnCommentTypeofInstr.Visible = false;
        txtCommentTypeofInstr.ReadOnly = false;

        lblMeasurementSystems.Visible = true;
        measurementsystemtooltip.Visible = true;
        txtMeasurementSystems.Visible = true;
        btnMeasurementSystems.Visible = false;
        txtMeasurementSystems.ReadOnly = false;

        lblcommentset.Visible = true;
        commentSetTooltip.Visible = true;
        commentSetTooltip.Visible = true;
        txtcommentset.Visible = true;
        btncommentset.Visible = false;
        txtcommentset.ReadOnly = false;

        lblcommentStep.Visible = true;
        commentStepTooltip.Visible = true;
        txtcommentStep.Visible = true;
        btncommentStep.Visible = false;
        txtcommentStep.ReadOnly = false;

        lblcommentPrompt.Visible = true;
        commentPromptTooltp.Visible = true;
        txtcommentPrompt.Visible = true;
        btncommentPrompt.Visible = false;
        txtcommentPrompt.ReadOnly = false;

        lblcommentLessonProcedure.Visible = true;
        commentProceduretooltp.Visible = true;
        txtcommentLessonProcedure.Visible = true;
        btncommentLessonProcedure.Visible = false;
        txtcommentLessonProcedure.ReadOnly = false;

    }

    protected void BtnSubmit_Click(object sender, EventArgs e)             // Submit Template
    {
        Hdfsavemeasure.Value = "";
        textBoxDisableEnable(true);
        btndoc.Style.Add("display", "None");
        objData = new clsData();
        string hdrid = "";
        int TemplateId = 0;
        string setMatch = "";
        string matchSelctd = "";
        int length = 0;
        bool validSet = false;
        bool flag1 = true;
        bool validStep = false;
        int setCriteria = 0;
        int stepCriteria = 0;
        object objTeach = null;
        string teachName = "";
        string message = "";
        string strQuerry = "";
        string skilltype = "";
        int teachId = 0;
        tdReadMsg.InnerHtml = "";
        bool IsVTValid = true;
        bool validonSub = true;
        DataTable dt = new DataTable();
        sess = (clsSession)Session["UserSession"];
        // int noOfTrial = Convert.ToInt32(txtNoofTrail.Text);
        string alertmsg = "";
        tdReadMsg.Visible = false;


        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        string txtCommentLessonInfoP        = txtCommentLessonInfo.Text.Trim().Replace("'", "''");
        string txtCommentTypeofInstrP       = txtCommentTypeofInstr.Text.Trim().Replace("'", "''");
        string txtMeasurementSystemsP       = txtMeasurementSystems.Text.Trim().Replace("'", "''");
        string txtcommentsetP               = txtcommentset.Text.Trim().Replace("'", "''");
        string txtcommentStepP              = txtcommentStep.Text.Trim().Replace("'", "''");
        string txtcommentPromptP            = txtcommentPrompt.Text.Trim().Replace("'", "''");
        string txtcommentLessonProcedureP   = txtcommentLessonProcedure.Text.Trim().Replace("'", "''");

        string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteLessonInfo='" + txtCommentLessonInfoP + "',"+
                                                  "ApprNoteTypeInstruction='" + txtCommentTypeofInstrP + "'," +
                                                  "ApprNoteMeasurement='" + txtMeasurementSystemsP + "'," +
                                                  "ApprNoteSet='" + txtcommentsetP + "'," +
                                                  "ApprNoteStep='" + txtcommentStepP + "'," +
                                                  "ApprNotePrompt='" + txtcommentPromptP + "'," +
                                                  "ApprNoteLessonProc='" + txtcommentLessonProcedureP + "' " +
                                                  "WHERE DSTempHdrId='" + TemplateId + "'";
        objData.Execute(UpdateAppr);

        string qString = "select DSTempSetId from DSTempSet where DSTempHdrId=" + TemplateId + " and ActiveInd='A'";        //get set ids
        DataTable dtSetId = objData.ReturnDataTable(qString, false);

        objData = new clsData();
        string strQry2 = "select DSTemplateName,LessonSDate,LessonEDate from DSTempHdr where DSTempHdrId= " + TemplateId + "";
        DataTable dtCheck2 = objData.ReturnDataTable(strQry2, false);
        if (dtCheck2 != null)
        {
            if (dtCheck2.Rows.Count > 0)
            {
                if (dtCheck2.Rows[0]["DSTemplateName"].ToString() == "0")
                {
                    message += "Lesson plan Name ,";
                    flag1 = false;
                }
                if (dtCheck2.Rows[0]["LessonSDate"].ToString() == "")
                {
                    message += "IEP/ISP/LP Start Date ,";
                    flag1 = false;
                }
                if (dtCheck2.Rows[0]["LessonEDate"].ToString() == "")
                {
                    message += "IEP/ISP/LP End Date ";
                    flag1 = false;
                }
                if (lessonSDate.Text != "" && lessonEDate.Text != "")
                {
                    DateTime dtst = new DateTime();
                    DateTime dted = new DateTime();
                    dtst = DateTime.ParseExact(lessonSDate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    dted = DateTime.ParseExact(lessonEDate.Text.Trim().Replace("-", "/"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    if (dtst > dted)
                    {
                        message += "Start date is must before the End date ,";
                        flag1 = false;
                    }
                    if (message.Length > 0) message = message.Substring(0, message.Length - 1);
                }
            }
        }
        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. " + message + " are missing");
        alertmsg = "Please complete template details before submitting. " + message + " are missing";
        // FUnction to asign sets and steps If the lesson plan is a visual lesson 
        if (message == "")
        {
            IsVTValid = FunInsrtSetStepVT();

            if (IsVTValid == true)
            {
                try
                {
                    //string skilltype = Convert.ToString(objData.FetchValue("SELECT SkillType FROM [dbo].[DSTempHdr] WHERE StudentId='" + sess.StudentId + "' and DSTempHdrId='" + TemplateId + "'"));
                    strQuerry = "SELECT SkillType,TeachingProcId FROM [dbo].[DSTempHdr] WHERE StudentId='" + sess.StudentId + "' and DSTempHdrId=" + TemplateId;
                    DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
                    if (dtNew != null)
                    {
                        if (dtNew.Rows.Count > 0)
                        {
                            skilltype = dtNew.Rows[0]["SkillType"].ToString();
                            try
                            {
                                teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                            }
                            catch
                            {
                                teachId = 0;
                            }
                            if (teachId > 0)
                            {
                                try
                                {
                                    strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                                    objTeach = objData.FetchValue(strQuerry);
                                    if (objTeach != null)
                                    {
                                        teachName = objTeach.ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }

                    if (skilltype == "Chained")
                    {
                        //hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));

                        hdrid = Convert.ToString(objData.FetchValue("SELECT  DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' AND IsDynamic=0 INTERSECT SELECT DSTempHdrId FROM  [dbo].[DSTempSetCol] WHERE DSTempHdrId =" + TemplateId + ""));
                        if (hdrid != "")
                        {
                            dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId " +
                                              "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId " +
                                              "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +
                                                "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                        }
                    }
                    else if (skilltype == "Discrete")
                    {
                        hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet]  WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));
                        if (hdrid != "")
                        {
                            dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND  DSTempSetColId IN (SELECT DSTempSetColId " +
                                                          "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +

                                                                   "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                        }
                    }
                    else
                    {
                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. Skill type is missing");
                        alertmsg = "Please complete template details before submitting. Skill type is missing";
                        FillTypeOfInstruction(TemplateId);
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "script", "alert('" + alertmsg + "');", true);            
                        return;
                    }
                    if (hdrid != "")
                    {
                        if (dt != null)
                        {

                            int colcnt = dt.Columns.Count;  // colcnt=2 for Discrete and colcnt=3 for chained
                            if (colcnt > 0)
                            {
                                if (colcnt == 3)
                                {
                                    if (teachName == "Total Task")
                                    {
                                        if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                        {
                                            string msg = "";
                                            bool flag = true;
                                            string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                            "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                            DataTable dtCheck = objData.ReturnDataTable(strQry, false);

                                            if (dtCheck != null)
                                            {
                                                if (dtCheck.Rows.Count > 0)
                                                {
                                                    if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                    {
                                                        msg += "Set Moveup ,";
                                                        flag = false;
                                                    }
                                                    if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                    {
                                                        msg += "Set Movedown ,";
                                                        flag = false;
                                                    }
                                                    string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                    int SelecItms = lstSelectedPrompts.Items.Count;
                                                    if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                    {
                                                        string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                        DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                        if (dtPromptCheck != null)
                                                        {
                                                            if (dtPromptCheck.Rows.Count > 0)
                                                            {
                                                                if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Moveup ,";
                                                                    flag = false;
                                                                }
                                                                if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Movedown ,";
                                                                    flag = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                                }
                                            }


                                            if (flag)
                                            {
                                                PENDINGAPPROVE(TemplateId);

                                                LoadTemplateData(TemplateId);

                                                //GetStatus(TemplateId);
                                                FillData();                                      // Fill the goal type assigned for the student
                                                FillApprovedLessonData();
                                                FillCompltdLessonPlans();
                                                FillRejectedLessons();
                                                setApprovePermission();
                                            }
                                            else
                                            {
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. " + msg + " criterias are missing");
                                                alertmsg="Please complete template details before submitting. " + msg + " criterias are missing";
                                            }
                                        }
                                        else
                                        {
                                            setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                            if (setCriteria == 0)
                                            {
                                                message = "Set criteria details are missing";
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                                alertmsg="Please complete template details before submitting.." + message + "";
                                            }
                                            else
                                            {
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting..Undefined error.");
                                                alertmsg="Please complete template details before submitting..Undefined error.";
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToInt32(dt.Rows[0]["STEP"]) > 0)
                                        {
                                            string msg = "";
                                            bool flag = true;
                                            string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND CriteriaType='MOVE UP' " +
                                                            "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='STEP'  AND CriteriaType='MOVE DOWN' " +
                                                            "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                            "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                            DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                            if (dtCheck != null)
                                            {
                                                if (dtCheck.Rows.Count > 0)
                                                {
                                                    if (dtCheck.Rows[0]["STEPUP"].ToString() == "0")
                                                    {
                                                        msg += "Step Moveup ,";
                                                        flag = false;
                                                    }
                                                    if (dtCheck.Rows[0]["STEPDOWN"].ToString() == "0")
                                                    {
                                                        msg += "Step Movedown ,";
                                                        flag = false;
                                                    }
                                                    if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                    {
                                                        msg += "Set Moveup ,";
                                                        flag = false;
                                                    }
                                                    if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                    {
                                                        msg += "Set Movedown ,";
                                                        flag = false;
                                                    }


                                                    string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                    int SelecItms = lstSelectedPrompts.Items.Count;
                                                    if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                    {
                                                        string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                        DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                        if (dtPromptCheck != null)
                                                        {
                                                            if (dtPromptCheck.Rows.Count > 0)
                                                            {
                                                                if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Moveup ,";
                                                                    flag = false;
                                                                }
                                                                if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Movedown ,";
                                                                    flag = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                                }
                                            }

                                            if (flag)
                                            {
                                                PENDINGAPPROVE(TemplateId);

                                                LoadTemplateData(TemplateId);

                                                //GetStatus(TemplateId);
                                                FillData();                                      // Fill the goal type assigned for the student
                                                FillApprovedLessonData();
                                                FillCompltdLessonPlans();
                                                FillRejectedLessons();
                                                setApprovePermission();
                                            }
                                            else
                                            {
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. " + msg + " criterias are missing");
                                                alertmsg="Please complete template details before submitting. " + msg + " criterias are missing";
                                            }
                                        }
                                        else
                                        {
                                            //  setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                            stepCriteria = Convert.ToInt32(dt.Rows[0]["STEP"]);
                                            //if (setCriteria == 0)
                                            //{
                                            //    message = "Set criteria details are missing";
                                            //    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                            //}
                                            if (stepCriteria == 0)
                                            {
                                                message = "Step criteria details are missing";
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. " + message + "");
                                                alertmsg="Please complete template details before submitting. " + message + "";
                                            }
                                            else
                                            {
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting");
                                                alertmsg="Please complete template details before submitting";
                                            }
                                        }
                                    }
                                }
                                else if (colcnt == 2)
                                {
                                    if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                    {
                                        validonSub = WriteVTData();

                                        if (validonSub == true)
                                        {

                                            string msg = "";
                                            bool flag = true;
                                            string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                            "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                            "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                            "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                            DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                            if (dtCheck != null)
                                            {
                                                if (dtCheck.Rows.Count > 0)
                                                {
                                                    if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                    {
                                                        msg += "Set Moveup ,";
                                                        flag = false;
                                                    }
                                                    if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                    {
                                                        msg += "Set Movedown ,";
                                                        flag = false;
                                                    }
                                                    string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                    int SelecItms = lstSelectedPrompts.Items.Count;
                                                    if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                    {
                                                        string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                        DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                        if (dtPromptCheck != null)
                                                        {
                                                            if (dtPromptCheck.Rows.Count > 0)
                                                            {
                                                                if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Moveup ,";
                                                                    flag = false;
                                                                }
                                                                if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                                {
                                                                    msg += "Prompt Movedown ,";
                                                                    flag = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                                }
                                            }

                                            //-----
                                            //-----
                                            clsMathToSamples.Step[] steps = null;
                                            bool tempval = true;
                                            if (IsMatchToSample(TemplateId) == true)        //if match to sample
                                            {
                                                if (matchSelctd != null)
                                                {

                                                    string setId = "";
                                                    for (int i = 0; i < dtSetId.Rows.Count; i++)
                                                    {
                                                        setId = dtSetId.Rows[i]["DSTempSetId"].ToString();
                                                        steps = null;
                                                        int setIdint = Convert.ToInt32(setId);
                                                        lstMatchSamples.Items.Clear();
                                                        matchSelctd = "";
                                                        setMatch = "";
                                                        EditSetData(setIdint);
                                                        string[] arryMatchValue = new string[lstMatchSamples.Items.Count];
                                                        if (lstMatchSamples.Items.Count > 0)
                                                        {
                                                            for (int index = 0; index < lstMatchSamples.Items.Count; index++)
                                                            {
                                                                arryMatchValue[index] = lstMatchSamples.Items[index].Value.ToString();
                                                            }
                                                            for (int arryInt = 0; arryInt < lstMatchSamples.Items.Count; arryInt++)
                                                            {
                                                                setMatch += arryMatchValue[arryInt].ToString() + ",";
                                                            }
                                                            length = setMatch.Length;
                                                            matchSelctd = setMatch.ToString().Substring(0, length - 1);
                                                            steps = MatchSampDef2(TemplateId, matchSelctd, setIdint);      //get steps
                                                        }
                                                        if (steps == null)
                                                        {
                                                            tempval = false;
                                                            flag = false;
                                                            msg = "Samples for some sets are missing";
                                                        }
                                                    }

                                                }
                                            }
                                            //-----
                                            //-----

                                            if (flag)
                                            {


                                                PENDINGAPPROVE(TemplateId);

                                                LoadTemplateData(TemplateId);

                                                //   GetStatus(TemplateId);
                                                FillData();                                      // Fill the goal type assigned for the student
                                                FillApprovedLessonData();
                                                FillCompltdLessonPlans();
                                                FillRejectedLessons();
                                                setApprovePermission();
                                            }
                                            else
                                            {
                                                if (tempval == false && flag == false)
                                                {
                                                    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. Samples for some sets are missing");
                                                    alertmsg="Please complete template details before submitting. Samples for some sets are missing";
                                                }
                                                else
                                                {
                                                    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting. " + msg + " criterias are missing");
                                                    alertmsg="Please complete template details before submitting. " + msg + " criterias are missing";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Sorry..Lesson plan submitting failed..Please try again ");
                                            alertmsg="Sorry..Lesson plan submitting failed..Please try again ";
                                        }
                                    }
                                    else
                                    {
                                        setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                        if (setCriteria == 0)
                                        {
                                            message = "Set criteria details are missing";
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                            alertmsg="Please complete template details before submitting.." + message + "";
                                        }
                                        else
                                        {
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.");
                                            alertmsg="Please complete template details before submitting.";
                                        }

                                    }
                                }
                                else
                                {
                                    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.");
                                    alertmsg="Please complete template details before submitting.";

                                }
                            }
                            else
                            {
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.");
                                    alertmsg="Please complete template details before submitting.";

                            }

                        }
                        else
                        {
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.");
                                alertmsg="Please complete template details before submitting.";

                        }
                    }
                    else
                    {
                        validSet = SetValidation(TemplateId);
                        validStep = StepValidation(TemplateId);

                        if (skilltype == "Chained")
                        {
                            if (validStep == false)
                            {
                                message = "Steps are missing.";
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                alertmsg="Please complete template details before submitting.." + message + "";
                            }
                            else
                            {
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting");
                                alertmsg="Please complete template details before submitting";
                            }
                        }
                        else
                        {
                            if (validSet == false)
                            {
                                message = "Sets are missing.";
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting..." + message + "");
                                alertmsg="Please complete template details before submitting..." + message + "";
                            }
                            else
                            {
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting");
                                alertmsg="Please complete template details before submitting";
                            }
                        }

                    }
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
            else
            {
                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting");
                alertmsg = "Please complete template details before submitting";
            }


        }
        if (tdReadMsg.InnerHtml == "<div class='valid_box'>Template Editor Successfully Submitted....</div>")
        {
            tdReadMsg.Visible=true;
            drpTasklist_SelectedIndexChanged1(sender, e);
            int prevTempId = getPrevVersion();
            BtnApproval_hdn.Visible = false;
            BtnApproval.Visible = true;
            loadSetsOverride(prevTempId, TemplateId);
            BtnUpdate_Click(sender, e);

            lessonSDate.Enabled = false;
            lessonEDate.Enabled = false;
        }
        else
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "script", "alert('" + alertmsg + "');", true);
}

    }


    protected void BtnPreview_Click(object sender, EventArgs e)             // Submit Template
    {
        //btndoc.Style.Add("display", "None");
        objData = new clsData();
        string hdrid = "";
        int TemplateId = 0;
        string setMatch = "";
        string matchSelctd = "";
        int length = 0;
        bool validSet = false;
        bool validStep = false;
        int setCriteria = 0;
        int stepCriteria = 0;
        object objTeach = null;
        string teachName = "";
        string message = "";
        string strQuerry = "";
        string skilltype = "";
        int teachId = 0;
        tdReadMsg.InnerHtml = "";
        bool IsVTValid = true;
        bool validonSub = true;
        bool previewSuccess = true;
        DataTable dt = new DataTable();
        ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
        sess = (clsSession)Session["UserSession"];
        // int noOfTrial = Convert.ToInt32(txtNoofTrail.Text);


        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        string qString = "select DSTempSetId from DSTempSet where DSTempHdrId=" + TemplateId + " and ActiveInd='A'";        //get set ids
        DataTable dtSetId = objData.ReturnDataTable(qString, false);

        objData = new clsData();

        // FUnction to asign sets and steps If the lesson plan is a visual lesson 

        IsVTValid = FunInsrtSetStepVT();

        if (IsVTValid == true)
        {
            try
            {
                //string skilltype = Convert.ToString(objData.FetchValue("SELECT SkillType FROM [dbo].[DSTempHdr] WHERE StudentId='" + sess.StudentId + "' and DSTempHdrId='" + TemplateId + "'"));
                strQuerry = "SELECT SkillType,TeachingProcId FROM [dbo].[DSTempHdr] WHERE StudentId='" + sess.StudentId + "' and DSTempHdrId=" + TemplateId;
                DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
                if (dtNew != null)
                {
                    if (dtNew.Rows.Count > 0)
                    {
                        skilltype = dtNew.Rows[0]["SkillType"].ToString();
                        try
                        {
                            teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                        }
                        catch
                        {
                            teachId = 0;
                        }
                        if (teachId > 0)
                        {
                            try
                            {
                                strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                                objTeach = objData.FetchValue(strQuerry);
                                if (objTeach != null)
                                {
                                    teachName = objTeach.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                previewSuccess = false;
                                throw ex;
                            }
                        }
                    }
                }

                if (skilltype == "Chained")
                {
                    //hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));

                    hdrid = Convert.ToString(objData.FetchValue("SELECT  DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' AND IsDynamic=0 INTERSECT SELECT DSTempHdrId FROM  [dbo].[DSTempSetCol] WHERE DSTempHdrId =" + TemplateId + ""));
                    if (hdrid != "")
                    {
                        dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId " +
                                          "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId " +
                                          "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +
                                            "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                    }
                }
                else if (skilltype == "Discrete")
                {
                    hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet]  WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));
                    if (hdrid != "")
                    {
                        dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND  DSTempSetColId IN (SELECT DSTempSetColId " +
                                                      "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +

                                                               "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                    }
                }
                else
                {
                    previewSuccess = false;
                    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. Skill type is missing");
                    FillTypeOfInstruction(TemplateId);
                    return;
                }
                if (hdrid != "")
                {
                    if (dt != null)
                    {

                        int colcnt = dt.Columns.Count;  // colcnt=2 for Discrete and colcnt=3 for chained
                        if (colcnt > 0)
                        {
                            if (colcnt == 3)
                            {
                                if (teachName == "Total Task")
                                {
                                    if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                    {
                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {
                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                        if (setCriteria == 0)
                                        {
                                            previewSuccess = false;
                                            message = "Set criteria details are missing";
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");
                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview..Undefined error.");
                                        }

                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(dt.Rows[0]["STEP"]) > 0)
                                    {
                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND CriteriaType='MOVE UP' " +
                                                        "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='STEP'  AND CriteriaType='MOVE DOWN' " +
                                                        "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["STEPUP"].ToString() == "0")
                                                {
                                                    msg += "Step Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["STEPDOWN"].ToString() == "0")
                                                {
                                                    msg += "Step Movedown ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {
                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        //  setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                        stepCriteria = Convert.ToInt32(dt.Rows[0]["STEP"]);
                                        //if (setCriteria == 0)
                                        //{
                                        //    message = "Set criteria details are missing";
                                        //    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                        //}
                                        if (stepCriteria == 0)
                                        {
                                            previewSuccess = false;
                                            message = "Step criteria details are missing";
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + message + "");
                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");
                                        }
                                    }
                                }
                            }
                            else if (colcnt == 2)
                            {
                                if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                {
                                    validonSub = WriteVTData();

                                    if (validonSub == true)
                                    {

                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {


                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////   GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        previewSuccess = false;
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Sorry..Lesson plan submitting failed..Please try again ");
                                    }
                                }
                                else
                                {
                                    setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                    if (setCriteria == 0)
                                    {
                                        previewSuccess = false;
                                        message = "Set criteria details are missing";
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");
                                    }
                                    else
                                    {
                                        previewSuccess = false;
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");
                                    }

                                }

                                if (IsMatchToSample(TemplateId) == true)        //if match to sample
                                {
                                    if (matchSelctd != null)
                                    {

                                        string setId = "";
                                        for (int i = 0; i < dtSetId.Rows.Count; i++)
                                        {
                                            setId = dtSetId.Rows[i]["DSTempSetId"].ToString();
                                            clsMathToSamples.Step[] steps = null;
                                            int setIdint = Convert.ToInt32(setId);
                                            lstMatchSamples.Items.Clear();
                                            matchSelctd = "";
                                            setMatch = "";
                                            EditSetData(setIdint);
                                            string[] arryMatchValue = new string[lstMatchSamples.Items.Count];
                                            if (lstMatchSamples.Items.Count > 0)
                                            {
                                                for (int index = 0; index < lstMatchSamples.Items.Count; index++)
                                                {
                                                    arryMatchValue[index] = lstMatchSamples.Items[index].Value.ToString();
                                                }
                                                for (int arryInt = 0; arryInt < lstMatchSamples.Items.Count; arryInt++)
                                                {
                                                    setMatch += arryMatchValue[arryInt].ToString() + ",";
                                                }
                                                length = setMatch.Length;
                                                matchSelctd = setMatch.ToString().Substring(0, length - 1);
                                                steps = MatchSampDef2(TemplateId, matchSelctd, setIdint);      //get steps
                                            }
                                            //if steps are null previewsuccess false
                                            if (steps == null)
                                            {
                                                previewSuccess = false;
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Samples for some sets are missing.");
                                            }
                                            ClearSetData();
                                            BtnUpdateSetDetails.Visible = false;
                                            btnAddSetDetails.Visible = true;
                                        }

                                    }
                                }
                                //-----
                            }
                            else
                            {
                                previewSuccess = false;
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                            }
                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                        }

                    }
                    else
                    {
                        previewSuccess = false;
                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                    }
                }
                else
                {
                    validSet = SetValidation(TemplateId);
                    validStep = StepValidation(TemplateId);

                    if (skilltype == "Chained")
                    {
                        if (validStep == false)
                        {
                            previewSuccess = false;
                            message = "Steps are missing.";
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");

                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");

                        }
                    }
                    else
                    {
                        if (validSet == false)
                        {
                            previewSuccess = false;
                            message = "Sets are missing.";
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview..." + message + "");

                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");
                        }
                    }

                }

                if (previewSuccess == true)
                {
                    //-----
                    Session["tempOverrideHT"] = null;
                    string sel = "SELECT StdtSessionHdrId FROM StdtSessionHdr WHERE StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND StdtClassId=" + sess.Classid + " AND DSTempHdrId=" + TemplateId + " AND SessionStatusCd='P'";
                    DataTable dtHdrs = objData.ReturnDataTable(sel, false);
                    ViewState["StdtSessHdr"] = null;
                    if (dtHdrs.Rows.Count > 0)
                    {
                        ViewState["StdtSessHdr"] = dtHdrs.Rows[0]["StdtSessionHdrId"].ToString();
                    }

                    if (ViewState["StdtSessHdr"] != null)
                    {
                        string updQry = "DELETE FROM StdtSessionHdr WHERE StdtSessionHdrId=" + ViewState["StdtSessHdr"];
                        int retrn = objData.Execute(updQry);
                        string selqry = "SELECT StdtDSStatId FROM StdtDSStat WHERE StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND DSTempHdrId=" + TemplateId + "";
                        DataTable dtDSStat = objData.ReturnDataTable(selqry, false);

                        //Code for Not Deleting data from Stdtdsstat Preview Issue in Maintenance [16-jul-2020] - Dev 2
                        int MaintStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance' "));
                        int AprovdStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));
                        int LsnStatusId = Convert.ToInt16(objData.FetchValue("select statusid from DSTempHdr where DSTempHdrId =" + TemplateId + " "));
                        if ((MaintStatusId != LsnStatusId) && (AprovdStatusId != LsnStatusId))
                        {
                            if (dtDSStat.Rows.Count > 0)
                            {
                                string dltQry = "DELETE FROM StdtDSStat WHERE DSTempHdrId=" + TemplateId + " ";
                                int dtlRetrn = objData.Execute(dltQry);
                            }
                        }
                        //Code for Not Deleting data from Stdtdsstat Preview Issue in Maintenance [16-jul-2020] - Dev 2
                    }
                }


            }
            catch (Exception Ex)
            {
                previewSuccess = false;
                throw Ex;
            }
        }
        else
        {
            previewSuccess = false;
            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");

        }
        drpTasklist_SelectedIndexChanged1(sender, e);
        if (previewSuccess)
        {
            //  ObjTempSess.TemplateId = TemplateId;
            UpdatePrompt(TemplateId);
            Session["NewTemplateId"] = TemplateId;
            ClientScript.RegisterStartupScript(this.GetType(), "", "LoadDatasheetView();", true);
            // Page.ClientScript.RegisterStartupScript(this.GetType(), "myScript", "LoadDatasheetView();", true);
        }
    }


    private bool SetValidation(int templateId)
    {
        int countSet = 0;
        string selQuerry = "";
        object objVal = null;

        selQuerry = "SELECT COUNT(DSTempSetId) FROM DSTempSet WHERE DSTempHdrId = " + templateId + " AND ActiveInd = 'A'";
        objVal = objData.FetchValue(selQuerry);
        if (objVal != null)
        {
            countSet = Convert.ToInt32(objVal);
        }
        if (countSet > 0)
        {
            return true;
        }
        else
        {
            return false;
        }


    }

    private bool StepValidation(int templateId)
    {
        int countStep = 0;
        object objVal = null;
        string selQuerry = "";
        selQuerry = "SELECT COUNT(DSTempStepId) FROM DSTempStep WHERE DSTempHdrId = " + templateId + " AND ActiveInd = 'A' AND IsDynamic=0";
        objVal = objData.FetchValue(selQuerry);
        if (objVal != null)
        {
            countStep = Convert.ToInt32(objVal);
        }
        if (countStep > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool WriteVTData()
    {
        objData = new clsData();
        oData = new DataClass();
        int indexSp = 0;
        string selctQuerry = "";
        bool returnData = true;
        int templateId = 0;
        if (ViewState["HeaderId"] != null)
        {
            templateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        object objVt = objData.FetchValue(selctQuerry);

        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                try
                {
                    selctQuerry = "sp_InsrtSeStepOnSubmit";
                    indexSp = oData.EcecuteScalar_SpSetStep(selctQuerry, sess.SchoolId, sess.LoginId, templateId, vtId);
                    if (indexSp == 1)
                    {
                        returnData = true;
                    }
                    else
                    {
                        returnData = false;
                    }

                }
                catch (Exception Ex)
                {
                    returnData = false;
                    throw Ex;
                    // tdMsg.InnerHtml = clsGeneral.failedMsg("Set Step Management Failed!!!!! Please try again!!!");
                }
            }
        }

        return returnData;
    }

    protected bool FunInsrtSetStepVT()
    {
        objData = new clsData();
        oData = new DataClass();
        int indexSp = 0;
        string selctQuerry = "";
        bool returnData = true;
        int templateId = 0;
        if (ViewState["HeaderId"] != null)
        {
            templateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        selctQuerry = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        object objVt = objData.FetchValue(selctQuerry);

        int noOfTrial = (txtNoofTrail.Text != "") ? Convert.ToInt32(txtNoofTrail.Text) : 0;


        if (objVt.ToString() != "")
        {
            int vtId = Convert.ToInt32(objVt);
            if (vtId > 0)
            {
                try
                {

                    selctQuerry = "UPDATE DSTempHdr SET VTLessonId = " + vtId + ",IsVisualTool = 1,SkillType = 'Discrete', NbrOfTrials = " + noOfTrial + " WHERE DSTempHdrId = " + templateId;
                    indexSp = objData.Execute(selctQuerry);

                    if (indexSp > 0)
                    {
                        returnData = true;
                    }
                    else
                    {
                        returnData = false;
                    }

                    //selctQuerry = "sp_InsrtSeStepOnSubmit";
                    //indexSp = oData.EcecuteScalar_SpSetStep(selctQuerry, sess.SchoolId, sess.LoginId, templateId, vtId);
                    //if (indexSp == 1)
                    //{
                    //    returnData = true;
                    //}
                    //else
                    //{
                    //    returnData = false;
                    //}

                }
                catch (Exception Ex)
                {
                    returnData = false;
                    throw Ex;
                    // tdMsg.InnerHtml = clsGeneral.failedMsg("Set Step Management Failed!!!!! Please try again!!!");
                }
            }
        }

        return returnData;
    }

    private void PENDINGAPPROVE(int templateId)
    {
        int StatusId = 0;
        try
        {
            UpdateSet(templateId);
            UpdatePrompt(templateId);
            StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Pending Approval' "));
            objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + " WHERE DSTempHdrId= " + templateId + "");
            BtnSubmit.Visible = false;
            BtnPreview.Visible = true;
            BtnApproval.Visible = true;
            BtnReject.Visible = true;
            BtnUpdateLessonPlan.Visible = false;
            //  imgApprove.Visible = true;
            //  imgpending.Visible = false;
            //  idLessonType.Visible = false;
            tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Submitted...");
            VisibleApprovalNotes(true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }


    protected void UpdateSet(int templateId)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string strQuerry = "";
        object objcount = null;
        int count = 0;
        int sort = 1;
        try
        {
            strQuerry = "SELECT COUNT(DSTempHdrId) FROM [dbo].[DSTempSet] WHERE ActiveInd = 'A' AND DSTempHdrId = " + templateId;
            objcount = objData.FetchValue(strQuerry);
            if (objcount != null)
            {
                count = Convert.ToInt32(objcount);
            }
            if (count == 0)
            {
                strQuerry = "Insert Into DSTempSet(SchoolId,DSTempHdrId,SetCd,SetName,SortOrder,ActiveInd,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) ";
                strQuerry += " Values(" + sess.SchoolId + "," + templateId + ",'No Set','No Set'," + sort + ",'A'," + sess.LoginId + ",getdate()," + sess.LoginId + " ,getdate()) ";

                objData.Execute(strQuerry);
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }

    protected void UpdatePrompt(int tempId)
    {
        objData = new clsData();
        object objId = null;
        string strQuerry = "";
        int promptId = 0;
        int naId = 0;

        strQuerry = "SELECT ISNULL(PromptTypeId,0) As PromptId FROM DSTempHdr WHERE DSTempHdrId = " + tempId;
        objId = objData.FetchValue(strQuerry);

        if (objId != null)
        {
            promptId = Convert.ToInt32(objId);
        }

        if (promptId == 0)
        {

            strQuerry = "SELECT LookupId FROM LookUp WHERE LookupType = 'Datasheet-Prompt Procedures' AND LookupName = 'NA'";
            objId = objData.FetchValue(strQuerry);
            if (objId != null)
            {
                naId = Convert.ToInt32(objId);
            }

            strQuerry = "UPDATE DSTempHdr SET PromptTypeId = " + naId + " WHERE DSTempHdrId = " + tempId;
            objData.Execute(strQuerry);
        }


    }

    protected void CreateDocument(int tempid, int newtempId)
    {
        try
        {
            objData = new clsData();
            sess = (clsSession)Session["UserSession"];
            DataTable dtdoc = new DataTable();
            clsDocumentasBinary objBinary = new clsDocumentasBinary();
            dtdoc = objData.ReturnDataTable("SELECT LPDoc FROM LPDoc WHERE DSTempHdrId=" + tempid + "", false);
            if (dtdoc != null)
            {
                if (dtdoc.Rows.Count > 0)
                {
                    foreach (DataRow row in dtdoc.Rows)
                    {
                        string strquerry = "INSERT INTO LPDoc(SchoolId,DSTempHdrId,DocURL,CreatedBy,CreatedOn) SELECT SchoolId," + newtempId + ",DocURL," + sess.LoginId + ",GETDATE() FROM LPDoc WHERE LPDoc='" + row["LPDoc"].ToString() + "'";
                        int docid = objData.ExecuteWithScope(strquerry);
                        string binarydata = "SELECT Data,DocumentName FROM binaryFiles WHERE DocId=" + row["LPDoc"].ToString() + " AND type='LP_DOC' ";
                        DataTable dtbinary = objData.ReturnDataTable(binarydata, false);
                        byte[] myData = (byte[])dtbinary.Rows[0]["Data"];
                        string filename = Convert.ToString(dtbinary.Rows[0]["DocumentName"]);
                        int binaryid = objBinary.saveDocument(myData, filename, "", "LP_DOC", docid, "LessonPlanDoc", sess.SchoolId, 0, sess.LoginId);

                    }
                }
            }
            FillDoc(newtempId);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void ResetIOAStatus(int TmpHdrId)
    {
        objData = new clsData();
        string strQry = "UPDATE DSTempHdr SET IsMT_IOA=0 WHERE DSTempHdrId=" + TmpHdrId;
        objData.Execute(strQry);
    }

    protected void BtnMaintenanace_Click(object sender, EventArgs e)         //Maintenance
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();
            string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
            objLessId = objData.FetchValue(selQuerry);
            if (objLessId != null)
            {
                LessonPlanId = Convert.ToInt32(objLessId);
            }

            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance' "));
                    if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                    {
                        objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusId + " AND LessonPlanId = " + LessonPlanId + "");
                    }

                    objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + " , DSMode='MAINTENANCE' WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;

                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnMaintenance.Visible = false;
                    BtnMakeRegular.Visible = false;
                    BtnCopyTemplate.Visible = false;
                    BtnExportTemplate.Visible = false;
                    btnFromReject.Visible = false;
                    btnFromRejectDup.Visible = false;
                    GetStatus(TemplateId);
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    FillMaintenanceLessonData();
                    FillInactiveLessonData();
                    drpTasklist_SelectedIndexChanged1(sender, e);
                    VisibleApprovalNotes(false);
                    VisibleApprovalNote();
                    ResetIOAStatus(TemplateId);
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Go To Maintenance Mode...");

                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
        // Delete the Active session 
        if (TemplateId > 0)
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "parent.ChangeAcitveSess(" + sess.StudentId + TemplateId + ");", true);
        }
    }


    protected void BtnMakeRegular_Click(object sender, EventArgs e)         //Make it Regular
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        BtnMakeRegular.Visible = false;
        tdReadMsg.InnerHtml = "";
        string chkver = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        objData = new clsData();
        string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
        objLessId = objData.FetchValue(selQuerry);
        if (objLessId != null)
        {
            LessonPlanId = Convert.ToInt32(objLessId);
        }
        string selQuerrychk = "SELECT Dstemplatename FROM DSTempHdr WHERE statusid in(select LookupId from [lookup] where LookupName ='approved' and LookupType='templatestatus' ) and StudentId=" + sess.StudentId + " and LessonPlanId=" + LessonPlanId;
        chkver = Convert.ToString((objData.FetchValue(selQuerrychk)));


        if (chkver == null || chkver == "")
        {
                if (sess != null)
                {
                    try
                    {
                        int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));
                        if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                        {
                            objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusId + " AND LessonPlanId = " + LessonPlanId + "");
                        }

                        objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + " , DSMode='' WHERE DSTempHdrId= " + TemplateId + "");
                        objData.Execute("Update StdtDSStat set [statusMessage]='NOT COMPLETED' WHERE DSTempHdrId= " + TemplateId + "");
                        BtnSubmit.Visible = false;
                        // BtnPreview.Visible = false;
                        BtnApproval.Visible = false;
                        BtnReject.Visible = false;
                        BtnMaintenance.Visible = false;
                        BtnExportTemplate.Visible = false;
                        BtnCopyTemplate.Visible = false;
                        btnFromReject.Visible = false;
                        btnFromRejectDup.Visible = false;
                        GetStatus(TemplateId);
                        FillData();                                      // Fill the goal type assigned for the student
                        FillApprovedLessonData();
                        FillCompltdLessonPlans();
                        FillRejectedLessons();
                        FillMaintenanceLessonData();
                        FillInactiveLessonData();
                        VisibleApprovalNotes(false);
                        VisibleApprovalNote();
                        tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Approved....");

                    }
                    catch (Exception Ex)
                    {
                        throw Ex;
                    }
                }
            
            drpTasklist_SelectedIndexChanged1(sender, e);
        }
        else
        {
            GetStatus(TemplateId);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "script", "alert('Do you want to replace the lesson plan " + chkver + " with a lower version? Please note that you cannot run more than one version of the same lesson. If you need to have a duplicate of this lesson please use the copy command.');", true);
        }
                   


    }

    protected void BtnApproval_Click(object sender, EventArgs e)         //Approval
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;

        object selVal = null;
        int tempId = 0;

        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();
            string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
            objLessId = objData.FetchValue(selQuerry);
            if (objLessId != null)
            {
                LessonPlanId = Convert.ToInt32(objLessId);
            }

            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));
                    if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and (StatusId=" + StatusId + " OR StatusId=(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance')) and LessonPlanId=" + LessonPlanId + ""))
                    {
                        objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive'),ModifiedOn=GETDATE()  WHERE StudentId= " + sess.StudentId + " and (StatusId=" + StatusId + " OR StatusId=(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance')) AND LessonPlanId = " + LessonPlanId + "");
                    }

                    objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + ",ModifiedBy=" + sess.LoginId + ",ModifiedOn=GETDATE() WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;
                    BtnPreview.Visible = true;
                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnMakeRegular.Visible = false;
                    BtnCopyTemplate.Visible = false;
                    BtnExportTemplate.Visible = true;
                    btnFromReject.Visible = false;
                    btnFromRejectDup.Visible = false;
                    GetStatus(TemplateId);
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    FillMaintenanceLessonData();
                    FillInactiveLessonData();
                    drpTasklist_SelectedIndexChanged1(sender, e);
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Approved...");
                    BtnApproval_hdn.Visible = false;
                    string VerNbr = Convert.ToString(objData.FetchValue("Select VerNbr from DSTempHdr where DSTemphdrId = " + TemplateId + ""));
                    if (VerNbr != "")
                    {
                        String insertquery = "Insert into StdtSessEvent (SchoolId, StudentId, ClassId, LessonPlanId, DSTempHdrId, EventName, StdtSessEventType, EvntTs, CreatedBy, CreatedOn, ModifiedOn, EventType, TimeStampForReport) Values (" + sess.SchoolId + ", " + sess.StudentId + ", " + sess.Classid + ", " + objLessId + ", " + TemplateId + ", 'LP modified', 'Major', GETDATE(), " + sess.LoginId + ", GETDATE() , GETDATE(), 'EV', GETDATE())";
                        objData.Execute(insertquery);
                    }

                    VisibleApprovalNotes(false);
                    VisibleApprovalNote();

                    //Delete from Datasheet Active Session

                    int StatusVal = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive' "));
                    if (objData.IFExists("SELECT LessonPlanId FROM DSTempHdr WHERE StatusId=" + StatusVal + " AND LessonPlanId=" + LessonPlanId + " AND StudentId=" + sess.StudentId + ""))
                    {
                        string selQry = "SELECT MAX(DSTempHdrId) FROM DSTempHdr WHERE DSTempHdrId <>" + TemplateId + " AND LessonPlanId=" + LessonPlanId + " AND StudentId=" + sess.StudentId;
                        selVal = objData.FetchValue(selQry);

                        if (selVal != null)
                        {
                            tempId = Convert.ToInt32(selVal);
                            if (tempId > 0)
                            {
                                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "parent.ChangeAcitveSess(" + sess.StudentId + tempId + ");", true);
                            }
                        }
                    }
                    Session["GoalID_Approved"] = objData.FetchValue("SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + TemplateId + "')");
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);

    }


    protected void BtnReject_Click(object sender, EventArgs e)         //Reject Lesson Plan
    {
        btnrejectedNotes.Visible = true;
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();
            string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
            objLessId = objData.FetchValue(selQuerry);
            if (objLessId != null)
            {
                LessonPlanId = Convert.ToInt32(objLessId);
            }

            string txtCommentLessonInfoP        = txtCommentLessonInfo.Text.Trim().Replace("'", "''");
            string txtCommentTypeofInstrP       = txtCommentTypeofInstr.Text.Trim().Replace("'", "''");
            string txtMeasurementSystemsP       = txtMeasurementSystems.Text.Trim().Replace("'", "''");
            string txtcommentsetP               = txtcommentset.Text.Trim().Replace("'", "''");
            string txtcommentStepP              = txtcommentStep.Text.Trim().Replace("'", "''");
            string txtcommentPromptP            = txtcommentPrompt.Text.Trim().Replace("'", "''");
            string txtcommentLessonProcedureP   = txtcommentLessonProcedure.Text.Trim().Replace("'", "''");

            string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteLessonInfo='" + txtCommentLessonInfoP + "'," +
                                                      "ApprNoteTypeInstruction='" + txtCommentTypeofInstrP + "'," +
                                                      "ApprNoteMeasurement='" + txtMeasurementSystemsP + "'," +
                                                      "ApprNoteSet='" + txtcommentsetP + "'," +
                                                      "ApprNoteStep='" + txtcommentStepP + "'," +
                                                      "ApprNotePrompt='" + txtcommentPromptP + "'," +
                                                      "ApprNoteLessonProc='" + txtcommentLessonProcedureP + "' " +
                                                      "WHERE DSTempHdrId='" + TemplateId + "'";
            objData.Execute(UpdateAppr);
   
            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired' "));
                    //if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                    //{
                    //    objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE StudentId= " + sess.StudentId + " AND LessonPlanId = " + LessonPlanId + "");
                    //}
                    string RejectedNote = Convert.ToString(objData.FetchValue("SELECT RejectedReason FROM DSTempHdr WHERE DSTempHdrId= " + TemplateId + ""));
                    //RejectedNote = (RejectedNote != "") ? RejectedNote + "_&_" + txtReason.Text + "_&_" + DateTime.Now.ToString() : txtReason.Text + "_&_" + DateTime.Now.ToString();
					RejectedNote = (RejectedNote != "") ? RejectedNote + "_&_" + clsGeneral.convertQuotes(txtReason.Text.Trim()) + "_&_" + DateTime.Now.ToString() : txtReason.Text + "_&_" + DateTime.Now.ToString();

                    //objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + ",RejectedReason='" + RejectedNote + "',ModifiedOn=GETDATE() WHERE DSTempHdrId= " + TemplateId + "");
					objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + ",RejectedReason='" + clsGeneral.convertQuotes(RejectedNote) + "',ModifiedOn=GETDATE() WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;
                    BtnPreview.Visible = false;
                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnMaintenance.Visible = false;
                    GetStatus(TemplateId);
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    FillMaintenanceLessonData();
                    FillInactiveLessonData();
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Rejected...");
                    VisibleApprovalNotes(false);
                    VisibleApprovalNote();
                    btnFromReject.Visible = GetRejectedLPsInInProgressStatus(TemplateId);
                    if (btnFromReject.Visible == false) btnFromRejectDup.Visible = true;
                    else btnFromRejectDup.Visible = false;
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    protected void imgCreateEqutn_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string strQuerry = "";
        int headerId = 0;
        string columnName = "";
        txtCalcuType.Visible = true;
        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        string currentColumnName = txtColumnName.Text.ToString();
        if (currentColumnName == "")
        {
            tdMsgMeasure.InnerHtml = clsGeneral.warningMsg("Please enter Current Column Name...");
            return;
        }
        divBtn.Visible = true;
        strQuerry = "SELECT DSTempSetColId,ColName FROM DSTempSetCol WHERE DSTempHdrId = " + headerId + " AND ColTypeCd = 'Text' AND ActiveInd = 'A'";
        DataTable dtList = objData.ReturnDataTable(strQuerry, false);

        currentColumnName = "<input type='button' id='BtnCurrntClm' class='lbtn' onclick='getValueBtn(this.value)' style='width: 100px' value='" + currentColumnName + "' alt='" + currentColumnName + "' />";
        if (dtList != null)
        {
            if (dtList.Rows.Count > 0)
            {
                if (Convert.ToBoolean(ViewState["New"]) == false) currentColumnName = "";
            }
        }

        divBtn.InnerHtml = " <br />  " +
                           "  <input type='button' id='NFButton' class='lbtn' onclick='getValueBtn(this.value)' style='width: 95px' value='Sum(' alt='Sum(' />  " +
                           " <input type='button' id='Button1' class='lbtn' onclick='getValueBtn(this.value)' style='width: 95px' value='Avg(' alt='Avg(' />" +
                           "<input type='button' id='imgClear' class='smllbtn' onclick='cAsgn()' style='height: 23px' value='C' alt='C' />" +
                           "   <input type='button' id='Button17' class='smllbtn' onclick='backspace()' style='width: 32px' value='BS' alt='BS' />" +
                           " <input type='button' id='Button2' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='+' alt='+' />" +
                           " <input type='button' id='Button3' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='-' alt='-' />" +
                           "  <input type='button' id='Button4' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='*' alt='*' />" +
                           " <input type='button' id='Button5' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='/' alt='/' />" +
                           " <input type='button' id='Button6' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='%' alt='%' />" +
                           " <input type='button' id='Button7' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='(' alt='(' />" +
                           " <input type='button' id='Button8' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value=')' alt=')' />" +
                           "  <br />" +
                           " <input type='button' id='Button9' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='0' alt='0' />" +
                           " <input type='button' id='Button10' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='1' alt='1' />" +
                           "<input type='button' id='Button11' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='2' alt='2' />" +
                           " <input type='button' id='Button12' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='3' alt='3' />" +
                           " <input type='button' id='Button13' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='4' alt='4' />" +
                           " <input type='button' id='Button14' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='5' alt='5' />" +
                           " <input type='button' id='Button15' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='6' alt='6' />" +
                           " <input type='button' id='Button16' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='7' alt='7' />" +
                           "   <input type='button' id='Button17' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='8' alt='8' />" +
                           " <input type='button' id='Button18' class='smllbtn' onclick='getValueBtn(this.value)' style='width: 32px' value='9' alt='9' />" + currentColumnName;


        if (dtList != null)
        {
            if (dtList.Rows.Count > 0)
            {
                for (int i = 0; i < dtList.Rows.Count; i++)
                {
                    columnName = dtList.Rows[i]["ColName"].ToString();
                    if (columnName.Length > 15)
                    {
                        columnName = columnName.ToString().Substring(0, 15) + "...";
                    }
                    divBtn.InnerHtml += " <input type='button' id='Button" + columnName + "' class='lbtn' onclick='getValueBtn(this.value)' style='width: 100px' value='" + columnName + "' alt='" + columnName + "' />";


                }
            }
        }

        //" <%--<input type='button' id='Button19' style='width: 162px' value='OK' alt='OK' />--%>";







        //divBtn.InnerHtml += "<input type='button' class='NFBUTTON' id='btnEdit' value = 'New' onclick='getValueBtn(this.value)'>";

        // divBtn.InnerHtml = 

        //     divBtn.InnerHtml += "<input type='button' class='NFBUTTON' id='btnEdit' onclick='getValueBtn(Edit Member);'>";

        //Button btnTest = new Button();
        //btnTest.ID = "btnEdit";
        //btnTest.Text = "Edit Member";
        //btnTest.CssClass = "NFBUTTON";

        //btnTest.OnClientClick = "return getValueBtn(" + btnTest.Text + ")";
        ////   btnTest.Click += new EventHandler(btnEdit_Click);
        //divBtn.Controls.Add(btnTest);
    }



    protected void imageCollapseDiv_Click(object sender, EventArgs e)
    {
        divBtn.Visible = false;
        txtCalcuType.Visible = false;
    }


    protected void btnRefresh_Click(object sender, ImageClickEventArgs e)
    {



        LoadData();
        lessonSDate.Text = "";
        lessonEDate.Text = "";        btndoc.Visible = false;
        btnDelLp.Visible = false;
        btnrejectedNotes.Visible = false;
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        BtnSubmit.Visible = false;
        BtnPreview.Visible = false;
        BtnCopyTemplate.Visible = false;
        BtnExportTemplate.Visible = false;
        BtnActive.Visible = false;
        BtnApproval.Visible = false;
        BtnReject.Visible = false;
        BtnMaintenance.Visible = false;
        BtnInactive.Visible = false;
        BtnMakeRegular.Visible = false;
        LoadTemplateData(0);
        BtnUpdateLessonPlan.Visible = false;
    }


    protected void chkCurrentPrompt_CheckedChanged(object sender, EventArgs e)
    {
        if (chkCurrentPrompt.Checked == true)
        {
            divCurrentPrompt.Visible = false;
        }
        else
        {
            divCurrentPrompt.Visible = true;
        }
    }

    protected void btneditMode_Click(object sender, EventArgs e)
    {
        //Edit mode on Visual Lesson asigned.                          
        svc_lessonManagement.ResponseOut response = new svc_lessonManagement.ResponseOut();
        objData = new clsData();
        ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
        sess = (clsSession)Session["UserSession"];
        int dsHeadrId = 0;
        int vtLessonId = 0;
        string catName = "";
        if (ViewState["HeaderId"] != null)
        {
            dsHeadrId = Convert.ToInt32(ViewState["HeaderId"]);
            string selData = "SELECT VTLessonId FROM DSTempHdr WHERE DSTempHdrId = " + dsHeadrId;
            vtLessonId = Convert.ToInt32(objData.FetchValue(selData));
            Session["EditValue"] = vtLessonId;                                     //Get the current Visual LessonId

            // Directly redirecting to editor page on editmode with selected Visual lessonID.
            Session["VTLessonID"] = vtLessonId;
            req.lesnId = vtLessonId;
            response = lp.EditLessonData(req);
            svc_lessonManagement.output[] listData = response.outputList;           // WebService to redirect the current LessonType.
            catName = listData.ElementAt(0).lpType;

            if (catName == "mouse")
            {
                Response.Redirect("~/VisualTool/StudentBinder_mouseEditor.aspx?edit=1&curlesId=" + dsHeadrId + "");

            }
            else if (catName == "time")
            {
                Response.Redirect("~/VisualTool/StudentBinder_timeEditor.aspx?edit=1&curlesId=" + dsHeadrId + "");
            }
            else if (catName == "match")
            {
                Response.Redirect("~/VisualTool/StudentBinder_MatchingLessons.aspx?edit=1&curlesId=" + dsHeadrId + "");
            }
            else if (catName == "coin")
            {
                Response.Redirect("~/VisualTool/StudentBinder_CoinLessons.aspx?edit=1&curlesId=" + dsHeadrId + "");
            }

            else
            {
                Session["EditValue"] = null;
            }
        }
    }



    protected void btnconvrtLessonPlan_Click(object sender, EventArgs e)
    {
        //Convert LessonPlan Method
        int studId = 0;
        int dsHeadrId = 0;
        int lessonPlanId = 0;
        string updateHeader = "";
        int indexId = 0;
        objData = new clsData();
        oData = new DataClass();
        ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
        sess = (clsSession)Session["UserSession"];

        if (ViewState["HeaderId"] != null)
        {
            dsHeadrId = Convert.ToInt32(ViewState["HeaderId"]);
            ObjTempSess.TemplateId = dsHeadrId;                     // current lessonId keeps in a session for accessing visual lessons

            studId = sess.StudentId;
            if (btnconvrtLessonPlan.Text == "Convert To VisualTool")           // If LessonPlan COnvert to VIsual Lp, it will redirect to Visual Lesson List.
            {
                Response.Redirect("~/VisualTool/VisualLessonsList.aspx");
            }
            else if (btnconvrtLessonPlan.Text == "Convert To NonVisualTool")
            {
                // Convert VIsual Lesson to NonVisual Lesson                               

                //  dsHeadrId = Convert.ToInt32(ObjTempSess.TemplateId);
                //   lessonPlanId = Convert.ToInt32(ObjTempSess.LessonPlanId);
                updateHeader = "UPDATE DSTempHdr SET IsVisualTool = 0 WHERE DSTempHdrId = " + dsHeadrId;
                int index = objData.Execute(updateHeader);
                try
                {
                    string selctQuerry = "sp_DeleteForNonVT";                            // SP to delete all the   new set rows,step rows, column rows before assigning a non- visual lesson.          
                    indexId = oData.Execute_SpDeletion(selctQuerry, dsHeadrId);

                    btnconvrtLessonPlan.Text = "NonVisualTool";
                    Response.Redirect("CustomizeTemplateEditor.aspx?vLessonId=" + dsHeadrId + "");
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }

        }
    }
    protected void btnreasignNewLsn_Click(object sender, EventArgs e)
    {
        int dsHeadrId = 0;
        Session["ReAssign"] = 1;
        if (ViewState["HeaderId"] != null)
        {
            dsHeadrId = Convert.ToInt32(ViewState["HeaderId"]);
            ObjTempSess.TemplateId = dsHeadrId;
        }
        Response.Redirect("~/VisualTool/VisualLessonsList.aspx");
    }



    protected void BtnVTSavePrompt_Click(object sender, EventArgs e)
    {
        objData = new clsData();

        string insertQuerry = "";
        int headerId = 0;
        int returnId = 0;
        int index = 0;
        int count = 0;


        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }


        string selQuerry = "SELECT COUNT(DSTempSetColId) FROM DSTempSetCol WHERE ColName = 'Column3' AND ColTypeCd = 'Prompt' AND ActiveInd = 'A' AND DSTempHdrId = " + headerId;
        count = Convert.ToInt32(objData.FetchValue(selQuerry));

        if (count == 0)
        {

            SqlTransaction Transs = null;
            SqlConnection con = objData.Open();

            try
            {
                clsData.blnTrans = true;
                Transs = con.BeginTransaction();

                insertQuerry = "Insert Into dbo.DSTempSetCol (SchoolId,DSTempHdrId,ColName,ColTypeCd,IncMisTrialInd" +
                                      ",CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd) " +
                                      "Values(" + sess.SchoolId + "," + headerId + ",'Column3','Prompt' ,0," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A') ";
                returnId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));


                insertQuerry = "Insert Into DSTempSetColCalc (SchoolId,DSTempSetColId,CalcType,CalcRptLabel,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn,ActiveInd) " +
                                   "Values(" + sess.SchoolId + "," + returnId + ",'%Accuracy','%Accuracy'," + sess.LoginId + ",getdate()," + sess.LoginId + ",getdate(),'A') ";
                index = Convert.ToInt32(objData.ExecuteWithScopeandConnection(insertQuerry, con, Transs));

                objData.CommitTransation(Transs, con);
                GetMeasureData(headerId);

                IsVisualTool(headerId);
                //  SetLessonProcedure(headerId);

            }
            catch (Exception Ex)
            {
                objData.RollBackTransation(Transs, con);
                string error = Ex.Message;
                tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Insertion Failed! <br> '" + error + "' ");
            }

        }
        else
        {
            // ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlreadyPromptDef();", true);

            // code to delete prompt
            string delIdQuery = "SELECT DSTempSetColId FROM DSTempSetCol WHERE ColName = 'Column3' AND ColTypeCd = 'Prompt' AND ActiveInd = 'A' AND DSTempHdrId = " + headerId;

            DataTable dt = objData.ReturnDataTable(delIdQuery, false);

            if (dt.Rows.Count > 0)
            {

                int delId = Convert.ToInt32(dt.Rows[0]["DSTempSetColId"].ToString());

                SqlTransaction Transs = null;
                SqlConnection con = objData.Open();

                try
                {
                    clsData.blnTrans = true;
                    Transs = con.BeginTransaction();

                    insertQuerry = "DELETE FROM dbo.DSTempSetCol WHERE DSTempSetColId =" + delId;
                    returnId = Convert.ToInt32(objData.ExecuteWithTrans(insertQuerry, con, Transs));


                    insertQuerry = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId=" + delId;
                    index = Convert.ToInt32(objData.ExecuteWithTrans(insertQuerry, con, Transs));

                    objData.CommitTransation(Transs, con);
                    GetMeasureData(headerId);
                    IsVisualTool(headerId);

                }
                catch (Exception Ex)
                {
                    objData.RollBackTransation(Transs, con);
                    string error = Ex.Message;
                    tdMsgMeasure.InnerHtml = clsGeneral.failedMsg("Deletion Failed! <br> '" + error + "' ");
                }
            }


            //end
        }


    }
    protected void imgUp_Click(object sender, ImageClickEventArgs e)
    {
        objData = new clsData();
        try
        {
            ImageButton btnUp = (ImageButton)sender;
            int setId = Convert.ToInt32(btnUp.CommandArgument);
            string Query = "SELECT SortOrder FROM DSTempSet WHERE DSTempSetId='" + setId + "'";
            int SortOrder = Convert.ToInt32(objData.FetchValue(Query));
            if (SortOrder != 1)
            {
                DataTable OldSortOrder = objData.ReturnDataTable("SELECT DSTempSetId,SortOrder FROM DSTempSet WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempSet WHERE DSTempSetId='" + setId + "') AND [ActiveInd]='A' ORDER BY SortOrder DESC ", false);
                if (OldSortOrder != null)
                {
                    for (int i = (SortOrder - 1); i > 0; i--)
                    {
                        foreach (DataRow row in OldSortOrder.Rows)
                        {
                            if (i == Convert.ToInt32(row["SortOrder"].ToString()))
                            {
                                string Update = "UPDATE DSTempSet SET SortOrder='" + i + "' WHERE DSTempSetId='" + setId + "'";
                                objData.Execute(Update);
                                string Update1 = "UPDATE DSTempSet SET SortOrder='" + SortOrder + "' WHERE DSTempSetId='" + row["DSTempSetId"] + "'";
                                objData.Execute(Update1);
                                //Query = "SELECT DSTempSetId FROM DSTempStep WHERE DSTempSetId='" + stepId + "'";
                                //int step_Id = Convert.ToInt32(objData.FetchValue(Query));
                                //UpdateCompleteSteponParentSet(setId, Convert.ToInt32(ViewState["HeaderId"]));
                                GetSetData(Convert.ToInt32(ViewState["HeaderId"]));
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "alert('Sorry..Unable to shuffle Sets')", true);
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void imgDown_Click(object sender, ImageClickEventArgs e)
    {
        objData = new clsData();
        try
        {
            ImageButton btnUp = (ImageButton)sender;
            int setId = Convert.ToInt32(btnUp.CommandArgument);
            string Query = "SELECT SortOrder FROM DSTempSet WHERE DSTempSetId='" + setId + "'";
            int SortOrder = Convert.ToInt32(objData.FetchValue(Query));
            if (Convert.ToInt32(objData.FetchValue("SELECT MAX(SortOrder) FROM DSTempSet WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempSet WHERE DSTempSetId='" + setId + "') AND [ActiveInd]='A'")) > SortOrder)
            {
                DataTable OldSortOrder = objData.ReturnDataTable("SELECT DSTempSetId,SortOrder FROM DSTempSet WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempSet WHERE DSTempSetId='" + setId + "') AND [ActiveInd]='A' ORDER BY SortOrder ", false);
                if (OldSortOrder != null)
                {
                    for (int i = (SortOrder + 1); i > 0; i++)
                    {
                        foreach (DataRow row in OldSortOrder.Rows)
                        {
                            if (i == Convert.ToInt32(row["SortOrder"].ToString()))
                            {
                                string Update = "UPDATE DSTempSet SET SortOrder='" + i + "' WHERE DSTempSetId='" + setId + "'";
                                string Update1 = "UPDATE DSTempSet SET SortOrder='" + SortOrder + "' WHERE DSTempSetId='" + row["DSTempSetId"] + "'";
                                objData.Execute(Update);
                                objData.Execute(Update1);
                                //UpdateCompleteSteponParentSet(setId, Convert.ToInt32(ViewState["HeaderId"]));
                                GetSetData(Convert.ToInt32(ViewState["HeaderId"]));
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "alert('Sorry..Unable to shuffle Sets')", true);
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    //protected void imgUp_Click(object sender, ImageClickEventArgs e)
    //{
    //    objData = new clsData();
    //    try
    //    {
    //        ImageButton btnUp = (ImageButton)sender;
    //        int stepId = Convert.ToInt32(btnUp.CommandArgument);
    //        string Query = "SELECT SortOrder FROM DSTempParentStep WHERE DSTempParentStepId='" + stepId + "'";
    //        int SortOrder = Convert.ToInt32(objData.FetchValue(Query));
    //        if (SortOrder != 1)
    //        {
    //            DataTable OldSortOrder = objData.ReturnDataTable("SELECT DSTempParentStepId,SortOrder FROM DSTempParentStep WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempParentStep WHERE DSTempParentStepId='" + stepId + "') AND [ActiveInd]='A' ORDER BY SortOrder DESC ", false);
    //            if (OldSortOrder != null)
    //            {
    //                for (int i = (SortOrder - 1); i > 0; i--)
    //                {
    //                    foreach (DataRow row in OldSortOrder.Rows)
    //                    {
    //                        if (i == Convert.ToInt32(row["SortOrder"].ToString()))
    //                        {
    //                            string Update = "UPDATE DSTempParentStep SET SortOrder='" + i + "' WHERE DSTempParentStepId='" + stepId + "'";
    //                            objData.Execute(Update);
    //                            string Update1 = "UPDATE DSTempParentStep SET SortOrder='" + SortOrder + "' WHERE DSTempParentStepId='" + row["DSTempParentStepId"] + "'";
    //                            objData.Execute(Update1);
    //                            //Query = "SELECT DSTempSetId FROM DSTempStep WHERE DSTempParentStepId='" + stepId + "'";
    //                            //int step_Id = Convert.ToInt32(objData.FetchValue(Query));
    //                            UpdateCompleteSteponParentSet(stepId, Convert.ToInt32(ViewState["HeaderId"]));
    //                            GetStepData(Convert.ToInt32(ViewState["HeaderId"]));
    //                            return;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "alert('Sorry..Unable to shuffle Steps with different parent Sets')", true);
    //        }
    //    }
    //    catch (Exception Ex)
    //    {
    //        throw Ex;
    //    }
    //}
    //protected void imgDown_Click(object sender, ImageClickEventArgs e)
    //{
    //    objData = new clsData();
    //    try
    //    {
    //        ImageButton btnUp = (ImageButton)sender;
    //        int stepId = Convert.ToInt32(btnUp.CommandArgument);
    //        string Query = "SELECT SortOrder FROM DSTempParentStep WHERE DSTempParentStepId='" + stepId + "'";
    //        int SortOrder = Convert.ToInt32(objData.FetchValue(Query));
    //        if (Convert.ToInt32(objData.FetchValue("SELECT MAX(SortOrder) FROM DSTempParentStep WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempParentStep WHERE DSTempParentStepId='" + stepId + "') AND [ActiveInd]='A'")) > SortOrder)
    //        {
    //            DataTable OldSortOrder = objData.ReturnDataTable("SELECT DSTempParentStepId,SortOrder FROM DSTempParentStep WHERE [DSTempHdrId]=(SELECT DSTempHdrId FROM DSTempParentStep WHERE DSTempParentStepId='" + stepId + "') AND [ActiveInd]='A' ORDER BY SortOrder ", false);
    //            if (OldSortOrder != null)
    //            {
    //                for (int i = (SortOrder + 1); i > 0; i++)
    //                {
    //                    foreach (DataRow row in OldSortOrder.Rows)
    //                    {
    //                        if (i == Convert.ToInt32(row["SortOrder"].ToString()))
    //                        {
    //                            string Update = "UPDATE DSTempParentStep SET SortOrder='" + i + "' WHERE DSTempParentStepId='" + stepId + "'";
    //                            string Update1 = "UPDATE DSTempParentStep SET SortOrder='" + SortOrder + "' WHERE DSTempParentStepId='" + row["DSTempParentStepId"] + "'";
    //                            objData.Execute(Update);
    //                            objData.Execute(Update1);
    //                            UpdateCompleteSteponParentSet(stepId, Convert.ToInt32(ViewState["HeaderId"]));
    //                            GetStepData(Convert.ToInt32(ViewState["HeaderId"]));
    //                            return;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "alert('Sorry..Unable to shuffle Steps with different parent Sets')", true);
    //        }

    //    }
    //    catch (Exception Ex)
    //    {
    //        throw Ex;
    //    }
    //}
    private bool ModificationExit()
    {
        int countSet = 0;
        int headerId = 0;
        bool IsExit = false;
        objData = new clsData();

        if (ViewState["HeaderId"] != null)
        {
            headerId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        if (headerId > 0)
        {
            string selQuerry = "  Select count(*) from DSTempRule DSR  Where DSR.RuleType ='SET' And  DSR.CriteriaType='MODIFICATION' And  DSR.ActiveInd='A' And DSR.SchoolId=" + sess.SchoolId + "   And DSTempHdrId=" + headerId + " ";

            object objVal = objData.FetchValue(selQuerry);
            if (objVal != null)
            {
                countSet = Convert.ToInt32(objVal);
            }
            if (countSet > 0)
            {
                IsExit = true;
            }
            else
            {
                IsExit = false;
            }
        }
        return IsExit;

    }
    protected void ddlCriteriaType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlCriteriaType.SelectedIndex == 3)
        {
            if (ModificationExit() == true)
            {
                ddlCriteriaType.SelectedIndex = 0;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "ModificationExit();", true);
                return;
            }
            trModification.Style.Add("visibility", "visible");
            trModification1.Style.Add("visibility", "visible");
            setModificationVisibility(false);
        }
        else
        {
            trModification.Style.Add("visibility", "hidden");
            trModification1.Style.Add("visibility", "hidden");
            setModificationVisibility(true);

            ApplyAllDisableVisual();
        }
    }
    protected void chkIsComments_CheckedChanged(object sender, EventArgs e)
    {
        chkScore.Checked = false;
        txtModNo.Enabled = false;
        txtModNo.Text = "";
        txtModComments.Enabled = true;
        IsComments.Value = "1";
    }
    protected void chkScore_CheckedChanged(object sender, EventArgs e)
    {
        chkIsComments.Checked = false;
        txtModComments.Enabled = false;
        txtModNo.Enabled = true;
        txtModComments.Text = "";
        IsComments.Value = "0";
    }
    protected void btnFromReject_Click(object sender, EventArgs e)
    {
        btnrejectedNotes.Visible = false;
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        lessonSDate.Enabled = true;
        lessonEDate.Enabled = true;
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();


            if (sess != null)
            {
                try
                {
                    int StatusIdR = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired' "));
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='In Progress' "));
                    if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusIdR + " and DSTempHdrId=" + TemplateId + ""))
                    {
                        objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + "  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusIdR + " AND DSTempHdrId = " + TemplateId + "");
                    }

                    // objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + " WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;
                    BtnPreview.Visible = false;
                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnMaintenance.Visible = false;
                    GetStatus(TemplateId);
                    BtnReject.Visible = false;
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    btnFromReject.Visible = false;
                    btnFromRejectDup.Visible = false;
                    btndoc.Visible = true;
                    BtnPreview.Visible = true;
                    VisibleApprovalNotesPR();
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template successfully move to in progress...");

                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }


    private void setModificationVisibility(bool Mod)
    {
        if (Hdfsavemeasure.Value != "")
        {
        }
        else
        {
            if (Mod == false)
            {
                rbtnIoaReq.Enabled = false;
                ddlTempColumn.Enabled = false;
                ddlTempMeasure.Enabled = false;
                txtRequiredScore.Enabled = false;
                rbtnMultitchr.Enabled = false;
                rbtnConsectiveSes.Enabled = false;
                rbtnConsectiveAvg.Enabled = false;
                txtNumbrSessions.Enabled = false;
                txtIns1.Enabled = false;
                txtIns2.Enabled = false;

                chkCpyStepCri.Enabled = false;
                chkCpyPromptCri.Enabled = false;
                chkCpySetCri.Enabled = false;
                chkNACriteria.Visible = false;
            }
            else
            {
                rbtnIoaReq.Enabled = true;
                ddlTempColumn.Enabled = true;
                ddlTempMeasure.Enabled = true;
                txtRequiredScore.Enabled = true;
                rbtnMultitchr.Enabled = true;
                rbtnConsectiveSes.Enabled = true;
                rbtnConsectiveAvg.Enabled = true;
                txtNumbrSessions.Enabled = true;
                txtIns1.Enabled = true;
                txtIns2.Enabled = true;

                chkCpyStepCri.Enabled = true;
                chkCpyPromptCri.Enabled = true;
                chkCpySetCri.Enabled = true;
                chkNACriteria.Visible = true;
            }

        }
    }

    protected void BtnReject_Click1(object sender, EventArgs e)
    {
        txtReason.Text = "";
        Page.ClientScript.RegisterStartupScript(this.GetType(), "myfn", "PopupReject();", true);

    }



    protected string GetContentType(string extension)
    {
        string ContentType = "";
        switch (extension)
        {
            case ".txt":
                ContentType = "text/plain";
                break;
            case ".doc":
                ContentType = "application/msword";
                break;
            case ".docx":
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                break;
            case ".pdf":
                ContentType = "application/pdf";
                break;
            case ".xls":
                ContentType = "application/vnd.ms-excel";
                break;
            case ".xlsx":
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                break;
            case ".csv":
                ContentType = "application/vnd.ms-excel";
                break;
        }
        return ContentType;
    }
    protected void btUpload_Click(object sender, EventArgs e)
    {
        try
        {
            if (ViewState["HeaderId"] != null)
            {
                int headerId = Convert.ToInt32(ViewState["HeaderId"]);
                objData = new clsData();
                divMessage.InnerHtml = "";
                clsDocumentasBinary objBinary = new clsDocumentasBinary();
                if (fupDoc.HasFile)
                {
                    string ext = System.IO.Path.GetExtension(fupDoc.FileName);
                    if (ext.ToLower() == ".txt" || ext.ToLower() == ".docx" || ext.ToLower() == ".doc" || ext.ToLower() == ".pdf" || ext.ToLower() == ".csv" || ext.ToLower() == ".xlsx" || ext.ToLower() == ".xls" || ext.ToLower() == ".jpg"
                        ||ext.ToLower() == ".mp4" || ext.ToLower() == ".webm" || ext.ToLower() == ".mkv" || ext.ToLower() == ".flv" || ext.ToLower() == ".vob" || ext.ToLower() == ".ogg" || ext.ToLower() == ".ogv" || ext.ToLower() == ".drc"
                        || ext.ToLower() == ".gif" || ext.ToLower() == ".gifv" || ext.ToLower() == ".mng" || ext.ToLower() == ".avi" || ext.ToLower() == ".MTS" || ext.ToLower() == ".M2TS" || ext.ToLower() == ".TS" || ext.ToLower() == ".mov"
                        || ext.ToLower() == ".qt" || ext.ToLower() == ".wmv" || ext.ToLower() == ".yuv" || ext.ToLower() == ".rm" || ext.ToLower() == ".rmvb" || ext.ToLower() == ".viv" || ext.ToLower() == ".asf" || ext.ToLower() == ".amv"
                        || ext.ToLower() == ".m4p" || ext.ToLower() == ".m4v" || ext.ToLower() == ".mpg" || ext.ToLower() == ".mp2" || ext.ToLower() == ".mpeg" || ext.ToLower() == ".mpe" || ext.ToLower() == ".mpv" || ext.ToLower() == ".m2v"
                        || ext.ToLower() == ".svi" || ext.ToLower() == ".3gp" || ext.ToLower() == ".3g2" || ext.ToLower() == ".mxf" || ext.ToLower() == ".roq" || ext.ToLower() == ".nsv" || ext.ToLower() == ".flv" || ext.ToLower() == ".f4v"
                        || ext.ToLower() == ".f4p" || ext.ToLower() == ".f4a" || ext.ToLower() == ".f4b" )                       
                    {

                        string filename = System.IO.Path.GetFileName(fupDoc.FileName);
                        HttpPostedFile myFile = fupDoc.PostedFile;
                        int nFileLen = myFile.ContentLength;
                            byte[] myData = new byte[nFileLen];
                            myFile.InputStream.Read(myData, 0, nFileLen);
                            string strquerry = "INSERT INTO LPDoc(SchoolId,DSTempHdrId,DocURL,CreatedBy,CreatedOn) values(" + sess.SchoolId + "," + headerId + ",'" + filename + "'," + sess.LoginId + ",GETDATE())";
                            int docid = objData.ExecuteWithScope(strquerry);
                            int binaryid = objBinary.saveDocument(myData, filename, "", "LP_DOC", docid, "LessonPlanDoc", sess.SchoolId, 0, sess.LoginId);
                            FillDocSmall(headerId);
                            ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){popPrompts();});", true);
                       
                        }
                    else
                    {
                        divMessage.InnerHtml = clsGeneral.warningMsg("Invalid file format...");
                        ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){popPrompts();});", true);

                    }
                }
                else
                {
                    divMessage.InnerHtml = clsGeneral.warningMsg("Please select file...");
                    ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){popPrompts();});", true);
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "alert('Please select or add a lesson plan');", true);
            }
        }
        catch (Exception ex)
        {
            lMsg.ForeColor = System.Drawing.Color.Red;
            lMsg.Text = "Error:" + ex.Message.ToString();
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    protected void grdFile_RowEditing(object sender, GridViewEditEventArgs e)
    {
        e.Cancel = true;
        e.NewEditIndex = -1;
        grdFile.DataBind();
    }
    protected void FillDoc(int templateId)
    {
        objData = new clsData();
        string strQuery = "";
        strQuery = "Select ROW_NUMBER() OVER (ORDER BY LPDoc) AS No,DocURL as Document, LPDoc FROM LPDoc Where DocURL<>'' And DSTempHdrId = " + templateId + "";
        DataTable Dt = objData.ReturnDataTable(strQuery, false);
        if (Dt != null)
        {
            if (Dt.Rows.Count > 0)
            {
                Dt.Columns.Add("Name", typeof(string));


                for (int i = 0; i < Dt.Rows.Count; i++)
                {
                    string name = Dt.Rows[i]["Document"].ToString();
                    string[] ext = name.Split('.');
                    string ext_name = ext[1];
                    if (name != "")
                    {
                        if (name.Length > 50)
                        {
                            name = name.Substring(0, 30) + "....";
                            name += ext_name;
                        }
                    }
                    Dt.Rows[i]["Name"] = name;
                }
            }
        }
        grdFile.DataSource = Dt;
        grdFile.DataBind();
    }
    protected void FillDocSmall(int templateId)
    {
        objData = new clsData();
        string strQuery = "";
        strQuery = "Select ROW_NUMBER() OVER (ORDER BY LPDoc) AS No,DocURL as Document, LPDoc FROM LPDoc Where DocURL<>'' And DSTempHdrId = " + templateId + "";
        DataTable Dt = objData.ReturnDataTable(strQuery, false);
        if (Dt != null)
        {
            if (Dt.Rows.Count > 0)
            {
                Dt.Columns.Add("Name", typeof(string));


                for (int i = 0; i < Dt.Rows.Count; i++)
                {
                    string name = Dt.Rows[i]["Document"].ToString();
                    string[] ext = name.Split('.');
                    string ext_name = ext[1];
                    if (name != "")
                    {
                        if (name.Length > 50)
                        {
                            name = name.Substring(0, 30) + "....";
                            name += ext_name;
                        }
                    }
                    Dt.Rows[i]["Name"] = name;
                }
            }
        }
        grdFile.DataSource = Dt;
        grdFile.DataBind();
    }
    protected void grdFile_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        grdFile.PageIndex = e.NewPageIndex;
        int headerid = Convert.ToInt32(ViewState["HeaderId"]);
        FillDocSmall(headerid);
    }
    protected void grdFile_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            divMessage.InnerHtml = "";
            string file = Convert.ToString(e.CommandArgument);
            objData = new clsData();
            if (e.CommandName == "Edit")
            {
                try
                {
                    if (ViewState["HeaderId"] != null)
                    {
                        int headerId = Convert.ToInt32(ViewState["HeaderId"]);
                        deltDoccuments(file);
                        FillDocSmall(headerId);
                    }
                }
                catch
                {

                }

            }
            else if (e.CommandName == "download")
            {
                try
                {
                    HttpResponse Response = HttpContext.Current.Response;
                    Response.Clear();
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.Buffer = true;
                    Byte[] data = (Byte[])objData.FetchValue("SELECT Data FROM binaryFiles WHERE DocId='" + file + "' AND type='LP_DOC' ");
                    string docURL = Convert.ToString(objData.FetchValue("SELECT DocURL FROM LPDoc WHERE LPDoc='" + file + "'"));
                    string contentType = GetContentType(Path.GetExtension(docURL).ToLower().ToString());
                    Response.AddHeader("Content-type", contentType);
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + docURL);
                    Response.BinaryWrite(data);
                    Response.Flush();
                    Response.End();


                }
                catch (Exception ex)
                {
                }
                // DownloadFile(file);
            }

            ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){popPrompts();});", true);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }

    public void deltDoccuments(string fileName)
    {
        string strQuerry = "";
        try
        {
            clsData objData = null;
            objData = new clsData();
            clsSession sess = (clsSession)Session["UserSession"];
            strQuerry = "DELETE FROM LPDoc WHERE  LPDoc = '" + fileName + "' ";
            objData.Execute(strQuerry);
            strQuerry = "DELETE FROM binaryFiles WHERE  DocId = '" + fileName + "' AND type='LP_DOC' ";
            objData.Execute(strQuerry);
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void grdFile_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lbn = e.Row.FindControl("lnkDownload") as LinkButton;
                ScriptManager.GetCurrent(this).RegisterPostBackControl(lbn);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void btnrejectedNotes_Click(object sender, EventArgs e)
    {
        try
        {
            RejectedNote();
            //btnFromReject.Visible = false;//
            ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){ShowRejectedNote();});", true);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    private void RejectedNote()
    {
        objData = new clsData();
        int HeaderId = Convert.ToInt32(ViewState["HeaderId"]);
        sess = (clsSession)Session["UserSession"];
        string RejectedNote = "SELECT RejectedReason AS Note,ModifiedOn AS RejectedDate FROM DSTempHdr WHERE LessonPlanId=(SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + HeaderId + "') AND StudentId='" + sess.StudentId + "' AND DSTempHdrId='" + HeaderId + "' ";
        DataTable dtrejected = objData.ReturnDataTable(RejectedNote, false);
        ClassDatatable oDt = new ClassDatatable();
        DataTable _dtTest = new System.Data.DataTable();
        _dtTest = oDt.CreateColumn("Note", _dtTest);
        _dtTest = oDt.CreateColumn("RejectedDate", _dtTest);
        DataRow drnote = null;
        foreach (DataRow dr in dtrejected.Rows)
        {
            int i = 0;
            string notes = Convert.ToString(dr["Note"]);
            if (notes != "" && notes.Contains("_&_"))
            {
                string[] results = notes.Split(new[] { "_&_" }, StringSplitOptions.None);
                for (int j = 0; j < (results.Count()) / 2; j++)
                {
                    drnote = _dtTest.NewRow();
                    drnote["Note"] = results[i];
                    if (results[i + 1].Contains("/"))
                        drnote["RejectedDate"] = results[i + 1];
                    else
                        drnote["RejectedDate"] = Convert.ToDateTime(results[i + 1]).ToString("MM/dd/yyyy").Replace("-", "/");
                    _dtTest.Rows.Add(drnote);
                    i = i + 2;
                }
                i = 0;
            }
            else
            {
                drnote = _dtTest.NewRow();
                drnote["Note"] = notes;
                drnote["RejectedDate"] = Convert.ToDateTime(dr["RejectedDate"]).ToString("MM/dd/yyyy").Replace("-", "/");
                _dtTest.Rows.Add(drnote);
            }
        }
        GrdRejectedNote.DataSource = _dtTest;
        GrdRejectedNote.DataBind();
    }
    protected void GrdRejectedNote_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        try
        {
            GrdRejectedNote.PageIndex = e.NewPageIndex;
            RejectedNote();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    protected void lnkMaintenanceLp_Click1(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
        lessonSDate.Text = "";
        lessonEDate.Text = "";
        lessonSDate.Enabled = false;
        lessonEDate.Enabled = false;
        Hdfsavemeasure.Value = "1";
        textBoxDisableEnable(false);
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        BtnMakeRegular.Visible = true;
        BtnActive.Visible = false;
        BtnInactive.Visible = false;
        btndoc.Style.Add("display", "None");
        btnrejectedNotes.Visible = false;
        BtnUpdateLessonPlan.Visible = false;
        BtnPreview.Visible = true;
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        // bool visibility = false;
        LinkButton lnkApprvdless = (LinkButton)sender;
        try
        {
            int headerId = Convert.ToInt32(lnkApprvdless.CommandArgument);
            ViewState["HeaderId"] = headerId;

            LoadTemplateData(headerId);

            // CheckAsigned(headerId);

            setWritePermissions(true);

            VisibleApprovalNotes(false);
            VisibleApprovalNote();


        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }




    protected void lnkInactiveLp_Click(object sender, EventArgs e)
    {
        removeTypeOfInstructions(sender);
        lessonSDate.Text = "";
        lessonEDate.Text = "";
        lessonSDate.Enabled = false;
        lessonEDate.Enabled = false;
        Hdfsavemeasure.Value = "1";
        textBoxDisableEnable(false);
        btnFromReject.Visible = false;
        btnFromRejectDup.Visible = false;
        BtnMakeRegular.Visible = false;
        BtnInactive.Visible = false;
        BtnActive.Visible = true;
        btnDelLp.Visible = true;
        btndoc.Style.Add("display", "None");
        btnrejectedNotes.Visible = false;
        BtnSubmit.Visible = false;
        BtnUpdateLessonPlan.Visible = false;
        BtnPreview.Visible = false;
        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        // bool visibility = false;
        LinkButton lnkApprvdless = (LinkButton)sender;
        try
        {
            int headerId = Convert.ToInt32(lnkApprvdless.CommandArgument);
            ViewState["HeaderId"] = headerId;

            LoadTemplateData(headerId);

            // CheckAsigned(headerId);

            setWritePermissions(true);
            BtnCopyTemplate.Visible = false;
            BtnExportTemplate.Visible = true;
            checkversion();
            VisibleApprovalNotes(false);
            VisibleApprovalNote();
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);




    }
    //JIS
    private void checkversion()
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        hfLessonResult.Value = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();
            string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
            objLessId = objData.FetchValue(selQuerry);
            if (objLessId != null)
            {
                LessonPlanId = Convert.ToInt32(objLessId);
            }

            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));

                    if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                    {
                        string verAPR = (objData.FetchValue("SELECT VerNbr FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + "")).ToString();
                        string verINCT = (objData.FetchValue("SELECT VerNbr FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId)).ToString();
                        bool version = checkversionApproved(verAPR, verINCT);
                        if (version)
                        {
                            string lesssonName = (objData.FetchValue("SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId)).ToString();
                            hfLessonResult.Value = lesssonName;
                        }
                        string strQry = "";



                    }

                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

    }






    protected void BtnActive_Click(object sender, EventArgs e)
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        objData = new clsData();
        string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
        objLessId = objData.FetchValue(selQuerry);
        if (objLessId != null)
        {
            LessonPlanId = Convert.ToInt32(objLessId);
        }
        string selQuerrychk = "SELECT Dstemplatename FROM DSTempHdr WHERE statusid in(select LookupId from [lookup] where "+
        " LookupName in ('approved','maintenance') and LookupType='templatestatus' ) and StudentId="+ sess.StudentId + " and  LessonPlanId=" + LessonPlanId;
        string Chkver = Convert.ToString((objData.FetchValue(selQuerrychk)));
       
        if (Chkver == null|| Chkver == "" )
        {                

            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));
                    if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                    {
                        objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive')  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusId + " AND LessonPlanId = " + LessonPlanId + " ");
                    }




                    objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved'),DSMode='' WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;
                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnPreview.Visible = true;
                    // BtnMaintenance.Visible = false;
                    BtnMakeRegular.Visible = false;
                    BtnCopyTemplate.Visible = false;
                    BtnExportTemplate.Visible = false;
                    btnFromReject.Visible = false;
                    btnFromRejectDup.Visible = false;
                    GetStatus(TemplateId);
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    FillMaintenanceLessonData();
                    FillInactiveLessonData();
                    drpTasklist_SelectedIndexChanged1(sender, e);
                    VisibleApprovalNotes(false);
                    VisibleApprovalNote();
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Activated...");
                }



                catch (Exception Ex)
                {
                    throw Ex;
                }
            }       
        drpTasklist_SelectedIndexChanged1(sender, e);
        }
         else
         {
             GetStatus(TemplateId);
             ScriptManager.RegisterStartupScript(this, this.GetType(), "script", "alert('Do you want to replace the lesson plan " + Chkver + " with a lower version? Please note that you cannot run more than one version of the same lesson. If you need to have a duplicate of this lesson please use the copy command.');", true);
         }
    }
    //public void updateApprovedStatus(int TemplateId, int LessonPlanId, int StatusId)
    //{
    //    objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusId + " AND LessonPlanId = " + LessonPlanId + " ");
    //    objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved'),DSMode='' WHERE DSTempHdrId= " + TemplateId + "");
    //    BtnSubmit.Visible = false;
    //    BtnApproval.Visible = false;
    //    BtnReject.Visible = false;
    //    //BtnMaintenance.Visible = false;
    //    BtnMakeRegular.Visible = false;
    //    BtnCopyTemplate.Visible = false;
    //    btnFromReject.Visible = false;
    //    GetStatus(TemplateId);
    //    FillData();                                      // Fill the goal type assigned for the student
    //    FillApprovedLessonData();
    //    FillCompltdLessonPlans();
    //    FillRejectedLessons();
    //    FillMaintenanceLessonData();
    //    FillInactiveLessonData();
    //    drpTasklist_SelectedIndexChanged1(sender, e);
    //    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Activated...");
    //}

    protected bool checkversionApproved(string ver1, string ver2)
    {

        if (ver2 != null && ver2 != "")
        {
            if (ver1 == "" || ver1 == null)
            {
                ver1 = "0.0";
            }
            string[] tempver1 = ver1.Split('.');
            int first1 = Convert.ToInt32(tempver1[0]);
            int second1 = Convert.ToInt32(tempver1[1]);
            String[] tempver2 = ver2.Split('.');
            int first2 = Convert.ToInt32(tempver2[0]);
            int second2 = Convert.ToInt32(tempver2[1]);
            if (first1 == first2)
            {
                if (second1 > second2)
                {
                    return true;
                }
                return false;
            }
            else if (first1 > first2)
            {
                return true;
            }
            else
                return false;


        }
        return true;

    }


    protected void BtnInactive_Click(object sender, EventArgs e)
    {
        sess = (clsSession)Session["UserSession"];
        int TemplateId = 0;
        object objLessId = null;
        int LessonPlanId = 0;
        tdReadMsg.InnerHtml = "";
        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        try
        {
            objData = new clsData();
            string selQuerry = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + TemplateId;
            objLessId = objData.FetchValue(selQuerry);
            if (objLessId != null)
            {
                LessonPlanId = Convert.ToInt32(objLessId);
            }

            if (sess != null)
            {
                try
                {
                    int StatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Inactive' "));
                    //if (objData.IFExists("SELECT DSTempHdrId FROM DSTempHdr WHERE StudentId=" + sess.StudentId + " and StatusId=" + StatusId + " and LessonPlanId=" + LessonPlanId + ""))
                    //{
                    //    objData.Execute("Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE StudentId= " + sess.StudentId + " and StatusId=" + StatusId + " AND LessonPlanId = " + LessonPlanId + "");
                    //}

                    objData.Execute("Update DSTempHdr set [StatusId]=" + StatusId + " WHERE DSTempHdrId= " + TemplateId + "");
                    BtnSubmit.Visible = false;
                    BtnApproval.Visible = false;
                    BtnReject.Visible = false;
                    BtnMaintenance.Visible = false;
                    BtnMakeRegular.Visible = false;
                    BtnCopyTemplate.Visible = false;
                    btnFromReject.Visible = false;
                    btnFromRejectDup.Visible = false;
                    GetStatus(TemplateId);
                    FillData();                                      // Fill the goal type assigned for the student
                    FillApprovedLessonData();
                    FillCompltdLessonPlans();
                    FillRejectedLessons();
                    FillMaintenanceLessonData();
                    FillInactiveLessonData();
                    drpTasklist_SelectedIndexChanged1(sender, e);
                    VisibleApprovalNotes(false);
                    VisibleApprovalNote();
                    ResetIOAStatus(TemplateId);
                    tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Editor Successfully Go To Inactive...");
                    if (TemplateId > 0)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "parent.ChangeAcitveSess(" + sess.StudentId + TemplateId + ");", true);
                    }
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }





    protected void btnDelLp_Click1(object sender, EventArgs e)
    {
        lessonSDate.Text = "";
        lessonEDate.Text = "";        clsData oData = new clsData();
        sess = (clsSession)Session["UserSession"];
        if (sess != null)
        {
            //int stdtLessonplanAprvdId = 0;
            string excutQry = "";
            int headerid = Convert.ToInt32(ViewState["HeaderId"]);
            //string lukupQryApp = "(select LookupId from LookUp where LookupName='Approved' and LookupType='TemplateStatus')";

            int stdtLessonplanid = Convert.ToInt32(oData.FetchValue("select StdtLessonplanId FROM DstempHdr where DSTempHdrId=" + headerid));
            //stdtLessonplanAprvdId = Convert.ToInt32(oData.FetchValue("select DSTempHdrId FROM DstempHdr where StudentId=" + sess.StudentId + " AND DSTempHdrId=" + headerid + " AND StatusId=" + lukupQryApp + " "));
            string lukupQry = "(select LookupId from LookUp where LookupName='Deleted' and LookupType='TemplateStatus')";
            //if (stdtLessonplanAprvdId > 0)
            //{
            //    //excutQry = "Delete from StdtLessonplan WHERE StudentId=" + sess.StudentId + " AND StdtLessonPlanId=" + stdtLessonplanid;
            //    //oData.Execute(excutQry);
            //    excutQry = "Delete from DSTempHdr WHERE StudentId=" + sess.StudentId + " AND DSTempHdrId=" + headerid;
            //    oData.Execute(excutQry);
            //}

            //else
            //{


            excutQry = "UPDATE DSTempHdr SET StatusId=" + lukupQry + "WHERE StudentId=" + sess.StudentId + " AND DSTempHdrId=" + headerid;
            oData.Execute(excutQry);
            // excutQry = "Delete from StdtLessonplan WHERE StudentId=" + sess.StudentId + " AND StdtLessonPlanId=" + stdtLessonplanid;

            //}
            int countLPId = Convert.ToInt32(oData.FetchValue("select count(DSTempHdrId) FROM DstempHdr where StudentId=" + sess.StudentId + " AND StdtLessonPlanId=" + stdtLessonplanid + " and StatusId<>" + lukupQry + " "));
            if (countLPId == 0)
            {
                excutQry = "UPDATE  StdtLessonplan set ActiveInd='D' WHERE StudentId=" + sess.StudentId + " AND StdtLessonPlanId=" + stdtLessonplanid + "";
                oData.Execute(excutQry);
            }
            btndoc.Visible = false;
            btnDelLp.Visible = false;
            btnrejectedNotes.Visible = false;
            btnFromReject.Visible = false;
            btnFromRejectDup.Visible = false;
            BtnSubmit.Visible = false;
            BtnPreview.Visible = false;
            BtnActive.Visible = false;
            BtnMaintenance.Visible = false;
            BtnInactive.Visible = false;
            BtnExportTemplate.Visible = false;
            BtnCopyTemplate.Visible = false;
            BtnApproval.Visible = false;
            BtnReject.Visible = false;
            LoadData();
            LoadTemplateData(0);
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }



    protected void drpTeachingProc_SelectedIndexChanged(object sender, EventArgs e)
    {
		//pramod
		objData = new clsData();
		string teachingProcId = "";
		int headerId = 0;
		if (ViewState["HeaderId"] != null)
		{
			headerId = Convert.ToInt32(ViewState["HeaderId"]);
		}

		string selQuerry = "SELECT TeachingProcId, SkillType,NbrOfTrials,ChainType,MajorSetting,MinorSetting,Baseline,Objective FROm DSTempHdr WHERE DSTempHdrId = " + headerId;
		DataTable dtList = objData.ReturnDataTable(selQuerry, false);
		if (dtList != null)
		{
			if (dtList.Rows.Count > 0)
			{
				teachingProcId = dtList.Rows[0]["TeachingProcId"].ToString();
			}
		}

		string teachingProcId_tochange = drpTeachingProc.SelectedValue.ToString();

		string query = "Select LookupId from LookUp where LookupCode in ('Discrete Trial Training - Tacting/Manding/Intraverbal','Discrete Trial Training - Match To Sample')";
		DataTable dt = new DataTable();
		dt = objData.ReturnDataTable(query, false);

		bool currIsDiscrete = false;
		if (dt.Rows.Count > 0)
		{

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				if (teachingProcId == dt.Rows[i]["LookupId"].ToString())
				{
					currIsDiscrete = true;
				}
			}


		}

		if (currIsDiscrete)
		{
			query = "select LookupId from LookUp where LookupCode ='Match-to-Sample'";
			DataTable dt1 = objData.ReturnDataTable(query, false);

			if (dt != null)
			{
				if (dt.Rows.Count > 0)
				{
					if (teachingProcId_tochange == dt1.Rows[0]["LookupId"].ToString())
					{

						FillTypeOfInstruction(headerId);
						ScriptManager.RegisterClientScriptBlock(UpdatePanel11, UpdatePanel11.GetType(), "", "AlertConvertDisc();", true);
					}

				}
			}
		}

		/// SHOW/HIDE OPTIONS WITH RESPECT TO TEACHING METHOD SELECTED
		/// 
		setOptions(false);


		string drpTProcTxt = drpTeachingProc.SelectedItem.Text.Trim();
		string drpTProcId = drpTeachingProc.SelectedItem.Value;
		string query1 = "";

		query1 = "select LookupName,lookupDesc from LookUp where LookupId='" + drpTProcId + "'";
		DataTable dt2 = objData.ReturnDataTable(query1, false);
		if (dt2.Rows.Count > 0)
		{
			string drpValue1 = dt2.Rows[0]["lookupDesc"].ToString();
			if (drpValue1 == "Match-to-Sample")
			{
				//rdoRandomMoveover.Visible = true;
				rbtnMatchToSampleExpressive.Visible = true;
				if (rbtnMatchToSampleExpressive.SelectedItem.Text.Trim() == "Receptive")
				{
					rdoRandomMoveover.Visible = true;
				}
				else
					rdoRandomMoveover.Visible = false;
			}
			else if (drpValue1 == "Total Task")
			{
				chkTotalRandom.Visible = true;
			}
			else
			{
				//chkTotalRandom.Visible = false;
				rdoRandomMoveover.Visible = false;
			}
		}
		drpTasklist_SelectedIndexChanged1(sender, e);        
    }

    private void setOptions(bool setTeachingForamat)
    {
        objData = new clsData();
        int teachingMethodId = Convert.ToInt32(drpTeachingProc.SelectedItem.Value);
        string query = "select LookupName,LookupDesc,ParentLookupId from LookUp where LookupId='" + teachingMethodId + "'";
        DataTable dt = objData.ReturnDataTable(query, false);

        if (dt.Rows.Count > 0)
        {
            hideAllOptions();
            switch (dt.Rows[0]["LookupDesc"].ToString()) //Match-to-Sample
            {
                case "Discrete":
                    pnl_discrete.Enabled = true;
                    chkDiscrete.Checked = true;
                    chk_stepBystep.Visible = false;
                    chk_stepBysteptooltip.Visible = false;
                    break;
                case "Match-to-Sample":
                    pnl_matchToSample.Enabled = true;
                    pnl_discrete.Enabled = true;
                    chkDiscrete.Checked = true;
                    chk_stepBystep.Visible = false;
                    chk_stepBysteptooltip.Visible = false;
                    break;
                case "Total Task":
                    pnl_taskAnalysis.Enabled = true;
                    chk_stepBystep.Visible = true;
                    drpTasklist.SelectedIndex = 3;
                    chkTotalRandom.Visible = true;
                    break;
                case "Forward Chain":
                    pnl_taskAnalysis.Enabled = true;
                    drpTasklist.SelectedIndex = 1;
                    chk_stepBystep.Visible = false;
                    chk_stepBysteptooltip.Visible = false;
                    break;
                case "Backward Chain":
                    pnl_taskAnalysis.Enabled = true;
                    drpTasklist.SelectedIndex = 2;
                    chk_stepBystep.Visible = false;
                    chk_stepBysteptooltip.Visible = false;
                    break;
            }
        }
        else
        {
            hideAllOptions();
        }



    }

    private void fn_setTeachingForamat(string ParentId)
    {
        if (ParentId != "")
        {
            drp_teachingFormat.SelectedValue = ParentId;
        }
    }
    private void hideAllOptions()
    {
        pnl_discrete.Enabled = false;
        pnl_taskAnalysis.Enabled = false;
        pnl_matchToSample.Enabled = false;
        // pnl_selTeachProc.Visible = true;

        chkDiscrete.Checked = false;
    }
    protected void drpTasklist_SelectedIndexChanged1(object sender, EventArgs e)
    {
        objData = new clsData();
        // rbtnPromptLevel.Visible = false;
        // lblPromptLevel.Visible = false;
        string drpTProcTxt = "";
        string drpTProcId = "";
        try
        {
            drpTProcTxt = drpTeachingProc.SelectedItem.Text;
            drpTProcId = drpTeachingProc.SelectedItem.Value;
        }
        catch { }
        string query1 = "";
        query1 = "select LookupName,LookupDesc from LookUp where LookupId='" + drpTProcId + "'";
        DataTable dt1 = objData.ReturnDataTable(query1, false);
        if (dt1.Rows.Count > 0)
        {
            string drpValue1 = dt1.Rows[0]["LookupDesc"].ToString();
            if ((drpTasklist.SelectedIndex == 3 || drpTasklist.SelectedIndex == 4) && drpValue1 == "Total Task" && chkDiscrete.Checked == false)
            {
                rbtnPromptLevel.Visible = true;
                //lblPromptLevel.Visible = true;
            }
            if (drpValue1 == "Match-to-Sample")
            {
                //rdoRandomMoveover.Visible = true;
                rbtnMatchToSampleExpressive.Visible = true;
                if (rbtnMatchToSampleExpressive.SelectedItem.Text.Trim() == "Receptive")
                {
                    rdoRandomMoveover.Visible = true;
                }
                else
                    rdoRandomMoveover.Visible = false;
            }
            else if (drpValue1 == "Total Task")
            {
                chkTotalRandom.Visible = true;
            }
            else
            {
                //chkTotalRandom.Visible = false;
                rdoRandomMoveover.Visible = false;
            }
        }
    }

    protected void showMatchToSampleDrop()
    {
        objData = new clsData();
        //string drpVal = drpTeachingProc.SelectedItem.Text.Trim();
        string drpTProcTxt = drpTeachingProc.SelectedItem.Text.Trim();
        string drpTProcId = drpTeachingProc.SelectedItem.Value;
        string query1 = "";
        query1 = "select LookupName,LookupDesc from LookUp where LookupId='" + drpTProcId + "'";
        DataTable dt1 = objData.ReturnDataTable(query1, false);
        if (dt1.Rows.Count > 0)
        {
            string drpValue1 = dt1.Rows[0]["LookupDesc"].ToString();
            if (drpValue1 == "Match-to-Sample")
            {
                rdoRandomMoveover.Visible = true;
                if (rdoRandomMoveover.SelectedItem.Text.Trim() == "Randomized")
                {
                    rdoRandomMoveover.SelectedValue = "1";
                }
                else
                {
                    rdoRandomMoveover.SelectedValue = "0";
                }

                rbtnMatchToSampleExpressive.Visible = true;
                if (rbtnMatchToSampleExpressive.SelectedItem.Text.Trim() == "Receptive")
                {
                    rdoRandomMoveover.Visible = true;
                }
                else
                    rdoRandomMoveover.Visible = false;
            }
            else
                rdoRandomMoveover.Visible = false;



        }
    }


    protected void BtnUpdateLessonPlan_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string strQuery = "";
        int headerId = 0;
        int lessonPlanId = 0;
        int newLessonId = 0;
        int newHeaderId = 0;
        object objId;
        string fileName = "";
        string dsessn = "";
        if (cbx_deletesess.Checked)
        {
            dsessn = "true";
        }
        else
        {
            dsessn = "false";
        }
        try
        {
            if (BtnUpdateLessonPlan.Text == "Save")
            {
                if (ValidateLesson() == true)
                {

                    SqlTransaction Transs = null;
                    SqlConnection con = objData.Open();
                    clsData.blnTrans = true;
                    Transs = con.BeginTransaction();

                    try
                    {
                        /// check the Lesson Plan name is already exists
                        /// 
                        string GoalApproved = "";
                        foreach (ListItem li in chkgoal.Items)
                        {
                            if (li.Selected == true)
                            {
                                GoalApproved = li.Value + "," + GoalApproved;
                            }
                        }
                        GoalApproved = GoalApproved.Remove(GoalApproved.Length - 1, 1);
                        string lessonName = clsGeneral.convertQuotes(txtLessonName.Text.Trim());
                        //string strCheckLPName = "SELECT DSTemplateName FROM DSTempHdr WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName.Trim() + "'))) and StatusId<>(select LookupId from LookUp where LookupType='TemplateStatus' and LookupName='Deleted') ";
                        string strCheckLPName = "SELECT DSTemplateName FROM DSTempHdr HD inner join GoalLPRel GR on HD.lessonplanid=GR.lessonplanid WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName + "'))) " +
                            "and StatusId NOT IN (select LookupId from LookUp where LookupType='TemplateStatus' and LookupName IN ('Deleted', 'SoftDelete')) AND StudentId IS NULL AND isDynamic=0 AND GR.GOALID IN (" + GoalApproved + ") AND GR.ACTIVEIND='A'";

                        DataTable dt = objData.ReturnDataTable(strCheckLPName, false);
                        if (dt.Rows.Count > 0)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "MsgLPNameAlreadyExists();", true);
                        }
                        else
                        {
                            // UploadLessonPlan();               // lessonplan file upload name

                            strQuery = "INSERT INTO  LessonPlan(SchoolId,ActiveInd,LessonPlanName,FrameandStrand,SpecStandard,SpecEntryPoint,PreReq,Materials,ImageURL,CreatedBy,ModifiedBy,CreatedOn,LessonSDate,LessonEDate) values(" + sess.SchoolId + ",'A','" + clsGeneral.convertQuotes(txtLessonName.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "','" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "','" + fileName + "'," + sess.LoginId + "," + sess.LoginId + " ,(SELECT Convert(Varchar,getdate(),100)),'" + lessonSDate.Text +  "','" + lessonEDate.Text + "') ";
                            newLessonId = objData.ExecuteWithScopeandConnection(strQuery, con, Transs);
                            strQuery = "INSERT INTO DSTempHdr(DSTemplateName,SchoolId,LessonPlanId,NoofTimesTried,NoofTimesTriedPer,isDynamic,CreatedBy,CreatedOn,StatusId,LessonPlanGoal,[FrameandStrand],[SpecStandard],[SpecEntryPoint],[PreReq],[Materials],deletessn,[LessonSDate],[LessonEDate]) " +
                                        "VALUES ('" + lessonName + "'," + sess.SchoolId + "," + newLessonId + ",'" + txtNoofTimesTried.Text + "','" + noofTimesTriedPer.SelectedValue + "',0," + sess.LoginId + ",(SELECT Convert(Varchar,getdate(),100))," +
                                                       "(select LookupId from LookUp where LookupName = 'In Progress' and LookupType = 'TemplateStatus'),'" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',(SELECT [FrameandStrand] FROM LessonPlan WHERE LessonPlanId=" + newLessonId + "),(SELECT [SpecStandard] FROM LessonPlan WHERE LessonPlanId=" + newLessonId + ") ,(SELECT [SpecEntryPoint]  FROM LessonPlan WHERE LessonPlanId=" + newLessonId + ") ,(SELECT [PreReq] FROM LessonPlan WHERE LessonPlanId=" + newLessonId + ") ,(SELECT [Materials] FROM LessonPlan WHERE LessonPlanId=" + newLessonId + "),'" + dsessn + "',(SELECT LessonSDate FROM LessonPlan WHERE LessonPlanId=" + newLessonId + "),(SELECT LessonEDate FROM LessonPlan WHERE LessonPlanId=" + newLessonId + "))";
                            newHeaderId = objData.ExecuteWithScopeandConnection(strQuery, con, Transs);


                            //UploadLessonPlan(newHeaderId, Transs, con);        // inserting the docc path to the database

                            foreach (ListItem li in chkgoal.Items)
                            {

                                if (li.Selected == true)
                                {
                                    int goalId = Convert.ToInt32(li.Value);
                                    strQuery = "INSERT into GoalLPRel(GoalId,LessonPlanId,ActiveInd,CreatedBy,CreatedOn) Values(" + goalId + "," + newLessonId + ",'A'," + sess.LoginId + ",(SELECT Convert(Varchar,getdate(),100)))";
                                    objData.ExecuteWithScopeandConnection(strQuery, con, Transs);
                                }
                            }

                            ViewState["HeaderId"] = newHeaderId;
                            objData.CommitTransation(Transs, con);
                            FillApprovedLessonData();
                            lblDataFill(newHeaderId);
                            BtnUpdateLessonPlan.Text = "Update";
                            btnNew.Style.Add("display", "block");
                            if (DeletePermission())
                            {
                                btnDeleteLPAdmin.Visible = true;
                            }
                            //ClientScript.RegisterStartupScript(this.GetType(), "", "LoadAdminTemplateView(1);", true);
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "LoadAdminTemplateView(1);", true);
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
                        }
                        ///end
                        ///
                    }
                    catch (Exception Ex)
                    {

                    }
                }
            }
            else
            {
               
                    if (ViewState["HeaderId"] != null)
                    {
                        if (Request.QueryString["admin"] != null && Request.QueryString["admin"] == "true")// Manage Lesson plan
                        {
                            string HeaderId = ViewState["HeaderId"].ToString();
                            strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + HeaderId;
                            objId = objData.FetchValue(strQuery);
                            if (objId != null)
                            {
                                lessonPlanId = Convert.ToInt32(objId);
                            }
                            /// check the Lesson Plan name is already exists
                            /// 
                            string GoalApproved = "";
                            foreach (ListItem li in chkgoal.Items)
                            {
                                if (li.Selected == true)
                                {
                                    GoalApproved = li.Value + "," + GoalApproved;
                                }
                            }
                            GoalApproved = GoalApproved.Remove(GoalApproved.Length - 1, 1);

                            string txtCommentLessonInfoP = txtCommentLessonInfo.Text.Trim().Replace("'", "''");
                            string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteLessonInfo='" + txtCommentLessonInfoP + "' WHERE DSTempHdrId='" + HeaderId + "'";
                            objData.Execute(UpdateAppr);

                            string lessonName = clsGeneral.convertQuotes(txtLessonName.Text.Trim());
                            //string strCheckLPName = "SELECT DSTemplateName FROM DSTempHdr WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName.Trim() + "'))) " +
                            //    "and StatusId<>(select LookupId from LookUp where LookupType='TemplateStatus' and LookupName='Deleted') and LessonPlanId <> " + lessonPlanId ;
                            string strCheckLPName = "SELECT DSTemplateName FROM DSTempHdr HD inner join GoalLPRel GR on HD.lessonplanid=GR.lessonplanid WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName + "'))) " +
                               "and StatusId NOT IN (select LookupId from LookUp where LookupType='TemplateStatus' and LookupName IN ('Deleted','SoftDelete')) AND StudentId IS NULL AND isDynamic=0 AND GR.GOALID IN (" + GoalApproved + ") AND GR.ACTIVEIND='A' AND HD.LessonPlanId <> " + lessonPlanId;
                            DataTable dt = objData.ReturnDataTable(strCheckLPName, false);                           


                            if (dt.Rows.Count > 0)
                            {
                                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "MsgLPNameAlreadyExists();", true);
                            }
                           
                            else
                            {
                                strQuery = "UPDATE LessonPlan SET LessonPlanName = '" + lessonName + "', FrameandStrand = '" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "', SpecStandard = '" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint = '" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "'," +
                                          "  PreReq = '" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "', Materials = '" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "',ImageURL = '" + fileName + "',LessonSDate= '" + lessonSDate.Text + "',LessonEDate= '" + lessonEDate.Text + "' WHERE LessonPlanId = " + lessonPlanId;

                                objData.Execute(strQuery);
                                //strQuery = "Update DSTempHdr SET LessonPlanGoal='" + txtLessonPlanGoal.Text.Trim() + "' WHERE DSTempHdrId='" + HeaderId + "' ";
                                strQuery = "UPDATE DSTempHdr SET DSTemplateName='" + lessonName + "',NoofTimesTried='" + txtNoofTimesTried.Text + "',NoofTimesTriedPer='" + noofTimesTriedPer.SelectedValue + "',LessonPlanGoal='" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',FrameandStrand='" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "',SpecStandard='" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint='" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "',PreReq='" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "',Materials='" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "', deletessn = '" + dsessn + "' ,LessonSDate= '" + lessonSDate.Text + "', LessonEDate= '" + lessonEDate.Text + "' WHERE DSTempHdrId='" + HeaderId + "'";
                                objData.Execute(strQuery);
                                strQuery = "delete from GoalLPRel where LessonPlanId = " + lessonPlanId + "";
                                objData.Execute(strQuery);
                                foreach (ListItem li in chkgoal.Items)
                                {
                                    if (li.Selected == true)
                                    {
                                        int goalId = Convert.ToInt32(li.Value);
                                        strQuery = "INSERT into GoalLPRel(GoalId,LessonPlanId,ActiveInd,CreatedBy,CreatedOn) Values(" + goalId + "," + lessonPlanId + ",'A'," + sess.LoginId + ",(SELECT Convert(Varchar,getdate(),100)))";
                                        objData.Execute(strQuery);
                                    }
                                }
                                lblDataFill(Convert.ToInt32(HeaderId));
                                FillData();
                                if(lessonSDate.Text==""  || lessonEDate.Text=="")
                                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "NoDate();", true);
                                else
                                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
                                
                            }
                            ///end
                            ///
                        }
                        else
                        {

                            string HeaderId = ViewState["HeaderId"].ToString();
                            /// check the Lesson Plan name is already exists
                            /// 
                            string txtCommentLessonInfoP = txtCommentLessonInfo.Text.Trim().Replace("'", "''");                            
                            string UpdateAppr = "UPDATE DSTempHdr SET ApprNoteLessonInfo='" + txtCommentLessonInfoP + "' WHERE DSTempHdrId='" + HeaderId + "'";
                            objData.Execute(UpdateAppr);

                            strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + HeaderId;
                            lessonPlanId = Convert.ToInt32(objData.FetchValue(strQuery));
                            string lessonName = clsGeneral.convertQuotes(txtLessonName.Text.Trim());
                            int GoalApproved = Convert.ToInt32(objData.FetchValue("SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId=" + HeaderId + ")"));

                            //string strCheckLPName = "select DSTemplateName from DSTempHdr where (StudentId=" + sess.StudentId + " and LessonPlanId <> " + lessonPlanId + " and (RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName.Trim() + "'))) " +
                            //    "and StatusId<>(select LookupId from LookUp where LookupType='TemplateStatus' and LookupName='Deleted'))) or (RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName.Trim() + "'))) AND StudentId IS NULL AND isDynamic=0 and LessonPlanId <> " + lessonPlanId + ")";
                            string strCheckLPName = "SELECT DSTemplateName FROM DSTempHdr inner join LookUp lu on lu.LookupId=DSTempHdr.StatusId WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + lessonName + "'))) " +
                                "and lu.LookupName <> 'Deleted' AND StudentId=" + sess.StudentId + " AND (SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=DSTempHdr.StdtLessonplanId)=" + GoalApproved + " and LessonPlanId <> " + lessonPlanId;
                            DataTable dt = objData.ReturnDataTable(strCheckLPName, false);
                            if (dt.Rows.Count > 0)
                            {
                                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "MsgLPNameAlreadyExists();", true);
                            }
                            else
                            {
                                string UpdateDSTemphdr = "";
                                if (lessonSDate.Text == "" && lessonEDate.Text=="")
                                {
                                    UpdateDSTemphdr = "UPDATE DSTempHdr SET DSTemplateName='" + lessonName + "',NoofTimesTried='" + txtNoofTimesTried.Text + "',NoofTimesTriedPer='" + noofTimesTriedPer.SelectedValue + "',LessonPlanGoal='" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',FrameandStrand='" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "',SpecStandard='" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint='" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "',PreReq='" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "',Materials='" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "',deletessn = '" + dsessn + "' ,LessonSDate= NULL,LessonEDate= NULL WHERE DSTempHdrId='" + HeaderId + "'";
                                }
                                else if(lessonSDate.Text == "" && lessonEDate.Text!="")
                                    UpdateDSTemphdr = "UPDATE DSTempHdr SET DSTemplateName='" + lessonName + "',NoofTimesTried='" + txtNoofTimesTried.Text + "',NoofTimesTriedPer='" + noofTimesTriedPer.SelectedValue + "',LessonPlanGoal='" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',FrameandStrand='" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "',SpecStandard='" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint='" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "',PreReq='" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "',Materials='" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "',deletessn = '" + dsessn + "' ,LessonSDate= NULL,LessonEDate=  '" + lessonEDate.Text + "' WHERE DSTempHdrId='" + HeaderId + "'";
                                else if (lessonEDate.Text == "" && lessonSDate.Text != "")
                                    UpdateDSTemphdr = "UPDATE DSTempHdr SET DSTemplateName='" + lessonName + "',NoofTimesTried='" + txtNoofTimesTried.Text + "',NoofTimesTriedPer='" + noofTimesTriedPer.SelectedValue + "',LessonPlanGoal='" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',FrameandStrand='" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "',SpecStandard='" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint='" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "',PreReq='" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "',Materials='" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "',deletessn = '" + dsessn + "' ,LessonSDate= '" + lessonSDate.Text + "',LessonEDate= NULL WHERE DSTempHdrId='" + HeaderId + "'";
                                else
                                    UpdateDSTemphdr = "UPDATE DSTempHdr SET DSTemplateName='" + lessonName + "',NoofTimesTried='" + txtNoofTimesTried.Text + "',NoofTimesTriedPer='" + noofTimesTriedPer.SelectedValue + "',LessonPlanGoal='" + clsGeneral.convertQuotes(txtLessonPlanGoal.Text.Trim()) + "',FrameandStrand='" + clsGeneral.convertQuotes(txtFramework.Text.Trim()) + "',SpecStandard='" + clsGeneral.convertQuotes(txtSpecStandrd.Text.Trim()) + "',SpecEntryPoint='" + clsGeneral.convertQuotes(txtSpecEntrypoint.Text.Trim()) + "',PreReq='" + clsGeneral.convertQuotes(txtPreSkills.Text.Trim()) + "',Materials='" + clsGeneral.convertQuotes(txtMaterials.Text.Trim()) + "',deletessn = '" + dsessn + "' ,LessonSDate= '" + lessonSDate.Text + "' ,LessonEDate= '" + lessonEDate.Text + "' WHERE DSTempHdrId='" + HeaderId + "'";
                                objData.Execute(UpdateDSTemphdr);
                                lblDataFill(Convert.ToInt32(HeaderId));
                                FillData();
                                if(lessonSDate.Text==""||lessonEDate.Text=="")
                                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "NoDate();", true);
                                else
                                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
                                
                            }
                            ///end
                            ///

                        }
                    }
                
              

            }



        }
        catch (Exception Ex)
        {

        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }

    private void fn_setChkTotalTask(int HeaderId)
    {
        objData = new clsData();
        string SelectDSTemphdr = "SELECT TotalTaskType FROM DSTempHdr WHERE DSTempHdrId='" + HeaderId + "'";
        object obj_result = objData.FetchValue(SelectDSTemphdr);
        if (obj_result != null && obj_result != System.DBNull.Value)
        {
            if (Convert.ToInt32(obj_result) == 1)
            {
                chk_stepBystep.Checked = true;
            }
            else
            {
                chk_stepBystep.Checked = false;
            }
        }
        else
        {
            chk_stepBystep.Checked = false;
        }

    }

    protected void rbtnCalcuType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rbtnCalcuType.SelectedIndex == 0)
        {
            imgCreateEqutn_Click(sender, e);
        }
        else
        {
            imageCollapseDiv_Click(sender, e);
        }
    }

    protected void btnCommentLessonInfo_Click(object sender, EventArgs e)
    {
        try
        {
            objData = new clsData();
            string HeaderId = ViewState["HeaderId"].ToString();
            string txtCommentLessonInfoP        = txtCommentLessonInfo.Text.Trim().Replace("'", "''");
            string txtCommentTypeofInstrP       = txtCommentTypeofInstr.Text.Trim().Replace("'", "''");
            string txtMeasurementSystemsP       = txtMeasurementSystems.Text.Trim().Replace("'", "''");
            string txtcommentsetP               = txtcommentset.Text.Trim().Replace("'", "''");
            string txtcommentStepP              = txtcommentStep.Text.Trim().Replace("'", "''");
            string txtcommentPromptP            = txtcommentPrompt.Text.Trim().Replace("'", "''");
            string txtcommentLessonProcedureP   = txtcommentLessonProcedure.Text.Trim().Replace("'", "''");

            string UpdateDSTemphdr = "UPDATE DSTempHdr SET ApprNoteLessonInfo='" + txtCommentLessonInfoP + "'," +
                                                      "ApprNoteTypeInstruction='" + txtCommentTypeofInstrP + "'," +
                                                      "ApprNoteMeasurement='" + txtMeasurementSystemsP + "'," +
                                                      "ApprNoteSet='" + txtcommentsetP + "'," +
                                                      "ApprNoteStep='" + txtcommentStepP + "'," +
                                                      "ApprNotePrompt='" + txtcommentPromptP + "'," +
                                                      "ApprNoteLessonProc='" + txtcommentLessonProcedureP + "' " +
                                                      "WHERE DSTempHdrId='" + HeaderId + "'";
            int Result = objData.Execute(UpdateDSTemphdr);

            if (Result == 1)
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "AlertSuccessMsg();", true);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }
    protected void ddchkCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblSelParentSets.Text = "";
        int flag = 0;

        foreach (System.Web.UI.WebControls.ListItem item in ddchkCountry.Items)
        {

            if (item.Selected == true)
            {
                string text = "";
                if (item.Text != "")
                {
                    if (item.Text.ToString().Length > 80)
                    {
                        text = item.Text.ToString().Substring(0, 80) + "........";
                    }
                    else
                    {
                        text = item.Text;
                    }
                }


                lblSelParentSets.Text += "- " + text + "</br>";
                flag++;
            }
        }
        if (flag == ddchkCountry.Items.Count)
        {
            lblSelParentSets.Text = "All Sets";
        }
        else if (flag == 0)
        {
            lblSelParentSets.Text = "Not Assigned to any Sets";
        }
        flag = 0;
        drpTasklist_SelectedIndexChanged1(sender, e);
    }
    protected void rbtnMatchToSampleExpressive_SelectedIndexChanged(object sender, EventArgs e)
    {
        showMatchToSampleDrop();
    }
    //----------Copy Template to Student and as template--------<<

    //protected void chkCpyStdtTemplate_CheckedChanged(object sender, EventArgs e)
    //{
    //    if (chkCpyStdtTemplate.Checked)
    //    {
    //        txtSname.Enabled = false;
    //        imgsearch.Enabled = false;            
    //        ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "studclsSearch();", true);
    //    }
    //    else
    //    {
    //        txtSname.Enabled = true;
    //        imgsearch.Enabled = true;
    //        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#tdMsgExprt').empty();", true);
    //    }
    //    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#tdMsgExprt').empty();", true);
    //}
    protected void imgsearch_Click(object sender, ImageClickEventArgs e)
    {
        objData = new clsData();
        int headerId = Convert.ToInt32(ViewState["HeaderId"]);
        hdDefaultName.Value = Convert.ToString(objData.FetchValue("SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId='" + headerId + "'"));
        LBLClassnotfound.Text = "";
        sess = (clsSession)Session["UserSession"];
        string DlClass = "";
        if (txtSname.Text.Trim() != "Student Name")
        {
            int length = txtSname.Text.Trim().Length;
            if (length >= 3)
            {

                if (objData.IFExists("SELECT  StudentPersonalId,StudentFname FROM Student ST where ST.StudentFname='" + txtSname.Text.Trim() + "' OR ST.StudentLname='" + txtSname.Text.Trim() + "'  OR ST.StudentFname+' '+ST.StudentLname='" + txtSname.Text.Trim() + "' OR ST.StudentLname+' '+ST.StudentFname='" + txtSname.Text.Trim() + "' OR ST.StudentLname LIKE +'%'+'" + txtSname.Text.Trim() + "'+'%'  OR ST.StudentFname LIKE +'%'+'" + txtSname.Text.Trim() + "'+'%'") == true)
                {
                    string studentdetail = "SELECT  top(50) StudentPersonalId,StudentFname,StudentLname FROM Student ST where ST.StudentFname='" + txtSname.Text.Trim() + "' OR ST.StudentLname='" + txtSname.Text.Trim() + "' " +
                                            "OR ST.StudentFname+' '+ST.StudentLname='" + txtSname.Text.Trim() + "' OR ST.StudentLname+' '+ST.StudentFname='" + txtSname.Text.Trim() + "' OR ST.StudentLname LIKE +'%'+'" + txtSname.Text.Trim() + "'+'%'  OR ST.StudentFname" +
                                            " LIKE +'%'+'" + txtSname.Text.Trim() + "'+'%'";

                    DataTable dt = objData.ReturnDataTable(studentdetail, false);
                    if (dt == null) return;
                    if (dt.Rows.Count > 0)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#DlStudent').empty();", true);
                        foreach (DataRow row in dt.Rows)
                        {
                            string functn = "selectStudentId(" + row["StudentPersonalId"] + ");";
                            DlClass += "<div class='cpyTemp' id='" + row["StudentPersonalId"] + "' onclick='" + functn + "'>" + row["StudentFname"] + " " + row["StudentLname"] + "</div>";
                            //hfSelectedStudent.Value = row["StudentPersonalId"].ToString();
                        }

                        string x = "$('#DlStudent').append('" + DlClass + "');";
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#DlStudent').append(\"" + DlClass + "\");", true);

                    }
                    else if (dt.Rows.Count == 0)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#DlStudent').empty();", true);
                        LBLClassnotfound.ForeColor = System.Drawing.Color.Red;
                        LBLClassnotfound.Text = "You are not autherized to Access this Student";
                    }
                    else
                    {
                        //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#DlStudent').empty();", true);

                    }

                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "$('#DlStudent').empty();", true);
                    LBLClassnotfound.Text = "Individual not found";
                }
            }
            else
            {
                LBLClassnotfound.Text = "Please Enter Minimum 3 Letters";
            }
            tdMsgExprt.InnerHtml = "";
            tdReadMsg.InnerHtml = "";
        }
        else
        {
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), Guid.NewGuid().ToString(), "classBind();", true);
            LBLClassnotfound.Text = "Please enter search Value";
        }

    }
    protected int AddLessonPlan(string LpName, DataTable GoalId, string Type, int oldLp, string TypeofLP, int DSTempId, out object objYearId)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        objYearId = objData.FetchValue("SELECT AsmntYearId FROM AsmntYear WHERE CurrentInd='A'");
        object objStat = objData.FetchValue("SELECT LookupId FROM LookUp WHERE LookupType='LP Status' AND LookupName='In Progress'");

        //if (oData.IFExists("select LessonPlanId from LessonPlan where LessonPlanName='" + txtLPname.Text + "'") == false)
        //{
        SqlConnection con = new SqlConnection();
        con = objData.Open();

        SqlTransaction trans = con.BeginTransaction();
        int LPid = 0;
        try
        {
            if (sess != null)
            {
                string insLP = "";
                if (TypeofLP == "Temp")
                {
                    insLP = "insert into LessonPlan(SchoolId,[PreReq],[BaselineProc],[Materials],[FrameandStrand],[SpecStandard],[SpecEntryPoint],ActiveInd,LessonPlanName,CreatedBy,CreatedOn,[Baseline],[Objective],LessonSDate,LessonEDate) " +
                    "SELECT " + sess.SchoolId + ",[PreReq],[BaselineProc],[Materials],[FrameandStrand],[SpecStandard],[SpecEntryPoint],'A','" + LpName + "'," + sess.LoginId + ",GETDATE(),[Baseline],[Objective],LessonSDate,LessonEDate FROM DSTempHdr WHERE DSTempHdrId=" + DSTempId;
                }
                else
                {
                    insLP = "insert into LessonPlan(SchoolId,[PreReq],[TeacherSD],[TeacherInst],[Consequence],[BaselineProc],[PostCheckProc],[ImageURL],[Materials],[FrameandStrand],[SpecStandard],[SpecEntryPoint],ActiveInd,LessonPlanName,CreatedBy,CreatedOn,[Baseline],[Objective],LessonSDate,LessonEDate) " +
                    "SELECT " + sess.SchoolId + ",[PreReq],[TeacherSD],[TeacherInst],[Consequence],[BaselineProc],[PostCheckProc],[ImageURL],[Materials],[FrameandStrand],[SpecStandard],[SpecEntryPoint],'A','" + LpName + "'," + sess.LoginId + ",GETDATE(),[Baseline],[Objective],LessonSDate,LessonEDate FROM [dbo].[LessonPlan] WHERE LessonPlanId=" + oldLp;
                }

                LPid = objData.ExecuteWithScopeandConnection(insLP, con, trans);
                if (LPid > 0)
                {
                    foreach (DataRow row in GoalId.Rows)
                    {
                        string strQuery = "INSERT into GoalLPRel(GoalId,LessonPlanId,ActiveInd,CreatedBy,CreatedOn) " +
                        "Values('" + row["GoalId"].ToString() + "'," + LPid.ToString() + ",'A'," + sess.LoginId + ",(SELECT Convert(Varchar,getdate(),100)))";
                        objData.ExecuteWithScopeandConnection(strQuery, con, trans);

                    }
                    if (Type == "1")
                    {
                        string insHDR = "insert into DSTempHdr(SchoolId,LessonPlanId,DSTemplateName,isDynamic,CreatedBy,CreatedOn) " +
                       "values(" + sess.SchoolId + "," + LPid + ",'" + LpName + "'," + Type + "," + sess.LoginId + ",(SELECT Convert(Varchar,getdate(),100))) ";
                        objData.ExecuteWithScopeandConnection(insHDR, con, trans);
                    }

                    objData.CommitTransation(trans, con);

                }

            }
            return LPid;
        }
        catch (Exception ex)
        {
            objData.RollBackTransation(trans, con);
            con.Close();
            return 0;
            throw ex;
        }

    }


    protected void btnCopyTempAdmin_click(object sender, EventArgs e)
    {
        objData = new clsData();
        int apprvdLessonId = 0;
        int visualLessonId = 0;
        DataTable GoalId = new DataTable();
        int NewLpid = 0;
        object AsmntYr;
        if (ViewState["HeaderId"] != null)
        {
            apprvdLessonId = Convert.ToInt32(ViewState["HeaderId"]);
        }
        tdReadMsg.InnerHtml = "";
        sess = (clsSession)Session["UserSession"];
        visualLessonId = ReturnNewVLessonId(apprvdLessonId);
        GoalId = objData.ReturnDataTable("SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonplanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')", false);
        int OldLpId = Convert.ToInt32(objData.FetchValue("SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId));

        try
        {
            if (sess != null)
            {
                if (chkCpyStdtTemplate.Checked)
                {
                    NewLpid = AddLessonPlan(clsGeneral.convertQuotes(hdLessonName.Value), GoalId, "0", OldLpId, "Temp", apprvdLessonId, out AsmntYr);
                    if (NewLpid > 0)
                    {
                        int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId);
                        if (tempid > 0)
                        {
                            CreateDocument(apprvdLessonId, tempid);

                            string UpdateLessonName = "UPDATE DSTempHdr SET DSTemplateName='" + clsGeneral.convertQuotes(hdLessonName.Value) + "',LessonPlanId='" + NewLpid + "',isDynamic=0 WHERE DSTempHdrId=" + tempid;
                            objData.Execute(UpdateLessonName);

                            //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);
                            string NewName = "";
                            if (hdLessonName.Value != "")
                            {
                                NewName = "to <h3>" + hdLessonName.Value + "</h3>";
                            }
                            tdMsgExprt.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
                            tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
                            txtSname.Text = "";
                            chkCpyStdtTemplate.Checked = false;
                            txtSname.Enabled = true;
                            imgsearch.Enabled = true;
                            //chkCpyStdtTemplate_CheckedChanged(sender, e);
                        }
                    }
                }
                else
                {

                    if (hfSelectedStudent.Value != "")
                    {
                        string strQuery = "SELECT LessonPlanId,StdtLessonPlanId from DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId;
                        DataTable dt = new DataTable();
                        dt = objData.ReturnDataTable(strQuery, false);
                        if (dt != null)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                string strQryGoal = "SELECT Goalid from StdtLessonPlan WHERE StdtLessonPlanId=" + Convert.ToInt32(dt.Rows[0]["StdtLessonPlanId"]);
                                string goalid = objData.FetchValue(strQryGoal).ToString();
                                string strStdtGoal = "select count(*) from stdtgoal where studentid=" + Convert.ToInt32(hfSelectedStudent.Value) + " and GoalId=" + goalid + "";
                                int stdtGoal = Convert.ToInt32(objData.FetchValue(strStdtGoal));
                                if (stdtGoal == 0)
                                {
                                    object objYearId = objData.FetchValue("SELECT AsmntYearId FROM AsmntYear WHERE CurrentInd='A'");
                                    object objStat = objData.FetchValue("SELECT LookupId FROM LookUp WHERE LookupType='Goal Status' AND LookupName='In Progress'");
                                    string nsGoal = "INSERT INTO StdtGoal(SchoolId,StudentId,GoalId,AsmntYearId,IncludeIEP,StatusId,ActiveInd,IEPGoalNo," +
                                            "CreatedBy,CreatedOn) VALUES(" + sess.SchoolId + "," + Convert.ToInt32(hfSelectedStudent.Value) + "," + goalid + "," + objYearId.ToString() + ",0," +
                                            "" + objStat.ToString() + ",'A',(SELECT ISNULL(MAX(IEPGoalNo),0)+1 FROM StdtGoal WHERE StudentId=" + Convert.ToInt32(hfSelectedStudent.Value) + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A')," + sess.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";
                                    objData.Execute(nsGoal);
                                }
                                //string rtnStdt = SaveLessons(dt.Rows[0]["LessonPlanId"].ToString(), goalid, hfSelectedStudent.Value);

                                //if (rtnStdt != null)
                                //{
                                //if (rtnStdt == "exists")
                                //{
                                //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);


                                //tdMsgExprt.InnerHtml = clsGeneral.failedMsg("Lesson Plan already exists for the student ");
                                //txtSname.Text = "";
                                //return;

                                NewLpid = AddLessonPlan(clsGeneral.convertQuotes(hdLessonName.Value), GoalId, "1", OldLpId, "LP", apprvdLessonId, out AsmntYr);
                                if (NewLpid > 0)
                                {
                                    int StdtLpid = objData.ExecuteWithScope("INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,IncludeIEP,ActiveInd,StatusId,LessonPlanTypeDay,LessonPlanTypeResi,CreatedBy,CreatedOn,isDynamic) " +
                                                   " VALUES('" + sess.SchoolId + "','" + Convert.ToInt32(hfSelectedStudent.Value) + "'," + NewLpid + ",'" + GoalId.Rows[0]["GoalId"].ToString() + "','" + AsmntYr + "','false','A',(SELECT LookupId FROM LookUp WHERE LookupType='LP Status' AND LookupName='In Progress'),(SELECT LessonPlanTypeDay FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),(SELECT LessonPlanTypeResi FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),'" + sess.LoginId + "',GETDATE(),1)");
                                    //Convert.ToInt32(objData.FetchValue("SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "'"));
                                    int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId, Convert.ToInt32(hfSelectedStudent.Value), Convert.ToInt32(StdtLpid));
                                    if (tempid > 0)
                                    {
                                        CreateDocument(apprvdLessonId, tempid);
                                    }
                                    if (tempid > 0)
                                    {
                                        string UpdateLessonName = "UPDATE DSTempHdr SET DSTemplateName='" + clsGeneral.convertQuotes(hdLessonName.Value) + "',LessonPlanId='" + NewLpid + "',isDynamic=1 WHERE DSTempHdrId=" + tempid;
                                        objData.Execute(UpdateLessonName);
                                    }
                                    FillData();
                                }
                                //}
                                //else
                                //{
                                //    NewLpid = AddLessonPlan(hdLessonName.Value, GoalId, out AsmntYr);
                                //    if (NewLpid > 0)
                                //    {
                                //        int StdtLpid = objData.ExecuteWithScope("INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,IncludeIEP,ActiveInd,StatusId,LessonPlanTypeDay,LessonPlanTypeResi,CreatedBy,CreatedOn) " +
                                //                       " VALUES('" + sess.SchoolId + "','" + Convert.ToInt32(hfSelectedStudent.Value) + "'," + NewLpid + ",'" + GoalId + "','" + AsmntYr + "','false','A',(SELECT LookupId FROM LookUp WHERE LookupType='LP Status' AND LookupName='In Progress'),(SELECT LessonPlanTypeDay FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),(SELECT LessonPlanTypeResi FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),'" + sess.LoginId + "',GETDATE())");
                                //        Convert.ToInt32(objData.FetchValue("SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "'"));
                                //        int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId, Convert.ToInt32(hfSelectedStudent.Value), Convert.ToInt32(StdtLpid));
                                //        if (tempid > 0)
                                //        {
                                //            CreateDocument(apprvdLessonId, tempid);
                                //        }
                                //    }
                                //}
                                //}

                            }
                        }
                        txtSname.Text = "";
                        string NewName = "";
                        if (hdLessonName.Value != "")
                        {
                            NewName = "to <h3>" + hdLessonName.Value + "</h3>";
                        }
                        tdMsgExprt.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
                        tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
                        // ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);
                    }
                    else
                    {
                        txtSname.Text = "";
                        tdMsgExprt.InnerHtml = clsGeneral.warningMsg("Please select a student......");
                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please select a student......");
                        //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "AlertCopySelectMsg();", true);
                    }
                    hfSelectedStudent.Value = "";

                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

        //objData = new clsData();
        //int apprvdLessonId = 0;
        //int visualLessonId = 0;
        //string NewLessonName = "";
        //int NewLpid = 0;
        //string GoalId = "";
        //object AsmntYr;
        //if (ViewState["HeaderId"] != null)
        //{
        //    apprvdLessonId = Convert.ToInt32(ViewState["HeaderId"]);
        //}
        //tdReadMsg.InnerHtml = "";
        //sess = (clsSession)Session["UserSession"];
        //visualLessonId = ReturnNewVLessonId(apprvdLessonId);
        //try
        //{
        //    if (sess != null)
        //    {
        //        if (chkCpyStdtTemplate.Checked)
        //        {
        //string strQuery = "SELECT LessonPlanId from DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId;
        //string lpId = objData.FetchValue(strQuery).ToString();
        //if (lpId != null && lpId != "")
        //{
        //    string strQryHdr = "SELECT DSTempHdrId from DSTempHdr WHERE LessonPlanId=" + Convert.ToInt32(lpId) + " and studentId is NULL";
        //    string hdrId = objData.FetchValue(strQryHdr).ToString();
        //if (hdrId != null && hdrId != "")
        //{
        //    string deleteTemp = "DELETE from DSTempHdr where LessonPlanId=" + Convert.ToInt32(lpId) + " and studentId is NULL";
        //    int index = objData.Execute(deleteTemp);
        //}
        //}

        //GetGoalAndLessonName(apprvdLessonId, "0", out NewLessonName, out NewLpid, out GoalId, out AsmntYr);
        //    if (NewLpid > 0)
        //    {
        //        int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId);
        //        if (tempid > 0)
        //        {
        //            CreateDocument(apprvdLessonId, tempid);

        //            string UpdateLessonName = "UPDATE DSTempHdr SET DSTemplateName='" + NewLessonName + "',LessonPlanId='" + NewLpid + "' WHERE DSTempHdrId=" + tempid;
        //            objData.Execute(UpdateLessonName);

        //            //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);
        //            string NewName = "";
        //            if (NewLessonName != "")
        //            {
        //                NewName = "to <h3>" + NewLessonName + "</h3>";
        //            }
        //            tdMsgExprt.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
        //            tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
        //            txtSname.Text = "";
        //            chkCpyStdtTemplate.Checked = false;
        //            txtSname.Enabled = true;
        //            imgsearch.Enabled = true;
        //            //chkCpyStdtTemplate_CheckedChanged(sender, e);
        //        }
        //    }
        //}
        //else
        //{
        //    try
        //    {
        //        if (hfSelectedStudent.Value != "")
        //        {
        //            string strQuery = "SELECT LessonPlanId,StdtLessonPlanId from DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId;
        //            DataTable dt = new DataTable();
        //            dt = objData.ReturnDataTable(strQuery, false);
        //            if (dt != null)
        //            {
        //                if (dt.Rows.Count > 0)
        //                {
        //                    string strQryGoal = "SELECT Goalid from StdtLessonPlan WHERE StdtLessonPlanId=" + Convert.ToInt32(dt.Rows[0]["StdtLessonPlanId"]);
        //                    string goalid = objData.FetchValue(strQryGoal).ToString();
        //                    string strStdtGoal = "select count(*) from stdtgoal where studentid=" + Convert.ToInt32(hfSelectedStudent.Value) + " and GoalId=" + goalid + "";
        //                    int stdtGoal = Convert.ToInt32(objData.FetchValue(strStdtGoal));
        //                    if (stdtGoal == 0)
        //                    {
        //                        object objYearId = objData.FetchValue("SELECT AsmntYearId FROM AsmntYear WHERE CurrentInd='A'");
        //                        object objStat = objData.FetchValue("SELECT LookupId FROM LookUp WHERE LookupType='Goal Status' AND LookupName='In Progress'");
        //                        string nsGoal = "INSERT INTO StdtGoal(SchoolId,StudentId,GoalId,AsmntYearId,IncludeIEP,StatusId,ActiveInd,IEPGoalNo," +
        //                                "CreatedBy,CreatedOn) VALUES(" + sess.SchoolId + "," + Convert.ToInt32(hfSelectedStudent.Value) + "," + goalid + "," + objYearId.ToString() + ",0," +
        //                                "" + objStat.ToString() + ",'A',(SELECT ISNULL(MAX(IEPGoalNo),0)+1 FROM StdtGoal WHERE StudentId=" + Convert.ToInt32(hfSelectedStudent.Value) + " AND SchoolId=" + sess.SchoolId + " AND ActiveInd='A')," + sess.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";
        //                        objData.Execute(nsGoal);
        //                    }
        //                    string rtnStdt = SaveLessons(dt.Rows[0]["LessonPlanId"].ToString(), goalid, hfSelectedStudent.Value);

        //                    if (rtnStdt != null)
        //                    {
        //                        if (rtnStdt == "exists")
        //                        {
        //                            //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);


        //                            //tdMsgExprt.InnerHtml = clsGeneral.failedMsg("Lesson Plan already exists for the student ");
        //                            //txtSname.Text = "";
        //                            //return;

        //                            GetGoalAndLessonName(apprvdLessonId, hfSelectedStudent.Value, out NewLessonName, out NewLpid, out GoalId, out AsmntYr);
        //                            if (NewLpid > 0)
        //                            {
        //                                int StdtLpid = objData.ExecuteWithScope("INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,IncludeIEP,ActiveInd,StatusId,LessonPlanTypeDay,LessonPlanTypeResi,CreatedBy,CreatedOn) " +
        //                                               " VALUES('" + sess.SchoolId + "','" + Convert.ToInt32(hfSelectedStudent.Value) + "'," + NewLpid + ",'" + GoalId + "','" + AsmntYr + "','false','A',(SELECT LookupId FROM LookUp WHERE LookupType='LP Status' AND LookupName='In Progress'),(SELECT LessonPlanTypeDay FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),(SELECT LessonPlanTypeResi FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')),'" + sess.LoginId + "',GETDATE())");
        //                                Convert.ToInt32(objData.FetchValue("SELECT StdtLessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "'"));
        //                                int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId, Convert.ToInt32(hfSelectedStudent.Value), Convert.ToInt32(StdtLpid));
        //                                if (tempid > 0)
        //                                {
        //                                    CreateDocument(apprvdLessonId, tempid);
        //                                }
        //                                if (tempid > 0)
        //                                {
        //                                    string UpdateLessonName = "UPDATE DSTempHdr SET DSTemplateName='" + NewLessonName + "',LessonPlanId='" + NewLpid + "' WHERE DSTempHdrId=" + tempid;
        //                                    objData.Execute(UpdateLessonName);
        //                                }
        //                                FillData();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            int tempid = CopyCustomtemplate(apprvdLessonId, sess.LoginId, visualLessonId, Convert.ToInt32(hfSelectedStudent.Value), Convert.ToInt32(rtnStdt));
        //                            if (tempid > 0)
        //                            {
        //                                CreateDocument(apprvdLessonId, tempid);
        //                            }
        //                        }
        //                    }

        //                }
        //            }
        //            txtSname.Text = "";
        //            string NewName = "";
        //            if (NewLessonName != "")
        //            {
        //                NewName = "to <h3>" + NewLessonName + "</h3>";
        //            }
        //            tdMsgExprt.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
        //            tdReadMsg.InnerHtml = clsGeneral.sucessMsg("Template Successfully Copied " + NewName);
        //            // ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "closePOP();", true);
        //        }
        //        else
        //        {
        //            txtSname.Text = "";
        //            tdMsgExprt.InnerHtml = clsGeneral.warningMsg("Please select a student......");
        //            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please select a student......");
        //            //ScriptManager.RegisterClientScriptBlock(UpdatePanel4, UpdatePanel4.GetType(), "", "AlertCopySelectMsg();", true);
        //        }
        //        hfSelectedStudent.Value = "";
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //    }
        //}
        //catch (Exception ex)
        //{

        //}

    }

    private void GetGoalAndLessonName(int apprvdLessonId, string Studentid, out string NewLessonName, out int NewLpid, out DataTable GoalId, out object AsmntYr)
    {
        //NewLessonName = Convert.ToString(objData.FetchValue("SELECT  CASE WHEN  CHARINDEX('Copy of - ',DSTemplateName)>0 THEN DSTemplateName+'('+CONVERT(VARCHAR,(SELECT COUNT(*) FROM DSTempHdr WHERE "+
        //                " DSTemplateName LIKE 'Copy of - '+(SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId + ")+'%'))+')' ELSE CASE WHEN (SELECT COUNT(*) FROM DSTempHdr WHERE " +
        //                " DSTemplateName LIKE 'Copy of - '+ (SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId + ")+'%')>0 THEN ('Copy of - '+DSTemplateName+'('+CONVERT(VARCHAR,(SELECT COUNT(*) FROM DSTempHdr WHERE " +
        //                " DSTemplateName LIKE 'Copy of - '+(SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')+'%'))+')') ELSE 'Copy of - '+DSTemplateName END END TemplateName  " +
        //                " FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "'"));
        string LessonName = Convert.ToString(objData.FetchValue("SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId + ""));
        string Condition = "";
        if (Studentid != "0")
        {
            Condition = "(StudentId IS NULL OR StudentId='" + Studentid + "') AND isDynamic=0 AND ";
        }

        if (LessonName.Contains("_"))
        {
            DataTable dtName = objData.ReturnDataTable("SELECT DSTemplateName FROM DSTempHdr WHERE " + Condition + " DSTemplateName LIKE '" + LessonName.Split(new string[] { "_" }, StringSplitOptions.None)[0] + "'+'%'", false);
            string NewName = "";
            NewName = GetNewName(LessonName, dtName, NewName);

            NewLessonName = NewName;
        }
        else
        {
            if (Studentid != "0")
            {
                NewLessonName = Convert.ToString(objData.FetchValue("SELECT CASE WHEN (SELECT COUNT(*) FROM DSTempHdr WHERE  DSTemplateName LIKE (SELECT DSTemplateName FROM DSTempHdr WHERE " +
                          " DSTempHdrId=" + apprvdLessonId + ")+'%')>0 THEN (DSTemplateName+'_'+CONVERT(VARCHAR,((SELECT COUNT(*) FROM DSTempHdr WHERE  StudentId IS NOT NULL AND " +
                          " DSTemplateName LIKE (SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId + ")+'%')))) END TemplateName  " +
                          " FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId));
            }
            else
            {
                NewLessonName = Convert.ToString(objData.FetchValue("SELECT CASE WHEN (SELECT COUNT(*) FROM DSTempHdr WHERE  DSTemplateName LIKE (SELECT DSTemplateName FROM DSTempHdr WHERE " +
                          " DSTempHdrId=" + apprvdLessonId + ")+'%')>0 THEN (DSTemplateName+'_'+CONVERT(VARCHAR,((SELECT COUNT(*) FROM DSTempHdr WHERE  " +
                          " DSTemplateName LIKE (SELECT DSTemplateName FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId + ")+'%')))) END TemplateName  " +
                          " FROM DSTempHdr WHERE DSTempHdrId=" + apprvdLessonId));
            }
        }


        GoalId = objData.ReturnDataTable("SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=(SELECT StdtLessonplanId FROM DSTempHdr WHERE DSTempHdrId='" + apprvdLessonId + "')", false);

        NewLpid = AddLessonPlan(NewLessonName, GoalId, "", 0, "0", 1, out AsmntYr);
    }

    private static string GetNewName(string LessonName, DataTable dtName, string NewName)
    {
        int count = 1;
        while (count > 0)
        {
            bool IsExist = false;
            foreach (DataRow dtRow in dtName.Rows)
            {
                var field1 = dtRow["DSTemplateName"].ToString();
                if (LessonName + "_" + count.ToString() == field1)
                {
                    IsExist = true;
                }

            }

            if (IsExist == false)
            {
                NewName = LessonName + "_" + count.ToString();
                break;
            }
            count++;
        }
        return NewName;
    }
    public int CopyCustomtemplate(int templateid, int loginid, int visualLessonId, int studentid = 0, int stdtLpId = 0)
    {
        SqlTransaction Trans = null;
        SqlConnection Con = null;
        objData = new clsData();
        string strQuery = "";
        int oldSetId = 0;
        int parentSetId = 0;
        clsAssignLessonPlan AssignLP = new clsAssignLessonPlan();
        try
        {
            Con = objData.Open();
            Trans = Con.BeginTransaction();
            strQuery = "SELECT LessonPlanId,StudentId,SchoolId from DSTempHdr WHERE DSTempHdrId=" + templateid;
            DataTable dt = new DataTable();
            dt = objData.ReturnDataTable(strQuery, Con, Trans, false);
            //strQuery = "select goalid from goalLpRel where  LessonPlanId = " + dt.Rows[0]["LessonPlanId"].ToString() + "";
            //int glId=Convert.ToInt32(objData.FetchValueTrans(strQuery, Trans, Con));

            //strQuery = "SELECT MAX(VerNbr) from DSTempHdr WHERE LessonPlanId=" + Convert.ToInt32(dt.Rows[0]["LessonPlanId"]) + " AND StudentId=" + Convert.ToInt32(dt.Rows[0]["StudentId"]) + " AND [StatusId]<>(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Deleted')";
            //string version = objData.FetchValueTrans(strQuery, Trans, Con).ToString();
            //version = checkversion(version);
            //strQuery = "select StdtLessonPlanid from DSTempHdr where DSTempHdrId=" + templateid;
            //int stdtLpId = Convert.ToInt32(objData.FetchValueTrans(strQuery, Trans, Con));
            //strQuery = "Update DSTempHdr set [StatusId]=(SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Expired')  WHERE DSTempHdrId= " + templateid;
            //int expiredId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
            string stid = "", stval = "";

            int schoolid = Convert.ToInt32(dt.Rows[0]["SchoolId"]);
            if (studentid == 0 && stdtLpId == 0)
            {
                stid = ",";
                stval = ",";
            }
            else
            {
                stid = ",[StudentId],[StdtLessonplanId],";
                stval = ",'" + studentid + "','" + stdtLpId + "',";
            }
            if(schoolid==1)
                    strQuery = "INSERT INTO DSTempHdr ([SchoolId]" + stid + "[LessonPlanId],[TeachingProcId],[DSTemplateName]," +
                   "[DSTemplateDesc],[VerBeginDate],[NoofTimesTried],[NoofTimesTriedPer],[VerEndDate],[CurrVerInd],[MultiSetsInd],[MultiStepInd],[SkillType],[MatchToSampleType],[NbrOfTrials]," +
                   "[ChainType],[TotalTaskFormat],[TotalTaskType],[TaskOther],[MatchToSampleRecOrExp],[PromptTypeId],[TotNbrOfSessions],[SessionFreq],[NbrOfSession],[CompCurrInd],[StatusId],[IsVisualTool]," +
                   "[VTLessonId],[Baseline],[Objective],[GeneralProcedure],[BaselineProc],[BaselineStart],[BaselineEnd],[CorrRespDef]," +
                   "[CorrectResponse],[StudCorrRespDef],[IncorrRespDef],[StudIncorrRespDef],[CorrectionProc],[ReinforcementProc]," +
                   "[TeacherRespReadness],[StudentReadCrita],[MajorSetting],[MinorSetting],[LessonDefInst],[Mistrial],[MistrialResponse]," +
                   "[TeacherPrepare],[StudentPrepare],[StudResponse],[DSMode],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn]," +
                   "[FrameandStrand],[LessonPlanGoal],[SpecStandard],[SpecEntryPoint],[PreReq],[Materials],[ApprNoteLessonInfo],[ApprNoteTypeInstruction],[ApprNoteMeasurement],[ApprNoteSet],[ApprNoteStep],[ApprNotePrompt],[ApprNoteLessonProc],[deletessn],[LessonOrder],[LessonSDate],[LessonEDate]) SELECT [SchoolId]" + stval + "[LessonPlanId]," +
                   "[TeachingProcId],[DSTemplateName],[DSTemplateDesc],[VerBeginDate],[NoofTimesTried],[NoofTimesTriedPer],[VerEndDate],[CurrVerInd],[MultiSetsInd],[MultiStepInd]," +
                   "[SkillType],[MatchToSampleType],[NbrOfTrials],[ChainType],[TotalTaskFormat],[TotalTaskType],[TaskOther],[MatchToSampleRecOrExp],[PromptTypeId],[TotNbrOfSessions],[SessionFreq],[NbrOfSession],[CompCurrInd]," +
                   "(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='In Progress'),[IsVisualTool]," +
                   "'" + visualLessonId + "',[Baseline],[Objective],[GeneralProcedure],[BaselineProc],[BaselineStart],[BaselineEnd]," +
                   "[CorrRespDef],[CorrectResponse],[StudCorrRespDef],[IncorrRespDef],[StudIncorrRespDef],[CorrectionProc],[ReinforcementProc]," +
                   "[TeacherRespReadness],[StudentReadCrita],[MajorSetting],[MinorSetting],[LessonDefInst],[Mistrial],[MistrialResponse]," +
                   "[TeacherPrepare],[StudentPrepare],[StudResponse],[DSMode]," + loginid + ",GETDATE()," + loginid + ",GETDATE(),[FrameandStrand],[LessonPlanGoal]," +
                   "[SpecStandard],[SpecEntryPoint],[PreReq],[Materials],[ApprNoteLessonInfo],[ApprNoteTypeInstruction],[ApprNoteMeasurement],[ApprNoteSet],[ApprNoteStep],[ApprNotePrompt],[ApprNoteLessonProc],[deletessn],(select isnull( max(LessonOrder)+1,1) from dstemphdr where studentid=" + studentid + "),"+
                   "(Select DISTINCT EffStartDate  from StDtLessonPlan inner join StDtIEP on StDtLessonPlan.StDtIEPId=StDtIEP.StDtIEPId where StDtLessonPlan.StudentId = " + studentid + " AND StdtIEP.StatusId=65) ," +
                    "(Select DISTINCT EffEndDate  from StDtLessonPlan inner join StDtIEP on StDtLessonPlan.StDtIEPId=StDtIEP.StDtIEPId where StDtLessonPlan.StudentId = " + studentid + " AND StdtIEP.StatusId=65) FROM DSTempHdr WHERE DSTempHdrId='" + templateid + "'";
            if(schoolid==2)
                strQuery = "INSERT INTO DSTempHdr ([SchoolId]" + stid + "[LessonPlanId],[TeachingProcId],[DSTemplateName]," +
                  "[DSTemplateDesc],[VerBeginDate],[NoofTimesTried],[NoofTimesTriedPer],[VerEndDate],[CurrVerInd],[MultiSetsInd],[MultiStepInd],[SkillType],[MatchToSampleType],[NbrOfTrials]," +
                  "[ChainType],[TotalTaskFormat],[TotalTaskType],[TaskOther],[MatchToSampleRecOrExp],[PromptTypeId],[TotNbrOfSessions],[SessionFreq],[NbrOfSession],[CompCurrInd],[StatusId],[IsVisualTool]," +
                  "[VTLessonId],[Baseline],[Objective],[GeneralProcedure],[BaselineProc],[BaselineStart],[BaselineEnd],[CorrRespDef]," +
                  "[CorrectResponse],[StudCorrRespDef],[IncorrRespDef],[StudIncorrRespDef],[CorrectionProc],[ReinforcementProc]," +
                  "[TeacherRespReadness],[StudentReadCrita],[MajorSetting],[MinorSetting],[LessonDefInst],[Mistrial],[MistrialResponse]," +
                  "[TeacherPrepare],[StudentPrepare],[StudResponse],[DSMode],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn]," +
                  "[FrameandStrand],[LessonPlanGoal],[SpecStandard],[SpecEntryPoint],[PreReq],[Materials],[ApprNoteLessonInfo],[ApprNoteTypeInstruction],[ApprNoteMeasurement],[ApprNoteSet],[ApprNoteStep],[ApprNotePrompt],[ApprNoteLessonProc],[deletessn],[LessonOrder],[LessonSDate],[LessonEDate]) SELECT [SchoolId]" + stval + "[LessonPlanId]," +
                  "[TeachingProcId],[DSTemplateName],[DSTemplateDesc],[VerBeginDate],[NoofTimesTried],[NoofTimesTriedPer],[VerEndDate],[CurrVerInd],[MultiSetsInd],[MultiStepInd]," +
                  "[SkillType],[MatchToSampleType],[NbrOfTrials],[ChainType],[TotalTaskFormat],[TotalTaskType],[TaskOther],[MatchToSampleRecOrExp],[PromptTypeId],[TotNbrOfSessions],[SessionFreq],[NbrOfSession],[CompCurrInd]," +
                  "(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='In Progress'),[IsVisualTool]," +
                  "'" + visualLessonId + "',[Baseline],[Objective],[GeneralProcedure],[BaselineProc],[BaselineStart],[BaselineEnd]," +
                  "[CorrRespDef],[CorrectResponse],[StudCorrRespDef],[IncorrRespDef],[StudIncorrRespDef],[CorrectionProc],[ReinforcementProc]," +
                  "[TeacherRespReadness],[StudentReadCrita],[MajorSetting],[MinorSetting],[LessonDefInst],[Mistrial],[MistrialResponse]," +
                  "[TeacherPrepare],[StudentPrepare],[StudResponse],[DSMode]," + loginid + ",GETDATE()," + loginid + ",GETDATE(),[FrameandStrand],[LessonPlanGoal]," +
                  "[SpecStandard],[SpecEntryPoint],[PreReq],[Materials],[ApprNoteLessonInfo],[ApprNoteTypeInstruction],[ApprNoteMeasurement],[ApprNoteSet],[ApprNoteStep],[ApprNotePrompt],[ApprNoteLessonProc],[deletessn],(select isnull( max(LessonOrder)+1,1) from dstemphdr where studentid=" + studentid + ")," +
                  "(Select DISTINCT EffStartDate  from StDtLessonPlan inner join StDtIEP_PE on StDtLessonPlan.StDtIEPId=StDtIEP_PE.StDtIEP_PEId where StDtLessonPlan.StudentId = " + studentid + " AND StdtIEP_PE.StatusId=65) ," +
                   "(Select DISTINCT EffEndDate  from StDtLessonPlan inner join StDtIEP_PE on StDtLessonPlan.StDtIEPId=StDtIEP_PE.StDtIEP_PEId where StDtLessonPlan.StudentId = " + studentid + " AND StdtIEP_PE.StatusId=65) FROM DSTempHdr WHERE DSTempHdrId='" + templateid + "'";
            int TId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
            //strQuery = "UPDATE DSTempHdr SET VerNbr='" + version + "' WHERE DSTempHdrId=" + TId;
            //objData.ExecuteWithTrans(strQuery, Con, Trans);

            DataTable dtpromt = new DataTable();
            dtpromt = objData.ReturnDataTable("SELECT DSTempPromptId FROM DSTempPrompt WHERE DSTempHdrId=" + templateid + "", Con, Trans, false);
            if (dtpromt != null)
            {
                if (dtpromt.Rows.Count > 0)
                {
                    foreach (DataRow row in dtpromt.Rows)
                    {
                        strQuery = "INSERT INTO DSTempPrompt(DSTempHdrId,PromptId,PromptOrder,ActiveInd,CreatedBy,CreatedOn) ";
                        strQuery += "SELECT " + TId + ",PromptId,PromptOrder,ActiveInd," + loginid + ",CreatedOn FROM DSTempPrompt WHERE DSTempPromptId=" + Convert.ToInt32(row["DSTempPromptId"]) + "";
                        int PromptId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                    }
                }
            }
            DataTable dtset = new DataTable();
            Hashtable ht = new Hashtable();
            dtset = objData.ReturnDataTable("SELECT DSTempSetId FROM DSTempSet WHERE DSTempHdrId=" + templateid + "", Con, Trans, false);
            if (dtset != null)
            {
                if (dtset.Rows.Count > 0)
                {
                    foreach (DataRow row in dtset.Rows)
                    {
                        strQuery = "INSERT INTO DSTempSet(SchoolId,DSTempHdrId,PrevSetId,SetCd,SetName,Samples,SortOrder,ActiveInd,CreatedBy,CreatedOn,DistractorSamples,DistractorSamplesCount) ";
                        strQuery += "SELECT  SchoolId," + TId + ",PrevSetId,SetCd,SetName,Samples,SortOrder,ActiveInd," + loginid + ",getdate(),DistractorSamples,DistractorSamplesCount FROM DSTempSet WHERE DSTempSetId = " + Convert.ToInt32(row["DSTempSetId"]) + " ";
                        int SetId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                        if (!ht.ContainsKey(row["DSTempSetId"]))
                        {
                            ht.Add(row["DSTempSetId"], SetId);
                        }
                    }
                }
            }
            string teachingProc = "";
            string sqlStr = "";
            sqlStr = "SELECT DH.LessonPlanId,ISNULL(LU.LookupName,'') AS TeachingProc,ISNULL(LU.LookupDesc,'') AS TeachProc,ISNULL(LUp.LookupName,'') as PromptProc ,SkillType,ISNULL(NbrOfTrials,0) as NbrOfTrials," +
                    "LP.LessonPlanName,ISNULL(LP.Materials,'') as Mat,ISNULL(ChainType,'') AS ChainType,DH.IsVisualTool,ISNULL(DH.VTLessonId,0) as VTLessonId,ISNULL(ModificationInd,0) as ModificationInd FROM DSTempHdr DH JOIN LessonPlan LP ON LP.LessonPlanId=DH.LessonPlanId LEFT " +
                    "JOIN LookUp LU ON TeachingProcId=LU.LookUpId INNER JOIN Lookup LUp ON LUp.LookupId=PromptTypeId WHERE DSTempHdrId=" + templateid;
            DataTable dtTmpHdrDtls = objData.ReturnDataTable(sqlStr, false);
            if (dtTmpHdrDtls != null)
            {
                if (dtTmpHdrDtls.Rows.Count > 0)
                {
                    //teachingProc = dtTmpHdrDtls.Rows[0]["TeachingProc"].ToString();
                    teachingProc = dtTmpHdrDtls.Rows[0]["TeachProc"].ToString();
                }
            }
            if (teachingProc == "Match-to-Sample")
            {
                DataTable dtstep = new DataTable();
                dtstep = objData.ReturnDataTable("SELECT DSTempStepId,DSTempSetId FROM DSTempStep WHERE DSTempHdrId=" + templateid + " AND IsDynamic=0", Con, Trans, false);
                if (dtstep.Rows.Count > 0)
                {
                    foreach (DataRow row in dtstep.Rows)
                    {
                        oldSetId = Convert.ToInt32(row["DSTempSetId"]);
                        if (oldSetId != 0)
                        {
                            parentSetId = AssignLP.SetUpdateCopy(oldSetId, TId, Trans, Con);
                        }
                        strQuery =
                        strQuery = "INSERT INTO DSTempStep(SchoolId,DSTempHdrId,DSTempSetId,PrevStepId,DSTempParentStepId,StepCd,StepName,SortOrder,CreatedBy,ActiveInd,CreatedOn) ";
                        strQuery += "SELECT SchoolId," + TId + "," + parentSetId + ",PrevStepId,DSTempParentStepId,StepCd,StepName,SortOrder," + loginid + ",ActiveInd,GETDATE()	FROM DSTempStep WHERE DSTempStepId = " + Convert.ToInt32(row["DSTempStepId"]) + " AND IsDynamic=0 ";
                        int StepId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                    }
                }
            }
            else
            {
                int oldParentSetId = 0;
                DataTable dtParentStep = new DataTable();
                // strQuery = "INSERT INTO DSTempParentStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,SortOrder,SetIds,SetNames,ActiveInd,CreatedBy,CreatedOn) ";
                strQuery = "SELECT  DSTempParentStepId,SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,SortOrder,SetIds,SetNames,ActiveInd,CreatedBy,CreatedOn"
                    + " FROM DSTempParentStep WHERE DSTempHdrId = " + templateid;
                dtParentStep = objData.ReturnDataTable(strQuery, Con, Trans, false);
                //  int DSTempParentStepId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                // DataTable dt
                if (dtParentStep != null)
                {
                    if (dtParentStep.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtParentStep.Rows)
                        {
                            string newsetids = "";
                            foreach (string setid in row["SetIds"].ToString().Split(','))
                            {
                                if (setid != "")
                                {
                                    if (ht.ContainsKey(Convert.ToInt32(setid)))
                                    {
                                        newsetids += ht[Convert.ToInt32(setid)] + ",";
                                    }
                                }
                            }
                            oldParentSetId = Convert.ToInt32(row["DSTempParentStepId"]);
                            strQuery = "INSERT INTO DSTempParentStep(SchoolId,DSTempHdrId,StepCd,StepName,DSTempSetId,SortOrder,SetIds,SetNames,ActiveInd,CreatedBy,CreatedOn) "
                                        + "SELECT  SchoolId," + TId + ",StepCd,StepName,DSTempSetId,SortOrder,'" + newsetids + "',SetNames,ActiveInd," + loginid + ",getdate()"
                                        + " FROM DSTempParentStep WHERE DSTempHdrId = " + templateid + " AND DSTempParentStepId=" + oldParentSetId;
                            parentSetId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));

                            DataTable dtstep = new DataTable();

                            strQuery = "SELECT  SchoolId,PrevStepId,SortOrder,PreDefinedInd,CustomById,VTStepId,DSTempSetId,StepCd,StepName,ActiveInd,"
                                    + "DSTempParentStepId FROM DSTempStep WHERE DSTempParentStepId=" + oldParentSetId + " AND IsDynamic=0 AND DSTempHdrId = " + templateid;
                            dtstep = objData.ReturnDataTable(strQuery, Con, Trans, false);
                            if (dtstep.Rows.Count > 0)
                            {
                                foreach (DataRow rows in dtstep.Rows)
                                {
                                    oldSetId = Convert.ToInt32(rows["DSTempSetId"]);

                                    strQuery = "INSERT INTO DSTempStep(SchoolId,DSTempHdrId,DSTempSetId,PrevStepId,DSTempParentStepId,StepCd,StepName,SortOrder,CreatedBy,ActiveInd,CreatedOn) ";
                                    strQuery += "SELECT SchoolId," + TId + ",DSTempSetId,PrevStepId,DSTempParentStepId,StepCd,StepName,SortOrder," + loginid + ",ActiveInd,GETDATE()"
                                        + "	FROM DSTempStep WHERE DSTempSetId = " + oldSetId + " AND IsDynamic=0 AND DSTempParentStepId=" + oldParentSetId + " AND DSTempHdrId = " + templateid;
                                    int StepId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                                    strQuery = "SELECT DSTempSetId FROM DSTempStep WHERE DSTempStepId=" + StepId + " AND IsDynamic=0";
                                    int NewSetId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                                    if (ht.ContainsKey(Convert.ToInt32(NewSetId)))
                                    {
                                        newsetids = ht[Convert.ToInt32(NewSetId)].ToString();
                                        strQuery = "UPDATE DSTempStep SET DSTempSetId=" + Convert.ToInt32(ht[Convert.ToInt32(NewSetId)]) + ",DSTempParentStepId=" + parentSetId + " "
                                            + " WHERE DSTempStepId=" + StepId + " AND IsDynamic=0";
                                        int updateId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                                    }

                                }
                            }

                        }
                    }
                }
            }

            DataTable dtsetcol = new DataTable();
            dtsetcol = objData.ReturnDataTable("SELECT DSTempSetColId FROM DSTempSetCol WHERE DSTempHdrId=" + templateid + "", Con, Trans, false);
            if (dtsetcol != null)
            {
                if (dtsetcol.Rows.Count > 0)
                {
                    foreach (DataRow row in dtsetcol.Rows)
                    {
                        strQuery = "INSERT INTO DSTempSetCol(SchoolId, DSTempHdrId,ColName,ColTypeCd,CorrRespType,CorrResp,CorrRespDesc	,InCorrRespDesc,CorrStdtResp	,InCorrStdResp,IncMisTrialInd,MisTrialDesc,CalcuType,CalcuData,ActiveInd,CreatedBy,CreatedOn,MoveUpstat) ";
                        strQuery += "SELECT SchoolId, " + TId + ",ColName,ColTypeCd,CorrRespType,CorrResp,CorrRespDesc	,InCorrRespDesc,CorrStdtResp,InCorrStdResp,IncMisTrialInd,MisTrialDesc,CalcuType,CalcuData,ActiveInd," + loginid + ",CreatedOn,MoveUpstat FROM DSTempSetCol WHERE DSTempSetColId = " + Convert.ToInt32(row["DSTempSetColId"]) + " ";
                        int setColNewId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                        DataTable dtsetcolcalc = new DataTable();
                        dtsetcolcalc = objData.ReturnDataTable("SELECT DSTempSetColCalcId FROM DSTempSetColCalc WHERE DSTempSetColId=" + Convert.ToInt32(row["DSTempSetColId"]) + "", Con, Trans, false);
                        if (dtsetcolcalc.Rows.Count > 0)
                        {
                            foreach (DataRow rowc in dtsetcolcalc.Rows)
                            {
                                strQuery = "INSERT INTO DSTempSetColCalc(SchoolId,DSTempSetColId,CalcType,CalcLabel,CalcFormula,CalcRptLabel,ActiveInd,CreatedBy,CreatedOn,IncludeInGraph) " +
                                            "SELECT SchoolId," + setColNewId + ",CalcType,CalcLabel,CalcFormula,CalcRptLabel,ActiveInd," + loginid + ",getdate(),IncludeInGraph FROM DSTempSetColCalc WHERE DSTempSetColCalcId=" + Convert.ToInt32(rowc["DSTempSetColCalcId"]) + "";
                                int setColCalId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));

                                strQuery = "INSERT INTO DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,ConsequetiveAvgInd,MultiTeacherReqInd,IOAReqInd,LogicalCombType,ActiveInd,IsComment,IsNA,ModificationComment,ModificationRule,CreatedBy,CreatedOn) "; //--- [New Criteria] May 2020 ---//
                                strQuery += "SELECT  " + TId + ",SchoolId," + setColNewId + "," + setColCalId + ",RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,ISNULL(ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,MultiTeacherReqInd,IOAReqInd,LogicalCombType,ActiveInd,IsComment,IsNA,ModificationComment,ModificationRule,CreatedBy,CreatedOn FROM DSTempRule WHERE DSTempSetColId=" + Convert.ToInt32(row["DSTempSetColId"]) + " And DSTempSetColCalcId=" + Convert.ToInt32(rowc["DSTempSetColCalcId"]) + " "; //--- [New Criteria] May 2020 ---//
                                int lastId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));
                            }
                        }

                    }
                    strQuery = "INSERT INTO DSTempRule(DSTempHdrId,SchoolId,DSTempSetColId,DSTempSetColCalcId,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,ConsequetiveAvgInd,MultiTeacherReqInd,IOAReqInd,LogicalCombType,ActiveInd,IsComment,IsNA,ModificationComment,ModificationRule,CreatedBy,CreatedOn) "; //--- [New Criteria] May 2020 ---//
                    strQuery += "SELECT  " + TId + ",SchoolId,0,0,RuleType,CriteriaType,ScoreReq,TotalInstance,TotCorrInstance,ConsequetiveInd,ISNULL(ConsequetiveAvgInd,0) AS ConsequetiveAvgInd,MultiTeacherReqInd,IOAReqInd," //--- [New Criteria] May 2020 ---//
                        + "LogicalCombType,ActiveInd,IsComment,IsNA,ModificationComment,ModificationRule,CreatedBy,CreatedOn FROM DSTempRule WHERE" +
                        " DSTempSetColId=0 And DSTempSetColCalcId=0 AND DSTempHdrId=" + templateid;
                    int lastModRuleId = Convert.ToInt32(objData.ExecuteWithScopeandConnection(strQuery, Con, Trans));

                }
            }
            objData.CommitTransation(Trans, Con);
            return TId;
        }
        catch (Exception Ex)
        {
            objData.RollBackTransation(Trans, Con);
            ClsErrorLog errlog = new ClsErrorLog();
            errlog.WriteToLog("Page Name: " + clsGeneral.getPageName() + "\n" + Ex.ToString());
            return 0;
        }
    }

    public string SaveLessons(string LPid, string goalid, string studentid)
    {
        SqlTransaction Trans = null;
        SqlConnection Con = new SqlConnection();
        clsData oData = new clsData();
        try
        {

            Con = oData.Open();
            Trans = Con.BeginTransaction();
            object objIEP = "";
            clsSession oSession = (clsSession)Session["UserSession"]; ;
            if (oSession != null)
            {
                if (oSession.SchoolId == 2)
                {
                    string selQry = "select isnull(StdtIEP_PEId,0) StdtIEP_PEId from StdtIEP_PE I inner join AsmntYear yr on I.AsmntYearId=yr.AsmntYearId "
                        + " where StatusId=(select LookUpId from [LookUp] where LookupName='In Progress' and LookupType='IEP Status')"
                        + " and StudentId=" + studentid + " AND I.SchoolId=" + oSession.SchoolId + " AND Yr.CurrentInd='A' ";
                    objIEP = oData.FetchValueTrans(selQry, Trans, Con);
                }
                else if (oSession.SchoolId == 1)
                {
                    string selQry = "select isnull(StdtIEPId,0) StdtIEPId from StdtIEP I inner join AsmntYear yr on I.AsmntYearId=yr.AsmntYearId "
                        + " where StatusId=(select LookUpId from [LookUp] where LookupName='In Progress' and LookupType='IEP Status')"
                        + " and StudentId=" + studentid + " AND I.SchoolId=" + oSession.SchoolId + " AND Yr.CurrentInd='A' ";
                    objIEP = oData.FetchValueTrans(selQry, Trans, Con);
                }

                if (objIEP == null)
                    objIEP = 0;
                //query to check current class status;
                string qryclass = "select ResidenceInd from [dbo].[Class] where classid=" + oSession.Classid;
                bool isDay = Convert.ToBoolean(oData.FetchValueTrans(qryclass, Trans, Con));

                bool lpDupExists = oData.IFExistsWithTranss("SELECT * FROM StdtLessonPlan WHERE StudentId=" + studentid + " AND LessonPlanId=" + LPid + " AND ActiveInd='A'", Trans, Con);
                //bool lpDelExists = oData.IFExistsWithTranss("SELECT * FROM StdtLessonPlan WHERE StudentId=" + studentid + " AND LessonPlanId=" + LPid + " AND ActiveInd='D'", Trans, Con);
                // bool lpExists = oData.IFExists("SELECT * FROM StdtLessonPlan WHERE StudentId=" + oSession.StudentId + " AND GoalId=" + goalid + " AND LessonPlanId=" + LPid + " AND ActiveInd='A' AND (IncludeIEP=0 OR (IncludeIEP=1 AND StdtIEPId=" + objIEP.ToString() + "))");
                //if (!lpDupExists && !lpDelExists)
                if (!lpDupExists)  //chech whether the LP is exists or not.....
                {
                    object objYearId = oData.FetchValueTrans("SELECT AsmntYearId FROM AsmntYear WHERE CurrentInd='A'", Trans, Con);
                    if (objYearId != null)
                    {
                        string strQuery = "SELECT DSTempHdrId FROM dbo.DSTempHdr WHERE LessonPlanId=" + LPid + " and StudentId IS NULL ";
                        object flag = oData.FetchValueTrans(strQuery, Trans, Con);
                        object objStat = oData.FetchValueTrans("SELECT LookupId FROM LookUp WHERE LookupType='LP Status' AND LookupName='In Progress'", Trans, Con);
                        object objLPname = oData.FetchValueTrans("SELECT LessonPlanName FROM LessonPlan WHERE LessonPlanId=" + LPid + "", Trans, Con);
                        if ((objStat != null) && (objLPname != null))
                        {
                            string insLP = "";
                            if (Convert.ToInt32(objIEP) == 0)
                            {

                                if (Convert.ToInt32(flag) > 0)
                                {
                                    insLP = "INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,IncludeIEP,StatusId," +
                                                    "ActiveInd,isDynamic,CreatedBy,CreatedOn) VALUES(" + oSession.SchoolId + "," + studentid + "," + LPid + "," + goalid + "," + objYearId.ToString() + ",0," +
                                                                       "" + objStat.ToString() + ",'A',0," + oSession.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";
                                }
                                else
                                    insLP = "INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,IncludeIEP,StatusId," +
                                                    "ActiveInd,isDynamic,CreatedBy,CreatedOn) VALUES(" + oSession.SchoolId + "," + studentid + "," + LPid + "," + goalid + "," + objYearId.ToString() + ",0," +
                                                                       "" + objStat.ToString() + ",'A',1," + oSession.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";

                            }
                            else
                            {
                                if (Convert.ToInt32(flag) > 0)
                                {
                                    insLP = "INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,StdtIEPId,IncludeIEP,StatusId," +
                                                    "ActiveInd,isDynamic,CreatedBy,CreatedOn) VALUES(" + oSession.SchoolId + "," + studentid + "," + LPid + "," + goalid + "," + objYearId.ToString() + "," + objIEP.ToString() + ",1," +
                                                                       "" + objStat.ToString() + ",'A',0," + oSession.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";
                                }
                                else
                                    insLP = "INSERT INTO StdtLessonPlan(SchoolId,StudentId,LessonPlanId,GoalId,AsmntYearId,StdtIEPId,IncludeIEP,StatusId," +
                                                    "ActiveInd,isDynamic,CreatedBy,CreatedOn) VALUES(" + oSession.SchoolId + "," + studentid + "," + LPid + "," + goalid + "," + objYearId.ToString() + "," + objIEP.ToString() + ",1," +
                                                                       "" + objStat.ToString() + ",'A',1," + oSession.LoginId + ",(SELECT convert(varchar, getdate(), 100)))";
                            }

                            int stdtLPID = oData.ExecuteWithScopeandConnection(insLP, Con, Trans);

                            string condtn = "";// 

                            if (!isDay)
                                condtn = "SET LessonPlanTypeDay=1";
                            else
                                condtn = "SET LessonPlanTypeResi=1";

                            oData.ExecuteWithScopeandConnection("UPDATE StdtLessonPlan " + condtn + " WHERE StdtLessonPlanId=" + stdtLPID + "", Con, Trans);

                            oData.CommitTransation(Trans, Con);
                            return stdtLPID.ToString();

                        }
                        else
                        {
                            //oData.RollBackTransation(Trans, Con);
                            return "0";
                        }

                    }
                    else
                    {
                        return "0";
                    }

                }
                else
                {
                    //if (lpDelExists)
                    //{
                    //    clsAssignLessonPlan objTemplateAssign = new clsAssignLessonPlan();
                    //    object objLPname = oData.FetchValueTrans("SELECT LessonPlanName FROM LessonPlan WHERE LessonPlanId=" + LPid, Trans, Con);

                    //    int result = oData.ExecuteWithTrans("UPDATE StdtLessonPlan SET ActiveInd='A' WHERE StudentId=" + studentid + " AND LessonPlanId=" + LPid + " ", Con, Trans);
                    //    int stdtLessonPID = Convert.ToInt32(oData.FetchValueTrans("SELECT StdtLessonPlanId FROM  StdtLessonPlan WHERE StudentId=" + oSession.StudentId + " AND LessonPlanId=" + LPid + " AND ActiveInd='A'", Trans, Con));

                    //    oData.CommitTransation(Trans, Con);
                    //    return stdtLessonPID.ToString();

                    //}


                    //returns 'exists' if already exists....
                    return "exists";
                }
            }
            else
            {
                return "0";
            }

        }
        catch (Exception ex)
        {
            oData.RollBackTransation(Trans, Con);
            ClsErrorLog errlog = new ClsErrorLog();
            errlog.WriteToLog("Page Name: " + clsGeneral.getPageName() + "\n" + ex.ToString());
            throw ex;
        }


    }

    //------->>----->>>   

    protected void BtnSearchLesson_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        string strQuerry = "";
        try
        {
            if (txtSearcLesson.Text != null)
            {
                strQuerry = "SELECT DSTempHdrId As ID,DSTemplateName As Name FROM DSTempHdr WHERE DSTemplateName like '%" + clsGeneral.convertQuotes(txtSearcLesson.Text.Trim()) + "%' AND isDynamic=0 AND StudentId IS NULL AND StatusId NOT IN(SELECT LookupId FROM LookUp WHERE LookupName='SoftDelete') ORDER BY Name ";
                DataTable dtList = objData.ReturnDataTable(strQuerry, false);
                if (dtList != null)
                {
                    if (dtList.Rows.Count > 0)
                    {
                        dlApprovedLessonsAdmin.DataSource = dtList;
                        dlApprovedLessonsAdmin.DataBind();
                    }
                    else
                    {
                        dlApprovedLessonsAdmin.DataSource = dtList;
                        dlApprovedLessonsAdmin.DataBind();
                    }
                }
                else
                {
                    dlApprovedLessonsAdmin.DataSource = dtList;
                    dlApprovedLessonsAdmin.DataBind();
                }
            }
            else
            {
                FillApprovedLessonDataAdmin();
            }
        }
        catch (Exception Ex)
        {

        }
    }
    protected void dlApprovedLessonsAdmin_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        objData = new clsData();
        LinkButton lnkApprvdLess = (LinkButton)e.Item.FindControl("lnkApprovedLessonsAdmin");
        try
        {
            if (lnkApprvdLess.Text.ToString() != null)
            {
                if (lnkApprvdLess.Text.ToString().Length > 26)
                {
                    lnkApprvdLess.Text = lnkApprvdLess.Text.ToString().Substring(0, 26) + ".....";
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void FillApprovedLessonDataAdmin()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string selctQuerry = "";

        try
        {
            if (sess != null)
            {

                //selctQuerry = " SELECT LP.LessonPlanName as Name,StdtLP.GoalId, DTmp.DSTempHdrId As ID,DTmp.StatusId,LU.LookupName " +
                //                      " FROM (StdtLessonPlan StdtLP INNER JOIN (DSTempHdr DTmp  INNER JOIN LookUp LU ON LU.LookupId=DTmp.StatusId)  " +
                //                    " ON DTmp.LessonPlanId=StdtLP.LessonPlanId AND DTmp.StudentId=StdtLP.StudentId) " +
                //                        "   INNER JOIN LessonPlan LP ON StdtLP.LessonPlanId=LP.LessonPlanId WHERE StdtLP.StudentId=" + sess.StudentId + " AND " +
                //                             "  StdtLP.ActiveInd='A' AND LU.LookupName<>'Expired' AND LU.LookupName = 'Approved' ";


                //selctQuerry = "SELECT  LessonPlanId As ID,LessonPlanName As Name FROM LessonPlan WHERE ActiveInd = 'A'";
                selctQuerry = "SELECT DSTempHdrId As ID,DSTemplateName As Name FROM DSTempHdr WHERE StudentId IS NULL AND isDynamic=0 AND StatusId NOT IN(SELECT LookupId FROM LookUp WHERE LookupName='SoftDelete') ORDER BY Name";
                DataTable dtList = objData.ReturnDataTable(selctQuerry, false);
                dlApprovedLessonsAdmin.DataSource = dtList;
                dlApprovedLessonsAdmin.DataBind();
            }

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void lnkApprovedLessonsAdmin_Click(object sender, EventArgs e)
    {

        objData = new clsData();
        tdReadMsg.InnerHtml = "";
        btnDelLp.Visible = true;
        //BtnUpdateLessonPlan.Visible = true;
        // bool visibility = false;
        
        try
        {
            Clear();
            int headerId = 0;
            if (Request.QueryString["DatabankMode"] == "OpenOrEdit")
            {
                BtnAdminPreview.Visible = true;
                headerId = Convert.ToInt32(Request.QueryString["DSTempHdrId"].ToString());
            }
            else
            {
                LinkButton lnkApprvdless = (LinkButton)sender;
                headerId = Convert.ToInt32(lnkApprvdless.CommandArgument);
                if (DeletePermission())
                {
                    btnDeleteLPAdmin.Visible = true;
                }
            }
            ViewState["HeaderId"] = headerId;

            lblloadAlert.Visible = false;
            btnNew.Style.Add("display", "block");

            BtnUpdateLessonPlan.Text = "Update";
            MainDiv.Visible = true;                      // Right Main Div Visiblity option set true.
            IsDiscrete(headerId);                       // Check the template is discrete
            lblDataFill(headerId);                      // Fill Current LessonName
            LessonInfoAdmin(headerId);                       // Fill the lessonPlanDetails.
            LoadGoalAdmin(headerId);                     //fill Goals for the Template
            GetMeasureData(headerId);                   // Fill the measure Data 
            FillTypeOfInstruction(headerId);           // Fill Type of Instruction Data
            GetSetData(headerId);                      // Fill the Sets Data
            GetStepData(headerId);                    // Fill the Steps Data
            GetSetCriteriaData(headerId);             // Fill the Set Criteria Data
            GetStepCriteriaData(headerId);           // Fill the Step Criteria Data
            GetPromptCriteriaData(headerId);        // Fill the Prompt Criteria Data
            GetPromptProcedureList(headerId);      // Fill the prompt Procedure...
            FillDropPrompt(headerId);             // Fill dropdownlist prompt procedure...
            GetLessonProcData(headerId);           // Fill LessonProcedureData
            fillStepAndSet(headerId);
            FillDocSmall(headerId);                 //fill Doc
            FindType(headerId);                       //Check Is Visual Lesson or not
            EnableEdit(headerId);


            CheckAsigned(headerId);

            //setWritePermissions(true);
            VisibleApprovalNotes(false);
            VisibleApprovalNote();
            ApprovalNoteVisibilityAdmin();

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        drpTasklist_SelectedIndexChanged1(sender, e);
    }


    protected void BtnAddNewLessonPlan_Click(object sender, EventArgs e)
    {
        btnNew.Style.Add("display", "none");
        btnDeleteLPAdmin.Visible = false;
        BtnUpdateLessonPlan.Text = "Save";
        AddNewLesson();
        FillApprovedLessonData();
        ApprovalNoteVisibilityAdmin();
        txtSearcLesson.Text = "";
        grdFile.DataSource = null;
        grdFile.DataBind();
        ClientScript.RegisterStartupScript(this.GetType(), "", "LoadAdminTemplateView(0);", true);
    }

    protected void btnDeleteLPAdmin_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        SqlTransaction Transs = null;
        SqlConnection con = objData.Open();
        string DelQuery = "";
        string UpdateQuery = "";
        int HeaderId = Convert.ToInt32(ViewState["HeaderId"]);
        clsData.blnTrans = true;
        Transs = con.BeginTransaction();
        DelQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId='" + HeaderId + "'";
        int LessonPlanId = Convert.ToInt32(objData.FetchValueTrans(DelQuery, Transs, con));
        //DelQuery = "DELETE FROM LessonPlan WHERE LessonPlanId='" + LessonPlanId + "'";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM GoalLPRel WHERE LessonPlanId='" + LessonPlanId + "'";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM StdtLessonPlan WHERE LessonPlanId='" + LessonPlanId + "'";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempPrompt WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempSet WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempStep WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempParentStep WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempRule WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempSetColCalc WHERE DSTempSetColId IN  (SELECT DSTempSetColId FROM DSTempSetCol WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "'))";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempSetCol WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        UpdateQuery = "UPDATE DSTempHdr SET StatusId=(SELECT LookupId FROM LookUp WHERE LookupName='SoftDelete') WHERE DSTempHdrId='" + HeaderId + "'";
        objData.ExecuteWithTrans(UpdateQuery, con, Transs);
        DataTable dtdoc = objData.ReturnDataTableWithTransaction("SELECT LPDoc FROM LPDoc WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')", con, Transs, false);
        if (dtdoc != null)
        {
            if (dtdoc.Rows.Count > 0)
            {
                foreach (DataRow row in dtdoc.Rows)
                {
                    DelQuery = "DELETE FROM binaryFiles WHERE DocId=" + row["LPDoc"].ToString() + " AND type='LP_DOC' ";
                    objData.ExecuteWithTrans(DelQuery, con, Transs);
                }
            }
        }

        //DelQuery = "DELETE FROM LPDoc WHERE DSTempHdrId IN (SELECT DSTempHdrId FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "')";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);
        //DelQuery = "DELETE FROM DSTempHdr WHERE LessonPlanId='" + LessonPlanId + "'";
        //objData.ExecuteWithTrans(DelQuery, con, Transs);


        objData.CommitTransation(Transs, con);

        btnNew.Style.Add("display", "none");
        btnDeleteLPAdmin.Visible = false;
        BtnAddNewLessonPlan_Click(sender, e);
        FillApprovedLessonDataAdmin();
    }


    protected void AddNewLesson()
    {
        objData = new clsData();
        int headerId = -1;
        ViewState["HeaderId"] = null;
        txtLessonName.Text = "";
        txtNoofTimesTried.Text = "";
        noofTimesTriedPer.SelectedIndex = 0;
        txtFramework.Text = "";
        txtSpecStandrd.Text = "";
        txtSpecEntrypoint.Text = "";
        txtPreSkills.Text = "";
        txtMaterials.Text = "";
        txtBaseline.Text = "";
        txtobjective.Text = "";
        txtGenProce.Text = "";
        txtLessonPlanGoal.Text = "";

        // rdolistDatasheet.SelectedValue = "1";
        //showtrailid.Visible = false;
        //txtNoofTrail.Visible = false;
        taskAnalysis.Visible = true;
        drpTasklist.Visible = true;
        drpTasklist.SelectedIndex = 0;
        txtNoofTrail.Text = "";
        txtmajorset.Text = "";
        txtminorset.Text = "";
        rdoRandomMoveover.Visible = false;

        txtSDInstruction.Text = "";
        txtResponseOutcome.Text = "";
        //     txtCorrectResponse.Text = "";
        txtIncorrectResponse.Text = "";
        txtCorrectionProcedure.Text = "";
        txtReinforcementProc.Text = "";

        ddlPromptProcedure.SelectedIndex = 0;
        lstSelectedPrompts.Items.Clear();
        objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt'", lstCompletePrompts);



        MainDiv.Visible = true;                      // Right Main Div Visiblity option set true.
        IsDiscrete(headerId);                       // Check the template is discrete
        lblDataFill(headerId);                      // Fill Current LessonName
        LessonInfoAdmin(headerId);                       // Fill the lessonPlanDetails.
        GetMeasureData(headerId);                   // Fill the measure Data 
        FillTypeOfInstruction(headerId);                   // Fill Type of Instruction Data
        GetSetData(headerId);                      // Fill the Sets Data
        GetStepData(headerId);                    // Fill the Steps Data
        GetSetCriteriaData(headerId);             // Fill the Set Criteria Data
        GetStepCriteriaData(headerId);           // Fill the Step Criteria Data
        GetPromptCriteriaData(headerId);        // Fill the Prompt Criteria Data
        FillDropPrompt(headerId);             // Fill dropdownlist prompt procedure...
        GetPromptProcedureList(headerId);      // Fill the prompt Procedure...
        GetLessonProcData(headerId);           // Fill LessonProcedureData
        //   GetStatus(headerId);                    // Button Permissions
        //    SetLessonProcedure(headerId);                       // Fill Lesson Procedure Data

        foreach (ListItem li in chkgoal.Items)
        {
            li.Selected = false;
        }

        lblCaptnLesson.Text = "";
        lblcurrntLessonName.Text = "";
        //lblFleUploadTxt.Text = "";





        //  setWritePermissions(false);

        BtnUpdate.Text = "Save";
        btUpdateLessonProc.Text = "Save";
        BtnSavePrompt.Text = "Save";
    }
    private void FillGoalData()  //GoalCode is used to display the Edited goals
    {
        try
        {
            //stdLevel.Visible = false;
            objData = new clsData();
            DataTable dtList = new DataTable();
            dtList = objData.ReturnDataTable("select Goal.GoalId,Goal.GoalCode from Goal inner join GoalType on Goal.GoalTypeId =GoalType.GoalTypeId where GoalType.GoalTypeName = 'Academic Goals' and Goal.ActiveInd='A' order by Goal.GoalCode", true);
            chkgoal.DataSource = dtList;
            chkgoal.DataTextField = "GoalCode";
            chkgoal.DataValueField = "GoalId";
            chkgoal.DataBind();

            // objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt'", lstCompletePrompts);

        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected bool ValidateLesson()
    {
        if (txtLessonName.Text == "")
        {
            ClientScript.RegisterStartupScript(this.GetType(), "", "NoLessonName();", true);
            return false;
        }
        return true;
    }

    protected void LoadGoalAdmin(int templateId)
    {
        object objId;
        int lessonPlanId = 0;
        string strQuery = "";
        strQuery = "SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId = " + templateId;
        objId = objData.FetchValue(strQuery);
        if (objId.ToString() != "")
        {
            lessonPlanId = Convert.ToInt32(objId);
        }
        try
        {
            strQuery = "select GoalId from dbo.GoalLPRel where lessonPlanId = " + lessonPlanId + " and ActiveInd = 'A'";
            DataTable dt = objData.ReturnDataTable(strQuery, true);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            foreach (ListItem li in chkgoal.Items)
                            {
                                if (li.Value == dt.Rows[i]["GoalId"].ToString())
                                {
                                    li.Selected = true;
                                }
                            }
                        }

                    }
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
    }
    protected void Clear()
    {
        objData = new clsData();
        txtLessonName.Text = "";
        txtNoofTimesTried.Text = "";
        noofTimesTriedPer.SelectedIndex = 0;
        txtFramework.Text = "";
        txtSpecStandrd.Text = "";
        txtSpecEntrypoint.Text = "";
        txtPreSkills.Text = "";
        txtMaterials.Text = "";
        txtBaseline.Text = "";
        txtobjective.Text = "";
        txtLessonPlanGoal.Text = "";


        //showtrailid.Visible = false;
        //txtNoofTrail.Visible = false;
        taskAnalysis.Visible = true;
        drpTasklist.Visible = true;
        drpTasklist.SelectedIndex = 0;
        txtNoofTrail.Text = "";
        txtmajorset.Text = "";
        txtminorset.Text = "";
        rdoRandomMoveover.Visible = false;

        txtSDInstruction.Text = "";
        txtResponseOutcome.Text = "";
        //     txtCorrectResponse.Text = "";
        txtIncorrectResponse.Text = "";
        txtCorrectionProcedure.Text = "";
        txtReinforcementProc.Text = "";

        ddlPromptProcedure.SelectedIndex = 0;
        lstSelectedPrompts.Items.Clear();
        objData.ReturnListBox("Select LookupId as Id , LookupName as Name from dbo.LookUp where LookupType='DSTempPrompt'", lstCompletePrompts);


        foreach (ListItem li in chkgoal.Items)
        {
            li.Selected = false;
        }

        lblCaptnLesson.Text = "";
        lblcurrntLessonName.Text = "";
        //lblFleUploadTxt.Text = "";

    }
    protected void ApprovalNoteVisibilityAdmin()
    {
        txtCommentLessonInfo.Visible = false;
        txtCommentTypeofInstr.Visible = false;
        txtMeasurementSystems.Visible = false;
        txtcommentset.Visible = false;
        txtcommentStep.Visible = false;
        txtcommentPrompt.Visible = false;
        txtcommentLessonProcedure.Visible = false;

        lblCommentLessonInfo.Visible = false;
        LessoninfoApptooltip.Visible = false;
        lblCommentTypeofInstr.Visible = false;
        TypeInstructTooltip.Visible = false;
        lblMeasurementSystems.Visible = false;
        measurementsystemtooltip.Visible = false;
        lblcommentset.Visible = false;
        commentSetTooltip.Visible = false;
        lblcommentStep.Visible = false;
        commentStepTooltip.Visible = false;
        lblcommentPrompt.Visible = false;
        commentPromptTooltp.Visible = false;
        lblcommentLessonProcedure.Visible = false;
        commentProceduretooltp.Visible = false;
        lblMeasureStart.Visible = false;
        lblSetStart.Visible = false;
        lblSetCriStart.Visible = false;
        lblStepStart.Visible = false;
        lblStepCriStart.Visible = false;
        lblPromptCriStart.Visible = false;
    }
    [WebMethod]
    public static string SearchLessonPlanList(string Name, string StudId)
    {
        object GoalApproved = HttpContext.Current.Session["GoalID_Approved"];
        Dataobj = new clsData();
        //int Template = 0;
        //int StudentLp = 0;
        int LPcount = 0;
        if (StudId == "")
        {
            //Template = Convert.ToInt32(Dataobj.FetchValue("SELECT COUNT(*) FROM DSTempHdr WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + Name.Trim() + "'))) and StatusId<>(select LookupId from LookUp where LookupType='TemplateStatus' and LookupName='Deleted') "));
            LPcount = Convert.ToInt32(Dataobj.FetchValue("SELECT COUNT(*) FROM DSTempHdr HD inner join GoalLPRel GR on HD.lessonplanid=GR.lessonplanid WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + clsGeneral.convertQuotes(Name.Trim()) + "'))) and StatusId<>(select LookupId from LookUp where LookupType='TemplateStatus' and LookupName='Deleted') AND StudentId IS NULL AND isDynamic=0 AND GR.GOALID='" + GoalApproved + "' AND GR.ACTIVEIND='A'" ));
        }
        else
        {
            //Template = Convert.ToInt32(Dataobj.FetchValue("SELECT COUNT(*) FROM DSTempHdr WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + Name.Trim() + "'))) AND StudentId IS NULL AND isDynamic=0"));
            //StudentLp = Convert.ToInt32(Dataobj.FetchValue("SELECT COUNT(*) FROM DSTempHdr inner join LookUp lu on lu.LookupId=DSTempHdr.StatusId WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + Name.Trim() + "'))) AND StudentId='" + StudId + "' AND lu.LookupName <> 'Deleted'"));
            LPcount = Convert.ToInt32(Dataobj.FetchValue("SELECT COUNT(*) FROM DSTempHdr inner join LookUp lu on lu.LookupId=DSTempHdr.StatusId WHERE  RTRIM(LTRIM(LOWER(DSTemplateName)))= RTRIM(LTRIM(LOWER('" + clsGeneral.convertQuotes(Name.Trim()) + "'))) AND StudentId='" + StudId + "' AND (SELECT GoalId FROM StdtLessonPlan WHERE StdtLessonPlanId=DSTempHdr.StdtLessonplanId)='" + GoalApproved + "' and lu.LookupName <> 'Deleted'"));
        }

        if (LPcount > 0)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }
    protected void btnRjctApLp_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        int tmpId = Convert.ToInt32(ViewState["HeaderId"]);
        txtApLPReason.Text = "";
        string Qury = "UPDATE DSTempHdr SET Reason_New='" + clsGeneral.convertQuotes(txtApLPReason.Text.Trim()) + "' WHERE DSTempHdrId=" + tmpId + "";
        int ast = objData.Execute(Qury);

    }
    protected void btnAddApLp_Click(object sender, EventArgs e)
    {
        sess = (clsSession)Session["UserSession"];
        objData = new clsData();
        int tmpId = Convert.ToInt32(ViewState["HeaderId"]);
        btndoc.Visible = true;        
        if (ViewState["HeaderId"] != null)
        {
            int apprvLessId = Convert.ToInt32(objData.FetchValue("SELECT COUNT(*) FROM DSTempHdr WHERE LessonPlanId= (SELECT LessonPlanId FROM DSTempHdr WHERE DSTempHdrId=" + tmpId + ") AND StudentId=" + sess.StudentId + " AND StatusId IN (SELECT LookupId FROM LookUp WHERE LookupType='TemplateStatuS' AND (LookupName='In Progress' OR LookupName='Pending Approval'))"));
            if (apprvLessId == 0)
            {
                BtnCopyTemplate_Click(sender, e);
                //string Qury = "UPDATE DSTempHdr SET LessonSDate='" + lessonSDate.Text + "' and LessonEDate='" + lessonEDate.Text + "' and Reason_New='" + clsGeneral.convertQuotes(txtApLPReason.Text.Trim()) + "' WHERE DSTempHdrId=" + tmpId + "";  //---Commmented for Fixing Errorlog production log 2020
                string Qury = "UPDATE DSTempHdr SET LessonSDate='" + lessonSDate.Text + "' ,LessonEDate='" + lessonEDate.Text + "' ,Reason_New='" + clsGeneral.convertQuotes(txtApLPReason.Text.Trim()) + "' WHERE DSTempHdrId=" + tmpId + ""; //Modifed query for above query prduction log
                int ast = objData.Execute(Qury);
                txtApLPReason.Text = "";
                lessonSDate.Enabled = true;
                lessonEDate.Enabled = true;
                VisibleApprovalNotesPR();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(System.Web.UI.Page), Guid.NewGuid().ToString(), "$(document).ready(function(){$('#overlay').fadeIn('slow', function () { $('#DivAlertAPPLP').animate({ top: '5%' },{duration: 'slow',easing: 'linear'}) }); });", true);                
            }
        }
        



    }
    //protected void chkNACriteria_CheckedChanged(object sender, EventArgs e)
    //{
    //    if (chkNACriteria.Checked)
    //    {
    //        rbtnIoaReq.Enabled = false;
    //        //ddlTempColumn.Enabled = false;
    //        ddlTempMeasure.Enabled = false;
    //        txtRequiredScore.Enabled = false;
    //        rbtnMultitchr.Enabled = false;
    //        rbtnConsectiveSes.Enabled = false;
    //        txtNumbrSessions.Enabled = false;
    //        txtIns1.Enabled = false;
    //        txtIns2.Enabled = false;
    //    }
    //    else
    //    {
    //        rbtnIoaReq.Enabled = true;
    //        //ddlTempColumn.Enabled = true;
    //        ddlTempMeasure.Enabled = false;
    //        txtRequiredScore.Enabled = true;
    //        rbtnMultitchr.Enabled = true;
    //        rbtnConsectiveSes.Enabled = true;
    //        txtNumbrSessions.Enabled = true;
    //        txtIns1.Enabled = true;
    //        txtIns2.Enabled = true;

    //    }
    //}


    protected void LoadLPforSorting()
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        string query = "";

        if (sess != null)
        {

            FillLessonPlanName();
            FillLessonorder();

           

            //query = "select distinct LessonPlanId," +
            //      "(select top 1 DSTempHdrId from DSTempHdr hdr where StudentId=" + sess.StudentId + " and hdr.LessonPlanId=hdr2.LessonPlanId and StatusId in (select LookupId from LookUp where LookUpName <> 'Deleted' and LookupType='TemplateStatus' and LookUpName<>'Maintenance' and LookUpName<>'In Progress' and LookUpName<>'Inactive' and LookUpName='Approved') order by hdr.DSTempHdrId desc) DSTempHdrId," +
            //      "(select top 1 DSTemplateName from DSTempHdr hdr where StudentId=" + sess.StudentId + " and hdr.LessonPlanId=hdr2.LessonPlanId and StatusId in (select LookupId from LookUp where LookUpName <> 'Deleted' and LookupType='TemplateStatus' and LookUpName<>'Maintenance' and LookUpName<>'In Progress' and LookUpName<>'Inactive' and LookUpName='Approved') order by hdr.DSTempHdrId desc) DSTemplateName," +
            //      "(select top 1 LessonOrder from DSTempHdr hdr where StudentId=" + sess.StudentId + " and hdr.LessonPlanId=hdr2.LessonPlanId and StatusId in (select LookupId from LookUp where LookUpName <> 'Deleted' and LookupType='TemplateStatus' and LookUpName<>'Maintenance' and LookUpName<>'In Progress' and LookUpName<>'Inactive' and LookUpName='Approved') order by hdr.DSTempHdrId desc) LessonOrder," +
            //      "null as SNo" +
            //      " from DSTempHdr hdr2 left join LookUp lu on lu.LookupId=hdr2.StatusId" +
            //      " where StudentId=" + sess.StudentId + " and lu.LookupName <> 'Deleted' and LookUpName<>'Maintenance' and LookUpName<>'In Progress' and LookUpName<>'Inactive' and LookUpName='Approved'" +
            //      " order by LessonOrder";
            query = "select distinct LessonPlanId,DSTempHdrId,DSTemplateName ,LessonOrder,StatusId,null as SNo " +
                   "from DSTempHdr where StudentId=" + sess.StudentId + " and StatusId in " +
                   "(select LookupId from LookUp where LookupType='TemplateStatus' and LookUpName <> 'Deleted' and " +
                   "LookUpName<>'In Progress' and LookUpName<>'Inactive' and " +
                   "(LookUpName='Approved' or LookupName='Maintenance')) order by LessonOrder ";

            int MaintainId = Convert.ToInt32(objData.FetchValue("select LookupId from LookUp where LookUpName='Maintenance' and LookupType='TemplateStatus'"));
            
            DataTable dtLPforSorting = objData.ReturnDataTable(query, false);
            int i = 1;
            dtLPforSorting.Columns.Add("checkmaint", typeof(System.String));
            foreach (DataRow dr in dtLPforSorting.Rows)
            {
                if (Convert.ToInt16(dr["StatusId"]) == MaintainId)
                {
                    dr["checkmaint"] = "<font size='3px'><b>" + "*" + "</b></font>";
                }
                else
                {
                    
                    dr["checkmaint"] ="";
                }
                dr["SNo"] = i.ToString();
                i++;
            }
            dlLPforSorting.DataSource = dtLPforSorting;
            dlLPforSorting.DataBind();
        }
    }

    private void FillLessonPlanName()
    {
        DataTable dtLP = new DataTable();
        dtLP.Columns.Add("Id", typeof(string));
        dtLP.Columns.Add("Name", typeof(string));
        DataRow dr0 = dtLP.NewRow();
        dr0["Id"] = 0;
        dr0["Name"] = "--Select Lesson Plan--";
        dtLP.Rows.Add(dr0);
        clsLessons oLessons = new clsLessons();
        DataTable DTLesson = oLessons.getDSLessonPlans(sess.StudentId.ToString(), "LP.LessonPlanId AS Id, DTmp.DSTemplateName AS Name", "(LU.LookupName='Approved' OR LU.LookupName='Maintenance') AND (DTmp.DSMode IS NULL OR DTmp.DSMode='MAINTENANCE' OR DTmp.DSMode='')");
        if (DTLesson != null)
        {
            if (DTLesson.Rows.Count > 0)
            {

                foreach (DataRow drLessn in DTLesson.Rows)
                {
                    DataRow drr = dtLP.NewRow();
                    drr["Id"] = drLessn.ItemArray[0];
                    drr["Name"] = drLessn.ItemArray[1];
                    dtLP.Rows.Add(drr);
                }

            }
        }
        ddlLessonname.DataSource = dtLP;
        ddlLessonname.DataTextField = "Name";
        ddlLessonname.DataValueField = "Id";
        ddlLessonname.DataBind();
    }
    private void FillLessonorder()
    {
        DataTable dtLP = new DataTable();
        dtLP.Columns.Add("Id", typeof(string));
        dtLP.Columns.Add("Order", typeof(string));
        DataRow dr0 = dtLP.NewRow();
        dr0["Id"] = 0;
        dr0["Order"] = "--Select order--";
        dtLP.Rows.Add(dr0);
        clsLessons oLessons = new clsLessons();
        int count = 1;
        DataTable DTLesson = oLessons.getDSLessonPlans(sess.StudentId.ToString(), "LP.LessonPlanId AS Id, DTmp.DSTemplateName AS Name", "(LU.LookupName='Approved' OR LU.LookupName='Maintenance') AND (DTmp.DSMode IS NULL OR DTmp.DSMode='MAINTENANCE' OR DTmp.DSMode='')");
        if (DTLesson != null)
        {
            if (DTLesson.Rows.Count > 0)
            {
                
                foreach (DataRow drLessn in DTLesson.Rows)
                {
                    DataRow drr = dtLP.NewRow();
                    drr["Id"] = drLessn.ItemArray[0];
                    drr["Order"] = count;
                    dtLP.Rows.Add(drr);
                    count++;
                }

            }
        }
        ddlLessonorder.DataSource = dtLP;
        ddlLessonorder.DataTextField = "Order";
        ddlLessonorder.DataValueField = "Id";
        ddlLessonorder.DataBind();
    }    
    protected void btnSaveOrder_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        foreach (DataListItem dli in dlLPforSorting.Items)
        {
            HiddenField hf_LPId = (HiddenField)dli.FindControl("hfLPId");
            TextBox lblSno = (TextBox)dli.FindControl("lblSNo");
            Label lblLPName = (Label)dli.FindControl("lblLPs");
            string query = "update DSTempHdr set LessonOrder=" + lblSno.Text + " where LessonPlanId=" + hf_LPId.Value + " and StudentId=" + sess.StudentId;
            objData.Execute(query);
        }
        if (ddlLessonname.SelectedIndex != 0 && ddlLessonorder.SelectedIndex != 0)
        {
            int lpid = Convert.ToInt32(ddlLessonname.SelectedValue);
            int lporo = Convert.ToInt32(objData.FetchValue("select LessonOrder from DSTempHdr where  statusId in(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved') and LessonPlanId=" + Convert.ToInt32(ddlLessonname.SelectedValue)));
            int lporon=(Convert.ToInt32(ddlLessonorder.SelectedItem.Text))-1;
            int count = Convert.ToInt32(objData.FetchValue("select max(LessonOrder) from DSTempHdr where StudentId=" + sess.StudentId + " and statusId in(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved')"));
            string queryorder="";
            if(lporo>lporon)
            {
                queryorder = "update DSTempHdr set LessonOrder=LessonOrder+1 where LessonOrder >=" + lporon + " and LessonOrder <" + lporo + "  and StudentId=" + sess.StudentId;
                objData.Execute(queryorder);

                queryorder = "update DSTempHdr set LessonOrder=" + lporon + " where LessonPlanId=" +lpid + " and StudentId=" + sess.StudentId;
                objData.Execute(queryorder);              
               
            }
            else if(lporo<lporon)
            {      
                queryorder = "update DSTempHdr set LessonOrder=LessonOrder-1 where LessonOrder <=" + lporon + " and LessonOrder >" + lporo + " and StudentId=" + sess.StudentId;
                objData.Execute(queryorder);

                queryorder = "update DSTempHdr set LessonOrder=" + lporon + " where LessonPlanId=" +lpid + " and StudentId=" + sess.StudentId;
                objData.Execute(queryorder); 
            }
        }        
        LoadLPforSorting();
        LoadData();
    }
    protected void btnPriorLevel_Click(object sender, EventArgs e)
    {
        objData = new clsData();
        if (totalTaskOverride.Visible == true)
        {
            foreach (RepeaterItem ri in rptr_listStep.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    CheckBox checkBoxInRepeater = ri.FindControl("stepCheckBox") as CheckBox;

                    if (checkBoxInRepeater.Checked == true)
                    {
                        HiddenField stepId = ri.FindControl("hdn_stepId") as HiddenField;
                        Session["iCurrentStep"] = objData.FetchValue("select SortOrder from DSTempStep where DSTempStepId=" + stepId.Value).ToString();
                        DropDownList promptDDL = ri.FindControl("stepDDL") as DropDownList;

                        if (promptDDL.SelectedValue != "")
                        {
                            Session["sCurrentPrompt"] = objData.FetchValue("SELECT PromptId FROM DSTempPrompt WHERE DSTempPromptId=" + promptDDL.SelectedValue);
                        }
                    }


                }
            }
        }
        string hdnRadSet = (Session["iCurrentSetId"] != null) ? Convert.ToString(Session["iCurrentSetId"]) : "0";
        string hdnRadStep = (Session["iCurrentStep"] != null) ? Convert.ToString(Session["iCurrentStep"]) : "0";
        string hdnRadPrompt = (Session["sCurrentPrompt"] != null) ? Convert.ToString(Session["sCurrentPrompt"]) : "0";

        int curSetId = Convert.ToInt32(hdnRadSet);
        string qString = "select SortOrder from DSTempSet where DSTempSetId=" + hdnRadSet;
        int curSet = Convert.ToInt32(objData.FetchValue(qString));

        int TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        string UpdateNewDetails = "UPDATE DSTempHdr SET crntset=" + hdnRadSet + ",crntstep=" + hdnRadStep + ",crntprompt=" + hdnRadPrompt + ",nextsetno=" + curSet + " WHERE DSTempHdrId=" + TemplateId;
        objData.Execute(UpdateNewDetails);
        Session["iCurrentSetId"] = null;
        Session["iCurrentStep"] = null;
        Session["sCurrentPrompt"] = null;

        string sqlStrchk = "SELECT isnull(ModificationInd,2) from DSTempHdr WHERE  StudentId= " + sess.StudentId + " and StatusId=(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved') AND LessonPlanId = (SELECT  lessonplanid FROM DSTemphdr WHERE DSTemphdrid=" + TemplateId +")";
        int chk = Convert.ToInt32(objData.FetchValue(sqlStrchk));
        if (chk == 1)
        {
            string UpdateQuery = "UPDATE DSTempHdr SET Bannerstatus=0 WHERE  StudentId= " + sess.StudentId + " and StatusId=(SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved') AND LessonPlanId = (SELECT  lessonplanid FROM DSTemphdr WHERE DSTemphdrid=" + TemplateId + ")";
            objData.Execute(UpdateQuery);
        }

        BtnApproval_Click(sender, e);

    }
    protected void btnResetLevel_Click(object sender, EventArgs e)
    {
        BtnApproval_Click(sender, e);
    }

    protected void chkEnd_CheckedChanged(object sender, EventArgs e)
    {
        if (chkEnd.Checked == true)
        {
            ddlSortOrder.Enabled = false;
        }
        else
        {
            ddlSortOrder.Enabled = true;
        }
    }
    protected void drp_teachingFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
		int parentLookupId = Convert.ToInt16(drp_teachingFormat.SelectedValue);
		drpTeachingProc.Items.Clear();
		objData = new clsData();
		objData.ReturnDropDown("Select LookupId as Id , LookupName as Name from dbo.LookUp where ParentLookupId=" + parentLookupId, drpTeachingProc);
		hideAllOptions();
    }
	
    protected void BtnAdminPreview_Click(object sender, EventArgs e)             // Submit Template
    {
        //btndoc.Style.Add("display", "None");
        objData = new clsData();
        string hdrid = "";
        int TemplateId = 0;
        string setMatch = "";
        string matchSelctd = "";
        int length = 0;
        bool validSet = false;
        bool validStep = false;
        int setCriteria = 0;
        int stepCriteria = 0;
        object objTeach = null;
        string teachName = "";
        string message = "";
        string strQuerry = "";
        string skilltype = "";
        int teachId = 0;
        tdReadMsg.InnerHtml = "";
        bool IsVTValid = true;
        bool validonSub = true;
        bool previewSuccess = true;
        DataTable dt = new DataTable();
        ObjTempSess = (ClsTemplateSession)Session["BiweeklySession"];
        sess = (clsSession)Session["UserSession"];
        // int noOfTrial = Convert.ToInt32(txtNoofTrail.Text);


        if (ViewState["HeaderId"] != null)
        {
            TemplateId = Convert.ToInt32(ViewState["HeaderId"]);
        }

        string qString = "select DSTempSetId from DSTempSet where DSTempHdrId=" + TemplateId + " and ActiveInd='A'";        //get set ids
        DataTable dtSetId = objData.ReturnDataTable(qString, false);

        objData = new clsData();

        // FUnction to asign sets and steps If the lesson plan is a visual lesson 

        IsVTValid = FunInsrtSetStepVT();

        if (IsVTValid == true)
        {
            try
            {
                //string skilltype = Convert.ToString(objData.FetchValue("SELECT SkillType FROM [dbo].[DSTempHdr] WHERE StudentId='" + sess.StudentId + "' and DSTempHdrId='" + TemplateId + "'"));
                strQuerry = "SELECT SkillType,TeachingProcId FROM [dbo].[DSTempHdr] WHERE DSTempHdrId=" + TemplateId;
                DataTable dtNew = objData.ReturnDataTable(strQuerry, false);
                if (dtNew != null)
                {
                    if (dtNew.Rows.Count > 0)
                    {
                        skilltype = dtNew.Rows[0]["SkillType"].ToString();
                        try
                        {
                            teachId = Convert.ToInt32(dtNew.Rows[0]["TeachingProcId"]);
                        }
                        catch
                        {
                            teachId = 0;
                        }
                        if (teachId > 0)
                        {
                            try
                            {
                                strQuerry = "SELECT LookupDesc FROM LookUp WHERE LookupId = " + teachId;
                                objTeach = objData.FetchValue(strQuerry);
                                if (objTeach != null)
                                {
                                    teachName = objTeach.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                previewSuccess = false;
                                throw ex;
                            }
                        }
                    }
                }

                if (skilltype == "Chained")
                {
                    //hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));

                    hdrid = Convert.ToString(objData.FetchValue("SELECT  DSTempHdrId FROM [dbo].[DSTempStep] WHERE ActiveInd = 'A' AND IsDynamic=0 INTERSECT SELECT DSTempHdrId FROM  [dbo].[DSTempSetCol] WHERE DSTempHdrId =" + TemplateId + ""));
                    if (hdrid != "")
                    {
                        dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId " +
                                          "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId " +
                                          "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +
                                            "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                    }
                }
                else if (skilltype == "Discrete")
                {
                    hdrid = Convert.ToString(objData.FetchValue("SELECT DSTempHdrId FROM [dbo].[DSTempSet]  WHERE ActiveInd = 'A' INTERSECT SELECT DSTempHdrId FROM [dbo].[DSTempSetCol] WHERE DSTempHdrId='" + TemplateId + "'"));
                    if (hdrid != "")
                    {
                        dt = objData.ReturnDataTable("SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='SET' AND ActiveInd = 'A' AND  DSTempSetColId IN (SELECT DSTempSetColId " +
                                                      "FROM DSTempSetCol WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SET1 FROM [dbo].[DSTempSetCol] col INNER JOIN [dbo].[DSTempRule] rul on " +

                                                               "col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "", false);
                    }
                }
                else
                {
                    previewSuccess = false;
                    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. Skill type is missing");
                    FillTypeOfInstruction(TemplateId);
                    return;
                }
                if (hdrid != "")
                {
                    if (dt != null)
                    {

                        int colcnt = dt.Columns.Count;  // colcnt=2 for Discrete and colcnt=3 for chained
                        if (colcnt > 0)
                        {
                            if (colcnt == 3)
                            {
                                if (teachName == "Total Task")
                                {
                                    if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                    {
                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {
                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                        if (setCriteria == 0)
                                        {
                                            previewSuccess = false;
                                            message = "Set criteria details are missing";
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");
                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview..Undefined error.");
                                        }

                                    }
                                }
                                else
                                {
                                    if (Convert.ToInt32(dt.Rows[0]["STEP"]) > 0)
                                    {
                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] WHERE RuleType='STEP' AND CriteriaType='MOVE UP' " +
                                                        "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='STEP'  AND CriteriaType='MOVE DOWN' " +
                                                        "AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS STEPDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["STEPUP"].ToString() == "0")
                                                {
                                                    msg += "Step Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["STEPDOWN"].ToString() == "0")
                                                {
                                                    msg += "Step Movedown ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {
                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        //  setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                        stepCriteria = Convert.ToInt32(dt.Rows[0]["STEP"]);
                                        //if (setCriteria == 0)
                                        //{
                                        //    message = "Set criteria details are missing";
                                        //    tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before submitting.." + message + "");
                                        //}
                                        if (stepCriteria == 0)
                                        {
                                            previewSuccess = false;
                                            message = "Step criteria details are missing";
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + message + "");
                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");
                                        }
                                    }
                                }
                            }
                            else if (colcnt == 2)
                            {
                                if (Convert.ToInt32(dt.Rows[0]["SET1"]) > 0)
                                {
                                    validonSub = WriteVTData();

                                    if (validonSub == true)
                                    {

                                        string msg = "";
                                        bool flag = true;
                                        string strQry = "SELECT COUNT(rul.DSTempHdrId),(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='SET' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS SETDOWN,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE UP' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTUP,(SELECT COUNT(RuleType) FROM [dbo].[DSTempRule] " +
                                                        "WHERE RuleType='PROMPT' AND CriteriaType='MOVE DOWN' AND ActiveInd = 'A' AND DSTempSetColId IN (SELECT  DSTempSetColId FROM DSTempSetCol " +
                                                        "WHERE DSTempHdrId=" + hdrid + " AND ActiveInd='A')) AS PROMPTDOWN FROM [dbo].[DSTempSetCol] col " +
                                                        "INNER JOIN [dbo].[DSTempRule] rul on col.DSTempSetColId=rul.DSTempSetColId WHERE col.DSTempHdrId=" + hdrid + "";
                                        DataTable dtCheck = objData.ReturnDataTable(strQry, false);
                                        if (dtCheck != null)
                                        {
                                            if (dtCheck.Rows.Count > 0)
                                            {
                                                if (dtCheck.Rows[0]["SETUP"].ToString() == "0")
                                                {
                                                    msg += "Set Moveup ,";
                                                    flag = false;
                                                }
                                                if (dtCheck.Rows[0]["SETDOWN"].ToString() == "0")
                                                {
                                                    msg += "Set Movedown ,";
                                                    flag = false;
                                                }
                                                string PrmptPrcdur = ddlPromptProcedure.SelectedValue;
                                                int SelecItms = lstSelectedPrompts.Items.Count;
                                                if ((PrmptPrcdur != "141") && (SelecItms > 0))
                                                {
                                                    string getPromptCriteria = "SELECT COUNT(DSTempPromptId) FROM DSTempPrompt WHERE DSTempHdrId = " + hdrid + "";
                                                    DataTable dtPromptCheck = objData.ReturnDataTable(getPromptCriteria, false);
                                                    if (dtPromptCheck != null)
                                                    {
                                                        if (dtPromptCheck.Rows.Count > 0)
                                                        {
                                                            if (dtCheck.Rows[0]["PROMPTUP"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Moveup ,";
                                                                flag = false;
                                                            }
                                                            if (dtCheck.Rows[0]["PROMPTDOWN"].ToString() == "0")
                                                            {
                                                                msg += "Prompt Movedown ,";
                                                                flag = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (msg.Length > 0) msg = msg.Substring(0, msg.Length - 1);
                                            }
                                        }

                                        if (flag)
                                        {


                                            //PENDINGAPPROVE(TemplateId);

                                            //LoadTemplateData(TemplateId);

                                            ////   GetStatus(TemplateId);
                                            //FillData();                                      // Fill the goal type assigned for the student
                                            //FillApprovedLessonData();
                                            //FillCompltdLessonPlans();
                                            //FillRejectedLessons();
                                            //setApprovePermission();

                                        }
                                        else
                                        {
                                            previewSuccess = false;
                                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview. " + msg + " criterias are missing");
                                        }
                                    }
                                    else
                                    {
                                        previewSuccess = false;
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Sorry..Lesson plan submitting failed..Please try again ");
                                    }
                                }
                                else
                                {
                                    setCriteria = Convert.ToInt32(dt.Rows[0]["SET1"]);
                                    if (setCriteria == 0)
                                    {
                                        previewSuccess = false;
                                        message = "Set criteria details are missing";
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");
                                    }
                                    else
                                    {
                                        previewSuccess = false;
                                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");
                                    }

                                }

                                if (IsMatchToSample(TemplateId) == true)        //if match to sample
                                {
                                    if (matchSelctd != null)
                                    {

                                        string setId = "";
                                        for (int i = 0; i < dtSetId.Rows.Count; i++)
                                        {
                                            setId = dtSetId.Rows[i]["DSTempSetId"].ToString();
                                            clsMathToSamples.Step[] steps = null;
                                            int setIdint = Convert.ToInt32(setId);
                                            lstMatchSamples.Items.Clear();
                                            matchSelctd = "";
                                            setMatch = "";
                                            EditSetData(setIdint);
                                            string[] arryMatchValue = new string[lstMatchSamples.Items.Count];
                                            if (lstMatchSamples.Items.Count > 0)
                                            {
                                                for (int index = 0; index < lstMatchSamples.Items.Count; index++)
                                                {
                                                    arryMatchValue[index] = lstMatchSamples.Items[index].Value.ToString();
                                                }
                                                for (int arryInt = 0; arryInt < lstMatchSamples.Items.Count; arryInt++)
                                                {
                                                    setMatch += arryMatchValue[arryInt].ToString() + ",";
                                                }
                                                length = setMatch.Length;
                                                matchSelctd = setMatch.ToString().Substring(0, length - 1);
                                                steps = MatchSampDef2(TemplateId, matchSelctd, setIdint);      //get steps
                                            }
                                            //if steps are null previewsuccess false
                                            if (steps == null)
                                            {
                                                previewSuccess = false;
                                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Samples for some sets are missing.");
                                            }
                                            ClearSetData();
                                            BtnUpdateSetDetails.Visible = false;
                                            btnAddSetDetails.Visible = true;
                                        }

                                    }
                                }
                                //-----
                            }
                            else
                            {
                                previewSuccess = false;
                                tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                            }
                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                        }

                    }
                    else
                    {
                        previewSuccess = false;
                        tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.");

                    }
                }
                else
                {
                    validSet = SetValidation(TemplateId);
                    validStep = StepValidation(TemplateId);

                    if (skilltype == "Chained")
                    {
                        if (validStep == false)
                        {
                            previewSuccess = false;
                            message = "Steps are missing.";
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview.." + message + "");

                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");

                        }
                    }
                    else
                    {
                        if (validSet == false)
                        {
                            previewSuccess = false;
                            message = "Sets are missing.";
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview..." + message + "");

                        }
                        else
                        {
                            previewSuccess = false;
                            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");
                        }
                    }

                }

                if (previewSuccess == true)
                {
                    //-----
                    Session["tempOverrideHT"] = null;
                    string sel = "SELECT StdtSessionHdrId FROM StdtSessionHdr WHERE StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND StdtClassId=" + sess.Classid + " AND DSTempHdrId=" + TemplateId + " AND SessionStatusCd='P'";
                    DataTable dtHdrs = objData.ReturnDataTable(sel, false);
                    ViewState["StdtSessHdr"] = null;
                    if (dtHdrs.Rows.Count > 0)
                    {
                        ViewState["StdtSessHdr"] = dtHdrs.Rows[0]["StdtSessionHdrId"].ToString();
                    }

                    if (ViewState["StdtSessHdr"] != null)
                    {
                        string updQry = "DELETE FROM StdtSessionHdr WHERE StdtSessionHdrId=" + ViewState["StdtSessHdr"];
                        int retrn = objData.Execute(updQry);
                        string selqry = "SELECT StdtDSStatId FROM StdtDSStat WHERE StudentId=" + sess.StudentId + " AND SchoolId=" + sess.SchoolId + " AND DSTempHdrId=" + TemplateId + "";
                        DataTable dtDSStat = objData.ReturnDataTable(selqry, false);

                        //Code for Not Deleting data from Stdtdsstat Preview Issue in Maintenance [16-jul-2020] - Dev 2
                        int MaintStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Maintenance' "));
                        int AprovdStatusId = Convert.ToInt16(objData.FetchValue("SELECT  LookupId FROM LookUp WHERE LookupType='TemplateStatus' And LookupName='Approved' "));
                        int LsnStatusId = Convert.ToInt16(objData.FetchValue("select statusid from DSTempHdr where DSTempHdrId =" + TemplateId + " "));
                        if ((MaintStatusId != LsnStatusId) && (AprovdStatusId != LsnStatusId))
                        {
                            if (dtDSStat.Rows.Count > 0)
                            {
                                string dltQry = "DELETE FROM StdtDSStat WHERE DSTempHdrId=" + TemplateId + " ";
                                int dtlRetrn = objData.Execute(dltQry);
                            }
                        }
                        //Code for Not Deleting data from Stdtdsstat Preview Issue in Maintenance [16-jul-2020] - Dev 2
                    }
                }


            }
            catch (Exception Ex)
            {
                previewSuccess = false;
                throw Ex;
            }
        }
        else
        {
            previewSuccess = false;
            tdReadMsg.InnerHtml = clsGeneral.warningMsg("Please complete template details before preview");

        }
        drpTasklist_SelectedIndexChanged1(sender, e);
        if (previewSuccess)
        {
            //  ObjTempSess.TemplateId = TemplateId;
            UpdatePrompt(TemplateId);
            Session["NewTemplateId"] = TemplateId;
            ClientScript.RegisterStartupScript(this.GetType(), "", "LoadDatasheetView();", true);
            // Page.ClientScript.RegisterStartupScript(this.GetType(), "myScript", "LoadDatasheetView();", true);
        }
    }

    public bool CopyCheck()
    {
        bool enableCopy = false;
        objData = new clsData();
        sess = (clsSession)Session["UserSession"];
        
        string strSql = " SELECT RGP.ReadInd,RGP.WriteInd FROM RoleGroupPerm RGP  INNER JOIN UserRoleGroup URG ON RGP.RoleGroupId = URG.RoleGroupId INNER JOIN  [Object] O ";
        strSql += "ON RGP.ObjectId = O.ObjectId WHERE    URG.SchoolId =  " + sess.SchoolId + " AND  URG.UserId = " + sess.LoginId + "  and O.ObjectName='CLP: Copy Lesson Plan As Template'";
       
        DataTable Dt = objData.ReturnDataTable(strSql, false);
        Boolean Read = false;
        Boolean Write = false;
        try
        {
            if (Dt != null)
            {
                if (Dt.Rows.Count > 0)
                {
                    foreach (DataRow Dr in Dt.Rows)
                    {
                        Read = Convert.ToBoolean(Dr["ReadInd"]);
                        Write = Convert.ToBoolean(Dr["WriteInd"]);

                        if (Write == true) break;
                    }
                }
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }

        if (Write == true)
        {
            enableCopy = true;
        }
        else
        {
            enableCopy = false;
        }

        return enableCopy;
    }

    protected void removeTypeOfInstructions(object sender)
    {
        try
        {
            txtNoofTrail.Text = "";
            chkTotalRandom.Checked = false;
            drpTeachingProc.SelectedIndex = 0;
            drp_teachingFormat.SelectedIndex = 0;
            rbtnMatchToSampleExpressive.SelectedValue = "0";
        }
        catch (Exception Ex)
        {
        }
    }

}
