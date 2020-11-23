## Planday Revenue Flow Demo - Console App

# Create API on Planday (skip if you already have one)
In order to use this app you need to register an API at the Planday portal.
Steps are described at the link below (follow the guide up to "Authorization code flow" section):
https://openapi.planday.com/gettingstarted/authorization#authorization-flow-for-technology-partners

# Access to Planday portal
In order to let the application access your Planday API account it's required to create a secrets.json file to provide the required credentials to the app.
To do so, in Visual Studio right click on the ConsoleApp project and select a "Manage User Secrets" action.

Inside the secrets.json file create the following structure to provide your credentials:
~~~
{
  "ApiConfiguration:RefreshToken": "<your_planday_API_access_token>",
  "ApiConfiguration:XClientId": "<your_planday_API_access_app_ID>"
}
~~~

The parameters mentioned above can be found in your Planday account in Settings > API Access tab:
* Access token - "Token" field of your API
* App ID - "App ID" field of your API

# Revenue Record Creation Flow
To create a revenue record some intermediate entities are required to be created via the Planday portal.
Follow the link to find all the requirements and flow guidelines: https://apollodigital.atlassian.net/wiki/spaces/AG/pages/133300225/Planday+API+Documentation.