using System.Diagnostics;

namespace Semprus.CLI;

public class AppScaffolder
{
    public void Scaffold(string appName, string outputPath)
    {
        string appPath = Path.Combine(outputPath, appName);
        
        if (Directory.Exists(appPath))
        {
            throw new InvalidOperationException($"Directory '{appName}' already exists.");
        }

        Console.WriteLine("Creating Blazor Server application...");
        CreateBlazorApp(appName, outputPath);
        
        Console.WriteLine("Adding Nethereum packages...");
        AddNethereumPackages(appPath);
        
        Console.WriteLine("Creating smart contract...");
        CreateSmartContract(appPath);
        
        Console.WriteLine("Creating contract services...");
        CreateContractServices(appPath);
        
        Console.WriteLine("Creating CRUD pages...");
        CreateCrudPages(appPath, appName);
        
        Console.WriteLine("Updating navigation...");
        UpdateNavigation(appPath);
        
        Console.WriteLine("Creating configuration files...");
        CreateConfiguration(appPath);
        
        Console.WriteLine("Creating README...");
        CreateReadme(appPath, appName);
    }

    private void CreateBlazorApp(string appName, string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"new blazor -n {appName} -f net8.0 --interactivity Server",
            WorkingDirectory = outputPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start dotnet process.");
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to create Blazor application. Error: {error}\nOutput: {output}");
        }
    }

    private void AddNethereumPackages(string appPath)
    {
        var packages = new[]
        {
            "Nethereum.Web3",
            "Nethereum.Contracts",
            "Nethereum.ABI",
            "Nethereum.Hex"
        };

        foreach (var package in packages)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add package {package}",
                WorkingDirectory = appPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
    }

    private void CreateSmartContract(string appPath)
    {
        string contractsPath = Path.Combine(appPath, "Contracts");
        Directory.CreateDirectory(contractsPath);

        string contractContent = @"// SPDX-License-Identifier: MIT
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
    
    event ItemCreated(uint256 id, string name, address owner);
    event ItemUpdated(uint256 id, string name);
    event ItemDeleted(uint256 id);
    
    function createItem(string memory name, string memory description) public returns (uint256) {
        itemCount++;
        items[itemCount] = Item(itemCount, name, description, msg.sender, block.timestamp);
        emit ItemCreated(itemCount, name, msg.sender);
        return itemCount;
    }
    
    function getItem(uint256 id) public view returns (Item memory) {
        require(id > 0 && id <= itemCount, ""Item does not exist"");
        return items[id];
    }
    
    function updateItem(uint256 id, string memory name, string memory description) public {
        require(id > 0 && id <= itemCount, ""Item does not exist"");
        require(items[id].owner == msg.sender, ""Not the owner"");
        
        items[id].name = name;
        items[id].description = description;
        emit ItemUpdated(id, name);
    }
    
    function deleteItem(uint256 id) public {
        require(id > 0 && id <= itemCount, ""Item does not exist"");
        require(items[id].owner == msg.sender, ""Not the owner"");
        
        delete items[id];
        emit ItemDeleted(id);
    }
    
    function getAllItems() public view returns (Item[] memory) {
        Item[] memory allItems = new Item[](itemCount);
        uint256 counter = 0;
        
        for (uint256 i = 1; i <= itemCount; i++) {
            if (items[i].id != 0) {
                allItems[counter] = items[i];
                counter++;
            }
        }
        
        return allItems;
    }
}";

        File.WriteAllText(Path.Combine(contractsPath, "ItemRegistry.sol"), contractContent);

        // Create ABI file (would normally be generated by Solidity compiler)
        string abiContent = @"[{""inputs"":[{""internalType"":""string"",""name"":""name"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""}],""name"":""createItem"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""}],""name"":""deleteItem"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""indexed"":false,""internalType"":""string"",""name"":""name"",""type"":""string""},{""indexed"":false,""internalType"":""address"",""name"":""owner"",""type"":""address""}],""name"":""ItemCreated"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint256"",""name"":""id"",""type"":""uint256""}],""name"":""ItemDeleted"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""indexed"":false,""internalType"":""string"",""name"":""name"",""type"":""string""}],""name"":""ItemUpdated"",""type"":""event""},{""inputs"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""internalType"":""string"",""name"":""name"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""}],""name"":""updateItem"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""getAllItems"",""outputs"":[{""components"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""internalType"":""string"",""name"":""name"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""},{""internalType"":""address"",""name"":""owner"",""type"":""address""},{""internalType"":""uint256"",""name"":""createdAt"",""type"":""uint256""}],""internalType"":""struct ItemRegistry.Item[]"",""name"":"""",""type"":""tuple[]""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""}],""name"":""getItem"",""outputs"":[{""components"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""internalType"":""string"",""name"":""name"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""},{""internalType"":""address"",""name"":""owner"",""type"":""address""},{""internalType"":""uint256"",""name"":""createdAt"",""type"":""uint256""}],""internalType"":""struct ItemRegistry.Item"",""name"":"""",""type"":""tuple""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""itemCount"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""items"",""outputs"":[{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""internalType"":""string"",""name"":""name"",""type"":""string""},{""internalType"":""string"",""name"":""description"",""type"":""string""},{""internalType"":""address"",""name"":""owner"",""type"":""address""},{""internalType"":""uint256"",""name"":""createdAt"",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""}]";

        File.WriteAllText(Path.Combine(contractsPath, "ItemRegistry.abi"), abiContent);
    }

    private void CreateContractServices(string appPath)
    {
        string servicesPath = Path.Combine(appPath, "Services");
        Directory.CreateDirectory(servicesPath);

        // Create Item model
        string itemModelContent = @"namespace Services;

public class Item
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public ulong CreatedAt { get; set; }
}";
        File.WriteAllText(Path.Combine(servicesPath, "Item.cs"), itemModelContent);

        // Create contract service - read ABI from file
        string contractServiceContent = @"using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using System.Numerics;

