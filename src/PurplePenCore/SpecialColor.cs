using PurplePen.Graphics2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // Color of a special item: Black, Purple, White, or Custom.
    public class SpecialColor
    {
        public enum ColorKind { Black, UpperPurple, LowerPurple, Custom }

        public readonly ColorKind Kind;
        public readonly CmykColor CustomColor;

        public readonly static SpecialColor Black = new SpecialColor(ColorKind.Black);
        public readonly static SpecialColor UpperPurple = new SpecialColor(ColorKind.UpperPurple);
        public readonly static SpecialColor LowerPurple = new SpecialColor(ColorKind.LowerPurple);

        public SpecialColor(ColorKind colorKind)
        {
            Debug.Assert(colorKind != ColorKind.Custom);
            this.Kind = colorKind;
        }

        public SpecialColor(float cyan, float magenta, float yellow, float black)
        {
            this.Kind = ColorKind.Custom;
            this.CustomColor = CmykColor.FromCmyk(cyan, magenta, yellow, black);
        }

        public SpecialColor(CmykColor color)
        {
            this.Kind = ColorKind.Custom;
            this.CustomColor = color;
        }

        public override string ToString()
        {
            switch (Kind) {
            case ColorKind.Black: return "black";
            case ColorKind.UpperPurple: return "purple";
            case ColorKind.LowerPurple: return "lower-purple";
            case ColorKind.Custom: return string.Format(CultureInfo.InvariantCulture, "{0:F},{1:F},{2:F},{3:F}", CustomColor.Cyan, CustomColor.Magenta, CustomColor.Yellow, CustomColor.Black);
            default: return base.ToString();
            }
        }

        public static SpecialColor Parse(string s)
        {
            if (s == "black")
                return SpecialColor.Black;
            else if (s == "purple")
                return SpecialColor.UpperPurple;
            else if (s == "lower-purple")
                return SpecialColor.LowerPurple;
            else {
                float c, m, y, k;
                string[] colors = s.Split(',');
                if (colors.Length != 4)
                    throw new FormatException();
                c = float.Parse(colors[0], CultureInfo.InvariantCulture);
                m = float.Parse(colors[1], CultureInfo.InvariantCulture);
                y = float.Parse(colors[2], CultureInfo.InvariantCulture);
                k = float.Parse(colors[3], CultureInfo.InvariantCulture);
                return new SpecialColor(c, m, y, k);
            }
        }

        public override bool Equals(object obj)
        {
            SpecialColor other = obj as SpecialColor;
            if (other == null)
                return false;

            if (Kind != ColorKind.Custom)
                return Kind == other.Kind;
            else
                return (Kind == other.Kind && CustomColor.Equals(other.CustomColor));
        }

        public override int GetHashCode()
        {
            if (Kind != ColorKind.Custom)
                return Kind.GetHashCode();
            else
                return CustomColor.GetHashCode();
        }
    }

}
