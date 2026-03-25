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

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
        {
			if (Instance == null)
			{
				GameObject obj = new GameObject("LocalPlayerManager");
				Instance = obj.AddComponent<LocalPlayerManager>();
				DontDestroyOnLoad(obj);
			}
        }
		
		public void InvokeOnCharacterSpawned(PlayerCharacter playerCharacter)
		{
			_playerCharacter = playerCharacter;
			OnCharacterSpawned?.Invoke(playerCharacter);
		}

	}
}