namespace Services;

public class ItemRegistryService
{
    private readonly Web3 _web3;
    private readonly Contract _contract;
    private readonly string _contractAddress;

    public ItemRegistryService(string nodeUrl, string contractAddress, string privateKey)
    {
        _contractAddress = contractAddress;
        
        if (!string.IsNullOrEmpty(privateKey))
        {
            var account = new Account(privateKey);
            _web3 = new Web3(account, nodeUrl);
        }
        else
        {
            _web3 = new Web3(nodeUrl);
        }

        string abi = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""Contracts"", ""ItemRegistry.abi""));
        _contract = _web3.Eth.GetContract(abi, contractAddress);
    }

    public async Task<ulong> CreateItemAsync(string name, string description)
    {
        var createFunction = _contract.GetFunction(""createItem"");
        var receipt = await createFunction.SendTransactionAndWaitForReceiptAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            name,
            description
        );

        // Get the item count to determine the new item ID
        var itemCountFunction = _contract.GetFunction(""itemCount"");
        var itemCount = await itemCountFunction.CallAsync<ulong>();
        
        return itemCount;
    }

    public async Task<Item?> GetItemAsync(ulong id)
    {
        try
        {
            var getFunction = _contract.GetFunction(""getItem"");
            var result = await getFunction.CallDeserializingToObjectAsync<ItemDTO>(id);
            
            return new Item
            {
                Id = (ulong)(BigInteger)result.Id,
                Name = result.Name,
                Description = result.Description,
                Owner = result.Owner,
                CreatedAt = (ulong)(BigInteger)result.CreatedAt
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        var getAllFunction = _contract.GetFunction(""getAllItems"");
        var items = await getAllFunction.CallDeserializingToObjectAsync<List<ItemDTO>>();
        
        return items
            .Where(i => i.Id != null && (BigInteger)i.Id > 0)
            .Select(i => new Item
            {
                Id = (ulong)(BigInteger)i.Id!,
                Name = i.Name ?? string.Empty,
                Description = i.Description ?? string.Empty,
                Owner = i.Owner ?? string.Empty,
                CreatedAt = (ulong)(BigInteger)(i.CreatedAt ?? 0)
            })
            .ToList();
    }

    public async Task UpdateItemAsync(ulong id, string name, string description)
    {
        var updateFunction = _contract.GetFunction(""updateItem"");
        await updateFunction.SendTransactionAndWaitForReceiptAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            id,
            name,
            description
        );
    }

    public async Task DeleteItemAsync(ulong id)
    {
        var deleteFunction = _contract.GetFunction(""deleteItem"");
        await deleteFunction.SendTransactionAndWaitForReceiptAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            id
        );
    }
}

