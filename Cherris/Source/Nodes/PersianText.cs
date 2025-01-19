using Raylib_cs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Cherris
{
    public class PersianText : Node2D, IDisposable
    {
        private Texture2D texture;
        public string Text { get; set; } = "سلام";

        public override void Ready()
        {
        }

        public override void Update()
        {
            base.Update();
        }

        protected override void Draw()
        {
            base.Draw();
        }

        public void Dispose()
        {
        }
    }
}