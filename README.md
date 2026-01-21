# nethereum_scaffold

A .NET CLI tool that scaffolds censorship resistant CRUD applications in seconds.

## Installation

Install the Semprus CLI tool globally:

```bash
cd Semprus.CLI
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release Semprus.CLI
```

## Usage

Create a new Blazor CRUD application with Nethereum smart contract backend:

```bash
semprus scaffold MyWebApp
```

This will generate a complete .NET/Blazor application with:
- ✅ Blazor Server UI
- ✅ Full CRUD operations (Create, Read, Update, Delete)
- ✅ Nethereum integration for Ethereum smart contracts
- ✅ Solidity smart contract for decentralized data storage
- ✅ No external dependencies (no Infura, no Web3.js, no Truffle/Ganache)
- ✅ 100% Microsoft .NET stack (except Nethereum library)

## Features

The scaffolded application includes:

- **Blazor Server Application**: Modern web UI with real-time updates
- **Smart Contract**: Solidity contract for CRUD operations on blockchain
- **Service Layer**: C# wrappers for contract interaction using Nethereum
- **CRUD Pages**: Complete UI for managing items
- **Configuration**: Easily configurable blockchain connection settings

## Running Generated Applications

After scaffolding:

1. Navigate to your app directory:
   ```bash
   cd MyWebApp
   ```

2. Set up a local blockchain (e.g., using Hardhat):
   ```bash
   npx hardhat node
   ```

3. Deploy the smart contract (see the generated README.md for details)

4. Update `appsettings.json` with your contract address and configuration

5. Run the application:
   ```bash
   dotnet run
   ```

6. Open your browser and navigate to the Items page to start managing blockchain-based data!

## Tech Stack

- **.NET 8.0**: Modern, cross-platform framework
- **Blazor Server**: Interactive web UI framework
- **Nethereum**: .NET integration library for Ethereum
- **Solidity**: Smart contract programming language

## Architecture

The tool generates applications with the following structure:

```
MyWebApp/
├── Contracts/              # Solidity smart contracts
│   ├── ItemRegistry.sol
│   └── ItemRegistry.abi
├── Services/               # Contract service wrappers
│   ├── Item.cs
│   └── ItemRegistryService.cs
├── Components/
│   ├── Pages/             # Blazor CRUD pages
│   │   ├── Items.razor
│   │   ├── CreateItem.razor
│   │   └── EditItem.razor
│   └── Layout/            # Layout components
├── appsettings.json       # Configuration
├── Program.cs             # Application entry point
└── README.md              # App-specific documentation
```

## Requirements

- .NET 8.0 SDK or later
- An Ethereum node (local or remote) for blockchain interaction

## License

MIT

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

