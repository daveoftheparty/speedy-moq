using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Features.Interfaces;

namespace Features.MoqGenerator
{
	public class FileHandler : IFileHandler
	{
		private readonly Dictionary<string, string> _md5ByPath = new();
		public bool HasFileChanged(string filePath, string text)
		{
			var hash = GetMd5(text);

			if(!_md5ByPath.TryGetValue(filePath, out var md5))
			{
				_md5ByPath.Add(filePath, hash);
				return true;
			}
			
			if(md5 != hash)
			{
				_md5ByPath[filePath] = hash;
				return true;
			}

			return false;
		}

		private string GetMd5(string text)
		{
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
				return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
			}
		}
	}
}