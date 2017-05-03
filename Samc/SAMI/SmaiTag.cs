using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Samc.SAMI
{
	public class SmaiTag
	{
		public SmaiTag_Row[] Tags_Cut(SmaiTag_Row tagOri, string sTag)
		{
			return this.Tags_Cut(tagOri.Full, sTag);
		}


		/// <summary>
		/// 태그를 검색하여 dataTag객체를 리턴합니다.
		/// </summary>
		/// <param name="sOri">검색할 원본 데이터</param>
		/// <param name="sTag">검색할 태그</param>
		/// <returns></returns>
		public SmaiTag_Row[] Tags_Cut(string sOri, string sTag)
		{

			List<SmaiTag_Row> listReturn = new List<SmaiTag_Row>();
			

			//태그 추출
			 string[] sFull = Regex.Matches(sOri
						, "<" + sTag + "(.*?)>(.*?)</" + sTag + ">"
						, RegexOptions.IgnoreCase | RegexOptions.Singleline)
					.Cast<Match>().Select(m => m.Value).ToArray();

			//태그 갯수 만큼 아이템 생성
			foreach (string sItem in sFull)
			{
				SmaiTag_Row tagTemp = new SmaiTag_Row();
				//태그
				tagTemp.Tag = sTag;
				//전체 데이터
				tagTemp.Full = sItem;

				//태그 해드 추출
				tagTemp.Head = Regex.Matches(tagTemp.Full
												, "<" + sTag + "(.*?)>"
												, RegexOptions.IgnoreCase 
													| RegexOptions.Singleline)[0].Value;
				//태그 헤드 제거
				tagTemp.Value = Regex.Replace(tagTemp.Full
												, "<" + sTag + "(.*?)>"
												, string.Empty
												, RegexOptions.IgnoreCase);
				//테그 풋 제거
				tagTemp.Value = Regex.Replace(tagTemp.Value
												, "</" + sTag + ">"
												, string.Empty
												, RegexOptions.IgnoreCase);

				//리스트에 추가 한다.
				listReturn.Add(tagTemp);
			}
			
			//태그 해드 추출
			//var bbb = Regex.Matches(sOri
			//			, "<" + sTag + ">"
			//			, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			//어트리뷰트 추출
			//var ccc = Regex.Matches(sOri
			//			, @"[^>] *= ""(.*?)"
			//			, RegexOptions.IgnoreCase);

			//어트리 뷰트만 선택
			//Regex.Matches(tagTemp.Head
						//, @"(\S+)=[""']?((?:.(?![""']?\s + (?:\S +)=|[> ""']))+.)[""']?"
							//	 , RegexOptions.IgnoreCase | RegexOptions.Singleline)
			//정규식  c# regular expression multiple lines



			return listReturn.ToArray();
		}


		/// <summary>
		/// "SYNC" 태그만 있는 데이터를 잘라줍니다.
		/// </summary>
		/// <param name="tagOri"></param>
		/// <returns></returns>
		public SmaiSync[] Tags_Cut_Sync(string sOri)
		{
			//리턴할 리스트
			List<SmaiSync> listReturn = new List<SmaiSync>();

			//태그
			string sSync = "sync";

			//태그를 기준으로 자르고
			string[] sCutData = Regex.Split(sOri, "<" + sSync, RegexOptions.IgnoreCase);

			//한개씩 처리
			foreach (string sItem in sCutData)
			{
				SmaiSync dataTemp = new SmaiSync();

				//<sami>의 해드를 분리한다.
				string[] sData = sItem.Split(">".ToCharArray(), 2);


				//해드 처리
				string sHead = sData[0];

				//분리한 해드에서 어트리뷰트를 추출한다.
				//어트리뷰트 추출
				string[] sAttribute = Regex.Matches(sHead
						, @"(\S+)=[""']?((?:.(?![""']?\s + (?:\S +)=|[> ""']))+.)[""']?"
								 , RegexOptions.IgnoreCase | RegexOptions.Singleline)
					.Cast<Match>().Select(m => m.Value).ToArray();

				//어트리뷰트 만큰 생성
				foreach (string sAttr in sAttribute)
				{
					//어트리 뷰트 추출
					string[] sCut = sAttr.Split("=".ToCharArray(), 2);
					if (2 == sCut.Length)
					{//내용이 있다.
						if ("start" == sCut[0].ToLower())
						{//start 어트리뷰트가 있다.
							//변환해서 넣는다.
							dataTemp.Start = Convert.ToInt32(this.CutQuotes(sCut[1]));
						}	
					}

					//컨텐츠 추출
					dataTemp.Content = sData[1];
				}

				if (0 <= dataTemp.Start)
				{//스타트에 값이 있다.
					//값이 있을때만 데이터를 넣는다.
					listReturn.Add(dataTemp);
				}
			}


			return listReturn.ToArray();
		}


		/// <summary>
		/// 맨앞쪽과 맨뒷쪽 따옴표를 찾아 지운다.
		/// </summary>
		/// <param name="sData"></param>
		/// <returns></returns>
		public string CutQuotes(string sData)
		{
			string sReturn = sData;
			//공백 제거
			sReturn = sReturn.Trim();

			//앞쪽 찾기
			if (('\"' == sReturn[0])
				|| ('\'' == sReturn[0]))
			{//앞쪽에 따옴표가 있다.
			 //앞쪽 한칸을 지운다.
				sReturn = sReturn.Substring(1, sReturn.Length - 2);
			}

			//뒷쪽 찾기
			if (('\"' == sReturn[sReturn.Length - 1])
				|| ('\'' == sReturn[sReturn.Length - 1]))
			{//뒷쪽에 따옴표가 있다.
			 //뒷쪽 한칸을 지운다.
				sReturn = sReturn.Substring(0, sReturn.Length - 2);
			}

			return sReturn;
		}
	}

	
	
}
