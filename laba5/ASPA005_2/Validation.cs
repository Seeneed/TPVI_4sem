using DAL004;
using System;
using System.Net.WebSockets;

namespace Validation
{
	public class SurnameFilter : IEndpointFilter
	{
		public static IRepository? Repository { get; set; }
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
		{
			var celebrity = context.GetArgument<Celebrity>(0);
			if (celebrity == null) throw new AbsurdeException("POST /Celebrities error, Server Error");
			if (string.IsNullOrEmpty(celebrity.Surname) || celebrity.Surname.Length < 2) throw new ConflictException("POST /Celebrities error, Surname is wrong");
			if (Repository!.doesSurnameExists(celebrity.Surname)) throw new ConflictException("POST /Celebrities error, Surname is wrong");
			return await next(context);
        }
	}

	public class PhotoExistsFilter : IEndpointFilter
	{
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
		{
			var celebrity = context.GetArgument<Celebrity>(0);
			if (celebrity == null) throw new AbsurdeException("POST /Celebrities error, Server Error");
			var basePath = "D:\\Денис\\4 сем\\ТПВИ\\laba5\\DAL004\\Celebrities";
			var fileName = Path.GetFileName(celebrity.PhotoPath);
			var filePath = Path.Combine(basePath, fileName);
			if (!File.Exists(filePath))
			{
				context.HttpContext.Response.Headers.Append("X-Celebrity", $"Not found = {fileName}");
			}
			return await next(context);
		}
	}

	public class UpdateCelebrityExistsFilter : IEndpointFilter
	{
		public static IRepository? Repository { get; set; }
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
		{
			var id = context.GetArgument<int>(0);
			var celebrity = Repository!.GetCelebrityById(id);
			if (celebrity == null) throw new UpdatedException($"Celebrity Id = {id} not found");
			return await next(context);
		}
	}

	public class DeleteCelebrityExistsFilter : IEndpointFilter
	{
		public static IRepository? Repository { get; set; }

		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
		{
			var id = context.GetArgument<int>(0);
			var celebrity = Repository!.GetCelebrityById(id);
			if (celebrity == null) throw new DelByIdException($"Celebrity with Id = {id} not found");
			return await next(context);
		}
	}
}
