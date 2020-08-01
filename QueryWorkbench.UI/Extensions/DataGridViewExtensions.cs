using System;
using System.IO;
using System.Windows.Forms;

namespace QueryWorkbenchUI.Extensions {
    public static class DatadataGridViewExtensions {
        public static void WriteDelimited(this DataGridView dataGridView, TextWriter writer, string columnDelimiter = ",", string rowDelimiter = null) {
            if (rowDelimiter == null) {
                rowDelimiter = Environment.NewLine;
            }
            int columnCount = dataGridView.Columns.Count;
            for (int j = 0; j < columnCount; j++) {
                if (!dataGridView.Columns[j].Visible) continue;

                writer.Write(dataGridView.Columns[j].HeaderText);
                if (j < dataGridView.Columns.Count - 1) {
                    writer.Write(columnDelimiter);
                }
                else {
                    writer.Write(writer.NewLine);
                }
            }

            for (int i = 1; (i - 1) < dataGridView.Rows.Count; i++) {
                for (int j = 0; j < columnCount; j++) {
                    if (!dataGridView.Columns[j].Visible) continue;

                    var cellValue = dataGridView.Rows[i - 1].Cells[j].Value.ToString();
                    if (cellValue.Contains(columnDelimiter)) {
                        cellValue = string.Format("\"{0}\"", cellValue);
                    }
                    writer.Write(cellValue);
                    if (j < dataGridView.Columns.Count - 1) {
                        writer.Write(columnDelimiter);
                    }
                }
                writer.Write(rowDelimiter);
            }
        }

        public static bool AllColumnsShown(this DataGridView dataGridView) {
            for (int j = 0; j < dataGridView.Columns.Count; j++) {
                if (!dataGridView.Columns[j].Visible) {
                    return false;
                }
            }
            return true;
        }

        public static bool ToggleColumnVisibility(this DataGridView dataGridView,  string columnName) {
            dataGridView.Columns[columnName].Visible = !dataGridView.Columns[columnName].Visible;
            return dataGridView.Columns[columnName].Visible;
        }
    }
}
