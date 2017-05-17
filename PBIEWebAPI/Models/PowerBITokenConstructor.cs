using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PBIEWebAPI.Models
{
    public class PowerBITokenConstructor
    {
        public string WorkspaceCollectionName { get; set; }
        public string WorkspaceId { get; set; }
        public string ReportId { get; set; }
        public string DatasetId { get; set; }
    }
}