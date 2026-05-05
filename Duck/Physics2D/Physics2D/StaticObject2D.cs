using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class StaticObject2D : IObject2D
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public float Scale { get; set; } = 1;
        Vector2 IObject2D.Velocity { get; set; }

        public Collider2D Collider { get; set; }

        // 마찰
        public float Friction { get; set; } = 0.95f;    // 마찰
        public float Restitution { get; set; } = 0.8f;  // 반발 계수
        public float Mass { get; set; } = 1.0f;         // 질량

        public StaticObject2D(Vector2 position, Vector2 size, float rotation, EColliderType colliderType = EColliderType.CIRCLE)
        {
            Position = position;
            Rotation = rotation;
            Collider = new Collider2D(this, colliderType, Vector2.Zero, size, 0); // 기본 콜라이더 설정 (원형, 오프셋 없음, 레이어 0)
        }

        public StaticObject2D(EColliderType colliderType)
            : this(new Vector2(0, 0), new Vector2(5, 100), 0.0f, colliderType)
        {
        }

        virtual public void Dispose()
        {
        }

        virtual public void Update(float deltaTime)
        {
        }
    }
}
