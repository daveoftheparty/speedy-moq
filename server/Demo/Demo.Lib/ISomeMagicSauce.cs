using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Lib
{
	public interface ISomeMagicSauce
	{
		public string SomeSaucyText { get; }

		void Boom(DateTime? timeForBoom);
		Task<IEnumerable<double>> GetStuffAsync(IReadOnlyDictionary<int, IEnumerable<double>> transformData);
	}
}