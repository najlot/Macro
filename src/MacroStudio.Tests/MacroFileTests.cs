using MacroStudio.App.Services;
using System.IO.Compression;

namespace MacroStudio.Tests;

public class MacroFileTests
{
	[Fact]
	public async Task ReadAsync_WithValidMacro_ReturnsCodeAndExecutions()
	{
		var service = new MacroFileService();
		var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".macro");
		try
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				await WriteEntryAsync(archive, "version", "1");
				await WriteEntryAsync(archive, "code", "Console.WriteLine(\"hi\");");
				await WriteEntryAsync(archive, "executions", "3");
			}

			var document = await service.LoadAsync(path);
			Assert.Equal("Console.WriteLine(\"hi\");", document.Code);
			Assert.Equal(3, document.Executions);
			Assert.Empty(document.Resources);
		}
		finally
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	[Fact]
	public async Task ReadAsync_WithoutExecutions_DefaultsToOne()
	{
		var service = new MacroFileService();
		var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".macro");
		try
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				await WriteEntryAsync(archive, "version", "1");
				await WriteEntryAsync(archive, "code", "x");
			}

			var document = await service.LoadAsync(path);
			Assert.Equal(1, document.Executions);
			Assert.Equal("x", document.Code);
			Assert.Empty(document.Resources);
		}
		finally
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	private static async Task WriteEntryAsync(ZipArchive archive, string name, string content)
	{
		var entry = archive.CreateEntry(name, CompressionLevel.NoCompression);
		await using var s = entry.Open();
		await using var writer = new StreamWriter(s);
		await writer.WriteAsync(content);
	}
}
