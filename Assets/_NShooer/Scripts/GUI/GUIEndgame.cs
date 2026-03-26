using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace NShooter 
{
	public class GUIEndgame : MonoBehaviour
	{
		[SerializeField] float _endgameFadeInDuration = 1f;
		[SerializeField] float _scoreboardFadeInDuration = 1f;
		[SerializeField] CanvasGroup _canvasGroupEndGame;
		[SerializeField] UIScoreboard _uiScoreboard;
		[SerializeField] CanvasGroup _canvasGroupScoreboard;
		[SerializeField] Ease _easeFadeInEndgame = Ease.OutQuad;
		[SerializeField] Ease _easeFadeInScoreboard = Ease.OutQuad;

        void Start()
        {
            // GamemodeManager gamemodeManager = FindObjectOfType<GamemodeManager>();
			GamemodeManager.OnGameEndEvent += OnGameEnd;
        }

		[ContextMenu("OnGameEnd")]
		void OnGameEnd()
		{
			_uiScoreboard.UpdateScoreboard();
			_canvasGroupEndGame.DOFade(1f, _endgameFadeInDuration)
				.SetEase(_easeFadeInEndgame)
				.OnComplete(() =>
				{
					ScoreboardFade();
				});
		}

		void ScoreboardFade()
		{
			_canvasGroupScoreboard.DOFade(1f, _scoreboardFadeInDuration).SetEase(_easeFadeInScoreboard);
		}
    }
}