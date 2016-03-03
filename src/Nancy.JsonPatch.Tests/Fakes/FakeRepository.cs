namespace Nancy.JsonPatch.Tests.Fakes
{
    public class FakeRepository
    {
        private ExampleTarget _target;

        public ExampleTarget Load()
        {
            return _target;
        }

        public void Save(ExampleTarget target)
        {
            _target = target;
        }
    }
}