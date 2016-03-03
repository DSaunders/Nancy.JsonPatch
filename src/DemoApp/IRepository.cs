namespace DemoApp
{
    using System;

    public interface IRepository
    {
        Customer GetCustomer(Guid id);
        void SaveCustomer(Customer customer);
    }
}