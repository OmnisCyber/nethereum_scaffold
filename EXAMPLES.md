# Semprus Examples

Advanced examples and use cases for Semprus blockchain applications.

## Table of Contents

- [Custom Smart Contracts](#custom-smart-contracts)
- [Advanced CRUD Operations](#advanced-crud-operations)
- [Event Handling](#event-handling)
- [Multi-Chain Deployment](#multi-chain-deployment)
- [Authentication & Authorization](#authentication--authorization)
- [Real-World Use Cases](#real-world-use-cases)

---

## Custom Smart Contracts

### Example 1: Product Registry with Pricing

Extend the default contract to include pricing:

```solidity
// Contracts/ProductRegistry.sol
contract ProductRegistry {
    struct Product {
        uint256 id;
        string name;
        string description;
        uint256 price;
        string category;
        address owner;
        uint256 createdAt;
        bool isDeleted;
    }
    
    mapping(uint256 => Product) public products;
    uint256 public productCount;
    
    event ProductCreated(uint256 indexed id, string name, uint256 price);
    event ProductPriceUpdated(uint256 indexed id, uint256 oldPrice, uint256 newPrice);
    
    function createProduct(
        string memory name, 
        string memory description,
        uint256 price,
        string memory category
    ) public returns (uint256) {
        productCount++;
        products[productCount] = Product({
            id: productCount,
            name: name,
            description: description,
            price: price,
            category: category,
            owner: msg.sender,
            createdAt: block.timestamp,
            isDeleted: false
        });
        
        emit ProductCreated(productCount, name, price);
        return productCount;
    }
    
    function updatePrice(uint256 id, uint256 newPrice) public {
        require(products[id].owner == msg.sender, "Only owner can update");
        uint256 oldPrice = products[id].price;
        products[id].price = newPrice;
        
        emit ProductPriceUpdated(id, oldPrice, newPrice);
    }
}
```

Update the service:

```csharp
// Services/ProductRegistryService.cs
public class ProductRegistryService
{
    public async Task<ulong> CreateProductAsync(
        string name, 
        string description, 
        decimal price,
        string category)
    {
        var createFunction = _contract.GetFunction("createProduct");
        var priceWei = Web3.Convert.ToWei(price);
        
        var receipt = await createFunction.SendTransactionAndWaitForReceiptAsync(
            _account.Address,
            new HexBigInteger(3000000),
            null,
            null,
            name,
            description,
            priceWei,
            category
        );

        var productCountFunction = _contract.GetFunction("productCount");
        var count = await productCountFunction.CallAsync<BigInteger>();
        
        return (ulong)count;
    }
    
    public async Task UpdatePriceAsync(ulong id, decimal newPrice)
    {
        var updatePriceFunction = _contract.GetFunction("updatePrice");
        var priceWei = Web3.Convert.ToWei(newPrice);
        
        await updatePriceFunction.SendTransactionAndWaitForReceiptAsync(
            _account.Address,
            new HexBigInteger(3000000),
            null,
            null,
            new BigInteger(id),
            priceWei
        );
    }
}
```

---

## Advanced CRUD Operations

### Example 2: Batch Operations

```csharp
// Services/ItemRegistryService.cs (extended)
public async Task<List<ulong>> CreateMultipleItemsAsync(List<(string name, string description)> items)
{
    var itemIds = new List<ulong>();
    
    foreach (var item in items)
    {
        var id = await CreateItemAsync(item.name, item.description);
        itemIds.Add(id);
    }
    
    return itemIds;
}

public async Task<List<Item>> GetItemsByOwnerAsync(string ownerAddress)
{
    var allItems = await GetAllItemsAsync();
    return allItems.Where(i => i.Owner.Equals(ownerAddress, StringComparison.OrdinalIgnoreCase))
                   .ToList();
}
```

### Example 3: Pagination

```csharp
// Services/ItemRegistryService.cs (extended)
public async Task<(List<Item> items, int totalCount)> GetItemsPaginatedAsync(int page, int pageSize)
{
    var allItems = await GetAllItemsAsync();
    var totalCount = allItems.Count;
    
    var items = allItems
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    
    return (items, totalCount);
}
```

Blazor page:

```razor
@page "/items"
@inject ItemRegistryService ItemService

<h1>Items (Page @currentPage)</h1>

<table class="table">
    @foreach (var item in items)
    {
        <tr>
            <td>@item.Name</td>
            <td>@item.Description</td>
        </tr>
    }
</table>

<nav>
    <ul class="pagination">
        @for (int i = 1; i <= totalPages; i++)
        {
            var pageNum = i;
            <li class="page-item @(currentPage == pageNum ? "active" : "")">
                <button class="page-link" @onclick="() => LoadPage(pageNum)">@pageNum</button>
            </li>
        }
    </ul>
</nav>

@code {
    private List<Item> items = new();
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalPages = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadPage(1);
    }

    private async Task LoadPage(int page)
    {
        currentPage = page;
        var (pageItems, totalCount) = await ItemService.GetItemsPaginatedAsync(page, pageSize);
        items = pageItems;
        totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    }
}
```

---

## Event Handling

### Example 4: Listen to Blockchain Events

```csharp
// Services/EventMonitorService.cs
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

public class EventMonitorService
{
    private readonly Web3 _web3;
    private readonly Contract _contract;
    
    public async Task<List<ItemCreatedEvent>> GetItemCreatedEventsAsync()
    {
        var itemCreatedEvent = _contract.GetEvent("ItemCreated");
        
        var filterAll = itemCreatedEvent.CreateFilterInput();
        var logs = await itemCreatedEvent.GetAllChangesAsync(filterAll);
        
        var events = logs.Select(log => new ItemCreatedEvent
        {
            Id = (ulong)log.Event.Id,
            Name = log.Event.Name,
            Owner = log.Event.Owner,
            BlockNumber = (ulong)log.Log.BlockNumber.Value,
            TransactionHash = log.Log.TransactionHash
        }).ToList();
        
        return events;
    }
    
    public async Task MonitorEventsAsync(Action<ItemCreatedEvent> onEventReceived)
    {
        var itemCreatedEvent = _contract.GetEvent("ItemCreated");
        var filterAll = itemCreatedEvent.CreateFilterInput();
        
        // Poll for new events every 5 seconds
        while (true)
        {
            var logs = await itemCreatedEvent.GetAllChangesAsync(filterAll);
            
            foreach (var log in logs)
            {
                var evt = new ItemCreatedEvent
                {
                    Id = (ulong)log.Event.Id,
                    Name = log.Event.Name,
                    Owner = log.Event.Owner
                };
                
                onEventReceived(evt);
            }
            
            await Task.Delay(5000);
        }
    }
}

public class ItemCreatedEvent
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public ulong BlockNumber { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
}
```

---

## Multi-Chain Deployment

### Example 5: Deploy to Multiple Networks

Update configuration to support multiple networks:

```json
// appsettings.json
{
  "Blockchain": {
    "Networks": {
      "Mainnet": {
        "NodeUrl": "https://mainnet.infura.io/v3/YOUR_KEY",
        "ContractAddress": "0xMainnetAddress",
        "PrivateKey": "0xMainnetPrivateKey"
      },
      "Sepolia": {
        "NodeUrl": "https://sepolia.infura.io/v3/YOUR_KEY",
        "ContractAddress": "0xSepoliaAddress",
        "PrivateKey": "0xSepoliaPrivateKey"
      },
      "Polygon": {
        "NodeUrl": "https://polygon-rpc.com",
        "ContractAddress": "0xPolygonAddress",
        "PrivateKey": "0xPolygonPrivateKey"
      }
    },
    "ActiveNetwork": "Sepolia"
  }
}
```

Service implementation:

```csharp
// Services/MultiChainItemRegistryService.cs
public class MultiChainItemRegistryService
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, ItemRegistryService> _services;

    public MultiChainItemRegistryService(IConfiguration configuration)
    {
        _configuration = configuration;
        _services = new Dictionary<string, ItemRegistryService>();
        
        // Initialize services for each network
        var networks = configuration.GetSection("Blockchain:Networks").GetChildren();
        foreach (var network in networks)
        {
            var service = new ItemRegistryService(network);
            _services[network.Key] = service;
        }
    }

    public ItemRegistryService GetService(string? networkName = null)
    {
        networkName ??= _configuration["Blockchain:ActiveNetwork"] ?? "Sepolia";
        return _services[networkName];
    }
}
```

---

## Authentication & Authorization

### Example 6: Wallet-Based Authentication

```csharp
// Services/AuthService.cs
using Nethereum.Signer;

public class AuthService
{
    public bool ValidateSignature(string message, string signature, string expectedAddress)
    {
        var signer = new EthereumMessageSigner();
        var recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);
        
        return recoveredAddress.Equals(expectedAddress, StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<bool> IsOwnerAsync(string address, ulong itemId, ItemRegistryService service)
    {
        var item = await service.GetItemAsync(itemId);
        return item?.Owner.Equals(address, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
```

Blazor component with wallet auth:

```razor
@page "/items/edit/{id:long}"
@inject AuthService AuthService
@inject ItemRegistryService ItemService

@if (!isAuthorized)
{
    <div class="alert alert-warning">
        You must be the owner to edit this item.
    </div>
}
else
{
    <EditForm Model="@model" OnValidSubmit="HandleSubmit">
        <!-- Form fields -->
    </EditForm>
}

@code {
    [Parameter]
    public long Id { get; set; }
    
    private bool isAuthorized = false;
    private string userAddress = ""; // Get from wallet connection

    protected override async Task OnInitializedAsync()
    {
        isAuthorized = await AuthService.IsOwnerAsync(userAddress, (ulong)Id, ItemService);
    }
}
```

---

## Real-World Use Cases

### Example 7: NFT Marketplace

```solidity
// Contracts/NFTMarketplace.sol
contract NFTMarketplace {
    struct NFT {
        uint256 id;
        string name;
        string imageUrl;
        string metadata;
        uint256 price;
        address owner;
        bool forSale;
    }
    
    mapping(uint256 => NFT) public nfts;
    uint256 public nftCount;
    
    function mintNFT(string memory name, string memory imageUrl, string memory metadata) public returns (uint256) {
        nftCount++;
        nfts[nftCount] = NFT({
            id: nftCount,
            name: name,
            imageUrl: imageUrl,
            metadata: metadata,
            price: 0,
            owner: msg.sender,
            forSale: false
        });
        
        return nftCount;
    }
    
    function listForSale(uint256 id, uint256 price) public {
        require(nfts[id].owner == msg.sender, "Only owner can list");
        nfts[id].price = price;
        nfts[id].forSale = true;
    }
    
    function buyNFT(uint256 id) public payable {
        require(nfts[id].forSale, "NFT not for sale");
        require(msg.value >= nfts[id].price, "Insufficient payment");
        
        address previousOwner = nfts[id].owner;
        nfts[id].owner = msg.sender;
        nfts[id].forSale = false;
        
        payable(previousOwner).transfer(msg.value);
    }
}
```

### Example 8: Supply Chain Tracking

```solidity
// Contracts/SupplyChain.sol
contract SupplyChain {
    enum Status { Created, InTransit, Delivered, Verified }
    
    struct Package {
        uint256 id;
        string productName;
        address manufacturer;
        address currentHandler;
        Status status;
        uint256[] checkpoints;
        string[] locations;
    }
    
    mapping(uint256 => Package) public packages;
    uint256 public packageCount;
    
    function createPackage(string memory productName) public returns (uint256) {
        packageCount++;
        packages[packageCount].id = packageCount;
        packages[packageCount].productName = productName;
        packages[packageCount].manufacturer = msg.sender;
        packages[packageCount].currentHandler = msg.sender;
        packages[packageCount].status = Status.Created;
        
        return packageCount;
    }
    
    function updateStatus(uint256 id, Status newStatus, string memory location) public {
        require(packages[id].currentHandler == msg.sender, "Not authorized");
        
        packages[id].status = newStatus;
        packages[id].checkpoints.push(block.timestamp);
        packages[id].locations.push(location);
    }
    
    function transferOwnership(uint256 id, address newHandler) public {
        require(packages[id].currentHandler == msg.sender, "Not authorized");
        packages[id].currentHandler = newHandler;
    }
}
```

### Example 9: Voting System

```solidity
// Contracts/VotingSystem.sol
contract VotingSystem {
    struct Proposal {
        uint256 id;
        string title;
        string description;
        uint256 votesFor;
        uint256 votesAgainst;
        uint256 deadline;
        bool executed;
    }
    
    mapping(uint256 => Proposal) public proposals;
    mapping(uint256 => mapping(address => bool)) public hasVoted;
    uint256 public proposalCount;
    
    function createProposal(string memory title, string memory description, uint256 durationInDays) public {
        proposalCount++;
        proposals[proposalCount] = Proposal({
            id: proposalCount,
            title: title,
            description: description,
            votesFor: 0,
            votesAgainst: 0,
            deadline: block.timestamp + (durationInDays * 1 days),
            executed: false
        });
    }
    
    function vote(uint256 proposalId, bool support) public {
        require(!hasVoted[proposalId][msg.sender], "Already voted");
        require(block.timestamp < proposals[proposalId].deadline, "Voting ended");
        
        hasVoted[proposalId][msg.sender] = true;
        
        if (support) {
            proposals[proposalId].votesFor++;
        } else {
            proposals[proposalId].votesAgainst++;
        }
    }
}
```

---

## Performance Optimization

### Example 10: Caching Strategy

```csharp
// Services/CachedItemRegistryService.cs
using Microsoft.Extensions.Caching.Memory;

public class CachedItemRegistryService
{
    private readonly ItemRegistryService _service;
    private readonly IMemoryCache _cache;
    
    public CachedItemRegistryService(ItemRegistryService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }
    
    public async Task<List<Item>> GetAllItemsAsync()
    {
        const string cacheKey = "all_items";
        
        if (_cache.TryGetValue(cacheKey, out List<Item>? cachedItems))
        {
            return cachedItems!;
        }
        
        var items = await _service.GetAllItemsAsync();
        
        _cache.Set(cacheKey, items, TimeSpan.FromMinutes(5));
        
        return items;
    }
    
    public async Task<ulong> CreateItemAsync(string name, string description)
    {
        var id = await _service.CreateItemAsync(name, description);
        
        // Invalidate cache
        _cache.Remove("all_items");
        
        return id;
    }
}
```

---

## Testing

### Example 11: Integration Tests

```csharp
// Tests/ItemRegistryServiceTests.cs
using Xunit;

public class ItemRegistryServiceTests : IClassFixture<BlockchainFixture>
{
    private readonly ItemRegistryService _service;
    
    public ItemRegistryServiceTests(BlockchainFixture fixture)
    {
        _service = fixture.Service;
    }
    
    [Fact]
    public async Task CreateItem_ShouldReturnValidId()
    {
        // Arrange
        var name = "Test Item";
        var description = "Test Description";
        
        // Act
        var id = await _service.CreateItemAsync(name, description);
        
        // Assert
        Assert.True(id > 0);
    }
    
    [Fact]
    public async Task GetItem_ShouldReturnCreatedItem()
    {
        // Arrange
        var name = "Test Item";
        var description = "Test Description";
        var id = await _service.CreateItemAsync(name, description);
        
        // Act
        var item = await _service.GetItemAsync(id);
        
        // Assert
        Assert.NotNull(item);
        Assert.Equal(name, item.Name);
        Assert.Equal(description, item.Description);
    }
}
```

---

## Additional Resources

- [Nethereum Playground](https://playground.nethereum.com/)
- [Solidity by Example](https://solidity-by-example.org/)
- [Blazor Component Samples](https://blazor.radzen.com/)

---

**Have your own example?** Contribute to this guide by submitting a PR!
