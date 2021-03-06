using System;
using System.Collections.Generic;
using AnFake.Api;
using AnFake.Integration.Tfs2012;
using Microsoft.TeamFoundation.Build.Client;

namespace AnFake.Plugins.Tfs2012
{
	/// <summary>
	///		Represents custom section in build summary displayed in Visual Studio's build view.
	/// </summary>
	public sealed class TfsBuildSummarySection
	{
		private readonly TfsMessageBuilder _message;
		private readonly IBuildDetail _build;
		private readonly string _key;
		private readonly string _header;
		private readonly int _priority;
		
		internal TfsBuildSummarySection(IBuildDetail build, string key, string header, int priority)
		{
			_message = new TfsMessageBuilder();
			_build = build;
			_key = key;
			_header = header;
			_priority = priority;			
		}
				
		public TfsBuildSummarySection Append(string text)
		{
			if (text == null)
				throw new ArgumentException("TfsBuildSummarySection.Append(text): text must not be null");

			_message.Append(text);

			return this;
		}

		public TfsBuildSummarySection AppendFormat(string format, params object[] args)
		{
			if (format == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendFormat(format, ...): format must not be null");

			_message.AppendFormat(format, args);

			return this;
		}

		public TfsBuildSummarySection AppendLink(Hyperlink link)
		{
			if (link == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLink(link): link must not be null");

			_message.AppendLink(link);

			return this;
		}

		public TfsBuildSummarySection AppendLink(Uri href)
		{
			if (href == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLink(href): href must not be null");

			_message.AppendLink(href);

			return this;
		}

		public TfsBuildSummarySection AppendLink(string label, Uri href)
		{
			if (String.IsNullOrEmpty(label))
				throw new ArgumentException("TfsBuildSummarySection.AppendLink(label, href): label must not be null or empty");
			if (href == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLink(label, href): href must not be null");

			_message.AppendLink(label, href);

			return this;
		}		

		public TfsBuildSummarySection AppendLinks(IList<Hyperlink> links, string prefix = " ", string separator = " | ", string suffix = "")
		{
			if (links == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLinks(links[, prefix, separator, suffix]): links must not be null");
			if (prefix == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLinks(links, prefix, separator, suffix): prefix must not be null");
			if (separator == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLinks(links, prefix, separator, suffix): separator must not be null");
			if (suffix == null)
				throw new ArgumentException("TfsBuildSummarySection.AppendLinks(links, prefix, separator, suffix): suffix must not be null");

			_message.AppendLinks(links, prefix, separator, suffix);

			return this;
		}

		public TfsBuildSummarySection Push()
		{
			_build.Information
				.AddCustomSummaryInformation(_message.ToString(), _key, _header, _priority);

			_message.Clear();

			return this;
		}

		public void Save()
		{
			_build.Information.Save();
		}				
	}
}