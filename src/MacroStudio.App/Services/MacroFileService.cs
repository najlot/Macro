using System.Collections.ObjectModel;
using System.IO.Compression;
using MacroStudio.App.Models;

namespace MacroStudio.App.Services;

public sealed class MacroFileService : IMacroFileService
{
	private const string CodeEntryName = "code";
	private const string VersionEntryName = "version";
	private const string ExecutionsEntryName = "executions";
	private const string ResourcePrefix = "resources/";
	private const int CurrentVersion = 2;

	public async Task<MacroDocument> LoadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		using var zipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: false);

		var versionText = await ReadRequiredEntryAsync(archive, VersionEntryName, cancellationToken);
		if (!int.TryParse(versionText, out var version) || version is < 1 or > CurrentVersion)
		{
			throw new InvalidDataException($"Unsupported macro version '{versionText}'.");
		}

		var executions = 1;
		var executionsEntry = archive.GetEntry(ExecutionsEntryName);
		if (executionsEntry is not null)
		{
			var executionsText = await ReadEntryAsync(executionsEntry, cancellationToken);
			if (int.TryParse(executionsText, out var parsedExecutions) && parsedExecutions > 0)
			{
				executions = parsedExecutions;
			}
		}

		var resources = new Collection<MacroResource>();
		foreach (var entry in archive.Entries)
		{
			if (!entry.FullName.StartsWith(ResourcePrefix, StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith('/'))
			{
				continue;
			}

			await using var resourceStream = entry.Open();
			using var buffer = new MemoryStream();
			await resourceStream.CopyToAsync(buffer, cancellationToken);
			resources.Add(new MacroResource
			{
				Name = entry.FullName[ResourcePrefix.Length..],
				Value = buffer.ToArray()
			});
		}

		return new MacroDocument
		{
			Code = await ReadRequiredEntryAsync(archive, CodeEntryName, cancellationToken),
			Executions = executions,
			Resources = resources
		};
	}

	public async Task SaveAsync(string filePath, MacroDocument document, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
		ArgumentNullException.ThrowIfNull(document);

		using var zipStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
		using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false);

		await WriteEntryAsync(archive, CodeEntryName, document.Code, CompressionLevel.Optimal, cancellationToken);
		await WriteEntryAsync(archive, VersionEntryName, CurrentVersion.ToString(), CompressionLevel.NoCompression, cancellationToken);
		await WriteEntryAsync(archive, ExecutionsEntryName, document.Executions.ToString(), CompressionLevel.NoCompression, cancellationToken);

		foreach (var resource in document.Resources)
		{
			var entry = archive.CreateEntry(ResourcePrefix + resource.Name, CompressionLevel.Optimal);
			await using var entryStream = entry.Open();
			await entryStream.WriteAsync(resource.Value, cancellationToken);
		}
	}

	private static async Task<string> ReadRequiredEntryAsync(ZipArchive archive, string entryName, CancellationToken cancellationToken)
	{
		var entry = archive.GetEntry(entryName)
			?? throw new InvalidDataException($"Macro file is missing required entry '{entryName}'.");

		return await ReadEntryAsync(entry, cancellationToken);
	}

	private static async Task<string> ReadEntryAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
	{
		await using var entryStream = entry.Open();
		using var reader = new StreamReader(entryStream);
		return await reader.ReadToEndAsync(cancellationToken);
	}

	private static async Task WriteEntryAsync(ZipArchive archive, string entryName, string content, CompressionLevel compressionLevel, CancellationToken cancellationToken)
	{
		var entry = archive.CreateEntry(entryName, compressionLevel);
		await using var entryStream = entry.Open();
		await using var writer = new StreamWriter(entryStream);
		await writer.WriteAsync(content.AsMemory(), cancellationToken);
	}
}