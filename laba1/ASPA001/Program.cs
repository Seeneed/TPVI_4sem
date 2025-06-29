using Microsoft.AspNetCore.HttpLogging; //подключаем пространство имен для логирования HTTP-запросов
internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args); //создаем экземпляр WebApplicationBuilder с переданными аргументами
		builder.Services.AddHttpLogging(options => //добавляем службу логирования HTTP-запросов
		{
			options.LoggingFields = HttpLoggingFields.All; //указываем, что нужно логировать все поля HTTP-запросов
		});

		// добавляем службы контейнер
		builder.Services.AddRazorPages();

		var app = builder.Build(); // создаем экземпляр приложения

		app.UseHttpLogging(); // включаем логирование HTTP-запросов

		app.MapGet("/", () => "Мое первое aspa"); // выводим приветственное сообщение на экран

		//настраиваем конвейер обработки HTTP-запросов
		if (!app.Environment.IsDevelopment()) //если среда выполнения не является средой разработки,
		{
			app.UseExceptionHandler("/Error"); //используем обработчик исключений для перенаправления на страницу ошибок
			app.UseHsts(); //включаем HTTP Strict Transport Security (HSTS) для повышения безопасности
		}

		app.UseHttpsRedirection(); //перенаправляем HTTP-запросы на HTTPS
		app.UseStaticFiles(); //включаем поддержку статических файлов

		app.UseRouting(); //включаем маршрутизацию

		app.UseAuthorization(); //включаем авторизацию

		app.MapRazorPages(); //настраиваем маршрутизацию для Razor Pages

		app.Run(); //запускаем приложение
	}
}