using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samc.SAMI
{
	/// <summary>
	/// 읽어들인 sami파일의 분석정보
	/// </summary>
	public class SmaiData
	{
		/// <summary>
		/// 타이틀
		/// </summary>
		//public string Title { get; set; }
		/// <summary>
		/// 스타일
		/// </summary>
		//public string Style { get; set; }


		/// <summary>
		/// 해더
		/// </summary>
		public SmaiTag_Row Head { get; set; }

		/// <summary>
		/// 자막 데이터
		/// </summary>
		public List<SmaiSync> Data { get; set; }
	}

	
}
