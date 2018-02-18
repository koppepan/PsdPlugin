/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
// http://psdplugin.codeplex.com/
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2016 Tao Yue
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using UnityEngine;

namespace PhotoshopFile.Compression
{
    public class ZipPredict32Image : ImageData
    {
        private ZipImage zipImage;

        protected override bool AltersWrittenData
        {
            // Prediction will pack the data into a temporary buffer, so the
            // original data will remain unchanged.
            get { return false; }
        }

        public ZipPredict32Image(byte[] zipData, Vector2 size) : base(size, 32)
        {
            zipImage = new ZipImage(zipData, size, 32);
        }

        internal override void Read(byte[] buffer)
        {
            if (buffer.Length == 0)
            {
                return;
            }

            var predictedData = new byte[buffer.Length];
            zipImage.Read(predictedData);

            unsafe
            {
                fixed (byte* ptrData = &predictedData[0])
                fixed (byte* ptrOutput = &buffer[0])
                {
                    Unpredict(ptrData, (Int32*)ptrOutput);
                }
            }
        }

        public override byte[] ReadCompressed()
        {
            return zipImage.ReadCompressed();
        }

        internal override void WriteInternal(byte[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            var predictedData = new byte[array.Length];
            unsafe
            {
                fixed (byte* ptrData = &array[0])
                fixed (byte* ptrOutput = &predictedData[0])
                {
                    Predict((Int32*)ptrData, ptrOutput);
                }
            }

            zipImage.WriteInternal(predictedData);
        }

        unsafe private void Predict(Int32* ptrData, byte* ptrOutput)
        {
            for (int i = 0; i < (int)Size.y; i++)
            {
                // Pack together the individual bytes of the 32-bit words, high-order
                // bytes before low-order bytes.
                int offset1 = (int)Size.x;
                int offset2 = 2 * offset1;
                int offset3 = 3 * offset1;

                Int32* ptrDataRow = ptrData;
                Int32* ptrDataRowEnd = ptrDataRow + (int)Size.x;
                byte* ptrOutputRow = ptrOutput;
                byte* ptrOutputRowEnd = ptrOutputRow + BytesPerRow;
                while (ptrData < ptrDataRowEnd)
                {
                    *(ptrOutput) = (byte)(*ptrData >> 24);
                    *(ptrOutput + offset1) = (byte)(*ptrData >> 16);
                    *(ptrOutput + offset2) = (byte)(*ptrData >> 8);
                    *(ptrOutput + offset3) = (byte)(*ptrData);

                    ptrData++;
                    ptrOutput++;
                }

                // Delta-encode the row
                ptrOutput = ptrOutputRowEnd - 1;
                while (ptrOutput > ptrOutputRow)
                {
                    *ptrOutput -= *(ptrOutput - 1);
                    ptrOutput--;
                }

                // Advance pointer to next row
                ptrOutput = ptrOutputRowEnd;
                System.Diagnostics.Debug.Assert(ptrData == ptrDataRowEnd);
            }
        }

        /// <summary>
        /// Unpredicts the raw decompressed image data into a 32-bpp bitmap with
        /// native endianness.
        /// </summary>
        unsafe private void Unpredict(byte* ptrData, Int32* ptrOutput)
        {
            for (int i = 0; i < Size.y; i++)
            {
                byte* ptrDataRow = ptrData;
                byte* ptrDataRowEnd = ptrDataRow + BytesPerRow;

                // Delta-decode each row
                ptrData++;
                while (ptrData < ptrDataRowEnd)
                {
                    *ptrData += *(ptrData - 1);
                    ptrData++;
                }

                // Within each row, the individual bytes of the 32-bit words are
                // packed together, high-order bytes before low-order bytes.
                // We now unpack them into words.
                int offset1 = (int)Size.x;
                int offset2 = 2 * offset1;
                int offset3 = 3 * offset1;

                ptrData = ptrDataRow;
                Int32* ptrOutputRowEnd = ptrOutput + (int)Size.x;
                while (ptrOutput < ptrOutputRowEnd)
                {
                    *ptrOutput = *(ptrData) << 24
                      | *(ptrData + offset1) << 16
                      | *(ptrData + offset2) << 8
                      | *(ptrData + offset3);

                    ptrData++;
                    ptrOutput++;
                }

                // Advance pointer to next row
                ptrData = ptrDataRowEnd;
                System.Diagnostics.Debug.Assert(ptrOutput == ptrOutputRowEnd);
            }
        }
    }
}
