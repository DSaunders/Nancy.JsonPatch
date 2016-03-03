namespace DemoApp
{
    using System;

    public class Repository : IRepository
    {
        private Customer _customer = new Customer {Id = Guid.Parse("688B1F53-7FC5-41ED-A6E7-B4F1DA1A30AA")};

        public Customer GetCustomer(Guid id)
        {
            return _customer;
        }
        
        public void SaveCustomer(Customer customer)
        {
            _customer = customer;
        }
    }
}