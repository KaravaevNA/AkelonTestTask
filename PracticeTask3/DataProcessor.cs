using ClosedXML.Excel;
using System.Data;
using System.Globalization;

namespace PracticeTask3
{
    public class DataProcessor : IDisposable
    {
        private string FilePath { get; set; }
        private readonly XLWorkbook _workbook;
        private readonly IXLWorksheet _productsWorksheet;
        private readonly IXLWorksheet _clientsWorksheet;
        private readonly IXLWorksheet _ordersWorksheet;

        private delegate void DataProcessorOutputHandler(string message, bool isNewLine = true);
        private event DataProcessorOutputHandler Output;
        private event Func<string> Input;

        private bool IsDisposed { get; set; }

        public DataProcessor(string filePath)
        {
            Output += ConsoleUI.Output;
            Input += ConsoleUI.Input;

            FilePath = filePath;
            while (true)
            {
                try
                {
                    _workbook = new XLWorkbook(FilePath);
                    break;
                }
                catch (Exception ex)
                {
                    Output.Invoke($"Ошибка при открытии файла: {ex.Message}");
                    Output.Invoke("Пожалуйста, введите путь к файлу заново:");
                    FilePath = Input.Invoke();
                }
            }
            _productsWorksheet = _workbook.Worksheet("Товары");
            _clientsWorksheet = _workbook.Worksheet("Клиенты");
            _ordersWorksheet = _workbook.Worksheet("Заявки");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            _workbook?.Dispose();
            IsDisposed = true;
        }

        ~DataProcessor() => Dispose(false);

        public void Menu()
        {
            while (true)
            {
                Output.Invoke("Выберите команду:");
                Output.Invoke("1. Просмотреть заказы товара");
                Output.Invoke("2. Изменить контактное лицо клиента");
                Output.Invoke("3. Определить золотого клиента");
                Output.Invoke("4. Выход");
                Output.Invoke("Введите номер команды: ", false);
                string command = Input.Invoke();

                switch (command)
                {
                    case "1":
                        ViewProductOrders();
                        break;
                    case "2":
                        ChangeContactPerson();
                        break;
                    case "3":
                        FindGoldenClient();
                        break;
                    case "4":
                        return;
                    default:
                        Output.Invoke("Неверная команда, попробуйте еще раз");
                        break;
                }
            }
        }

