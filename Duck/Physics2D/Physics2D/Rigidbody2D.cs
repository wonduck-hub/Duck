using DuckPhysics.Physics2D;
using System.Numerics;

namespace DuckPhysics.Physics2D
{
    public class Rigidbody2D : IObject2D
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        #region 속성


        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
