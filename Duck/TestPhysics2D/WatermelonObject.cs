using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace WatermelonGame
{
    public class WatermelonObject : DinamicObject2D
    {
        private short mLevel = 0;
        public short Level
        {
            get { return mLevel; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                mLevel = value;
            }
        }
        public WatermelonObject(Vector2 position,
                                Vector2 velocity,
                                Vector2 size,
                                float rotation,
                                short level,
                                EColliderType colliderType = EColliderType.CIRCLE)
            : base(position, velocity, size, rotation, colliderType)
        {
            Restitution = 0.1f; // 수박 게임에서 과일끼리 튕겨나가지 않음
            mLevel = level;
        }
    }
}
