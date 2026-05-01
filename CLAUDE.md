# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build
dotnet run
dotnet run --project TassetBlockChain.csproj
```

There are no tests. Build errors are the primary correctness signal.

## Architecture

A .NET 10 console app that mines a single blockchain block from a flat text file of transactions.

**Data flow:**
1. `BlockChainMain` reads `StudentBlockTasset.txt`, concatenates 4 lines into one `Data` string, constructs a `Block`, then calls `MineBlock`.
2. Each valid hash found is appended to `Success.csv` as: `hash,previousHash,data,timestamp,nonce`.
3. Ctrl+C triggers `CancellationToken` cancellation and optionally copies `Success.csv` to `History.csv`.

**Input line format:** `*sender pays recipient amount txid*`

**Mining — how difficulty maps to the byte-level target:**
- `difficulty` (set in `BlockChainMain`, currently 9) controls how many leading hex zeros are required.
- `zeroBytes = difficulty / 2` — number of full zero bytes checked.
- `requireHalfByte = (difficulty % 2 != 0)` — if odd, the upper nibble of the next byte must also be zero.
- Example: `difficulty=9` → 4 full zero bytes + upper nibble of byte 5 = 0, equivalent to 9 leading hex zeros.
- `MeetsTarget` compares raw `SHA256` bytes (no string conversion in the hot path).

**Parallelism:** `MineBlock` spawns one `Task` per `Environment.ProcessorCount` core. Each task owns a thread-local input buffer and stack-allocated hash/nonce buffers. Nonces are striped: thread `i` starts at `threadCount + i` and increments by `threadCount`. A `lock` ensures only the first valid result is recorded.

**Hash format:** `SHA256.HashData` output formatted via `BitConverter.ToString` → uppercase hex with dashes (`"AB-CD-EF-..."`). Used consistently in the `Block` constructor and `MineBlock`'s `onHit` callback — do not change without regenerating `Success.csv`.

**Key optimization in `MineBlock`:** The input buffer is laid out as `[prefix][nonce][suffix]`. The suffix (`Data`) is only copied into the buffer when the nonce's digit count changes, avoiding redundant writes on most iterations.

**Class responsibilities:**
- `Block` — entity fields + `CalculateHash` (constructor) + `MineBlock` + `MeetsTarget`
- `BlockChainMain` — entry point, file I/O, `CancellationTokenSource` wiring, `onHit` callback

**Note:** `ILGPU` is listed as a package dependency but is not used in the current source.
