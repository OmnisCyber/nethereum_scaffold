# Semprus Quick Start Guide

Get your blockchain application running in under 5 minutes!

## Prerequisites

- .NET 8.0 SDK installed
- Basic understanding of Ethereum/blockchain concepts
- An Ethereum node (we'll use Ganache for this guide)

## Step 1: Install Semprus CLI

```bash
dotnet tool install -g Semprus.CLI
```

Verify installation:
```bash
semprus --version
```

## Step 2: Set Up Local Blockchain

### Option A: Using Ganache (Recommended for Beginners)

1. Install Ganache:
   ```bash
   npm install -g ganache
   ```

2. Start Ganache:
   ```bash
   ganache
   ```

   You'll see output like:
   ```
   Available Accounts
   ==================
   (0) 0x90F8bf6A479f320ead074411a4B0e7944Ea8c9C1 (100 ETH)
   
   Private Keys
   ==================
   (0) 0x4f3edf983ac636a65a842ce7c78d9aa706d3b113bce9c46f30d7d21715b23b1d
   
   Listening on 127.0.0.1:8545
   ```

3. Save the first account's private key - you'll need it later!

### Option B: Using Hardhat

```bash
npx hardhat node
```

## Step 3: Generate Your Application

```bash
semprus scaffold MyFirstBlockchainApp
cd MyFirstBlockchainApp
```

This creates a complete Blazor application with:
- Smart contract (Solidity)
- Service layer (C# + Nethereum)
- UI pages (Blazor)
- Configuration files

## Step 4: Deploy the Smart Contract

### Using Remix IDE (Easiest Method)

1. Open [Remix IDE](https://remix.ethereum.org/)

2. Create a new file `ItemRegistry.sol`

3. Copy the content from `Contracts/ItemRegistry.sol` in your generated app

4. Compile:
   - Click "Solidity Compiler" tab
   - Click "Compile ItemRegistry.sol"

5. Deploy:
   - Click "Deploy & Run Transactions" tab
   - Change Environment to "Injected Provider - MetaMask" or "Web3 Provider"
   - For Web3 Provider, enter: `http://localhost:8545`
   - Click "Deploy"
   - **Save the deployed contract address!**

### Alternative: Using Hardhat

```bash
# In a separate directory
npx hardhat init
# Choose "Create a JavaScript project"

# Copy your contract
cp ../MyFirstBlockchainApp/Contracts/ItemRegistry.sol contracts/

# Create deployment script
cat > scripts/deploy.js << 'EOF'
async function main() {
  const ItemRegistry = await ethers.getContractFactory("ItemRegistry");
  const registry = await ItemRegistry.deploy();
  await registry.deployed();
  console.log("ItemRegistry deployed to:", registry.address);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
EOF

# Deploy
npx hardhat run scripts/deploy.js --network localhost
```

## Step 5: Configure Your Application

Edit `appsettings.json`:

```json
{
  "Blockchain": {
    "NodeUrl": "http://localhost:8545",
    "ContractAddress": "0xYOUR_DEPLOYED_CONTRACT_ADDRESS",
    "PrivateKey": "0xYOUR_GANACHE_PRIVATE_KEY"
  }
}
```

Replace:
- `0xYOUR_DEPLOYED_CONTRACT_ADDRESS` with the address from Remix/Hardhat
- `0xYOUR_GANACHE_PRIVATE_KEY` with the private key from Ganache

## Step 6: Run Your Application

```bash
dotnet run
```

Open your browser to `https://localhost:5001`

## Step 7: Create Your First Blockchain Item

1. Click "View Items" on the home page
2. Click "Create New Item"
3. Fill in the form:
   - Name: "My First Blockchain Item"
   - Description: "Stored immutably on Ethereum!"
4. Click "Create"
5. Wait for the transaction to be mined (~15 seconds)
6. See your item appear in the list!

## What Just Happened?

🎉 Congratulations! You just:

1. ✅ Generated a complete blockchain application
2. ✅ Deployed a smart contract to Ethereum
3. ✅ Created data on the blockchain via a web UI
4. ✅ Read data from the blockchain

## Next Steps

### Explore the CRUD Operations

- **Create**: Add more items
- **Read**: View item details
- **Update**: Edit an existing item (only the owner can update)
- **Delete**: Soft-delete an item (only the owner can delete)

### View Blockchain Transactions

In Ganache, you'll see:
- Transaction hashes
- Gas used
- Block numbers

### Modify the Smart Contract

1. Edit `Contracts/ItemRegistry.sol`
2. Add a new field to the `Item` struct
3. Redeploy the contract
4. Update the service layer in `Services/ItemRegistryService.cs`
5. Update the UI in `Pages/Items/`

### Deploy to a Testnet

1. Get testnet ETH from a faucet:
   - Sepolia: https://sepoliafaucet.com/
   - Goerli: https://goerlifaucet.com/

2. Update `appsettings.json`:
   ```json
   {
     "Blockchain": {
       "NodeUrl": "https://sepolia.infura.io/v3/YOUR_INFURA_KEY",
       "ContractAddress": "0xYourContractAddress",
       "PrivateKey": "0xYourPrivateKey"
     }
   }
   ```

3. Deploy contract to testnet using Remix or Hardhat

4. Run your app!

## Common Issues

### "Cannot connect to blockchain node"

- Verify Ganache is running
- Check `NodeUrl` in `appsettings.json`
- Ensure no firewall is blocking port 8545

### "Transaction failed"

- Check account has sufficient ETH
- Verify private key is correct
- Increase gas limit in `ItemRegistryService.cs`

### "Contract not found"

- Verify contract address is correct
- Ensure contract is deployed
- Check you're connecting to the right network

## Tips

💡 **Development**: Keep Ganache running during development for faster transactions

💡 **Testing**: Use Ganache's built-in account (100 ETH) for unlimited testing

💡 **Security**: Never commit real private keys to version control

💡 **Gas**: Monitor gas usage in Ganache to optimize your contract

## Need Help?

- Check [README.md](README.md) for detailed documentation
- See [EXAMPLES.md](EXAMPLES.md) for advanced use cases
- Review Nethereum docs: https://docs.nethereum.com/

---

Happy blockchain coding! 🚀
