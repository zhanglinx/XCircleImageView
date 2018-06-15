using Android.Graphics;
using Java.Lang;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Views;
using Android.Content;
using Android.Util;
using Android.Content.Res;
using Android.Widget;
using Android.OS;
using System;
using Android.Runtime;
using XCircleImageView;

namespace com.zhanglinx.xcircleimageview
{
    public class XCircleImageView : ImageView
    {
        private static int DEFAULT_BORDER_WIDTH = 0;
        private static Color DEFAULT_BORDER_COLOR = Color.Black;
        private static Color DEFAULT_CIRCLE_BACKGROUND_COLOR = Color.Transparent;
        private static bool DEFAULT_BORDER_OVERLAY = false;
        private static int COLORDRAWABLE_DIMENSION = 2;
        private Color mBorderColor = DEFAULT_BORDER_COLOR;
        private int mBorderWidth = DEFAULT_BORDER_WIDTH;
        private Color mCircleBackgroundColor = DEFAULT_CIRCLE_BACKGROUND_COLOR;

        private static ScaleType SCALE_TYPE = ScaleType.CenterCrop;
        private static Bitmap.Config BITMAP_CONFIG = Bitmap.Config.Argb8888;

        private bool mBorderOverlay;
        private bool mReady;
        private bool mSetupPending;
        private bool mDisableCircularTransformation;

        private RectF mDrawableRect = new RectF();
        public RectF mBorderRect = new RectF();

        private Matrix mShaderMatrix = new Matrix();
        private Paint mBitmapPaint = new Paint();
        private Paint mBorderPaint = new Paint();
        private Paint mCircleBackgroundPaint = new Paint();

        private float mDrawableRadius;
        private float mBorderRadius;


        private ColorFilter mColorFilter;
        private Bitmap mBitmap;
        private BitmapShader mBitmapShader;
        private int mBitmapWidth;
        private int mBitmapHeight;
        public XCircleImageView(IntPtr a, JniHandleOwnership b) : base(a, b)
        {
        }
        public XCircleImageView(Context context) : base(context)
        {
            init();
        }
        public XCircleImageView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {

        }
        public XCircleImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.CircleImageView, defStyle, 0);

