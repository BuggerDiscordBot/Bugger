using System;
using System.Collections.Generic;
using System.Text;

namespace Bugger.Helpers
{
    public static class MessageFormater
    {

        public static string CreateTable(TableSettings settings, in object[,] values)
        {
            if (settings.header != null && settings.header.Length != values.GetLength(1)) { settings.header = null; }

            // The passed spacing value represents the actual space
            // that is acounted for the value,
            // however for formatting purposes on each side a white space
            // is inserted which are represented in the actualSpacing
            var actualSpacing = settings.spacing + (Math.Sign(settings.spacing) * 2);

            var updatedSettings = settings;
            updatedSettings.rows = values.GetLength(0);
            updatedSettings.cols = values.GetLength(1);
            updatedSettings.spacing = actualSpacing;

            var table = new StringBuilder();
            table.Append("```");
            InsertHeader(updatedSettings, table);
            InsertTableValues(updatedSettings, table, values);
            table.Append("```");

            return table.ToString();
        }

        private static void InsertHeader(TableSettings settings, StringBuilder table)
        {

            if (!String.IsNullOrEmpty(settings.title))
            {
                var titleSettings = new TableSettings("", 1, 1, settings.spacing);
                InsertTableValues(titleSettings, table, new[] { settings.title });
                table.Append("\n");
            }
            if (settings.header != null && settings.header.Length != 0)
            {
                var headerSettings = new TableSettings("", 1, settings.header.Length, settings.spacing);
                InsertTableValues(headerSettings, table, settings.header);
            }
        }

        private static void InsertTableValues(TableSettings settings, StringBuilder table, in object[] values)
        {
            var values2D = new object[1, values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values2D[0, i] = values[i];
            }
            InsertTableValues(settings, table, values2D);
        }

        private static void InsertTableValues(TableSettings settings, StringBuilder table, in object[,] values)
        {
            InsertTableSeperator(settings, table);

            Action<TableSettings, StringBuilder> seperateEveryLine, seperate;

            if (settings.seperatorForEveryLine)
            {
                seperateEveryLine = (s, t) => InsertTableSeperator(s, t);
                seperate = (s, t) => { };
            }
            else
            {
                seperateEveryLine = (s, t) => { };
                seperate = (s, t) => InsertTableSeperator(s, t);
            }

            var actualSpacing = (settings.spacing - (Math.Sign(settings.spacing) * 2));
            for (int i = 0; i < settings.rows; i++)
            {
                var colBase = "| {0," + actualSpacing + "} ";
                for (int j = 0; j < settings.cols; j++)
                {
                    table.AppendFormat(colBase, values[i, j].ToString());
                }
                table.Append("|\n");
                seperateEveryLine(settings, table);
            }

            seperate(settings, table);
        }

        private static void InsertTableSeperator(TableSettings settings, StringBuilder table)
        {
            var tableWidth = Math.Abs(settings.spacing) * settings.cols + 1 + settings.cols;
            var count = 0;
            for (int i = 0; i < tableWidth; i++)
            {
                if (i == 0 || i == tableWidth - 1 || count == Math.Abs(settings.spacing))
                {
                    table.Append("+");
                    count = 0;
                }
                else
                {
                    table.Append("-");
                    count++;
                }
            }
            table.Append("\n");
        }

        public struct TableSettings : IEquatable<object>
        {
            public string title { get; set; }
            public string[] header { get; set; }
            public int rows { get; set; }
            public int cols { get; set; }
            public int spacing { get; set; }
            public bool seperatorForEveryLine { get; set; }

            public TableSettings(string title, int spacing) :
                this(title, null, 0, 0, spacing, false)
            {
            }

            public TableSettings(string title, int spacing, bool seperatorForEveryLine) :
                this(title, null, 0, 0, spacing, seperatorForEveryLine)
            {
            }

            public TableSettings(string[] header, int spacing) :
                this(null, header, 0, 0, spacing, false)
            {
            }

            public TableSettings(string[] header, int spacing, bool seperatorForEveryLine) :
                this(null, header, 0, 0, spacing, seperatorForEveryLine)
            {
            }

            public TableSettings(string title, string[] header, int spacing) :
                this(title, header, 0, 0, spacing, false)
            {
            }

            public TableSettings(string title, string[] header, int spacing, bool seperatorForEveryLine) :
                this(title, header, 0, 0, spacing, seperatorForEveryLine)
            {
            }

            public TableSettings(string title, int rows, int cols, int spacing) :
                this(title, null, rows, cols, spacing, false)
            {
            }

            public TableSettings(string title, int rows, int cols, int spacing, bool seperatorForEveryLine) :
                this(title, null, rows, cols, spacing, seperatorForEveryLine)
            {
            }

            public TableSettings(string[] header, int rows, int cols, int spacing) :
                this(null, header, rows, cols, spacing, false)
            {
            }

            public TableSettings(string[] header, int rows, int cols, int spacing, bool seperatorForEveryLine) :
                this(null, header, rows, cols, spacing, seperatorForEveryLine)
            {
            }

            public TableSettings(string title, string[] header, int rows, int cols, int spacing) :
                this(null, header, rows, cols, spacing, false)
            {
            }

            public TableSettings(string title, string[] header, int rows, int cols, int spacing, bool seperatorForEveryLine)
            {
                this.title = title;
                this.header = header;
                this.rows = rows;
                this.cols = cols;
                this.spacing = spacing;
                this.seperatorForEveryLine = seperatorForEveryLine;
            }
        }
    }
}
