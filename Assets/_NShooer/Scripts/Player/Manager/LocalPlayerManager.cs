using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NShooter 
{
	public class LocalPlayerManager : MonoBehaviour
	{
		public static LocalPlayerManager Instance { get; private set; }
		public event Action<PlayerCharacter> OnCharacterSpawned;

		private PlayerCharacter _playerCharacter;
		public PlayerCharacter PlayerCharacter => _playerCharacter;

		private void Start()
        {
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
        }
		
		public void InvokeOnCharacterSpawned(PlayerCharacter playerCharacter)
		{
			_playerCharacter = playerCharacter;
			OnCharacterSpawned?.Invoke(playerCharacter);
		}

	}
}