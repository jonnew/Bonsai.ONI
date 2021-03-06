﻿using OpenCV.Net;
using System.Collections.Generic;

namespace Bonsai.ONIX
{
    /// <summary>
    /// Base class for buffered groups of data frames using <see cref="ushort"/> as
    /// the underlying data type.
    /// </summary>
    public class U16DataBlockFrame
    {
        public U16DataBlockFrame(IList<ONIManagedFrame<ushort>> frameBlock)
        {
            var frameClock = new ulong[frameBlock.Count];
            var dataClock = new ulong[frameBlock.Count];

            for (int i = 0; i < frameBlock.Count; i++)
            {
                frameClock[i] = frameBlock[i].FrameClock;
                dataClock[i] = ((ulong)frameBlock[i].Sample[0] << 48) |
                               ((ulong)frameBlock[i].Sample[1] << 32) |
                               ((ulong)frameBlock[i].Sample[2] << 16) |
                               ((ulong)frameBlock[i].Sample[3] << 0);
            }


            FrameClock = GetClock(frameClock);
            DataClock = GetClock(dataClock);
        }

        public U16DataBlockFrame(IList<ONIManagedFrame<short>> frameBlock)
        {
            var frameClock = new ulong[frameBlock.Count];
            var dataClock = new ulong[frameBlock.Count];

            for (int i = 0; i < frameBlock.Count; i++)
            {
                frameClock[i] = frameBlock[i].FrameClock;
                dataClock[i] = ((ulong)frameBlock[i].Sample[0] << 48) |
                               ((ulong)frameBlock[i].Sample[1] << 32) |
                               ((ulong)frameBlock[i].Sample[2] << 16) |
                               ((ulong)frameBlock[i].Sample[3] << 0);
            }


            FrameClock = GetClock(frameClock);
            DataClock = GetClock(dataClock);
        }

        // TODO: This copies!
        Mat GetClock(ulong[] data)
        {
            // TODO: abusing double to fit ulong
            // NB: OpenCV does not have a Depth.U64 which would allow to user Mat.Header and Mat.Convert for zero-copy
            return Mat.FromArray(data, 1, data.Length, Depth.F64, 1);
        }

        /// <summary>
        /// The sample clock, create locally alongside the source device.
        /// </summary>
        public Mat DataClock { get; private set; }

        /// <summary>
        /// The frame clock. Created by the host when receiving the sample from the device.
        /// </summary>
        public Mat FrameClock { get; private set; }
    }
}
