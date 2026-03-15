using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public interface IObject2D : IDisposable
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
    }
}
