using System.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SecureNotes
{
    public class ColorItem
    {
        public string NameKey { get; }
        public string Hex { get; }
        public Color Color { get; }

        public ColorItem(string nameKey, string hex)
        {
            NameKey = nameKey;
            Hex = hex;
            Color = ColorTranslator.FromHtml(hex);
        }

        public override string ToString() => LocalizationManager.Get(NameKey);
    }
}