// DTO for Nethereum deserialization
public class ItemDTO
{
    public object? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public object? CreatedAt { get; set; }
}";
        File.WriteAllText(Path.Combine(servicesPath, "ItemRegistryService.cs"), contractServiceContent);
    }

    private void CreateCrudPages(string appPath, string appName)
    {
        string pagesPath = Path.Combine(appPath, "Components", "Pages");
        
        // Create Items list page
        string itemsPageContent = @"@page ""/items""
@using Services
@inject ItemRegistryService ItemService

<PageTitle>Items</PageTitle>

<h1>Items</h1>

<p>Manage your blockchain items below.</p>

<p>
    <a href=""/items/create"" class=""btn btn-primary"">Create New Item</a>
</p>

@if (items == null)
{
    <p><em>Loading...</em></p>
}
else if (!items.Any())
{
    <p><em>No items found. Create your first item!</em></p>
}
else
{
    <table class=""table"">
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
                        <a href=""/items/edit/@item.Id"" class=""btn btn-sm btn-warning"">Edit</a>
                        <button class=""btn btn-sm btn-danger"" @onclick=""() => DeleteItem(item.Id)"">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<Item>? items;

    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }

    private async Task LoadItems()
    {
        try
        {
            items = await ItemService.GetAllItemsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($""Error loading items: {ex.Message}"");
            items = new List<Item>();
        }
    }

    private async Task DeleteItem(ulong id)
    {
        try
        {
            await ItemService.DeleteItemAsync(id);
            await LoadItems();
        }
        catch (Exception ex)
        {
            Console.WriteLine($""Error deleting item: {ex.Message}"");
        }
    }
}";
        File.WriteAllText(Path.Combine(pagesPath, "Items.razor"), itemsPageContent);

        // Create Item create page
        string createPageContent = @"@page ""/items/create""
@using Services
@inject ItemRegistryService ItemService
@inject NavigationManager Navigation

<PageTitle>Create Item</PageTitle>

<h1>Create Item</h1>

<div class=""row"">
    <div class=""col-md-6"">
        <EditForm Model=""@newItem"" OnValidSubmit=""@HandleSubmit"">
            <DataAnnotationsValidator />
            <ValidationSummary />

            <div class=""mb-3"">
                <label for=""name"" class=""form-label"">Name</label>
                <InputText id=""name"" @bind-Value=""newItem.Name"" class=""form-control"" />
            </div>

            <div class=""mb-3"">
                <label for=""description"" class=""form-label"">Description</label>
                <InputTextArea id=""description"" @bind-Value=""newItem.Description"" class=""form-control"" rows=""4"" />
            </div>

            <div class=""mb-3"">
                <button type=""submit"" class=""btn btn-primary"" disabled=""@isSubmitting"">
                    @if (isSubmitting)
                    {
                        <span>Creating...</span>
                    }
                    else
                    {
                        <span>Create</span>
                    }
                </button>
                <a href=""/items"" class=""btn btn-secondary"">Cancel</a>
            </div>

            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <div class=""alert alert-danger"">@errorMessage</div>
            }
        </EditForm>
    </div>
</div>

@code {
    private Item newItem = new Item();
    private bool isSubmitting = false;
    private string errorMessage = string.Empty;

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        errorMessage = string.Empty;

        try
        {
            await ItemService.CreateItemAsync(newItem.Name, newItem.Description);
            Navigation.NavigateTo(""/items"");
        }
        catch (Exception ex)
        {
            errorMessage = $""Error creating item: {ex.Message}"";
        }
        finally
        {
            isSubmitting = false;
        }
    }
}";
        File.WriteAllText(Path.Combine(pagesPath, "CreateItem.razor"), createPageContent);

        // Create Item edit page
        string editPageContent = @"@page ""/items/edit/{id:long}""
@using Services
@inject ItemRegistryService ItemService
@inject NavigationManager Navigation

<PageTitle>Edit Item</PageTitle>

<h1>Edit Item</h1>

