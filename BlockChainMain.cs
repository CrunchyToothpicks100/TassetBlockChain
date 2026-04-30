using System.Reflection.PortableExecutable;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockChain
{
    class BlockChainMain
    {
        const string INPUT_FILE_NAME = @"C:\Users\USER\oliver\repos\TassetBlockChain\StudentBlockTasset.txt";
        const string OUTPUT_FILE_NAME = @"C:\Users\USER\oliver\repos\TassetBlockChain\Success.csv";
        const int BLOCKLINECOUNT = 4;
        static int difficulty = 9; // number of zeros

        static void Main(string[] args)
        {
            Console.WriteLine("Creating a new block from data file.\n");
            StreamReader fileIn = File.OpenText(INPUT_FILE_NAME);

            string blockLines = ReadABlockOfData(fileIn);
            Block block = new Block(blockLines);

            Console.WriteLine($"Mining block...\n");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { 
                e.Cancel = true; 
                cts.Cancel(); 
                Console.WriteLine("Tasks cancelled safely.");
            };

            using (StreamWriter fileOut = new StreamWriter(OUTPUT_FILE_NAME, true)) // append mode = true
            {
                block.MineBlock(
                    difficulty,
                    onHit: (hash, nonce) =>
                    {
                        Console.WriteLine($"Found hash!");
                        Console.WriteLine($"Time: {(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - block.TimeStamp) / 1000} seconds\n");
                        Console.WriteLine($"Data:\t\t{block.Data}\nPrevious Hash:\t{block.previousHash}\nTime Stamp:\t{block.TimeStamp}\nHash:\t\t{hash}\nNonce:\t\t{nonce:n0}\n");
                        fileOut.WriteLine($"{hash},{block.previousHash},{block.Data},{block.TimeStamp},{nonce}");
                        fileOut.Flush();
                    },
                    stopToken: cts.Token
                );
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
    }
}
