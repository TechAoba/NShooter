using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerMovement : NetworkBehaviour
	{
		[SerializeField] private Rigidbody _rb;
        [SerializeField] private float _speed = 6f;
        [SerializeField] private float _rotateSpeed = 10f;
        [SerializeField] private Animator _animator;

        Camera camera;
        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            // Move accord to camera
            Vector3 direction = GetMoveDirection();
            transform.Translate(direction * Time.deltaTime * _speed);

            // Set Animator attribute
            _animator.SetFloat("Horizontal", direction.x);
            _animator.SetFloat("Vertical", direction.z);
            // print(direction.magnitude);
            _animator.SetFloat("Speed", direction.magnitude);

            // Get Raycast Position On Plane
            Vector3 raycastPosition = GetRaycastPosition();
            // Rotate Player
            RotatePlayer(raycastPosition);
        }

        private void RotatePlayer(Vector3 raycastPosition)
        {
            // 1. 计算原始方向
            Vector3 lookDirection = raycastPosition - _rb.transform.position;

            // 2. 强制将 Y 轴设为 0，确保方向向量平行于地面
            lookDirection.y = 0f;

            // 3. 如果方向向量长度为 0 (例如鼠标点在玩家正下方)，则不旋转，防止报错或乱转
            if (lookDirection.sqrMagnitude < 0.001f) return;

            // 4. 执行旋转（使用 Slerp 进行平滑插值）
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            _rb.transform.rotation = Quaternion.Slerp(_rb.transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
        }

        private Vector3 GetRaycastPosition()
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }
            return Vector3.zero;
        }

        private Vector3 GetMoveDirection()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 forward = camera.transform.forward * vertical;
            Vector3 right = camera.transform.right * horizontal;

            Vector3 direction = (new Vector3(forward.x + right.x, 0f, forward.z + right.z)).normalized;
            return direction;
        }
    }
}