namespace Module7
{


    // Класс Программ для тестирования входа выхода данных, мне так удобнее пока нас не научили создавать интерфейс пользователю и делать тесты
    class Program 
    {
        static void Main(string[] args)
        {

            
            // Вспоминаем чему учились и выносим ввод данных в отдельные методы
            var customer = GetCustomerDetails();
            var homeDelivery = GetDeliveryDetails();
            var cardPayment = GetPaymentDetails();
            var order = new Order<HomeDelivery, CardPayment>(homeDelivery, cardPayment, customer);

            while (true)
            {
                Console.WriteLine("Введите название товара (или введите 'end' для завершения ввода):");
                string productName = Console.ReadLine();
                if (productName.ToLower() == "end")
                {
                    break;
                }

                decimal productPrice = GetPositiveDecimal("Введите цену товара:\n");
                int productQuantity = (int)GetPositiveDecimal("Введите количество товара:\n");

                order.AddProduct(new Product(productName, productPrice, productQuantity));
            }

            CompleteAndDisplayOrder(order, customer, cardPayment);
        }

        static Customer GetCustomerDetails()
        {
            Console.WriteLine("Введите имя клиента:");
            string name = Console.ReadLine();
            Console.WriteLine("Введите фамилию клиента:");
            string lastName = Console.ReadLine();
            return new Customer(name, lastName);
        }

        static HomeDelivery GetDeliveryDetails()
        {
            Console.WriteLine("Введите адрес доставки:");
            string address = Console.ReadLine();
            Console.WriteLine("Введите название курьерской службы:");
            string deliveryCompany = Console.ReadLine();
            return new HomeDelivery(address, deliveryCompany);
        }

        //Здесь сделал по простому без всякой валидации тк практикуем ООП
        static CardPayment GetPaymentDetails()
        {
            Console.WriteLine("Введите номер карты:");
            string cardNumber = Console.ReadLine();

            Console.WriteLine("Введите дату окончания действия карты (мм/гг):");
            string expirationDate = Console.ReadLine();

            int securityCode = Get3DSecurityCode();
            return new CardPayment(cardNumber, expirationDate, securityCode);
        }



        static int Get3DSecurityCode()
        {
            int securityCode;
            while (!int.TryParse(Console.ReadLine(), out securityCode) || securityCode.ToString().Length != 3)
            {
                Console.WriteLine("CVV должен состоять из 3-х цифр. Пожалуйста, введите код:");
            }
            return securityCode;
        }

        static decimal GetPositiveDecimal(string message)
        {
            decimal number;
            while (true)
            {
                Console.Write(message);
                if (decimal.TryParse(Console.ReadLine(), out number) && number > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Число неверное. Введите положительное число.");
                }
            }
            return number;
        }

        static void CompleteAndDisplayOrder(Order<HomeDelivery, CardPayment> order, Customer customer, CardPayment cardPayment)
        {
            if (order.CompleteOrder())
            {
                Console.WriteLine("Заказ успешно завершен!");

                Console.WriteLine("Данные клиента:");
                Console.WriteLine($"Имя: {customer.Name}, Фамилия: {customer.LastName}");
                Console.WriteLine($"Номер карты: {cardPayment.CardNumber}, Дата истечения: {cardPayment.ExpirationDate}");

                order.DisplayProducts();
            }
            else
            {
                Console.WriteLine("Произошла ошибка при завершении заказа.");
            }
        }
    }

    public abstract class Delivery
    {
        protected string Address { get; }
        protected Delivery(string address) => Address = address;

        public virtual void DisplayAddress() => Console.WriteLine(Address);
        public abstract void Deliver();
    }

    public class HomeDelivery : Delivery
    {
        private readonly string _deliveryCompany;

        public HomeDelivery(string address, string deliveryCompany) : base(address) => _deliveryCompany = deliveryCompany;

        public override void Deliver() => Console.WriteLine($"Доставка на дом через компанию {_deliveryCompany}");
    }

    public class Order<TDelivery, TPayment>
        where TDelivery : Delivery
        where TPayment : PaymentMethod
    {
        private Product[] _products;
        private int _productCount;
        private TDelivery _delivery;
        private TPayment _paymentMethod;

        internal decimal TotalPrice { get; private set; }

        protected Customer Customer { get; }

        public Order(TDelivery delivery, TPayment paymentMethod, Customer customer)
        {
            _delivery = delivery;
            _paymentMethod = paymentMethod;
            Customer = customer;
            _products = new Product[100]; // Максимальное количество продуктов
            TotalPrice = 0; // Инициализируем общую стоимость
        }


        //Неуверен что индекс сделал правильно и не до конца пока понимаю его практическую ценность, возможно в этом контексте он не уместен
        public Product this[int index]
        {
            get
            {
                if (index < 0 || index >= _productCount)
                    throw new IndexOutOfRangeException("Недоступный индекс.");
                return _products[index];
            }
        }

        public void AddProduct(Product product)
        {
            if (_productCount < _products.Length)
            {
                _products[_productCount++] = product;
                CalculateTotalPrice();
            }
            else
            {
                Console.WriteLine("Достигнуто максимальное количество продуктов в заказе!");
            }
        }

        protected void CalculateTotalPrice()
        {
            TotalPrice = 0; // Сбрасываем общую стоимость
            for (int i = 0; i < _productCount; i++)
            {
                TotalPrice += _products[i].Price * _products[i].Quantity;
            }
        }

        public void DisplayProducts()
        {
            Console.WriteLine("Заказанные продукты:");
            for (int i = 0; i < _productCount; i++)
            {
                Console.WriteLine($"- {_products[i].Name}, Цена: {_products[i].Price}, Количество: {_products[i].Quantity}");
            }
            Console.WriteLine($"Общая стоимость: {TotalPrice}");
        }

        public bool CompleteOrder() => _paymentMethod.ProcessPayment(TotalPrice);
    }

    public class Product
    {
        public string Name { get; }
        public int Quantity { get; }
        private decimal _price;

        public Product(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }


        // Там валидация тоже есть на вводе, а если ее не будет там? Добавим логику в свойство
        public decimal Price
        {
            get => _price;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("Цена должна быть положительной.");
                _price = value;
            }
        }


        //Перегрузка, тоже не совсе уверен что так делают в продуктовой жизни
        public static bool operator ==(Product a, Product b) => a.Name == b.Name;
        public static bool operator !=(Product a, Product b) => !(a == b);
        public override bool Equals(object obj) => obj is Product product && Name == product.Name;
        public override int GetHashCode() => HashCode.Combine(Name);
    }

    public class Customer
    {
        public string Name { get; }
        public string LastName { get; }

        public Customer(string firstName, string lastName)
        {
            Name = firstName;
            LastName = lastName;
        }
    }

    public abstract class PaymentMethod
    {
        public string Description { get; set; }
        public abstract bool ProcessPayment(decimal amount);
    }

    public class CardPayment : PaymentMethod
    {
        public string CardNumber { get; }
        public string ExpirationDate { get; }
        public int SecurityCode { get; }

        public CardPayment(string cardNumber, string expirationDate, int securityCode)
        {
            CardNumber = cardNumber;
            ExpirationDate = expirationDate;
            SecurityCode = securityCode;
        }

        public override bool ProcessPayment(decimal amount)
        {
            // Логика обработки платежа
            return true;
        }
    }

    public class CashOnDeliveryPayment : PaymentMethod
    {
        public override bool ProcessPayment(decimal amount)
        {
            // Логика обработки наличного расчета
            return true;
        }
    }
}