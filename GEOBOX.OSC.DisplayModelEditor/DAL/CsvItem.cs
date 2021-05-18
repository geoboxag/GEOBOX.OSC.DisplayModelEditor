using System.Collections.Generic;
using System.Text;

namespace GEOBOX.OSC.DisplayModelEditor.DAL
{
    internal class CsvItem
    {
        internal string Layername { get; set; }
        internal string Caption { get; set; }
        internal List<string> Items { get; set; }

        public CsvItem()
        {
            Items = new List<string>();
        }

        internal string GetCsvString()
        {
            var builder = new StringBuilder();
            builder.Append($"{Layername};");
            builder.Append($"{Caption};");
            foreach (string item in Items)
            {
                builder.Append($"{item};");
            }

            return builder.ToString();
        }
    }
}
