namespace PracticeTask3
{
    internal class Order
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public int ClientId { get; private set; }
        public int Number { get; private set; }
        public int ProductAmount { get; private set; }
        public DateOnly Date { get; private set; }

        public Product? Product { get; set; }
        public Client? Client { get; set; }

        public Order(int id, int productId, int clientId, int number, int productAmount, DateOnly date)
        {
            Id            = id;
            ProductId     = productId;
            ClientId      = clientId;
            Number        = number;
            ProductAmount = productAmount;
            Date          = date;
        }
    }
}
