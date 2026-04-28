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

        public Block(string data, string previousHash)
        {
            Data = data;
            this.previousHash = previousHash;
            TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Hash = CalculateHash();
        }

        public Block(string data, string previousHash, long timeStamp, string hash, ulong nonce)
        {
            Data = data;
            this.previousHash = previousHash;
            TimeStamp = timeStamp;
            Hash = hash;
            Nonce = nonce;
        }

        public string CalculateHash() =>
            StringUtil.ApplySha256(previousHash + TimeStamp + Nonce + Data);

        public void MineBlock(int difficulty)
        {
            byte[] ba = Encoding.UTF8.GetBytes(new string('\0', difficulty));
            string target = BitConverter.ToString(ba);

            // Adjust length to account for dashes in BitConverter format (e.g. "00-00-00")
            difficulty += difficulty / 2;

            while (Hash.Substring(0, difficulty) != target.Substring(0, difficulty))
            {
                Nonce++;
                Hash = CalculateHash();
            }
            Console.WriteLine($"Block Mined:\t{Hash}");
            Console.WriteLine($"Time:\t{(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TimeStamp) / 1000} seconds\n");
        }

        public override string ToString() =>
            $"Data:\t\t{Data}\nPrevious Hash:\t{previousHash}\nTime Stamp:\t{TimeStamp}\nHash:\t\t{Hash}\nNonce:\t\t{Nonce:n0}\n";
    }
}
