using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AnFake.Api
{
	/// <summary>
	///     Represents typed message of build trace.
	/// </summary>
	[DataContract(Name = "Generic", Namespace = "")]
	public sealed class TraceMessage : IFormattable
	{
		public sealed class Builder
		{
			private readonly TraceMessage _msg = new TraceMessage();
			private readonly Action<TraceMessage> _endAction;

			public Builder(Action<TraceMessage> endAction)
			{
				if (endAction == null)
					throw new ArgumentException("TraceMessage.Builder(endAction): endAction must not be null");

				_endAction = endAction;
			}

			public Builder Info()
			{
				_msg.Level = TraceMessageLevel.Info;
				return this;
			}

			public Builder Warn()
			{
				_msg.Level = TraceMessageLevel.Warning;
				return this;
			}

			public Builder Error()
			{
				_msg.Level = TraceMessageLevel.Error;
				return this;
			}

			public Builder Summary()
			{
				_msg.Level = TraceMessageLevel.Summary;
				return this;
			}

			public Builder WithText(string text)
			{
				if (String.IsNullOrEmpty(text))
					throw new ArgumentException("TraceMessage.Builder.WithText(text): text must not be null or empty");

				_msg.Message = text;
				return this;
			}

			public Builder WithText(StringBuilder text)
			{
				if (text == null)
					throw new ArgumentException("TraceMessage.Builder.WithText(text): text must not be null");

				_msg.Message = text.ToString();
				return this;
			}

			public Builder WithFormat(string fmt, params object[] args)
			{
				if (String.IsNullOrEmpty(fmt))
					throw new ArgumentException("TraceMessage.Builder.WithFormat(fmt, ...): fmt must not be null or empty");

				_msg.Message = String.Format(fmt, args);
				return this;
			}

			public Builder WithDetails(string details)
			{
				_msg.Details = details;
				return this;
			}

			public Builder WithLinks(IEnumerable<Hyperlink> links)
			{
				if (links == null)
					throw new ArgumentException("TraceMessage.Builder.WithLinks(links): links must not be null");

				_msg.Links.AddRange(links);
				return this;
			}

			public Builder WithLink(string href, string label)
			{
				_msg.Links.Add(new Hyperlink(href, label));
				return this;
			}

			public Builder WithLink(Uri href, string label)
			{
				_msg.Links.Add(new Hyperlink(href, label));
				return this;
			}

			public Builder WithCategory(string category)
			{
				if (String.IsNullOrEmpty(category))
					throw new ArgumentException("TraceMessage.Builder.WithCategory(category): category must not be null or empty");

				_msg.Category = category;
				return this;
			}

			public Builder AsTestTrace()
			{
				_msg.Category = TraceMessageCategory.TestTrace;
				return this;
			}

			public Builder AsTestSummary()
			{
				_msg.Category = TraceMessageCategory.TestSummary;
				return this;
			}

			public void End()
			{
				if (String.IsNullOrEmpty(_msg.Message))
					throw new ArgumentException("TraceMessage.Builder.End: message must not be null or empty");

				_endAction.Invoke(_msg);
			}
		}

		private List<Hyperlink> _links;

		private TraceMessage()
		{			
		}

		public TraceMessage(TraceMessageLevel level, string message)
		{
			Level = level;
			Message = message;
		}

		[DataMember]
		public TraceMessageLevel Level { get; private set; }

		[DataMember]
		public string Message { get; private set; }

		[DataMember(EmitDefaultValue = false)]
		public string Details { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Code { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string File { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Project { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public int Line { get; set; }

		public bool HasLine
		{
			get { return Line > 0; }
		}

		[DataMember(EmitDefaultValue = false)]
		public int Column { get; set; }

		public bool HasColumn
		{
			get { return Line > 0; }
		}

		[DataMember(EmitDefaultValue = false)]
		public string Target { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public int NodeId { get; set; }

		public bool HasNodeId
		{
			get { return NodeId > 0; }
		}

		[DataMember(EmitDefaultValue = false)]
		public int ProcessId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public int ThreadId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Category { get; set; }

		[DataMember]
		public List<Hyperlink> Links
		{
			get { return _links ?? (_links = new List<Hyperlink>()); }
		}		

		/// <summary>
		///     Formats message.
		/// </summary>
		/// <remarks>
		///     <para>
		///         Message text is a default string representation. Additionally the following information might be included:
		///     </para>
		///		<para>
		///			m - message itself prefixed with code (if any)
		///		</para>
		///		<para>
		///			nm - message itself prefixed with node id and/or code (if any)
		///		</para>
		///     <para>
		///         l - link if specified;
		///     </para>
		///     <para>
		///         f - file/project reference if specified (for warning or error only);
		///     </para>
		///		<para>
		///         F - file/project reference if specified (for all levels);
		///     </para>
		///     <para>
		///         d - details if specified;
		///     </para>
		///		<para>
		///			a - compactly formatted message with file, line and column number;
		///		</para>
		///		<para>
		///			p - project if specified;
		///		</para>
		/// </remarks>
		/// <param name="format"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			const int ident = 2;
			
			var sb = new StringBuilder(512);

			var prevField = '\0';
			foreach (var field in format)
			{
				switch (field)
				{
					case 'm':
						if (sb.Length > 0) 
							sb.AppendLine();

						if (HasNodeId && prevField == 'n')
							sb.Append(NodeId).Append("> ");

						if (!String.IsNullOrEmpty(Code))			
							sb.Append(Code).Append(": ");

						sb.Append(Message);
						break;

					case 'l':						
						if (_links == null)
							break;

						foreach (var link in _links)
						{
							if (sb.Length > 0)
								sb.AppendLine();
							
							sb.Append(' ', ident).Append(link);
						}
						break;

					case 'F':
					case 'f':
						if (field == 'F' || Level >= TraceMessageLevel.Warning)
						{
							if (!String.IsNullOrEmpty(File))
							{
								if (sb.Length > 0)
									sb.AppendLine();

								sb.Append(' ', ident).Append(File);
								if (HasLine)
									sb.AppendFormat(" Ln: {0}", Line);

								if (HasColumn)
									sb.AppendFormat(" Col: {0}", Column);
							}

							if (!String.IsNullOrEmpty(Project))
							{
								if (sb.Length > 0)
									sb.AppendLine();

								sb.Append(' ', ident).Append(Project);
							}
						}
						break;

					case 'p':
						if (!String.IsNullOrEmpty(Project))
						{
							if (sb.Length > 0)
								sb.AppendLine();

							sb.Append(' ', ident).Append(Project);
						}
						break;

					case 'a':
						if (sb.Length > 0)
							sb.AppendLine();

						if (!String.IsNullOrEmpty(File))
						{
							if (File.Length > 48)							
								sb.Append('\u2026').Append(File.Substring(File.Length - 47));							
							else							
								sb.Append(File);
							
							if (HasLine)
							{
								sb.Append('(').Append(Line);
								if (HasColumn)
									sb.Append(", ").Append(Column).Append("): ");
							}							
						}

						if (!String.IsNullOrEmpty(Code))			
							sb.Append(Code).Append(": ");

						sb.Append(Message);
						break;

					case 'd':
						if (!String.IsNullOrWhiteSpace(Details))
						{
							if (sb.Length > 0)
								sb.AppendLine();

							sb.Append(Details);
						}
						break;
				}

				prevField = field;
			}

			return sb.ToString();
		}

		/// <summary>
		///		Formats message. Equals to <c>ToString(format, null)</c>
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format)
		{
			return ToString(format, null);
		}

		/// <summary>
		///     Formats message with default presentation 'nmlfd'. <see cref="ToString(string,System.IFormatProvider)" />
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToString("nmlfd", null);
		}
	}
}