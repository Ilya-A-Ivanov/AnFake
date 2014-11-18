﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AnFake.Api;
using AnFake.Core.Exceptions;
using Common.Logging;

namespace AnFake.Core
{
	public static class NuGet
	{
		private static readonly ILog Log = LogManager.GetLogger("AnFake.Process.NuGet");

		private static readonly string[] Locations =
		{
			".nuget/NuGet.exe"
		};

		public sealed class Params
		{
			public bool IncludeReferencedProjects;
			public bool NoPackageAnalysis;
			public bool NoDefaultExcludes;
			public string AccessKey;
			public string SourceUrl;
			public TimeSpan Timeout;
			public FileSystemPath ToolPath;
			public string ToolArguments;

			internal Params()
			{
				Timeout = TimeSpan.MaxValue;				
			}

			public Params Clone()
			{
				return (Params) MemberwiseClone();
			}
		}

		public static Params Defaults { get; private set; }

		static NuGet()
		{
			Defaults = new Params();

			MyBuild.Initialized += (s, p) =>
			{
				Defaults.ToolPath = Locations.AsFileSet().Select(x => x.Path).FirstOrDefault();
			};
		}

		public static NuSpec.v25.Package Spec25(Action<NuSpec.v25.Metadata> setMeta)
		{
			var pkg = new NuSpec.v25.Package { Metadata = new NuSpec.v25.Metadata() };

			setMeta(pkg.Metadata);

			return pkg;			
		}

		public static IToolExecutionResult Pack(NuSpec.v25.Package nuspec, FolderItem dstFolder)
		{
			return Pack(nuspec, dstFolder, dstFolder, p => { });
		}

		public static IToolExecutionResult Pack(NuSpec.v25.Package nuspec, FolderItem dstFolder, Action<Params> setParams)
		{
			return Pack(nuspec, dstFolder, dstFolder, setParams);
		}

		public static IToolExecutionResult Pack(NuSpec.v25.Package nuspec, FolderItem srcFolder, FolderItem dstFolder)
		{
			return Pack(nuspec, srcFolder, dstFolder, p => { });
		}

		public static IToolExecutionResult Pack(NuSpec.v25.Package nuspec, FolderItem srcFolder, FolderItem dstFolder, Action<Params> setParams)
		{
			if (nuspec == null)
				throw new AnFakeArgumentException("NuGet.Pack(nuspec, srcFolder, dstFolder, setParams): nuspec must not be null");
			if (srcFolder == null)
				throw new AnFakeArgumentException("NuGet.Pack(nuspec, srcFolder, dstFolder, setParams): srcFolder must not be null");
			if (dstFolder == null)
				throw new AnFakeArgumentException("NuGet.Pack(nuspec, srcFolder, dstFolder, setParams): dstFolder must not be null");
			if (setParams == null)
				throw new AnFakeArgumentException("NuGet.Pack(nuspec, srcFolder, dstFolder, setParams): setParams must not be null");

			if (String.IsNullOrEmpty(nuspec.Metadata.Id))
				throw new AnFakeArgumentException("NuSpec.v25.Metadata.Id must not be null or empty");
			if (String.IsNullOrEmpty(nuspec.Metadata.Version))
				throw new AnFakeArgumentException("NuSpec.v25.Metadata.Version must not be null or empty");
			if (String.IsNullOrEmpty(nuspec.Metadata.Authors))
				throw new AnFakeArgumentException("NuSpec.v25.Metadata.Authors must not be null or empty");
			if (String.IsNullOrEmpty(nuspec.Metadata.Description))
				throw new AnFakeArgumentException("NuSpec.v25.Metadata.Description must not be null or empty");			

			var parameters = Defaults.Clone();
			setParams(parameters);

			EnsureToolPath(parameters);
			// TODO: check other parameters			

			var nuspecFile = GenerateNuspecFile(nuspec, srcFolder);

			Logger.DebugFormat("NuGet.Pack => {0}", nuspecFile);

			var args = new Args("-", " ")
				.Command("pack")
				.Param(nuspecFile.Path.Full)
				.Option("OutputDirectory", dstFolder.Path)
				.Option("NoPackageAnalysis", parameters.NoPackageAnalysis)
				.Option("NoDefaultExcludes", parameters.NoDefaultExcludes)
				.Option("IncludeReferencedProjects", parameters.IncludeReferencedProjects)
				.Other(parameters.ToolArguments);

			Folders.Create(dstFolder);

			var result = Process.Run(p =>
			{
				p.FileName = parameters.ToolPath;
				p.Timeout = parameters.Timeout;
				p.Arguments = args.ToString();
				p.Logger = Log;
			});

			result
				.FailIfAnyError("Target terminated due to NuGet errors.")
				.FailIfExitCodeNonZero(String.Format("NuGet.Pack failed with exit code {0}. Package: {1}", result.ExitCode, nuspecFile));

			return result;
		}

		public static IToolExecutionResult Push(FileItem package, Action<Params> setParams)
		{
			if (package == null)
				throw new AnFakeArgumentException("NuGet.Push(package, setParams): package must not be null");
			if (setParams == null)
				throw new AnFakeArgumentException("NuGet.Push(package, setParams): setParams must not be null");

			var parameters = Defaults.Clone();
			setParams(parameters);

			EnsureToolPath(parameters);

			if (String.IsNullOrEmpty(parameters.AccessKey))
				throw new AnFakeArgumentException("NuGet.Params.AccessKey must not be null or empty");

			// TODO: check other parameters

			Logger.DebugFormat("NuGet.Push => {0}", package);

			var args = new Args("-", " ")
				.Command("push")
				.Param(package.Path.Full)
				.Param(parameters.AccessKey)
				.Option("s", parameters.SourceUrl)
				.Other(parameters.ToolArguments);

			var result = Process.Run(p =>
			{
				p.FileName = parameters.ToolPath;
				p.Timeout = parameters.Timeout;
				p.Arguments = args.ToString();
				p.Logger = Log;
			});

			result
				.FailIfAnyError("Target terminated due to NuGet errors.")
				.FailIfExitCodeNonZero(String.Format("NuGet.Push failed with exit code {0}. Package: {1}", result.ExitCode, package));

			return result;
		}

		private static void EnsureToolPath(Params parameters)
		{
			if (parameters.ToolPath == null)
				throw new AnFakeArgumentException(
					String.Format(
						"NuGet.Params.ToolPath must not be null.\nHint: probably, NuGet.exe not found.\nSearch path:\n  {0}",
						String.Join("\n  ", Locations)));
		}

		private static FileItem GenerateNuspecFile(NuSpec.v25.Package nuspec, FolderItem srcFolder)
		{
			var nuspecFile = (srcFolder.Path / nuspec.Metadata.Id + ".nuspec").AsFile();
			
			Folders.Create(nuspecFile.Folder);
			using (var stm = new FileStream(nuspecFile.Path.Full, FileMode.Create, FileAccess.Write))
			{
				new XmlSerializer(typeof(NuSpec.v25.Package)).Serialize(stm, nuspec);
			}

			return nuspecFile;
		}
	}
}