        private void ViewProductOrders()
        {
            Output.Invoke("Введите наименование товара: ", false);
            string productName = Input.Invoke().ToUpper();

            bool isProductFound = false;

            var productsRows = _productsWorksheet.RowsUsed().Skip(1);
            foreach (var productRow in productsRows)
            {
                if (productRow.Cell("B").Value.ToString().ToUpper() == productName)
                {
                    Product product = new Product
                    (
                        int.Parse(productRow.Cell("A").Value.ToString()),
                        productRow.Cell("B").Value.ToString(),
                        productRow.Cell("C").Value.ToString(),
                        int.Parse(productRow.Cell("D").Value.ToString())
                    );

                    List<Order> orders = new List<Order>();

                    var ordersRows = _ordersWorksheet.RowsUsed().Skip(1);
                    foreach (var orderRow in ordersRows)
                    {
                        if (orderRow.Cell("B").Value.ToString() == product.Id.ToString())
                        {
                            Order order = new Order
                            (
                                int.Parse(orderRow.Cell("A").Value.ToString()),
                                int.Parse(orderRow.Cell("B").Value.ToString()),
                                int.Parse(orderRow.Cell("C").Value.ToString()),
                                int.Parse(orderRow.Cell("D").Value.ToString()),
                                int.Parse(orderRow.Cell("E").Value.ToString()),
                                DateOnly.ParseExact(orderRow.Cell("F").Value.ToString(), "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture)
                            );
                            order.Product = product;
                            orders.Add(order);
                        }
                    }

                    foreach (var order in orders)
                    {
                        var clientsRows = _clientsWorksheet.RowsUsed().Skip(1);
                        foreach (var clientRow in clientsRows)
                        {
                            if (clientRow.Cell("A").Value.ToString() == order.ClientId.ToString())
                            {
                                Client client = new Client
                                (
                                    int.Parse(clientRow.Cell("A").Value.ToString()),
                                    clientRow.Cell("B").Value.ToString(),
                                    clientRow.Cell("C").Value.ToString(),
                                    clientRow.Cell("D").Value.ToString()
                                );
                                order.Client = client;
                                break;
                            }
                        }
                    }

                    Output.Invoke($"\nКоличество заказов товара {product.Name}: {orders.Count}\n");

                    foreach (var order in orders)
                    {
                        Output.Invoke($"Клиент: {order.Client?.Name ?? "данные о клиенте не найдены в файле"}");
                        Output.Invoke($"Количество товара: {order.ProductAmount} {order.Product.UnitOfMeasure}");
                        Output.Invoke($"Цена за {order.Product.UnitOfMeasure}: {order.Product.Price}");
                        Output.Invoke($"Общая цена заказа: {order.ProductAmount * order.Product.Price}");
                        Output.Invoke($"Дата заказа: {order.Date}\n");
                    }

                    isProductFound = true;
                    break;
                }
            }

            if (!isProductFound)
                Output.Invoke("\nТовара с подобным названием в таблице нет\n");
        }

        private void ChangeContactPerson()
        {
            Output.Invoke("Введите наименование организации: ", false);
            string organization = Input.Invoke().ToUpper();
            Output.Invoke("Введите ФИО нового контактного лица: ", false);
            string contactPerson = Input.Invoke();

            bool isClientFound = false;

            var clientsRows = _clientsWorksheet.RowsUsed().Skip(1);
            foreach (var clientRow in clientsRows)
            {
                if (clientRow.Cell("B").Value.ToString().ToUpper().Contains(organization))
                {
                    if (clientRow.Cell("B").Value.ToString().ToUpper() != organization)
                    {
                        Output.Invoke($"Найдено похожее наименование организации: {clientRow.Cell("B").Value}");
                        Output.Invoke($"Вы имели в виду ее? Да/Нет: ", false);
                        if (Input.Invoke().ToUpper() == "ДА")
                            isClientFound = true;
                    }
                    else
                        isClientFound = true;

                    if (isClientFound)
                    {
                        Output.Invoke($"\nСтарое контактное лицо {clientRow.Cell("B").Value}: {clientRow.Cell("D").Value}");
                        clientRow.Cell("D").Value = contactPerson;
                        Output.Invoke($"Новое контактное лицо {clientRow.Cell("B").Value}: {clientRow.Cell("D").Value}");
                        _workbook.Save();
                        Output.Invoke("Изменения сохранены\n");

                        break;
                    }
                }
            }

            if (!isClientFound)
            {
                Output.Invoke("\nОрганизация с подобным наименованием не найдена в таблице\n");
            }
        }

        private void FindGoldenClient()
        {
            Output.Invoke("Будет осуществлен поиск клиента с наибольшим числом заказов за указанный период");
            int desiredYear;
            int desiredMonth;
            bool isCorrectYear = false;
            bool isCorrectMonth = false;

            while (true)
            {
                Output.Invoke("Введите год: ", false);
                isCorrectYear = int.TryParse(Input.Invoke(), out desiredYear);
                Output.Invoke("Введите месяц: ", false);
                isCorrectMonth = int.TryParse(Input.Invoke(), out desiredMonth);

                if (isCorrectYear && isCorrectMonth)
                    break;
                else
                    Output.Invoke("Допущена ошибка во введенных значения искомого года и месяца");
            }

            var clientIdOrdersCount = new Dictionary<int, int>();
            var ordersRows = _ordersWorksheet.RowsUsed().Skip(1);

            foreach (var orderRow in ordersRows)
            {
                DateOnly date = DateOnly.ParseExact(orderRow.Cell("F").Value.ToString(), "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture);

                if (date.Year == desiredYear && date.Month == desiredMonth)
                {
                    int clientId = int.Parse(orderRow.Cell("C").Value.ToString());
                    if (clientIdOrdersCount.ContainsKey(clientId))
                        clientIdOrdersCount[clientId]++;
                    else
                        clientIdOrdersCount.Add(clientId, 1);
                }
            }

            if (clientIdOrdersCount.Count == 0)
            {
                Output.Invoke("\nВ таблице заказов не удалось найти клиентов по заданным критериям\n");
                return;
            }

            int maxOrdersCount = clientIdOrdersCount.Max(x => x.Value);
            List<int> IdMaxOrdersCount = clientIdOrdersCount.Where(x => x.Value == maxOrdersCount)
                                                            .Select(x => x.Key)
                                                            .ToList();
            List<Client> clientsMaxOrders = new List<Client>();
            var clientsRows = _clientsWorksheet.RowsUsed().Skip(1);

            foreach (var clientRow in clientsRows)
            {
                foreach (var id in IdMaxOrdersCount)
                {
                    if (int.Parse(clientRow.Cell("A").Value.ToString()) == id)
                    {
                        clientsMaxOrders.Add(new Client
                        (
                            int.Parse(clientRow.Cell("A").Value.ToString()),
                            clientRow.Cell("B").Value.ToString(),
                            clientRow.Cell("C").Value.ToString(),
                            clientRow.Cell("D").Value.ToString()
                        ));
                    }
                }
            }

            if (clientsMaxOrders.Count != 0)
            {
                Output.Invoke($"\nМаксимальное количество заказов: {maxOrdersCount}");
                Output.Invoke($"Клиентов с максимальным числом заказов: {clientsMaxOrders.Count}\n");

                foreach (var client in clientsMaxOrders)
                {
                    Output.Invoke($"Наименование организации: {client.Name}");
                    Output.Invoke($"Адрес: {client.Address}");
                    Output.Invoke($"Контактное лицо: {client.Person}\n");
                }
            }
            else
                Output.Invoke("\nВ таблице клиентов не удалось найти клиента, который числится в таблице заявок с максимальным числом заказов\n");
        }
    }
}
