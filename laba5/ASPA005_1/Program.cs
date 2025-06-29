using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using DAL004;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

using (IRepository repository = Repository.Create("Celebrities"))
{
	app.UseExceptionHandler("/Celebrities/Error");

	app.MapGet("/Celebrities", () => repository.getAllCelebrities());

	app.MapGet("/Celebrities/{id:int}", (int id) =>
	{
		Celebrity? celebrity = repository.GetCelebrityById(id);
		if (celebrity == null) throw new FoundByIdException($"Celebrity Id = {id}");
		return celebrity;
	});

	app.MapPost("/Celebrities", (Celebrity celebrity) =>
	{
		int? id = repository.addCelebrity(celebrity);
		if (id == null) throw new AddCelebrityException("/Celebrities error, id == null");
		if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
		return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
	})
	.AddEndpointFilter(async (context, next) =>
	{
		Celebrity? celebrity = context.GetArgument<Celebrity>(0);
		if (celebrity == null) throw new AbsurdeException("POST /Celebrities error, Server Error");
		if (celebrity.Surname == null || celebrity.Surname.Length < 2) throw new ConflictException("POST /Celebrities error, Surname is wrong");
		return await next(context);
	})
	.AddEndpointFilter(async (context, next) =>
	{
		Celebrity? celebrity = context.GetArgument<Celebrity>(0);
		if (celebrity == null) throw new AbsurdeException("POST /Celebrities error, Server Error");
		if (repository.doesSurnameExists(celebrity.Surname)) throw new ConflictException("POST / Celebrities error, Surname is doubled");
		return await next(context);
	})
	.AddEndpointFilter(async (context, next) =>
	{
		Celebrity? celebrity = context.GetArgument<Celebrity>(0);
		if (celebrity == null) throw new AbsurdeException("POST /Celebrities error, Server Error");
		var basePath = "D:\\Äåíèñ\\4 ñåì\\ÒÏÂÈ\\laba5\\DAL004\\Celebrities";
		var fileName = Path.GetFileName(celebrity.PhotoPath);
		var filePath = Path.Combine(basePath, fileName);
		if(!File.Exists(filePath))
		{
			context.HttpContext.Response.Headers.Append("X-Celebrity", $"Not found = {fileName}");
		}
		return await next(context);
	});

	app.MapDelete("/Celebrities/{id:int}", (int id) =>
	{
		var celebrity = repository.GetCelebrityById(id);
		if (celebrity == null) throw new DelByIdException($"Celebrity Id = {id}");
		repository.delCelebrityById(id);
		if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
		return Results.Ok($"Celebrity with Id = {id} deleted");
	});

	app.MapPut("/Celebrities/{id:int}", (int id, Celebrity celebrity) =>
	{
		var celebrity1 = repository.GetCelebrityById(id);
		int? updId = repository.updCelebrityById(id, celebrity);
		if (updId == null) throw new UpdatedException($"Failed to update Celebrity Id = {id}");
		if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
		return Results.Ok(new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath));
	});

	app.MapFallback((HttpContext ctx) => Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

	app.Map("/Celebrities/Error", (HttpContext ctx) =>
	{
		Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
		IResult rc = Results.Problem(detail: ex?.Message ?? "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);
		if (ex != null)
		{
			if (ex is ConflictException) rc = Results.Conflict(ex.Message);
			if (ex is AbsurdeException) rc = Results.BadRequest(ex.Message);
			if (ex is UpdatedException) rc = Results.NotFound(ex.Message);
			if (ex is DelByIdException) rc = Results.NotFound(ex.Message);
			if (ex is FileNotFoundException fileNotFound) rc = Results.Problem(detail: $"Could not find file '{fileNotFound.FileName}'", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);
			if (ex is FoundByIdException) rc = Results.NotFound(ex.Message);
			if (ex is BadHttpRequestException) rc = Results.BadRequest(ex.Message);
			if (ex is SaveException) rc = Results.Problem(title: "ASPA004/SaveChanges", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
			if (ex is AddCelebrityException) rc = Results.Problem(title: "ASPA004/addCelebrity", detail: ex.Message, instance: app.Environment.EnvironmentName, statusCode: 500);
		}
		return rc;
	});

	app.Run();
}
public class AbsurdeException : Exception { public  AbsurdeException(string message) : base($"Value: {message}") { } };
public class ConflictException : Exception { public ConflictException(string message) : base($"Value: {message}") { } };
public class UpdatedException : Exception { public UpdatedException(string message) : base($"Update by ID: {message}") { } };
public class DelByIdException : Exception { public DelByIdException(string message) : base($"Del by ID: {message}") { } };
public class FoundByIdException : Exception { public FoundByIdException(string message) : base($"Found by ID: {message}") { } };
public class SaveException : Exception { public SaveException(string message) : base($"SaveChanges error: {message}") { } };
public class AddCelebrityException : Exception { public AddCelebrityException(string message) : base($"AddCelebrityException error: {message}") { } };