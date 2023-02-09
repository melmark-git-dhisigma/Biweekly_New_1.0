using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text.RegularExpressions;


/// <summary>
/// Summary description for ClsAccSheetExport
/// </summary>
public class ClsAccSheetExport
{
    clsData objData = null;
    DataTable Dt = null;
    string strQuery = "";
    string[] IEPC = null;
    string[] IEPP = null;
    clsSession sess = null;

    string[] Common = null;
    string[] aC = null;
    string[] aP = null;
    int Count = 0, objcnt = 0, RCount = 0;

	public ClsAccSheetExport()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public DataTable getAccSheet(int StudentId, int SchoolId,string dateToLoad) //Query to Export the Academic Coversheet
    {
        string strtDate = "";
        string endDate = "";
        try
        {

            if (dateToLoad != null && dateToLoad != "")
            {
                strtDate = dateToLoad.Split('-')[0];
                endDate = dateToLoad.Split('-')[1];
            }
            string StdtIEPTable = "StdtIEP";
            if (SchoolId == 2)
            {
                StdtIEPTable = "StdtIEP_PE";
            }
            objData = new clsData();
            Dt = new DataTable();
            strQuery = "select Student.StudentFname+' '+Student.StudentLname as 'Name'," +
            "(Select TOP 1 'From:'+CONVERT(VARCHAR, SDIEP.EffStartDate,101)+' To :'+CONVERT(VARCHAR,SDIEP.EffEndDate,101)  from " + StdtIEPTable + " SDIEP where StudentId=" + StudentId + "  AND StatusId In (select LookupId from LookUp where LookupName!='Not Started'  And  LookupName!='Pending Approval'   And  LookupName!='Rejected'  AND LookupType='IEP Status')) AS  'IepDates'," +
            "StdtAcdSheet.DateOfMeeting," +
            "StdtAcdSheet.GoalArea,StdtAcdSheet.Goal,StdtAcdSheet.Benchmarks,StdtAcdSheet.Period1,StdtAcdSheet.Period2,StdtAcdSheet.Period3,StdtAcdSheet.Period4," +
            "StdtAcdSheet.Period5,StdtAcdSheet.Period6,StdtAcdSheet.Period7,StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.TypeOfInstruction," +
            "StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.TypeOfInstruction,StdtAcdSheet.Set1," +
            "StdtAcdSheet.Set2,StdtAcdSheet.Set3,StdtAcdSheet.Set4,StdtAcdSheet.Set5,StdtAcdSheet.Set6,StdtAcdSheet.Set7,StdtAcdSheet.Prompt1,StdtAcdSheet.Prompt2," +
            "StdtAcdSheet.Prompt3,StdtAcdSheet.Prompt4,StdtAcdSheet.Prompt5,StdtAcdSheet.Prompt6,StdtAcdSheet.Prompt7,StdtAcdSheet.IOA1,StdtAcdSheet.IOA2," +
            "StdtAcdSheet.IOA3,StdtAcdSheet.IOA4,StdtAcdSheet.IOA5,StdtAcdSheet.IOA6,StdtAcdSheet.IOA7,StdtAcdSheet.FeedBack,StdtAcdSheet.PreposalDiss," +
            "StdtAcdSheet.PersonResNdDeadline,StdtAcdSheet.NoOfTimes1,StdtAcdSheet.NoOfTimes2,StdtAcdSheet.NoOfTimes3,StdtAcdSheet.NoOfTimes4,StdtAcdSheet.NoOfTimes5,StdtAcdSheet.NoOfTimes6,StdtAcdSheet.NoOfTimes7,StdtAcdSheet.LessonPlanId,(SELECT TOP 1 LessonOrder FROM DSTempHdr WHERE DSTempHdr.LessonPlanId=StdtAcdSheet.LessonPlanId AND DSTempHdr.StudentId="+StudentId+") LessonOrder " +
            "from StdtAcdSheet inner join Student on StdtAcdSheet.StudentId=Student.StudentId where StdtAcdSheet.StudentId=" + StudentId + "" +
            "and CONVERT(datetime,DateOfMeeting)=CONVERT(datetime,'" + strtDate + "') and CONVERT(datetime,EndDate)=CONVERT(datetime,'" + endDate + "') ORDER BY LessonOrder";
            Dt = objData.ReturnDataTable(strQuery, false);
            //foreach (DataRow dr in Dt.Rows)      /// To Convert DB string to Html
            //{
            //    if (dr["Benchmarks"] != null && dr["Benchmarks"] != "")
            //    {
            //        dr["Benchmarks"] = StripHTML(dr["Benchmarks"].ToString()).Replace("&nbsp;"," ");  //Remove all html tags
            //    }
            //}
           
        }
        catch (Exception Ex)
        {
            ClsErrorLog errlog = new ClsErrorLog();
            errlog.WriteToLog("Page Name: "+clsGeneral.getPageName()+"\n"+ Ex.ToString());
        }
        return Dt;
    }
    public static string StripHTML(string input)
    {
        return Regex.Replace(input, "<.*?>", String.Empty);
    }
  

}