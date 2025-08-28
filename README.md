# POE-CLDV6212

# ABC Retailers - ASP.NET Core MVC Application

## Overview

ABC Retailers is a comprehensive e-commerce management system built with ASP.NET Core MVC that integrates with Azure Storage services. The application provides complete CRUD operations for customers, products, and orders, along with file upload capabilities for product images and payment proofs.

## Features

- **Dashboard**: Overview with statistics and featured products
- **Customer Management**: Full CRUD operations for customer data
- **Product Catalog**: Manage products with image upload capabilities
- **Order Processing**: Create and manage orders with real-time pricing
- **File Upload**: Support for payment proof documentation
- **Azure Integration**: Seamless integration with Azure Storage services

## Technology Stack

- **Framework**: ASP.NET Core MVC
- **Storage**: Azure Storage (Tables, Blobs, Queues, File Shares)
- **Frontend**: Bootstrap, Font Awesome icons
- **Validation**: Client-side and server-side validation

## Project Structure

```
ABCRetailers/
├── Controllers/
│   ├── HomeController.cs
│   ├── CustomerController.cs
│   ├── ProductController.cs
│   ├── OrderController.cs
│   └── UploadController.cs
├── Models/
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── FileUploadModel.cs
│   └── ErrorViewModel.cs
├── ViewModels/
│   ├── HomeViewModel.cs
│   └── OrderCreateViewModel.cs
├── Services/
│   ├── IAzureStorageService.cs
│   └── AzureStorageService.cs
├── Views/
│   ├── Shared/
│   ├── Home/
│   ├── Customer/
│   ├── Product/
│   ├── Order/
│   └── Upload/
├── appsettings.json
└── Program.cs
```

## Setup Instructions

### Prerequisites

1. .NET 6.0 SDK or later
2. Azure account with Storage service
3. IDE (Visual Studio, VS Code, or Rider)

### Configuration

1. **Clone or create the project**:
   ```bash
   mkdir ABCRetailers
   cd ABCRetailers
   dotnet new mvc --auth Individual
   ```

2. **Install required NuGet packages**:
   ```bash
   dotnet add package Azure.Data.Tables
   dotnet add package Azure.Storage.Blobs
   dotnet add package Azure.Storage.Queues
   dotnet add package Azure.Storage.Files.Shares
   ```

3. **Configure Azure Storage**:
   - Create an Azure Storage account in the Azure portal
   - Retrieve your connection string from the "Access Keys" section
   - Update the `appsettings.json` file with your connection string:
   ```json
   {
     "ConnectionStrings": {
       "AzureStorage": "Your_Connection_String_Here"
     }
   }
   ```

4. **Register the service**:
   - In `Program.cs`, add the following line:
   ```csharp
   builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();
   ```

5. **Run the application**:
   ```bash
   dotnet run
   ```

## Azure Storage Configuration

The application automatically creates the following Azure resources on first run:

### Tables
- Customers
- Products
- Orders

### Blob Containers
- product-images (for product photos)
- payment-proofs (for payment documentation)

### Queues
- orders (for order processing messages)
- notifications (for system notifications)

### File Shares
- contracts (for document storage)

## Usage Guide

### Managing Customers
1. Navigate to the Customers section
2. Click "Add New Customer" to create new customer records
3. Use Edit/Delete actions with confirmation modals

### Managing Products
1. Go to the Products section
2. Add products with images using the upload functionality
3. Stock levels are displayed with color-coded indicators

### Processing Orders
1. Create orders using dropdown selections for customers and products
2. Real-time pricing and stock validation
3. Update order status through the Edit functionality

### File Uploads
1. Use the Upload section to submit payment proofs
2. Files are validated for type and size
3. Optional Order ID and Customer Name fields for organization

## API Endpoints

### Order Controller
- `GET /Order/GetProductPrice?productId={id}` - Returns JSON with product price and stock

## Validation Features

- Client-side validation for forms
- File type validation for uploads
- Stock validation during order creation
- Real-time price calculations

## Error Handling

- Global error handling middleware
- Custom error pages
- Validation error messages

## Deployment

### To Azure App Service
1. Right-click project → Publish
2. Select Azure → Azure App Service
3. Create new or select existing App Service
4. Deploy

### Using Azure DevOps
1. Create build pipeline with .NET Core template
2. Add Azure App Service deploy task
3. Configure connection strings in Azure portal

## Troubleshooting

### Common Issues

1. **Azure Connection Errors**:
   - Verify connection string in appsettings.json
   - Check firewall settings in Azure Storage account

2. **File Upload Issues**:
   - Verify container permissions in Azure Blob Storage
   - Check file size limits

3. **Table Operations Failing**:
   - Verify table names and partition keys match Azure Storage

### Logs

- Application logs are configured in appsettings.json
- Azure Storage client library provides detailed error messages

## Security Considerations

- Always use HTTPS in production
- Validate all user inputs
- Implement proper authentication/authorization
- Secure Azure Storage connection strings
- Regularly rotate storage account keys

## Support

For issues related to:
- Azure configuration: Check Azure documentation
- Application code: Review provided implementation
- Deployment: Refer to Azure App Service documentation

## License

This project is provided as a sample implementation. Please ensure proper licensing for production use.

