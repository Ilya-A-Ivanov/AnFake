﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using AnFake.Api;
using AnFake.Core.Exceptions;

namespace AnFake.Core
{
	public static class MyBuild
	{
		public sealed class Params
		{
			public readonly IDictionary<string, string> Properties;
			public readonly FileSystemPath Path;
			public readonly FileItem LogFile;
			public readonly FileItem ScriptFile;
			public readonly string[] Targets;			
			public readonly Verbosity Verbosity;

			internal Params(FileSystemPath path, FileItem logFile, FileItem scriptFile,
				Verbosity verbosity, string[] targets, IDictionary<string, string> properties)
			{
				Path = path;
				LogFile = logFile;
				ScriptFile = scriptFile;
				Targets = targets;
				Properties = properties;
				Verbosity = verbosity;								
			}			
		}

		public static Params Defaults { get; private set; }

		private static bool _isInitialized;
		private static event EventHandler<Params> InitializedHandlers;

		internal static void Initialize(Params @params)
		{
			if (_isInitialized)
				throw new InvalidConfigurationException("MyBuild already initialized.");

			Debug.Assert(Path.IsPathRooted(@params.LogFile.Path.Spec), "LogFile must have absolute path.");
			Debug.Assert(Path.IsPathRooted(@params.ScriptFile.Path.Spec), "ScriptFile must have absolute path.");

			Defaults = @params;
			
			_isInitialized = true;
			if (InitializedHandlers != null)
			{
				InitializedHandlers.Invoke(null, Defaults);
			}
		}

		public static event EventHandler<Params> Initialized
		{
			add
			{
				InitializedHandlers += value;
				if (_isInitialized)
				{
					value.Invoke(null, Defaults);
				}
			}
			remove { InitializedHandlers -= value; }
		}

		/// <summary>
		/// Returns true if build property 'name' is specified and has non-empty value.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool HasProp(string name)
		{
			if (String.IsNullOrEmpty(name))
				throw new AnFakeArgumentException("MyBuild.HasProp(name): name must not be null or empty");

			string value;
			return Defaults.Properties.TryGetValue(name, out value) && !String.IsNullOrEmpty(value);
		}

		/// <summary>
		/// Returns non empty value of build property 'name'. If no such property specified or it has empty value then exception is thrown.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string GetProp(string name)
		{
			if (String.IsNullOrEmpty(name))
				throw new AnFakeArgumentException("MyBuild.GetProp(name): name must not be null or empty");
			
			string value;
			if (!Defaults.Properties.TryGetValue(name, out value))
				throw new InvalidConfigurationException(String.Format("Build property '{0}' is not specified.", name));
			if (String.IsNullOrEmpty(value))
				throw new InvalidConfigurationException(String.Format("Build property '{0}' has empty value.", name));

			return value;
		}		

		/// <summary>
		/// Returns non empty value of build property 'name'. If no such property specified or it has empty value then 'defaultValue' is returned.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetProp(string name, string defaultValue)
		{
			if (String.IsNullOrEmpty(name))
				throw new AnFakeArgumentException("MyBuild.GetProp(name, defaultValue): name must not be null or empty");

			string value;
			return Defaults.Properties.TryGetValue(name, out value) && !String.IsNullOrEmpty(value)
				? value
				: defaultValue;
		}

		public static void SetProp(string name, string value)
		{
			if (String.IsNullOrEmpty(name))
				throw new AnFakeArgumentException("MyBuild.SetProp(name, value): name must not be null or empty");
			if (value == null)
				throw new AnFakeArgumentException("MyBuild.SetProp(name, value): value must not be null");
			
			Defaults.Properties[name] = value;
		}

		public static void SaveProp(params string[] names)
		{
			if (names.Length == 0)
				throw new ArgumentException("MyBuild.SaveProp(name[, ...]): at least one name should be specified");

			if (names.Any(String.IsNullOrEmpty))
				throw new ArgumentException("MyBuild.SaveProp(name[, ...]): name must not be null or empty");

			foreach (var name in names)
			{
				string value;
				if (Defaults.Properties.TryGetValue(name, out value))
				{
					Settings.Current.Set(name, value);
				}
				else
				{
					Settings.Current.Remove(name);
				}				
			}

			Settings.Current.Save();
		}		

		public static void Failed(string format, params object[] args)
		{
			if (String.IsNullOrEmpty(format))
				throw new AnFakeArgumentException("MyBuild.Failed(format): format must not be null or empty");

			throw new TargetFailureException(String.Format(format, args));
		}		
	}
}