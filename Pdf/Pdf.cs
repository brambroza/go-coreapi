using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;
using System.Data;
namespace coreapi.Pdf
    {
    
            public class ExportPdf
            {
                private readonly byte[] _contentBytes;

                public ExportPdf(string reportPath, string newPath, string id)
                {
                    ReportDocument reportDocument = new ReportDocument();

                    reportDocument.Load(reportPath, OpenReportMethod.OpenReportByDefault);
                    //reportDocument.SetDataSource(dataSet);
                  //  reportDocument.SetDatabaseLogon("sa", "1234@pass", "PCLDK\\PCLDKERP", "DB_Payroll");
                    reportDocument.SetDatabaseLogon("sa", "1234", "NOHF\\NEXPROJECT", "NSDBs");
                    reportDocument.SetParameterValue("QuatationNo", id);
                    reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, newPath);
                    _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.PortableDocFormat));
                }
                private static byte[] StreamToBytes(Stream input)
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
            }



            public class ExportPdfNew
            {
                private readonly byte[] _contentBytes;

                public ExportPdfNew(string reportPath, string newPath, string id)
                {
                    ReportDocument reportDocument = new ReportDocument();
                    try
                    {
                        reportDocument.Load(reportPath, OpenReportMethod.OpenReportByDefault);
                    }
                    catch (Exception)
                    {

                        try
                        {
                            reportDocument.Load(reportPath);
                        }
                        catch (Exception)
                        {

                            reportDocument.Load(reportPath, OpenReportMethod.OpenReportByTempCopy);
                        }
                    }

                    //reportDocument.SetDataSource(dataSet);
                  //  reportDocument.SetDatabaseLogon("sa", "1234@pass", "PCLDK\\PCLDKERP", "DB_Payroll");
            //  reportDocument.SetDatabaseLogon("sa", "1234", "NOHF", "DB_Payroll");
                  reportDocument.SetDatabaseLogon("sa", "1234", "NOHF\\NEXPROJECT", "NSDBs");
           // reportDocument.SetDatabaseLogon("sa", "dr0wss@p", "SRV-EXPRESS\\WEBAPP", "NSDBs");
            reportDocument.SetParameterValue("QuatationNo", id);
                    reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, newPath);
                    _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.PortableDocFormat));
                }


                private static byte[] StreamToBytes(Stream input)
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
            }

    public class ExportPdfProblemreport
    {
        private readonly byte[] _contentBytes;

        public ExportPdfProblemreport(string reportPath, string newPath, string pmcustcode , string pmsdate , string pmedate , string statewait , string statefinish )
        {
            ReportDocument reportDocument = new ReportDocument();
            
            try
            {
                reportDocument.Load(reportPath, OpenReportMethod.OpenReportByDefault);
               // reportDocument.Load(reportPath, OpenReportMethod.OpenReportByTempCopy);
             
            }
            catch (Exception)
            {

                try
                {
                    reportDocument.Load(reportPath);
                }
                catch (Exception)
                {

                    reportDocument.Load(reportPath, OpenReportMethod.OpenReportByTempCopy);
                }
            }

            //reportDocument.SetDataSource(dataSet);
            //  reportDocument.SetDatabaseLogon("sa", "1234@pass", "PCLDK\\PCLDKERP", "DB_Payroll");
            //  reportDocument.SetDatabaseLogon("sa", "1234", "NOHF", "DB_Payroll");
            // reportDocument.SetDatabaseLogon("sa", "1234", "NOHF\\NEXPROJECT", "NSDBs");
        
           //reportDocument.SetDatabaseLogon("sa", "dr0wss@p", "SRV-EXPRESS\\WEBAPP", "NSDBs" );
           // reportDocument.DataSourceConnections[0].IntegratedSecurity = true;
           // reportDocument.SetParameterValue("custcode", pmcustcode);
           // reportDocument.SetParameterValue("edate", pmedate);
           // reportDocument.SetParameterValue("sdate", pmsdate);
           // reportDocument.SetParameterValue("statewait", statewait  );
           // reportDocument.SetParameterValue("statefinish", statefinish );
           DataTable  dt;
            string _cmd;
            _cmd = "  select *  from NSDBs.dbo.v_problemservicereport      ";
            _cmd += "  where  CustCode in ('" + pmcustcode.Replace("|", "','").Substring(0, pmcustcode.Length + 1) + "' ) ";
            _cmd += "  and   ReceiveDatef between '" + pmsdate.Replace("~", "' and  '") +"'" ;
            _cmd += "  and ( statusfinish='" + statefinish + "'";
            _cmd += "  or statuswait='" + statewait + "' )"; 

            dt = coreapi.DB.DBConn.GetDataTable(_cmd);
             reportDocument.SetDataSource(dt);

            //reportDocument.SetParameterValue("custcode", pmcustcode);
            //reportDocument.SetParameterValue("edate", pmedate);
            //reportDocument.SetParameterValue("sdate", pmsdate);
            //reportDocument.SetParameterValue("statewait", statewait);
            //reportDocument.SetParameterValue("statefinish", statefinish);
            //PdfFormatOptions formatOpt = new PdfFormatOptions();
            //formatOpt.FirstPageNumber = 0;
            //formatOpt.LastPageNumber = 0;
            //formatOpt.UsePageRange = false;
            //formatOpt.CreateBookmarksFromGroupTree = false;

            //DiskFileDestinationOptions dest = new DiskFileDestinationOptions();
            //dest.DiskFileName = "C:\\Windows\\Temp\\5555.pdf";

            //ExportOptions ex = new ExportOptions();
            //ex.ExportDestinationType = ExportDestinationType.DiskFile;
            //ex.ExportDestinationOptions = dest;
            //ex.ExportFormatType = ExportFormatType.PortableDocFormat;
            //ex.ExportFormatOptions = formatOpt;
            //reportDocument.Export(ex);
             
            // reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, newPath );

            ExportOptions CrExportOptions;
            DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
            PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
            CrDiskFileDestinationOptions.DiskFileName = newPath;
            CrExportOptions = reportDocument.ExportOptions;
            {
                CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
                CrExportOptions.FormatOptions = CrFormatTypeOptions;
            }
            reportDocument.Export();


            //  reportDocument.SaveAs(newPath, true);
            //  _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.PortableDocFormat));

            //  reportDocument.SaveAs(newPath);
            //reportDocument.Export(ex);
            //reportDocument.Dispose();


            //Export to PDF
            //ExportOptions CrExportOptions;
            //DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
            //PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
            //CrDiskFileDestinationOptions.DiskFileName =  newPath ;
            //CrExportOptions = reportDocument.ExportOptions;
            //CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
            //CrExportOptions.ExportFormatType = ExportFormatType.CrystalReport;
            //CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
            //CrExportOptions.FormatOptions = CrFormatTypeOptions;
            //reportDocument.Export(CrExportOptions);




            //_contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.PortableDocFormat));

        }


        private static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read );
                }
                 
                return ms.ToArray();
            }
        }
    }

}