@if (item == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <div class=""row"">
        <div class=""col-md-6"">
            <EditForm Model=""@item"" OnValidSubmit=""@HandleSubmit"">
                <DataAnnotationsValidator />
                <ValidationSummary />

                <div class=""mb-3"">
                    <label for=""name"" class=""form-label"">Name</label>
                    <InputText id=""name"" @bind-Value=""item.Name"" class=""form-control"" />
                </div>

                <div class=""mb-3"">
                    <label for=""description"" class=""form-label"">Description</label>
                    <InputTextArea id=""description"" @bind-Value=""item.Description"" class=""form-control"" rows=""4"" />
                </div>

                <div class=""mb-3"">
                    <button type=""submit"" class=""btn btn-primary"" disabled=""@isSubmitting"">
                        @if (isSubmitting)
                        {
                            <span>Updating...</span>
                        }
                        else
                        {
                            <span>Update</span>
                        }
                    </button>
                    <a href=""/items"" class=""btn btn-secondary"">Cancel</a>
                </div>

                @if (!string.IsNullOrEmpty(errorMessage))
                {
                    <div class=""alert alert-danger"">@errorMessage</div>
                }
            </EditForm>
        </div>
    </div>
}

@code {
    [Parameter]
    public long Id { get; set; }

    private Item? item;
    private bool isSubmitting = false;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            item = await ItemService.GetItemAsync((ulong)Id);
            if (item == null)
            {
                errorMessage = ""Item not found."";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $""Error loading item: {ex.Message}"";
        }
    }

    private async Task HandleSubmit()
    {
        if (item == null) return;

        isSubmitting = true;
        errorMessage = string.Empty;

        try
        {
            await ItemService.UpdateItemAsync(item.Id, item.Name, item.Description);
            Navigation.NavigateTo(""/items"");
        }
        catch (Exception ex)
        {
            errorMessage = $""Error updating item: {ex.Message}"";
        }
        finally
        {
            isSubmitting = false;
        }
    }
}";
        File.WriteAllText(Path.Combine(pagesPath, "EditItem.razor"), editPageContent);
    }

    private void UpdateNavigation(string appPath)
    {
        string navMenuPath = Path.Combine(appPath, "Components", "Layout", "NavMenu.razor");
        
        if (File.Exists(navMenuPath))
        {
            string navContent = File.ReadAllText(navMenuPath);
            
            // Add Items link after Weather
            if (!navContent.Contains("href=\"items\""))
            {
                string itemsNav = @"
        <div class=""nav-item px-3"">
            <NavLink class=""nav-link"" href=""items"">
                <span class=""bi bi-list-nested-nav-menu"" aria-hidden=""true""></span> Items
            </NavLink>
        </div>";
                
                int weatherIndex = navContent.IndexOf("href=\"weather\"");
                if (weatherIndex > 0)
                {
                    int divEnd = navContent.IndexOf("</div>", weatherIndex);
                    if (divEnd > 0)
                    {
                        divEnd = navContent.IndexOf("</div>", divEnd + 6);
                        if (divEnd > 0)
                        {
                            navContent = navContent.Insert(divEnd + 6, itemsNav);
                            File.WriteAllText(navMenuPath, navContent);
                        }
                    }
                }
            }
        }
    }

    private void CreateConfiguration(string appPath)
    {
        string appsettingsPath = Path.Combine(appPath, "appsettings.json");
        
        if (File.Exists(appsettingsPath))
        {
            string configContent = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  },
  ""AllowedHosts"": ""*"",
  ""Blockchain"": {
    ""NodeUrl"": ""http://localhost:8545"",
    ""ContractAddress"": ""0x0000000000000000000000000000000000000000"",
    ""PrivateKey"": """"
  }
}";
            File.WriteAllText(appsettingsPath, configContent);
        }

        // Update Program.cs to register the service
        string programPath = Path.Combine(appPath, "Program.cs");
        if (File.Exists(programPath))
        {
            string programContent = File.ReadAllText(programPath);
            
            if (!programContent.Contains("ItemRegistryService"))
            {
                // Add using statement
                if (!programContent.Contains("using Services;"))
                {
                    programContent = "using Services;\n" + programContent;
                }

                // Add service registration before app.Run()
                string serviceRegistration = @"
// Register blockchain service
var nodeUrl = builder.Configuration[""Blockchain:NodeUrl""] ?? ""http://localhost:8545"";
var contractAddress = builder.Configuration[""Blockchain:ContractAddress""] ?? ""0x0000000000000000000000000000000000000000"";
var privateKey = builder.Configuration[""Blockchain:PrivateKey""] ?? """";
builder.Services.AddSingleton(new ItemRegistryService(nodeUrl, contractAddress, privateKey));

";
                int runIndex = programContent.LastIndexOf("app.Run();");
                if (runIndex > 0)
                {
                    programContent = programContent.Insert(runIndex, serviceRegistration);
                    File.WriteAllText(programPath, programContent);
                }
            }
        }

        // Copy the ABI file to the output directory
        string csprojPath = Directory.GetFiles(appPath, "*.csproj").FirstOrDefault();
        if (csprojPath != null)
        {
            string csprojContent = File.ReadAllText(csprojPath);
            if (!csprojContent.Contains("ItemRegistry.abi"))
            {
                // Add a section to copy the Contracts folder to output
                string copyToOutput = @"
  <ItemGroup>
    <None Update=""Contracts\**"">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
";
                int projectEndIndex = csprojContent.LastIndexOf("</Project>");
                if (projectEndIndex > 0)
                {
                    csprojContent = csprojContent.Insert(projectEndIndex, copyToOutput);
                    File.WriteAllText(csprojPath, csprojContent);
                }
            }
        }
    }

    private void CreateReadme(string appPath, string appName)
    {
        string readmeContent = $@"# {appName}

A Blazor Server CRUD application with Nethereum smart contract backend.

## Prerequisites

- .NET 8.0 SDK or later
- A local Ethereum node (e.g., Hardhat, Anvil) or access to an Ethereum network

## Getting Started

1. **Start a local Ethereum node** (example using Hardhat):
   ```bash
   npx hardhat node
   ```

2. **Deploy the smart contract**:
   - The contract source is in `Contracts/ItemRegistry.sol`
   - Deploy it to your local network
   - Update `appsettings.json` with the deployed contract address

3. **Configure the application**:
   - Edit `appsettings.json`
   - Set `Blockchain:NodeUrl` to your Ethereum node URL (default: http://localhost:8545)
   - Set `Blockchain:ContractAddress` to your deployed contract address
   - Set `Blockchain:PrivateKey` to an account private key with funds

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Navigate to Items**:
   - Open your browser to https://localhost:5001 (or the URL shown in console)
   - Click on ""Items"" in the navigation menu
   - Create, read, update, and delete items stored on the blockchain!

## Features

- ✅ Full CRUD operations (Create, Read, Update, Delete)
- ✅ Blazor Server UI
- ✅ Nethereum integration for Ethereum smart contracts
- ✅ Decentralized data storage on blockchain
- ✅ No external dependencies (Infura, Web3.js, etc.)
- ✅ 100% Microsoft .NET stack (except Nethereum)

## Project Structure

- `Contracts/` - Solidity smart contracts
- `Services/` - Contract service wrappers
- `Components/Pages/` - Blazor pages
- `appsettings.json` - Configuration

## Smart Contract

The `ItemRegistry` contract provides:
- `createItem(name, description)` - Create a new item
- `getItem(id)` - Get an item by ID
- `getAllItems()` - Get all items
- `updateItem(id, name, description)` - Update an item
- `deleteItem(id)` - Delete an item

## Notes

- Make sure your Ethereum account has sufficient ETH for gas fees
- The contract address and private key in appsettings.json should be kept secure
- For production, use environment variables or Azure Key Vault for secrets

## Deploying the Smart Contract

Here's a simple example using Hardhat:

1. Install Hardhat:
   ```bash
   npm install --save-dev hardhat @nomicfoundation/hardhat-toolbox
   ```

2. Initialize Hardhat in a separate directory:
   ```bash
   npx hardhat init
   ```

3. Copy `Contracts/ItemRegistry.sol` to the Hardhat `contracts/` directory

4. Create a deployment script in `scripts/deploy.js`:
   ```javascript
   const hre = require(""hardhat"");

   async function main() {{
     const ItemRegistry = await hre.ethers.getContractFactory(""ItemRegistry"");
     const registry = await ItemRegistry.deploy();
     await registry.waitForDeployment();
     console.log(""ItemRegistry deployed to:"", await registry.getAddress());
   }}

   main().catch((error) => {{
     console.error(error);
     process.exitCode = 1;
   }});
   ```

5. Deploy:
   ```bash
   npx hardhat run scripts/deploy.js --network localhost
   ```

6. Copy the deployed address to your `appsettings.json`

---

Generated with ❤️ by Semprus Scaffold Tool
";

        File.WriteAllText(Path.Combine(appPath, "README.md"), readmeContent);
    }
}
