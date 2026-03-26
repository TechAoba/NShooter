using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NShooter 
{
	public class UIListingPlayerInfo : MonoBehaviour
	{
		[SerializeField] Image _image;
		[SerializeField] TMP_Text _textNametag;
		Transform _playerTarget;

		public Transform PlayerTarget => _playerTarget;

		public void SetTarget(Transform target)
		{
			_playerTarget = target;
		}
		public void SetNametag(string nickname)
		{
			_textNametag.SetText(nickname);
		}

		public void SetColor(Color color)
		{
			_image.color = color;
		}
	}
}