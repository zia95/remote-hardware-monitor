using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAutoMessenger
{
    public static class StringHelper
    {
        public enum TextType
        {
            Default,
            ASCII,
            UTF8,
            UTF32,
            Unicode
        }
        private static Encoding get_encoding(TextType type)
        {
            switch (type)
            {
                case TextType.Default: return Encoding.Default;
                case TextType.ASCII: return Encoding.ASCII;
                case TextType.UTF8: return Encoding.UTF8;
                case TextType.UTF32: return Encoding.UTF32;
                case TextType.Unicode: return Encoding.Unicode;
                default: throw new NotSupportedException("type (" + type + ") is not supported!");
            }
        }
        public static String BytesToString(byte[] buffer, TextType type = TextType.Default) => get_encoding(type).GetString(buffer);
        public static byte[] StringToBytes(String buffer, TextType type = TextType.Default) => get_encoding(type).GetBytes(buffer);
    }
}
