using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Api.V1.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PBIEMobile
{
    public static class PBIEClient
    {
        static private PowerBIClient PbiClient { get; set; }
        static string Key { get; set; }
        static string WorkspaceCollectionName { get; set; }
        static string WorkspaceId { get; set; }
        static string WorkspaceUri = "https://api.powerbi.com/v1.0/collections/{0}/workspaces/";
        static string ImportsUri = "/{0}/imports";

        /// <summary>
        /// Initialize a PBI Embedded Client to use the methods in this class
        /// </summary>
        /// <param name="key">The Primary Key for this Power BI Embedded Workspace Collection</param>
        /// <param name="workspaceCollectionName">The Name from this Power BI Embedded Workspace Collection</param>
        public static void InitializePBIEClient(string key, 
            string workspaceCollectionName)
        {
            Key = key;
            WorkspaceCollectionName = workspaceCollectionName;

            WorkspaceUri = String.Format(WorkspaceUri, workspaceCollectionName);
            ImportsUri = WorkspaceUri + ImportsUri;

            var credentials = new TokenCredentials(Key, "AppKey");
            PbiClient = new PowerBIClient(credentials);
        }

        #region Workspaces
        /// <summary>
        /// Obtains a workspace list from the current workspace collection
        /// </summary>
        /// <returns>Returns an IList of  Workspace objects</returns>
        public static async Task<IList<Workspace>> GetWorkspaces()
        {
            var result = await PbiClient.Workspaces.
                GetWorkspacesByCollectionNameAsync(WorkspaceCollectionName);
            return result.Value;
        }

        /// <summary>
        /// Create a workspace within the current workspace collection given a new workspace name
        /// </summary>
        /// <param name="workspaceName">The name that will be assigned to the new workspace</param>
        /// <returns>Returns a Workspace object</returns>
        public static async Task<Workspace> CreateWorkspace(string workspaceName)
        {
            var result = await PbiClient.Workspaces.
                PostWorkspaceAsync(WorkspaceCollectionName, new CreateWorkspaceRequest(workspaceName));
            return result;
        }

        //public static async void DeleteWorkspace(string workspaceId)
        //{
        //    //var result = await PbiClient.DeleteAsync(WorkspaceUri + "(" + WorkspaceId+")");
        //}
        #endregion

        #region Datasets
        //public static async Task<List<Dataset>> GetDatasets(string workspaceId)
        //{
        //    var result = await pbiClient.Datasets.
        //        GetDatasetsAsync(WorkspaceCollectionName, workspaceId);
        //    return result.Value;
        //}

        //public static async Task<string> DeleteDataset(string workspaceId,
        //    string datasetId)
        //{
        //    var result = await pbiClient.Datasets.
        //        DeleteDatasetByIdAsync(WorkspaceCollectionName, workspaceId,
        //        datasetId);
        //    return result.ToString();
        //}

        //public static async Task<String> UpdateConnectionString(
        //    string datasetId,
        //    string connectionString)
        //{
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "connectionString", connectionString }
        //    };

        //    var result = await pbiClient.Datasets.
        //        SetAllConnectionsAsync(WorkspaceCollectionName, WorkspaceId,
        //        datasetId, parameters);
        //    return result.ToString();
        //}

        //public static async Task<string> UpdateCredentials(
        //    string datasetId, string userName, string password)
        //{
        //    var dataSources = await pbiClient.Datasets.
        //        GetGatewayDatasourcesAsync(WorkspaceCollectionName,
        //        WorkspaceId, datasetId);

        //    var credentials = new GatewayDatasource
        //    {
        //        CredentialType = "Basic",
        //        BasicCredentials = new BasicCredentials
        //        {
        //            Username = userName,
        //            Password = password
        //        }
        //    };

        //    int dataSourcesCount = dataSources.Value.Count;
        //    if (dataSourcesCount == 0)
        //    {
        //        return "There are no datasources in this dataset";
        //    }
        //    else
        //    {
        //        await pbiClient.Gateways.
        //            PatchDatasourceAsync(WorkspaceCollectionName,
        //            WorkspaceId, dataSources.Value[0].GatewayId,
        //            dataSources.Value[0].Id, credentials);
        //        if (dataSourcesCount == 1)
        //            return "The connection credentials were updated successfully";
        //        else return "The connection credentials were updated for the first DataSource in the current dataset";
        //    }

        //}
        #endregion

        #region Reports
        //public static async Task<Report> CloneReport(string reportId, 
        //    string reportName, string targetWorkspaceId)
        //{
        //    var request = new CloneReportRequest();
        //    request.Name = reportName;
        //    request.TargetWorkspaceId = targetWorkspaceId;
        //    var result = await pbiClient.Reports.
        //        CloneReportAsync(WorkspaceCollectionName, WorkspaceId, 
        //        reportId, request);
        //    return result;
        //}

        //public static async Task<string> DeleteReport(string reportId)
        //{
        //    var result = await pbiClient.Reports.
        //        DeleteReportAsync(WorkspaceCollectionName, WorkspaceId, reportId);
        //    return result.ToString();
        //}

        /// <summary>
        /// Method that generates an HTML string ready to embed a report given its Workspace Collection, ReportID and DataSetID
        /// The token for the embedded part is being generated in an Azure WebAPI given that the PowerBI.Core SDK for .NET is not supported in UWP Apps.
        /// </summary>
        /// <param name="Report">Report Object containing IDs needed for the method</param>
        /// <param name="DatasetId">Dataset ID from the Import containing the Report</param>
        /// <returns>Returns an HTML string</returns>
        public static async Task<string> LoadReport(Report Report,string DatasetId)
        {
            string token = await PBIEClient.GetToken(Report.Id, DatasetId);
            token = token.Replace("\"","");
            return PBIEClient.GetHtmlToNavigate(Report.Id, Report.EmbedUrl, token);
        }
        #endregion

        #region PBIX Files
        /// <summary>
        /// Obtains an Imports list from the current workspace collection
        /// </summary>
        /// <returns>Returns a List of Import objects</returns>
        public static async Task<IList<Import>> GetImports()
        {
            var result = await PbiClient.Imports.GetImportsAsync(WorkspaceCollectionName,WorkspaceId);
            return result.Value;
        }

        /// <summary>
        /// Method to Upload an import given a Stream representing the PBIX file and a Dataset name
        /// </summary>
        /// <param name="DatasetName">Dataset name for the Import that will be created</param>
        /// <param name="Stream">Stream representing the PBIX File that needs to be uploaded</param>
        /// <returns></returns>
        public static async Task<Import> UploadImport(string DatasetName, Stream Stream)
        {
            var result = await PbiClient.Imports.
                PostImportWithFileAsync(WorkspaceCollectionName, WorkspaceId, Stream, DatasetName);
            return result;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Utility method that Sets a WorkspaceID for the PBIClient when a new Workspace is selected
        /// </summary>
        /// <param name="workspaceId"></param>
        public static void SetWorkspaceId(string workspaceId)
        {
            WorkspaceId = workspaceId;
            ImportsUri = String.Format(ImportsUri, workspaceId);
        }

        /// <summary>
        /// Method that generates HTML to embed a report given a ReportId, ReportEmbedUrl and a Token
        /// </summary>
        /// <param name="ReportId">ID from the report that needs to be embedded</param>
        /// <param name="ReportEmbedUrl">URL from the report that needs to be embedded</param>
        /// <param name="Token">Token to embed the report</param>
        /// <returns></returns>
        private static string GetHtmlToNavigate(string ReportId, string ReportEmbedUrl, string Token)
        {
            string html = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><head><head><body>" +
                    $"<iframe src=\"{ReportEmbedUrl}\" name=\"{Token}\" id=\"ifrTile\" height=\"480\" width=\"640\"></iframe>" +
                    "<script>" +
                    "(function() {" +
                    "var iframe = document.getElementById('ifrTile');" +
                    $"iframe.src = \"{ReportEmbedUrl}\";" +
                    "iframe.onload = function() {" +
                    "var msgJson = {" +
                    "action: \"loadReport\"," +
                    $"accessToken: \"{Token}\"" +
                    "};" +
                    "var msgTxt = JSON.stringify(msgJson);" +
                    "iframe.contentWindow.postMessage(msgTxt, \"*\");" +
                    "};" +
                    "}());" +
                    "</script></body>";
            return html;
        }

        /// <summary>
        /// Method that generates a Token for a Report given its ReportId and its DatasetId
        /// </summary>
        /// <param name="ReportId">ID from the report that needs the Token to be generated</param>
        /// <param name="DatasetId">DatasetID from the report that needs the Token to be generated</param>
        /// <returns></returns>
        private static async Task<string> GetToken(string ReportId,string DatasetId)
        {
            HttpClient client = new HttpClient();
            String body = "{"+
                "\"WorkspaceCollectionName\":\""+WorkspaceCollectionName+"\"," +
	            "\"WorkspaceId\":\""+WorkspaceId+"\"," +
	            "\"ReportId\":\""+ReportId+"\"," +
	            "\"DatasetId\":\""+DatasetId+"\"" +
                "}";
            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            var result = await client.PostAsync("http://pbiewebapi.azurewebsites.net/api/powerbi", content);
            var token = await result.Content.ReadAsStringAsync();
            return token;
        }
        #endregion
    }
} 