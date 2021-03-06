﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnFake.Core
{
	public static class FormatExtension
	{
		public static IEnumerable<FileItem> AsFormattable(this IEnumerable<FileItem> files)
		{
			return files is FileSet || files is ICollection<FileItem> ? files : files.ToList();
		}

		public static IEnumerable<FolderItem> AsFormattable(this IEnumerable<FolderItem> folders)
		{
			return folders is FolderSet || folders is ICollection<FolderItem> ? folders : folders.ToList();
		}

		public static string ToFormattedString(this object obj)
		{
			return ToFormattedString((dynamic) obj);
		}

		private static string ToFormattedString(FileSet files)
		{
			return files.ToString();
		}

		private static string ToFormattedString(ICollection<FileItem> files)
		{
			var sb = new StringBuilder(128);
			foreach (var file in files)
			{
				if (sb.Length > 64)
				{
					sb.Append("...");
					break;
				}

				if (sb.Length > 0)
				{
					sb.Append(", ");
				}

				sb.Append('\'').Append(file).Append('\'');
			}

			return sb.ToString();
		}

		private static string ToFormattedString(IEnumerable<FileItem> files)
		{
			return "...";
		}

		private static string ToFormattedString(FolderSet folders)
		{
			return folders.ToString();
		}

		private static string ToFormattedString(ICollection<FolderItem> folders)
		{
			var sb = new StringBuilder(128);
			foreach (var folder in folders)
			{
				if (sb.Length > 64)
				{
					sb.Append("...");
					break;
				}

				if (sb.Length > 0)
				{
					sb.Append(", ");
				}

				sb.Append('\'').Append(folder).Append('\'');
			}

			return sb.ToString();
		}

		private static string ToFormattedString(IEnumerable<FolderItem> folders)
		{
			return "...";
		}		
	}
}