using System;
using DAL004;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Diagnostics;

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
	});

	app.MapDelete("/Celebrities/{id:int}", (int id) =>
	{
		var celebrity = repository.GetCelebrityById(id);
		if (celebrity == null) throw new DelByIdException($"Celebrity Id = {id}");
		repository.delCelebrityById(id);
		return Results.Ok($"Celebrity with Id = {id} deleted");
	});

	app.MapFallback((HttpContext ctx) => Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

	app.Map("/Celebrities/Error", (HttpContext ctx) =>
	{
		Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
		IResult rc = Results.Problem(detail: ex?.Message ?? "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);
		if (ex != null)
		{
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
public class DelByIdException : Exception { public  DelByIdException(string message) : base($"Del by ID: {message}") { } };
public class FoundByIdException : Exception { public FoundByIdException(string message) : base($"Found by ID: {message}") { } };
public class SaveException : Exception { public SaveException(string message) : base($"SaveChanges error: {message}") { } };
public class AddCelebrityException : Exception { public AddCelebrityException(string message) : base($"AddCelebrityException error: {message}") { } };