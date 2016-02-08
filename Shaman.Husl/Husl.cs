using System;
using System.Drawing;
using System.Globalization;
#if NETFX_CORE || WINDOWS_PHONE
using Color = Windows.UI.Color;
#else
using Color = System.Drawing.Color;
#endif


namespace Shaman.Types
{

    /// <summary>
    /// Represents a color in the HUSL color space.
    /// <a href="http://boronine.com/husl/">HUSL - Human friendly HSL</a>
    /// </summary>
    public struct HuslColor : IEquatable<HuslColor>
    {
        /// <summary>
        /// Creates an instance based on the provided values.
        /// </summary>
        /// <param name="hue">The hue, from 0.0 to 1.0.</param>
        /// <param name="saturation">The saturation, from 0.0 to 1.0.</param>
        /// <param name="lightness">The lightness, from 0.0 to 1.0.</param>
        public HuslColor(float hue, float saturation, float lightness)
            : this(hue, saturation, lightness, 1)
        {
        }

        /// <summary>
        /// Creates an instance based on the provided values.
        /// </summary>
        /// <param name="hue">The hue, from 0.0 to 1.0.</param>
        /// <param name="saturation">The saturation, from 0.0 to 1.0.</param>
        /// <param name="lightness">The lightness, from 0.0 to 1.0.</param>
        /// <param name="alpha">The alpha, from 0.0 to 1.0.</param>
        public HuslColor(float hue, float saturation, float lightness, float alpha)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.lightness = lightness;
            this.alpha = alpha;
        }
        private float hue;
        private float saturation;
        private float lightness;
        private float alpha;

        /// <summary>
        /// The transparent color.
        /// </summary>
        public static readonly HuslColor Transparent = new HuslColor(0, 0, 0, 0);

        /// <summary>
        /// The black color.
        /// </summary>
        public static readonly HuslColor Black = new HuslColor(0, 0, 0, 1);


        /// <summary>
        /// The white color.
        /// </summary>
        public static readonly HuslColor White = new HuslColor(0, 0, 1, 1);

        /// <summary>
        /// Gets or sets the hue of the color, from 0.0 to 1.0.
        /// </summary>
        public float Hue
        {
            get { return hue; }
            set { hue = value; }
        }

        public static float GetHueDistance(HuslColor color1, HuslColor color2)
        {
            var h1 = Math.Min(color1.Hue, color2.Hue);
            var h2 = Math.Max(color1.Hue, color2.Hue);
            return Math.Min(h2 - h1, h1 + (1 - h2));
        }

        /// <summary>
        /// Gets or sets the saturation of the color, from 0.0 (gray) to 1.0 (saturated).
        /// </summary>
        public float Saturation
        {
            get { return saturation; }
            set { saturation = value; }
        }

        /// <summary>
        /// Gets or sets the lightness of the color, from 0.0 (black) to 1.0 (white).
        /// </summary>
        public float Lightness
        {
            get { return lightness; }
            set { lightness = value; }
        }

