using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NShooter 
{
	public class UIPlayerWeapon : MonoBehaviour
	{
		[SerializeField] private TMP_Text _textBullet;
		[SerializeField] private TMP_Text _textMaxAmmo;
		[SerializeField] private float _canvasBulletTargetAlpha = 0.15f;
		[SerializeField] private CanvasGroup _canvasGroupBullet;
		[SerializeField] private CanvasGroup _canvasGroupReloading;

		private PlayerCharacterWeapon _weapon;

		public void SetCurrentAmmo(int currentAmmo)
		{
			_textBullet.SetText("{0}", currentAmmo);
		}
		public void SetMaxAmmo(int maxAmmo)
		{
			_textMaxAmmo.SetText("{0}", maxAmmo);
		}

		public void SetIsReloding(bool isReloading)
		{
			_canvasGroupBullet.alpha = isReloading ? _canvasBulletTargetAlpha : 1f;
			_canvasGroupReloading.alpha = isReloading ? 1.0f : 0f;
		}
	}
}