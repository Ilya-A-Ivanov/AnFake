﻿using System;
using System.Activities;
using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace AnFake.Integration.Tfs2012
{
	[BuildActivity(HostEnvironmentOption.All)]
	public sealed class ExpandWorkspace : CodeActivity
	{
		[RequiredArgument]
		public InArgument<IBuildDetail> BuildDetail { get; set; }

		[RequiredArgument]
		public InArgument<Workspace> Workspace { get; set; }

		public InArgument<UseWorkspaceFrom> WorkspaceSource { get; set; }

		public InArgument<string> WorkspaceFile { get; set; }

		public ExpandWorkspace()
		{
			WorkspaceSource = new InArgument<UseWorkspaceFrom>(UseWorkspaceFrom.File);
			WorkspaceFile = new InArgument<string>(".workspace");
		}

		protected override void Execute(CodeActivityContext context)
		{
			var buildDetail = BuildDetail.Get(context);
			var workspace = Workspace.Get(context);
			var workspaceSource = WorkspaceSource.Get(context);
			var workspaceFile = WorkspaceFile.Get(context);

			if (workspaceSource != UseWorkspaceFrom.File)
				throw new NotSupportedException("Only UseWorkspaceFrom.File is supported now.");

			if (workspace.Folders.Length != 1)
				throw new InvalidOperationException(
					String.Format(
						"ExpandWorkspace: Just one folder mapping expected in 'UseWorkspaceFrom.File' mode." +
						" Workspace to be expanded should contain only mapping for folder with '{0}' file.",
						workspaceFile));

			var wsPath = ServerPathUtils.Combine(workspace.Folders[0].ServerItem, workspaceFile);
			var localPath = workspace.Folders[0].LocalItem;

			context.TrackBuildMessage(String.Format("Expanding workspace: {0} => {1}", wsPath, localPath));

			var wsDesc = GetTextContent(buildDetail.BuildServer.TeamProjectCollection, wsPath);
			var mappings = VcsMappings.Parse(wsDesc, workspace.Folders[0].ServerItem, localPath);

			context.TrackBuildMessage(String.Format("{0} // {1}", workspace.Name, workspace.Comment));
			foreach (var mapping in mappings)
			{
				context.TrackBuildMessage(String.Format("  {0} => {1}", mapping.ServerItem, mapping.IsCloaked ? "(cloacked)" : mapping.LocalItem));
			}

			workspace.Update(workspace.Name, workspace.Comment, mappings);

			context.TrackBuildMessage("Workspace sucessfully expanded.");
		}

		private static string GetTextContent(TfsConnection tfs, string tfsPath)
		{
			var vcs = tfs.GetService<VersionControlServer>();

			var item = vcs.GetItem(tfsPath);
			using (var downstream = item.DownloadFile())
			{
				return new StreamReader(downstream).ReadToEnd();
			}
		}
	}
}