# Example: Generated Application Structure

When you run `semprus scaffold MyApp`, the tool generates a complete Blazor application with the following structure:

## File Structure

```
MyApp/
├── Contracts/                          # Smart Contract Files
│   ├── ItemRegistry.sol                # Solidity smart contract
│   └── ItemRegistry.abi                # Contract ABI (generated)
│
├── Services/                           # Business Logic Layer
│   ├── Item.cs                         # Item model/entity
│   └── ItemRegistryService.cs          # Nethereum service wrapper
│
├── Components/
│   ├── Pages/                          # Blazor CRUD Pages
│   │   ├── Items.razor                 # List all items
│   │   ├── CreateItem.razor            # Create new item
│   │   ├── EditItem.razor              # Edit existing item
│   │   ├── Home.razor                  # Default home page
│   │   ├── Counter.razor               # Sample counter page
│   │   ├── Weather.razor               # Sample weather page
│   │   └── Error.razor                 # Error page
│   │
│   ├── Layout/                         # Layout Components
│   │   ├── MainLayout.razor            # Main layout
│   │   ├── NavMenu.razor               # Navigation menu (updated with Items link)
│   │   └── NavMenu.razor.css           # Navigation styles
│   │
│   ├── App.razor                       # Root component
│   ├── Routes.razor                    # Routing configuration
│   └── _Imports.razor                  # Global imports
│
├── Properties/
│   └── launchSettings.json             # Launch profiles
│
├── wwwroot/                            # Static files
│   ├── app.css                         # Application styles
│   ├── bootstrap/                      # Bootstrap framework
│   └── favicon.png                     # Favicon
│
├── appsettings.json                    # Configuration (with Blockchain section)
├── appsettings.Development.json        # Development configuration
├── Program.cs                          # Application entry point (updated)
├── MyApp.csproj                        # Project file (with Nethereum packages)
└── README.md                           # Application-specific README
```

## Generated Code Examples

### 1. Smart Contract (ItemRegistry.sol)

```solidity
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract ItemRegistry {
    struct Item {
        uint256 id;
        string name;
        string description;
        address owner;
        uint256 createdAt;
    }
    
    mapping(uint256 => Item) public items;
    uint256 public itemCount;
    
    function createItem(string memory name, string memory description) 
        public returns (uint256) {
        itemCount++;
        items[itemCount] = Item(itemCount, name, description, 
                               msg.sender, block.timestamp);
        emit ItemCreated(itemCount, name, msg.sender);
        return itemCount;
    }
    
    // Additional functions: getItem, updateItem, deleteItem, getAllItems
    // Events: ItemCreated, ItemUpdated, ItemDeleted
}
```

### 2. Item Model (Item.cs)

```csharp
namespace Services;

public class Item
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public ulong CreatedAt { get; set; }
}
```

### 3. Service Layer (ItemRegistryService.cs)

```csharp
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Services;

public class ItemRegistryService
{
    private readonly Web3 _web3;
    private readonly Contract _contract;

    public ItemRegistryService(string nodeUrl, string contractAddress, string privateKey)
    {
        // Initialize Web3 and contract...
    }

    public async Task<ulong> CreateItemAsync(string name, string description)
    {
        var createFunction = _contract.GetFunction("createItem");
        var receipt = await createFunction.SendTransactionAndWaitForReceiptAsync(...);
        return itemId;
    }

    // Additional methods: GetItemAsync, GetAllItemsAsync, 
    //                     UpdateItemAsync, DeleteItemAsync
}
```

### 4. Blazor CRUD Page (Items.razor)

```razor
@page "/items"
@using Services
@inject ItemRegistryService ItemService

<PageTitle>Items</PageTitle>

<h1>Items</h1>
<p>Manage your blockchain items below.</p>

<p>
    <a href="/items/create" class="btn btn-primary">Create New Item</a>
</p>

<table class="table">
    <thead>
        <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Description</th>
            <th>Owner</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in items)
        {
            <tr>
                <td>@item.Id</td>
                <td>@item.Name</td>
                <td>@item.Description</td>
                <td>@item.Owner[..8]...</td>
                <td>
                    <a href="/items/edit/@item.Id">Edit</a>
                    <button @onclick="() => DeleteItem(item.Id)">Delete</button>
                </td>
            </tr>
        }
    </tbody>
</table>

@code {
    private List<Item>? items;

    protected override async Task OnInitializedAsync()
    {
        items = await ItemService.GetAllItemsAsync();
    }

    private async Task DeleteItem(ulong id)
    {
        await ItemService.DeleteItemAsync(id);
        await LoadItems();
    }
}
```

### 5. Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Blockchain": {
    "NodeUrl": "http://localhost:8545",
    "ContractAddress": "0x0000000000000000000000000000000000000000",
    "PrivateKey": ""
  }
}
```

### 6. Dependency Injection (Program.cs - added section)

```csharp
using Services;

// ...

// Register blockchain service
var nodeUrl = builder.Configuration["Blockchain:NodeUrl"] ?? "http://localhost:8545";
var contractAddress = builder.Configuration["Blockchain:ContractAddress"] ?? "0x0000000000000000000000000000000000000000";
var privateKey = builder.Configuration["Blockchain:PrivateKey"] ?? "";
builder.Services.AddSingleton(new ItemRegistryService(nodeUrl, contractAddress, privateKey));

app.Run();
```

## NuGet Packages Included

The generated project includes these Nethereum packages:

- `Nethereum.Web3` - Core Web3 functionality
- `Nethereum.Contracts` - Smart contract interaction
- `Nethereum.ABI` - ABI encoding/decoding
- `Nethereum.Hex` - Hexadecimal utilities

## Features Included

✅ **Complete CRUD Operations**
- Create items with blockchain transactions
- Read items from smart contract
- Update items (owner-only)
- Delete items (owner-only)

✅ **Security Features**
- Owner verification in smart contract
- Private key management
- Transaction signing

✅ **User Interface**
- Responsive Bootstrap styling
- Real-time UI updates
- Form validation
- Error handling

✅ **Developer Experience**
- Fully commented code
- README with deployment instructions
- Configuration templates
- Ready to build and run

## Build and Run

```bash
cd MyApp
dotnet build    # Compiles successfully
dotnet run      # Starts the application
```

## What Makes This Different?

Unlike other scaffolding tools, Semprus generates:

1. **Decentralized Backend**: Data stored on blockchain, not in a database
2. **No Dependencies on Third Parties**: No Infura, no hosted services
3. **100% .NET Stack**: Pure Microsoft technologies (except Nethereum)
4. **Production Ready**: Includes error handling, validation, and security
5. **Censorship Resistant**: Data cannot be deleted or modified by central authority

## Next Steps After Scaffolding

1. Deploy the smart contract to a blockchain network
2. Update the configuration with your contract address
3. Customize the Item model for your use case
4. Add authentication and authorization
5. Deploy to Azure or other cloud platform
6. Add more entities and relationships

---

This is just the beginning. The scaffolded application is a fully functional starting point that you can extend and customize for your specific needs!
