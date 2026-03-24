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
        [SerializeField] private float _maxLastVelocityInterpolationSpeed = 3f;

        // sync move speed
        [SyncVar(hook = nameof(OnSpeedChanged))]
        private float _syncedSpeed;
        private float _lastSentSpeed;
        private Vector3 _lastVelocity;
        private Vector2 _moveInput;
        private Vector2 _mousePos;

        Camera camera;
        private void Start()
        {
            camera = Camera.main;
        }

        public void SetInput(Vector2 moveInput, Vector2 mousePos)
        {
            _moveInput = moveInput;
            _mousePos = mousePos;
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            // Get Raycast Position On Plane
            Vector3 raycastPosition = GetRaycastPosition();
            // Rotate Player
            RotatePlayer(raycastPosition);

            // Move Player
            float horizontal = _moveInput.x;
            float vertical = _moveInput.y;
            Vector3 inputDirection = (new Vector3(horizontal, 0, vertical)).normalized;
            Vector3 direction = GetMoveDirection(horizontal, vertical);
            transform.position += direction * Time.deltaTime * _speed;
            
            _lastVelocity = Vector3.Lerp(_lastVelocity, inputDirection * _speed, Time.deltaTime * _maxLastVelocityInterpolationSpeed);
            _lastVelocity = Vector3.ClampMagnitude(_lastVelocity, 1f);

            // Set Animator attribute
            _animator.SetFloat("Horizontal", _lastVelocity.x);
            _animator.SetFloat("Vertical", _lastVelocity.z);
            // Sync Attribute(Speed) 只有在速度变化大时才同步速度（减少同步压力）
            float currentSpeed = direction.magnitude;
            if (Mathf.Abs(currentSpeed - _lastSentSpeed) > 0.1f)
            {
                CmdSetSpeed(direction.magnitude);
                _lastSentSpeed = currentSpeed;
            }
        }

        private void RotatePlayer(Vector3 raycastPosition)
        {
            // 1. ����ԭʼ����
            Vector3 lookDirection = raycastPosition - _rb.transform.position;

            // 2. ǿ�ƽ� Y ����Ϊ 0��ȷ����������ƽ���ڵ���
            lookDirection.y = 0f;

            // 3. ���������������Ϊ 0 (����������������·�)������ת����ֹ��������ת
            if (lookDirection.sqrMagnitude < 0.001f) return;

            // 4. ִ����ת��ʹ�� Slerp ����ƽ����ֵ��
            _rb.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            // _rb.transform.rotation = Quaternion.Slerp(_rb.transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
        }

        private Vector3 GetRaycastPosition()
        {
            Ray ray = camera.ScreenPointToRay(_mousePos);
            Plane plane = new(Vector3.up, new Vector3(0, 1, 0));
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            // 射线未命中地面，返回当前位置前方一点（避免看向原点）
            return transform.position + transform.forward * 5f;
        }

        private Vector3 GetMoveDirection(float horizontal, float vertical)
        {
            if (horizontal == 0f && vertical == 0f)
                return Vector3.zero;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // 确保水平
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * vertical + right * horizontal).normalized;
        }

        [Command]
        private void CmdSetSpeed(float speed)
        {
            _syncedSpeed = speed; // 服务器更新 SyncVar，自动广播给所有客户端
        }

        // 所有客户端（包括非本地）都会调用此 hook
        private void OnSpeedChanged(float oldSpeed, float newSpeed)
        {
            _animator.SetFloat("Speed", newSpeed);
        }
    }
}