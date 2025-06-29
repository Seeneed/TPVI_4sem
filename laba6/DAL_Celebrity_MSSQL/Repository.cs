using DAL_Celebrity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Celebrity_MSSQL
{
	public class Repository : IRepository
	{
		Context context;
		public Repository() { this.context = new Context(); }
		public Repository(string connectionstring) { this.context = new Context(connectionstring); }
		public static IRepository Create() { return new Repository(); }
		public static IRepository Create(string connectionstring) { return new Repository(connectionstring); }
		public List<Celebrity> GetAllCelebrities() { return this.context.Celebrities.ToList<Celebrity>(); }
		public Celebrity? GetCelebrityById(int id) { return this.context.Celebrities.FirstOrDefault(c => c.Id == id); }

		public bool AddCelebrity(Celebrity celebrity)
		{
			if (celebrity == null) return false;
			this.context.Celebrities.Add(celebrity);
			this.context.SaveChanges();
			return true;
		}

		public bool DelCelebrity(int id)
		{
			var celebrity = this.context.Celebrities.FirstOrDefault(c => c.Id == id);
			if (celebrity != null)
			{
				this.context.Celebrities.Remove(celebrity);
				this.context.SaveChanges();
				return true;
			}
			return false;
		}

		public bool UpdCelebrity(int id, Celebrity celebrity)
		{
			if (celebrity == null) return false;
			var existingCelebrity = this.context.Celebrities.FirstOrDefault(c => c.Id == id);
			if (existingCelebrity != null)
			{
				existingCelebrity.FullName = celebrity.FullName;
				existingCelebrity.Nationality = celebrity.Nationality;
				existingCelebrity.ReqPhotoPath = celebrity.ReqPhotoPath;
				this.context.SaveChanges();
				return true;
			}
			return false;
		}

		public List<Lifeevent> GetAllLifeevents() { return this.context.Lifeevents.ToList<Lifeevent>(); }
		public Lifeevent? GetLifeeventById(int id) { return this.context.Lifeevents.FirstOrDefault(le => le.Id == id); }

		public bool AddLifeevent(Lifeevent lifeevent)
		{
			if (lifeevent == null) return false;
			this.context.Lifeevents.Add(lifeevent);
			this.context.SaveChanges();
			return true;
		}

		public bool DelLifeevent(int id)
		{
			var lifeevent = this.context.Lifeevents.FirstOrDefault(c => c.Id == id);
			if (lifeevent != null)
			{
				this.context.Lifeevents.Remove(lifeevent);
				this.context.SaveChanges();
				return true;
			}
			return false;
		}

		public bool UpdLifeevent(int id, Lifeevent lifeevent)
		{
			if (lifeevent == null) return false;
			var existingLifeevent = this.context.Lifeevents.FirstOrDefault(c => c.Id == id);
			if (existingLifeevent != null)
			{
				existingLifeevent.Date = lifeevent.Date;
				existingLifeevent.Description = lifeevent.Description;
				existingLifeevent.ReqPhotoPath = lifeevent.ReqPhotoPath;
				existingLifeevent.CelebrityId = lifeevent.CelebrityId;
				this.context.SaveChanges();
				return true;
			}
			return false;
		}

		public List<Lifeevent> GetLifeeventsByCelebrityId(int CelebrityId)
		{
			return this.context.Lifeevents.Where(p => p.CelebrityId == CelebrityId).ToList();
		}

		public Celebrity? GetCelebrityByLifeeventId(int lifeeventId)
		{
			var lifeevent = this.context.Lifeevents
							   .AsNoTracking()
							   .FirstOrDefault(le => le.Id == lifeeventId);

			if (lifeevent != null)
			{
				int targetCelebrityId = lifeevent.CelebrityId;

				var celebrity = this.context.Celebrities
									.AsNoTracking()
									.FirstOrDefault(c => c.Id == targetCelebrityId);
				return celebrity;
			}
			else
			{
				return null;
			}
		}

		public int GetCelebrityIdByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return 0;
			}

			string searchTerm = name.ToLower();

			var celebrity = this.context.Celebrities
								.AsNoTracking()
								.FirstOrDefault(c => c.FullName.ToLower().Contains(searchTerm));
			return celebrity?.Id ?? 0;
		}

		public void Dispose() { }
	}
}