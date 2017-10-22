using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Objects
{
    public class AnalysisResults
    {
        public TimeSpan Span { get; set; }
        public String File { get; set; }
        public Boolean Error { get; set; }

        public String ErrorMessage { get; set; }

        public AnalysisResults()
        {
            Error = false;
        }
    }
}
