# Quick Start Guide

This guide will walk you through creating your first blockchain-powered CRUD application using Semprus.

## Prerequisites

- .NET 8.0 SDK
- Node.js (for running a local blockchain)

## Step 1: Install Semprus CLI

```bash
cd Semprus.CLI
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release Semprus.CLI
```

## Step 2: Create Your Application

```bash
semprus scaffold MyFirstApp
cd MyFirstApp
```

## Step 3: Set Up Local Blockchain

We'll use Hardhat for a local Ethereum development network:

```bash
# In a separate terminal
npm install --save-dev hardhat @nomicfoundation/hardhat-toolbox
npx hardhat init
```

Choose "Create an empty hardhat.config.js" when prompted.

Start the local blockchain:

```bash
npx hardhat node
```

This will start a local blockchain at `http://localhost:8545` and display several test accounts with private keys.

## Step 4: Deploy the Smart Contract

Copy the generated contract to Hardhat:

```bash
cp Contracts/ItemRegistry.sol ../hardhat/contracts/
```

Create a deployment script `scripts/deploy.js`:

```javascript
async function main() {
  const ItemRegistry = await ethers.getContractFactory("ItemRegistry");
  const registry = await ItemRegistry.deploy();
  await registry.waitForDeployment();
  const address = await registry.getAddress();
  console.log("ItemRegistry deployed to:", address);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
```

Deploy the contract:

```bash
npx hardhat run scripts/deploy.js --network localhost
```

Copy the deployed contract address from the output.

## Step 5: Configure Your Application

Edit `appsettings.json` and update the blockchain settings:

```json
{
  "Blockchain": {
    "NodeUrl": "http://localhost:8545",
    "ContractAddress": "0x... (paste your deployed address here)",
    "PrivateKey": "0x... (paste a test account private key from Hardhat)"
  }
}
```

## Step 6: Run Your Application

```bash
dotnet run
```

Open your browser to `https://localhost:5001` (or the URL shown in the console).

## Step 7: Use Your Application

1. Click on "Items" in the navigation menu
2. Click "Create New Item"
3. Enter a name and description
4. Click "Create" - this will create a transaction on the blockchain!
5. View your item in the list
6. Try editing and deleting items

## What Just Happened?

Every CRUD operation you performed was executed as a transaction on the Ethereum blockchain:

- **Create**: Calls the `createItem` function on your smart contract
- **Read**: Calls the `getItem` or `getAllItems` view functions
- **Update**: Calls the `updateItem` function
- **Delete**: Calls the `deleteItem` function

All data is stored on-chain, making it censorship-resistant and decentralized!

## Architecture Overview

```
┌─────────────────────────────────────┐
│       Blazor Server UI              │
│  (Components/Pages/*.razor)         │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│   C# Service Layer                  │
│  (ItemRegistryService.cs)           │
│  - Uses Nethereum library           │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│   Ethereum Blockchain               │
│  (Solidity Smart Contract)          │
│  - Stores all data on-chain         │
└─────────────────────────────────────┘
```

## Key Technologies

- **Blazor Server**: Microsoft's web framework for interactive UIs
- **Nethereum**: .NET integration library for Ethereum
- **Solidity**: Smart contract programming language
- **Hardhat**: Ethereum development environment

## Next Steps

- Modify the `Item` model to add more properties
- Update the smart contract to handle new fields
- Customize the UI styling
- Deploy to a testnet for wider access
- Explore advanced Nethereum features

## Troubleshooting

**Error: Could not connect to blockchain**
- Ensure Hardhat node is running
- Check the `NodeUrl` in appsettings.json

**Error: Transaction failed**
- Ensure the private key has sufficient ETH
- Check the contract address is correct

**Build errors**
- Ensure all NuGet packages are restored: `dotnet restore`

## Support

For issues or questions, please open an issue on GitHub.
