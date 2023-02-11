using engine;
using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Game_Engine.Shaders
{
    public class ShaderManager
    {
        public static void Test(IntPtr texture)
        {

            int w;
            int h;
            int pitch;
            int bpp; // bytes per pixel
            uint Rmask;
            uint Gmask;
            uint Bmask;
            uint Amask;

            int access;
            uint format;
            IntPtr pixels;
            IntPtr IntPtrFormat;

            SDL.SDL_QueryTexture(texture, out format, out access, out w, out h);
            SDL.SDL_LockTexture(texture, IntPtr.Zero, out pixels, out pitch);

            IntPtr surface = SDL.SDL_CreateRGBSurfaceWithFormatFrom(pixels, w, h, 32, pitch, format);

            unsafe
            {
                SDL.SDL_Surface* surface_surf = (SDL.SDL_Surface*)surface;
                IntPtrFormat = surface_surf->format;
            }

            byte bytesPerPixel = SDL.SDL_BYTESPERPIXEL(format);

            //IntPtr formatIntPtr = IntPtr.Parse(format.ToString()); // a maybe working way of getting format into IntPtr form

            SDL.SDL_PixelFormatEnumToMasks(format, out bpp, out Rmask, out Gmask, out Bmask, out Amask);
            
            //SDL.SDL_memcpy // if i need to copy one part of memory to another

            //int first = pixels.

            int index = 0;
            int y = 0;
            while (y < w)
            {
                int x = 0;
                while (x < h)
                {
                    uint pixel = 0; // default val - not great :(

                    int offset = bytesPerPixel*(y * w + x);
                    
                    switch (bytesPerPixel)
                    {
                        case 1:
                            pixel = Marshal.ReadByte(IntPtr.Add(pixels, offset));
                            break;
                        case 2:
                            pixel = (uint)Marshal.ReadInt16(IntPtr.Add(pixels, offset));
                            break;
                        case 3:
                            Console.WriteLine("SHADER EDGE CASE! Bytes per pixel is 3");
                            break;
                        case 4:
                            pixel = (uint)Marshal.ReadInt32(IntPtr.Add(pixels, offset));
                            break;
                    }

                    byte r;
                    byte g;
                    byte b;
                    byte a;

                    SDL.SDL_GetRGBA(pixel, IntPtrFormat, out r, out g, out b, out a);
                    SDL.SDL_SetRenderDrawColor(EngineManager.booterInstance.instance.Renderer, r, g, b, a);
                    SDL.SDL_RenderDrawPoint(EngineManager.booterInstance.instance.Renderer, x, y);
                    x++;
                    index++;
                }
                y++;
            }

            SDL.SDL_UnlockTexture(texture);

            //SDL.SDL_RenderReadPixelsEngineManager.booterInstance.instance.Renderer, texture);
            //SDL.SDL_
            //SDL.SDL_GetRGBA()
        }

        /*
        public static void Apply_FlipX(string imageFilePath)
        {
            Bitmap image = (Bitmap)Bitmap.FromFile(imageFilePath); // load bitmap image
            Bitmap result = new Bitmap(image.Width, image.Height);
            
            int x = 0;
            while (x < image.Width - 1)
            {
                int y = 0;
                while (x < image.Height - 1)
                {
                    result.SetPixel(x, y, image.GetPixel(image.Width-x, y));
                    y++;
                }
                x++;
            }

            result.SaveAdd(new EncoderParameters());
        }
        */
    }
}
