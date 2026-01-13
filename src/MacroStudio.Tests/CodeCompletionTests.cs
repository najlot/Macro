using MacroStudio.Core.Completion;
using Microsoft.CodeAnalysis;

namespace MacroStudio.Tests;

public class CodeCompletionTests
{
	[Fact]
	public async Task SystemConsoleWriteLineTest()
	{
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        };

        var service = new CSharpCompletionService(
            usings: ["System"],
            references: references);

        var code = "Console.";

        var list = await service.GetCompletionItemsAsync(code, code.Length);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, item => item.DisplayText == "WriteLine");
    }

    [Fact]
    public async Task GetClipboardTextTest()
    {
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        };

        var service = new CSharpCompletionService(
            usings: ["System"],
            references: references);

        var code = "GetClip";

        var list = await service.GetCompletionItemsAsync(code, code.Length);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, item => item.DisplayText == "GetClipboardText");
    }
}
