namespace PracticeTask3
{
    public static class ConsoleUI
    {
        public static void Output(string message, bool isNewLine = true)
        {
            if (isNewLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
        }

        public static string Input()
        {
            return Console.ReadLine() ?? string.Empty;
        }
    }
}
