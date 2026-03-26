using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public enum EColliderType
    {
        CIRCLE,
        RECTANGLE,
        POLYGON
    }
    public class Collider2D
    {
        #region 속성들
        private IObject2D mOwner { get; set; }

        public EColliderType Type { get; set; }
        public Vector2 Offset { get; set; } // 콜라이더의 중심이 객체의 위치에서 얼마나 떨어져 있는지 나타내는 벡터
        public float Radius { get; set; }     // Circle
        public Vector2 Size { get; set; }     // Rectangle
        public List<Vector2> Points { get; set; } // Polygon

        public bool IsTrigger { get; set; }
        public bool IsStatic { get; set; }
        public int Layer { get; set; } // 충돌 그룹을 나누기 위한 레이어 -> 예: - 예: Player는 Enemy와 충돌하지만, PlayerProjectile은 Player와는 충돌하지 않도록 설정
        #endregion

        public Collider2D(IObject2D owner, EColliderType type, Vector2 offset, int layer)
        {
            mOwner = owner;
            Type = type;
            Offset = offset;

            IsTrigger = true;
            IsStatic = false;
            Layer = layer;
            Points = new List<Vector2>();
        }
    }
}
