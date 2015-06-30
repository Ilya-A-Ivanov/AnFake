﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AnFake.Api;
using AnFake.Core;
using AnFake.Core.Exceptions;
using Microsoft.FSharp.Compiler;
using Microsoft.FSharp.Compiler.SimpleSourceCodeServices;

namespace AnFake.Scripting
{
	internal static class EmbeddedFsxCompiler
	{
		private const string TempAnFakeFsc = "[Temp]/AnFake.Fsc";
		private const string RootTypeName = "BuildScript";
		private const string VersionPropertyName = "__anfake_fsc_evaluator_version";
		private const string InternalVersion = "1.0";

		public sealed class CompiledScript
		{
			public CompiledScript(FileItem assembly, FileItem source, int linesOffset)
			{
				Assembly = assembly;
				Source = source;
				LinesOffset = linesOffset;
			}

			public FileItem Assembly { get; private set; }

			public FileItem Source { get; private set; }

			public int LinesOffset { get; private set; }

			public void Evaluate()
			{
				var versionProp = System.Reflection.Assembly
					.LoadFile(Assembly.Path.Full)
					.GetType(RootTypeName, true)
					.GetProperty(VersionPropertyName);

				if (versionProp == null)
					throw new EvaluationException(
						String.Format(
							"Property '{0}.{1}' not found in pre-compiled assembly '{2}'.",
							RootTypeName, VersionPropertyName, Assembly));

				// getting value of static property also triggers script evaluation
				try
				{
					var ver = versionProp.GetValue(null);
					if (!InternalVersion.AsVersion().Equals(ver))
						throw new EvaluationException(
							String.Format(
								"Pre-compiled assembly has incompatible version. Expected {0}, actual {1}, assembly path '{2}'.",
								InternalVersion, ver, Assembly));
				}
				catch (TargetInvocationException e)
				{
					Exception anfakeEx = e;
					while (anfakeEx != null && !(anfakeEx is AnFakeException))
					{
						anfakeEx = anfakeEx.InnerException;
					}

					throw anfakeEx ?? e;
				}

				/*var entryPoint = System.Reflection.Assembly
					.LoadFile(Assembly.Path.Full)
					.GetType(_rootType, true)
					.GetMethod(_entryPoint);

				if (entryPoint == null)
					throw new EvaluationException(
						String.Format(
							"Entry point '{0}.{1}' not found in pre-compiled assembly '{2}'",
							_rootType, _entryPoint, Assembly.Path));

				entryPoint.Invoke(null, new object[0]);*/
			}
		}

		private static readonly ISet<string> PredefinedReferences = new HashSet<string>
		{
			"mscorlib",
			"System",
			"System.Core",
			"FSharp.Core"
		};		

		private class PseudoProject
		{
			public string Code { get; private set; }
			public FileItem[] References { get; private set; }
			public string Name { get; private set; }
			public int LinesOffset { get; private set; }

			public FileItem Input
			{
				get { return (TempAnFakeFsc.AsPath() / Name + ".g.fs").AsFile(); }
			}

			public FileItem Output
			{
				get { return (TempAnFakeFsc.AsPath() / Name + ".dll").AsFile(); }
			}

			public PseudoProject(FileItem script, string code, FileItem[] references, int linesOffset)
			{
				Code = code;
				References = references;
				LinesOffset = linesOffset;

				var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(code));
				var name = new StringBuilder(256);
				name.Append(script.Name).Append('.');
				foreach (var b in hash)
				{
					name.AppendFormat("{0:x}", b);
				}

				Name = name.ToString();
			}
		}

		public static CompiledScript Compile(FileItem script)
		{
			Log.DebugFormat("FSC: Compilation requested: '{0}'.", script);

			var fsproj = GeneratePseudoProject(script);

			Log.DebugFormat("FSC: Looking for pre-compiled assembly: '{0}'.", fsproj.Output);

			if (!fsproj.Output.Exists())
			{
				Log.Debug("FSC: Pre-compiled assembly NOT found. Will compile.");

				DoCompile(fsproj);
			}
			else
			{
				Log.Debug("FSC: Pre-compiled assembly found. No compilation needed.");
			}

			return new CompiledScript(fsproj.Output, fsproj.Input, fsproj.LinesOffset);
		}

