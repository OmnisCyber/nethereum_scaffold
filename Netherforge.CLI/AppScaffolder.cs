using System.Diagnostics;
using System.Reflection;
using System.Text;
using Spectre.Console;

namespace Netherforge.CLI;

public class AppScaffolder
{
    private string _appName = string.Empty;
    private string _appPath = string.Empty;

    public async Task ScaffoldAsync(string appName)
    {
        _appName = appName;
        _appPath = Path.Combine(Directory.GetCurrentDirectory(), appName);

        AnsiConsole.MarkupLine($"[cyan]Creating Nethersmith blockchain application:[/] [yellow]{appName}[/]");
        AnsiConsole.WriteLine();

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var createProjectTask = ctx.AddTask("[green]Creating project structure[/]");
                CreateProjectStructure();
                createProjectTask.Increment(100);

                var installPackagesTask = ctx.AddTask("[green]Installing NuGet packages[/]");
                await InstallPackagesAsync();
                installPackagesTask.Increment(100);

                var generateFilesTask = ctx.AddTask("[green]Generating files[/]");
                GenerateAllFiles();
                generateFilesTask.Increment(100);

                var buildTask = ctx.AddTask("[green]Building project[/]");
                await BuildProjectAsync();
                buildTask.Increment(100);
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]✓[/] Application created successfully!");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Next steps:[/]");
        AnsiConsole.MarkupLine($"  cd {appName}");
        AnsiConsole.MarkupLine("  dotnet run");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Configure blockchain settings in appsettings.json[/]");
    }

    private void CreateProjectStructure()
    {
        // Create main directories
        Directory.CreateDirectory(_appPath);
        Directory.CreateDirectory(Path.Combine(_appPath, "Contracts"));
        Directory.CreateDirectory(Path.Combine(_appPath, "Services"));
        Directory.CreateDirectory(Path.Combine(_appPath, "Models"));
        Directory.CreateDirectory(Path.Combine(_appPath, "Pages"));
        Directory.CreateDirectory(Path.Combine(_appPath, "Pages", "Items"));
        Directory.CreateDirectory(Path.Combine(_appPath, "wwwroot"));
        Directory.CreateDirectory(Path.Combine(_appPath, "wwwroot", "css"));

        // Create initial .csproj file
        var csprojContent = $$"""
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <None Update="Contracts\*.abi">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
""";

        File.WriteAllText(Path.Combine(_appPath, $"{_appName}.csproj"), csprojContent);
    }

    private async Task InstallPackagesAsync()
    {
        var packages = new[]
        {
            "Nethereum.Web3",
            "Nethereum.Contracts"
        };

        foreach (var package in packages)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add package {package}",
                WorkingDirectory = _appPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
    }

    private void GenerateAllFiles()
    {
        GenerateSmartContract();
        GenerateContractABI();
        GenerateModels();
        GenerateServices();
        GeneratePages();
        GenerateConfiguration();
        GenerateProgramCs();
        GenerateImportsRazor();
        GenerateAppRazor();
        GenerateLayoutFiles();
    }

    private void GenerateSmartContract()
    {
        var contractContent = """
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract ItemRegistry {
    struct Item {
        uint256 id;
        string name;
        string description;
        address owner;
        uint256 createdAt;
        bool isDeleted;
    }
    
    mapping(uint256 => Item) public items;
    uint256 public itemCount;
    
    event ItemCreated(uint256 indexed id, string name, address indexed owner);
    event ItemUpdated(uint256 indexed id, string name);
    event ItemDeleted(uint256 indexed id);
    
    function createItem(string memory name, string memory description) public returns (uint256) {
        itemCount++;
        items[itemCount] = Item({
            id: itemCount,
            name: name,
            description: description,
            owner: msg.sender,
            createdAt: block.timestamp,
            isDeleted: false
        });
        
        emit ItemCreated(itemCount, name, msg.sender);
        return itemCount;
    }
    
    function getItem(uint256 id) public view returns (Item memory) {
        require(id > 0 && id <= itemCount, "Item does not exist");
        require(!items[id].isDeleted, "Item has been deleted");
        return items[id];
    }
    
    function updateItem(uint256 id, string memory name, string memory description) public {
        require(id > 0 && id <= itemCount, "Item does not exist");
        require(!items[id].isDeleted, "Item has been deleted");
        require(items[id].owner == msg.sender, "Only owner can update");
        
        items[id].name = name;
        items[id].description = description;
        
        emit ItemUpdated(id, name);
    }
    
    function deleteItem(uint256 id) public {
        require(id > 0 && id <= itemCount, "Item does not exist");
        require(!items[id].isDeleted, "Item already deleted");
        require(items[id].owner == msg.sender, "Only owner can delete");
        
        items[id].isDeleted = true;
        
        emit ItemDeleted(id);
    }
    
    function getAllItems() public view returns (Item[] memory) {
        uint256 activeCount = 0;
        
        // Count active items
        for (uint256 i = 1; i <= itemCount; i++) {
            if (!items[i].isDeleted) {
                activeCount++;
            }
        }
        
        // Create result array
        Item[] memory result = new Item[](activeCount);
        uint256 resultIndex = 0;
        
        for (uint256 i = 1; i <= itemCount; i++) {
            if (!items[i].isDeleted) {
                result[resultIndex] = items[i];
                resultIndex++;
            }
        }
        
        return result;
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Contracts", "ItemRegistry.sol"), contractContent);
    }

    private void GenerateContractABI()
    {
        var abiContent = """
[
  {
    "anonymous": false,
    "inputs": [
      {"indexed": true, "internalType": "uint256", "name": "id", "type": "uint256"},
      {"indexed": false, "internalType": "string", "name": "name", "type": "string"},
      {"indexed": true, "internalType": "address", "name": "owner", "type": "address"}
    ],
    "name": "ItemCreated",
    "type": "event"
  },
  {
    "anonymous": false,
    "inputs": [
      {"indexed": true, "internalType": "uint256", "name": "id", "type": "uint256"}
    ],
    "name": "ItemDeleted",
    "type": "event"
  },
  {
    "anonymous": false,
    "inputs": [
      {"indexed": true, "internalType": "uint256", "name": "id", "type": "uint256"},
      {"indexed": false, "internalType": "string", "name": "name", "type": "string"}
    ],
    "name": "ItemUpdated",
    "type": "event"
  },
  {
    "inputs": [
      {"internalType": "string", "name": "name", "type": "string"},
      {"internalType": "string", "name": "description", "type": "string"}
    ],
    "name": "createItem",
    "outputs": [{"internalType": "uint256", "name": "", "type": "uint256"}],
    "stateMutability": "nonpayable",
    "type": "function"
  },
  {
    "inputs": [{"internalType": "uint256", "name": "id", "type": "uint256"}],
    "name": "deleteItem",
    "outputs": [],
    "stateMutability": "nonpayable",
    "type": "function"
  },
  {
    "inputs": [],
    "name": "getAllItems",
    "outputs": [
      {
        "components": [
          {"internalType": "uint256", "name": "id", "type": "uint256"},
          {"internalType": "string", "name": "name", "type": "string"},
          {"internalType": "string", "name": "description", "type": "string"},
          {"internalType": "address", "name": "owner", "type": "address"},
          {"internalType": "uint256", "name": "createdAt", "type": "uint256"},
          {"internalType": "bool", "name": "isDeleted", "type": "bool"}
        ],
        "internalType": "struct ItemRegistry.Item[]",
        "name": "",
        "type": "tuple[]"
      }
    ],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [{"internalType": "uint256", "name": "id", "type": "uint256"}],
    "name": "getItem",
    "outputs": [
      {
        "components": [
          {"internalType": "uint256", "name": "id", "type": "uint256"},
          {"internalType": "string", "name": "name", "type": "string"},
          {"internalType": "string", "name": "description", "type": "string"},
          {"internalType": "address", "name": "owner", "type": "address"},
          {"internalType": "uint256", "name": "createdAt", "type": "uint256"},
          {"internalType": "bool", "name": "isDeleted", "type": "bool"}
        ],
        "internalType": "struct ItemRegistry.Item",
        "name": "",
        "type": "tuple"
      }
    ],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [],
    "name": "itemCount",
    "outputs": [{"internalType": "uint256", "name": "", "type": "uint256"}],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [{"internalType": "uint256", "name": "", "type": "uint256"}],
    "name": "items",
    "outputs": [
      {"internalType": "uint256", "name": "id", "type": "uint256"},
      {"internalType": "string", "name": "name", "type": "string"},
      {"internalType": "string", "name": "description", "type": "string"},
      {"internalType": "address", "name": "owner", "type": "address"},
      {"internalType": "uint256", "name": "createdAt", "type": "uint256"},
      {"internalType": "bool", "name": "isDeleted", "type": "bool"}
    ],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [
      {"internalType": "uint256", "name": "id", "type": "uint256"},
      {"internalType": "string", "name": "name", "type": "string"},
      {"internalType": "string", "name": "description", "type": "string"}
    ],
    "name": "updateItem",
    "outputs": [],
    "stateMutability": "nonpayable",
    "type": "function"
  }
]
""";

        File.WriteAllText(Path.Combine(_appPath, "Contracts", "ItemRegistry.abi"), abiContent);
    }

    private void GenerateModels()
    {
        var itemModelContent = """
using System.Numerics;

namespace {{_appName}}.Models;

public class Item
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public ulong CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds((long)CreatedAt).DateTime;
}
""".Replace("{{_appName}}", _appName);

        File.WriteAllText(Path.Combine(_appPath, "Models", "Item.cs"), itemModelContent);

        var blockchainConfigContent = """
