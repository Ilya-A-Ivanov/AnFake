﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AnFake.Plugins.Tfs2012.Test;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnFake.Tfs2012.Test
{	
	[TestClass]
	public class Experimental : TfsTestSuite
	{
		[TestInitialize]
		public override void Initialize()
		{
			base.Initialize();
		}

		[Ignore]
		[TestMethod]
		public void CreateBuild()
		{
			var buildUri = CreateTestBuild().Uri;

			Trace.WriteLine("Build: " + buildUri);
		}

		[Ignore]
		[TestMethod]
		public void ExploreBuildDetailInformation()
		{
			var teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsUri));
			var buildService = (IBuildServer)teamProjectCollection.GetService(typeof(IBuildServer));

			//IBuildDefinition buildDefinition = buildService.GetBuildDefinition(TeamProject, "BuildDefinitionName");
			var buildDetail = buildService.QueryBuildsByUri(
				new[] { new Uri("vstfs:///Build/Build/34861") }, 
				new[] { "*" }, 
				QueryOptions.All).Single();		

			//var node = Find(buildDetail.Information.Nodes[0], "DisplayText", "package must be empty");
			var node = Find(buildDetail.Information.Nodes[0], "Any CPU|Debug|AllInOne");
		}

		[Ignore]
		[TestMethod]
		public void Test2()
		{
			var teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsUri));
			var buildServer = (IBuildServer)teamProjectCollection.GetService(typeof(IBuildServer));

			var definition = buildServer.GetBuildDefinition(TeamProject, BuildDefinition);
			var detail = definition.CreateManualBuild("0006", DropLocation);

			detail.Information
				.AddActivityTracking("0001", "Sequence", "General")/*.Node.Children
				.AddBuildError("FileSet_should_support_up_steps_and_wildcard_steps: Assert.IsTrue failed.", DateTime.Now)*/;

			/*detail.Information
				.AddActivityTracking("01", "Sequence", "General").Node.Children				
				.AddBuildError("Test failure", DateTime.Now).ErrorType = "Test.Unit";

			detail.Information
				.AddConfigurationSummary("Configuration", "Platform").Node.Children
				.AddBuildError("Compilation failure", DateTime.Now).ErrorType = "Compilation";*/

			/*detail.Information
				.AddCustomSummaryInformation("Compile: 0 error(s) 0 warning(s) SUCCESSED", "AnFake", "AnFake 'Build' Summary", 170);
			detail.Information
				.AddCustomSummaryInformation("Test.Unit: 1 error(s) 0 warning(s) FAILED", "AnFake", "AnFake 'Build' Summary", 170);*/
			//detail.Information
			//	.AddCustomSummaryInformation("[0001] FileSet_should_support_up_steps_and_wildcard_steps: Assert.IsTrue failed.", "AnFake", "'Build' Summary", 170);
			/*detail.Information
				.AddCustomSummaryInformation("====================", "AnFake", "AnFake 'Build' Summary", 170);
			detail.Information
				.AddCustomSummaryInformation("'Build' Failed. See the section below for error/warning details...", "AnFake", "AnFake 'Build' Summary", 170);*/

			/*var node = detail.Information.AddBuildProjectNode(DateTime.Now, "Debug", "MySolution.sln", "x86", "$/project/MySolution.sln", DateTime.Now, "Default");
			node.CompilationErrors = 1;
			node.CompilationWarnings = 1;			

			node.Node.Children.AddBuildError("Compilation", "File1.cs", 12, 5, "", "Syntax error", DateTime.Now);
			node.Node.Children.AddBuildWarning("File2.cs", 3, 1, "", "Some warning", DateTime.Now, "Compilation");
			
			node.Node.Children.AddBuildError("Test failure", DateTime.Now).ErrorType = "Test";

			node.Node.Children.AddExternalLink("Log File", new Uri(@"\\server\share\logfiledebug.txt"));			
			node.Save();*/
			
			/*buildProjectNode = detail.Information.AddBuildProjectNode(DateTime.Now, "Release", "MySolution.sln", "x86", "$/project/MySolution.sln", DateTime.Now, "Default");
			buildProjectNode.CompilationErrors = 0;
			buildProjectNode.CompilationWarnings = 0;

			buildProjectNode.Node.Children.AddExternalLink("Log File", new Uri(@"\\server\share\logfilerelease.txt"));
			buildProjectNode.Save();*/

			detail.Information.Save();
			detail.FinalizeStatus(BuildStatus.Failed);

			var uri = detail.Uri;
			//detail.Save();
		}

		[Ignore]
		[TestMethod]
		public void FindAndDeleteBuild()
		{
			var teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsUri));
			var buildService = (IBuildServer)teamProjectCollection.GetService(typeof(IBuildServer));

			var buildDefinition = buildService.GetBuildDefinition(TeamProject, BuildDefinition);
			var buildDetail = buildDefinition
				.QueryBuilds()
				.First(x => (x.Status & BuildStatus.InProgress) != 0);

			buildDetail.Stop();

			buildDetail.KeepForever = false;
			buildDetail.Save();
			
			buildDetail.Delete();
		}

		[Ignore]
		[TestMethod]
		public void GetSpecificVersion()
		{
			var teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsUri));
			var vcs = teamProjectCollection.GetService<VersionControlServer>();

			var content = "";
			var item = vcs.GetItem(".workspace", VersionSpec.ParseSingleSpec("C", ""));
			using (var downstream = item.DownloadFile())
			{
				content = new StreamReader(downstream).ReadToEnd();
			}			

			Assert.IsTrue(content.Contains("AnFake/1.0.2"));
		}

		private static IBuildInformationNode Find(IBuildInformationNode node, string fieldName, string fieldValue)
		{
			if (node.Fields.ContainsKey(fieldName) && node.Fields[fieldName].Contains(fieldValue))
				return node;

			return node.Children.Nodes
				.Select(child => Find(child, fieldName, fieldValue))
				.FirstOrDefault(ret => ret != null);
		}

		private static IBuildInformationNode Find(IBuildInformationNode node, string fieldValue)
		{
			if (node.Fields.Values.Any(x => x.Contains(fieldValue)))
				return node;

			return node.Children.Nodes
				.Select(child => Find(child, fieldValue))
				.FirstOrDefault(ret => ret != null);
		}
	}
}
