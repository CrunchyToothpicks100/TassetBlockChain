# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build          # compile
dotnet run            # build and run (reads StudentBlockTasset.txt, writes BlockChain.csv)
dotnet run --project TassetBlockChain.csproj
```

There are no tests. Build errors are the primary correctness signal.

## Architecture

A .NET 10 console app that mines and validates a simple blockchain from a flat text file of transactions.

**Data flow:**
1. `StudentBlockTasset.txt` → `BlockList.CreateNewBlockChain()` → mines each block → `BlockChain.csv`
2. Each block contains 4 concatenated lines from the input file as its `Data` field.
3. Input line format: `*sender pays recipient amount txid*`

**Hash format — important invariant:** Hashes are produced by `StringUtil.ApplySha256` using `SHA256.HashData` and formatted with `BitConverter.ToString`, which yields uppercase hex with dashes: `"AB-CD-EF-..."`. This non-standard format is used consistently across mining, validation, and CSV storage — do not change the format without regenerating the CSV.

**Mining difficulty:** Controlled by `BlockList.difficulty` (currently 6). The mining target is computed as `difficulty + difficulty/2` leading characters of the zero-byte target string in the dash-separated format. For `difficulty=6`, the prefix checked is 9 characters (`"00-00-00-0"`). Both `MineBlock` and `IsChainValid` compute this the same way — they must stay in sync.

**File I/O pattern:** All file handles are opened by helper methods (`OpenInputFile`, `OpenCSVInputFile`, `OpenOutputFile`) and consumed via `using` declarations in the calling method. `TextFieldParser` (from `Microsoft.VisualBasic.FileIO`) handles CSV parsing.

**Class responsibilities:**
- `Block` — entity + `CalculateHash()` + `MineBlock(difficulty)`
- `BlockList` — `CreateNewBlockChain`, `ReadBlockChainFromFile`, `WriteBlockChainToFile`, `IsChainValid`
- `StringUtil` — single static `ApplySha256(string)` method
- `BlockChainMain` — entry point only; no application logic
