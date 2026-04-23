using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

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
    public class WatermelonObject : DinamicObject2D
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
        public WatermelonObject(Vector2 position, EWatermelonLevel level)
            : base(position, new Vector2(0, 0), new Vector2(0, 0), 0, EColliderType.CIRCLE)
        {
            Restitution = 0.1f; // 수박 게임에서 과일끼리 튕겨나가지 않음
            mLevel = level;

            Vector2 size = new Vector2(1, 1);
            switch (mLevel)
            {
                case EWatermelonLevel.ZERO:
                    size = new Vector2(20, 20);
                    break;
                case EWatermelonLevel.ONE:
                    size = new Vector2(25, 25);
                    break;
                case EWatermelonLevel.TWO:
                    size = new Vector2(30, 30);
                    break;
                case EWatermelonLevel.THREE:
                    size = new Vector2(35, 35);
                    break;
                case EWatermelonLevel.FOUR:
                    size = new Vector2(40, 40);
                    break;
                case EWatermelonLevel.FIVE:
                    size = new Vector2(50, 50);
                    break;
                case EWatermelonLevel.SIX:
                    size = new Vector2(60, 60);
                    break;
                case EWatermelonLevel.SEVEN:
                    size = new Vector2(70, 70);
                    break;
                case EWatermelonLevel.EIGHT:
                    size = new Vector2(80, 80);
                    break;
                case EWatermelonLevel.NINE:
                    size = new Vector2(90, 90);
                    break;
                case EWatermelonLevel.TEN:
                    size = new Vector2(100, 100);
                    break;
                case EWatermelonLevel.ELEVEN:
                    size = new Vector2(120, 120);
                    break;
                default:
                    Debug.Fail("지원하지 않는 WatermelonLevel 입니다.");
                    break;
            }

            Collider = new Collider2D(this, EColliderType.CIRCLE, new Vector2(0, 0), size, 0);
        }
    }
}
