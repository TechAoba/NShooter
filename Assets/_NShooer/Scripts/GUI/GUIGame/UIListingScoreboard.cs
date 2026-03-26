using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NShooter 
{
	public class UIListingScoreboard : MonoBehaviour
	{
		[SerializeField] private TMP_Text _textPlayerNumber;
		[SerializeField] private TMP_Text _textPlayerName;
		[SerializeField] private TMP_Text _textPlayerKD;

		public void UpdateUI(int order, int kill, int death)
		{
			_textPlayerNumber.SetText("{0}", order);
			_textPlayerKD.SetText("{0} / {1}", kill, death);
		}

		public void UpdateNickname(string nickname)
		{
			_textPlayerName.SetText(nickname);
		}

		public void Clear()
		{
			_textPlayerNumber.SetText("");
			_textPlayerKD.SetText("");
			_textPlayerName.SetText("");
		}
	}
}