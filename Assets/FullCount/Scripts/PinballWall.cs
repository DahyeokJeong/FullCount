using UnityEngine;

namespace FullCount
{
    [DisallowMultipleComponent]
    public sealed class PinballWall : MonoBehaviour
    {
        #region Rebound settings
        [SerializeField, Min(0.1f), Tooltip("충돌 전 수평 속도에 곱하는 반발 배율")]
        private float speedMultiplier = 1.12f;
        [SerializeField, Min(0.1f), Tooltip("벽에서 튕겨 나오는 최소 속도")]
        private float minimumSpeed = 6f;
        [SerializeField, Min(0.1f), Tooltip("반복 충돌 시 속도가 과도하게 커지는 것을 막는 상한")]
        private float maximumSpeed = 14f;
        [SerializeField, Range(0f, 45f), Tooltip("반사 방향에 더하는 무작위 수평 각도. 0이면 정반사")]
        private float directionVariation = 12f;
        #endregion

        private void OnValidate()
        {
            maximumSpeed = Mathf.Max(minimumSpeed, maximumSpeed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody body = collision.rigidbody;
            if (body == null || body.isKinematic || !body.TryGetComponent<SphereCollider>(out var sphere))
                return;

            Vector3 incoming = Vector3.ProjectOnPlane(collision.relativeVelocity, Vector3.up);
            if (incoming.sqrMagnitude < 0.0025f)
                return;

            Vector3 wallNormal = Vector3.zero;
            float strongestImpact = 0.01f;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                Vector3 normal = Vector3.ProjectOnPlane(contact.normal, Vector3.up);
                if (normal.sqrMagnitude < 0.5f)
                    continue;

                normal.Normalize();
                if (Vector3.Dot(normal, body.worldCenterOfMass - contact.point) < 0f)
                    normal = -normal;

                float impact = Mathf.Abs(Vector3.Dot(incoming, normal));
                if (impact > strongestImpact)
                {
                    strongestImpact = impact;
                    wallNormal = normal;
                }
            }

            if (wallNormal == Vector3.zero)
                return;

            // 접촉 법선을 구슬 쪽으로 맞춰 콜백의 상대 속도 방향과 무관하게 입사 방향을 얻는다.
            if (Vector3.Dot(incoming, wallNormal) > 0f)
                incoming = -incoming;

            Vector3 reflected = Vector3.Reflect(incoming, wallNormal).normalized;
            Vector3 tangent = Vector3.Cross(Vector3.up, wallNormal);
            float angle = Mathf.Atan2(Vector3.Dot(reflected, tangent), Vector3.Dot(reflected, wallNormal))
                * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle + Random.Range(-directionVariation, directionVariation), -75f, 75f);
            Vector3 direction = wallNormal * Mathf.Cos(angle * Mathf.Deg2Rad)
                + tangent * Mathf.Sin(angle * Mathf.Deg2Rad);
            float speed = Mathf.Clamp(incoming.magnitude * speedMultiplier, minimumSpeed, maximumSpeed);
            Vector3 horizontalVelocity = direction * speed;
            body.linearVelocity = horizontalVelocity + Vector3.up * body.linearVelocity.y;

            float radius = sphere.radius * Mathf.Abs(sphere.transform.lossyScale.y);
            body.maxAngularVelocity = Mathf.Max(body.maxAngularVelocity, speed / radius);
            body.angularVelocity = Vector3.Cross(Vector3.up, horizontalVelocity) / radius;
        }
    }
}
