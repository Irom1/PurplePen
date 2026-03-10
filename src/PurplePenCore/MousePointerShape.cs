using System;
using System.Collections.Generic;
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
    }

    // Predefined mouse pointer shapes.
    public enum PredefinedMousePointerShape
    {
        None,
        Arrow,
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
    }
}
