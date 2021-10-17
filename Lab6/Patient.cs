using System;

namespace Lab6
{
	public class Patient
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public DateTime DateBirth { get; set; }
		public Guid IdPolis { get; set; }
	}
}
