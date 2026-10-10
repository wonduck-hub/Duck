using System.Numerics;

using DuckPhysics.Physics2D;

namespace WatermelonGame
{
    public enum EWatermelonLevel
    {
        ZERO,
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        ELEVEN
    }
    public class Watermelon : DinamicObject2D
    {
        private EWatermelonLevel mLevel;
        public EWatermelonLevel Level
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

        /*
         * WatermelonObject circle1 = 
                new WatermelonObject(
                    new System.Numerics.Vector2(35, 200),
                    new System.Numerics.Vector2(0, 0),
                    new System.Numerics.Vector2(30, 30),
                    0f, 
                    1, 
                    EColliderType.CIRCLE);
         */
        public Watermelon(Vector2 position, EWatermelonLevel level)
            : base(position, new Vector2(0, 0), new Vector2(0, 0), 0, EColliderType.CIRCLE)
        {
            mLevel = level;

            Vector2 size = new Vector2(1, 1);
            Collider = new Collider2D(this, EColliderType.CIRCLE, new Vector2(0, 0), size, 0);

            Restitution = 0.1f; // 수박 게임에서 과일끼리 튕겨나가지 않음
            Friction = 0f; // 수박 게임에서는 마찰이 없음
        }
    }
}
