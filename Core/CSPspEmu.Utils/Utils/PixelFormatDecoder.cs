﻿using System;
using CSharpUtils;
using CSharpUtils.Drawing;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Utils
{
    public unsafe sealed class PixelFormatDecoder
    {
        internal PixelFormatDecoder()
        {
        }

        public struct Dxt1Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
        }

        public struct Dxt3Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
            public ulong Alpha;
        }

        public struct Dxt5Block
        {
            public uint ColorLookup;
            public ushort Color0;
            public ushort Color1;
            public ulong Alpha;
        }

        static readonly double[] Sizes =
        {
            // Rgba
            2, 2, 2, 4,
            // Palette
            0.5f, 1, 2, 4,
            // Compressed
            1, 1, 1
        };

        public static int GetPixelsBits(GuPixelFormats PixelFormat)
        {
            return (int) (Sizes[(int) PixelFormat] * 8);
        }

        public static int GetPixelsSize(GuPixelFormats PixelFormat, int PixelCount)
        {
            return (int) (Sizes[(int) PixelFormat] * PixelCount);
        }

        void* _Input;
        byte* InputByte;
        ushort* InputShort;
        uint* InputInt;
        OutputPixel* Output;
        int Width;
        int Height;
        void* Palette;
        GuPixelFormats PaletteType;
        int PaletteCount;
        int PaletteStart;
        int PaletteShift;
        int PaletteMask;
        int StrideWidth;

        public static ColorFormat ColorFormatFromPixelFormat(GuPixelFormats PixelFormat)
        {
            switch (PixelFormat)
            {
                case GuPixelFormats.RGBA_8888: return ColorFormats.Rgba8888;
                case GuPixelFormats.RGBA_5551: return ColorFormats.Rgba5551;
                case GuPixelFormats.RGBA_5650: return ColorFormats.Rgba5650;
                case GuPixelFormats.RGBA_4444: return ColorFormats.Rgba4444;
                default: throw(new NotImplementedException("Not implemented " + PixelFormat));
            }
        }

        /*
        public static uint EncodePixel(GuPixelFormats PixelFormat, OutputPixel Color)
        {
            return ColorFormatFromPixelFormat(PixelFormat).Encode(Color.R, Color.G, Color.B, Color.A);
        }

        public static OutputPixel DecodePixel(GuPixelFormats PixelFormat, uint Value)
        {
            throw new NotImplementedException();
        }
        */

        public static void Decode(GuPixelFormats PixelFormat, void* Input, OutputPixel* Output, int Width, int Height,
            void* Palette = null, GuPixelFormats PaletteType = GuPixelFormats.NONE, int PaletteCount = 0,
            int PaletteStart = 0, int PaletteShift = 0, int PaletteMask = 0xFF, int StrideWidth = -1,
            bool IgnoreAlpha = false)
        {
            if (StrideWidth == -1) StrideWidth = GetPixelsSize(PixelFormat, Width);
            var PixelFormatInt = (int) PixelFormat;
            var PixelFormatDecoder = new PixelFormatDecoder()
            {
                _Input = Input,
                InputByte = (byte*) Input,
                InputShort = (ushort*) Input,
                InputInt = (uint*) Input,
                Output = Output,
                StrideWidth = StrideWidth,
                Width = Width,
                Height = Height,
                Palette = Palette,
                PaletteType = PaletteType,
                PaletteCount = PaletteCount,
                PaletteStart = PaletteStart,
                PaletteShift = PaletteShift,
                PaletteMask = PaletteMask,
            };
            //Console.WriteLine(PixelFormat);
            switch (PixelFormat)
            {
                case GuPixelFormats.RGBA_5650:
                    PixelFormatDecoder.Decode_RGBA_5650();
                    break;
                case GuPixelFormats.RGBA_5551:
                    PixelFormatDecoder.Decode_RGBA_5551();
                    break;
                case GuPixelFormats.RGBA_4444:
                    PixelFormatDecoder.Decode_RGBA_4444();
                    break;
                case GuPixelFormats.RGBA_8888:
                    PixelFormatDecoder.Decode_RGBA_8888();
                    break;
                case GuPixelFormats.PALETTE_T4:
                    PixelFormatDecoder.Decode_PALETTE_T4();
                    break;
                case GuPixelFormats.PALETTE_T8:
                    PixelFormatDecoder.Decode_PALETTE_T8();
                    break;
                case GuPixelFormats.PALETTE_T16:
                    PixelFormatDecoder.Decode_PALETTE_T16();
                    break;
                case GuPixelFormats.PALETTE_T32:
                    PixelFormatDecoder.Decode_PALETTE_T32();
                    break;
                case GuPixelFormats.COMPRESSED_DXT1:
                    PixelFormatDecoder.Decode_COMPRESSED_DXT1();
                    break;
                case GuPixelFormats.COMPRESSED_DXT3:
                    PixelFormatDecoder.Decode_COMPRESSED_DXT3();
                    break;
                case GuPixelFormats.COMPRESSED_DXT5:
                    PixelFormatDecoder.Decode_COMPRESSED_DXT5();
                    break;
                default: throw(new InvalidOperationException());
            }
            if (IgnoreAlpha)
            {
                for (int y = 0, n = 0; y < Height; y++) for (int x = 0; x < Width; x++, n++) Output[n].A = 0xFF;
            }
            //DecoderCallbackTable[PixelFormatInt](Input, Output, PixelCount, Width, Palette, PaletteType, PaletteCount, PaletteStart, PaletteShift, PaletteMask);
        }

        private unsafe void _Decode_Unimplemented()
        {
            for (int y = 0, n = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++, n++)
                {
                    OutputPixel OutputPixel;
                    OutputPixel.R = 0xFF;
                    OutputPixel.G = (byte) (((n & 1) == 0) ? 0xFF : 0x00);
                    OutputPixel.B = 0x00;
                    OutputPixel.A = 0xFF;
                    Output[n] = OutputPixel;
                }
            }
        }

        private unsafe void Decode_COMPRESSED_DXT5()
        {
            //Console.Error.WriteLine("Not Implemented: Decode_COMPRESSED_DXT5");
            //throw new NotImplementedException();

            //_Decode_Unimplemented();

            var Colors = new OutputPixel[4];

            for (int y = 0, ni = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4, ni++)
                {
                    var Block = ((Dxt5Block*) InputByte)[ni];
                    Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
                    Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
                    Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                        (a, b) => { return (byte) (((a * 2) / 3) + ((b * 1) / 3)); });
                    Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                        (a, b) => { return (byte) (((a * 1) / 3) + ((b * 2) / 3)); });

                    // Create Alpha Lookup
                    var AlphaLookup = new byte[8];
                    var Alphas = (ushort) (Block.Alpha >> 48);
                    var Alpha0 = (byte) ((Alphas >> 0) & 0xFF);
                    var Alpha1 = (byte) ((Alphas >> 8) & 0xFF);

                    AlphaLookup[0] = Alpha0;
                    AlphaLookup[1] = Alpha1;
                    if (Alpha0 > Alpha1)
                    {
                        AlphaLookup[2] = (byte) ((6 * Alpha0 + Alpha1) / 7);
                        AlphaLookup[3] = (byte) ((5 * Alpha0 + 2 * Alpha1) / 7);
                        AlphaLookup[4] = (byte) ((4 * Alpha0 + 3 * Alpha1) / 7);
                        AlphaLookup[5] = (byte) ((3 * Alpha0 + 4 * Alpha1) / 7);
                        AlphaLookup[6] = (byte) ((2 * Alpha0 + 5 * Alpha1) / 7);
                        AlphaLookup[7] = (byte) ((Alpha0 + 6 * Alpha1) / 7);
                    }
                    else
                    {
                        AlphaLookup[2] = (byte) ((4 * Alpha0 + Alpha1) / 5);
                        AlphaLookup[3] = (byte) ((3 * Alpha0 + 2 * Alpha1) / 5);
                        AlphaLookup[4] = (byte) ((2 * Alpha0 + 3 * Alpha1) / 5);
                        AlphaLookup[5] = (byte) ((Alpha0 + 4 * Alpha1) / 5);
                        AlphaLookup[6] = (byte) (0x00);
                        AlphaLookup[7] = (byte) (0xFF);
                    }

                    for (int y2 = 0, no = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++, no++)
                        {
                            var Alpha = AlphaLookup[((Block.Alpha >> (3 * no)) & 0x7)];
                            var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

                            int rx = (x + x2);
                            int ry = (y + y2);
                            int n = ry * Width + rx;

                            Output[n] = Colors[Color];
                            Output[n].A = Alpha;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// DXT2 and DXT3 (collectively also known as Block Compression 2 or BC2) converts 16 input pixels
        /// (corresponding to a 4x4 pixel block) into 128 bits of output, consisting of 64 bits of alpha channel data
        /// (4 bits for each pixel) followed by 64 bits of color data, encoded the same way as DXT1 (with the exception
        /// that the 4 color version of the DXT1 algorithm is always used instead of deciding which version to use based
        /// on the relative values of  and ). In DXT2, the color data is interpreted as being premultiplied by alpha, in
        /// DXT3 it is interpreted as not having been premultiplied by alpha. Typically DXT2/3 are well suited to images
        /// with sharp alpha transitions, between translucent and opaque areas.
        /// </summary>
        private unsafe void Decode_COMPRESSED_DXT3()
        {
            var Colors = new OutputPixel[4];

            for (int y = 0, ni = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4, ni++)
                {
                    var Block = ((Dxt3Block*) InputByte)[ni];
                    Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
                    Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
                    Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                        (a, b) => { return (byte) (((a * 2) / 3) + ((b * 1) / 3)); });
                    Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                        (a, b) => { return (byte) (((a * 1) / 3) + ((b * 2) / 3)); });

                    for (int y2 = 0, no = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++, no++)
                        {
                            var Alpha = ((Block.Alpha >> (4 * no)) & 0xF);
                            var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

                            int rx = (x + x2);
                            int ry = (y + y2);
                            int n = ry * Width + rx;

                            Output[n] = Colors[Color];
                            Output[n].A = (byte) ((Alpha * 0xFF) / 0xF);
                        }
                    }
                }
            }
        }

        private unsafe void Decode_COMPRESSED_DXT1()
        {
            var Colors = new OutputPixel[4];

            for (int y = 0, ni = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4, ni++)
                {
                    var Block = ((Dxt1Block*) InputByte)[ni];

                    Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
                    Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1)
                        .Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));

                    if (Block.Color0 > Block.Color1)
                    {
                        Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                            (a, b) => { return (byte) (((a * 2) / 3) + ((b * 1) / 3)); });
                        Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                            (a, b) => { return (byte) (((a * 1) / 3) + ((b * 2) / 3)); });
                    }
                    else
                    {
                        Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1],
                            (a, b) => { return (byte) (((a * 1) / 2) + ((b * 1) / 2)); });
                        Colors[3] = OutputPixel.FromRGBA(0, 0, 0, 0);
                    }

                    for (int y2 = 0, no = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++, no++)
                        {
                            var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

                            int rx = (x + x2);
                            int ry = (y + y2);
                            int n = ry * Width + rx;

                            Output[n] = Colors[Color];
                        }
                    }
                }
            }
        }

        private unsafe void Decode_PALETTE_T32()
        {
            throw new NotImplementedException("Decode_PALETTE_T32");
        }

        private unsafe void Decode_PALETTE_T16()
        {
            throw new NotImplementedException("Decode_PALETTE_T16");
        }

        private unsafe void Decode_PALETTE_T8()
        {
            var Input = (byte*) _Input;

            int PaletteSize = 256;
            OutputPixel[] PalettePixels = new OutputPixel[PaletteSize];
            var Translate = new int[PaletteSize];

            if (Palette == null || PaletteType == GuPixelFormats.NONE)
            {
                for (int y = 0, n = 0; y < Height; y++)
                {
                    var InputRow = (byte*) &InputByte[y * StrideWidth];
                    for (int x = 0; x < Width; x++, n++)
                    {
                        Output[n] = PalettePixels[0];
                    }
                }
            }
            else
            {
                fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
                {
                    Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
                    //Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
                }
                for (int n = 0; n < PaletteSize; n++)
                {
                    Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
                }

                for (int y = 0, n = 0; y < Height; y++)
                {
                    var InputRow = (byte*) &InputByte[y * StrideWidth];
                    for (int x = 0; x < Width; x++, n++)
                    {
                        byte Value = InputRow[x];
                        Output[n] = PalettePixels[Translate[(Value >> 0) & 0xFF]];
                    }
                }
            }
        }

        private unsafe void Decode_PALETTE_T4()
        {
            var Input = (byte*) _Input;

            if (Palette == null || PaletteType == GuPixelFormats.NONE)
            {
                Console.WriteLine("Palette required!");
                return;
            }

            OutputPixel[] PalettePixels;
            int PaletteSize = 256;
            PalettePixels = new OutputPixel[PaletteSize];
            var Translate = new int[PaletteSize];
            fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
            {
                Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
                //Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
            }
            //Console.WriteLine(PalettePixels.Length);
            for (int n = 0; n < 16; n++)
            {
                Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
                //Console.WriteLine(PalettePixels[Translate[n]]);
            }

            for (int y = 0, n = 0; y < Height; y++)
            {
                var InputRow = (byte*) &InputByte[y * StrideWidth];
                for (int x = 0; x < Width / 2; x++, n++)
                {
                    byte Value = InputRow[x];
                    Output[n * 2 + 0] = PalettePixels[Translate[(Value >> 0) & 0xF]];
                    Output[n * 2 + 1] = PalettePixels[Translate[(Value >> 4) & 0xF]];
                }
            }
        }

        private unsafe void _Decode_RGBA_XXXX_uint(Func<uint, OutputPixel> DecodePixel)
        {
            var Input = (uint*) _Input;

            for (int y = 0, n = 0; y < Height; y++)
            {
                var InputRow = (uint*) &InputByte[y * StrideWidth];
                for (int x = 0; x < Width; x++, n++)
                {
                    Output[n] = DecodePixel(InputRow[x]);
                }
            }
        }

        private unsafe void _Decode_RGBA_XXXX_ushort(Func<ushort, OutputPixel> DecodePixel)
        {
            var Input = (uint*) _Input;

            for (int y = 0, n = 0; y < Height; y++)
            {
                var InputRow = (ushort*) &InputByte[y * StrideWidth];
                for (int x = 0; x < Width; x++, n++)
                {
                    Output[n] = DecodePixel(InputRow[x]);
                }
            }
        }

        private unsafe void Decode_RGBA_8888()
        {
            _Decode_RGBA_XXXX_uint(Decode_RGBA_8888_Pixel);
        }

        private unsafe void Decode_RGBA_4444()
        {
            _Decode_RGBA_XXXX_ushort(Decode_RGBA_4444_Pixel);
        }

        private unsafe void Decode_RGBA_5551()
        {
            _Decode_RGBA_XXXX_ushort(Decode_RGBA_5551_Pixel);
        }

        private unsafe void Decode_RGBA_5650()
        {
            _Decode_RGBA_XXXX_ushort(Decode_RGBA_5650_Pixel);
        }

        public static unsafe ushort Encode_RGBA_4444_Pixel(OutputPixel Pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 4, Pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 4, 4, Pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 8, 4, Pixel.B, 255);
            BitUtils.InsertScaled(ref Out, 12, 4, Pixel.A, 255);
            return (ushort) Out;
        }

        public static unsafe ushort Encode_RGBA_5551_Pixel(OutputPixel Pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 5, Pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 5, 5, Pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 10, 5, Pixel.B, 255);
            BitUtils.InsertScaled(ref Out, 15, 1, Pixel.A, 255);
            return (ushort) Out;
        }

        public static unsafe ushort Encode_RGBA_5650_Pixel(OutputPixel Pixel)
        {
            uint Out = 0;
            BitUtils.InsertScaled(ref Out, 0, 5, Pixel.R, 255);
            BitUtils.InsertScaled(ref Out, 5, 6, Pixel.G, 255);
            BitUtils.InsertScaled(ref Out, 11, 5, Pixel.B, 255);
            return (ushort) Out;
        }

        public static unsafe uint Encode_RGBA_8888_Pixel(OutputPixel Pixel)
        {
            return *(uint*) &Pixel;
        }

        public static unsafe OutputPixel Decode_RGBA_4444_Pixel(ushort Value)
        {
            return new OutputPixel()
            {
                R = (byte) BitUtils.ExtractScaled(Value, 0, 4, 255),
                G = (byte) BitUtils.ExtractScaled(Value, 4, 4, 255),
                B = (byte) BitUtils.ExtractScaled(Value, 8, 4, 255),
                A = (byte) BitUtils.ExtractScaled(Value, 12, 4, 255),
            };
        }

        public static unsafe OutputPixel Decode_RGBA_5551_Pixel(ushort Value)
        {
            return new OutputPixel()
            {
                R = (byte) BitUtils.ExtractScaled(Value, 0, 5, 255),
                G = (byte) BitUtils.ExtractScaled(Value, 5, 5, 255),
                B = (byte) BitUtils.ExtractScaled(Value, 10, 5, 255),
                A = (byte) BitUtils.ExtractScaled(Value, 15, 1, 255),
            };
        }

        public static unsafe OutputPixel Decode_RGBA_5650_Pixel(ushort Value)
        {
            return new OutputPixel()
            {
                R = (byte) BitUtils.ExtractScaled(Value, 0, 5, 255),
                G = (byte) BitUtils.ExtractScaled(Value, 5, 6, 255),
                B = (byte) BitUtils.ExtractScaled(Value, 11, 5, 255),
                A = 0xFF,
            };
        }

        public static unsafe OutputPixel Decode_RGBA_8888_Pixel(uint Value)
        {
            return *(OutputPixel*) &Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Output"></param>
        /// <param name="RowWidth">Width of the texture. In bytes? In pixels? Maybe bytes?</param>
        /// <param name="TextureHeight">Height of the texture</param>
        public static unsafe void Unswizzle(byte[] Input, byte[] Output, int RowWidth, int TextureHeight)
        {
            fixed (void* InputPtr = Input)
            fixed (void* OutputPtr = Output)
            {
                Unswizzle(InputPtr, OutputPtr, RowWidth, TextureHeight);
            }
        }

        public static unsafe void Unswizzle(void* Input, void* Output, int RowWidth, int TextureHeight)
        {
            int pitch = (RowWidth - 16) / 4;
            int bxc = RowWidth / 16;
            int byc = TextureHeight / 8;

            var src = (uint*) Input;
            var ydest = (byte*) Output;
            for (int by = 0; by < byc; by++)
            {
                var xdest = ydest;
                for (int bx = 0; bx < bxc; bx++)
                {
                    var dest = (uint*) xdest;
                    for (int n = 0; n < 8; n++, dest += pitch)
                    {
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                        *(dest++) = *(src++);
                    }
                    xdest += 16;
                }
                ydest += RowWidth * 8;
            }
        }

        public static unsafe void UnswizzleInline(void* Data, int RowWidth, int TextureHeight)
        {
            var Temp = new byte[RowWidth * TextureHeight];
            fixed (void* TempPointer = Temp)
            {
                Unswizzle(Data, TempPointer, RowWidth, TextureHeight);
            }
            PointerUtils.Memcpy((byte*) Data, Temp, RowWidth * TextureHeight);
        }

        public static unsafe void UnswizzleInline(GuPixelFormats Format, void* Data, int Width, int Height)
        {
            UnswizzleInline(Data, GetPixelsSize(Format, Width), Height);
        }

        public static unsafe ulong Hash(GuPixelFormats PixelFormat, void* Input, int Width, int Height)
        {
            int TotalBytes = GetPixelsSize(PixelFormat, Width * Height);

            return Hashing.FastHash((byte*) Input, TotalBytes, (ulong) ((int) PixelFormat * Width * Height));
        }

        public static void Encode(GuPixelFormats GuPixelFormat, OutputPixel* Input, byte* _Output, int Count)
        {
            switch (GuPixelFormat)
            {
                case GuPixelFormats.RGBA_8888:
                {
                    var Output = (uint*) _Output;
                    for (int n = 0; n < Count; n++) *Output++ = Encode_RGBA_8888_Pixel(*Input++);
                }
                    break;
                case GuPixelFormats.RGBA_5551:
                {
                    var Output = (ushort*) _Output;
                    for (int n = 0; n < Count; n++) *Output++ = Encode_RGBA_5551_Pixel(*Input++);
                }
                    break;
                case GuPixelFormats.RGBA_5650:
                {
                    var Output = (ushort*) _Output;
                    for (int n = 0; n < Count; n++) *Output++ = Encode_RGBA_5650_Pixel(*Input++);
                }
                    break;
                case GuPixelFormats.RGBA_4444:
                {
                    var Output = (ushort*) _Output;
                    for (int n = 0; n < Count; n++) *Output++ = Encode_RGBA_4444_Pixel(*Input++);
                }
                    break;
                default:
                    throw(new NotImplementedException("Not implemented " + GuPixelFormat));
            }
        }

        public static void Encode(GuPixelFormats GuPixelFormat, OutputPixel* Input, byte* Output, int BufferWidth,
            int Width, int Height)
        {
            var IncOut = GetPixelsSize(GuPixelFormat, BufferWidth);

            for (int y = 0; y < Height; y++)
            {
                Encode(GuPixelFormat, Input, Output, Width);
                Input += Width;
                Output += IncOut;
            }
        }
    }
}