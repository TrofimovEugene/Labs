using System;

namespace Lab3
{
	[Serializable]
	public class Patient
	{
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public DateTime DateBirth { get; set; }
		public string PolisNumber { get; set; }
	}
}
