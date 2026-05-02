using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows.Media;

namespace WatermelonGame
{
    internal class LevelVisual
    {
        public ImageBrush? ImgBrushOrNull { get; set; }
        public Vector2 Size { get; set; }

        public LevelVisual(ImageBrush? imgBrush, Vector2 size)
        {
            ImgBrushOrNull = imgBrush;
            Size = size;
        }
    }
}
