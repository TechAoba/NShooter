using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	// 必须标记为 [System.Serializable] 才能被 Mirror 序列化
	[System.Serializable]
	public struct BulletData
	{
		public Vector3 origin;		// 起点
		public Vector3 direction;	// 飞行方向
		public float distance;		// 实际飞行距离
	}
}