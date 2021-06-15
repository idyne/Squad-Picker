using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class NewCameraFollow : MonoBehaviour
    {
        public static NewCameraFollow Instance = null;
        public NewTroop Target = null;
        public Vector3 InitialOffset = Vector3.zero;
        private Vector3 offset = Vector3.zero;
        public Vector3 CrowdedOffset = Vector3.zero;
        public float Speed = 1;
        public Vector3 Rotation;
        [SerializeField] private bool freezeX = false;
        [SerializeField] private bool freezeY = false;
        [SerializeField] private bool freezeZ = false;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            else
            {
                DestroyImmediate(gameObject);
                return;
            }
            offset = InitialOffset;
        }

        private void FixedUpdate()
        {
            if (Target)
            {
                Follow();
                offset = InitialOffset - transform.forward * (Mathf.Clamp(Target.Size, 0, 312) / Target.MaxNumberOfHumans);
            }

        }

        private void Follow()
        {
            bool crowded = false;
            Vector3 pos = Target.Focus.position + offset;
            if (freezeX)
                pos.x = transform.position.x;
            if (freezeY)
                pos.y = transform.position.y;
            if (freezeZ)
                pos.z = transform.position.z;
            Quaternion desiredRot = Quaternion.Euler(Rotation.x, Target.Angle - (crowded ? 0 : 0), Rotation.z);
            float distance = Vector3.Distance(transform.position, pos);
            float angle = Quaternion.Angle(transform.rotation, desiredRot);

            float rotatingSpeed = angle * Speed / distance;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, rotatingSpeed * Time.fixedDeltaTime);

            if (angle > 1)
                transform.position = Vector3.Lerp(transform.position, pos, Speed * Time.fixedDeltaTime);
            else
                transform.position = Vector3.MoveTowards(transform.position, pos, Speed * Time.fixedDeltaTime);

        }
        public void Turn(float angle)
        {
            InitialOffset = Quaternion.Euler(0, angle, 0) * InitialOffset;
            offset = InitialOffset - transform.forward * (Target.Size / Target.MaxNumberOfHumans);

        }
    }

}
