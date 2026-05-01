using System.Reflection.PortableExecutable;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockChain
{
    class BlockChainMain
    {
        const string INPUT_FILE_NAME = @"StudentBlockTasset.txt";
        const string OUTPUT_FILE_NAME = @"Success.csv";
        const string BACKUP_FILE_NAME = @"Success_backup.csv";

        const int BLOCKLINECOUNT = 4;
        static int difficulty = 9; // number of zeros

        static void Main(string[] args)
        {
            Console.WriteLine("Creating a new block from data file.");
            StreamReader fileIn = File.OpenText(INPUT_FILE_NAME);

            string blockLines = ReadABlockOfData(fileIn);
            Block block = new Block(blockLines);

            Console.WriteLine($"Mining block... \nCtrl+C to exit \n");

            using var cts = new CancellationTokenSource();

            // When Ctrl+C is pressed...
            Console.CancelKeyPress += (_, e) => {
                // Use cancellation token to signal the mining process to stop
                e.Cancel = true; 
                cts.Cancel(); 
                Console.WriteLine("Tasks cancelled safely.");

                // This method creates and/or opens the backup file and copies the output to it
                File.AppendAllText(BACKUP_FILE_NAME, File.ReadAllText(OUTPUT_FILE_NAME));
                Console.WriteLine($"Backup of '{OUTPUT_FILE_NAME}' created as '{BACKUP_FILE_NAME}'.");
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
            } // fileOut is automatically closed here
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
