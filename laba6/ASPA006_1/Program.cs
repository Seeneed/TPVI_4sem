using DAL_Celebrity;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
public class Program
{
	private static void Main(string[] args)
	{
		string CS = @"Data source = DESKTOP-SNMNO70; Initial Catalog = LES01;" +
								 @"TrustServerCertificate=True; Integrated Security=True;";
		Init init = new Init(CS);
		Init.Execute(create: true, delete: true);
		IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("Celebrities.config.json").Build();
		

		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddControllersWithViews();
		builder.Services.AddScoped<IRepository, Repository>((IServiceProvider p) =>
		{
			CelebritiesConfig config = p.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
			return new Repository(CS);
		});

		var app = builder.Build();
		app.UseDefaultFiles();
		app.UseStaticFiles();

		var photoFolder = configuration.GetSection("Celebrities").GetSection("PhotosFolder").Value;
		if(!Directory.Exists(photoFolder))
		{
			throw new Exception("Not found this folder");
		}
		app.UseExceptionHandler("/Celebrities/Error");

		var celebrities = app.MapGroup("/api/Celebrities");
		celebrities.MapGet("/", (IRepository repo) => repo.GetAllCelebrities());
		celebrities.MapGet("/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			Celebrity? celebrity = repo.GetCelebrityById(id);
			if (celebrity == null) { throw new FoundByIdException($"Celebrity ID = {id}"); }
			return celebrity;
		});
		celebrities.MapGet("/Lifeevents/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			Celebrity? celebrity = repo.GetCelebrityById(id);
			if (celebrity == null) { throw new FoundByIdException($"Celebrity ID = {id}"); }
			return celebrity;
		});
		celebrities.MapDelete("/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			if (repo.DelCelebrity(id)) { return $"Celebrity with id: {id} deleted"; }
			else { throw new DelByIdException($"DELETE /Celebrities error, Id = {id}"); }
		});
		celebrities.MapPost("/", (IRepository repo, Celebrity celebrity) =>
		{
			if (!repo.AddCelebrity(celebrity)) { throw new SaveException("/Celebrities error, SaveChanges() <= 0"); }
			else celebrity.Id = repo.GetCelebrityIdByName(celebrity.FullName);
			return celebrity;
		});
		celebrities.MapPut("/{id:int:min(1)}", (IRepository repo, int id, Celebrity celebrity) =>
		{
			if (!repo.UpdCelebrity(id, celebrity)) { throw new SaveException("/Celebrities error, SaveChanges() <= 0"); }
			else celebrity.Id = repo.GetCelebrityIdByName(celebrity.FullName);
			return celebrity;		
		});
		celebrities.MapGet("/photo/{fname}", async (IRepository repo, string fname) =>
		{
			var photoPath = Path.Combine(photoFolder, fname);
			if (!File.Exists(photoPath)) { throw new FileNotFoundException($"File {fname} was not found"); }
			else
			{
				try
				{
					var bytes = await File.ReadAllBytesAsync(photoPath);
					string contentType = GetContentType(Path.GetExtension(photoPath));
					return Results.File(bytes, contentType);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
					return Results.Problem($"Ошибка при чтении файла: {ex.Message}");
				}
			}
		});

		var lifeevents = app.MapGroup("/api/Lifeevents");
		lifeevents.MapGet("/", (IRepository repo) => repo.GetAllLifeevents());
		lifeevents.MapGet("/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			Lifeevent? lifeevent = repo.GetLifeeventById(id);
			if (lifeevent == null) { throw new FoundByIdException($"Lifeevent ID = {id}"); }
			return lifeevent;
		});
		lifeevents.MapGet("/Celebrities/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			Lifeevent? lifeevent = repo.GetLifeeventById(id);
			if (lifeevent == null) { throw new FoundByIdException($"Lifeevent for Celebrity ID = {id}"); }
			return lifeevent;
		});
		lifeevents.MapDelete("/{id:int:min(1)}", (IRepository repo, int id) =>
		{
			if (repo.DelLifeevent(id)) { return $"Lifeevent with id: {id} deleted"; }
			else { throw new DelByIdException($"DELETE /Lifeevents error, Id = {id}"); }
		});
		lifeevents.MapPost("/", (IRepository repo, Lifeevent lifeevent) =>
		{
			if (!repo.AddLifeevent(lifeevent)) { throw new SaveException("/Celebrities error, SaveChanges() <= 0"); }
			else lifeevent.Id = repo.GetLifeeventsByCelebrityId(lifeevent.CelebrityId).Last().Id;
			return lifeevent;
		});
		lifeevents.MapPut("/{id:int:min(1)}", (IRepository repo, int id, Lifeevent lifeevent) =>
		{
			if (!repo.UpdLifeevent(id, lifeevent)) { throw new SaveException("/Celebrities error, SaveChanges() <= 0"); }
			else lifeevent.Id = repo.GetLifeeventsByCelebrityId(lifeevent.CelebrityId).Last().Id;
			return lifeevent;
		});

		app.Map("/Celebrities/Error", (HttpContext ctx) =>
		{
			Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
			IResult rc = Results.Problem(detail: "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);
			if (ex != null)
			{
				if (ex is FileNotFoundException) rc = Results.NotFound(ex.Message); 
				if (ex is ConflictException) rc = Results.Conflict(ex.Message);
				if (ex is AbsurdeException) rc = Results.BadRequest(ex.Message);
				if (ex is UpdatedException) rc = Results.NotFound(ex.Message);
				if (ex is DelByIdException) rc = Results.NotFound(ex.Message);
				if (ex is FoundByIdException) rc = Results.NotFound(ex.Message);
				if (ex is BadHttpRequestException) rc = Results.BadRequest(ex.Message);
				if (ex is SaveException) rc = Results.Problem(title: "ASPA004/SaveChanges", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
				if (ex is AddCelebrityException) rc = Results.Problem(title: "ASPA004/addCelebrity", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
			}
			return rc;
		});

		app.Run();
	}
	public class CelebritiesConfig
	{
		public string PhotosFolder { get; set; }
		public string ConnectionString { get; set; }
	}
	static string GetContentType(string fileName)
	{
		var extension = Path.GetExtension(fileName).ToLowerInvariant();
		return extension switch
		{
			".jpg" or ".jpeg" => "image/jpeg",
			".png" => "image/png",
			".gif" => "image/gif",
			".bmp" => "image/bmp",
			".webp" => "image/webp",
			_ => "application/octet-stream"
		};
	}
	public class AbsurdeException : Exception { public AbsurdeException(string message) : base($"Value: {message}") { } };
	public class ConflictException : Exception { public ConflictException(string message) : base($"Value: {message}") { } };
	public class UpdatedException : Exception { public UpdatedException(string message) : base($"Update by ID: {message}") { } };
	public class DelByIdException : Exception { public DelByIdException(string message) : base($"Del by ID: {message}") { } };
	public class FoundByIdException : Exception { public FoundByIdException(string message) : base($"Found by ID: {message}") { } };
	public class SaveException : Exception { public SaveException(string message) : base($"SaveChanges error: {message}") { } };
	public class AddCelebrityException : Exception { public AddCelebrityException(string message) : base($"AddCelebrityException error: {message}") { } };
}