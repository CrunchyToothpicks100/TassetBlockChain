# TassetBlockChain

## What it does

1. Take transaction data from StudentBlockTasset.txt
2. Use transaction data to create first block (first block will always have "0" as the previous hash)
3. Mine the block over and over again, writing any successful output to Success.csv
4. Does not terminate until you kill the terminal

## Install and Run

1. Install .NET 10.0 SDK with `winget install Microsoft.DotNet.SDK.10`
2. Copy raw .zip file from repository
3. Extract to folder of choice
4. Open terminal and `cd` into the repository
5. Open BlockChainMain.cs and update the file paths, or difficulty
6. Open Block.cs and change 'ProcessorCount / 2' to 'ProcessorCount' for full throttle
7. Save and close text editors
8. `dotnet run`

## How is this different from 2BlockChain?

2BlockChain builds a blockchain from a file using a set number of "lines" for each block. TassetBlockChain is not _really_ a blockchain. It takes one block and mines it over and over, writing all successful output until the user kills the process. The lack of termination is intentional. You might get really lucky and get a hash with more zeros than your chosen difficulty.

Some technical differences in the new Block.cs
- Spawns a set number of parallel tasks, each given their own nonce space
- Checks raw bytes directly via MeetsTarget(), comparing zero bytes and a half-nibble. No string allocation.
- Precomputes the static prefix/suffix bytes once, reuses a single inputBuf, and uses stackalloc for the hash buffer and nonce chars. Zero heap allocation per iteration.
- Threads run until the CancellationToken is cancelled. They record the first hit but keep mining until the caller stops them externally.
- MineBlock(int difficulty, Action<string, ulong>? onHit, CancellationToken stopToken) - supports a callback on every valid hash found and cooperative cancellation
