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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
