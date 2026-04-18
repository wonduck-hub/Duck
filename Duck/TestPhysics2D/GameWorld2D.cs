using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Numerics;
using System.Text;

namespace WatermelonGame
{
    public class GameWorld2D : World2D
    {
        public GameWorld2D() : base(0.5f)
        {
        }

        protected override void ResolveCollision(DinamicObject2D a, DinamicObject2D b, Vector2 normal, float penetration)
        {
            if (a is WatermelonObject)
            {

            }
            else
            {
                base.ResolveCollision(a, b, normal, penetration);
            }
        }
    }
}
