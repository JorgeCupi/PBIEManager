# PBIEManager
This is a Power BI Embedded Manager UWP App built as a quick response to the needed of Azure users to create, 
modify and view Workspaces, Imports and Reports they currently have in the "Power BI Embedded" solution within Azure.
<br/><br/>
Currently, to manage a Power BI Embedded service in Azure a user needs to provision its Workspace Collection using
the Provisioning Sample found in https://github.com/Azure-Samples/power-bi-embedded-integrate-report-into-web-app 
which is a powerful Console App, but doesn't provide a friendly interface and also doesn't allow to preview a report that has been uploaded.
<br/><br/>
So as a result this project is born with the mission to faccilitate the management of a Power BI Embedded workspace collection on Azure.
<h2>Features</h2>
The app (on its current version) allows you to:
<ul>
<li>Connect to your PBIE Workspace Collection using your Access Key and Workspace Collection Name</li>
<li>Create a Workspace within your collection</li>
<li>View a Workspace's Imports list</li>
<li>Upload a PBIX File as an Import to your Workspace</li>
<li>View a list of Reports from a given Import</li>
<li>Preview your report on an embedded WebView</li>
</ul>

<h2>Wishlist</h2>
<ul>
<li>Delete a Workspace (Needs to be done via REST API as the PBI SDK doesn't support the method as of today</li>
<li>Delete an import</li>
<li>Delete a dataset or a report from a given import</li>
<li>Easy Copy/Paste interface to embed reports in a glance to your webpage</li>
<li>Token generator interface</li>
<li>Modify the connection string from a DirectLine report</li>
<li><b>Xamarin support</b></li>
</ul>

<h2>Important!</h2>
The App presented here heavily uses the Microsoft.PowerBI.API SDK, except for the Token generation.
<br/>
Currently, the App uses an Azure WebApp to generate a report Token because the Microsoft.PowerBI.Core SDK for .NET 
doesn't support UWP or Xamarin.
<br/>
Therefore, a WebApp quick sample is included in these repo to show how to generate a Token given a set of parameters. 
As far as it is possible to generate a Token via REST API in an easy way, the WebApp project should be dismissed
