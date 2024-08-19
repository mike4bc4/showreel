using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Layers
{
    public struct Overscan
    {
        public float north;
        public float east;
        public float south;
        public float west;

        public Overscan(Vector4 vector)
        {
            north = vector.x;
            east = vector.y;
            south = vector.z;
            west = vector.w;
        }

        public Overscan(float north, float east, float south, float west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

        public Overscan(float pixels)
        {
            this.north = this.east = this.south = this.west = pixels;
        }

        public static implicit operator Vector4(Overscan os) => new Vector4(os.north, os.east, os.south, os.west);
        public static implicit operator Overscan(float value) => new Overscan(value);
    }
}
