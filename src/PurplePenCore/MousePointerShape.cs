using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // Represents that shape of the mouse pointer.
    public class MousePointerShape
    {
        private PredefinedMousePointerShape predefinedShape = PredefinedMousePointerShape.None;

        public MousePointerShape(PredefinedMousePointerShape predefinedShape)
        {
            this.predefinedShape = predefinedShape;
        }

        public PredefinedMousePointerShape PredefinedShape { get { return predefinedShape; } }

        override public bool Equals(object obj)
        {
            MousePointerShape other = obj as MousePointerShape;
            if (other == null)
                return false;
            return this.predefinedShape == other.predefinedShape;
        }

        public override int GetHashCode()
        {
            return this.predefinedShape.GetHashCode();
        }

        public static readonly MousePointerShape Arrow = new MousePointerShape(PredefinedMousePointerShape.Arrow);
        public static readonly MousePointerShape Default = new MousePointerShape(PredefinedMousePointerShape.Default);
        public static readonly MousePointerShape Cross = new MousePointerShape(PredefinedMousePointerShape.Cross);
        public static readonly MousePointerShape Hand = new MousePointerShape(PredefinedMousePointerShape.Hand);
        public static readonly MousePointerShape Help = new MousePointerShape(PredefinedMousePointerShape.Help);
        public static readonly MousePointerShape IBeam = new MousePointerShape(PredefinedMousePointerShape.IBeam);
        public static readonly MousePointerShape No = new MousePointerShape(PredefinedMousePointerShape.No);
        public static readonly MousePointerShape SizeAll = new MousePointerShape(PredefinedMousePointerShape.SizeAll);
        public static readonly MousePointerShape SizeNESW = new MousePointerShape(PredefinedMousePointerShape.SizeNESW);
        public static readonly MousePointerShape SizeNS = new MousePointerShape(PredefinedMousePointerShape.SizeNS);
        public static readonly MousePointerShape SizeNWSE = new MousePointerShape(PredefinedMousePointerShape.SizeNWSE);
        public static readonly MousePointerShape SizeWE = new MousePointerShape(PredefinedMousePointerShape.SizeWE);
        public static readonly MousePointerShape UpArrow = new MousePointerShape(PredefinedMousePointerShape.UpArrow);
        public static readonly MousePointerShape Wait = new MousePointerShape(PredefinedMousePointerShape.Wait);
        public static readonly MousePointerShape MoveHandle = new MousePointerShape(PredefinedMousePointerShape.MoveHandle);
        public static readonly MousePointerShape DeleteHandle = new MousePointerShape(PredefinedMousePointerShape.DeleteHandle);
    }

    // Predefined mouse pointer shapes.
    public enum PredefinedMousePointerShape
    {
        None,
        Arrow,
        Default = Arrow,
        Cross,
        Hand,
        Help,
        IBeam,
        No,
        SizeAll,
        SizeNESW,
        SizeNS,
        SizeNWSE,
        SizeWE,
        UpArrow,
        Wait,

        // These are custom shapes that Purple Pen uses,
        // not defined by the operating system. 
        MoveHandle,
        DeleteHandle,
        HandDrag
    }
}
