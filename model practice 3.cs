using System;
using System.Collections.Generic;

namespace OnlineStoreSOLID
{
    // ============================
    // SRP – Single Responsibility
    // ============================
    public class Product
    {
        public string Name { get; }
        public double Price { get; }

        public Product(string name, double price)
        {
            Name = name;
            Price = price;
        }
    }

    public class OrderItem
    {
        public Product Product { get; }
        public int Quantity { get; }

        public OrderItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public double GetTotal() => Product.Price * Quantity;
    }

    public class Order
    {
        private readonly List<OrderItem> _items = new List<OrderItem>();

        public IReadOnlyList<OrderItem> Items => _items;
        public IPayment PaymentMethod { get; set; }
        public IDelivery DeliveryMethod { get; set; }

        public void AddItem(Product product, int quantity)
        {
            _items.Add(new OrderItem(product, quantity));
        }

        public double GetSubTotal()
        {
            double total = 0;
            foreach (var item in _items)
                total += item.GetTotal();
            return total;
        }
    }

    // ============================
    // OCP + LSP – Payments
    // ============================
    public interface IPayment
    {
        void ProcessPayment(double amount);
    }

    public class CreditCardPayment : IPayment
    {
        public void ProcessPayment(double amount)
        {
            Console.WriteLine($"Paid {amount} with Credit Card.");
        }
    }

    public class PayPalPayment : IPayment
    {
        public void ProcessPayment(double amount)
        {
            Console.WriteLine($"Paid {amount} via PayPal.");
        }
    }

    public class BankTransferPayment : IPayment
    {
        public void ProcessPayment(double amount)
        {
            Console.WriteLine($"Paid {amount} via Bank Transfer.");
        }
    }

    // ============================
    // OCP + LSP – Deliveries
    // ============================
    public interface IDelivery
    {
        void DeliverOrder(Order order);
    }

    public class CourierDelivery : IDelivery
    {
        public void DeliverOrder(Order order)
        {
            Console.WriteLine("Order will be delivered by courier.");
        }
    }

    public class PostDelivery : IDelivery
    {
        public void DeliverOrder(Order order)
        {
            Console.WriteLine("Order will be delivered by postal service.");
        }
    }

    public class PickUpPointDelivery : IDelivery
    {
        public void DeliverOrder(Order order)
        {
            Console.WriteLine("Order can be picked up from collection point.");
        }
    }

    // ============================
    // ISP – Notifications
    // ============================
    public interface INotification
    {
        void SendNotification(string message);
    }

    public class EmailNotification : INotification
    {
        public void SendNotification(string message)
        {
            Console.WriteLine($"Email notification: {message}");
        }
    }

    public class SmsNotification : INotification
    {
        public void SendNotification(string message)
        {
            Console.WriteLine($"SMS notification: {message}");
        }
    }

    // ============================
    // OCP – Discount Strategy
    // ============================
    public interface IDiscountStrategy
    {
        double ApplyDiscount(double amount);
    }

    public class NoDiscount : IDiscountStrategy
    {
        public double ApplyDiscount(double amount) => amount;
    }

    public class PercentageDiscount : IDiscountStrategy
    {
        private readonly double _percentage; // e.g. 0.1 = 10%
        public PercentageDiscount(double percentage)
        {
            _percentage = percentage;
        }

        public double ApplyDiscount(double amount) => amount * (1 - _percentage);
    }

    public class FixedAmountDiscount : IDiscountStrategy
    {
        private readonly double _amount;
        public FixedAmountDiscount(double amount)
        {
            _amount = amount;
        }

        public double ApplyDiscount(double amount) => Math.Max(0, amount - _amount);
    }

    public class DiscountCalculator
    {
        private readonly IDiscountStrategy _discountStrategy;
        public DiscountCalculator(IDiscountStrategy discountStrategy)
        {
            _discountStrategy = discountStrategy;
        }

        public double Calculate(double amount) => _discountStrategy.ApplyDiscount(amount);
    }

    // ============================
    // DIP – Order Processor
    // ============================
    public class OrderProcessor
    {
        private readonly INotification _notification;

        public OrderProcessor(INotification notification)
        {
            _notification = notification;
        }

        public void Process(Order order, DiscountCalculator discountCalculator)
        {
            // Calculate price
            double subTotal = order.GetSubTotal();
            double total = discountCalculator.Calculate(subTotal);

            // Process payment
            order.PaymentMethod.ProcessPayment(total);

            // Deliver order
            order.DeliveryMethod.DeliverOrder(order);

            // Send notification
            _notification.SendNotification($"Your order has been processed. Total = {total}");
        }
    }

    // ============================
    // Demo Program
    // ============================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Online Store Order System (SOLID Demo) ===");

            // Create order
            var order = new Order();
            order.AddItem(new Product("Laptop", 1000), 1);
            order.AddItem(new Product("Mouse", 50), 2);

            // Select payment and delivery methods
            order.PaymentMethod = new PayPalPayment();
            order.DeliveryMethod = new CourierDelivery();

            // Choose discount
            var discountCalculator = new DiscountCalculator(new PercentageDiscount(0.1)); // 10% off

            // Process order with notification
            var processor = new OrderProcessor(new EmailNotification());
            processor.Process(order, discountCalculator);

            Console.WriteLine("\n=== Another Example: SRP Ticket Purchase ===");

            // SRP small example: Ticket purchase
            var ticketOrder = new Order();
            ticketOrder.AddItem(new Product("Concert Ticket", 200), 2);
            ticketOrder.PaymentMethod = new CreditCardPayment();
            ticketOrder.DeliveryMethod = new PickUpPointDelivery();

            var ticketDiscount = new DiscountCalculator(new FixedAmountDiscount(50));
            var ticketProcessor = new OrderProcessor(new SmsNotification());
            ticketProcessor.Process(ticketOrder, ticketDiscount);
        }
    }
}
