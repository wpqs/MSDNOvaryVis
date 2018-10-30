# MSDNOvaryVis
Source code and resources needed to build the projects described in the following MSDN Magazine articles by Will Stott. Each project is complete in its own right and the later ones build on the earlier ones.

## MSDN.OvaryVis Mag Article Dec18
### Article
* [Using Azure Containers to Provide an On-Demand R Server](http://msdn.microsoft.com/magazine/xxx) (only available after publication on 1st Dec)
### Summary: 
* Using C# with Azure.Management.Fluent and an Azure Function (timer trigger) to start and stop an Azure Container Instance provisioned from an image on Docker Hub
### Technologies discussed:
* Creating and deleting Azure Container Instances from a Docker image using C# and Azure.Management.Fluent
* Using an Azure Function to implement periodic processing
* Provisioning resources using the Azure Portal PowerShell Console
### Visual Studion Solution and Source Code:
* [MSDNOvaryVis Solution](https://github.com/wpqs/MSDNOvaryVis/tree/master/MSDN.OvaryVis%20Mag%20Article%20Dec18/MSDNOvaryVis) - extends the MSDN.OvaryVis Mag Article Nov18 Project (below)
### File containing list of commands for creating Azure Resources:
* [link]

## MSDN.OvaryVis Mag Article Nov18
### Article
* [Web Site Background Processing with Azure Service Bus Queues](http://msdn.microsoft.com/magazine/mt830371)
### Summary: 
* How to perform long-running processing for an ASP.NET Core 2.1 Web App in the background using Azure Functions and a Service Bus queue
### Technologies discussed:
* Using Azure Service Bus Queue and Azure Functions to implement background processing for a Web App
* Use of Entity Framework to provide database access to an Azure Function
* Provisioning resources using Azure Cloud Shell
### Visual Studion Solution and Source Code
* [MSDNOvaryVis Solution](https://github.com/wpqs/MSDNOvaryVis/tree/master/MSDN.OvaryVis%20Mag%20Article%20Nov18/MSDNOvaryVis) - extends the MSDN.OvaryVis BaseProject (below)
### File containing list of commands for creating Azure Resources
* [link]

The above projects were developed from a simple Web site which was built using ASP.NET 2.1 MVC WebApp with Entity Framework and SQL Server. An online only article describing how to build this website together with its source code and commands for creating the necessary Azure resources can be found below: 

## MSDN.OvaryVis BaseProject
### Article (online only)
* [ASP.NET Core 2.1 with SQL Server Deployed to Azure](https://github.com/wpqs/MSDNOvaryVis/blob/master/MSDN.OvaryVis%20BaseProject/ASPNETWebAppWithSQLServer.pdf) - download to access hyperlinks
### Summary: 
* Step-by-step instructions for building a simple ASP.NET Core 2.1 MVC Web App and SQL Database as well provisioning the resources needed to publish them to the Azure Cloud
### Technologies discussed
* Creating an ASP.NET Core 2.1 MVC Web App
* Use of Entity Framework Core to create a database for the Web App and provide the required access
* Provisioning resources for the Web App and SQL Server database using Azure Portal Powershell Console
### Visual Studion Solution and Source Code
* [MSDNOvaryVis Solution](https://github.com/wpqs/MSDNOvaryVis/tree/master/MSDN.OvaryVis%20BaseProject/MSDNOvaryVis)
### File containing list of commands for creating Azure Resources
* [link]
