using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Templates
{
    public abstract class DisposableObject : IDisposable
    {
        protected bool m_Disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        public override string ToString()
        {
            if (m_Disposed)
            {
                return "Null";
            }

            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            if (m_Disposed && ReferenceEquals(obj, null))
            {
                return true;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DisposableObject left, object right)
        {
            if ((ReferenceEquals(left, null) || left.m_Disposed) && ReferenceEquals(right, null))
            {
                return true;
            }

            return Equals(left, right);
        }

        public static bool operator !=(DisposableObject left, object right)
        {
            if ((ReferenceEquals(left, null) || left.m_Disposed) && ReferenceEquals(right, null))
            {
                return false;
            }

            return !Equals(left, right);
        }
    }
}