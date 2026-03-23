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

		public event Action OnHealthChangedAction;

		[Server]
		public void ReduceHealth(int amount)
		{
			if (_isDead) return;	// 已死亡，不再扣血
			_health -= amount;
			if (_health <= 0)
			{
				_isDead = true;
				RpcOnDeath();
			}
		}

		[ClientRpc]
		private void RpcOnDeath()
		{
			print("current health: " + _health);
		}

		private void OnHealthChanged(int oldHealth, int newHealth)
		{
			OnHealthChangedAction?.Invoke();
		}
	}
}