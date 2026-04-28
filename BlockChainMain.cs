namespace BlockChain
{
    class BlockChainMain
    {
        const string INPUT_FILE_NAME = @"..\..\..\StudentBlockTasset.txt";
        const string OUTPUT_FILE_NAME = @"..\..\..\BlockChain.csv";

        static void Main(string[] args)
        {
            var blockchain = BlockList.CreateNewBlockChain(INPUT_FILE_NAME);

            Console.WriteLine("Displaying blockchain.\n");
            foreach (Block block in blockchain)
                Console.WriteLine(block);

            Console.WriteLine("The blockchain is " +
               (BlockList.IsChainValid(blockchain) ? "valid." : "invalid."));

            Console.WriteLine("\nAttempting to save blockchain to file.");
            BlockList.WriteBlockChainToFile(OUTPUT_FILE_NAME, blockchain);
        }
    }
}
