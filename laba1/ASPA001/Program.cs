using Microsoft.AspNetCore.HttpLogging; //���������� ������������ ���� ��� ����������� HTTP-��������
internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args); //������� ��������� WebApplicationBuilder � ����������� �����������
		builder.Services.AddHttpLogging(options => //��������� ������ ����������� HTTP-��������
		{
			options.LoggingFields = HttpLoggingFields.All; //���������, ��� ����� ���������� ��� ���� HTTP-��������
		});

		// ��������� ������ ���������
		builder.Services.AddRazorPages();

		var app = builder.Build(); // ������� ��������� ����������

		app.UseHttpLogging(); // �������� ����������� HTTP-��������

		app.MapGet("/", () => "��� ������ aspa"); // ������� �������������� ��������� �� �����

		//����������� �������� ��������� HTTP-��������
		if (!app.Environment.IsDevelopment()) //���� ����� ���������� �� �������� ������ ����������,
		{
			app.UseExceptionHandler("/Error"); //���������� ���������� ���������� ��� ��������������� �� �������� ������
			app.UseHsts(); //�������� HTTP Strict Transport Security (HSTS) ��� ��������� ������������
		}

		app.UseHttpsRedirection(); //�������������� HTTP-������� �� HTTPS
		app.UseStaticFiles(); //�������� ��������� ����������� ������

		app.UseRouting(); //�������� �������������

		app.UseAuthorization(); //�������� �����������

		app.MapRazorPages(); //����������� ������������� ��� Razor Pages

		app.Run(); //��������� ����������
	}
}