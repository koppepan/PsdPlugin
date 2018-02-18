﻿/////////////////////////////////////////////////////////////////////////////////
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
using UnityEngine;

namespace PhotoshopFile.Compression
{
    public abstract class ImageData
    {
        public int BitDepth { get; private set; }

        public int BytesPerRow { get; private set; }

        public Vector2Int Size { get; private set; }

        protected abstract bool AltersWrittenData { get; }

        protected ImageData(Vector2Int size, int bitDepth)
        {
            Size = size;
            BitDepth = bitDepth;
            BytesPerRow = Util.BytesPerRow(size, bitDepth);
        }

        /// <summary>
        /// Reads decompressed image data.
        /// </summary>
        public virtual byte[] Read()
        {
            var imageLongLength = (long)BytesPerRow * (int)Size.y;
            Util.CheckByteArrayLength(imageLongLength);

            var buffer = new byte[imageLongLength];
            Read(buffer);
            return buffer;
        }

        internal abstract void Read(byte[] buffer);

        /// <summary>
        /// Reads compressed image data.
        /// </summary>
        public abstract byte[] ReadCompressed();

        /// <summary>
        /// Writes rows of image data into compressed format.
        /// </summary>
        /// <param name="array">An array containing the data to be compressed.</param>
        public void Write(byte[] array)
        {
            var imageLength = (long)BytesPerRow * (int)Size.y;
            if (array.Length != imageLength)
            {
                throw new ArgumentException(
                  "Array length is not equal to image length.",
                  nameof(array));
            }

            if (AltersWrittenData)
            {
                array = (byte[])array.Clone();
            }

            WriteInternal(array);
        }

        internal abstract void WriteInternal(byte[] array);
    }
}
