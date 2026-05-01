using System.Security.Cryptography;
using System.Text;

namespace BlockChain
{
    public class Block
    {
        public string Hash { get; set; }
        public string previousHash;
        public string Data { get; set; }
        public long TimeStamp { get; set; }
        public ulong Nonce { get; set; }

        public Block(string data)
        {
            previousHash = "0";
            Data = data;
            TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Nonce = 0;
            Hash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(previousHash + TimeStamp + Nonce + Data)));
        }

        public void MineBlock(int difficulty, Action<string, ulong>? onHit = null, CancellationToken stopToken = default)
        {
            // 00 = One byte of zeros
            int zeroBytes = difficulty / 2;
            bool requireHalfByte = (difficulty % 2) != 0;

            // Precompute constant parts of the input: [prefix][nonce][suffix]
            // This avoids rebuilding the entire string every iteration
            byte[] staticPrefix = Encoding.UTF8.GetBytes(previousHash + TimeStamp);
            byte[] staticSuffix = Encoding.UTF8.GetBytes(Data);

            // Use all CPU cores
            int threadCount = Environment.ProcessorCount;
            Console.WriteLine($"Threads: {threadCount}\n");

            // Store the FIRST valid result found
            ulong firstNonce = 0;
            string firstHash = string.Empty;

            // Lock to protect shared state across threads
            object lockObj = new();

            // Launch one task per CPU core
            Task[] tasks = Enumerable.Range(0, threadCount).Select(i => Task.Run(() =>
            {
                // Buffer for input: [prefix][nonce][suffix]
                byte[] inputBuf = new byte[staticPrefix.Length + 20 + staticSuffix.Length];
                staticPrefix.CopyTo(inputBuf, 0);

                Span<byte> hashBuf = stackalloc byte[32];
                Span<char> nonceChars = stackalloc char[20];
                ulong nonce = (ulong)(threadCount + i);
                int prevNonceLen = -1;

                while (!stopToken.IsCancellationRequested)
                {
                    nonce.TryFormat(nonceChars, out int nonceLen);

                    for (int j = 0; j < nonceLen; j++)
                        inputBuf[staticPrefix.Length + j] = (byte)nonceChars[j];

                    // avoids redundantly overwriting the suffix region of inputBuf when the nonce's digit count hasn't changed
                    if (nonceLen != prevNonceLen)
                    {
                        staticSuffix.CopyTo(inputBuf, staticPrefix.Length + nonceLen);
                        prevNonceLen = nonceLen;
                    }

                    SHA256.TryHashData(
                        inputBuf.AsSpan(0, staticPrefix.Length + nonceLen + staticSuffix.Length),
                        hashBuf, out _);

                    if (MeetsTarget(hashBuf, zeroBytes, requireHalfByte))
                    {
                        string hashStr = BitConverter.ToString(hashBuf.ToArray());
                        lock (lockObj)
                        {
                            if (firstHash == string.Empty)
                            {
                                firstNonce = nonce;
                                firstHash = hashStr;
                            }
                            onHit?.Invoke(hashStr, nonce);
                        }
                    }
                    nonce += (ulong)(threadCount);
                }
            })).ToArray();

            Task.WaitAll(tasks);

            Nonce = firstNonce;
            Hash = firstHash;
        }

        // Compare hash bytes directly against the zero target without converting to a string. 00 = One byte of zeros.
        static bool MeetsTarget(ReadOnlySpan<byte> hash, int zeroBytes, bool requireHalfByte)
        {
            // Check full zero bytes
            for (int i = 0; i < zeroBytes; i++)
            {
                if (hash[i] != 0)
                    return false;
            }

            // If odd number of hex zeros, check upper nibble (4 bits)
            if (requireHalfByte)
            {
                if ((hash[zeroBytes] >> 4) != 0)
                    return false;
            }

            return true;
        }
    }
}
