//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Pipeline.g4 by ANTLR 4.5

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

namespace AnFake.Api.Pipeline.Antlr {
using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5")]
[System.CLSCompliant(false)]
public partial class PipelineLexer : Lexer {
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, Identifier=9, 
		QuotedIdentifier=10, Spaces=11;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "Identifier", 
		"QuotedIdentifier", "Spaces"
	};


	public PipelineLexer(ICharStream input)
		: base(input)
	{
		Interpreter = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, "'=>'", "'->'", "'('", "')'", "'['", "']'", "'as'", "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, "Identifier", "QuotedIdentifier", 
		"Spaces"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Pipeline.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public static readonly string _serializedATN =
		"\x3\x430\xD6D1\x8206\xAD2D\x4417\xAEF1\x8D80\xAADD\x2\r\x42\b\x1\x4\x2"+
		"\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b\x4"+
		"\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x3\x2\x3\x2\x3\x2\x3\x3\x3\x3\x3\x3"+
		"\x3\x4\x3\x4\x3\x5\x3\x5\x3\x6\x3\x6\x3\a\x3\a\x3\b\x3\b\x3\b\x3\t\x3"+
		"\t\x3\n\x3\n\a\n/\n\n\f\n\xE\n\x32\v\n\x3\v\x3\v\x6\v\x36\n\v\r\v\xE\v"+
		"\x37\x3\v\x3\v\x3\f\x6\f=\n\f\r\f\xE\f>\x3\f\x3\f\x2\x2\r\x3\x3\x5\x4"+
		"\a\x5\t\x6\v\a\r\b\xF\t\x11\n\x13\v\x15\f\x17\r\x3\x2\a\x5\x2\x43\\\x61"+
		"\x61\x63|\x6\x2\x32;\x43\\\x61\x61\x63|\x3\x2))\f\x2\"\"%%\'(--/<??\x41"+
		"\\^^\x61\x61\x63|\x5\x2\v\f\xF\xF\"\"\x44\x2\x3\x3\x2\x2\x2\x2\x5\x3\x2"+
		"\x2\x2\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2\x2"+
		"\x2\x2\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15\x3"+
		"\x2\x2\x2\x2\x17\x3\x2\x2\x2\x3\x19\x3\x2\x2\x2\x5\x1C\x3\x2\x2\x2\a\x1F"+
		"\x3\x2\x2\x2\t!\x3\x2\x2\x2\v#\x3\x2\x2\x2\r%\x3\x2\x2\x2\xF\'\x3\x2\x2"+
		"\x2\x11*\x3\x2\x2\x2\x13,\x3\x2\x2\x2\x15\x33\x3\x2\x2\x2\x17<\x3\x2\x2"+
		"\x2\x19\x1A\a?\x2\x2\x1A\x1B\a@\x2\x2\x1B\x4\x3\x2\x2\x2\x1C\x1D\a/\x2"+
		"\x2\x1D\x1E\a@\x2\x2\x1E\x6\x3\x2\x2\x2\x1F \a*\x2\x2 \b\x3\x2\x2\x2!"+
		"\"\a+\x2\x2\"\n\x3\x2\x2\x2#$\a]\x2\x2$\f\x3\x2\x2\x2%&\a_\x2\x2&\xE\x3"+
		"\x2\x2\x2\'(\a\x63\x2\x2()\au\x2\x2)\x10\x3\x2\x2\x2*+\a.\x2\x2+\x12\x3"+
		"\x2\x2\x2,\x30\t\x2\x2\x2-/\t\x3\x2\x2.-\x3\x2\x2\x2/\x32\x3\x2\x2\x2"+
		"\x30.\x3\x2\x2\x2\x30\x31\x3\x2\x2\x2\x31\x14\x3\x2\x2\x2\x32\x30\x3\x2"+
		"\x2\x2\x33\x35\t\x4\x2\x2\x34\x36\t\x5\x2\x2\x35\x34\x3\x2\x2\x2\x36\x37"+
		"\x3\x2\x2\x2\x37\x35\x3\x2\x2\x2\x37\x38\x3\x2\x2\x2\x38\x39\x3\x2\x2"+
		"\x2\x39:\t\x4\x2\x2:\x16\x3\x2\x2\x2;=\t\x6\x2\x2<;\x3\x2\x2\x2=>\x3\x2"+
		"\x2\x2><\x3\x2\x2\x2>?\x3\x2\x2\x2?@\x3\x2\x2\x2@\x41\b\f\x2\x2\x41\x18"+
		"\x3\x2\x2\x2\x6\x2\x30\x37>\x3\b\x2\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace AnFake.Api.Pipeline.Antlr
