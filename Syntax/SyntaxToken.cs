using System.Diagnostics;

namespace Linde.Syntax;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed record class SyntaxToken(SyntaxKind Kind, string Text, int Position)
{
    public override string ToString() => $"SyntaxToken(Kind: {Kind}, Text: '{Text}', Position: {Position})";

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
