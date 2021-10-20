using AutoPoco;
using AutoPoco.DataSources;
using AutoPoco.Engine;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Lab6
{
	public class Program
	{
		static string sqlExpressionForPolis = @"SET @id = NEWID(); INSERT INTO Polis (Id, PolisNumber) VALUES (@id, @PolisNumber); ";
		static string sqlExpressionForPatient = @"SET @id = NEWID(); INSERT INTO Patient (Id, FirstName, SecondName, Patronymic, DateBirth, IdPolis) VALUES (@Id, @FirstName, @SecondName, @Patronymic, @DateBirth, @IdPolis);";
		static string sqlExpressionForClinic = @"SET @id = NEWID(); INSERT INTO Place (Id, NamePlace, Address, Email, NumberPhone) VALUES (@id, @NamePlace, @Address, @Email, @NumberPhone);";
		static string sqlExpressionForResult = @"SET @id = NEWID(); INSERT INTO Result (Id, ResultTest, Accuracy) VALUES (@id, @ResultTest, @Accuracy);";
		static string sqlExpressionForTest = @"INSERT INTO Test (DateTest, IdPatient, IdPlace, IdResult, IdTypeTest) VALUES (@DateTest, @IdPatient, @IdPlace, @IdResult, @IdTypeTest);";
		static string sqlExpressionForSelectPatient = @"SELECT p.SecondName, p.FirstName, p.Patronymic, p.DateBirth, Pol.PolisNumber, test.DateTest,
											place.NamePlace, place.Address, place.NumberPhone, place.Email, ResultTest, res.Accuracy, type.Name FROM Patient p
											JOIN Polis Pol ON p.IdPolis = Pol.Id
											JOIN Test test ON test.IdPatient = p.Id
											JOIN Place place ON test.IdPlace = place.Id
											JOIN Result res ON test.IdResult = res.Id
											JOIN TypeTest type ON test.IdTypeTest = type.Id
											WHERE @secondName = p.SecondName AND @firstName = p.FirstName 
												AND @patronymic = p.Patronymic AND @polis = Pol.PolisNumber;";

		public static async Task Main(string[] args)
		{
			// строка подключения
			string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Clinic;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				// подключение к БД
				await connection.OpenAsync();
				
				// генерация данных для 4 пациентов и 2 тестов для каждого пациента
				for (var i=0; i < 4; i++){ 
					await CreatePatientAsync(connection); 
				}
				// получение данных по пациенту
				await GetPatientAsync(connection, "Eugene", "Bond", "Hamish", "3805355829247019");
			}


		}

		// метод генерации результатов теста и места его проведения
		private static async Task CreateTestResult(SqlConnection connection, Guid patientID)
		{
			var rand = new Random();
			// лаборатория (место)
			var place = new Place
			{
				NamePlace = $"ОГАУЗ ИГБ №{rand.Next(1, 15)}",
				Address = $"ул. Смольная, д. {rand.Next(1, 100)}",
				Email = "ogauzIGB@mail.com",
				NumberPhone =  $"{rand.Next(100000, 999999)}"
			};

			var command = new SqlCommand(sqlExpressionForClinic, connection);
			var namePlace = new SqlParameter($"@{nameof(place.NamePlace)}", place.NamePlace);
			command.Parameters.Add(namePlace);
			var address = new SqlParameter($"@{nameof(place.Address)}", place.Address);
			command.Parameters.Add(address);
			var email = new SqlParameter($"@{nameof(place.Email)}", place.Email);
			command.Parameters.Add(email);
			var numberPhone = new SqlParameter($"@{nameof(place.NumberPhone)}", place.NumberPhone);
			command.Parameters.Add(numberPhone);
			var idPlace = new SqlParameter
			{
				ParameterName = "@id",
				SqlDbType = SqlDbType.UniqueIdentifier,
				Direction = ParameterDirection.Output // параметр выходной
			};
			command.Parameters.Add(idPlace);

			await command.ExecuteNonQueryAsync();
			
			// результат
			var result = new Result
			{
				ResultTest = rand.Next(0,100) >= 50 ? false : true,
				Accuracy = rand.Next(0, 100) >= 50 ? "количественный" : "качественный"
			};

			command = new SqlCommand(sqlExpressionForResult, connection);
			var resultTest = new SqlParameter($"@{nameof(result.ResultTest)}", result.ResultTest);
			command.Parameters.Add(resultTest);
			var accuracy = new SqlParameter($"@{nameof(result.Accuracy)}", result.Accuracy);
			command.Parameters.Add(accuracy);
			var idResult = new SqlParameter
			{
				ParameterName = "@id",
				SqlDbType = SqlDbType.UniqueIdentifier,
				Direction = ParameterDirection.Output // параметр выходной
			};
			command.Parameters.Add(idResult);

			await command.ExecuteNonQueryAsync();

			// собираем результат и лабораторию, тип теста был внесен в базу
			var test = new Test
			{
				DateTest = DateTime.Now,
				IdPatient = patientID,
				IdPlace = (Guid)idPlace.Value,
				IdResult = (Guid)idResult.Value,
				IdTypeTest = rand.Next(0, 100) >= 50 ? new Guid("c1711fc1-52c1-46ac-870b-6d9305d5ac49") : new Guid("f4952573-e619-4a63-b9be-8a8a2e90a8b3")
			};

			command = new SqlCommand(sqlExpressionForTest, connection);
			var dateTest = new SqlParameter($"@{nameof(test.DateTest)}", test.DateTest);
			command.Parameters.Add(dateTest);
			var idPatient = new SqlParameter($"@{nameof(test.IdPatient)}", test.IdPatient);
			command.Parameters.Add(idPatient);
			var IdPlace = new SqlParameter($"@{nameof(test.IdPlace)}", test.IdPlace);
			command.Parameters.Add(IdPlace);
			var IdResult = new SqlParameter($"@{nameof(test.IdResult)}", test.IdResult);
			command.Parameters.Add(IdResult);
			var IdTypeTest = new SqlParameter($"@{nameof(test.IdTypeTest)}", test.IdTypeTest);
			command.Parameters.Add(IdTypeTest);

			await command.ExecuteNonQueryAsync();
		}

		private static async Task CreatePatientAsync(SqlConnection connection)
		{
			var random = new Random();
			var randNumber = new StringBuilder("3805");
			randNumber.Append(random.Next(1000, 9999));
			randNumber.Append(random.Next(1000, 9999));
			randNumber.Append(random.Next(1000, 9999));
			// создаем номер полиса
			var polis = new Polis
			{
				PolisNumber = randNumber.ToString()
			};

			var command = new SqlCommand(sqlExpressionForPolis, connection);
			// создаем параметр для полиса
			var numberParam = new SqlParameter($"@{nameof(polis.PolisNumber)}", polis.PolisNumber);
			// добавляем параметр к команде
			command.Parameters.Add(numberParam);
			var idParam = new SqlParameter
			{
				ParameterName = "@id",
				SqlDbType = SqlDbType.UniqueIdentifier,
				Direction = ParameterDirection.Output // параметр выходной
			};
			command.Parameters.Add(idParam);

			await command.ExecuteNonQueryAsync();
			// создаем пациента
			var patient = new Patient
			{
				FirstName = random.Next(0, 100) >= 50 ? "Eugene" : "James",
				SecondName = random.Next(0, 100) >= 50 ? "Trofimov" : "Bond",
				Patronymic = random.Next(0, 100) >= 50 ? "Hamish" : "Sherlock",
				DateBirth = RandomDayFunc(),
			};
			patient.IdPolis = (Guid)idParam.Value;

			command = new SqlCommand(sqlExpressionForPatient, connection);
			var firstName = new SqlParameter($"@{nameof(patient.FirstName)}", patient.FirstName);
			command.Parameters.Add(firstName);
			var secondName = new SqlParameter($"@{nameof(patient.SecondName)}", patient.SecondName);
			command.Parameters.Add(secondName);
			var patronymic = new SqlParameter($"@{nameof(patient.Patronymic)}", patient.Patronymic);
			command.Parameters.Add(patronymic);
			var dateBirth = new SqlParameter($"@{nameof(patient.DateBirth)}", patient.DateBirth);
			command.Parameters.Add(dateBirth);
			var idPolis = new SqlParameter($"@{nameof(patient.IdPolis)}", patient.IdPolis);
			command.Parameters.Add(idPolis);
			idParam = new SqlParameter
			{
				ParameterName = "@id",
				SqlDbType = SqlDbType.UniqueIdentifier,
				Direction = ParameterDirection.Output // параметр выходной
			};
			command.Parameters.Add(idParam);

			await command.ExecuteNonQueryAsync();
			
			// создаем для пациента результаты тестов
			await CreateTestResult(connection, (Guid) idParam.Value);
			await CreateTestResult(connection, (Guid) idParam.Value);
		}

		private static async Task GetPatientAsync(SqlConnection connection, string firstName, string secondName, string patronymic, string polis)
		{
			var command = new SqlCommand(sqlExpressionForSelectPatient, connection);
			var firstNameP = new SqlParameter("@firstName", firstName);
			command.Parameters.Add(firstNameP);
			var secondNameP = new SqlParameter("@secondName", secondName);
			command.Parameters.Add(secondNameP);
			var patronymicP = new SqlParameter("@patronymic", patronymic);
			command.Parameters.Add(patronymicP);
			var polisP = new SqlParameter("@polis", polis);
			command.Parameters.Add(polisP);

			SqlDataReader reader = await command.ExecuteReaderAsync();

			if (reader.HasRows) // если есть данные
			{
				// выводим названия столбцов
				var columnName1 = reader.GetName(0);
				var columnName2 = reader.GetName(1);
				var columnName3 = reader.GetName(2);
				var columnName4 = reader.GetName(3);
				var columnName5 = reader.GetName(4);
				var columnName6 = reader.GetName(5);
				var columnName7 = reader.GetName(6);
				var columnName8 = reader.GetName(7);
				var columnName9 = reader.GetName(8);
				var columnName10 = reader.GetName(9);
				var columnName11 = reader.GetName(10);
				var columnName12 = reader.GetName(11);

				Console.WriteLine($"{columnName1}\t{columnName2}\t{columnName3}\t{columnName4}\t{columnName5}\t{columnName6}\t{columnName7}\t{columnName8}\t{columnName9}\t{columnName10}\t{columnName11}\t{columnName12}");

				while (await reader.ReadAsync()) // построчно считываем данные
				{
					var secondNam = reader.GetValue(0);
					var firstNam = reader.GetValue(1);
					var patronym = reader.GetValue(2);
					var dateBirth = reader.GetValue(3);
					var polisNum = reader.GetValue(3);
					var dateTest = reader.GetValue(4);
					var namePlace = reader.GetValue(5);
					var address = reader.GetValue(6);
					var	numberPhone = reader.GetValue(7);
					var	email = reader.GetValue(8);
					var	result = reader.GetValue(9);
					var	accuracy = reader.GetValue(10);
					var	name = reader.GetValue(11);

					Console.WriteLine($"{secondNam} \t{firstNam} \t{patronym} \t{dateBirth} \t{polisNum} \t{dateTest} \t{namePlace} \t{address} \t{numberPhone} \t{email} \t{result} \t{accuracy} \t{name}");
				}
			}

			await reader.CloseAsync();
		}

		private static DateTime RandomDayFunc()
		{
			DateTime start = new DateTime(1995, 1, 1);
			Random gen = new Random();
			int range = ((TimeSpan)(DateTime.Today - start)).Days;
			return start.AddDays(gen.Next(range));
		}
	}
}
