using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samc.SAMI
{
	/// <summary>
	/// smai의 싱크 데이터
	/// </summary>
	public class SmaiSync
	{
		/// <summary>
		/// 시작 위치
		/// </summary>
		public int Start { get; set; }

		/// <summary>
		/// 가지고 있는 내용
		/// </summary>
		public string Content { get; set; }

		public SmaiSync()
		{
			this.Start = -1;
		}
	}
}
