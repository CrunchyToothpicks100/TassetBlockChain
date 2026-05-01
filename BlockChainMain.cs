using System.Reflection.PortableExecutable;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockChain
{
    class BlockChainMain
    {
        const string INPUT_FILE = @"StudentBlockTasset.txt";
        const string OUTPUT_FILE = @"Success.csv";
        const string BACKUP_FILE = @"Success_backup.csv"; // Not tracked by git

        const int BLOCKLINECOUNT = 4;
        static int difficulty = 9; // number of zeros

        static void Main(string[] args)
        {
            StreamReader fileIn = File.OpenText(INPUT_FILE);

            string blockLines = ReadABlockOfData(fileIn);
            Block block = new Block(blockLines);
            Console.WriteLine("\nCreated a new block from data file.");

            Console.WriteLine("\nUse only half CPU power to mine the block? (y/n)");
            bool halfPower = Console.ReadLine()?.Trim().ToLower() == "y";

            using var cts = new CancellationTokenSource();

            // When Ctrl+C is pressed...
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; 
                cts.Cancel(); 
                Console.WriteLine("Tasks cancelled safely.");
            };

            using StreamWriter fileOut = new StreamWriter(OUTPUT_FILE, true); // append mode = true
            using StreamWriter fileBackup = new StreamWriter(BACKUP_FILE, true);

            Console.WriteLine($"\nMining block... (Ctrl+C to exit) \n");

            block.MineBlock(
                difficulty,
                halfPower,
                onHit: (hash, nonce) =>
                {
                    Console.WriteLine($"Found hash!");
                    Console.WriteLine($"Time: {(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - block.TimeStamp) / 1000} seconds\n");
                    Console.WriteLine($"Data:\t\t{block.Data}\nPrevious Hash:\t{block.previousHash}\nTime Stamp:\t{block.TimeStamp}\nHash:\t\t{hash}\nNonce:\t\t{nonce:n0}\n");
                    fileOut.WriteLine($"{hash},{block.previousHash},{block.Data},{block.TimeStamp},{nonce}");
                    fileOut.Flush();
                    fileBackup.WriteLine($"{hash},{block.previousHash},{block.Data},{block.TimeStamp},{nonce}");
                    fileBackup.Flush();
                },
                stopToken: cts.Token
            );
            // fileOut is automatically closed here
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
