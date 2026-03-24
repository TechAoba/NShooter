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

		public event Action<PlayerCharacter> OnCharacterDespawned;
		public event Action<PlayerCharacter> OnCharacterSpawned;

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
		
		// 注册玩家（服务器调用）
		public void RegisterPlayerCharacter(NetworkConnectionToClient conn, GameObject player)
		{
			_playerCharacters.Add(conn.connectionId, player);
			Debug.Log($"[PlayerManager] 玩家 {conn.connectionId} 已注册");
		}

		// 注销玩家（服务器调用）
		public void RemovePlayerCharacter(NetworkConnectionToClient conn)
		{
			_playerCharacters.Remove(conn.connectionId);
			Debug.Log($"[PlayerManager] 玩家 {conn.connectionId} 已注销");
		}

		public void InvokeOnCharacterDespawned(PlayerCharacter playerCharacter)
		{
			OnCharacterDespawned?.Invoke(playerCharacter);
		}
	}
}