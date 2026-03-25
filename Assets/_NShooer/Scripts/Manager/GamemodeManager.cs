using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class GamemodeManager : NetworkBehaviour
	{
		[Header("Game Setting")]
		[SerializeField] private float _gameDuration = 180f;	// 游戏进行时间
		[SerializeField] private float _endGameDuration = 10f;	// 结算时间

		public enum GameState
		{
			Playing,	// 正常游戏
			EndGame,	// 结算阶段（不可操作）
			Finished,	// 游戏完全结束
		}

		[SyncVar(hook = nameof(OnGameStateChanged))]
		private GameState _currentState = GameState.Playing;

		[SyncVar]
		public int _secondsRemaining;

		public bool IsPlaying => _currentState == GameState.Playing;
		public bool IsEndGame => _currentState == GameState.EndGame;

        // 事件：供 UI 或其他系统监听
        public static System.Action<GameState, float> OnGameStateChangedEvent;

        public override void OnStartServer()
        {
            base.OnStartServer();
			
			_currentState = GameState.Playing;
			_secondsRemaining = Mathf.CeilToInt(_gameDuration);
			StartCoroutine(GameTimerCoroutine());
        }

		// 每秒同步一次时间
		private IEnumerator GameTimerCoroutine()
		{
			// === 阶段 1: 游戏进行中 ===
			float timer = _gameDuration;
			while (timer > 0f && _currentState == GameState.Playing)
			{
				yield return null;
				timer -= Time.deltaTime;

				// 只有当整秒变化时才更新 SyncVar
				int currenSeconds = Mathf.CeilToInt(timer);
				if (currenSeconds != _secondsRemaining)
				{
					_secondsRemaining = currenSeconds;
				}
			}

			// 时间到，进入结算阶段
			Debug.Log("游戏进入结算");
			RpcOnEnterEndGame();
			_currentState = GameState.EndGame;
			timer = _endGameDuration;
			_secondsRemaining = Mathf.CeilToInt(_endGameDuration);

			// === 阶段 2: 结算倒计时 ===
			while (timer > 0f && _currentState == GameState.EndGame)
			{
				yield return null;
				timer -= Time.deltaTime;

				int currenSeconds = Mathf.CeilToInt(timer);
				if (currenSeconds != _secondsRemaining)
				{
					_secondsRemaining = currenSeconds;
				}
			}

			// 结算结束，游戏完全结束
			Debug.Log("游戏结束");
			_currentState = GameState.Finished;
			RpcOnGameFinished();
		}

		[ClientRpc]
		private void RpcOnEnterEndGame()
		{
			// 遍历场景中所有 PlayerCharacter（包括自己和其他人）
			var allPlayers = FindObjectsOfType<PlayerCharacter>();
			foreach (var player in allPlayers)
			{
				// 1. 禁用输入（仅本地玩家需要，但统一处理也无妨）
				if (player.TryGetComponent(out PlayerInputHandler input))
				{
					input.enabled = false;
				}
				// 2. 禁用移动 & 动画
				if (player.TryGetComponent(out PlayerMovement move))
				{
					move.enabled = false;
					move._animator.speed = 0;
				}

				// 3. 获取刚体并清空速度 + 设为 Kinematic
				if (player.TryGetComponent(out Rigidbody rb))
				{
					rb.velocity = Vector3.zero;      // 清空线速度
					rb.angularVelocity = Vector3.zero; // 清空角速度
					rb.isKinematic = true;           // 不再受物理力影响
				}
			}

			// 隐藏游戏 UI
			GUIGame.Instance?.OnQuitGame();
		}

		[ClientRpc]
		private void RpcOnGameFinished()
		{
			Debug.Log("[Client] 游戏结束！");
		}

		// SyncVar Hook：客户端收到状态变更时触发
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            OnGameStateChangedEvent?.Invoke(newState, _secondsRemaining);
            // UI 可监听此事件更新显示
        }
	}
}