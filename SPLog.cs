using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LITC.Commons
{
    public class SPLog : SPDiagnosticsServiceBase
    {
        public static string DiagnosticAreaName = "MyDebug";
        private static SPLog _Current;
        public static SPLog Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new SPLog();
                }

                return _Current;
            }
        }

        public SPLog() : base("LiderIT Logging Service", SPFarm.Local)
        {

        }

        public enum Category
        {
            Unexpected,
            High,
            Medium,
            Information
        }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            List<SPDiagnosticsArea> areas = new List<SPDiagnosticsArea>
        {
            new SPDiagnosticsArea(DiagnosticAreaName, new List<SPDiagnosticsCategory>
            {
                new SPDiagnosticsCategory("Unexpected", TraceSeverity.Unexpected, EventSeverity.Error),
                new SPDiagnosticsCategory("High", TraceSeverity.High, EventSeverity.Warning),
                new SPDiagnosticsCategory("Medium", TraceSeverity.Medium, EventSeverity.Information),
                new SPDiagnosticsCategory("Information", TraceSeverity.Verbose, EventSeverity.Information)
            })
        };

            return areas;
        }

        public static void WriteLog(Category categoryName, string message)
        {
            SPDiagnosticsCategory category = SPLog.Current.Areas[DiagnosticAreaName].Categories[categoryName.ToString()];
            SPLog.Current.WriteTrace(0, category, category.TraceSeverity, string.Concat(message));
        }

        public static void WriteLog(Category categoryName, string source, string errorMessage)
        {
            SPDiagnosticsCategory category = SPLog.Current.Areas[DiagnosticAreaName].Categories[categoryName.ToString()];
            SPLog.Current.WriteTrace(0, category, category.TraceSeverity, string.Concat(source, ": ", errorMessage));
        }
    }
}
