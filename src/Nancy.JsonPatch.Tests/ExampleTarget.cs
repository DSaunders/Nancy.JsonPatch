namespace Nancy.JsonPatch.Tests
{
    using System.Collections.Generic;

    internal class ExampleTargetChild
    {
        public string ChildName { get; set; }
        public string ChildCantSetMe { get { return "I have no setter"; } }
    }

    internal class ExampleTarget
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