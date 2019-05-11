﻿using System;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Memory;
using SDL2;

class Program
{
    unsafe static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        using (var pspEmulator = new PspEmulator())
        {
            //pspEmulator.StartAndLoad("minifire.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("counter.elf", GuiRunner: (emulator) =>
            pspEmulator.StartAndLoad("HelloWorldPSP.pbp", GuiRunner: (emulator) =>
            {
                Console.WriteLine("Hello World!");
                if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
                {
                    Console.Error.WriteLine("Couldn't initialize SDL");
                    return;
                }

                var window = SDL.SDL_CreateWindow(
                    "",
                    SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                    480 * 2, 272 * 2,
                    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
                );
                SDL.SDL_SetWindowTitle(window, "C# PSP Emulator");
                var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
                /*
                var surface = SDL.SDL_CreateRGBSurface(0, 480, 272, 32, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);

                SDL.SDL_LockSurface(surface);
                var surfaceInfo = (SDL.SDL_Surface*) surface.ToPointer();
                var pixels = (uint*) surfaceInfo->pixels.ToPointer();
                for (var n = 0; n < 480 * 272; n++)
                {
                    pixels[n] = 0x0000FF00;
                }
                SDL.SDL_UnlockSurface(surface);
                */

                //SDL.SDL_FillRect(SDL.SDL_Rect)
                var texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888, (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 512, 272);
                //var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
                
                //var surface = SDL.SDL_GetWindowSurface(window);
                //var renderer = SDL.SDL_CreateSoftwareRenderer(surface);
                SDL.SDL_Event e;
                var running = true;

                var display = emulator.InjectContext.GetInstance<PspDisplay>();
                var memory = emulator.InjectContext.GetInstance<PspMemory>();

                //var image = SDL_image.IMG_Load("icon0.png");
                //var texture = SDL.SDL_CreateTextureFromSurface(renderer, image);
                while (running)
                {
                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        switch (e.type)
                        {
                            case SDL.SDL_EventType.SDL_QUIT:
                                running = false;
                                break;
                            case SDL.SDL_EventType.SDL_KEYDOWN:
                                break;
                        }
                    }

                    /*
                    SDL.SDL_SetRenderDrawColor(renderer, 0xFF, (byte)n, (byte)n, 0xFF);
                    n++;
                    SDL.SDL_RenderClear(renderer);
                    SDL.SDL_UpdateWindowSurface(window);
                    */
                    
                    SDL.SDL_RenderClear(renderer);
                    {
                        //Console.WriteLine(display.CurrentInfo.FrameAddress);
                        var pixels2 = new uint[512 * 272];
                        var displayData = (uint*)memory.PspAddressToPointerSafe(display.CurrentInfo.FrameAddress);
                        for (var m = 0; m < 512 * 272; m++)
                        {
                            var color = displayData[m];
                            var r = BitUtils.Extract(color, 0, 8);
                            var g = BitUtils.Extract(color, 8, 8);
                            var b = BitUtils.Extract(color, 16, 8);
                            pixels2[m] = (r << 24) | (g << 16) | (b << 8) | 0xFF;
                        }

                        fixed (uint* pp = pixels2)
                        {
                            var rect = new SDL.SDL_Rect() {x = 0, y = 0, w = 512, h = 272};
                            SDL.SDL_UpdateTexture(texture, ref rect, new IntPtr(pp), 512 * 4);
                        }
                    }
                    SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                    SDL.SDL_RenderPresent(renderer);
                    
                    SDL.SDL_Delay(16);
                }
                //SDL.SDL_FreeSurface(image);

                SDL.SDL_DestroyTexture(texture);
                SDL.SDL_DestroyRenderer(renderer);
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
            });
        }
    }
}
