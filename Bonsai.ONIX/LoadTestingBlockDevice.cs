﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.ONIX
{
    [Description("Variable load testing device, block version")]
    public class LoadTestingBlockDevice : ONIFrameReader<LoadTestingBlockDataFrame, ushort>
    {
        enum Register
        {
            ENABLE = 0,
            CLK_DIV = 1,  // Heartbeat clock divider ratio. Default results in 10 Hz heartbeat. Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
            CLK_HZ = 2, // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV
            FRAME_WORDS = 3, // Number of repetitions of 16-bit unsigned integer 42 sent with each frame. Note: max here depends of CLK_HZ and CLK_DIV. 
                             //There needs to be enough clock cycles CLK_HZ / CLK_DIV >= FRAME_WORDS + NO_TIMER_WORDS (4 usually) + 1. Going
                             // above this will result in _decreased_ bandwidth as samples will be skipped.
        }
        public LoadTestingBlockDevice() : base(ONIXDevices.ID.LoadTest) { }

        protected override IObservable<LoadTestingBlockDataFrame> Process(IObservable<ONIManagedFrame<ushort>> source)
        {
            return source
                .Buffer(BlockSize)
                .Select(block => { return new LoadTestingBlockDataFrame(block, (int)FrameWords); });
        }

        [Category("Configuration")]
        [Range(1, 10000)]
        [Description("The number of frames making up a single data block that is propagated in the observable sequence.")]
        public int BlockSize { get; set; } = 10;

        [Category("Configuration")]
        [Description("Enable the device data stream.")]
        public bool Enable
        {
            get
            {
                return ReadRegister((uint)Register.ENABLE) > 0;
            }
            set
            {
                WriteRegister((uint)Register.ENABLE, value ? (uint)1 : 0);
            }
        }

        [Category("Configuration")]
        [Description("Number of repetitions of the 16-bit unsigned integer 42 sent with each frame.")]
        [Range(0, 10e6)]
        public uint FrameWords
        {
            get
            {
                return ReadRegister((int)Register.FRAME_WORDS);
            }
            set
            {
                var max_size = ValidSize();
                var bounded = value > max_size ? max_size : value;
                WriteRegister((int)Register.FRAME_WORDS, bounded);
            }
        }

        [Category("Acquisition")]
        [Description("Rate at frames are produced.")]
        public uint FrameHz
        {
            get
            {
                var val = ReadRegister((int)Register.CLK_DIV);
                return ReadRegister((int)Register.CLK_HZ) / val;
            }
            set
            {
                WriteRegister((int)Register.CLK_DIV, ReadRegister((int)Register.CLK_HZ) / value);
                var max_size = ValidSize();
                if (FrameWords > max_size)
                {
                    FrameWords = max_size;
                }
            }
        }

        // Assumes 8-byte timer
        uint ValidSize()
        {
            var clk_div = ReadRegister((int)Register.CLK_DIV);
            return clk_div - 4 - 10; // -10 is overhead hack
        }
    }
}
