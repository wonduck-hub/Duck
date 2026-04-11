using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class StaticObject2D : IObject2D
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public float Scale { get; set; }
        Vector2 IObject2D.Velocity { get; set; }

        public Collider2D Collider { get; set; }

        public StaticObject2D(Vector2 position, Vector2 velocity, Vector2 size, float rotation, EColliderType colliderType = EColliderType.CIRCLE)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            Collider = new Collider2D(this, colliderType, Vector2.Zero, size, 0); // 기본 콜라이더 설정 (원형, 오프셋 없음, 레이어 0)
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Update(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
