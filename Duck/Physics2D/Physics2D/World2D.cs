using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class World2D : IDisposable
    {
        #region 월드 전역 속성들
        private float mDeltaTime;
        public float DeltaTime
        {
            get { return mDeltaTime; }
            set 
            { 
                if (value <= 0)
                {
                    value = 0.1f;
                }
                mDeltaTime = value; 
            }
        }
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
            DeltaTime = 1;
            Gravity = 9.8f;
        }
        public World2D(float deltaTime,  float gravitiy)
        {
            DeltaTime = deltaTime;
            Gravity = gravitiy;
        }
        public World2D (World2D world)
            : this(world.DeltaTime, world.Gravity)
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
    }
}
