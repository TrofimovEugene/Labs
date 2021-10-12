using System;

namespace Lab3
{
	[Serializable]
	public class Test
	{
		public string PolisNumber { get; set; }
		public DateTime DateTest { get; set; }
		public TypeTest TypeTest { get; set; }
		public Accuracy Accuracy { get; set; }
		public bool Result { get; set; }
	}
}
