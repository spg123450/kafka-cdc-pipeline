using Topics;

namespace CDCTarget.Modules
{
    public class Processor
    {
        public void ProcessAndSync(CdcEvent payload)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[RESULT SYNCHRONIZER] Successfully Synced Document ID: {payload.DocumentKey?._id}");
            Console.ResetColor();
        }
    }
}