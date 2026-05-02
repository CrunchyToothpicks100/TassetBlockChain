# TassetBlockChain

What is a blockchain? A blockchain is a chain of blocks. Each block contains transaction data, a timestamp, and the hash (fingerprint) of a previous block. When you string these blocks together, you get a blockchain. But a block has to be verified before it is added. A computer must guess the nonce (billions of random numbers) until it finds one that, when combined with the transaction data, produces a hash (fingerprint) starting with a certain number of zeros. This is called "mining a block". This is a program I wrote for creating a simple block, and mining it.

## What it does

1. Take transaction data from StudentBlockTasset.txt
2. Use transaction data to create first block (first block will always have "0" as the previous hash)
3. Mine the block over and over again, writing any successful output to Success.csv and Success_backup.csv (untracked)
4. Does not terminate until you kill the terminal

## Install and Run

1. Install .NET 10.0 SDK with `winget install Microsoft.DotNet.SDK.10`
2. Copy raw .zip file from repository
3. Extract to folder of choice
4. Open terminal and `cd` into the repository
5. Open BlockChainMain.cs and update the file paths, or difficulty
6. Save and close text editors
7. `dotnet run`
8. When prompted, choose whether to mine at half CPU power (y/n)

## Optimizations
- Spawns a set number of parallel tasks, each given their own nonce space
- Checks raw bytes directly via MeetsTarget(), comparing zero bytes and a half-nibble. No string allocation.
- Precomputes the static prefix/suffix bytes once, reuses a single inputBuf, and uses stackalloc for the hash buffer and nonce chars. Zero heap allocation per iteration.
- Threads run until the CancellationToken is cancelled. They record the first hit but keep mining until the caller stops them externally.
- MineBlock(int difficulty, bool halfPower, Action<string, ulong>? onHit, CancellationToken stopToken) - halfPower halves the thread count at runtime; supports a callback on every valid hash found and cooperative cancellation