        /// <summary>
        /// Gets or sets the alpha of the color, from 0.0 (transparent) to 1.0 (opaque).
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }


        private static float Clamp(float val)
        {
            if (val < 0) return 0;
            if (val > 1) return 1;
            return val;
        }

        /// <summary>
        /// Makes sure that all the parameters are within their ranges.
        /// </summary>
        public void Normalize()
        {
            hue = Clamp(hue);
            saturation = Clamp(saturation);
            lightness = Clamp(lightness);
            alpha = Clamp(alpha);

        }
        public override string ToString()
        {
            return string.Format("({0:0.00}, {1:0.00}, {2:0.00}, {3:0.00})", Hue, Saturation, Lightness, Alpha);
        }

        /// <summary>
        /// Converts the color to the RGB format.
        /// </summary>
        /// <returns>The converted color.</returns>
        public Color ToRgb()
        {
            Normalize();
            return HuslConverter.HuslToRgb(this);
        }

        public string ToHtmlColor()
        {
            var rgb = ToRgb();
            if (alpha < 1) return "rgba(" + rgb.R + ", " + rgb.G + ", " + rgb.B + ", " + alpha.ToString(CultureInfo.InvariantCulture) + ")";
            return "#" + rgb.R.ToString("X2") + rgb.G.ToString("X2") + rgb.B.ToString("X2");
        }



        public static bool operator ==(HuslColor col1, HuslColor col2)
        {
            return
                col1.Hue == col2.Hue &&
                col1.Lightness == col2.Lightness &&
                col1.Saturation == col2.Saturation &&
                col1.Alpha == col2.Alpha;
        }


        public static bool operator !=(HuslColor col1, HuslColor col2)
        {
            return !(col1 == col2);
        }



        public bool Equals(HuslColor other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is HuslColor) return (HuslColor)obj == this;
            return false;
        }

        public override int GetHashCode()
        {
            return (hue + lightness * 100 + saturation * 10000 + alpha * 1000000).GetHashCode();
        }

        public static HuslColor Mix(HuslColor firstColor, HuslColor secondColor, double firstColorRatio)
        {
            var rest = 1 - firstColorRatio;
            var hue = (firstColor.hue * firstColorRatio) + (secondColor.hue * rest);
            var lightness = (firstColor.lightness * firstColorRatio) + (secondColor.lightness * rest);
            var saturation = (firstColor.saturation * firstColorRatio) + (secondColor.saturation * rest);
            var alpha = (firstColor.alpha * firstColorRatio) + (secondColor.alpha * rest);
            return new HuslColor((float)hue, (float)saturation, (float)lightness, (float)alpha);
        }
        
        public static HuslColor FromRgb(Color col)
        {
            return HuslConverter.RgbToHusl(col);
        }
    }

    internal static class HuslConverter
    {
        private struct Vec3Float
        {
            public Vec3Float(float item0, float item1, float item2)
            {
                this.Item0 = item0;
                this.Item1 = item1;
                this.Item2 = item2;
            }
            public float Item0;
            public float Item1;
            public float Item2;
            public float this[int index]
            {
                get
                {
                    return
                        index == 0 ? Item0 :
                        index == 1 ? Item1 :
                        Item2;
                }
                set
                {
                    if (index == 0) Item0 = value;
                    else if (index == 1) Item1 = value;
                    else Item2 = value;
                }
            }
        }

        private static Vec3Float HuslToRgbInternal(float hue, float saturation, float lightness)
        {
            return XYZ_RGB(LUV_XYZ(LCH_LUV(HUSL_LCH(new Vec3Float(hue * 360, saturation * 100, lightness * 100)))));
        }

        public static Color HuslToRgb(float hue, float saturation, float lightness)
        {
            return HuslToRgb(hue, saturation, lightness, 1);
        }
        public static Color HuslToRgb(float hue, float saturation, float lightness, float alpha)
        {
            var components = HuslToRgbInternal(hue, saturation, lightness);
            return Color.FromArgb(ToByte(alpha), ToByte(components.Item0), ToByte(components.Item1), ToByte(components.Item2));
        }

        public static Color HuslToRgb(HuslColor husl)
        {
            return HuslToRgb(husl.Hue, husl.Saturation, husl.Lightness, husl.Alpha);
        }

        private static byte ToByte(float value)
        {
            var k = (int)(value * 255);
            if (k < 0) return 0;
            if (k > 255) return 255;
            return (byte)k;
        }
        private static float FromByte(byte value)
        {
            return (float)value / 256;
        }

        public static HuslColor RgbToHusl(Color color)
        {
            var t = RgbToHusl(FromByte(color.R), FromByte(color.G), FromByte(color.B));
            if (float.IsNaN(t.Item0)) t.Item0 = 0;
            if (float.IsNaN(t.Item1)) t.Item1 = 0;
            if (float.IsNaN(t.Item2)) t.Item2 = 0;
            return new HuslColor(t.Item0, t.Item1, t.Item2, FromByte(color.A));
        }

        private static Vec3Float RgbToHusl(float r, float g, float b)
        {
            return LCH_HUSL(LUV_LCH(XYZ_LUV(RGB_XYZ(new Vec3Float(r, g, b)))));
        }

        private static double PI = 3.1415926535897932384626433832795;
        private static Vec3Float[] m = {
                                         new Vec3Float(3.2406f, -1.5372f, -0.4986f),
                                         new Vec3Float(-0.9689f, 1.8758f, 0.0415f),
                                         new Vec3Float(0.0557f, -0.2040f, 1.0570f)
                                     };
        private static Vec3Float[] m_inv = {
                                             new Vec3Float(0.4124f, 0.3576f, 0.1805f),
                                             new Vec3Float(0.2126f, 0.7152f, 0.0722f),
                                             new Vec3Float(0.0193f, 0.1192f, 0.9505f)
                                         };
        // private static float refX = 0.95047f;
        private static float refY = 1.00000f;
        // private static float refZ = 1.08883f;
        private static float refU = 0.19784f;
        private static float refV = 0.46834f;
        private static float lab_e = 0.008856f;
        private static float lab_k = 903.3f;

        private static float maxChroma(float L, float H)
        {

            float C, bottom, cosH, hrad, lbottom, m1, m2, m3, rbottom, result, sinH, sub1, sub2, t, top;
            int _i, _j, _len, _len1;
            Vec3Float row;
            var _ref = new[] { 0.0f, 1.0f };


            hrad = (float)((H / 360.0f) * 2 * PI);
            sinH = (float)(Math.Sin(hrad));
            cosH = (float)(Math.Cos(hrad));
            sub1 = (float)(Math.Pow(L + 16, 3) / 1560896.0);
            sub2 = sub1 > 0.008856 ? sub1 : (float)(L / 903.3);
            result = float.PositiveInfinity;
            for (_i = 0, _len = 3; _i < _len; ++_i)
            {
                row = m[_i];
                m1 = row.Item0;
                m2 = row.Item1;
                m3 = row.Item2;
                top = (float)((0.99915 * m1 + 1.05122 * m2 + 1.14460 * m3) * sub2);
                rbottom = (float)(0.86330 * m3 - 0.17266 * m2);
                lbottom = (float)(0.12949 * m3 - 0.38848 * m1);
                bottom = (rbottom * sinH + lbottom * cosH) * sub2;

                for (_j = 0, _len1 = 2; _j < _len1; ++_j)
                {
                    t = _ref[_j];
                    C = (float)(L * (top - 1.05122 * t) / (bottom + 0.17266 * sinH * t));
                    if ((C > 0 && C < result))
                    {
                        result = C;
                    }
                }
            }
            return result;
        }

        private static float dotProduct(Vec3Float a, Vec3Float b, int len)
        {

            int i, _i, _ref;
            float ret = 0.0f;
            for (i = _i = 0, _ref = len - 1; 0 <= _ref ? _i <= _ref : _i >= _ref; i = 0 <= _ref ? ++_i : --_i)
            {
                ret += a[i] * b[i];
            }
            return ret;

        }

        private static float round(float num, int places)
        {
            float n;
            n = (float)(Math.Pow(10.0f, places));
            return (float)(Math.Floor(num * n) / n);
        }

        private static float f(float t)
        {
            if (t > lab_e)
            {
                return (float)(Math.Pow(t, 1.0f / 3.0f));
            }
            else
            {
                return (float)(7.787 * t + 16 / 116.0);
            }
        }

        private static float f_inv(float t)
        {
            if (Math.Pow(t, 3) > lab_e)
            {
                return (float)(Math.Pow(t, 3));
            }
            else
            {
                return (116 * t - 16) / lab_k;
            }
        }

        private static float fromLinear(float c)
        {
            if (c <= 0.0031308)
            {
                return 12.92f * c;
            }
            else
            {
                return (float)(1.055 * Math.Pow(c, 1 / 2.4f) - 0.055);
            }
        }

        private static float toLinear(float c)
        {
            float a = 0.055f;

            if (c > 0.04045)
            {
                return (float)(Math.Pow((c + a) / (1 + a), 2.4f));
            }
            else
            {
                return (float)(c / 12.92);
            }
        }

        private static Vec3Float rgbPrepare(Vec3Float tuple)
        {
            int i;

            for (i = 0; i < 3; ++i)
            {
                tuple[i] = round(tuple[i], 3);

                if (tuple[i] < 0 || tuple[i] > 1)
                {
                    if (tuple[i] < 0)
                        tuple[i] = 0;
                    else
                        tuple[i] = 1;
                    //System.out.println("Illegal rgb value: " + tuple[i]);
                }

                tuple[i] = round(tuple[i] * 255, 0);
            }

            return tuple;
        }

        private static Vec3Float XYZ_RGB(Vec3Float tuple)
        {
            float B, G, R;
            R = fromLinear(dotProduct(m[0], tuple, 3));
            G = fromLinear(dotProduct(m[1], tuple, 3));
            B = fromLinear(dotProduct(m[2], tuple, 3));

            tuple.Item0 = R;
            tuple.Item1 = G;
            tuple.Item2 = B;

            return tuple;
        }

        private static Vec3Float RGB_XYZ(Vec3Float tuple)
        {
            float B, G, R, X, Y, Z;
            Vec3Float rgbl;

            R = tuple.Item0;
            G = tuple.Item1;
            B = tuple.Item2;

            rgbl.Item0 = toLinear(R);
            rgbl.Item1 = toLinear(G);
            rgbl.Item2 = toLinear(B);

            X = dotProduct(m_inv[0], rgbl, 3);
            Y = dotProduct(m_inv[1], rgbl, 3);
            Z = dotProduct(m_inv[2], rgbl, 3);

            tuple.Item0 = X;
            tuple.Item1 = Y;
            tuple.Item2 = Z;

            return tuple;
        }

        private static Vec3Float XYZ_LUV(Vec3Float tuple)
        {
            float L, U, V, X, Y, Z, varU, varV;

            X = tuple.Item0;
            Y = tuple.Item1;
            Z = tuple.Item2;

            varU = (4 * X) / (X + (15.0f * Y) + (3 * Z));
            varV = (9 * Y) / (X + (15.0f * Y) + (3 * Z));
            L = 116 * f(Y / refY) - 16;
            U = 13 * L * (varU - refU);
            V = 13 * L * (varV - refV);

            tuple.Item0 = L;
            tuple.Item1 = U;
            tuple.Item2 = V;

            return tuple;
        }

        private static Vec3Float LUV_XYZ(Vec3Float tuple)
        {
            float L, U, V, X, Y, Z, varU, varV, varY;

            L = tuple.Item0;
            U = tuple.Item1;
            V = tuple.Item2;

            if (L == 0)
            {
                tuple.Item2 = tuple.Item1 = tuple.Item0 = 0.0f;
                return tuple;
            }

            varY = f_inv((L + 16) / 116.0f);
            varU = U / (13.0f * L) + refU;
            varV = V / (13.0f * L) + refV;
            Y = varY * refY;
            X = 0 - (9 * Y * varU) / ((varU - 4.0f) * varV - varU * varV);
            Z = (9 * Y - (15 * varV * Y) - (varV * X)) / (3.0f * varV);

            tuple.Item0 = X;
            tuple.Item1 = Y;
            tuple.Item2 = Z;

            return tuple;
        }

        private static Vec3Float LUV_LCH(Vec3Float tuple)
        {
            float C, H, Hrad, L, U, V;

            L = tuple.Item0;
            U = tuple.Item1;
            V = tuple.Item2;

            C = (float)(Math.Pow(Math.Pow(U, 2) + Math.Pow(V, 2), (1 / 2.0f)));
            Hrad = (float)(Math.Atan2(V, U));
            H = (float)(Hrad * 360.0f / 2.0f / PI);
            if (H < 0)
            {
                H = 360 + H;
            }

            tuple.Item0 = L;
            tuple.Item1 = C;
            tuple.Item2 = H;

            return tuple;
        }

        private static Vec3Float LCH_LUV(Vec3Float tuple)
        {
            float C, H, Hrad, L, U, V;

            L = tuple.Item0;
            C = tuple.Item1;
            H = tuple.Item2;

            Hrad = (float)(H / 360.0 * 2.0 * PI);
            U = (float)(Math.Cos(Hrad) * C);
            V = (float)(Math.Sin(Hrad) * C);

            tuple.Item0 = L;
            tuple.Item1 = U;
            tuple.Item2 = V;

            return tuple;
        }

        private static Vec3Float HUSL_LCH(Vec3Float tuple)
        {
            float C, H, L, S, max;

            H = tuple.Item0;
            S = tuple.Item1;
            L = tuple.Item2;

            max = maxChroma(L, H);
            C = max / 100.0f * S;

            tuple.Item0 = L;
            tuple.Item1 = C;
            tuple.Item2 = H;

            return tuple;
        }

        private static Vec3Float LCH_HUSL(Vec3Float tuple)
        {
            float C, H, L, S, max;

            L = tuple.Item0;
            C = tuple.Item1;
            H = tuple.Item2;

            max = maxChroma(L, H);
            S = C / max * 100;

            tuple.Item0 = H / 360;
            tuple.Item1 = S / 100;
            tuple.Item2 = L / 100;

            return tuple;
        }

    }
}