namespace {{_appName}}.Models;

public class BlockchainConfig
{
    public string NodeUrl { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
""".Replace("{{_appName}}", _appName);

        File.WriteAllText(Path.Combine(_appPath, "Models", "BlockchainConfig.cs"), blockchainConfigContent);
    }

    private void GenerateServices()
    {
        // Mock service for demo mode
        var mockServiceContent = $$"""
using {{_appName}}.Models;

namespace {{_appName}}.Services;

public class MockItemRegistryService : IItemRegistryService
{
    private readonly List<Item> _items = new();
    private ulong _nextId = 1;

    public Task<ulong> CreateItemAsync(string name, string description)
    {
        var item = new Item
        {
            Id = _nextId++,
            Name = name,
            Description = description,
            Owner = "0xDemoAddress",
            CreatedAt = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false
        };
        _items.Add(item);
        return Task.FromResult(item.Id);
    }

    public Task<Item?> GetItemAsync(ulong id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id && !i.IsDeleted);
        return Task.FromResult(item);
    }

    public Task<List<Item>> GetAllItemsAsync()
    {
        var activeItems = _items.Where(i => !i.IsDeleted).ToList();
        return Task.FromResult(activeItems);
    }

    public Task UpdateItemAsync(ulong id, string name, string description)
    {
        var item = _items.FirstOrDefault(i => i.Id == id && !i.IsDeleted);
        if (item != null)
        {
            item.Name = name;
            item.Description = description;
        }
        return Task.CompletedTask;
    }

    public Task DeleteItemAsync(ulong id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.IsDeleted = true;
        }
        return Task.CompletedTask;
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Services", "MockItemRegistryService.cs"), mockServiceContent);

        // Interface
        var interfaceContent = $$"""
using {{_appName}}.Models;

namespace {{_appName}}.Services;

public interface IItemRegistryService
{
    Task<ulong> CreateItemAsync(string name, string description);
    Task<Item?> GetItemAsync(ulong id);
    Task<List<Item>> GetAllItemsAsync();
    Task UpdateItemAsync(ulong id, string name, string description);
    Task DeleteItemAsync(ulong id);
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Services", "IItemRegistryService.cs"), interfaceContent);

        // Real blockchain service
        var serviceContent = $$"""
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using {{_appName}}.Models;
using System.Numerics;

namespace {{_appName}}.Services;

public class ItemRegistryService : IItemRegistryService
{
    private readonly Web3? _web3;
    private readonly Contract? _contract;
    private readonly string _contractAddress;
    private readonly Account? _account;
    private readonly bool _isConfigured;

    public ItemRegistryService(IConfiguration configuration)
    {
        try
        {
            var config = configuration.GetSection("Blockchain").Get<BlockchainConfig>();
            
            if (config == null || 
                config.ContractAddress == "0x0000000000000000000000000000000000000000" ||
                config.PrivateKey == "0x0000000000000000000000000000000000000000000000000000000000000000")
            {
                _isConfigured = false;
                _contractAddress = string.Empty;
                return;
            }

            _contractAddress = config.ContractAddress;
            
            // Initialize account and Web3
            _account = new Account(config.PrivateKey);
            _web3 = new Web3(_account, config.NodeUrl);

            // Load contract ABI
            var abiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Contracts", "ItemRegistry.abi");
            var abi = File.ReadAllText(abiPath);
            
            _contract = _web3.Eth.GetContract(abi, _contractAddress);
            _isConfigured = true;
        }
        catch
        {
            _isConfigured = false;
            _contractAddress = string.Empty;
        }
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured)
        {
            throw new InvalidOperationException("Blockchain not configured. Please update appsettings.json with valid NodeUrl, ContractAddress, and PrivateKey.");
        }
    }

    public async Task<ulong> CreateItemAsync(string name, string description)
    {
        EnsureConfigured();
        
        var createItemFunction = _contract!.GetFunction("createItem");
        
        var receipt = await createItemFunction.SendTransactionAndWaitForReceiptAsync(
            _account!.Address,
            new HexBigInteger(3000000), // gas limit
            null, // gas price (null for default)
            null, // value
            name,
            description
        );

        // Get the item count to return the new item ID
        var itemCountFunction = _contract.GetFunction("itemCount");
        var itemCount = await itemCountFunction.CallAsync<BigInteger>();
        
        return (ulong)itemCount;
    }

    public async Task<Item?> GetItemAsync(ulong id)
    {
        EnsureConfigured();
        
        try
        {
            var getItemFunction = _contract!.GetFunction("getItem");
            var result = await getItemFunction.CallDeserializingToObjectAsync<ItemDTO>(new BigInteger(id));
            
            return new Item
            {
                Id = (ulong)result.Id,
                Name = result.Name,
                Description = result.Description,
                Owner = result.Owner,
                CreatedAt = (ulong)result.CreatedAt,
                IsDeleted = result.IsDeleted
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        EnsureConfigured();
        
        var getAllItemsFunction = _contract!.GetFunction("getAllItems");
        var results = await getAllItemsFunction.CallDeserializingToObjectAsync<List<ItemDTO>>();
        
        return results.Select(dto => new Item
        {
            Id = (ulong)dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Owner = dto.Owner,
            CreatedAt = (ulong)dto.CreatedAt,
            IsDeleted = dto.IsDeleted
        }).ToList();
    }

    public async Task UpdateItemAsync(ulong id, string name, string description)
    {
        EnsureConfigured();
        
        var updateItemFunction = _contract!.GetFunction("updateItem");
        
        await updateItemFunction.SendTransactionAndWaitForReceiptAsync(
            _account!.Address,
            new HexBigInteger(3000000),
            null,
            null,
            new BigInteger(id),
            name,
            description
        );
    }

    public async Task DeleteItemAsync(ulong id)
    {
        EnsureConfigured();
        
        var deleteItemFunction = _contract!.GetFunction("deleteItem");
        
        await deleteItemFunction.SendTransactionAndWaitForReceiptAsync(
            _account!.Address,
            new HexBigInteger(3000000),
            null,
            null,
            new BigInteger(id)
        );
    }

    // DTO for Nethereum deserialization
    public class ItemDTO
    {
        public BigInteger Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public BigInteger CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Services", "ItemRegistryService.cs"), serviceContent);
    }

    private void GeneratePages()
    {
        // Items/Index.razor
        var indexPageContent = $$"""
@page "/items"
@using {{_appName}}.Services
@using {{_appName}}.Models
@inject IItemRegistryService ItemService
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime

<PageTitle>Items</PageTitle>

<div class="page">
    <div class="container">
        <div class="header">
            <button class="home-link" @onclick="@(() => Navigation.NavigateTo("/"))">
                ← Back to Home
            </button>
            <h1>Blockchain Item Registry</h1>
            <p class="subtitle">Manage your items on the Ethereum blockchain</p>
        </div>

        <div class="actions">
            <button class="btn-primary" @onclick="@(() => Navigation.NavigateTo("/items/create"))">
                <span class="btn-icon">+</span> Create New Item
            </button>
        </div>

        @if (loading)
        {
            <div class="spinner-border"></div>
        }
        else if (error != null)
        {
            <div class="alert alert-danger">
                @error
            </div>
        }
        else if (items.Count == 0)
        {
            <div class="alert alert-info">
                No items found. Create your first item to get started!
            </div>
        }
        else
        {
            <div class="items-grid">
                @foreach (var item in items)
                {
                    <div class="item-card">
                        <h3>@item.Name</h3>
                        <p>@item.Description</p>
                        <div class="item-meta">
                            <div>ID: @item.Id</div>
                            <div>Owner: @item.Owner.Substring(0, 10)...</div>
                            <div>Created: @item.CreatedAtDateTime.ToString("g")</div>
                        </div>
                        <div class="item-actions">
                            <button class="btn-primary" @onclick="@(() => Navigation.NavigateTo($"/items/edit/{item.Id}"))">
                                Edit
                            </button>
                            <button class="btn-danger" @onclick="() => DeleteItem(item.Id)">
                                Delete
                            </button>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
</div>

@code {
    private List<Item> items = new();
    private bool loading = true;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        await LoadItems();
    }

    private async Task LoadItems()
    {
        try
        {
            loading = true;
            error = null;
            items = await ItemService.GetAllItemsAsync();
        }
        catch (Exception ex)
        {
            error = $"Failed to load items: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private async Task DeleteItem(ulong id)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", new[] { "Are you sure you want to delete this item?" }))
            return;

        try
        {
            await ItemService.DeleteItemAsync(id);
            await LoadItems();
        }
        catch (Exception ex)
        {
            error = $"Failed to delete item: {ex.Message}";
        }
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "Items", "Index.razor"), indexPageContent);

        // Items/Create.razor
        var createPageContent = $$"""
@page "/items/create"
@using {{_appName}}.Services
@using {{_appName}}.Models
@inject IItemRegistryService ItemService
@inject NavigationManager Navigation

<PageTitle>Create Item</PageTitle>

<div class="page">
    <div class="container">
        <div class="header">
            <button class="home-link" @onclick="@(() => Navigation.NavigateTo("/"))">
                ← Back to Home
            </button>
            <h1>Create New Item</h1>
            <p class="subtitle">Add a new record to the blockchain</p>
        </div>

        <EditForm Model="@model" OnValidSubmit="HandleSubmit">
            <DataAnnotationsValidator />
            <ValidationSummary />

            @if (error != null)
            {
                <div class="alert alert-danger">
                    @error
                </div>
            }

            <div class="mb-3">
                <label for="name" class="form-label">Name</label>
                <InputText id="name" @bind-Value="model.Name" class="form-control" />
            </div>

            <div class="mb-3">
                <label for="description" class="form-label">Description</label>
                <InputTextArea id="description" @bind-Value="model.Description" class="form-control" rows="4" />
            </div>

            <div class="mb-3">
                <button type="submit" class="btn-primary" disabled="@submitting">
                    @if (submitting)
                    {
                        <span class="spinner-border"></span>
                        <span> Creating...</span>
                    }
                    else
                    {
                        <span>Create Item</span>
                    }
                </button>
                <button type="button" class="btn-secondary" @onclick="@(() => Navigation.NavigateTo("/items"))">
                    Cancel
                </button>
            </div>
        </EditForm>
    </div>
</div>

@code {
    private ItemFormModel model = new();
    private bool submitting = false;
    private string? error;

    private async Task HandleSubmit()
    {
        try
        {
            submitting = true;
            error = null;
            
            await ItemService.CreateItemAsync(model.Name, model.Description);
            
            Navigation.NavigateTo("/items");
        }
        catch (Exception ex)
        {
            error = $"Failed to create item: {ex.Message}";
        }
        finally
        {
            submitting = false;
        }
    }

    public class ItemFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "Items", "Create.razor"), createPageContent);

        // Items/Edit.razor
        var editPageContent = $$"""
@page "/items/edit/{id:long}"
@using {{_appName}}.Services
@using {{_appName}}.Models
@inject IItemRegistryService ItemService
@inject NavigationManager Navigation

<PageTitle>Edit Item</PageTitle>

<div class="page">
    <div class="container">
        <div class="header">
            <button class="home-link" @onclick="@(() => Navigation.NavigateTo("/"))">
                ← Back to Home
            </button>
            <h1>Edit Item #@Id</h1>
            <p class="subtitle">Update blockchain record</p>
        </div>

        @if (loading)
        {
            <div class="spinner-border"></div>
        }
        else if (model == null)
        {
            <div class="alert alert-danger">
                Item not found.
            </div>
        }
        else
        {
            <EditForm Model="@model" OnValidSubmit="HandleSubmit">
                <DataAnnotationsValidator />
                <ValidationSummary />

                @if (error != null)
                {
                    <div class="alert alert-danger">
                        @error
                    </div>
                }

                <div class="mb-3">
                    <label for="name" class="form-label">Name</label>
                    <InputText id="name" @bind-Value="model.Name" class="form-control" />
                </div>

                <div class="mb-3">
                    <label for="description" class="form-label">Description</label>
                    <InputTextArea id="description" @bind-Value="model.Description" class="form-control" rows="4" />
                </div>

                <div class="mb-3">
                    <button type="submit" class="btn-primary" disabled="@submitting">
                        @if (submitting)
                        {
                            <span class="spinner-border"></span>
                            <span> Updating...</span>
                        }
                        else
                        {
                            <span>Update Item</span>
                        }
                    </button>
                    <button type="button" class="btn-secondary" @onclick="@(() => Navigation.NavigateTo("/items"))">
                        Cancel
                    </button>
                </div>
            </EditForm>
        }
    </div>
</div>

@code {
    [Parameter]
    public long Id { get; set; }

    private ItemFormModel? model;
    private bool loading = true;
    private bool submitting = false;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var item = await ItemService.GetItemAsync((ulong)Id);
            if (item != null)
            {
                model = new ItemFormModel
                {
                    Name = item.Name,
                    Description = item.Description
                };
            }
        }
        catch (Exception ex)
        {
            error = $"Failed to load item: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            submitting = true;
            error = null;
            
            await ItemService.UpdateItemAsync((ulong)Id, model!.Name, model.Description);
            
            Navigation.NavigateTo("/items");
        }
        catch (Exception ex)
        {
            error = $"Failed to update item: {ex.Message}";
        }
        finally
        {
            submitting = false;
        }
    }

    public class ItemFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "Items", "Edit.razor"), editPageContent);
    }

    private void GenerateConfiguration()
    {
        var appsettingsContent = """
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
    "PrivateKey": "0x0000000000000000000000000000000000000000000000000000000000000000"
  }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "appsettings.json"), appsettingsContent);

