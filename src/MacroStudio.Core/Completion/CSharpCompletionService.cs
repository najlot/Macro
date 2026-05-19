using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MacroStudio.Core.Completion;

public sealed class CSharpCompletionService
{
    private readonly AdhocWorkspace _workspace;
    private readonly DocumentId _documentId;
    private readonly string _prefix;

    public CSharpCompletionService(string[] usings, MetadataReference[] references)
    {
        _prefix = string.Join("\n", usings.Select(static u => $"using {u};"));
        if (_prefix.Length > 0)
        {
            _prefix += "\n\n";
        }

        _prefix += GlobalMethods + "\n\n";

        _workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

        var projectId = ProjectId.CreateNewId();
        _documentId = DocumentId.CreateNewId(projectId);

        var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Script);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithUsings(usings)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        var projectInfo = ProjectInfo
            .Create(
                projectId,
                VersionStamp.Create(),
                name: "MacroScript",
                assemblyName: "MacroScript",
                language: LanguageNames.CSharp,
                parseOptions: parseOptions,
                compilationOptions: compilationOptions)
            .WithMetadataReferences(references);

        _workspace.AddProject(projectInfo);
        _workspace.AddDocument(DocumentInfo.Create(
            _documentId,
            name: "Macro.csx",
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(string.Empty), VersionStamp.Create()))));
    }

    public async Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string code, int caretOffset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code, nameof(code));

        // Inject explicit using directives into the script text. Roslyn completion reliably brings `Console` into scope this way.
        var mappedCode = _prefix + code;
        var mappedCaret = Math.Clamp(_prefix.Length + caretOffset, 0, mappedCode.Length);

        var newText = SourceText.From(mappedCode);
        var updatedSolution = _workspace.CurrentSolution.WithDocumentText(_documentId, newText);
        _workspace.TryApplyChanges(updatedSolution);

        var document = _workspace.CurrentSolution.GetDocument(_documentId);
        if (document is null)
        {
            return [];
        }

        var completionService = CompletionService.GetService(document);
        if (completionService is null)
        {
            return [];
        }

        var list = await completionService.GetCompletionsAsync(document, mappedCaret, cancellationToken: cancellationToken);
        return list?.ItemsList ?? [];
    }

    public string GlobalMethods { get; } = @"
string GetClipboardText(){return default;}
void SetClipboardText(string text){}
Bitmap GetScreenshot(){return default;}
Bitmap GetBitmap(string path){return default;}
Bitmap GetResourceBitmap(string name){return default;}
void SaveBitmap(string path, Bitmap bitmap){}
bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0){return default;}
bool HasBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0){return default;}
Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, int startX = 0, int startY = 0){return default;}
Rectangle SearchBitmap(Bitmap smallBmp, Bitmap bigBmp, double tolerance, int startX = 0, int startY = 0){return default;}
void Simulate(int key, int x, int y, int waitTime){}
void Simulate(int key, int x, int y, bool down, int waitTime){}
void Wait(int milliseconds){}
void WaitMiliseconds(int milliseconds){}
void WaitSeconds(int seconds){}
void WaitMinutes(int minutes){}
void WaitHours(int hours){}
void SetCursorPosition(int x, int y){}
void MoveCursorTo(int x, int y, int ms){}
void MouseLeftDown(){}
void MouseLeftUp(){}
void MouseMiddleDown(){}
void MouseMiddleUp(){}
void MouseRightDown(){}
void MouseRightUp(){}
void PressKeyboardKey(byte key){}
void ReleaseKeyboardKey(byte key){}";
}

