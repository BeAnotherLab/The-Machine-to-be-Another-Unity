using Byn.Awrtc;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Byn.Awrtc.Unity
{
    public class UnityMediaHelper
    {
#pragma warning disable 0618
        /// <summary>
        /// Will update an existing texture or recreate a texture based 
        /// on the raw frame given and format given.
        /// This call will destroy an existing texture object if the width, 
        /// height or format has changed.
        /// </summary>
        /// <param name="tex">A ref to a Texture2D. Will be created if null</param>
        /// <param name="frame">the raw frame to use to update the texture</param>
        /// <param name="format">Format of the raw frame</param>
        /// <returns>Returns true if a new texture object was created</returns>
        [Obsolete("Replaced with UpdateTexture using IFrame instead")]
        public static bool UpdateTexture(ref Texture2D tex, RawFrame frame, FramePixelFormat format)
        {
            bool newTextureCreated = false;
            //texture exists but has the wrong height /width? -> destroy it and set the value to null
            if (tex != null && (tex.width != frame.Width || tex.height != frame.Height))
            {
                Texture2D.Destroy(tex);
                tex = null;
            }
            //no texture? create a new one first
            if (tex == null)
            {
                newTextureCreated = true;
                //current default format for compatibility reasons
                if (format == FramePixelFormat.ABGR)
                {
                    tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
                }
                tex.wrapMode = TextureWrapMode.Clamp;
            }
            //copy image data into the texture and apply
            tex.LoadRawTextureData(frame.Buffer);
            tex.Apply();
            return newTextureCreated;
        }

#pragma warning restore 0618
        /// <summary>
        /// Updates a texture with a new IFrame. 
        /// Only ABGR (RGBA32 in unity) frames are properly supported at the moment.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static bool UpdateTexture(IFrame frame, ref Texture2D tex)
        {
            var format = frame.Format;
            if (frame.Format == FramePixelFormat.ABGR || frame.Format == FramePixelFormat.YUY2)
            {
                bool newTextureCreated = false;
                //texture exists but has the wrong height /width? -> destroy it and set the value to null
                if (tex != null && (tex.width != frame.Width || tex.height != frame.Height))
                {
                    Texture2D.Destroy(tex);
                    tex = null;
                }
                //no texture? create a new one first
                if (tex == null)
                {
                    newTextureCreated = true;
                    Debug.Log("Creating new texture with resolution " + frame.Width + "x" + frame.Height + " Format:" + format);

                    //so far only ABGR is really supported. this will change later
                    if (format == FramePixelFormat.ABGR)
                    {
                        tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
                    }
                    else
                    {
                        Debug.LogWarning("YUY2 texture is set. This is only for testing");
                        tex = new Texture2D(frame.Width, frame.Height, TextureFormat.YUY2, false);
                    }
                    tex.wrapMode = TextureWrapMode.Clamp;
                }
                //copy image data into the texture and apply
                //Watch out the RawImage has the top pixels in the top row but
                //unity has the top pixels in the bottom row. Result is an image that is
                //flipped. Fixing this here would waste a lot of CPU power thus
                //the UI will simply set scale.Y of the UI element to -1 to reverse this.
                tex.LoadRawTextureData(frame.Buffer);
                tex.Apply();
                return newTextureCreated;
            }
            else if (frame.Format == FramePixelFormat.I420p && frame is IDirectMemoryFrame)
            {
                //this one shouldn't be used. It squeezes all planes into a single texture 
                var dframe = frame as IDirectMemoryFrame;
                int texHeight = (int)(frame.Height * 1.5f);
                bool newTextureCreated = EnsureTex(frame.Width, texHeight, TextureFormat.R8, ref tex);
                tex.LoadRawTextureData(dframe.GetIntPtr(), dframe.GetSize());
                dframe.Dispose();
                tex.Apply();
                return newTextureCreated;
            }
            else
            {
                Debug.LogError("Format not supported");
                return false;
            }
        }


        /// <summary>
        /// Helper to ensure the texture has a fixed size and format.
        /// If not it will be created / destroyed and recreated
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="texFormat"></param>
        /// <param name="tex"></param>
        /// <returns>True if a new texture was created.
        /// Meaning external references need to be refreshed!
        /// </returns>
        private static bool EnsureTex(int width, int height, TextureFormat texFormat, ref Texture2D tex)
        {
            if (tex != null && (tex.width != width || tex.height != height))
            {
                Texture2D.Destroy(tex);
                tex = null;
            }
            if (tex == null)
            {
                tex = new Texture2D(width, height, texFormat, false);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This is a prototype! Do not use except for testing!
        /// Will update the texture based based on IDirectMemoryFrame.
        /// Only i420p supported!
        /// Note that this method can easily cause crashes if the memory
        /// isn't structured as expected.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="yplane"></param>
        /// <param name="uplane"></param>
        /// <param name="vplane"></param>
        /// <returns></returns>
        public static bool UpdateTexture(IDirectMemoryFrame frame, ref Texture2D yplane, ref Texture2D uplane, ref Texture2D vplane)
        {

            if (frame.Format == FramePixelFormat.I420p)
            {
                var dframe = frame as IDirectMemoryFrame;
                int width = frame.Width;
                int height = frame.Height;
                int hwidth = frame.Width / 2;
                int hheight = frame.Height / 2;
                TextureFormat texFormat = TextureFormat.R8;
                bool newTextureCreated = EnsureTex(width, height, texFormat, ref yplane);
                newTextureCreated |= EnsureTex(hwidth, hheight, texFormat, ref uplane);
                newTextureCreated |= EnsureTex(hwidth, hheight, texFormat, ref vplane);
                
                IntPtr ystart = dframe.GetIntPtr();
                long ylength = width * height;
                IntPtr ustart = new IntPtr(ystart.ToInt64() + ylength);
                long ulength = (hwidth * hheight);
                IntPtr vstart = new IntPtr(ustart.ToInt64() + ulength);

                yplane.LoadRawTextureData(ystart, (int)ylength);
                uplane.LoadRawTextureData(ustart, (int)ulength);
                vplane.LoadRawTextureData(vstart, (int)ulength);
                yplane.Apply();
                uplane.Apply();
                vplane.Apply();
                return newTextureCreated;
            }
            else
            {
                Debug.LogError("Format not supported");
                return false;
            }
        }

        /// <summary>
        /// Material used to show i420 image via Unitys RawImage
        /// </summary>
        public static readonly string I420_MAT_NAME = "wrtcI420Mat";

        /// <summary>
        /// U plane texture name
        /// </summary>
        public static readonly string I420_MAT_UPLANE = "_UPlane";

        /// <summary>
        /// V plane texture name
        /// </summary>
        public static readonly string I420_MAT_VPLANE = "_VPlane";

        /// <summary>
        /// Helper to update a RawImage based on IFrame.
        /// 
        /// Watch out this method might change the material and textures 
        /// used for the RawImage.
        /// 
        /// This method can be used in the future to support i420p using
        /// a specific shader / material / texture combination.
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static bool UpdateRawImage(RawImage target, IFrame frame)
        {
            if (target == null || frame == null)
                return false;


            if (frame.Format == FramePixelFormat.I420p
                && frame is IDirectMemoryFrame)
            {
                if (target.material.name != I420_MAT_NAME)
                {
                    Material mat = Resources.Load<Material>(I420_MAT_NAME);
                    target.material = new Material(mat);
                }

                //using the texture references in the shader if possible
                //makes it easier reusable
                Texture2D mainTex = target.texture as Texture2D;
                Texture2D uTex = target.material.GetTexture(I420_MAT_UPLANE) as Texture2D;
                Texture2D vTex = target.material.GetTexture(I420_MAT_VPLANE) as Texture2D;

                IDirectMemoryFrame dframe = frame as IDirectMemoryFrame;

                //update existing textures or create new ones
                bool textureCreated = UnityMediaHelper.UpdateTexture(dframe, ref mainTex, ref uTex, ref vTex);
                dframe.Dispose();
                dframe = null;
                if (textureCreated)
                {
                    //new textures where creates (e.g. due to resolution change)
                    //update the UI
                    target.texture = mainTex;
                    target.material.SetTexture(I420_MAT_UPLANE, uTex);
                    target.material.SetTexture(I420_MAT_VPLANE, vTex);
                }
                return textureCreated;
            }
            else
            {
                if (target.material != target.defaultMaterial)
                {
                    target.material = target.defaultMaterial;
                }
                Texture2D mainTex = target.texture as Texture2D;
                bool textureCreated = UnityMediaHelper.UpdateTexture(frame, ref mainTex);
                if (textureCreated)
                {
                    //helper had to create a new texture object -> refresh ui
                    target.texture = mainTex;
                }
                return textureCreated;
            }
        }
    }
}
