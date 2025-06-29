using DAL_Celebrity;
using System.Numerics;

namespace DAL_Celebrity_MSSQL
{
	public interface IRepository : DAL_Celebrity.IRepository<Celebrity, Lifeevent> { }
	public class Celebrity
	{
		public Celebrity() { this.FullName = string.Empty; this.Nationality = string.Empty; }
		public int Id { get; set; }
		public string FullName { get; set; }
		public string Nationality { get; set; }
		public string? ReqPhotoPath { get; set; }
		public virtual bool Update(Celebrity celebrity) => false;
	}

	public class Lifeevent
	{
		public Lifeevent() { this.Description = string.Empty; }
		public int Id { get; set; }
		public int CelebrityId { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public string? ReqPhotoPath { get; set; }
		public virtual bool Update(Lifeevent lifeevent) => false;
	}
}
