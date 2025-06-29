using System;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DAL004
{
	public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);
	public interface IRepository : IDisposable
	{
		string Basepath { get; }
		Celebrity[] getAllCelebrities();
		Celebrity? GetCelebrityById(int id);
		Celebrity[] GetCelebritiesBySurname(string Surname);
		string? getPhotoPathById(int id);
		int? addCelebrity(Celebrity celebrity);
		bool delCelebrityById(int id);
		int? updCelebrityById(int id, Celebrity celebrity);
		int SaveChanges();
		bool doesSurnameExists(string Surname);
	}
	public class Repository : IRepository
	{
		public static string JSONFileName { get; } = "Celebrities.json";
		public string Basepath { get; }
		private List<Celebrity> AllCelebrity { get; set; }
		private Repository(string directoryPath)
		{
			Basepath = "D:\\Денис\\4 сем\\ТПВИ\\laba5\\DAL004\\" + directoryPath + "\\Сelebrities.json";
			if (File.Exists(Basepath))
			{
				var jsonData = File.ReadAllText(Basepath);
				AllCelebrity = JsonConvert.DeserializeObject<List<Celebrity>>(jsonData) ?? new List<Celebrity>();
			}
			else
			{
				AllCelebrity = new List<Celebrity>();
			}
		}
		public static Repository Create(string directoryName)
		{
			return new Repository(directoryName);
		}
		public Celebrity[] getAllCelebrities()
		{
			return AllCelebrity.ToArray();
		}
		public Celebrity? GetCelebrityById(int id)
		{
			return AllCelebrity.FirstOrDefault(x => x.Id == id);
		}
		public Celebrity[] GetCelebritiesBySurname(string Surname)
		{
			return AllCelebrity.Where(x => x.Surname.Equals(Surname, StringComparison.OrdinalIgnoreCase)).ToArray();
		}
		public string? getPhotoPathById(int id)
		{
			return AllCelebrity.FirstOrDefault(y => y.Id == id)?.PhotoPath;
		}
		public int? addCelebrity(Celebrity celebrity)
		{
			if (celebrity.Id == 0 || AllCelebrity.Any(c => c.Id == celebrity.Id))
			{
				int newId = AllCelebrity.Count > 0 ? AllCelebrity.Max(c => c.Id) + 1 : 1;
				var newCelebrity = celebrity with { Id = newId };
				AllCelebrity.Add(newCelebrity);
				SaveChanges();
				return newCelebrity.Id;
			}

			AllCelebrity.Add(celebrity);
			SaveChanges();
			return celebrity.Id;
		}
		public bool delCelebrityById(int id)
		{
			var celebrity = AllCelebrity.FirstOrDefault(c => c.Id == id);
			if (celebrity != null)
			{
				AllCelebrity.Remove(celebrity);
				SaveChanges();
				return true;
			}
			return false;
		}
		public int? updCelebrityById(int id, Celebrity celebrity)
		{
			var existsCelebrity = AllCelebrity.FirstOrDefault(celebrity => celebrity.Id == id);
			if (existsCelebrity != null)
			{
				AllCelebrity.Remove(existsCelebrity);
				AllCelebrity.Add(celebrity);
				SaveChanges();
				return celebrity.Id;
			}
			return null;
		}
		public int SaveChanges()
		{
			var jsonData = JsonConvert.SerializeObject(AllCelebrity, Formatting.Indented);
			File.WriteAllText(Basepath, jsonData);
			return AllCelebrity.Count;
		}
		public bool doesSurnameExists(string surname)
		{
			return AllCelebrity.Any(s => string.Equals(s.Surname, surname, StringComparison.OrdinalIgnoreCase));
		}
		public void Dispose() { }
	}
}
