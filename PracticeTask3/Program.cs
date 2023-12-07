namespace PracticeTask3
{
    class Program
    {
        static void Main()
        {
            ConsoleUI.Output("Введите путь к файлу с данными:");
            string filePath = ConsoleUI.Input();
            using (DataProcessor dataProcessor = new DataProcessor(filePath))
            {
                dataProcessor.Menu();
            }
            ConsoleUI.Input();
        }
    }
}