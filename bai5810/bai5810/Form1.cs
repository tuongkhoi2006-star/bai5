namespace bai5810
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timerClock.Start();
            UpdateTotals();
            // add one empty row to start
            AddNewRow();
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            tsslTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void AddNewRow()
        {
            dgvItems.Rows.Add();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                AddNewRow();
                e.Handled = true;
            }
        }

        private void dgvItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (DataGridViewRow r in dgvItems.SelectedRows)
                {
                    if (!r.IsNewRow)
                        dgvItems.Rows.Remove(r);
                }
                UpdateTotals();
                e.Handled = true;
            }
        }

        private void dgvItems_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            UpdateTotals();
        }

        private void dgvItems_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // clear any previous error for editing control
            if (e.Control != null)
            {
                errorProvider.SetError(e.Control, string.Empty);
            }
        }

        private void dgvItems_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var colName = dgvItems.Columns[e.ColumnIndex].Name;
            string formatted = e.FormattedValue?.ToString() ?? string.Empty;
            if (colName == "colQuantity")
            {
                if (!int.TryParse(formatted, out int q) || q <= 0)
                {
                    // set cell error
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "Số lượng phải là số nguyên > 0";
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, "Số lượng phải > 0");
                }
                else
                {
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = string.Empty;
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, string.Empty);
                }
            }
            else if (colName == "colWeight")
            {
                if (!decimal.TryParse(formatted, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal w) || w <= 0)
                {
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "Trọng lượng phải > 0";
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, "Trọng lượng phải > 0");
                }
                else
                {
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = string.Empty;
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, string.Empty);
                }
            }
            else if (colName == "colUnitPrice")
            {
                if (!decimal.TryParse(formatted, NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out decimal p) || p < 0)
                {
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "Đơn giá phải >= 0";
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, "Đơn giá phải >= 0");
                }
                else
                {
                    dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = string.Empty;
                    if (dgvItems.EditingControl != null)
                        errorProvider.SetError(dgvItems.EditingControl, string.Empty);
                }
            }
        }

        private void dgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                RecalculateRowTotal(e.RowIndex);
            }
            finally
            {
                UpdateTotals();
            }
        }

        private void RecalculateRowTotal(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvItems.Rows.Count) return;
            var row = dgvItems.Rows[rowIndex];
            if (row.IsNewRow) return;

            decimal qty = 0m;
            decimal weight = 0m;
            decimal unit = 0m;

            var cQty = row.Cells["colQuantity"].Value?.ToString();
            var cWeight = row.Cells["colWeight"].Value?.ToString();
            var cUnit = row.Cells["colUnitPrice"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(cQty) && int.TryParse(cQty, out int q)) qty = q;
            if (!string.IsNullOrWhiteSpace(cWeight) && decimal.TryParse(cWeight, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal w)) weight = w;
            if (!string.IsNullOrWhiteSpace(cUnit) && decimal.TryParse(cUnit, NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out decimal u)) unit = u;

            decimal total = qty * unit;
            row.Cells["colTotal"].Value = total.ToString("C2");
        }

        private void UpdateTotals()
        {
            int totalQty = 0;
            decimal totalWeight = 0m;
            decimal totalAmount = 0m;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                var qv = row.Cells["colQuantity"].Value?.ToString();
                var wv = row.Cells["colWeight"].Value?.ToString();
                var tv = row.Cells["colTotal"].Value?.ToString();

                if (int.TryParse(qv, out int q)) totalQty += q;
                if (decimal.TryParse(wv, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal w)) totalWeight += w;
                // total is stored as currency string; try parse removing currency
                if (row.Cells["colTotal"].Value != null)
                {
                    var cellVal = row.Cells["colTotal"].Value;
                    if (cellVal is string s)
                    {
                        if (decimal.TryParse(s, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal t)) totalAmount += t;
                    }
                    else if (cellVal is decimal d)
                    {
                        totalAmount += d;
                    }
                }
            }

            tsslTotals.Text = $"SL: {totalQty}    Tổng trọng lượng(kg): {totalWeight:N2}    Thành tiền: {totalAmount:C2}";
        }
    }
}
