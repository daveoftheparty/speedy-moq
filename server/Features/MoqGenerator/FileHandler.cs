using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Features.Interfaces;

namespace Features.MoqGenerator
{
	public class FileHandler : IFileHandler
	{
		private readonly Dictionary<string, string> _md5ByPath = new();
		
		public async Task<bool> HasFileChangedAsync(string filePath, string text)
		{
			var hash = await GetMd5Async(text);

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

		private async Task<string> GetMd5Async(string text)
		{
			using (var md5 = MD5.Create())
			{
				using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
				{
					var hash = await md5.ComputeHashAsync(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
				}
			}
		}
	}
}