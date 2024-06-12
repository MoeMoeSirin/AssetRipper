using AssetRipper.Import.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AssetRipper.GUI.Web.Pages;

public static class Commands
{
	private const string RootPath = "/";
	private const string CommandsPath = "/Commands";

	public readonly struct LoadFile : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();
			IFormFile? file =form.Files.FirstOrDefault();
			if (file == null || file.Length < 10240||file.Length> 10485760)
				return null;
			Logger.Info(LogCategory.Import, $"Receive File: {file.FileName}  Size: {file.Length}");
			string path = Path.Combine("uploads", Path.GetRandomFileName());
			Directory.CreateDirectory("uploads");
			using (FileStream fs = new FileStream(path, FileMode.Create))
			{
				await file.CopyToAsync(fs);
			}
			GameFileLoader.LoadAndProcess([path]);
			File.Delete(path);
			return null;
		}
	}

	public readonly struct LoadFolder : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else if (Dialogs.Supported)
			{
				Dialogs.OpenFolder.GetUserInput(out path);
			}
			else
			{
				return CommandsPath;
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.LoadAndProcess([path]);
			}
			return null;
		}
	}

	public readonly struct Export : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else
			{
				return CommandsPath;
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.Export(path);
			}
			return null;
		}
	}

	public readonly struct Reset : ICommand
	{
		static Task<string?> ICommand.Execute(HttpRequest request)
		{
			GameFileLoader.Reset();
			return Task.FromResult<string?>(null);
		}
	}

	public static async Task HandleCommand<T>(HttpContext context) where T : ICommand
	{
		string? redirectionTarget = await T.Execute(context.Request);
		context.Response.Redirect(redirectionTarget ?? RootPath);
	}
}
