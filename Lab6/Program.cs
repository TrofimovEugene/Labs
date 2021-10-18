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

		public static async Task Main(string[] args)
		{
			string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Clinic;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();

				for (var i=0; i < 4; i++){ 
					await CreatePatientAsync(connection); 
				}
			}

		}

		private static async Task CreateTestResult(SqlConnection connection, Guid patientID)
		{
			

			var rand = new Random();
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

			var polis = new Polis
			{
				PolisNumber = randNumber.ToString()
			};

			var command = new SqlCommand(sqlExpressionForPolis, connection);

			// создаем параметр для имени
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

			IGenerationSessionFactory factory = AutoPocoContainer.Configure(x =>
			{
				x.Conventions(c =>
				{
					c.UseDefaultConventions();
				});
				x.AddFromAssemblyContainingType<Patient>();

				x.Include<Patient>()
					.Setup(c => c.FirstName).Use<FirstNameSource>()
					.Setup(c => c.SecondName).Use<LastNameSource>()
					.Setup(c => c.Patronymic).Use<FirstNameSource>()
					.Setup(c => c.DateBirth).Use<DateOfBirthSource>();
			});

			IGenerationSession session = factory.CreateSession();

			var patient = session.Single<Patient>().Get();
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

			await CreateTestResult(connection, (Guid) idParam.Value);
		}
	}
}
