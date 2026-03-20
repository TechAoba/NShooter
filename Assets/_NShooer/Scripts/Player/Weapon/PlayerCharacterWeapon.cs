using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NShooter 
{
	public class PlayerCharacterWeapon : NetworkBehaviour
	{
		public WeaponVisuals _weaponVisuals;

        [Header("Fire Config")]
		[SerializeField] private float _fireRate = 0.1f;		// 射速
		[SerializeField] private int _maxAmmo = 30;				// 弹夹容量
		public float _reloadDuration = 2f;						// 换弹速度
		[SerializeField] private float _maxDistance = 100;		// 射程	
		[SerializeField] private Transform _muzzlePoint;		// 枪口位置
		[SerializeField] private LayerMask _hitLayerMask;		// 击中Layer
		private float _lastShotTimeLocal; 	// 客户端本地记录（仅用于输入控制）

		[Header("Server Data")]
		[SerializeField] private float _lastShotTime;			// 上次射击时间（服务器维护）
		[SerializeField] private float _shotTimeTolerance = 0.5f;// 射击间隔波动允许（服务器维护）

		[Header("Sync Data")]
		[SyncVar(hook = nameof(OnAmmoChanged))]
		private int _currentAmmoAmount;							// 当前子弹数
		[SyncVar]
		private bool _isReloading = false;						// 当前是否在装弹

		public Action<int, int> onAmmoChanged;					// 供UI监听
		public Action<BulletData> onBulletFired;				// 通知视觉系统开火
		public Action<float> onReloadStarted;					// 通知开始换弹

		private WaitForSeconds _cachedReloadWait;

		public int MaxAmmo => _maxAmmo;
		public int CurrentAmmoAmount => _currentAmmoAmount;

        public override void OnStartServer()
        {
            base.OnStartServer();
			_currentAmmoAmount = _maxAmmo;
			_cachedReloadWait = new WaitForSeconds(_reloadDuration);
        }

        private void Update()
        {
			if (!isLocalPlayer) return;
			if (Input.GetMouseButton(0) && CanShotLocally())
			{
                PredictedFire(); // 本地立即播放
				CmdShooting();	 // 鼠标左键 射击（向服务器发送射击命令）
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
			print("shot time: " + Time.time);
			_lastShotTime = Time.time;
			--_currentAmmoAmount;

			BulletData bullet = calcBulletRayCast();

            onBulletFired?.Invoke(bullet);

			// 弹夹弹药打空，执行换弹
			if (_currentAmmoAmount <= 0)
			{
				ServerStartReload();
			}
		}

        [ClientCallback]
        private void PredictedFire()
        {
            if (!isLocalPlayer || _muzzlePoint == null) return;

            var bullet = calcBulletRayCast();

            // 直接告诉 WeaponVisuals 播放（绕过网络）
            _weaponVisuals?.PlayLocalBullet(bullet); // 你需要加这个方法
        }

        private BulletData calcBulletRayCast()
		{
            // 服务器执行射线检测
            Vector3 origin = _muzzlePoint.position;
            Vector3 direction = _muzzlePoint.forward.normalized;
            float actualDistance = _maxDistance;

            // 若命中物体
            if (Physics.Raycast(origin, direction,
                out RaycastHit hit, _maxDistance, _hitLayerMask))
            {
                actualDistance = hit.distance;
                print($"hit: {hit.collider.name}");
            }

            // 构造子弹数据
            BulletData bullet = new BulletData
            {
                origin = origin,
                direction = direction,
                distance = actualDistance,
            };
			return bullet;
        }

        [Server]
		private void ServerStartReload()
		{
			_isReloading = true;
			onReloadStarted?.Invoke(_reloadDuration);
			StartCoroutine(ReloadCoroutine());
		}

		[Server]
		private IEnumerator ReloadCoroutine()
		{
			yield return _cachedReloadWait;
			_currentAmmoAmount = _maxAmmo;
			_isReloading = false;
		}

		// 服务端判断是否可射击。“宽容验证”避免子弹射击失败
		[Server]
		private bool CanShoot()
		{
			// 正在填充弹药
			if (_isReloading || _currentAmmoAmount <= 0) return false;

			float timeSinceLast = Time.time - _lastShotTime;
			if (timeSinceLast >= (_fireRate - _shotTimeTolerance))
				return true;
			return false;
		}

		// 所有客户端改变子弹数量
		private void OnAmmoChanged(int oldAmmo, int newAmmo)
		{
			onAmmoChanged?.Invoke(oldAmmo, newAmmo);
		}
    }
}