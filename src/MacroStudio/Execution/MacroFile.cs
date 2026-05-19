using MacroStudio.ViewModels;
using System.IO;
using System.IO.Compression;

namespace MacroStudio.Execution;

public static class MacroFile
{
	public static async Task<(string Code, int Executions, ResourceViewModel[] Resources)> ReadAsync(string filePath)
	{
		ResourceViewModel[] resources = [];


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
			if (version.StartsWith('2')) // Version 2 supports resources (Bitmaps)
            {
                archive.Entries
					.Where(e => e.FullName.StartsWith("resources/", StringComparison.OrdinalIgnoreCase))
					.ToList()
					.ForEach(e =>
					{
						using var resourceStream = e.Open();
						using var memoryStream = new MemoryStream();
						resourceStream.CopyTo(memoryStream);
						resources = resources.Append(new ResourceViewModel
						{
							Name = Path.GetFileName(e.FullName),
							Value = memoryStream.ToArray()
						}).ToArray();
					});
            }
			else if (!version.StartsWith('1'))
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
			return (code, 1, resources);
		}

		using var executionsStream = executionsEntry.Open();
		using var executionsReader = new StreamReader(executionsStream);
		var executionsText = await executionsReader.ReadToEndAsync();
		return int.TryParse(executionsText, out var executions) ? (code, executions, resources) : (code, 1, resources);
	}
}
