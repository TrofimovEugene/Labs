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

		private static async Task CreatePatientAsync(SqlConnection connection)
		{
			var sqlExpressionForPolis = @"SET @id = NEWID(); INSERT INTO Polis (Id, PolisNumber) VALUES (@id, @PolisNumber); ";
			var sqlExpressionForPatient = @"INSERT INTO Patient (FirstName, SecondName, Patronymic, DateBirth, IdPolis) VALUES (@FirstName, @SecondName, @Patronymic, @DateBirth, @IdPolis);";

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
			Console.WriteLine($"id: {idParam.Value}");

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

			await command.ExecuteNonQueryAsync();
		}
	}
}
