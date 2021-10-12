using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Lab3
{
	public class Program
	{
		[Obsolete]
		public static async Task Main(string[] args)
		{
			CheckPatient();

			// сохранение данных
			using (FileStream fs = new FileStream("test.json", FileMode.OpenOrCreate))
			{
				var test = new Test
				{
					PolisNumber = "380004578526572452",
					DateTest = DateTime.Now,
					TypeTest = TypeTest.IgG,
					Accuracy = Accuracy.Quantitative,
					Result = false
				};
				await JsonSerializer.SerializeAsync<Test>(fs, test);
				Console.WriteLine("Data has been saved to file");
				fs.Close();
			}

			// чтение данных
			using (FileStream fs = new FileStream("test1.json", FileMode.OpenOrCreate))
			{
				var schema = JsonSchema.Parse(
				@"{
					'type': 'object',
					'properties': {
						'PolisNumber': {'type':'string'},
						'DateTest': {'type': 'string'},
						'TypeTest': {'type': 'integer'},
						'Accuracy': {'type': 'integer'},
						'Result': {'type': 'boolean'},
					},
					'additionalProperties': false
					}");

				// преобразуем строку в байты
				byte[] array = new byte[fs.Length];
				// считываем данные
				fs.Read(array, 0, array.Length);
				// декодируем байты в строку
				string textFromFile = System.Text.Encoding.Default.GetString(array);
				var obj = JObject.Parse(textFromFile);
				if (obj.IsValid(schema))
					Console.WriteLine("Схема валидна");
				else
					Console.WriteLine("Схема не валидна");
			}

		}

		private static void CheckPatient()
		{
			var patient = new Patient
			{
				FirstName = "Евгений",
				SecondName = "Трофимов",
				Patronymic = "Сергеевич",
				DateBirth = new DateTime(1998, 2, 8),
				PolisNumber = "380004578526572452"
			};

			XmlSerializer formatter = new XmlSerializer(typeof(Patient));

			// получаем поток, куда будем записывать сериализованный объект
			using var fs = new FileStream("patient.xml", FileMode.OpenOrCreate);
			formatter.Serialize(fs, patient);

			Console.WriteLine("Объект сериализован");
			fs.Close();


			// открываем файл с сериализованным объектом
			var fst = new FileStream("patient.xml", FileMode.Open);
			var reader = new XmlTextReader(fst);
			XmlSerializer serializer = new XmlSerializer(typeof(Patient));
			// проверяем возможность десереализации
			if (serializer.CanDeserialize(reader))
			{
				var o = (Patient)serializer.Deserialize(reader);
				Console.WriteLine($"{o.FirstName} {o.SecondName} - {o.PolisNumber}");
			}
			fst.Close();
		}
	}
}