        var appsettingsDevelopmentContent = """
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "appsettings.Development.json"), appsettingsDevelopmentContent);
    }

    private void GenerateProgramCs()
    {
        var programContent = $$"""
using {{_appName}}.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register ItemRegistryService as singleton
// Use mock service for demo mode (default), real blockchain when configured
var blockchainConfig = builder.Configuration.GetSection("Blockchain");
var nodeUrl = blockchainConfig["NodeUrl"];
var isConfigured = !string.IsNullOrEmpty(nodeUrl) && 
                   nodeUrl != "http://localhost:8545" &&
                   !string.IsNullOrEmpty(blockchainConfig["ContractAddress"]) &&
                   blockchainConfig["ContractAddress"] != "YOUR_DEPLOYED_CONTRACT_ADDRESS_HERE";

if (isConfigured)
{
    builder.Services.AddSingleton<IItemRegistryService, ItemRegistryService>();
    Console.WriteLine("✓ Using real blockchain configuration");
}
else
{
    builder.Services.AddSingleton<IItemRegistryService, MockItemRegistryService>();
    Console.WriteLine("✓ Running in DEMO mode with in-memory storage");
    Console.WriteLine("  To use real blockchain, configure appsettings.json");
}


// Configure Kestrel to use HTTP only
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5050); // HTTP only
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
""";

        File.WriteAllText(Path.Combine(_appPath, "Program.cs"), programContent);
    }

    private void GenerateImportsRazor()
    {
        var importsContent = $$"""
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using {{_appName}}
@using {{_appName}}.Models
@using {{_appName}}.Services
@using {{_appName}}.Shared
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "_Imports.razor"), importsContent);
    }

    private void GenerateAppRazor()
    {
        var appContent = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="~/" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link href="css/site.css" rel="stylesheet" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
""";

        File.WriteAllText(Path.Combine(_appPath, "App.razor"), appContent);
    }

    private void GenerateLayoutFiles()
    {
        // Pages/_Host.cshtml
        var hostContent = $$"""
@page "/"
@using Microsoft.AspNetCore.Components.Web
@namespace {{_appName}}.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="~/" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
    <link href="css/site.css" rel="stylesheet" />
    <component type="typeof(HeadOutlet)" render-mode="ServerPrerendered" />
</head>
<body>
    <component type="typeof(App)" render-mode="ServerPrerendered" />

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <script src="_framework/blazor.server.js"></script>
</body>
</html>
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "_Host.cshtml"), hostContent);

        // Pages/Index.razor
        var indexContent = """
@page "/"
@inject NavigationManager Navigation

<PageTitle>Blockchain CRUD</PageTitle>

<div class="zen-hero">
    <div class="zen-container">
        <div class="logo-minimal">
            <svg viewBox="0 0 120 120" xmlns="http://www.w3.org/2000/svg">
                <defs>
                    <radialGradient id="sphere3D" cx="35%" cy="35%">
                        <stop offset="0%" style="stop-color:#66C3E8;stop-opacity:1" />
                        <stop offset="30%" style="stop-color:#1FA3D8;stop-opacity:1" />
                        <stop offset="70%" style="stop-color:#007DB8;stop-opacity:1" />
                        <stop offset="100%" style="stop-color:#003E5C;stop-opacity:1" />
                    </radialGradient>
                    <radialGradient id="shine" cx="30%" cy="25%">
                        <stop offset="0%" style="stop-color:#FFFFFF;stop-opacity:0.95" />
                        <stop offset="20%" style="stop-color:#FFFFFF;stop-opacity:0.6" />
                        <stop offset="50%" style="stop-color:#FFFFFF;stop-opacity:0.2" />
                        <stop offset="100%" style="stop-color:#FFFFFF;stop-opacity:0" />
                    </radialGradient>
                </defs>
                
                <circle cx="60" cy="60" r="48" fill="url(#sphere3D)"/>
                <circle cx="60" cy="60" r="48" fill="url(#shine)"/>
            </svg>
        </div>
        <h1 class="zen-title" style="text-align: center;">NETHERSMITH</h1>
        <p class="zen-tagline">Rails-like scaffolding for .NET blockchain applications. Build production-ready Blazor CRUD apps backed by Ethereum smart contracts—no external dependencies required. Scaffold, deploy, and run entirely in .NET. <a href="https://github.com/OmnisCyber/nethereum_scaffold" target="_blank" style="color: var(--primary-color); text-decoration: underline;">View on GitHub</a></p>
    </div>
</div>

<section class="zen-stats">
    <div class="zen-stats-container">
        <div class="stat-minimal">
            <div class="stat-value">Blockchain</div>
            <div class="stat-value">CRUD</div>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-minimal">
            <div class="stat-value">Smart</div>
            <div class="stat-value">Contracts</div>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-minimal">
            <div class="stat-value">Ethereum</div>
        </div>
    </div>
</section>

<section class="zen-features">
    <div class="zen-container-wide">
        <div class="feature-minimal">
            <span class="feature-number">CREATE</span>
            <h3>Add New Items</h3>
            <p>Create new records on the blockchain with instant transaction confirmation and immutable storage powered by Solidity smart contracts.</p>
            <div class="contact-trigger">
                <button class="contact-link" @onclick="@(() => Navigation.NavigateTo("/items/create"))">
                    Create Item
                </button>
            </div>
        </div>
        <div class="feature-minimal">
            <span class="feature-number">READ</span>
            <h3>View All Items</h3>
            <p>Browse your complete item registry with real-time data fetched directly from the Ethereum blockchain via Nethereum integration.</p>
            <div class="contact-trigger">
                <button class="contact-link" @onclick="@(() => Navigation.NavigateTo("/items"))">
                    View All Items
                </button>
            </div>
        </div>
        <div class="feature-minimal">
            <span class="feature-number">UPDATE</span>
            <h3>Edit Items</h3>
            <p>Modify existing records with ownership verification and blockchain transaction confirmation.</p>
            <div class="contact-trigger">
                <button class="contact-link" @onclick="@(() => Navigation.NavigateTo("/items"))">
                    Manage Items
                </button>
            </div>
        </div>
    </div>
</section>

<section class="zen-philosophy">
    <div class="zen-container">
        <div class="philosophy-content">
            <h2 class="philosophy-title">Full-Stack Blockchain CRUD</h2>
            <p class="philosophy-text">Complete Create, Read, Update, Delete operations powered by Solidity smart contracts and Nethereum. All data stored immutably on Ethereum with type-safe C# integration.</p>
        </div>
    </div>
</section>

<section class="zen-philosophy">
    <div class="zen-container">
        <div class="philosophy-content">
            <h2 class="philosophy-title">Technology Stack</h2>
            <p class="philosophy-text">Security-first .NET Core engineering building robust back-ends that interface with smart contracts, designed for auditability, operational security, and minimal attack surfaces.</p>
        </div>
    </div>
</section>
""";

        File.WriteAllText(Path.Combine(_appPath, "Pages", "Index.razor"), indexContent);

        // Shared/MainLayout.razor
        Directory.CreateDirectory(Path.Combine(_appPath, "Shared"));
        var layoutContent = """
@inherits LayoutComponentBase

<div class="page">
    <main>
        @Body
    </main>
</div>
""";

        File.WriteAllText(Path.Combine(_appPath, "Shared", "MainLayout.razor"), layoutContent);

        // App.razor (Router configuration)
        var appRouterContent = """
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web

<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(Shared.MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(Shared.MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
""";

        File.WriteAllText(Path.Combine(_appPath, "App.razor"), appRouterContent);

        // wwwroot/css/site.css
        var cssContent = """
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600&family=Playfair+Display:wght@400;500;600&display=swap');

:root {
    --primary-color: #007DB8;
    --primary-light: #66C3E8;
    --primary-dark: #003E5C;
    --text-primary: #1a1a1a;
    --text-secondary: #666;
    --bg-light: #fafafa;
    
    --space-xs: 0.5rem;
    --space-sm: 1rem;
    --space-md: 1.5rem;
    --space-lg: 2rem;
    --space-xl: 3rem;
    --space-2xl: 4rem;
    --space-3xl: 6rem;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

html, body, #app {
    margin: 0;
    padding: 0;
    width: 100%;
    overflow-x: hidden;
}

html {
    scroll-behavior: smooth;
    width: 100%;
    overflow-x: hidden;
}

body {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
    color: var(--text-primary);
    line-height: 1.6;
    background: white;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    width: 100%;
    overflow-x: hidden;
}

.zen-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 0 var(--space-md);
    text-align: center;
}

.zen-container-wide {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 var(--space-md);
    text-align: center;
}

.zen-hero {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
    background: white;
}

.logo-minimal {
    width: 120px;
    height: 120px;
    margin: 0 auto var(--space-xl);
}

.zen-title {
    font-family: 'Playfair Display', serif;
    font-size: clamp(3rem, 8vw, 6rem);
    font-weight: 400;
    letter-spacing: 0.4em;
    color: var(--primary-color);
    margin: 0 auto var(--space-md);
    border: none;
    outline: none;
    background: transparent;
    box-shadow: none;
    text-align: center;
    width: 100%;
    text-indent: 0.4em;
}

.zen-tagline {
    font-size: 1.1rem;
    font-weight: 300;
    color: var(--text-secondary);
    letter-spacing: 0.05em;
    max-width: 700px;
    margin: 0 auto;
    text-align: center;
    padding: 0 var(--space-md);
}

.zen-features {
    padding: var(--space-3xl) var(--space-md);
    background: white;
    text-align: center;
}

.zen-container-wide {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-2xl);
    max-width: 1200px;
    margin: 0 auto;
    justify-items: center;
    align-items: start;
}

.feature-minimal {
    text-align: center;
    padding: var(--space-lg);
    width: 100%;
    max-width: 350px;
    margin: 0 auto;
}

.feature-number {
    display: block;
    font-family: 'Playfair Display', serif;
    font-size: 0.875rem;
    color: var(--primary-color);
    margin-bottom: var(--space-md);
    letter-spacing: 0.2em;
}

.feature-minimal h3 {
    font-family: 'Playfair Display', serif;
    font-size: 1.5rem;
    font-weight: 500;
    margin: 0 auto var(--space-sm);
    color: var(--text-primary);
    text-align: center;
}

.feature-minimal p {
    font-size: 0.95rem;
    color: var(--text-secondary);
    line-height: 1.8;
    text-align: center;
    margin: 0 auto;
}

.zen-stats {
    padding: var(--space-3xl) 0;
    background: var(--bg-light);
    width: 100%;
    display: flex;
    justify-content: center;
}

.zen-stats-container {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: var(--space-xl);
    flex-wrap: nowrap;
}

.zen-stats .zen-container {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: var(--space-xl);
    flex-wrap: nowrap;
    width: auto !important;
    max-width: none !important;
    margin: 0 !important;
    padding: 0 !important;
    text-align: center;
}

.stat-minimal {
    text-align: center;
    flex: 0 0 auto;
    white-space: nowrap;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.stat-value {
    font-family: 'Playfair Display', serif;
    font-size: 2rem;
    font-weight: 500;
    color: var(--primary-color);
    margin: 0;
    line-height: 1.2;
}

.stat-desc {
    font-size: 0.875rem;
    color: var(--text-secondary);
    letter-spacing: 0.1em;
    text-transform: uppercase;
}

.stat-divider {
    width: 1px;
    height: 60px;
    background: #e0e0e0;
}

.zen-philosophy {
    padding: var(--space-3xl) var(--space-md);
    background: white;
    text-align: center;
}

.philosophy-content {
    max-width: 700px;
    margin: 0 auto;
    text-align: center;
}

.philosophy-title {
    font-family: 'Playfair Display', serif;
    font-size: clamp(1.75rem, 4vw, 2.5rem);
    font-weight: 400;
    color: var(--text-primary);
    margin-bottom: var(--space-lg);
    line-height: 1.3;
}

.philosophy-text {
    font-size: 1.05rem;
    color: var(--text-secondary);
    line-height: 1.9;
    font-weight: 300;
}

.contact-trigger {
    margin-top: var(--space-2xl);
    text-align: center;
}

.contact-link {
    font-family: 'Inter', sans-serif;
    font-size: 1.25rem;
    padding: var(--space-lg) var(--space-2xl);
    margin-top: var(--space-xl);
    letter-spacing: 0.08em;
    font-weight: 400;
    color: var(--primary-color);
    background: transparent;
    border: none;
    cursor: pointer;
    transition: all 0.3s ease;
    position: relative;
}

.contact-link::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    width: 0;
    height: 1px;
    background: var(--primary-color);
    transition: width 0.3s ease;
}

.contact-link:hover::after {
    width: 100%;
}

@media (max-width: 768px) {
    .zen-container-wide {
        grid-template-columns: 1fr;
        gap: var(--space-xl);
        text-align: center;
    }
    
    .stat-divider {
        display: none;
    }
    
    .zen-stats .zen-container {
        flex-direction: column;
        gap: var(--space-lg);
    }
    
    .stat-minimal {
        width: 100%;
    }
}

/* CRUD Pages */
.page {
    min-height: 100vh;
    background: white;
    padding: var(--space-2xl) var(--space-md);
}

.container {
    max-width: 1200px;
    margin: 0 auto;
}

.header {
    text-align: center;
    margin-bottom: var(--space-2xl);
}

.home-link {
    font-family: 'Inter', sans-serif;
    font-size: 0.95rem;
    font-weight: 400;
    color: var(--text-secondary);
    background: transparent;
    border: none;
    cursor: pointer;
    margin-bottom: var(--space-lg);
    padding: var(--space-sm) 0;
    transition: color 0.3s ease;
    display: inline-block;
}

.home-link:hover {
    color: var(--primary-color);
}

.header h1 {
    font-family: 'Playfair Display', serif;
    font-size: clamp(2rem, 4vw, 3rem);
    color: var(--primary-color);
    letter-spacing: 0.1em;
    margin-bottom: var(--space-sm);
}

.subtitle {
    font-size: 1.05rem;
    color: var(--text-secondary);
    font-weight: 300;
}

.actions {
    text-align: center;
    margin-bottom: var(--space-2xl);
}

.btn-primary {
    font-family: 'Inter', sans-serif;
    font-size: 0.95rem;
    font-weight: 400;
    color: white;
    background: var(--primary-color);
    border: 1px solid var(--primary-color);
    padding: var(--space-md) var(--space-2xl);
    cursor: pointer;
    letter-spacing: 0.05em;
    transition: all 0.3s ease;
    border-radius: 0.375rem;
}

.btn-primary:hover {
    background: var(--primary-dark);
    border-color: var(--primary-dark);
}

.btn-secondary {
    font-family: 'Inter', sans-serif;
    font-size: 0.95rem;
    font-weight: 400;
    color: var(--text-primary);
    background: transparent;
    border: 1px solid var(--text-secondary);
    padding: var(--space-md) var(--space-2xl);
    cursor: pointer;
    letter-spacing: 0.05em;
    transition: all 0.3s ease;
    border-radius: 0.375rem;
    margin-left: var(--space-sm);
}

.btn-secondary:hover {
    background: var(--bg-light);
}

.btn-danger {
    font-family: 'Inter', sans-serif;
    font-size: 0.875rem;
    font-weight: 400;
    color: white;
    background: #dc2626;
    border: 1px solid #dc2626;
    padding: 0.5rem 1rem;
    cursor: pointer;
    letter-spacing: 0.05em;
    transition: all 0.3s ease;
    border-radius: 0.375rem;
}

.btn-danger:hover {
    background: #991b1b;
}

.btn-icon {
    font-size: 1.25rem;
    margin-right: 0.25rem;
}

.items-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: var(--space-lg);
    margin-top: var(--space-2xl);
}

.item-card {
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 0.5rem;
    padding: var(--space-lg);
    transition: all 0.3s ease;
}

.item-card:hover {
    box-shadow: 0 4px 12px rgba(0,125,184,0.1);
    border-color: var(--primary-light);
}

.item-card h3 {
    font-family: 'Playfair Display', serif;
    font-size: 1.25rem;
    color: var(--text-primary);
    margin-bottom: var(--space-sm);
}

.item-card p {
    color: var(--text-secondary);
    font-size: 0.95rem;
    margin-bottom: var(--space-md);
}

.item-meta {
    font-size: 0.875rem;
    color: var(--text-secondary);
    margin-bottom: var(--space-md);
}

.item-actions {
    display: flex;
    gap: var(--space-sm);
    margin-top: var(--space-md);
}

.alert {
    padding: var(--space-md);
    border-radius: 0.375rem;
    margin-bottom: var(--space-lg);
    font-size: 0.95rem;
}

.alert-info {
    background: #eff6ff;
    color: #1e40af;
    border: 1px solid #bfdbfe;
}

.alert-danger {
    background: #fef2f2;
    color: #991b1b;
    border: 1px solid #fecaca;
}

.spinner-border {
    display: inline-block;
    width: 2rem;
    height: 2rem;
    border: 0.25em solid var(--primary-light);
    border-right-color: transparent;
    border-radius: 50%;
    animation: spinner 0.75s linear infinite;
}

@keyframes spinner {
    to { transform: rotate(360deg); }
}

.form-label {
    font-family: 'Inter', sans-serif;
    font-weight: 500;
    color: var(--text-primary);
    margin-bottom: 0.5rem;
    display: block;
}

.form-control {
    width: 100%;
    padding: var(--space-md);
    font-family: 'Inter', sans-serif;
    font-size: 1rem;
    border: 1px solid #e0e0e0;
    border-radius: 0.375rem;
    transition: border-color 0.3s ease;
}

.form-control:focus {
    outline: none;
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(0,125,184,0.1);
}

.mb-3 {
    margin-bottom: var(--space-md);
}

.validation-errors {
    color: #dc2626;
    font-size: 0.875rem;
}


@keyframes spinner-border {
    to { transform: rotate(360deg); }
}

/* Error UI */
#blazor-error-ui {
    background: #fef2f2;
    bottom: 0;
    box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.15);
    display: none;
    left: 0;
    padding: 1rem 1.5rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
    border-top: 3px solid #dc2626;
    color: #991b1b;
}

#blazor-error-ui .dismiss {
    cursor: pointer;
    position: absolute;
    right: 1rem;
    top: 50%;
    transform: translateY(-50%);
    font-size: 1.25rem;
    color: #991b1b;
}

/* Responsive */
@media (max-width: 768px) {
    .container {
        padding: 0 var(--space-md);
    }
    
    .table {
        font-size: 0.875rem;
    }
    
    .btn {
        width: 100%;
        margin-bottom: 0.5rem;
    }
}
""";

        File.WriteAllText(Path.Combine(_appPath, "wwwroot", "css", "site.css"), cssContent);
    }

    private async Task BuildProjectAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build",
            WorkingDirectory = _appPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
        }
    }
}
