using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Adjustment
{
    public unsafe class WaveEffects 
    {

        public struct PixelData 
        {
            public byte blue;
            public byte green;
            public byte red;
            public byte alpha;
        }

        Bitmap Subject;
        int SubjectWidth;
        BitmapData bitmapData = null;
        Byte* pBase = null;
        bool isLocked = false;
        int _bits = 0;

        public WaveEffects(Bitmap SubjectBitmap, int bits) 
        {
            this.Subject = SubjectBitmap;
            _bits = bits;
            try 
            {
                //LockBits();
            } 
            catch (Exception ex) 
            {
                throw ex;
            }            
        }

        public void Release() 
        {
            try 
            {
                UnlockBits();
            } 
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public Bitmap Bitmap
        {
            get 
            {
                return Subject;
            }
        }

        public void SetPixel(int X, int Y, Color Colour) 
        {
            try 
            {
                PixelData* p = PixelAt(X, Y);
                p->red = Colour.R;
                p->green = Colour.G;
                p->blue = Colour.B;
            }
            catch (AccessViolationException ave)
            {
                throw (ave);
            } 
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public Color GetPixel(int X, int Y) 
        {
            try
            {
                PixelData* p = PixelAt(X, Y);
                return Color.FromArgb((int)p->red, (int)p->green, (int)p->blue);
            } 
            catch (AccessViolationException ave) 
            {
                throw (ave);
            } 
            catch (Exception ex) 
            {
                throw ex;
            }            
        }

        public int Width() { return Subject.Width; }
        public int Height() { return Subject.Height; }
        public bool  IsLocked() { return isLocked; }
        public BitmapData Data() { return bitmapData; }

        public void LockBits() 
        {
            if (isLocked) return;
            try
            {                
                GraphicsUnit unit = GraphicsUnit.Pixel;
                RectangleF boundsF = Subject.GetBounds(ref unit);
                Rectangle bounds = new Rectangle((int)boundsF.X,
                    (int)boundsF.Y,
                    (int)boundsF.Width,
                    (int)boundsF.Height);

                SubjectWidth = (int)boundsF.Width * sizeof(PixelData);
                if (SubjectWidth % _bits != 0)
                {
                    SubjectWidth = _bits * (SubjectWidth / _bits + 1);
                }
                if (_bits == 3)
                    bitmapData = Subject.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                else 
                    bitmapData = Subject.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                pBase = (Byte*)bitmapData.Scan0.ToPointer();
            }
            finally
            {
                isLocked = true ;
            }
        }

        private PixelData* PixelAt(int x, int y)
        {
            return (PixelData*)(pBase + y * SubjectWidth + x * sizeof(PixelData));            
        }

        private void UnlockBits() 
        {
            if (bitmapData == null) return;
            Subject.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
            isLocked = false;
        }
    }
}

