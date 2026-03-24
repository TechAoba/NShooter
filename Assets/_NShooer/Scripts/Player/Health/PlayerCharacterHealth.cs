using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacterHealth : NetworkBehaviour
	{
		[SyncVar(hook = nameof(OnHealthChanged))]
		public int _health = 100;
		[SyncVar]
		private bool _isDead = false;
		public bool IsDead => _isDead;
		private PlayerSession _session;

		public event Action OnHealthDecrease;

		public override void OnStartClient()
		{
			_session = GetComponent<PlayerSession>();
		}

		[Server]
		public void ReduceHealth(int amount)
		{
			if (_isDead) return;	// 已死亡，不再扣血
			_health -= amount;
			if (_health <= 0)
			{
				_isDead = true;
				// 死亡次数加1
				AddDeath();

				// 执行回调
				PlayerCharacter pc = gameObject.GetComponent<PlayerCharacter>();
				PlayerManager.Instance?.InvokeOnCharacterDespawned(pc);

				pc.RpcOnDespawn();
			}
		}

		[Server]
		private void AddDeath()
		{
			_session.AddDeath();
		}

		[Server]
		public void ServerRespawn()
		{
			_health = 100;
			_isDead = false;
		}

		private void OnHealthChanged(int oldHealth, int newHealth)
		{
			// 玩家收到攻击并且血量大于0（存活状态）
			if (newHealth < oldHealth && newHealth > 0)
			{
				OnHealthDecrease?.Invoke();
			}
		}
	}
}