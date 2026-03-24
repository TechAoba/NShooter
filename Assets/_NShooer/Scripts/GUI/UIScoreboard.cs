using Mirror;
using Mirror.Examples.AssignAuthority;
using Mirror.Examples.MultipleAdditiveScenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NShooter 
{
	public class UIScoreboard : MonoBehaviour
	{
		[SerializeField] private Transform _listingslot;
		[SerializeField] private UIListingScoreboard _listingPrefab;
		[SerializeField] private UIListingScoreboard[] _listings;

		private const int LISTING_SIZE = 8;
		private List<PlayerSession> _orderedPlayerSessions = new(LISTING_SIZE);

		// 有玩家 进入房间 / 退出房间 / 被击杀 事件时更新记分板
		void Start()
		{
			PlayerManager.OnCharacterEnterEvent += UpdateScoreboard;
			PlayerManager.OnCharacterQuitEvent += UpdateScoreboard;
			PlayerSession.OnKillChangedEvent += UpdateScoreboard;
			PlayerSession.OnDeathChangedEvent += UpdateScoreboard;
		}

        public void UpdateScoreboard()
		{
			print("update score");
			_orderedPlayerSessions.Clear();
			var sessions = FindObjectsOfType<PlayerSession>()
				.OrderByDescending(s => s._kill).ToArray();
			for (int i = 0; i < _listings.Length; ++i)
			{
				UIListingScoreboard listing = _listings[i];
				if (i < sessions.Length)
				{
					PlayerSession session = sessions[i];
					listing.UpdateNickname(session._nickname);
					listing.UpdateUI(i + 1, session._kill, session._death);
				}
				else
				{
					listing.Clear();
				}
			}
		}

#if UNITY_EDITOR
		[ContextMenu("CacheListings")]
		private void CacheListings()
		{
			DestroyListings();
			_listings = new UIListingScoreboard[LISTING_SIZE];
			for (int i = 0; i < LISTING_SIZE; ++i)
			{
				UIListingScoreboard listing = (UIListingScoreboard)UnityEditor.PrefabUtility.InstantiatePrefab(_listingPrefab, _listingslot);
				_listings[i] = listing;
			}
		}

		[ContextMenu("DestoryListings")]
		private void DestroyListings()
		{
			for (int i = 0; i < _listings.Length; ++i)
			{
				UIListingScoreboard listing = _listings[i];

				if (listing == null) continue;

				DestroyImmediate(listing);
			}

			_listings = Array.Empty<UIListingScoreboard>();
		}
#endif

	}
}