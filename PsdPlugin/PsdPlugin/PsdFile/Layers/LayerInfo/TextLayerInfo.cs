using System;
using UnityEngine;

namespace PhotoshopFile
{
    public class TextLayerInfo : LayerInfo
    {
        public override string Signature
        {
            get { return "8BIM"; }
        }

        public override string Key
        {
            get { return "TySh"; }
        }

        public string Text { get; private set; }
        public float FontSize { get; private set; }
        public TextAnchor Alignment { get; private set; }
        public Color FillColor { get; private set; }

        public TextLayerInfo(PsdBinaryReader reader)
        {
            reader.Seek("TEXT");
            Text = reader.ReadUnicodeString();

            reader.Seek("/Justification ");
            int alignment = reader.ReadByte() - 48;
            Alignment = TextAnchor.MiddleLeft;
            if (alignment == 1)
            {
                Alignment = TextAnchor.MiddleRight;
            }
            else if (alignment == 2)
            {
                Alignment = TextAnchor.MiddleCenter;
            }

            reader.Seek("/FontSize ");
            FontSize = reader.ReadFloat();

            reader.Seek("/FillColor");
            reader.Seek("/Values [ ");
            FillColor = ReadColor(reader);
        }

        private Color ReadColor(PsdBinaryReader reader)
        {
            var color = new Color();

            color.a = reader.ReadFloat();
            reader.ReadByte();
            color.r = reader.ReadFloat();
            reader.ReadByte();
            color.g = reader.ReadFloat();
            reader.ReadByte();
            color.b = reader.ReadFloat();

            return color;
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
        }
    }
}
