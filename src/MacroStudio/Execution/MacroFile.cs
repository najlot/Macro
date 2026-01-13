using System.IO;
using System.IO.Compression;

namespace MacroStudio.Execution;

public static class MacroFile
{
	public static async Task<(string Code, int Executions)> ReadAsync(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("File path is required.", nameof(filePath));
		}

		using var zipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

		var versionEntry = archive.GetEntry("version") ?? throw new InvalidDataException("Version not found");
		using (var versionStream = versionEntry.Open())
		using (var versionReader = new StreamReader(versionStream))
		{
			var version = await versionReader.ReadToEndAsync();
			if (!version.StartsWith("1", StringComparison.Ordinal))
			{
				throw new InvalidDataException("Invalid version");
			}
		}

		var codeEntry = archive.GetEntry("code") ?? throw new InvalidDataException("Code not found");
		string code;
		using (var codeStream = codeEntry.Open())
		using (var codeReader = new StreamReader(codeStream))
		{
			code = await codeReader.ReadToEndAsync();
		}

		var executionsEntry = archive.GetEntry("executions");
		if (executionsEntry is null)
		{
			return (code, 1);
		}

		using var executionsStream = executionsEntry.Open();
		using var executionsReader = new StreamReader(executionsStream);
		var executionsText = await executionsReader.ReadToEndAsync();
		return int.TryParse(executionsText, out var executions) ? (code, executions) : (code, 1);
	}
}
