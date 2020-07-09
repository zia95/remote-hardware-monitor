using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAutoMessenger
{
    public class JSONWriter
    {
/*
{
  "name": "Benjamin Watson",
  "age": 31,
  "isMarried": true,
  "hobbies": ["Football", "Swimming"],
  "kids": [
    {
      "name": "Billy",
      "age": 5
    }, 
   {
      "name": "Milly",
      "age": 3
    }
  ]
}
*/
        private List<string> m_lines = null;


        private void remove_last_char()
        {
            string end = m_lines.Last();
            end = end.Remove(end.Length - 1, 1);
            m_lines.Add(end);
            m_lines.RemoveAt(m_lines.Count - 2);
        }
        public void Begin()
        {
            m_lines = null;
            m_lines = new List<string>();
            m_lines.Add("{");
        }
        public void End()
        {
            remove_last_char();
            m_lines.Add("}");
        }

        public string Generate()
        {
            string str = "";
            foreach(var ln in m_lines)
            {
                str += Environment.NewLine;
                str += ln;
            }
            return str;
        }


        public void AddPair(string key, string value)
        {
            m_lines.Add($"\"{key}\":\"{value}\",");
        }
        public void AddPair(string key, decimal value)
        {
            m_lines.Add($"\"{key}\":{value},");
        }
        public void AddPair<T>(string key, T value)
        {
            m_lines.Add($"\"{key}\":{value},");
        }
        public void BeginStructureArray(string key)
        {
            m_lines.Add($"\"{key}\":[");
        }

        public void BeginStructureArrayElement()
        {
            m_lines.Add("{");
        }
        public void AddStructureArrayPair(string key, string value) => AddPair(key, value);
        public void AddStructureArrayPair(string key, decimal value) => AddPair(key, value);
        public void AddStructureArrayPair<T>(string key, T value) => AddPair(key, value);

        public void EndStructureArrayElement()
        {
            remove_last_char();
            m_lines.Add("},");
        }
        public void EndStructureArray()
        {
            remove_last_char();
            m_lines.Add("],");
        }
    }
}
