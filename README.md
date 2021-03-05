# ProgrammaticExportSampleAppMPN

This is a sample app which shocases how to integrate with the Microsoft MPN partner analytics **Programmatic Access APIs**. The APIs allow you to easily schedule custom reports and ingest key data sets into your internal analytics systems.

## Prerequisites
None

## What does this sample app do?
- The main aim of this app is to demonstrate how the **Programmatic Access APIs** can be programmatically called.
- This is a very simple .NET core app which exposes a controller and a simple UI to call the controllers.
- You can check the controller code to see how **Programmatic Access APIs** need to be called.
- As of now, you can do the following in the sample app:
    1. Schedule reports
    2. View system queries and user defined queries
    3. Create custom queries
    4. Create custom queries

## Steps to run the app locally:
- Open the appsettings.Development.json file and provide the following parameters:
    1. TenantId
    2. WebAppClientId
    3. WebAppClientSecret
    4. Username
    5. Password
- Run the project from Visual Studio or go to the root directory and run this command "dotnet run".
- Open the URL https://localhost:44365/ProgrammaticExportSampleAppMPN/sample
- To start exploring the datasets, click on "Get Available Datasets"
- To frame your own query, select the columns, define conditions and ordering and then click on generate query to see the full report query.
- Cliking on create query button will create the query