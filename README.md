# Netherforge - Ethereum Blockchain Scaffolding for .NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Netherforge** is a Rails-like scaffolding tool that generates production-ready .NET/Blazor CRUD applications backed by Ethereum smart contracts via [Nethereum](https://nethereum.com/). With a single command, create full-stack blockchain applications without external dependencies like Truffle, Ganache, Infura, or Web3.js.

## Features

✨ **Single Command Setup** - Generate complete blockchain apps in ~10 seconds  
🔗 **Smart Contract Integration** - Solidity contracts with full CRUD operations  
⚡ **Blazor Server UI** - Modern, responsive UI with Bootstrap  
🛠️ **No External Dependencies** - Pure .NET solution using Nethereum  
📦 **Production Ready** - Complete with services, models, and configuration  
🎯 **Type Safe** - Full C# type safety across the stack

## Quick Start

### Installation

```bash
dotnet tool install -g Netherforge.CLI
```

### Create Your First App

```bash
netherforge scaffold MyBlockchainApp
cd MyBlockchainApp
```

### Configure Blockchain Settings

Edit `appsettings.json`:

```json
{
  "Blockchain": {
    "NodeUrl": "http://localhost:8545",
    "ContractAddress": "0xYourContractAddress",
    "PrivateKey": "0xYourPrivateKey"
  }
}
```

### Deploy Smart Contract

1. Compile the Solidity contract in `Contracts/ItemRegistry.sol`
2. Deploy to your Ethereum node (local or testnet)
3. Update `ContractAddress` in `appsettings.json`

### Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` to see your blockchain app in action!

## Architecture

### Generated Application Stack

```
MyBlockchainApp/
├── Contracts/
│   ├── ItemRegistry.sol      # Solidity smart contract
│   └── ItemRegistry.abi       # Contract ABI
├── Models/
│   ├── Item.cs                # Domain model
│   └── BlockchainConfig.cs    # Configuration model
├── Services/
│   └── ItemRegistryService.cs # Nethereum service layer
├── Pages/
│   ├── Index.razor            # Home page
│   └── Items/
│       ├── Index.razor        # List items
│       ├── Create.razor       # Create item
│       └── Edit.razor         # Edit item
├── Shared/
│   └── MainLayout.razor       # App layout
├── wwwroot/
│   └── css/
│       └── site.css           # Styles
└── appsettings.json           # Configuration
```

### Smart Contract Layer

The generated `ItemRegistry` smart contract provides:

- **Create**: Add new items to the blockchain
- **Read**: Query individual items or all items
- **Update**: Modify existing items (owner only)
- **Delete**: Soft-delete items (owner only)
- **Events**: Blockchain events for all operations

```solidity
contract ItemRegistry {
    struct Item {
        uint256 id;
        string name;
        string description;
        address owner;
        uint256 createdAt;
        bool isDeleted;
    }
    
    function createItem(string memory name, string memory description) public returns (uint256)
    function getItem(uint256 id) public view returns (Item memory)
    function updateItem(uint256 id, string memory name, string memory description) public
    function deleteItem(uint256 id) public
    function getAllItems() public view returns (Item[] memory)
}
```

### Service Layer

The `ItemRegistryService` provides a clean C# API over the smart contract:

```csharp
public class ItemRegistryService
{
    public async Task<ulong> CreateItemAsync(string name, string description)
    public async Task<Item?> GetItemAsync(ulong id)
    public async Task<List<Item>> GetAllItemsAsync()
    public async Task UpdateItemAsync(ulong id, string name, string description)
    public async Task DeleteItemAsync(ulong id)
}
```

### UI Layer

Blazor Server pages with full CRUD functionality:

- **Items/Index.razor** - List all items with pagination
- **Items/Create.razor** - Form to create new items
- **Items/Edit.razor** - Form to edit existing items
- Real-time transaction status updates
- Error handling and loading states

## Configuration

### Blockchain Settings

Configure your Ethereum connection in `appsettings.json`:

```json
{
  "Blockchain": {
    "NodeUrl": "http://localhost:8545",           // Your Ethereum node URL
    "ContractAddress": "0x...",                    // Deployed contract address
    "PrivateKey": "0x..."                          // Account private key
  }
}
```

### Supported Networks

- **Local Development**: Ganache, Hardhat Node
- **Testnets**: Sepolia, Goerli, Mumbai
- **Mainnet**: Ethereum, Polygon

## Development Workflow

### Local Testing with Ganache

1. Install and start Ganache:
   ```bash
   npm install -g ganache
   ganache
   ```

2. Deploy contract:
   ```bash
   # Use Remix IDE or Hardhat to deploy ItemRegistry.sol
   ```

3. Update configuration with contract address and private key

4. Run application:
   ```bash
   dotnet run
   ```

### Testing with Hardhat

1. Initialize Hardhat project:
   ```bash
   npx hardhat init
   ```

2. Copy `Contracts/ItemRegistry.sol` to Hardhat project

3. Deploy and get contract address

4. Update `appsettings.json`

## Use Cases

- **Asset Management**: Track physical or digital assets on-chain
- **Supply Chain**: Immutable record of product lifecycle
- **Voting Systems**: Transparent, tamper-proof voting
- **NFT Platforms**: CRUD operations for NFT metadata
- **Decentralized Databases**: Blockchain-backed data storage

## Customization

### Extending the Smart Contract

Modify `Contracts/ItemRegistry.sol` to add custom fields:

```solidity
struct Item {
    uint256 id;
    string name;
    string description;
    uint256 price;        // Add custom field
    string category;      // Add custom field
    address owner;
    uint256 createdAt;
    bool isDeleted;
}
```

### Extending the Service Layer

Update `Services/ItemRegistryService.cs` to match your contract changes.

### Customizing UI

Modify Razor pages in `Pages/Items/` to match your design requirements.

## Requirements

- .NET 8.0 SDK or later
- Ethereum node (local or remote)
- Deployed smart contract

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to Ethereum node  
**Solution**: Verify `NodeUrl` in `appsettings.json` and ensure node is running

### Transaction Failures

**Problem**: Transactions fail or timeout  
**Solution**: Check gas limits in `ItemRegistryService.cs` and account balance

### Contract Not Found

**Problem**: Contract methods fail  
**Solution**: Verify `ContractAddress` is correct and contract is deployed

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Resources

- [Nethereum Documentation](https://docs.nethereum.com/)
- [Blazor Documentation](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Solidity Documentation](https://docs.soliditylang.org/)
- [Ethereum Development](https://ethereum.org/developers)

## Support

For issues and questions:
- GitHub Issues: [Report a bug](https://github.com/yourusername/netherforge/issues)
- Documentation: See [QUICKSTART.md](QUICKSTART.md) and [EXAMPLES.md](EXAMPLES.md)

---

**Built with ❤️ using .NET, Blazor, and Nethereum**
