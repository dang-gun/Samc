using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samc.SAMI
{
	public class SmaiTag_Row
	{
		/// <summary>
		/// 지정된 테그
		/// </summary>
		public string Tag { get; set; }

		/// <summary>
		/// 전체 데이터
		/// </summary>
		public string Full { get; set; }

		/// <summary>
		/// 해드
		/// </summary>
		public string Head { get; set; }

		/// <summary>
		/// 태그를 제외한 데이터
		/// </summary>
		public string Value { get; set; }

	}
}
