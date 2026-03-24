using System.Collections.Generic;
using Mirror;
using Mirror.Examples.AssignAuthority;
using UnityEngine;
using System;

namespace NShooter 
{
	public class PlayerManager : NetworkBehaviour
	{
		public static PlayerManager Instance { get; private set; }
		private Dictionary<int, GameObject> _playerCharacters = new();

		public bool TryGetPlayerCharacter(int id, out GameObject playerCharacter)
			=> _playerCharacters.TryGetValue(id, out playerCharacter);

		public static event Action OnCharacterQuitEvent;
		public static event Action OnCharacterEnterEvent;

        public override void OnStartServer()
        {
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
        }
		// ========================
        // 服务器 API（仅服务器调用）
        // ========================
		
		[Server]
		public void RegisterPlayerCharacter(NetworkConnectionToClient conn, GameObject player)
		{
			_playerCharacters.Add(conn.connectionId, player);
			Debug.Log($"[PlayerManager] 玩家 {conn.connectionId} 已注册");
			InvokeOnCharacterEnter();
		}

		[Server]
		public void RemovePlayerCharacter(NetworkConnectionToClient conn)
		{
			_playerCharacters.Remove(conn.connectionId);
			Debug.Log($"[PlayerManager] 玩家 {conn.connectionId} 已注销");
			InvokeOnCharacterQuit();
		}

        // ========================
        // ClientRpc：广播给所有客户端
        // ========================
		[ClientRpc]
		public void InvokeOnCharacterQuit()
		{
			OnCharacterQuitEvent?.Invoke();
		}
		[ClientRpc]
		public void InvokeOnCharacterEnter()
		{
			OnCharacterEnterEvent?.Invoke();
		}
	}
}