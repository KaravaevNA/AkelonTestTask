namespace PracticeTask3
{
    internal class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string UnitOfMeasure { get; private set; }
        public int Price { get; private set; }

        public Product(int id, string name, string unitOfMeasure, int price)
        {
            Id            = id;
            Name          = name;
            UnitOfMeasure = unitOfMeasure;
            Price         = price;
        }
    }
}
