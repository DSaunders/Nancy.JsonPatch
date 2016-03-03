namespace Nancy.JsonPatch.Tests.Fakes
{
    using System.Collections.Generic;

    public class ExampleTargetChild
    {
        public string ChildName { get; set; }
        public string ChildCantSetMe { get { return "I have no setter"; } }
    }

    public class ExampleTarget
    {
        public string Name { get; set; }
        public string CantSetMe { get { return "I have no setter"; } }
        public int ValueType { get; set; }
        public ExampleTargetChild Child { get; set; }
        public List<string> StringList { get; set; }
        public List<int> IntList { get; set; }
        public List<ExampleTargetChild> ChildList { get; set; }
    }
}