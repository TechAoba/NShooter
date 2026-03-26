using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace NShooter 
{
	public class GUIPlayerInfo : MonoBehaviour
	{
		const int LISTING_SIZE = 8;
		[SerializeField] UIListingPlayerInfo _listingPrefab;
		[SerializeField] UIListingPlayerInfo[] _listings;
		[SerializeField] Vector3 _listingOffset;

		[Space]
		[SerializeField] Color _colorAlly;
		[SerializeField] Color _colorEnemy;

        void Start()
        {
			PlayerManager.OnCharacterEnterEvent += ReUpdateAllListings;
			PlayerManager.OnCharacterQuitEvent += ReUpdateAllListings;
			PlayerSession.OnKillChangedEvent += ReUpdateAllListings;
        }

        void ReUpdateAllListings()
		{
			DisableAllListings();

			var players = FindObjectsOfType<PlayerCharacter>();
			foreach (PlayerCharacter pc in players)
			{
				UIListingPlayerInfo listing = GetUnusedListing();
				if (listing == null) continue;

				pc.TryGetComponent(out PlayerCharacterVisual pcv);

				Color color = pc.isLocal() ? _colorAlly : _colorEnemy;
				// Color color = _colorAlly;
				listing.SetColor(color);
				listing.SetNametag(GetPlayerNickName(pc));
				listing.SetTarget(pcv._visualTransform);
				listing.gameObject.SetActive(true);
			}
		}

		void LateUpdate()
		{
			for (int i = 0; i < _listings.Length; i++)
			{
				if (_listings[i] == null)
				{
					Debug.LogError($"Listing[{i}] is DESTROYED!");
				}
			}
			UpdateAllListingPosition();
		}

		private void UpdateAllListingPosition()
		{
			Camera camera = Camera.main;

			foreach (UIListingPlayerInfo listing in _listings)
			{
				if (listing == null) continue;
				if (listing.PlayerTarget == null) continue;

				Vector3 screenSpacePosition = camera.WorldToScreenPoint(listing.PlayerTarget.position);
				listing.transform.position = screenSpacePosition + _listingOffset;
			}
		}

		void DisableAllListings()
		{
			foreach (UIListingPlayerInfo listing in _listings)
			{
				if (listing == null) continue;
				listing.gameObject.SetActive(false);
			}
		}

		string GetPlayerNickName(PlayerCharacter pc)
		{
			if (pc.TryGetComponent<PlayerSession>(out PlayerSession session))
			{
				return session._nickname;
			}
			return "";
		}

		UIListingPlayerInfo GetUnusedListing()
		{
			foreach (UIListingPlayerInfo listing in _listings)
			{
				if (listing == null) continue;
				if (!listing.gameObject.activeSelf)
					return listing;
			}
			Debug.LogError($"[{nameof(GUIPlayerInfo)}]: All Listing is used up!");
			return null;
		}

#if UNITY_EDITOR
		[ContextMenu("CacheListings")]
		void CacheListings()
		{
			if (Application.isPlaying)
			{
				Debug.LogWarning("Do not cache listings during play mode!");
				return;
			}
			DestroyAllListings();

			_listings = new UIListingPlayerInfo[LISTING_SIZE];
			for (int i = 0; i < LISTING_SIZE; ++i)
			{
				UIListingPlayerInfo listing = (UIListingPlayerInfo)UnityEditor.PrefabUtility.InstantiatePrefab(_listingPrefab, transform);
				listing.gameObject.SetActive(false);

				_listings[i] = listing;
			}
		}

		[ContextMenu("DestroyAllListings")]
		void DestroyAllListings()
		{
			foreach (UIListingPlayerInfo listing in _listings)
			{
				if (listing != null)
					DestroyImmediate(listing.gameObject);
			}

			_listings = Array.Empty<UIListingPlayerInfo>();
		}
#endif

	}
}