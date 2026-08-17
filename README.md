# Netherforge - Ethereum Blockchain Scaffolding for .NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Netherforge** is a scaffolding CLI that generates a Blazor Server CRUD application backed by a single Ethereum smart contract (`ItemRegistry`), using [Nethereum](https://nethereum.com/) as the client library. It's written entirely in C#/.NET — no Node.js, Truffle, or Web3.js required to *generate or run* the app.

One thing it does **not** do: compile or deploy the Solidity contract for you. Nethereum is an RPC client, not a blockchain node or a Solidity compiler — deploying `ItemRegistry.sol` still requires a separate tool (Remix, Hardhat, Foundry, etc.) and a running Ethereum-compatible node (Ganache, Hardhat node, Anvil, geth, or a testnet/mainnet RPC endpoint) to deploy against. There's no way around that requirement for any tool — it's inherent to talking to a blockchain, not a Netherforge limitation.

## What it generates

```bash
dotnet tool install -g Netherforge.CLI
netherforge scaffold MyBlockchainApp
cd MyBlockchainApp
dotnet run
```

```
MyBlockchainApp/
├── Contracts/
│   ├── ItemRegistry.sol       # Solidity smart contract
│   └── ItemRegistry.abi       # Contract ABI
├── Models/
│   ├── Item.cs
│   └── BlockchainConfig.cs
├── Services/
│   ├── IItemRegistryService.cs
│   ├── ItemRegistryService.cs      # Real Nethereum-backed implementation
│   └── MockItemRegistryService.cs  # In-memory fallback (see Demo mode below)
├── Pages/
│   ├── Index.razor
│   └── Items/ (Index, Create, Edit)
├── Shared/MainLayout.razor
├── wwwroot/css/site.css
└── appsettings.json
```

The app listens on **`http://localhost:5050`** (HTTP only — Kestrel is hardcoded, not HTTPS/5001).

## Demo mode

Out of the box, `appsettings.json` ships with placeholder zero-address values. When those are left unconfigured, the app automatically falls back to `MockItemRegistryService` — an in-memory list, no blockchain involved — so `dotnet run` works immediately with no setup. This is what you'll see the first time you run a freshly scaffolded app.

## The smart contract

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

`updateItem`/`deleteItem` are owner-gated; deletes are soft (an `isDeleted` flag, not a storage wipe).

## Connecting to a real chain

1. Start a local node — e.g. `npx hardhat node` or `ganache` (either works; the CLI doesn't depend on or bundle either).
2. Compile and deploy `Contracts/ItemRegistry.sol` yourself (Remix IDE is the fastest path — paste the file in, compile, deploy against your local node's RPC URL).
3. Fill in `appsettings.json`:
   ```json
   {
     "Blockchain": {
       "NodeUrl": "http://localhost:8545",
       "ContractAddress": "0xYourDeployedContractAddress",
       "PrivateKey": "0xYourAccountPrivateKey"
     }
   }
   ```
4. `dotnet run` — the app detects real values and switches from `MockItemRegistryService` to `ItemRegistryService`.

The same flow works against a testnet (Sepolia, etc.) by pointing `NodeUrl` at an RPC provider and using a funded testnet key — never a mainnet key with real funds.

## Customizing

- **Contract**: edit `Contracts/ItemRegistry.sol`, redeploy, update `ContractAddress`.
- **Service**: `Services/ItemRegistryService.cs` — this is hand-editable, generated once and not touched again by the CLI.
- **UI**: `Pages/Items/*.razor`.

Note there's currently one entity (`Item`) baked into the template — this isn't a general "scaffold any model" generator yet, it's a fixed CRUD example to build on top of.

## Requirements

- .NET 8.0 SDK
- An Ethereum-compatible node, only if/when you want real deployments (not required to run the demo)

## Troubleshooting

| Problem | Cause |
|---|---|
| Browser can't connect | Check you're using `http://localhost:5050`, and that `dotnet run` is still running in a terminal |
| App stuck in DEMO mode after configuring | Double-check `ContractAddress`/`PrivateKey` in `appsettings.json` aren't still the zero-filled placeholders |
| Transaction fails/times out | Check account balance and gas limit (hardcoded at 3,000,000 in `ItemRegistryService.cs`) |
| `getItem`/contract calls fail | Verify `ContractAddress` is correct for the network `NodeUrl` points at |

## License

MIT — see the LICENSE file for details.

## Resources

- [Nethereum Documentation](https://docs.nethereum.com/)
- [Blazor Documentation](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Solidity Documentation](https://docs.soliditylang.org/)

## Support

- GitHub Issues: [OmnisCyber/nethereum_scaffold](https://github.com/OmnisCyber/nethereum_scaffold/issues)
- See also [QUICKSTART.md](QUICKSTART.md) and [EXAMPLES.md](EXAMPLES.md)