            mBorderWidth = a.GetDimensionPixelSize(Resource.Styleable.CircleImageView_civ_border_width, DEFAULT_BORDER_WIDTH);
            mBorderColor = a.GetColor(Resource.Styleable.CircleImageView_civ_border_color, DEFAULT_BORDER_COLOR);
            mBorderOverlay = a.GetBoolean(Resource.Styleable.CircleImageView_civ_border_overlay, DEFAULT_BORDER_OVERLAY);
            if (a.HasValue(Resource.Styleable.CircleImageView_civ_circle_background_color))
            {
                mCircleBackgroundColor = a.GetColor(Resource.Styleable.CircleImageView_civ_circle_background_color, DEFAULT_CIRCLE_BACKGROUND_COLOR);
            }
            else if (a.HasValue(Resource.Styleable.CircleImageView_civ_fill_color))
            {
                mCircleBackgroundColor = a.GetColor(Resource.Styleable.CircleImageView_civ_fill_color, DEFAULT_CIRCLE_BACKGROUND_COLOR);
            }
            a.Recycle();
            init();
        }
        private void init()
        {
            SetScaleType(SCALE_TYPE);
            mReady = true;
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Lollipop)
            {
                OutlineProvider = new MyOutlineProvider(mBorderRect);
            }
            if (mSetupPending)
            {
                setup();
                mSetupPending = false;
            }
        }
        public override ScaleType GetScaleType()
        {
            return SCALE_TYPE;
        }
        public override void SetScaleType(ScaleType scaleType)
        {
            if (scaleType != SCALE_TYPE)
            {
                throw new Java.Lang.IllegalArgumentException(string.Format("ScaleType {0} s not  supported", scaleType));
            }
        }
        public override void SetAdjustViewBounds(bool adjustViewBounds)
        {
            if (adjustViewBounds)
            {
                throw new Java.Lang.IllegalArgumentException("adjustViewBounds   not  supported");
            }
        }
        protected override void OnDraw(Canvas canvas)
        {
            if (mDisableCircularTransformation)
            {
                base.OnDraw(canvas);
                return;
            }
            if (mBitmap == null)
            {
                return;
            }
            if (mCircleBackgroundColor != Color.Transparent)
            {
                canvas.DrawCircle(mDrawableRect.CenterX(), mDrawableRect.CenterY(), mDrawableRadius, mCircleBackgroundPaint);
            }
            canvas.DrawCircle(mDrawableRect.CenterX(), mDrawableRect.CenterY(), mDrawableRadius, mBitmapPaint);
            if (mBorderWidth > 0)
            {
                canvas.DrawCircle(mBorderRect.CenterX(), mBorderRect.CenterY(), mBorderRadius, mBorderPaint);
            }
        }
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            setup();
        }
        public override void SetPadding(int left, int top, int right, int bottom)
        {
            base.SetPadding(left, top, right, bottom);
            setup();
        }
        public override void SetPaddingRelative(int start, int top, int end, int bottom)
        {
            base.SetPaddingRelative(start, top, end, bottom);
            setup();
        }
        public int getBorderColor()
        {
            return mBorderColor;
        }
        public void setBorderColor(Color borderColor)
        {
            //int b =   boo
            if (borderColor == mBorderColor)
            {
                return;
            }
            mBorderColor = borderColor;
            mBorderPaint.Color = mBorderColor;
            Invalidate();
        }
        public int getCircleBackgroundColor()
        {
            return mCircleBackgroundColor;
        }
        public void setCircleBackgroundColor(Color circleBackgroundColor)
        {
            if (circleBackgroundColor == mCircleBackgroundColor)
                return;
            mCircleBackgroundColor = circleBackgroundColor;
            mCircleBackgroundPaint.Color = circleBackgroundColor;
            Invalidate();
        }
        public void setCirclebackgroundColorResource(int circleBackgroundRes)
        {
            setCircleBackgroundColor(Context.Resources.GetColor(circleBackgroundRes));
        }

        [Obsolete]
        public int getFillColor()
        {
            return getCircleBackgroundColor();
        }

        [Obsolete]
        public void setFillColor(Color fillColor)
        {
            setCircleBackgroundColor(fillColor);
        }

        [Obsolete]
        public void setFillColorResource(int fillColorRes)
        {
            setCirclebackgroundColorResource(fillColorRes);
        }
        public int getBorderWidth()
        {
            return mBorderWidth;
        }
        public void setBorderWidth(int borderWidth)
        {
            if (borderWidth == mBorderWidth)
                return;
            mBorderWidth = borderWidth;
            setup();
        }

        public bool isBorderOverlay()
        {
            return mBorderOverlay;
        }
        public void setBorderOverlay(bool borderOverlay)
        {
            if (borderOverlay == mBorderOverlay)
                return;
            mBorderOverlay = borderOverlay;
            setup();
        }
        public bool isDisableCircularTransformation()
        {
            return mDisableCircularTransformation;
        }
        public void setDisableCircularTransformation(bool disableCircularTransformation)
        {
            if (mDisableCircularTransformation == disableCircularTransformation)
                return;
            mDisableCircularTransformation = disableCircularTransformation;
            initializeBitmap();
        }
        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            initializeBitmap();
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            base.SetImageDrawable(drawable);
            initializeBitmap();
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            initializeBitmap();
        }
        public override void SetImageURI(Android.Net.Uri uri)
        {
            base.SetImageURI(uri);
            initializeBitmap();
        }

        public override void SetColorFilter(ColorFilter cf)
        {
            if (cf == mColorFilter)
                return;
            mColorFilter = cf;
            applyColorFilter();
            Invalidate();
        }

        public override ColorFilter ColorFilter => mColorFilter;

        private void applyColorFilter()
        {
            if (mBitmapPaint != null)
            {
                mBitmapPaint.SetColorFilter(mColorFilter);
            }
        }
        private Bitmap getBitmapFromDrawable(Drawable drawable)
        {
            if (drawable == null)
            {
                return null;
            }
            if (drawable is Drawable)
            {
                return ((BitmapDrawable)drawable).Bitmap;
            }
            try
            {
                Bitmap bitmap;
                if (drawable is ColorDrawable)
                {
                    bitmap = Bitmap.CreateBitmap(COLORDRAWABLE_DIMENSION, COLORDRAWABLE_DIMENSION, BITMAP_CONFIG);
                }
                else
                {
                    bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, BITMAP_CONFIG);
                }
                Canvas canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);
                return bitmap;
            }
            catch (Java.Lang.Exception ex)
            {
                ex.PrintStackTrace();
                return null;
            }
        }
        private void initializeBitmap()
        {
            if (mDisableCircularTransformation)
            {
                mBitmap = null;
            }
            else
            {
                mBitmap = getBitmapFromDrawable(Drawable);
            }
            setup();
        }
        private void setup()
        {
            if (!mReady)
            {
                mSetupPending = true;
                return;
            }
            if (Width == 0 && Height == 0)
            {
                return;
            }
            if (mBitmap == null)
            {
                Invalidate();
                return;
            }
            mBitmapShader = new BitmapShader(mBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

            mBitmapPaint.AntiAlias = true;
            mBitmapPaint.SetShader(mBitmapShader);


            mBorderPaint.SetStyle(Paint.Style.Stroke);
            mBorderPaint.Color = mBorderColor;
            mBorderPaint.AntiAlias = true;
            mBorderPaint.StrokeWidth = mBorderWidth;

            mCircleBackgroundPaint.SetStyle(Paint.Style.Fill);
            mCircleBackgroundPaint.AntiAlias = true;
            mCircleBackgroundPaint.Color = mCircleBackgroundColor;

            mBitmapHeight = mBitmap.Height;
            mBitmapWidth = mBitmap.Width;

            mBorderRect.Set(calculateBounds());
            mBorderRadius = System.Math.Min(mBorderRect.Height() - mBorderWidth / 2.0f, (mBorderRect.Width() - mBorderWidth) / 2.0f);

            mDrawableRect.Set(mBorderRect);
            if (!mBorderOverlay && mBorderWidth > 0)
            {
                mDrawableRect.Inset(mBorderWidth - 1.0f, mBorderWidth - 1.0f);
            }
            mDrawableRadius = System.Math.Min(mDrawableRect.Height() / 2.0f, mDrawableRect.Width() / 2.0f);
            applyColorFilter();
            updateShaderMatrix();
            Invalidate();
        }
        private RectF calculateBounds()
        {
            int availableWidth = Width - PaddingLeft - PaddingRight;
            int availableHeight = Height - PaddingTop - PaddingBottom;

            int sideLength = System.Math.Min(availableWidth, availableHeight);

            float left = PaddingLeft + (availableWidth - sideLength) / 2f;
            float top = PaddingTop + (availableHeight - sideLength) / 2f;
            return new RectF(left, top, left + sideLength, top + sideLength);
        }

        private void updateShaderMatrix()
        {
            float scale;
            float dx = 0, dy = 0;
            mShaderMatrix.Set(null);

            if (mBitmapWidth * mDrawableRect.Height() > mDrawableRect.Width() * mBitmapHeight)
            {
                scale = mDrawableRect.Height() / (float)mBitmapHeight;
                dx = (mDrawableRect.Width() - mBitmapWidth * scale) * 0.5f;
            }
            else
            {
                scale = mDrawableRect.Width() / (float)mBitmapWidth;
                dy = ((mDrawableRect.Height()) - mBitmapHeight * scale) * 0.5f;
            }
            mShaderMatrix.SetScale(scale, scale);
            mShaderMatrix.PostTranslate((int)(dx + 0.5f) + mDrawableRect.Left, (int)(dy + 0.5f) + mDrawableRect.Top);
            mBitmapShader.SetLocalMatrix(mShaderMatrix);
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            return inTouchableArea(e.GetX(), e.GetY()) && base.OnTouchEvent(e);
        }

        private bool inTouchableArea(float x, float y)
        {
            return System.Math.Pow(x - mBorderRect.CenterX(), 2) + System.Math.Pow(y - mBorderRect.CenterY(), 2) <= System.Math.Pow(mBorderRadius, 2);
        }

        private class MyOutlineProvider : ViewOutlineProvider
        {
            RectF mBorderRect;
            public MyOutlineProvider(RectF f)
            {
                mBorderRect = f;
            }
            public override void GetOutline(View view, Outline outline)
            {
                Rect bounds = new Rect();
                mBorderRect.RoundOut(bounds);
                outline.SetRoundRect(bounds, bounds.Width() / 2.0f);
            }
        }

    }
}