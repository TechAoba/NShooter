using Mirror;
using Mirror.Examples.BilliardsPredicted;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacter : NetworkBehaviour
	{
		[Header("Visibility Components")]
		[SerializeField] private SkinnedMeshRenderer[] _renderers;
		[SerializeField] private MeshRenderer _gunReder;
		[SerializeField] private Collider[] _colliders;
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private Canvas _nickNameCanvas;

		private PlayerCharacterHealth _health;
		private PlayerInputHandler _input;

        public override void OnStartClient()
        {
            if (isLocalPlayer)
			{
				LocalPlayerManager.Instance.InvokeOnCharacterSpawned(this);
			}
			_health = GetComponent<PlayerCharacterHealth>();
			_input = GetComponent<PlayerInputHandler>();
        }

		// 本地玩家点击重生按钮时调用
		public void RequestRespawn()
		{
			if (!isLocalPlayer) return;
			CmdRequestRespawn();
		}

		[Command]
		private void CmdRequestRespawn()
		{
			if (!_health.IsDead) return;
			// 服务器重生玩家状态
			_health.ServerRespawn();
			// 通知所有客户端 玩家重生
			RpcOnSpawn();
		}

		[ClientRpc]
		private void RpcOnSpawn()
		{
			// === 所有玩家更新 ===
			// 玩家显示
			SetVisible(true);

			// === 死亡玩家更新 ===
			if (!isLocalPlayer) return;
			// 启用玩家输入
        	if (_input != null) _input.enabled = true;
		}

		[ClientRpc]
		public void RpcOnDespawn()
		{
			// === 所有玩家更新 ===
			// 玩家消失
			SetVisible(false);

			// === 死亡玩家更新 ===
			if (!isLocalPlayer) return;
			// 禁用玩家输入
        	if (_input != null) _input.enabled = false;
			// 玩家UI更新
			GUIGame.Instance.OnWaitForRespawn();
		}

		public void SetVisible(bool visible)
		{
			// 渲染器
			foreach (var rederer in _renderers)
			{
				if (rederer != null) rederer.enabled = visible;
			}
			// 碰撞体
			foreach (var collider in _colliders)
			{
				if (collider != null) collider.enabled = visible;
			}
			// 刚体（防止死亡后物理乱飞）
			if (_rigidbody != null)
			{
				_rigidbody.isKinematic = !visible; // 死亡时设为 Kinematic（不受力）
				_rigidbody.detectCollisions = visible;
			}
			// 武器
			if (_gunReder != null)
			{
				_gunReder.enabled = visible;
			}
			// 玩家昵称
			if (_nickNameCanvas != null) 
				_nickNameCanvas.enabled = visible;
		}
	}
}