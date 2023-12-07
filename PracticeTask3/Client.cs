namespace PracticeTask3
{
    internal class Client
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Person { get; private set; }

        public Client(int id, string name, string address, string person)
        {
            Id      = id;
            Name    = name;
            Address = address;
            Person  = person;
        }
    }
}
