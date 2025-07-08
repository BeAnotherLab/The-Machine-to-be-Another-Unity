using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Byn.Awrtc.Unity
{
    /// <summary>
    /// Unity helper functions that are used in multiple parts of the asset
    /// </summary>
    public static class UnityHelper
    {

        /// <summary>
        /// WARNING: This function is inherently unsafe and can cause crashes easily.
        /// It pins an Color32 array returned by unity and gives the IntPtr to the
        /// raw memory to a callback. It makes sure the memory is released.
        /// Avoid this method whenever possible.
        /// 
        /// If you end up here investigating a crash the reason is likely here or in the
        /// callback being called.
        /// </summary>
        /// <param name="arg">Array of Color32 structs</param>
        /// <param name="callback">Callback receiving the IntPtr to the raw memory and its byte length
        /// </param>
        public static void PtrFromColor32(Color32[] arg, Action<IntPtr, uint> callback)
        {
            GCHandle handle = GCHandle.Alloc(arg, GCHandleType.Pinned);
            try
            {
                uint structSize = (uint)Marshal.SizeOf(typeof(Color32));
                uint size = ((uint)arg.Length) * structSize;
                callback(handle.AddrOfPinnedObject(), size);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
