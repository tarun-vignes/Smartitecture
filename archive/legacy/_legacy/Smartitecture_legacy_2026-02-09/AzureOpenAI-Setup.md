# Azure OpenAI Service Setup for Smartitecture

This guide provides instructions for setting up and configuring the Azure OpenAI service in the Smartitecture application.

## Prerequisites

- An Azure subscription
- Access to Azure OpenAI Service
- Azure CLI installed (optional)

## Configuration

### 1. Create an Azure OpenAI Resource

1. Sign in to the [Azure portal](https://portal.azure.com/)
2. Click on "Create a resource"
3. Search for "Azure OpenAI" and select it
4. Click "Create"
5. Fill in the required information:
   - Subscription: Select your Azure subscription
   - Resource group: Select an existing one or create a new one
   - Region: Select a region where Azure OpenAI is available
   - Name: Choose a name for your resource
   - Pricing tier: Select a pricing tier (S0 is recommended for development)
6. Click "Review + create" and then "Create"

### 2. Get Your API Key and Endpoint

1. Go to your Azure OpenAI resource in the Azure portal
2. Under "Resource Management", click on "Keys and Endpoint"
3. Copy the following values:
   - Endpoint (e.g., `https://your-resource-name.openai.azure.com/`)
   - Key 1 or Key 2 (either will work)

### 3. Deploy a Model

1. In your Azure OpenAI resource, go to "Model deployments" under "Resource Management"
2. Click "Manage deployments"
3. Click "Create new deployment"
4. Select a model (e.g., `gpt-4` or `gpt-35-turbo`)
5. Enter a deployment name (e.g., `gpt-4`)
6. Click "Create"

### 4. Update appsettings.json

Update the `appsettings.json` file with your Azure OpenAI settings:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "your-deployment-name",
    "MaxTokens": 1000,
    "Temperature": 0.2
  },
  "Agent": {
    "WeatherApiKey": "YOUR_WEATHER_API_KEY",
    "SearchApiKey": "YOUR_SEARCH_API_KEY",
    "SearchEngineId": "YOUR_SEARCH_ENGINE_ID"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Environment Variables (Alternative to appsettings.json)

As an alternative to storing sensitive information in `appsettings.json`, you can use environment variables:

```bash
# Windows (Command Prompt)
setx AzureOpenAI__Endpoint "https://your-resource-name.openai.azure.com/"
setx AzureOpenAI__ApiKey "your-api-key-here"
setx AzureOpenAI__DeploymentName "your-deployment-name"

# Windows (PowerShell)
[System.Environment]::SetEnvironmentVariable("AzureOpenAI__Endpoint", "https://your-resource-name.openai.azure.com/", "User")
[System.Environment]::SetEnvironmentVariable("AzureOpenAI__ApiKey", "your-api-key-here", "User")
[System.Environment]::SetEnvironmentVariable("AzureOpenAI__DeploymentName", "your-deployment-name", "User")

# Linux/macOS
export AzureOpenAI__Endpoint="https://your-resource-name.openai.azure.com/"
export AzureOpenAI__ApiKey="your-api-key-here"
export AzureOpenAI__DeploymentName="your-deployment-name"
```

## Testing the Setup

1. Build and run the Smartitecture application
2. Navigate to the Agent page
3. Send a test message to verify that the Azure OpenAI service is working correctly

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Verify that your API key is correct
   - Ensure the API key hasn't expired
   - Check that the endpoint URL is correct

2. **404 Not Found**
   - Verify that the deployment name is correct
   - Ensure the model deployment exists in the specified region

3. **Rate Limiting**
   - Check your Azure OpenAI service quotas and limits
   - Consider upgrading your pricing tier if needed

### Logging

Check the application logs for detailed error messages. The log level can be adjusted in the `appsettings.json` file.

## Security Best Practices

- Never commit sensitive information like API keys to version control
- Use Azure Key Vault for production environments
- Implement proper access controls and RBAC for your Azure resources
- Regularly rotate your API keys
- Monitor usage and set up alerts for unusual activity

## Additional Resources

- [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Azure OpenAI Samples](https://github.com/Azure/openai-samples)
- [Azure OpenAI Best Practices](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/concepts/best-practices)
