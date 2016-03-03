namespace DemoApp
{
    using Nancy;
    using Nancy.JsonPatch;
    
    public class DemoModule : NancyModule
    {
        private readonly IRepository _repository;

        public DemoModule(IRepository repository)
        {
            _repository = repository;

            Get["/"] = _ => "Running";

            Patch["/customer/{customerId:Guid}"] = _ => 
            {
                Customer customer = _repository.GetCustomer(_.customerId);
                if (this.JsonPatch(customer))
                    _repository.SaveCustomer(customer);

                return HttpStatusCode.NoContent;
            };
        }
    }
}
