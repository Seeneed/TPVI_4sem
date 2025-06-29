using System;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace DAL003
{
	public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);
	public interface IRepository : IDisposable
	{
		string Basepath { get; }
		Celebrity[] getAllCelebrities();
		Celebrity? GetCelebrityById(int id);
		Celebrity[] GetCelebritiesBySurname(string Surname);
		string? getPhotoPathById(int id);
	}
	public class Repository : IRepository
	{
		public static string JSONFileName { get; } = "Celebrities.json";
		public string Basepath { get; }
		private List<Celebrity> AllCelebrity { get; set; }
		private Repository(string directoryPath)
		{
			Basepath = "D:\\Денис\\4 сем\\ТПВИ\\laba3\\DAL003\\" + directoryPath + "\\Сelebrities.json";
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
		public void Dispose() { }
	}
}
