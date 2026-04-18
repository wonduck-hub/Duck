using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class DinamicObject2D : IObject2D
    {
        public Vector2 Position { get; set; }
        public Vector2 LastPosition { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public float Scale { get; set; }
        public Collider2D Collider { get; set; }

        // 마찰
        public float Friction { get; set; } = 0.95f;
        public float Restitution { get; set; } = 0.8f;
        public float Mass { get; set; } = 1.0f;

        public DinamicObject2D(Vector2 position, Vector2 velocity, Vector2 size, float rotation, EColliderType colliderType = EColliderType.CIRCLE)
        {
            Position = position;
            LastPosition = position;
            Velocity = velocity;
            Rotation = rotation;
            Collider = new Collider2D(this, colliderType, Vector2.Zero, size, 0); // 기본 콜라이더 설정 (원형, 오프셋 없음, 레이어 0)
        }
        public void Dispose()
        {
        }

        public void Update(float deltaTime)
        {
            LastPosition = Position;
            Position += Velocity * deltaTime;
        }
        public void AddVelocity(Vector2 moveVec)
        {
            Velocity += moveVec;
        }
        
    }
}
