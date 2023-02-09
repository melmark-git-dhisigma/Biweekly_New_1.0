<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BiweeklyBehaviorGraph.aspx.cs" Inherits="StudentBinder_BiweeklyBehaviorGraph" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10,9" />
    <link href="../Administration/CSS/MenuStyle.css" rel="stylesheet" />
    <link href="../Administration/CSS/buttons.css" rel="stylesheet" />
    <link href="../Administration/CSS/jsDatePickforGraph.css" rel="stylesheet" />
    <script src="../Administration/JS/jsDatePick.min.1.3.js"></script>
    <script src="../Administration/JS/jquery-1.8.0.js"></script>
    <script src="../Administration/JS/jquery.min.js"></script>
    <script src="../Administration/JS/jquery-ui-1.8.19.custom.min.js"></script>



    <style type="text/css">
        .divGrid {
            border-radius: 14px;
            -moz-border-radius: 14px;
            -webkit-border-radius: 14px;
            border: 5px solid #62BDF6;
            width: auto;
            height: auto;
        }

        .divGrid1 {
            border-radius: 14px;
            -moz-border-radius: 14px;
            -webkit-border-radius: 14px;
            border: 5px solid #62BDF6;
            width: auto;
            height: auto;
            display: none;
        }

        .divBackgrnd {
            padding: 26px 16px 16px 16px;
            width: 90%;
            height: 400px;
            overflow-y: scroll;
            overflow-x: hidden;
            -webkit-border-radius: 24px 24px 24px 24px;
            -moz-border-radius: 24px 24px 24px 24px;
            border-radius: 24px 24px 24px 24px;
            background: rgba(87,197,239,0.2);
            -webkit-box-shadow: #68A1B3 8px 8px 8px;
            -moz-box-shadow: #68A1B3 8px 8px 8px;
            box-shadow: #68A1B3 8px 8px 8px;
        }

        .pnlCSS {
        }
        /* FOR LOADING IMAGE AT PAGE LOAD */
        .loading {
            display: block;
            position: absolute;
            width: 100%;
            height: 800px;
            top: 0px;
            left: 0px;
            z-index: 1000;
            background-image: url("../Administration/images/overlay.png");
            /*background: repeat-x scroll center top transparent;*/
        }

        #fullContents {
            display: none;
        }

        .innerLoading {
            margin: auto;
            height: 50px;
            width: 250px;
            text-align: center;
            font-weight: bold;
            font-size: large;
        }

            .innerLoading img {
                margin-top: 200px;
                height: 10px;
                width: 100px;
            }
        /*LOADING IMAGE CLOSE */
        #RV_Behavior img
        {
            width:100% !important;
            height:100% !important;
        }

           .web_dialog {
            background: url("../images/smalllgomlmark.JPG") no-repeat scroll right bottom #F8F7FC;
            border: 5px solid #B2CCCA;
            color: #333333;
            display: block;
            font-family: Arial,Helvetica,sans-serif;
            font-size: 100%;
            height: auto;
            left: 30%;
            top: 5%;
            margin-left: -190px;
            padding: 5px 5px 30px;
            position: fixed;
            display: none;            
            width: 800px;
            z-index: 102;
        }


        .web_dialog_overlay {
            background: none repeat scroll 0 0 #000000;
            bottom: 0;
            display: none;
            height: 100%;
            left: 0;
            margin: 0;
            opacity: 0.15;
            padding: 0;
            position: fixed;
            right: 0;
            top: 0;
            width: 100%;
            z-index: 101;
        }
    </style>
    <script type="text/javascript">
        //$(window).load(function () {

        //});
        $(document).ready(function () {
            $('.loading').fadeOut('slow', function () {
                $('#fullContents').fadeIn('fast');
            });


            $("#chkbxevents").toggle(
function () {
    if ($('#<%=chkbxevents.ClientID %>').is(':checked')) {
               $('#<%=chkbxmajor.ClientID %>').attr('checked', true);
               $('#<%=chkbxminor.ClientID %>').attr('checked', true);
               $('#<%=chkbxarrow.ClientID %>').attr('checked', true);

           }
           else {
               $('#divpopupEvents').slideToggle("slow");
           }
});


            $("#chkbxevents").change(function () {
                if (this.checked) {
                    $('#divpopupEvents').slideToggle("slow");
                    $('#<%=chkbxmajor.ClientID %>').attr('checked', true);
                    $('#<%=chkbxminor.ClientID %>').attr('checked', true);
                    $('#<%=chkbxarrow.ClientID %>').attr('checked', true);
                }
                else {
                    $('#divpopupEvents').slideToggle("slow");
                    $('#<%=chkbxmajor.ClientID %>').attr('checked', false);
                    $('#<%=chkbxminor.ClientID %>').attr('checked', false);
                    $('#<%=chkbxarrow.ClientID %>').attr('checked', false);
                }
            });

            $('#<%=btnExport.ClientID %>').click(function () {
                $('.loading').fadeIn('fast');
            });

        });
        function LoadGraph() {
            $('.loading').fadeOut('slow', function () {
                $('#fullContents').fadeIn('fast');
            });
        }
        function loadWait() {
            $('.loading').fadeIn('fast');//, function () { });
        }
        function HideWait() {
            $('.loading').fadeOut('fast');//, function () { });
        }

        function printTrigger(elementsrc) {
            window.document.forms[0].target = '_blank'; setTimeout(function () { window.document.forms[0].target = ''; }, 500);
        }

        function eventinpopup() {
            $('#<%=chkbxevents.ClientID %>').attr('checked', false);

             if ($('#<%=chkbxmajor.ClientID %>').is(':checked') && $('#<%=chkbxminor.ClientID %>').is(':checked') && $('#<%=chkbxarrow.ClientID %>').is(':checked')) {
                 $('#<%=chkbxevents.ClientID %>').attr('checked', true);
            }
        }

        $(function () {

            adjustStyle();
            $(window).resize(function () {
                adjustStyle();
            });
        });


        function adjustStyle() {
            var isiPad = navigator.userAgent.match(/iPad/i);

            if (isiPad != null) {


                $('#RV_LPReport_ctl09').css("overflow", 'scroll');

            }
            else {


            }

        }

    </script>

    <script type="text/javascript">
        window.onload = function () {

            var valExport = document.getElementById("hdnExport").value;
            new JsDatePick({
                useMode: 2,
                target: "<%=txtSdate.ClientID%>",
                dateFormat: "%m/%d/%Y",
            });
            new JsDatePick({
                useMode: 2,
                target: "<%=txtEdate.ClientID%>",
                dateFormat: "%m/%d/%Y",
            });

            if (valExport == "true") {
                $('#overlay').show();
            }
        };

        function DownloadPopup() {
            $('.loading').fadeOut('fast');
            $('#overlay').fadeIn('fast', function () {
                $('#downloadPopup').fadeIn('fast');
            });
        }

        function CloseDownload() {
            document.getElementById("hdnExport").value = "";
            $('#overlay').fadeOut('fast', function () {
                $('#downloadPopup').fadeOut('fast');
            });
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="loading">
            <div class="innerLoading">
                <img src="../Administration/images/load.gif" alt="loading" />
                Please Wait...
            </div>
        </div>
        <div id="fullContents" style="width: 98%; margin-left: 1%">
            <div>
                <table style="width: 100%">

                    <tr>
                        <td colspan="4" runat="server" id="tdMsg"></td>
                    </tr>
                    <tr>
                        <td style="text-align: center"></td>
                        <td style="text-align: center">


                            <asp:HiddenField ID="hdnallLesson" runat="server" />


                        </td>
                        <td style="text-align: center">


                            <asp:ScriptManager ID="ScriptManager1" runat="server">
                            </asp:ScriptManager>
                        </td>
                        <td></td>
                    </tr>

                </table>
                <p>
                </p>
                <table style="width: 100%">

                    <tr>
                        <td style="text-align: center" class="tdText">Report
                Start Date
                            <br />
                            <br />
                            <br />
                            Report End Date
                            <br />
                            <br />
                            <br />
                            Select Behavior
                        </td>
                        <td style="text-align: left;width:25%">
                            <asp:TextBox ID="txtSdate" runat="server" class="textClass" onkeypress="return false"></asp:TextBox>
                            <br />
                            <br />
                            <asp:TextBox ID="txtEdate" runat="server" class="textClass" onkeypress="return false"></asp:TextBox>
                            <br />
                            <br />
                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                            <ContentTemplate>
                               <asp:CheckBox ID="CheckBox1" runat="server" OnCheckedChanged="CheckBox1_CheckedChanged" Text="Active" Checked="True" AutoPostBack="true"/>
                               <asp:CheckBox ID="CheckBox2" runat="server" OnCheckedChanged="CheckBox2_CheckedChanged" Text="Inactive"  Checked="False" AutoPostBack="true"/> 
                               <asp:DropDownList ID="ddlBehavior" runat="server" CssClass="drpClass"></asp:DropDownList>
                            </ContentTemplate>
                            </asp:UpdatePanel>
                        </td>
                        <td style="text-align: left;line-height:0px" class="tdText" rowspan="2" >
                            <asp:RadioButtonList ID="rbtnIncidentalRegular" runat="server" RepeatDirection="Horizontal"  >
                            <asp:ListItem Value="Incidental">Incidental Teaching Graph</asp:ListItem>
                            <asp:ListItem Value="Regular" Selected="True">Regular Graph</asp:ListItem>
                        </asp:RadioButtonList>
                             <asp:RadioButtonList ID="rbtnClassType" runat="server" RepeatDirection="Horizontal" Visible="true" >
                            <asp:ListItem Value="Day">Day</asp:ListItem>
                            <asp:ListItem Value="Residence">Residence</asp:ListItem>
                            <asp:ListItem Value="Day,Residence" Selected="True">Both</asp:ListItem>
                        </asp:RadioButtonList>
                            <br/>
                            <asp:CheckBox ID="chkbxevents" runat="server" Text="Display Events"></asp:CheckBox><br />
                             <div id="divpopupEvents" style="display:none">
                                    <asp:checkbox id="chkbxmajor" runat="server" text="Display Major Events" onclick="eventinpopup();" ></asp:checkbox>
                                    <br /> 
                                    <asp:checkbox id="chkbxminor" runat="server" text="Display Minor Events" onclick="eventinpopup();" ></asp:checkbox>
                                    <br />
                                    <asp:checkbox id="chkbxarrow" runat="server" text="Display Arrow Notes" onclick="eventinpopup();" ></asp:checkbox>
                                    <br />
                                </div>
                            <asp:CheckBox ID="chkbxIOA" runat="server" Text="Include IOA"></asp:CheckBox><br />
                            <asp:CheckBox ID="chkreptrend" runat="server" Text="Include Trend Line"></asp:CheckBox>
                            <br />
                            <asp:CheckBox ID="chkmedication" runat="server" Text="Include Medication"></asp:CheckBox><br />
                            <asp:CheckBox ID="chkrate" runat="server" Text="Include Rate Graph" ></asp:CheckBox>
                           
                            
                        </td>
                        <td style="text-align: right" rowspan="2">
                            <asp:Button ID="btnsubmit" Style="float: right; margin: 0 1px 0 1px" runat="server" ToolTip="Show Graph" CssClass="showgraph" OnClick="btnsubmit_Click" />
                            <asp:Button ID="btnExport" Style="float: right; margin: 0 1px 0 1px" runat="server" ToolTip="Export To PDF" CssClass="pdfPrint" OnClick="btnExport_Click" />
                            <asp:Button ID="btnPrint" Style="float: right; margin: 0 1px 0 1px" runat="server" ToolTip="Print" CssClass="print" OnClick="btnPrint_Click" Visible="false" />
                        </td>
                        <td style="text-align: left;width:10%" rowspan="2"></td>
                    </tr>

                    <tr>
                        <td style="text-align: center" class="tdText">
                &nbsp;</td>
                        <td style="text-align: left">
                            

                        </td>
                    </tr>

                    <tr>
                        <td style="text-align: center" class="tdText">&nbsp;&nbsp; &nbsp;</td>
                        <td style="text-align: left">
                           </td>
                    </tr>
                    
                </table>
            </div>
            <div>
                <table style="width:100%">
                    <tr>
                        
                        <td style="width:10%"></td>
                    </tr>
                </table>
            </div>
            <div>
                <table style="width: 100%">
                    
                    <tr>
                        <td style="text-align: center">
                            <div id="prdiv" style ="overflow:visible ">
                                <rsweb:ReportViewer ID="RV_Behavior" runat="server" ProcessingMode="Remote" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" ShowBackButton="false" ShowCredentialPrompts="false" ShowDocumentMapButton="true" ShowExportControls="false" ShowFindControls="false" ShowPageNavigationControls="true" ShowParameterPrompts="true" ShowPrintButton="false" ShowPromptAreaButton="true" ShowRefreshButton="false" ShowToolBar="true" ShowWaitControlCancelLink="true" ShowZoomControl="false" SizeToReportContent="true" Width="100%" Height="100" ZoomMode ="FullPage" AsyncRendering="true">

                                    <ServerReport ReportServerUrl="<%$ appSettings:ReportUrl %>" />

                                </rsweb:ReportViewer>
                              //  <script lang ="javascript" type ="text/javascript">
                                  //  ResizeReport();
                                  //  function ResizeReport() {
                                    //    var viewer = document.getElementById("<%=RV_Behavior.ClientID%>");
                                    //    var htmlheight = document.documentElement.clientHeight;
                                     //   viewer.style.height = (htmlheight - 30) + "px";
                                  //  }
                                   // window.onresize = function resize() { ResizeReport(); }
                                //</script>
                            </div>
                        </td>
                    </tr>
                </table>

            </div>

              <div id="overlay" class="web_dialog_overlay">
                </div>
             <div id="downloadPopup" class="web_dialog" style="width: 600px; ">

                            <div id="Div53" style="width: 700px;">


                                <table style="width: 97%">
                                    <tr>
                                        <td colspan="2">
                                            <table style="width: 85%">
                                                <tr>
                                                    <td runat="server" id="tdMsgExport" style="height: 50px"></td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="text-align: right">

                                            <asp:Button ID="btnDownload" runat="server" Text="Download" CssClass="NFButton" OnClick="btnDownload_Click" onclientclick="HideWait();" />

                                        </td>
                                        <td style="text-align: left">
                                             <input type="button" value="Done" class="NFButton" id="btnDone" onclick="CloseDownload();" />
                                          <%--  <asp:Button ID="btnDone" runat="server" Text="Done" CssClass="NFButton"  onclientclick="CloseDownload();" />--%>

                                        </td>
                                    </tr>
                                </table>

                            </div>
                        </div>
        </div>

        <asp:hiddenfield id="hdnExport" runat="server" value="" />

        <iframe id="iFramePdf" src="" style="display: none"></iframe>
    </form>
</body>
</html>
