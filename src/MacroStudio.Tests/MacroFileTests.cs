using MacroStudio.Execution;
using System.IO.Compression;

namespace MacroStudio.Tests;

public class MacroFileTests
{
	[Fact]
	public async Task ReadAsync_WithValidMacro_ReturnsCodeAndExecutions()
	{
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

			var (code, executions) = await MacroFile.ReadAsync(path);
			Assert.Equal("Console.WriteLine(\"hi\");", code);
			Assert.Equal(3, executions);
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
		var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".macro");
		try
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
			{
				await WriteEntryAsync(archive, "version", "1");
				await WriteEntryAsync(archive, "code", "x");
			}

			var (_, executions) = await MacroFile.ReadAsync(path);
			Assert.Equal(1, executions);
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
