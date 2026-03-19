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
		[SerializeField] private float _fireRate = 0.1f;		// 射速
		[SerializeField] private Transform _muzzlePoint;		// 枪口位置
		[SerializeField] private float _maxDistance = 100;		// 射程	
		[SerializeField] private LayerMask _hitLayerMask;		// 击中Layer
		[SerializeField] private int _maxAmmo = 30;				// 弹夹容量
		[SerializeField] private float _reloadSpeed = 2f;		// 换弹速度
		private float _lastShotTimeLocal; 	// 客户端本地记录（仅用于输入控制）

		[Header("Server Data")]
		[SerializeField] private float _lastShotTime;			// 上次射击时间（服务器维护）

		[Header("Sync Data")]
		[SyncVar(hook = nameof(OnAmmoChanged))]
		private int _currentAmmoAmount;							// 当前子弹数
		[SyncVar]
		private bool _isReloading = false;						// 当前是否在装弹

		[Header("Fire Data")]
		[SerializeField] private VFXBulletVisual _bulletPrefab;		// 子弹GO
		[SerializeField] private GameObject _vfxMuzzleFlash;		// 枪口火花GO
		[SerializeField] private float _muzzleFlashDuration = 0.05f;// 火花持续时间

        public override void OnStartServer()
        {
            base.OnStartServer();
			_currentAmmoAmount = _maxAmmo;
        }

        private void Start()
        {
            _vfxMuzzleFlash.SetActive(false);
        }

        private void Update()
        {
			if (!isLocalPlayer) return;
			if (Input.GetMouseButton(0) && CanShotLocally())
			{
				// 鼠标左键 射击（向服务器发送射击命令）
				CmdShooting();
				_lastShotTimeLocal = Time.time; // 更新本地时间
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				CmdReload();
			}
        }
		private bool CanShotLocally()
		{
			if (_isReloading || _currentAmmoAmount <= 0) 
				return false;
				
			return (Time.time - _lastShotTimeLocal) >= _fireRate;
		}

		// 服务器执行换弹请求
		[Command]
		private void CmdReload()
		{
			// 服务器验证是否需要 换弹
			if (_isReloading || _currentAmmoAmount >= _maxAmmo)
				return;
			
			ServerStartReload();
		}

		// 服务器验证并执行射击
		[Command]
		private void CmdShooting()
		{
			if (!CanShoot()) return;
			_lastShotTime = Time.time;
			// 弹夹弹药打空
			if (--_currentAmmoAmount <= 0)
			{
				ServerStartReload();
			}

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
		private void ServerStartReload()
		{
			_isReloading = true;
			StartCoroutine(ReloadCoroutine());
		}

		[Server]
		private IEnumerator ReloadCoroutine()
		{
			yield return new WaitForSeconds(_reloadSpeed);
			_currentAmmoAmount = _maxAmmo;
			_isReloading = false;
		}

		[Server]
		private bool CanShoot()
		{
			// 正在填充弹药
			if (_isReloading || _currentAmmoAmount <= 0) return false;
			// 射击间隔
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

		// 玩家向服务器发出换弹请求。所有客户端改变子弹数量，并执行 换弹逻辑/动画
		private void OnAmmoChanged(int oldAmmo, int newAmmo)
		{
			
		}
    }
}