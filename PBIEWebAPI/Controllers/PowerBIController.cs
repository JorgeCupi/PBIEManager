using System.Web.Http;
using Microsoft.PowerBI.Security;
using PBIEWebAPI.Models;
using System;
using System.Configuration;

namespace PBIEWebAPI.Controllers
{
    public class PowerBIController : ApiController
    {
        public string Post([FromBody]PowerBITokenConstructor constructor)
        {
            var date = DateTime.UtcNow.AddMinutes(120);
            var result = PowerBIToken.CreateReportEmbedToken(
                constructor.WorkspaceCollectionName,
                constructor.WorkspaceId,
                constructor.ReportId,
                constructor.DatasetId,
                date);

            var key = ConfigurationManager.AppSettings["PowerBIWorkspaceCollectionKey"];

            return result.Generate(key);
        }
    }
}