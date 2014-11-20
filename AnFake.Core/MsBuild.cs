﻿using System;
using System.Collections.Generic;
using System.Linq;
using AnFake.Api;
using AnFake.Core.Exceptions;
using Common.Logging;
using Microsoft.Build.Framework;

namespace AnFake.Core
{
	public static class MsBuild
	{
		private static readonly ILog Log = LogManager.GetLogger("AnFake.Process.MsBuild");

		private static readonly string[] Locations =
		{
			"[ProgramFilesx86]/MSBuild/12.0/Bin/MsBuild.exe"
		};

		public sealed class Params
		{
			public string[] Targets;
			public readonly IDictionary<string, string> Properties = new Dictionary<string, string>();
			public int? MaxCpuCount;
			public bool NodeReuse;
			public LoggerVerbosity Verbosity;
			public TimeSpan Timeout;
			public FileSystemPath ToolPath;
			public string ToolArguments;

			internal Params()
			{
				Targets = new[] {"Build"};
				Verbosity = LoggerVerbosity.Normal;
				Timeout = TimeSpan.MaxValue;
				ToolPath = Locations.AsFileSet().Select(x => x.Path).FirstOrDefault();
			}

			public Params Clone()
			{
				return (Params) MemberwiseClone();
			}
		}

		public static Params Defaults { get; private set; }

		static MsBuild()
		{
			Defaults = new Params();

			MyBuild.Initialized += (s, p) =>
			{
				Defaults.Verbosity = p.Verbosity;

				foreach (var property in p.Properties)
				{
					if (!property.Key.StartsWith("MsBuild.", StringComparison.InvariantCulture))
						continue;					

					Defaults.Properties[property.Key.Substring(8)] = property.Value;
				}
			};
		}

		public static IToolExecutionResult BuildDebug(FileItem solution)
		{
			if (solution == null)
				throw new AnFakeArgumentException("MsBuild.BuildDebug(solution): solution must not be null");

			return BuildDebug(new[] {solution});
		}

		public static IToolExecutionResult BuildDebug(IEnumerable<FileItem> projects)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.BuildDebug(projects): projects must not be null");

			return Build(projects, p => { p.Properties["Configuration"] = "Debug"; });
		}

		public static IToolExecutionResult BuildDebug(IEnumerable<FileItem> projects, FileSystemPath output)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.BuildDebug(projects, output): projects must not be null");
			if (output == null)
				throw new AnFakeArgumentException("MsBuild.BuildDebug(projects, output): output must not be null");

			return Build(projects, p =>
			{
				p.Properties["Configuration"] = "Debug";
				p.Properties["OutDir"] = output.Full;
			});
		}

		public static IToolExecutionResult BuildRelease(FileItem solution)
		{
			if (solution == null)
				throw new AnFakeArgumentException("MsBuild.BuildRelease(solution): solution must not be null");

			return BuildRelease(new[] {solution});
		}

		public static IToolExecutionResult BuildRelease(IEnumerable<FileItem> projects)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.BuildRelease(projects): projects must not be null");

			return Build(projects, p => { p.Properties["Configuration"] = "Release"; });
		}

		public static IToolExecutionResult BuildRelease(IEnumerable<FileItem> projects, FileSystemPath output)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.BuildRelease(projects, output): projects must not be null");
			if (output == null)
				throw new AnFakeArgumentException("MsBuild.BuildRelease(projects, output): output must not be null");

			return Build(projects, p =>
			{
				p.Properties["Configuration"] = "Release";
				p.Properties["OutDir"] = output.Full;
			});
		}

		public static IToolExecutionResult Build(FileItem solution)
		{
			if (solution == null)
				throw new AnFakeArgumentException("MsBuild.Build(solution): solution must not be null");

			return Build(new[] {solution}, p => { });
		}

		public static IToolExecutionResult Build(IEnumerable<FileItem> projects)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.Build(projects): projects must not be null");

			return Build(projects, p => { });
		}

		public static IToolExecutionResult Build(FileItem solution, Action<Params> setParams)
		{
			if (solution == null)
				throw new AnFakeArgumentException("MsBuild.Build(solution, setParams): solution must not be null");
			if (setParams == null)
				throw new AnFakeArgumentException("MsBuild.Build(solution, setParams): setParams must not be null");

			return Build(new[] {solution}, setParams);
		}

		public static IToolExecutionResult Build(IEnumerable<FileItem> projects, Action<Params> setParams)
		{
			if (projects == null)
				throw new AnFakeArgumentException("MsBuild.Build(projects, setParams): projects must not be null");
			if (setParams == null)
				throw new AnFakeArgumentException("MsBuild.Build(projects, setParams): setParams must not be null");

			var projArray = projects.ToArray();
			if (projArray.Length == 0)
				throw new AnFakeArgumentException("MsBuild.Build(projects, setParams): projects set must not be empty");

			var parameters = Defaults.Clone();
			setParams(parameters);

			if (parameters.ToolPath == null)
				throw new AnFakeArgumentException(
					String.Format(
						"MsBuild.Params.ToolPath must not be null.\nHint: probably, MsBuild.exe not found.\nSearch path:\n  {0}",
						String.Join("\n  ", Locations)));
			// TODO: check other parameters

			Logger.DebugFormat("MsBuild\n => {0}", String.Join("\n => ", projArray.Select(x => x.RelPath)));

			var summary = new ToolExecutionResult();
			foreach (var proj in projArray)
			{
				var args = new Args("/", ":")
					.Param(proj.Path.Full)
					.Option("t", parameters.Targets, ";")
					.Option("m", parameters.MaxCpuCount)
					.Option("nodeReuse", parameters.NodeReuse)
					.Option("v", parameters.Verbosity)
					.Other(parameters.ToolArguments);

				var propArgs = new Args("/", "=");
				foreach (var prop in parameters.Properties)
				{
					propArgs.Option(String.Format("p:{0}", prop.Key), prop.Value);
				}

				args.Space().NonQuotedValue(propArgs.ToString());

				var loggerT = typeof (Integration.MsBuild.Logger);
				args.Space()
					.ValuedOption("logger")
					.NonQuotedValue(loggerT.FullName)
					.NonQuotedValue(",")
					.QuotedValue(loggerT.Assembly.Location)
					.NonQuotedValue(";")
					.QuotedValue(Tracer.Uri + "#" + Target.Current.Name);

				var result = Process.Run(p =>
				{
					p.FileName = parameters.ToolPath;
					p.Timeout = parameters.Timeout;
					p.Arguments = args.ToString();
					p.Logger = Log;
					p.TrackExternalMessages = true;
				});

				result
					.FailIfAnyError("Target terminated due to MsBuild errors.")
					.FailIfExitCodeNonZero(String.Format("MsBuild failed with exit code {0}. Solution: {1}", result.ExitCode, proj));

				summary += result;
			}

			return summary;
		}
	}
}