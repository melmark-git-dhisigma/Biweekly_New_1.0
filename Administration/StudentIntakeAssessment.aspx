<%@ Page Title="" Language="C#" MasterPageFile="~/Administration/AdminMaster.master" AutoEventWireup="true"
    CodeFile="StudentIntakeAssessment.aspx.cs" Inherits="StudentIntakeAssessment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1" />
    <title></title>
    <link href="CSS/tabmenu.css" rel="stylesheet" type="text/css" />
    <script src="JS/tabber.js" type="text/javascript"></script>
     <script src="../StudentBinder/jsScripts/jq1.js" type="text/javascript" charset="utf-8"></script>
    <script type="text/javascript" src="../StudentBinder/jsScripts/eye.js"></script>
    <script type="text/javascript" src="../StudentBinder/jsScripts/layout.js"></script>
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            font-family: Calibri;
            color: Black;
            line-height: 22px;
            font-weight: bold;
            font-size: 13px;
            padding-right: 1px;
            text-align: right;
            width: 50%;
        }

        .auto-style2 {
            font-family: Arial;
            color: #666;
            padding-right: 1px;
            text-align: left;
            width: 7%;
        }
        .auto-style3 {
            font-family: Arial;
            color: #666;
            padding-right: 1px;
            text-align: left;
            width: 10%;
        }
        .auto-style4 {
            width: 11%;
        }
    </style>

      <style>
        .clear {
            margin: 0;
            padding: 0;
            border: 0;
            clear: both;
        }

        .border {
            border-right: thin solid #CCCCCC;
        }

        .borderLeft {
            border-left: thin solid #CCCCCC;
        }

        .boxContainerContainer {
            width: 94%;
            min-height: 500px;
            height: auto !important;
            height: 500px;
            margin: 0 auto;
            font-family: Arial, Helvetica, sans-serif;
        }

            .boxContainerContainer div.itlepartContainer {
                width: 100%;
                height: 35px;
                margin: 0;
                padding: 0;
            }

               

                .boxContainerContainer div.itlepartContainer h3.cf {
                    color: #0d668e;
                    font-size: 12px;
                    font-weight: bold;
                    height: 23px;
                    padding: 8px 0 5px 12px;
                    margin: 0 10px 0 0;
                    float: right;
                }


                .boxContainerContainer div.itlepartContainer h2 span {
                    margin: 0 0 0 20px;
                    color: #0d668e;
                }

            .boxContainerContainer div.btContainerPart {
                width: 100%;
                margin: 5px auto 0 auto;
                min-height: 200px;
                height: auto !important;
                height: 200px;
            }

                .boxContainerContainer div.btContainerPart div.lBxpartContainer {
                    width: 18%;
                    min-height: 300px;
                    height: auto !important;
                    height: 300px;
                    float: left;
                    padding: 10px;
                    background: #f7f5f5;
                    border: 5px solid #e4e4e4;
                    margin: 0 10px 0 0;
                }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer h3 {
                        font-size: 12px;
                        font-weight: bold;
                        color: #0d668e;
                        margin: 5px 0 5px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer input.radio {
                        float: left !important;
                        margin: 5px 0 0 0;
                        padding: 0;
                        width: 20px;
                        height: 20px;
                        display: block;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer a.lpb {
                        background: url(../StudentBinder/img/greenbtn.png) left top no-repeat;
                        width: 176px;
                        height: 22px;
                        font-size: 12px;
                        font-weight: normal;
                        color: #444545;
                        background-position: 0 0;
                        display: block;
                        text-decoration: none;
                        padding: 6px 0 0 25px;
                        float: right;
                        margin: 0 0 3px 0;
                    }

                        .boxContainerContainer div.btContainerPart div.lBxpartContainer a.lpb span {
                            font-size: 11px;
                            font-weight: bold;
                        }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer a.grb {
                        background: url(../StudentBinder/img/graybtn.png) left top no-repeat;
                        width: 176px;
                        height: 23px;
                        font-size: 12px;
                        font-weight: normal;
                        color: #444545;
                        background-position: 0 0;
                        display: block;
                        text-decoration: none;
                        padding: 6px 0 0 25px;
                        float: right;
                        margin: 0 0 3px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer input.txtfld {
                        border-radius: 3px;
                        border: 1px solid #c8cfcf;
                        margin: 0 0 0 44px;
                        height: 24px;
                        color: #666;
                        font-size: 12px;
                        width: 160px;
                        padding: 0 3px;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer input.btn {
                        width: 27px;
                        height: 28px;
                        float: left;
                        background: url(../StudentBinder/img/zoomlens.png) left top no-repeat;
                        cursor: pointer;
                        background-position: 0 0;
                        float: right !important;
                        border: none;
                        margin: -1px 0 0 0;
                    }

                        .boxContainerContainer div.btContainerPart div.lBxpartContainer input.btn:hover {
                            background-position: 0 -31px;
                        }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer p {
                        font-size: 12px;
                        color: #505050;
                        margin: 5px 0 5px 42px;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer input.smltxtfld {
                        border-radius: 3px;
                        border: 1px solid #c8cfcf;
                        margin: 0 5px 0 44px;
                        height: 24px;
                        color: #666;
                        font-size: 12px;
                        width: 67px;
                        padding: 0 3px;
                        float: left;
                    }

                    .boxContainerContainer div.btContainerPart div.lBxpartContainer input.smltxt {
                        border-radius: 3px;
                        border: 1px solid #c8cfcf;
                        margin: 0 5px 0 5px;
                        height: 24px;
                        color: #666;
                        font-size: 12px;
                        width: 70px;
                        padding: 0 3px;
                        float: left;
                    }

                .boxContainerContainer div.btContainerPart div.mBxContainer {
                    width: 20%;
                    height: 530px;
                    float: left !important;
                    padding: 10px;
                    background: #f7f5f5;
                    border: 5px solid #e4e4e4;
                    margin: 0 10px 0 0;
                    overflow-y: scroll;
                    overflow-x: hidden;
                }

                    .boxContainerContainer div.btContainerPart div.mBxContainer h3 {
                        font-size: 12px;
                        font-weight: bold;
                        color: #0d668e;
                        margin: 5px 0 5px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.mBxContainer a.gmb {
                        background: url(../StudentBinder/img/lbtngrn.png) left top no-repeat;
                        width: 200px;
                        height: 22px;
                        font-size: 12px;
                        font-weight: normal;
                        color: #444545;
                        background-position: 0 0;
                        display: block;
                        text-decoration: none;
                        padding: 7px 0 0 25px;
                        float: left;
                        margin: 0 0 3px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.mBxContainer a.grbmb {
                        background: url(../StudentBinder/img/lbtngrngray.png) left top no-repeat;
                        width: 200px;
                        height: 22px;
                        font-size: 12px;
                        font-weight: normal;
                        color: #444545;
                        background-position: 0 -0;
                        display: block;
                        text-decoration: none;
                        padding: 6px 0 0 25px;
                        float: left;
                        margin: 0 0 3px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.mBxContainer a.grmb {
                        background: url(../StudentBinder/img/lbtngrngray.png) left top no-repeat;
                        width: 200px;
                        height: 22px;
                        font-size: 11px;
                        font-weight: normal;
                        color: #0d668e;
                        background-position: 0 0;
                        display: block;
                        text-decoration: none;
                        padding: 6px 0 0 25px;
                        float: left;
                        margin: 0 0 3px 0;
                    }

                    .boxContainerContainer div.btContainerPart div.mBxContainer input.butn {
                        width: 220px;
                        height: 34px;
                        background: url(../StudentBinder/img/blbtn.png) left top no-repeat;
                        color: #fff;
                        font-weight: bold;
                        font-size: 11px;
                        border: none;
                        cursor: pointer;
                    }

                .boxContainerContainer div.btContainerPart div.righMainContainer {
                    width: 55%;
                    min-height: 543px;
                    height: auto !important;
                    height: 300px;
                    float: right !important;
                    padding: 4px;
                    background: #fff;
                    border: 5px solid #e4e4e4;
                    margin: 0 0px 0 0;
                }

                    .boxContainerContainer div.btContainerPart div.righMainContainer h3 {
                        font-size: 12px;
                        font-weight: bold;
                        color: #0d668e;
                        margin: 5px 0 5px 0;
                    }


        /*----------------------------------------------------------- JQuery Start ------------------------------------------------------*/

        h1, h2, h3, h4, h5, h6, p, ul, li {
            margin: 0;
            padding: 0;
            border: 0;
            vertical-align: baseline;
            list-style: none;
        }

            h2 a {
                padding: 7px 7px 7px 20px;
            }

            h3 a {
                padding: 7px 7px 7px 30px;
            }

            h2 a, h3, a {
                display: block;
                text-decoration: none;
            }

            .accordion li, li.accordion {
                border: none;
            }

        .accordion h2 a, .accordion h3 a {
            text-decoration: none;
            background: url(../StudentBinder/img/computer.png) 5px 6px no-repeat !important;
            z-index: 999;
            padding: 9px 0 2px 30px;
            font-size: 11px;
            font-weight: bold;
            color: #1f1f1f;
            width: 48%;
            float: left;
        }

            .accordion h2 a.kk, .accordion h3 a.kk {
                text-decoration: none;
                background: none !important;
                z-index: 999;
                padding: 2px 0 6px 0;
                font-size: 12px;
                font-weight: normal;
                color: #444545;
                width: 78%;
                float: left;
                border: none;
                text-align: left;
                cursor: pointer;
            }

        .accordion h2 span {
            cursor: pointer;
        }

        .accordion .wrapper {
            padding: 5px;
            line-height: 18px;
            width: 92%;
        }

        .accordion .nomar {
            padding: 0px;
            width: 100%;
        }

        .accordion h2,
        .accordion h3 {
            height: 31px;
            background: url(../StudentBinder/img/longgreenbar.png) right top no-repeat;
            font-size: 12px;
            font-weight: bold;
            color: #000;
            margin: 0 0 2px 0;
            padding: 0;
            width: 100%;
        }

            .accordion h2 span.dd,
            .accordion h3 span.dd {
                width: 4px;
                height: 31px;
                float: left;
                margin: 0;
                padding: 0;
                background: url(../StudentBinder/img/llbar.jpg) left top no-repeat;
                display: block;
            }

            .accordion h2.BG,
            .accordion h3.BG {
                height: 23px;
                background: url(../StudentBinder/img/dwngrenbg.png) left top no-repeat;
                width: 230px;
                font-size: 12px;
                font-weight: normal;
                color: #444545;
                margin: 0 0 3px 0;
                padding: 6px 0 0 25px;
                border: none;
                background-position: 0 0;
            }

                .accordion h2.BG:hover,
                .accordion h3.BG:hover {
                    background-position: 0 -32px;
                    color: #000 !important;
                }



            .accordion h2 div.container, .accordion h3 div.container {
                float: right;
                width: 35%;
                height: 20px;
                margin: 9px 0 0 0;
                padding: 0;
                background: none;
            }

                .accordion h2 div.container span, .accordion h3 div.container span {
                    float: left;
                    font-size: 12px;
                    font-weight: bold;
                    color: #1f1f1f;
                    display: block;
                    z-index: 999;
                    margin: 0 0 0 0;
                    width: 180px;
                }

                    .accordion h2 div.container span input.rdo, .accordion h3 div.container span input.rdo {
                        float: left !important;
                        display: block;
                        width: 20px;
                        height: 20px;
                        margin: -2px 10px 0 0;
                        margin: -6px 10px 0 0;
                    }

                .accordion h2 div.container a.dlt, .accordion h3 div.container a.dlt {
                    float: right;
                    width: 20px;
                    height: 17px;
                    display: block;
                    margin: 0 15px 0 0;
                    padding: 0;
                    background: url(../StudentBinder/img/delet0_2.png) left top no-repeat !important;
                    background-position: 0 0;
                    border-bottom: 0;
                }

                    .accordion h2 div.container a.dlt:hover, .accordion h3 div.container a.dlt:hover {
                        background-position: 0 -21px !important;
                    }


        .accordion .wrapper div.ingrContainer {
            border-radius: 5px;
            margin: 5px 0 0 0;
            min-height: 75px;
            height: auto !important;
            height: 75px;
            width: 104%;
            border: 1px solid #a1a1a1;
            background: #efefef;
            padding: 10px;
            font-size: 12px;
            font-weight: normal;
            color: #1f1f1f;
        }


        .accordion .wrapper div.nobdrrcontainer {
            margin: 1px 0 0 0;
            min-height: 25px;
            height: auto !important;
            height: 25px;
            width: 99%;
            font-size: 12px;
            font-weight: normal;
            color: #1f1f1f;
        }

        .accordion .wrapper div.ingrContainer h3 {
            font-size: 12px;
            color: #0d668e;
            font-weight: bold;
            float: left;
            background: none;
            margin: 0;
            padding: 0;
            width: 60%;
            float: left;
        }

        .accordion .wrapper div.ingrContainer p {
            font-size: 12px;
            font-weight: normal;
            color: #1f1f1f;
        }

        .accordion .wrapper div.ingrContainer div.container {
            float: right;
            width: 35%;
            height: 20px;
            margin: 0;
            padding: 0;
        }

            .accordion .wrapper div.ingrContainer div.container span {
                float: left;
                font-size: 12px;
                font-weight: bold;
                color: #1f1f1f;
                display: block;
                margin: 0 0 0 0;
                width: 180px;
            }

                .accordion .wrapper div.ingrContainer div.container span input.rdo {
                    float: left !important;
                    display: block;
                    width: 20px;
                    height: 20px;
                    margin: -2px 10px 0 0;
                    margin: -6px 10px 0 0 ;
                }


            .accordion .wrapper div.ingrContainer div.container a.dlt {
                float: right;
                width: 20px;
                height: 17px;
                display: block;
                margin: 0 15px 0 0;
                padding: 0;
                background: url(../StudentBinder/img/delet0_2.png) left top no-repeat;
                background-position: 0 0;
                border-bottom: 0;
            }

                .accordion .wrapper div.ingrContainer div.container a.dlt:hover {
                    background-position: 0 -21px;
                }





        .boxContainerContainer div.btContainerPart div.mBxContainer a.grmb div.grapgContainer {
            width: 50px;
            height: 18px;
            border: 1px solid #9ba1a1;
            border-radius: 3px;
            float: right;
            margin: -2px 28px 0 0;
            font-size: 9px;
            font-weight: normal;
            color: #0d668e;
        }

            .boxContainerContainer div.btContainerPart div.mBxContainer a.grmb div.grapgContainer span {
                width: 16px;
                height: 16px;
                margin: 0 0 0 3px;
                float: left;
                background: url(img/loder.png) left top no-repeat;
                background-position: -21px 0;
            }

            .boxContainerContainer div.btContainerPart div.mBxContainer a.grmb div.grapgContainer p {
                margin: 2px 0 0 22px;
                margin: 5px 0 0 22px;
            }

        .boxContainerContainer div.btContainerPart div.large {
            width: 75%;
            float: right !important;
        }
        /*-----------------------------------------------------------JQuery End-------------------------------------------*/


        /*-----------------------------------------------Second Page Start--------------------------------------------------*/
        fieldset, img {
            border: 0;
        }

        address, caption, cite, code, dfn, em, strong, th, var {
            font-style: normal;
            font-weight: normal;
        }

       .topper ol, .topper ul {
            list-style: none;
            width: 100%;
        }

        caption, th {
            text-align: left;
        }

        abbr, acronym {
            border: 0;
        }

        .topper {
            width: 98%;
            margin: 0 auto;
            text-align: left;
            padding:0 49px 0 0;
            /*background: #eeeeee;*/
        }

        h1 {
            font-size: 21px;
            height: 43px;
            line-height: 47px;
            text-transform: uppercase;
        }

        .navigationTabs {
            height: 35px;
            line-height: 23px;
            background: url(../StudentBinder/img/blrbg.jpg) right top no-repeat;
            margin: 0;
            padding: 0;
        }

            .navigationTabs span.bgb {
                margin: 0;
                padding: 0;
                background: url(../StudentBinder/img/lblbtn.png) left top no-repeat;
                float: left;
                display: block;
                width: 7px;
                height: 35px;
            }

            .navigationTabs li {
                float: left;
                height: 35px;
                line-height: 23px;
                padding-right: 0px;
                background: url(../StudentBinder/img/linecut.jpg) right top no-repeat;
            }

                .navigationTabs li.nobg {
                    float: left;
                    height: 35px;
                    line-height: 23px;
                    padding-right: 0px;
                    background: none;
                }

                .navigationTabs li a {
                    float: left;
                    dispaly: block;
                    height: 28px;
                    line-height: 23px;
                    padding: 7px 11px 0 11px;
                    overflow: hidden;
                    width: 115px;
                    margin: 0 1px;
                    color: #fff;
                    position: relative;
                    text-decoration: none;
                    font-family: Arial, Helvetica, sans-serif;
                    font-size: 11px;
                    text-align: center;
                    font-weight: bold;
                    background: url(../StudentBinder/img/warrow.png) center bottom no-repeat;
                }

                    .navigationTabs li a:hover {
                        background: url(img/hoverbggreen.jpg) left top repeat-x;
                        background-color: transparent !important;
                        border-bottom: none;
                        color: #000 !important;
                    }

                    .navigationTabs li a.active {
                        background: #86b2c6;
                        border-bottom: none;
                        color: #0d668e;
                    }

        .tabsContent {
            border-top: 0px solid;
            width: 100%;
            overflow: hidden;
            padding: 0 -2px !important;
        }

        .tab {
            /*padding: 10px;*/
            display: none;
            min-height: 485px;
            height: auto !important;
        }

            .tab h2 {
                font-size: 14px;
                font-weight: bold;
                color: #0d668e !important;
                margin: 5px 0 5px 0;
                letter-spacing: 1px;
            }

            .tab h3 {
                font-weight: bold;
                font-size: 12px;
                margin-top: 20px;
                color: #514f4f !important;
            }

            .tab p {
                margin-top: 16px;
                clear: both;
            }

            .tab ul {
                margin-top: 16px;
                list-style: disc;
            }

            .tab li {
                margin: 10px 0 0 35px;
            }

            .tab a {
                color: #8FB0CF;
            }

            .tab strong {
                font-weight: bold;
            }

            .tab pre {
                font-size: 11px;
                margin-top: 20px;
                width: 668px;
                overflow: auto;
                clear: both;
            }

            .tab table {
                width: 100%;
            }

                .tab table td {
                    padding: 0px 10px 0px 0;
                    vertical-align: middle;
                }

                    .tab table td input.rdoo {
                        width: 16px;
                        height: 16px;
                        float: left;
                        margin: 10px 5px 0 0;
                        margin: 6px 5px 0 0 \9;
                        border: none;
                        display: block;
                    }

      

        .tab table td input.tx {
            border-radius: 3px;
            border: 1px solid #b2b2b2;
            width: 300px;
            height: 23px;
            float: left;
            background: #f5f5f5;
            margin: 5px 0 0 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }

        .tab table td textarea.tarea {
            border-radius: 5px;
            border: 1px solid #b2b2b2;
            width: 300px;
            height: 75px;
            float: left;
            background: #f5f5f5;
            margin: 5px 0 0 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }

        .tab table td input.bTnn {
            background: url(../StudentBinder/img/btnbg.PNG) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 5px 0 0 0;
            padding: 0;
            border: 0;
            width: 97px;
            height: 29px;
            cursor: pointer;
        }

            .tab table td input.bTnn:hover {
                color: #000;
            }

        /*-----------------------------------------------Second Page End--------------------------------------------------*/



        /*-----------------------------------------------Third Page Start--------------------------------------------------*/
        .tab div.itomesContainer {
            width: 96%;
            min-height: 25px;
            height: auto !important;
            height: 25px;
            padding: 5px;
            margin: 0 10px;
            border: 5px solid #cccccc;
            background: #e1e1e1;
            text-align: left;
        }

        .tab table td p {
            font-size: 12px;
            font-weight: normal;
            color: #514f4f;
            margin: 0 0 0 0;
            padding: 5px 0 0 0;
            display: block;
            text-align: left;
            vertical-align:middle;
        }

        .tab table td a.smlbt {
            background: url(../StudentBinder/img/smlbtn.png) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 0 auto 0 auto;
            padding: 4px 0 0 0;
            border: 0;
            width: 69px;
            height: 20px;
            cursor: pointer;
            display: block;
            text-align: center !important;
        }

            .tab table td a.smlbt:hover {
                color: #000;
            }


        .tab table td input.smlbt {
            background: url(../StudentBinder/img/smlbtn.png) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 0 auto 0 auto;
            padding: 0px 0 0 0;
            border: 0;
            width: 69px;
            height: 24px;
            cursor: pointer;
            display: block;
            text-align: center !important;
        }

            .tab table td input.smlbt:hover {
                color: #000;
            }

        .tab input.rbtn {
            background: url(../StudentBinder/img/btnbg.PNG) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 5px 10px 5px 0;
            padding: 0;
            border: 0;
            width: 97px;
            height: 29px;
            float: right;
            cursor: pointer;
        }

            .tab input.rbtn:hover {
                color: #000;
            }

        .tab table td input.dx {
            border-radius: 3px;
            border: 1px solid #b2b2b2;
            width: 300px;
            height: 23px;
            float: left;
            background: #f5f5f5;
            margin: 0 0 0 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }

        .tab table td input.tp {
            margin: 0 0 5px -2px;
        }

        .tab table td input.ti {
            margin: 0 0 5px 0;
            margin: 0 0 5px 12px ;
        }

        .tab table td input.rdo1 {
            width: 16px;
            height: 16px;
            float: left;
            margin: 10px 5px 0 3px;
            margin: 6px 5px 0 3px ;
            border: none;
            display: block;
        }

        .tab hr {
            border: 1px dotted #0d668e;
            margin: 10px 0;
        }


        .tab table td a.smlRbt {
            background: url(../StudentBinder/img/smlbtn.png) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 5px auto 0 auto;
            padding: 4px 0 0 0;
            border: 0;
            width: 69px;
            height: 20px;
            cursor: pointer;
            display: block;
            text-align: center !important;
        }

            .tab table td a.smlRbt:hover {
                color: #000;
            }


        .tab table td input.smlRbt {
            background: url(../StudentBinder/img/smlbtn.png) left top no-repeat;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #fff;
            font-weight: bold;
            margin: 5px auto 0 auto;
            padding: 0px 0 0 0;
            border: 0;
            width: 69px;
            height: 25px;
            cursor: pointer;
            display: block;
            text-align: center !important;
        }

        .tab table td a.input:hover {
            color: #000;
        }


        /*-----------------------------------------------Third Page End--------------------------------------------------*/


        /*-----------------------------------------------Forth Page Tart--------------------------------------------------*/
        .tab textarea.tareaLong {
            border-radius: 5px;
            border: 1px solid #b2b2b2;
            width: 100%;
            height: 75px;
            float: left;
            background: #f5f5f5;
            margin: 5px auto 0 auto;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }
        /*-----------------------------------------------Forth Page End--------------------------------------------------*/


        /*-----------------------------------------------Fifth  Page Start--------------------------------------------------*/
        .tab select.txx {
            border-radius: 3px;
            border: 1px solid #b2b2b2;
            width: 250px;
            height: 23px;
            float: left;
            background: #f5f5f5;
            margin: 5px 0 0 0;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }

        .tab label.ii {
            font-size: 12px;
            font-weight: normal;
            color: #514f4f;
            margin: 0 0 0 0;
            padding: 10px 0 0 0;
            display: block;
            width: 430px;
            float: left;
        }

        .tab textarea.srBox {
            border-radius: 5px;
            border: 1px solid #b2b2b2;
            width: 100%;
            height: 110px !important;
            float: left;
            background: #f5f5f5;
            margin: 5px auto 0 auto;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 12px;
            text-align: left;
            color: #666;
        }
        /*-----------------------------------------------Fifth  Page End--------------------------------------------------*/
        #AddSetDiv {
            background-color: #fff;
            padding: 5px;
            position: absolute;
            top: 10px;
            background-color: White;
            display: none;
            margin: auto;
            z-index: 10000;
            width: 840px;
            min-height: 215px;
            height: auto !important;
        }

        #AddStepDiv {
            background-color: #fff;
            padding: 5px;
            position: absolute;
            top: 10px;
            background-color: White;
            display: none;
            margin: auto;
            z-index: 10000;
            width: 905px;
            min-height: 215px;
            height: auto !important;
        }

        #AddMeasureDiv {
            background-color: #fff;
            padding: 5px;
            position: absolute;
            top: 10px;
            background-color: White;
            display: none;
            margin: auto;
            z-index: 10000;
            width: 905px;
            min-height: 440px;
            height: auto !important;
        }


        #AddCriteriaDiv {
            background-color: #fff;
            padding: 5px;
            position: absolute;
            top: 10px;
            background-color: White;
            display: none;
            margin: auto;
            z-index: 10000;
            width: 974px;
            height: 270px;
        }


        .fullOverlay {
            display: none;
            top: 0px;
            left: 0px;
            position: fixed;
            z-index: 2000;
            width: 100%;
            height: 100%;
            background-image: url("../Administration/images/overlay.png");
        }

        #Close1 {
            display: block;
            height: 23px;
            overflow: hidden;
            position: absolute;
            right: 4px;
            top: 2px;
            width: 24px;
        }

        #close_Step {
            display: block;
            height: 23px;
            overflow: hidden;
            position: absolute;
            right: 4px;
            top: 2px;
            width: 24px;
        }

        #close_CriteriaPopup {
            display: block;
            height: 23px;
            overflow: hidden;
            position: absolute;
            right: 4px;
            top: 2px;
            width: 24px;
        }

        a {
            text-decoration: none;
        }


        .waitBlank {
            background-color: #FFFFFF;
            display: block;
            width: 10% !important;
            opacity: 0.6;
            margin: 0 auto !important;
            position: absolute;
            z-index: 1;
        }

            .waitBlank img {
                margin: 0 auto;
            }
    </style>


    <style>
        input.lbtn {
            width: 96px;
            height: 31px;
            background: url(../StudentBinder/img/longbtn.png) left top no-repeat;
            font-size: 0px;
            color: #333;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px !important;
            font-weight: bold;
            text-align: center;
            margin: 0 0 3px 0;
            padding: 0;
            border: 0;
            cursor: pointer;
            letter-spacing: 1px;
            background-position: 0 -40px;
            line-height: 5px;
        }

            input.lbtn:hover {
                background-position: 0 0px;
                color: #fff;
            }

        input.smllbtn {
            width: 31px;
            height: 31px;
            background: url(../StudentBinder/img/smallbtn.png) left top no-repeat;
            font-size: 0px;
            color: #333;
            font-family: Arial, Helvetica, sans-serif;
            font-size: 14px !important;
            font-weight: bold;
            text-align: center;
            margin: 0 0 3px 0;
            padding: 0;
            border: 0;
            line-height: 0;
            cursor: pointer;
            letter-spacing: 1px;
            background-position: 0 -40px;
        }

            input.smllbtn:hover {
                background-position: 0 0px;
                color: #fff;
            }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="Server">
   
            <table width="99%">

               
                <tr>
                    <td colspan="4">
                        <table width="99%">
                            <tr>
                                <td class="tdText"><b>Name Of Student: </b>
                                    <asp:Label ID="lblStNameAttend" runat="server" CssClass="tdText"></asp:Label>
                                </td>
                                <td class="tdText">
                                    <b>Date of Birth: </b>
                                    <asp:Label ID="lblAttendDob" runat="server" CssClass="tdText"></asp:Label>
                                </td>
                                <td class="tdText">
                                   <b>Gender: </b> 
                                    <asp:Label ID="lblAttendGender" runat="server" CssClass="tdText"></asp:Label>
                                </td>
                                <td style="text-align:right"><asp:Button ID="btnback" runat="server" Text="Back to View Student" CssClass="NFButton" OnClick="btnback_Click" Width="150px"/></td>
                            </tr>

                            
                        </table>

                    </td>
                </tr>
                
               
                
               
                <tr><td colspan="3"></td></tr>
               
            </table>
   
     <div class="righMainContainer large" id="MainDiv" runat="server" visible="true">
                        <div class="topper">
                            <ul class="navigationTabs">
                               <span class="bgb"></span>
                                <li><a href="#" rel="AS">Attending Skills</a></li>
                                <li><a href="#" rel="IS">Imitation Skills</a></li>
                                <li><a href="#" rel="RLS">Receptive Language Skills</a></li>
                                <li><a href="#" rel="ELS" >Expressive Language Skills</a></li>
                                <li><a href="#" rel="PS" >PreAcademic Skills</a></li>
                                <li><a href="#" rel="SHS" >Self Help Skills</a></li>
                                <%--<li class="nobg"><a href="#" rel="implement">Lesson Producure</a></li>--%>
                            </ul>

                            <div class="tabsContent">
                               

                                    

                                <div class="tab">
                                    
                                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                        <ContentTemplate>
                                            <table style="width:100%"><tr><td runat="server" id="AttendingSkills"></td></tr></table>
                                    <h2>Attending Skills</h2>
                                   <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText"></td>
                                                <td style="width: 20%;" align="left"></td>
                                                <td class="tdText" align="right"></td>
                                                <td style="width: 30%;" align="left" class="tdText">Last Modified :
                                                    <asp:Label ID="lblLastModified" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>

                                        </table>
                                        <asp:GridView ID="grdAttending" runat="server" AutoGenerateColumns="False" GridLines="none"
                                            Width="100%">
                                            <Columns>
                                                <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="10%" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="30%" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                    <ItemTemplate>
                                                        <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                            <asp:ListItem>Present</asp:ListItem>
                                                            <asp:ListItem>Not Present</asp:ListItem>
                                                            <asp:ListItem>Emerging</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="35%" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Comment" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="25%" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                            <RowStyle CssClass="RowStyle" />
                                            <AlternatingRowStyle CssClass="AltRowStyle" />
                                            <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                            <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                            <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                            <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                            <SortedAscendingHeaderStyle BackColor="#487575" />
                                            <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                            <SortedDescendingHeaderStyle BackColor="#275353" />
                                        </asp:GridView>
                                        <table width="100%">
                                            <tr>
                                                <td>&nbsp;<td>&nbsp;</td>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="text-align: center; width: 100%">


                                                    <asp:Button ID="btnSave" runat="server" CssClass="NFButton" OnClick="btnSave_Click"
                                                        Text="Save" />


                                                </td>

                                            </tr>
                                        </table>
                                            </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <br />
                                </div>


                                <div class="tab">
                                    
                                     <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                        <ContentTemplate>
                                             <table style="width:100%"><tr><td runat="server" id="ImitationSkills"></td></tr></table>
                                     <h2>Imitation Skills</h2>
                                    <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText">&nbsp;</td>
                                                <td style="width: 20%;" align="left">&nbsp;</td>
                                                <td class="tdText">Last Modified :
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:Label ID="lblDateModImitation" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>

                                        </table>
                                        <asp:GridView ID="grdImitation" runat="server" AutoGenerateColumns="False" GridLines="none"
                                            Width="100%">
                                            <Columns>
                                                <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="10%" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="35%" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                    <ItemTemplate>
                                                        <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                            <asp:ListItem>Present</asp:ListItem>
                                                            <asp:ListItem>Not Present</asp:ListItem>
                                                            <asp:ListItem>Emerging</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="35%" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Comment" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="25%" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                            <RowStyle CssClass="RowStyle" />
                                            <AlternatingRowStyle CssClass="AltRowStyle" />
                                            <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                            <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                            <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                            <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                            <SortedAscendingHeaderStyle BackColor="#487575" />
                                            <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                            <SortedDescendingHeaderStyle BackColor="#275353" />
                                        </asp:GridView>
                                        <table width="100%">
                                            <tr>
                                                <td>&nbsp;<td>&nbsp;</td>
                                                </td>
                                            </tr>

                                            <tr align="center">
                                                <td style="text-align: center; width: 100%">

                                                    <asp:Button ID="btnSave1" runat="server" CssClass="NFButton" Text="Save" OnClick="btnSave1_Click" />

                                                </td>
                                            </tr>
                                        </table>
                                    </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <br />
                                </div>


                                <div class="tab">
                                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                        <ContentTemplate>
                                             <table style="width:100%"><tr><td runat="server" id="ReceptiveLanguageSkills"></td></tr></table>
                                    <h2>Receptive Language Skills
                                        </h2>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText"></td>
                                                <td style="width: 20%;" align="left"></td>
                                                <td class="tdText">Last Modified :
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:Label ID="lblDateModifiedReceptive" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>
                                        <asp:GridView ID="grdReceptive" runat="server" AutoGenerateColumns="False" GridLines="none"
                                            Width="100%">
                                            <Columns>
                                                <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="10%" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30%">
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="30%" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                    <ItemTemplate>
                                                        <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                            <asp:ListItem>Present</asp:ListItem>
                                                            <asp:ListItem>Not Present</asp:ListItem>
                                                            <asp:ListItem>Emerging</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="35%" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Comment" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                    </ItemTemplate>
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                    <ItemStyle Width="25%" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                            <RowStyle CssClass="RowStyle" />
                                            <AlternatingRowStyle CssClass="AltRowStyle" />
                                            <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                            <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                            <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                            <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                            <SortedAscendingHeaderStyle BackColor="#487575" />
                                            <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                            <SortedDescendingHeaderStyle BackColor="#275353" />
                                        </asp:GridView>
                                        <table width="100%">
                                            <tr>
                                                <td>&nbsp;<td>&nbsp;</td>
                                                </td>
                                            </tr>
                                            <tr align="center">
                                                <td style="text-align: center; width: 100%">

                                                    <asp:Button ID="BtnSave2" runat="server" CssClass="NFButton" Text="Save" OnClick="BtnSave2_Click" />

                                                </td>
                                            </tr>
                                        </table>
                                  </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>

                                <div class="tab">
                                    <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                                        <ContentTemplate>
                                             <table style="width:100%"><tr><td runat="server" id="ExpressiveLanguageSkills"></td></tr></table>
                                        <h2 class="style5">Expressive Language Skills
                                        </h2>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText"></td>
                                                <td style="width: 20%;" align="left"></td>
                                                <td class="tdText">Last Modified :
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:Label ID="lblDateModifiedExpressive" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>
                                        <div>
                                            <asp:GridView ID="grdExpressive" runat="server" AutoGenerateColumns="False" GridLines="none"
                                                Width="100%">
                                                <Columns>
                                                    <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="10%" />
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="30%" />
                                                    </asp:BoundField>
                                                    <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                        <ItemTemplate>
                                                            <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                                <asp:ListItem>Present</asp:ListItem>
                                                                <asp:ListItem>Not Present</asp:ListItem>
                                                                <asp:ListItem>Emerging</asp:ListItem>
                                                            </asp:RadioButtonList>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="35%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Comment" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="25%" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                                <RowStyle CssClass="RowStyle" />
                                                <AlternatingRowStyle CssClass="AltRowStyle" />
                                                <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                                <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                                <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                                <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                                <SortedAscendingHeaderStyle BackColor="#487575" />
                                                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                                <SortedDescendingHeaderStyle BackColor="#275353" />
                                            </asp:GridView>
                                            <table width="100%">
                                                <tr>
                                                    <td>&nbsp;<td>&nbsp;</td>
                                                    </td>
                                                </tr>
                                                <tr align="center">
                                                    <td style="text-align: center; width: 100%">

                                                        <asp:Button ID="BtnSave3" runat="server" CssClass="NFButton" Text="Save" OnClick="BtnSave3_Click" />

                                                    </td>
                                                </tr>
                                            </table>
                                    <div class="clear"></div>
                                </div>
                                             </ContentTemplate>
                                    </asp:UpdatePanel>
                                    </div>

                                <div class="tab">
                                     <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                                        <ContentTemplate>
                                             <table style="width:100%"><tr><td runat="server" id="PreAcademicSkills"></td></tr></table>
                                     <h2>PreAcademic Skills
                                        </h2>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText"></td>
                                                <td style="width: 20%;" align="left"></td>
                                                <td class="tdText">Last Modified :
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:Label ID="lblModifiedPreAcademic" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>
                                        <div>
                                            <asp:GridView ID="grdPreacademics" runat="server" AutoGenerateColumns="False" GridLines="none"
                                                Width="100%">
                                                <Columns>
                                                    <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="10%" />
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="30%" />
                                                    </asp:BoundField>
                                                    <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                        <ItemTemplate>
                                                            <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                                <asp:ListItem>Present</asp:ListItem>
                                                                <asp:ListItem>Not Present</asp:ListItem>
                                                                <asp:ListItem>Emerging</asp:ListItem>
                                                            </asp:RadioButtonList>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="35%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Comment" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="25%" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                                <RowStyle CssClass="RowStyle" />
                                                <AlternatingRowStyle CssClass="AltRowStyle" />
                                                <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                                <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                                <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                                <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                                <SortedAscendingHeaderStyle BackColor="#487575" />
                                                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                                <SortedDescendingHeaderStyle BackColor="#275353" />
                                            </asp:GridView>
                                            <table width="100%">
                                                <tr>
                                                    <td>&nbsp;<td>&nbsp;</td>
                                                    </td>
                                                </tr>
                                                <tr align="center">
                                                    <td style="text-align: center; width: 100%">

                                                        <asp:Button ID="BtnSave4" runat="server" CssClass="NFButton" Text="Save" OnClick="BtnSave4_Click" />

                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>

                                <div class="tab">
                                   
                                   <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                                        <ContentTemplate>
                                             <table style="width:100%"><tr><td runat="server" id="SelfHelpSkills"></td></tr></table>
                                        <h2>Self Help Skills
                                        </h2>
                                        <table width="100%">
                                            <tr>
                                                <td style="width: 20%;" class="tdText"></td>
                                                <td style="width: 20%;" align="left"></td>
                                                <td class="tdText">Last Modified :
                                                </td>
                                                <td style="width: 30%;">
                                                    <asp:Label ID="lblSelf" runat="server" CssClass="tdText"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>

                                        <div>
                                            <asp:GridView ID="grdSelfHelp" runat="server" AutoGenerateColumns="False" GridLines="none"
                                                Width="100%">
                                                <Columns>
                                                    <asp:BoundField DataField="QId" HeaderText="QId" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="10%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="10%" />
                                                    </asp:BoundField>
                                                    <asp:BoundField DataField="Ques" HeaderText="Question" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30%">
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="30%" />
                                                    </asp:BoundField>
                                                    <asp:TemplateField HeaderText="Score" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="35%">
                                                        <ItemTemplate>
                                                            <asp:RadioButtonList ID="rdbScore" runat="server" RepeatDirection="Horizontal" Width="288px">
                                                                <asp:ListItem>Present</asp:ListItem>
                                                                <asp:ListItem>Not Present</asp:ListItem>
                                                                <asp:ListItem>Emerging</asp:ListItem>
                                                            </asp:RadioButtonList>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="35%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Comments" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="25%">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtComment" runat="server" CssClass="textClassWithoutWidth" TextMode="MultiLine" Rows="2" Columns="50"></asp:TextBox>
                                                        </ItemTemplate>
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                        <ItemStyle Width="25%" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <HeaderStyle CssClass="HeaderStyle" Font-Bold="True" ForeColor="White" />

                                                <RowStyle CssClass="RowStyle" />
                                                <AlternatingRowStyle CssClass="AltRowStyle" />
                                                <FooterStyle CssClass="FooterStyle" ForeColor="#333333" />
                                                <SelectedRowStyle CssClass="SelectedRowStyle" BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                                <PagerStyle CssClass="PagerStyle" ForeColor="White" HorizontalAlign="Center" />
                                                <EmptyDataRowStyle CssClass="EmptyDataRowStyle" />
                                                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                                <SortedAscendingHeaderStyle BackColor="#487575" />
                                                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                                <SortedDescendingHeaderStyle BackColor="#275353" />
                                            </asp:GridView>
                                            <table width="100%">
                                                <tr>
                                                    <td>&nbsp;<td>&nbsp;</td>
                                                    </td>
                                                </tr>
                                                <tr align="center">
                                                    <td style="text-align: center; width: 100%">

                                                        <asp:Button ID="BtnSave5" runat="server" CssClass="NFButton" Text="Save" OnClick="BtnSave5_Click" />

                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>

                               

                               

                            </div>

                        </div>
                        
                    </div>
</asp:Content>
