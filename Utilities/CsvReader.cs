using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SvenskFisk.Utilities
{
    public class CsvReader : IDisposable
    {
        private StreamReader sr;

        private char delimiter;

        public CsvReader(Stream stream, char delimiter)
        {
            this.delimiter = delimiter;

            this.sr = new StreamReader(stream);
        }

        public bool EndOfStream { get { return this.sr.EndOfStream; } }

        public string[] ReadRow()
        {
            if (!this.sr.EndOfStream)
            {
                var line = sr.ReadLine();
                return ParseLine(line);
            }
            else
            {
                return null;
            }
        }

        public async Task<string[]> ReadRowAsync()
        {
            if (!this.sr.EndOfStream)
            {
                var line =  await sr.ReadLineAsync();
                return ParseLine(line);
            }
            else
            {
                return null;
            }
        }

        private string[] ParseLine(string line)
        {
            var buffer = new List<string>();
            var pos = 0;
            string field;
            while (pos < line.Length)
            {
                if (line[pos] == this.delimiter)
                {
                    pos++;
                    if (pos == line.Length) // line ends with an empty field
                    {
                        buffer.Add(string.Empty);
                    }

                    continue;
                }

                if (line[pos] == '"')
                {
                    pos = this.ParseQuotedField(line, pos, out field);
                }
                else
                {
                    pos = this.ParseField(line, pos, out field);
                }

                buffer.Add(field);
            }

            return buffer.ToArray();
        }

        private int ParseField(string line, int startIndex, out string field)
        {
            var end = line.IndexOf(this.delimiter, startIndex);
            field = end != -1 ?
                line.Substring(startIndex, end - startIndex) :
                line.Substring(startIndex);

            return startIndex + field.Length;
        }

        private int ParseQuotedField(string line, int startIndex, out string field)
        {
            var end = startIndex + 1;
            while (end < line.Length)
            {
                if (line[end] == '"' && (end + 1 == line.Length || line[end + 1] == this.delimiter))
                {
                    break;
                }

                end++;
            }

            field = line.Substring(startIndex + 1, end - startIndex - 1).Replace("\"\"", "\"");
            return end + 1;
        }

        public void Dispose()
        {
            this.sr.Dispose();
        }
    }
}
