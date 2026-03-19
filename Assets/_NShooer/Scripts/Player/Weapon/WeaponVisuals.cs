using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{	
	// 武器效果（射出子弹、火花特效、换弹动作），纯客户端效果
	public class WeaponVisuals : NetworkBehaviour
	{
		[SerializeField] private PlayerCharacterWeapon _weapon;
		[SerializeField] private Animator _anim;
		[Header("Fire Data")]
		[SerializeField] private VFXBulletVisual _bulletPrefab;		// 子弹GO
		[SerializeField] private GameObject _vfxMuzzleFlash;		// 枪口火花GO
		[SerializeField] private float _muzzleFlashDuration = 0.05f;// 火花持续时间

		private const int LAYER_WEIGHT_UPPERBODY = 1;

        void Awake()
        {
            _vfxMuzzleFlash.SetActive(false);
			// 订阅武器事件
			_weapon.onBulletFired += OnBulletFired;
			_weapon.onReloadStarted += OnReloadStarted;
        }
        private void OnDestroy()
        {
			if (_weapon != null)
			{
				_weapon.onBulletFired -= OnBulletFired;
				_weapon.onReloadStarted -= OnReloadStarted;
			}
        }

		// 服务器 → 所有客户端：播放子弹
		[ClientRpc]
		private void RpcSpawnBullet(BulletData bullet)
		{
			// 每个客户端本地创建子弹特效
			SpawnBulletEffect(bullet);
			// 创建枪口火花特效
			StartCoroutine(SpawnFireFlash());
		}

		// 由服务器通过 ClientRpc 调用
		private void OnBulletFired(BulletData bullet)
		{
			if (isServer) RpcSpawnBullet(bullet);
		}

		[Client]
		private void SpawnBulletEffect(BulletData bulletData)
		{
			Vector3 startPoint = bulletData.origin;

			Quaternion rotation = Quaternion.LookRotation(bulletData.direction, Vector3.up);
			VFXBulletVisual bulletVisual = Instantiate(_bulletPrefab, startPoint, rotation);
			bulletVisual.FlyTo(bulletData.direction * bulletData.distance);
		}

		[Client]
		IEnumerator SpawnFireFlash()
		{
			_vfxMuzzleFlash.SetActive(true);
			yield return new WaitForSeconds(_muzzleFlashDuration);
			_vfxMuzzleFlash.SetActive(false);
		}

		// 换弹动画
		[ClientRpc]
		private void OnReloadStarted(float reloadDuration)
		{
			StartCoroutine(OnReloadAnim(reloadDuration));
		}

		private IEnumerator OnReloadAnim(float reloadDuration)
		{
			_anim.SetLayerWeight(LAYER_WEIGHT_UPPERBODY, 1f);
			_anim.SetTrigger("IsReloading");
			yield return new WaitForSeconds(reloadDuration);
			_anim.SetLayerWeight(LAYER_WEIGHT_UPPERBODY, 0f);
		}
    }
}