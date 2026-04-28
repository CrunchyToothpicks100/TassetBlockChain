using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace BlockChain
{
    class BlockList
    {
        const int BLOCKLINECOUNT = 4;
        static int difficulty = 6;

        public static List<Block> CreateNewBlockChain(string dataInputPath)
        {
            List<Block> blockchain = new List<Block>();
            int count = 0;

            Console.WriteLine("Creating a new blockchain from data file.\n");
            using StreamReader fileIn = OpenInputFile(dataInputPath);

            string blockLines = ReadABlockOfData(fileIn);
            Console.WriteLine("Blockchain is being created...\n");

            blockchain.Add(new Block(blockLines, "0"));
            Console.WriteLine("Trying to Mine block 1... ");
            blockchain[0].MineBlock(difficulty);

            while (fileIn.Peek() > -1)
            {
                blockLines = ReadABlockOfData(fileIn);
                blockchain.Add(new Block(blockLines, blockchain[count++].Hash));
                Console.WriteLine($"Trying to Mine block {count + 1}... ");
                blockchain[count].MineBlock(difficulty);
            }

            Console.WriteLine("Blockchain successfully created from data file.\n");
            return blockchain;
        }

        static StreamReader OpenInputFile(string dataInputPath)
        {
            try
            {
                var reader = File.OpenText(dataInputPath);
                Console.WriteLine($"{dataInputPath} was opened\n");
                return reader;
            }
            catch (IOException)
            {
                Console.WriteLine($"Error: {dataInputPath} does not exist\n");
                Environment.Exit(-1);
                return null!;
            }
        }

        static string ReadABlockOfData(StreamReader fileIn)
        {
            var totalLinesIn = new StringBuilder();
            int count = 0;

            while (count < BLOCKLINECOUNT && fileIn.ReadLine() is string lineIn)
            {
                totalLinesIn.Append(lineIn);
                count++;
            }
            return totalLinesIn.ToString();
        }

        public static List<Block> ReadBlockChainFromFile(string dataInputPath)
        {
            List<Block> blockchain = new List<Block>();

            using TextFieldParser fileInCSV = OpenCSVInputFile(dataInputPath);

            while (fileInCSV.ReadFields() is string[] words)
            {
                string hash = words[0];
                string previousHash = words[1];
                string data = words[2];
                long timeStamp = long.Parse(words[3]);
                ulong nonce = ulong.Parse(words[4]);

                blockchain.Add(new Block(data, previousHash, timeStamp, hash, nonce));
            }

            Console.WriteLine("Blockchain successfully read from file.");
            return blockchain;
        }

        static TextFieldParser OpenCSVInputFile(string dataInputPath)
        {
            try
            {
                var parser = new TextFieldParser(dataInputPath);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = false;
                Console.WriteLine($"{dataInputPath} was opened");
                return parser;
            }
            catch (IOException)
            {
                Console.WriteLine($"Error: {dataInputPath} does not exist\n");
                Environment.Exit(-1);
                return null!;
            }
        }

        static StreamWriter OpenOutputFile(string blockChainOutputPath)
        {
            try
            {
                var writer = File.CreateText(blockChainOutputPath);
                Console.WriteLine($"{blockChainOutputPath} was created");
                return writer;
            }
            catch (IOException)
            {
                Console.WriteLine($"Error: {blockChainOutputPath} could not be created\n");
                Environment.Exit(-1);
                return null!;
            }
        }

        public static void WriteBlockChainToFile(string blockChainOutputPath, List<Block> blockchain)
        {
            using StreamWriter fileOut = OpenOutputFile(blockChainOutputPath);
            foreach (Block block in blockchain)
                fileOut.WriteLine($"{block.Hash},{block.previousHash},{block.Data},{block.TimeStamp},{block.Nonce}");

            Console.WriteLine($"Blockchain successfully saved in {blockChainOutputPath}.");
        }

        public static bool IsChainValid(List<Block> blockchain)
        {
            byte[] ba = Encoding.UTF8.GetBytes(new string('\0', difficulty));
            string hashTarget = BitConverter.ToString(ba);
            int difficultyPlusDashes = difficulty + difficulty / 2;

            if (blockchain[0].Hash != blockchain[0].CalculateHash())
            {
                Console.WriteLine("Block 0 has an invalid hash digest");
                return false;
            }

            for (int i = 1; i < blockchain.Count; i++)
            {
                Block currentBlock = blockchain[i];
                Block previousBlock = blockchain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    Console.WriteLine($"Block number {i + 1} has an invalid hash digest");
                    return false;
                }

                if (previousBlock.Hash != currentBlock.previousHash)
                {
                    Console.WriteLine("Previous Hashes not equal");
                    Console.WriteLine($"Block number {i + 1} previous hash digest does not equal block number {i} hash digest");
                    return false;
                }

                if (currentBlock.Hash.Substring(0, difficultyPlusDashes) !=
                    hashTarget.Substring(0, difficultyPlusDashes))
                {
                    Console.WriteLine($"Block number {i + 1} hasn't been mined");
                    return false;
                }
            }
            return true;
        }
    }
}
