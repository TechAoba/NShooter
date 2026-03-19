using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacterWeapon : NetworkBehaviour
	{
		[Header("Fire Config")]
		[SerializeField] private float _fireRate = 0.1f;
		[SerializeField] private Transform _muzzlePoint;	// 枪口位置
		[SerializeField] private float _maxDistance = 100;	// 射程	
		[SerializeField] private LayerMask _hitLayerMask;	

		[Header("Server Data")]
		[SerializeField] private float _lastShotTime;	// 上次射击时间（服务器维护）

		[Header("Fire Data")]
		[SerializeField] private VFXBulletVisual _bulletPrefab;
		[SerializeField] private GameObject _vfxMuzzleFlash;
		[SerializeField] private float _muzzleFlashDuration = 0.05f;

        private void Start()
        {
            _vfxMuzzleFlash.SetActive(false);
        }

        private void FixedUpdate()
        {
			if (!isLocalPlayer) return;
			if (Input.GetKey(KeyCode.Mouse0))
			{
				// 鼠标左键 为射击
				CmdShooting();
			}
			
        }

		// 服务器验证并执行射击
		[Command]
		private void CmdShooting()
		{
			if (!CanShoot()) return;
			_lastShotTime = Time.time;

			// 服务器执行射线检测
			Vector3 origin = _muzzlePoint.position;
			Vector3 direction = _muzzlePoint.forward.normalized;
			float actualDistance = _maxDistance;

			// 若命中物体
			if (Physics.Raycast(_muzzlePoint.position, _muzzlePoint.forward, 
				out RaycastHit hitInfo, _maxDistance, _hitLayerMask))
			{
				actualDistance = hitInfo.distance;
				print($"hit: {hitInfo.collider.name}");
			}

			// 构造子弹数据
			BulletData bullet = new BulletData
			{
				origin = origin,
				direction = direction,
				distance = actualDistance,
			};
			// 子弹广播给所有客户端播放子弹动画
			RpcSpawnBullet(bullet);
		}

		[Server]
		private bool CanShoot()
		{
			float currentTime = Time.time;
			return (currentTime - _lastShotTime) >= _fireRate;
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
    }
}