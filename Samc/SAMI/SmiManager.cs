using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace Samc.SAMI
{

	public class SmiManager
	{
		/// <summary>
		/// 싱크값을 어떻게 추가할지 여부
		/// </summary>
		public enum TypeSyncAdd_Time
		{
			None = 0,
			/// <summary>
			/// 재생 시간 기준
			/// </summary>
			PlayTime,
			/// <summary>
			/// 0.5초 앞으로
			/// </summary>
			Minus500ms,
			/// <summary>
			/// 0.5초 뒤로
			/// </summary>
			Plus500ms,
		}

		public enum TypeSyncAdd_Taget
		{
			None = 0,
			/// <summary>
			/// 선택된 인덱스의 뒤로 모든 아이템
			/// </summary>
			NowBack,
			/// <summary>
			/// 선택한 아이템만
			/// </summary>
			One,
			/// <summary>
			/// 전체 아이템
			/// </summary>
			All,
		}

		private SmaiTag m_TagCut = new SmaiTag();

		/// <summary>
		/// 분석된 자막 데이터
		/// </summary>
		public SmaiData SMI { get; set; }

		/// <summary>
		/// 지정한 내용을 분석합니다.
		/// </summary>
		/// <param name="sData">분석할 내용</param>
		public void Analysis(string sData)
		{
			this.SMI = new SmaiData();


			SmaiTag_Row tagSami = this.m_TagCut.Tags_Cut(sData, "SAMI")[0];
			//해드 자르게
			this.SMI.Head = this.m_TagCut.Tags_Cut(tagSami.Value, "HEAD")[0];

			//바디 자르기
			SmaiTag_Row tagBody = this.m_TagCut.Tags_Cut(tagSami.Value, "BODY")[0];
			//자막 정보
			SmaiSync[] tagSync = this.m_TagCut.Tags_Cut_Sync(tagBody.Value);
			if (null != tagSync)
			{
				//리스트에 데이터를 바인딩한다!
				this.SMI.Data = tagSync.ToList();
			}
		}


		/// <summary>
		/// nStartIndex의 부터 뒷쪽 모든 sami데이터를 nAddSync만큼 더해줍니다.
		/// </summary>
		/// <param name="nStartIndex">시작할 인덱스</param>
		/// <param name="nAddSync">더해줄 싱크값(마이너스 값을 넣으면 빼줌)</param>
		public void Sync_Add(TypeSyncAdd_Taget typeTaget, int nStartIndex, int nAddSync)
		{
			int nStart = -1;
			int nTotla = -1;

			//타겟의 범위를 설정 한다.
			switch (typeTaget)
			{
				case TypeSyncAdd_Taget.One:
					nStart = nStartIndex;
					nTotla = nStartIndex + 1;
					break;

				case TypeSyncAdd_Taget.All:
					nStart = 0;
					nTotla = this.SMI.Data.Count;
					break;
				
				case TypeSyncAdd_Taget.NowBack:
				default://기본은 뒤로
					nStart = nStartIndex;
					nTotla = this.SMI.Data.Count;
					break;
			}
			

			//지정한 인덱스부터 끝까지 돌린다.
			for (int i = nStartIndex; i < this.SMI.Data.Count; ++i)
			{
				//아이템을 선택하고
				SmaiSync item = this.SMI.Data[i];

				//값을 추가한다.
				item.Start += nAddSync;
			}
		}

		/// <summary>
		/// 지정한 포지션에 해당하는 캡션을 가지고 온다.
		/// </summary>
		/// <param name="nNowPosition"></param>
		/// <returns></returns>
		public string GetCaption(double nNowPosition, out int nOutIndex )
		{
			string sReturn = "";
			nOutIndex = -1;

			int nNow = Convert.ToInt32(nNowPosition);

			for (int i = 0; i < this.SMI.Data.Count; ++i)
			{
				if ((0 == i) && (nNow < this.SMI.Data[i].Start))
				{
					sReturn = "";
					break;
				}
				else if ((i + 1 >= this.SMI.Data.Count)
					|| ((this.SMI.Data[i].Start <= nNow)
						&& (nNow < this.SMI.Data[i + 1].Start)))
				{//다음 아이템이 없다. || 지금 값과 다음값 사이에 있다.
				 //현재 아이템을 리턴해준다.
					sReturn = this.SMI.Data[i].Content;
					nOutIndex = i;
					break;
				}
			}//end for

			return sReturn;
		}

		/// <summary>
		/// 지금 가지고 있는 데이터를 파일에 기록할 데이터로 변환합니다.
		/// </summary>
		/// <returns></returns>
		public string GetFileData()
		{
			StringBuilder sbReturn = new StringBuilder();

			//sami 시작 태그
			sbReturn.Append("<sami>\r\n");

			//head 내용 입력
			sbReturn.Append(this.SMI.Head.Full + "\r\n");

			//바디 시작
			sbReturn.Append("<body>\r\n");

			//자막 내용
			foreach (SmaiSync item in this.SMI.Data)
			{
				sbReturn.Append(this.GetSync(item));
			}


			//바디 끝
			sbReturn.Append("</body>\r\n");

			//sami 끝 태그
			sbReturn.Append("</sami>");
			return sbReturn.ToString();
		}

		private string GetSync(SmaiSync sync)
		{
			return string.Format("<SYNC Start={0}>{1}", sync.Start, sync.Content);
		}

	}
}
