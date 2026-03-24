using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class World2D : IDisposable
    {
        #region 상수들
        float DEFAULT_GRAVITY = 9.8f;
        #endregion
        #region 월드 속성들
        private Stopwatch stopwatch = new Stopwatch();
        private double lastTime = 0;

        private float mGravitiy;
        public float Gravity
        {
            get { return mGravitiy; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                mGravitiy = value;
            }
        }

        private uint mWidth;
        public uint Width
        {
            get { return mWidth; }
            set { Width = value; }
        }
        private uint mHeight;
        public uint Height
        {
            get { return mHeight; }
            set { Height = value; }
        }

        private bool mHasGravity = false;
        public bool HasGravity
        {
            get { return mHasGravity; }
            set 
            { 
                mHasGravity = value; 
            }
        }
        #endregion

        #region 가지고 있는 개체들(component)
        private List<IObject2D> mObjects = new List<IObject2D>(1024);
        public List<IObject2D> Objects
        {
            get { return mObjects; }
        }
        #endregion

        #region Constructor
        public World2D()
        {
            Gravity = DEFAULT_GRAVITY;
        }
        public World2D(float gravitiy)
        {
            Gravity = gravitiy;
        }
        public World2D (World2D world) // 복사생성자
            : this(world.Gravity)
        {}


        #endregion
        public void Dispose()
        {
            Debug.Assert(mObjects != null);
            foreach (var obj in mObjects)
            {
                obj.Dispose();
            }
        }

        #region Methods
        public bool TryAddObject(IObject2D obj)
        {
            if (obj == null)
            {
                return false;
            }
            mObjects.Add(obj);
            return true;
        }

        public void Start()
        {
            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;
        }

        public void Update()
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentTime - lastTime);

            foreach (var obj in mObjects)
            {
                // 중력
                if (mHasGravity)
                {
                    obj.Velocity += new System.Numerics.Vector2(0, -Gravity) * deltaTime; // 중력 가속도 적용
                }

                //TODO: 충돌 처리

                //TODO: 월드 경계 처리

                //TODO: 기타 물리 계산

                // 개체 업데이트
                obj.Update(deltaTime);
            }

        }
        #endregion
    }
}