		public static void Cleanup()
		{
			var threshold = DateTime.UtcNow - TimeSpan.FromHours(24);

			foreach (var file in "*".AsFileSetFrom(TempAnFakeFsc)
				.Where(file => file.Info.LastAccessTimeUtc < threshold))
			{
				Files.Delete(file);
			}
		}

		private static void DoCompile(PseudoProject fsproj)
		{
			Log.Debug("FSC: Compiling...");
			
			Text.WriteTo(fsproj.Input, fsproj.Code);

			var args = new List<string>
			{
				"fsc.exe",				
				"-o",
				fsproj.Output.Path.Full,
				"-a",
				fsproj.Input.Path.Full,
				"--debug:pdbonly",
				"--optimize+",
				"--noframework"
			};

			foreach (var reference in fsproj.References)
			{
				args.Add("-r");
				args.Add(reference.Path.Spec);
			}

			var scs = new SimpleSourceCodeServices();
			var ret = scs.Compile(args.ToArray());

			foreach (var errorInfo in ret.Item1)
			{
				if (FSharpErrorSeverity.Error.Equals(errorInfo.Severity))
				{
					Log.ErrorFormat("[FSC-ERROR]: {0}", errorInfo);
				}
				else if (FSharpErrorSeverity.Warning.Equals(errorInfo.Severity))
				{
					Log.DebugFormat("[FSC-WARN ]: {0}", errorInfo);
				}
				else
				{
					Log.DebugFormat("[FSC-INFO ]: {0}", errorInfo);
				}
			}

			if (ret.Item2 != 0)
				throw new EvaluationException(String.Format("Unable to pre-compile script. fsc.exe exit code {0}.", ret.Item2));

			Log.DebugFormat("FSC: Pre-compiled assembly created: '{0}'.", fsproj.Output);
		}		

		private static PseudoProject GeneratePseudoProject(FileItem script)
		{
			var refs = new List<FileItem>();
			refs.AddRange(
				AppDomain.CurrentDomain
					.GetAssemblies()
					.Where(x => PredefinedReferences.Contains(x.GetName().Name))
					.Select(x => x.Location.AsFile()));
			
			var header = new StringBuilder(1024);
			header.Append("module ").Append(RootTypeName).AppendLine();
			var linesOffset = 1;

			var body = new StringBuilder(10240);
			
			script.AsTextDoc().ForEachLine(
				line =>
				{
					if (line.Text.StartsWith("#r "))
					{
						refs.Add(GetFileRefFromDerictive(line.Text, "r"));

						body.Append("//").AppendLine(line.Text);
					}
					else if (line.Text.StartsWith("open "))
					{
						header.AppendLine(line.Text);
						linesOffset++;

						body.Append("//").AppendLine(line.Text);
					}
					else if (line.Text.StartsWith("#load "))
					{
						var compiledScript = Compile(GetFileRefFromDerictive(line.Text, "load"));
						refs.Add(compiledScript.Assembly);

						body.Append("//").AppendLine(line.Text);
					}
					else
					{
						body.AppendLine(line.Text);
					}
				});

			body.Append("let ").Append(VersionPropertyName).Append("=new Version(\"").Append(InternalVersion).AppendLine("\")");

			return new PseudoProject(script, header.ToString() + body, refs.ToArray(), linesOffset);
		}

		private static FileItem GetFileRefFromDerictive(string line, string derictiveName)
		{
			var beg = line.IndexOf('"');
			var end = line.LastIndexOf('"');
			if (beg < 0 || end <= beg)
				throw new EvaluationException(String.Format("Invalid #{0} syntax. Expected: '#{0} \"<file-path>\"' but '{1}' given.", derictiveName, line));

			return line.Substring(beg + 1, end - beg - 1).AsFile();
		}
	}

	internal class FSharpFscEvaluator : IScriptEvaluator
	{
		public void Evaluate(FileItem script)
		{
			EmbeddedFsxCompiler.Cleanup();

			var compiledScript = EmbeddedFsxCompiler.Compile(script);

			AnFakeException.ScriptSource = new ScriptSourceInfo(
				script.Name, 
				compiledScript.Source.Name, 
				compiledScript.LinesOffset);

			compiledScript.Evaluate();		
		}
	}
}