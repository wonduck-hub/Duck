using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace WatermelonGame
{
    public class WatermelonObject : DinamicObject2D
    {
        public WatermelonObject(Vector2 position, 
                                Vector2 velocity, 
                                Vector2 size, 
                                float rotation, 
                                EColliderType colliderType = EColliderType.CIRCLE)
            : base(position, velocity, size, rotation, colliderType)
        {
        }
    }
}
