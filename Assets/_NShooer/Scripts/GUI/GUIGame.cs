using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace NShooter 
{
	public class GUIGame : MonoBehaviour
	{
		[SerializeField] private Button _buttonRespawn;
		[SerializeField] private UIPlayerWeapon _uiPlayerWeapon;
		public static GUIGame Instance { get; private set; }

		private PlayerCharacterWeapon _weapon;
		private bool _addedToGame;

		private void Start()
		{
			// 初始化单例
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
			// 初始隐藏GUIGame
			gameObject.SetActive(false);
			// 初始玩家不在房间中
			_addedToGame = false;
			_buttonRespawn?.onClick.AddListener(OnButtonRespawn);

			LocalPlayerManager.Instance.OnCharacterSpawned += OnLocalPlayerCharacterSpawned;
		}

		private void OnLocalPlayerCharacterSpawned(PlayerCharacter playerCharacter)
		{
			if (playerCharacter.TryGetComponent(out PlayerCharacterWeapon weapon))
			{
				_weapon = weapon;
				_weapon.onReloadStarted += OnReload;
				_weapon.onAmmoChanged += OnAmmoChanged;
				_uiPlayerWeapon.SetMaxAmmo(_weapon.MaxAmmo);
				_uiPlayerWeapon.SetCurrentAmmo(_weapon.CurrentAmmoAmount);
			}
		}

		private void OnReload(float duration)
		{
			StartCoroutine(ChangeWeaponUI2Reload(duration));
		}

		IEnumerator ChangeWeaponUI2Reload(float duration)
		{
			_uiPlayerWeapon.SetIsReloding(true);
			yield return new WaitForSeconds(duration);
			_uiPlayerWeapon.SetIsReloding(false);
		}

		private void OnAmmoChanged(int oldAmmo, int newAmmo)
		{
			_uiPlayerWeapon.SetCurrentAmmo(_weapon.CurrentAmmoAmount);
		}

		// 玩家死亡，等待重生 => 仅显示重生按钮
		public void OnWaitForRespawn()
		{
			_buttonRespawn?.gameObject.SetActive(true);
			_uiPlayerWeapon?.gameObject.SetActive(false);
		}
		// 玩家退出房间 => 隐藏整个GUIGame
		public void OnQuitGame()
		{
			gameObject.SetActive(false);
		}
		// 玩家进入房间 => 仅显示重生按钮
		public void OnEnterGame()
		{
			gameObject.SetActive(true);
			_buttonRespawn?.gameObject.SetActive(true);
			_uiPlayerWeapon?.gameObject.SetActive(false);
		}

		// 点击重生按钮
		private void OnButtonRespawn()
		{
			// 重生按钮消失，武器面板开启
			_buttonRespawn?.gameObject.SetActive(false);
			_uiPlayerWeapon?.gameObject.SetActive(true);
			
			// 玩家已经加入过游戏，仅重生
			if (_addedToGame)
			{
				LocalPlayerManager.Instance.PlayerCharacter.RequestRespawn();
				if (_weapon != null) 
					_weapon._currentAmmoAmount = _weapon.MaxAmmo;
			}
			else
			{
				_addedToGame = true;
				if (!NetworkClient.active)
				{
					Debug.Log("Not connected to server");
				}

				// Mirror 创建玩家
				NetworkClient.AddPlayer();
			}
		}
	}
}