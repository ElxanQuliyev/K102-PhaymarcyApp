using K102PhaymarcyApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace K102PhaymarcyApp
{
    public partial class Dashboard : Form
    {
        PhaymarcyDB _context = new PhaymarcyDB();
        public Dashboard()
        {
            InitializeComponent();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MedicineForm mdF = new MedicineForm();
            mdF.ShowDialog();
        }

        private void MedicineComboFill()
        {
            cmbMedicine.Items.AddRange(_context.Medicines.Select(m => m.MedicineName).ToArray());
        }
        private void TagComboFill()
        {
            cmbTags.Items.AddRange(_context.Tags.Select(m => m.Name).ToArray());
        }
        private void FillMedicineDataGrid()
        {
            dtgMedicineList.DataSource = _context.Medicine_To_Tags
                .Where(m => m.Medicine.MedicineName.Contains(cmbMedicine.Text) &&
                 m.Tag.Name.Contains(cmbTags.Text))
                .Select(md => new
                {
                    md.MedicineID,
                    Medicine_Name = md.Medicine.MedicineName,
                    md.Medicine.Price,
                    md.Medicine.Quantity,
                    Receipt = md.Medicine.IsReceipt ? "Reseptli" : "Resptsiz",
                    Firm_Name = md.Medicine.Firm.Name,
                    ProductionDate = md.Medicine.ProDate,
                    md.Medicine.ExperienceDate
                }).Distinct().ToList();
            dtgMedicineList.Columns[0].Visible = false;
            dtgMedicineList.Columns[5].DefaultCellStyle.Format = "dddd, dd MMMM yyyy";
            dtgMedicineList.Columns[6].DefaultCellStyle.Format = "dddd, dd MMMM yyyy";
            for (int i = 0; i < dtgMedicineList.RowCount; i++)
            {
                short quantity = (short)dtgMedicineList.Rows[i].Cells[3].Value;
                DateTime exDate = (DateTime)dtgMedicineList.Rows[i].Cells[7].Value;
                if (exDate < DateTime.Now)
                {
                    dtgMedicineList.Rows[i].DefaultCellStyle.BackColor = Color.OrangeRed;
                    dtgMedicineList.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
                if (quantity <= 0)
                {
                    dtgMedicineList.Rows[i].DefaultCellStyle.BackColor = Color.DarkRed;
                    dtgMedicineList.Rows[i].DefaultCellStyle.ForeColor = Color.White;

                }
                if (quantity <= 0 && exDate < DateTime.Now)
                {
                    dtgMedicineList.Rows[i].DefaultCellStyle.BackColor = Color.Black;
                    dtgMedicineList.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            MedicineComboFill();
            TagComboFill();
            FillMedicineDataGrid();
        }

        private void cmbMedicine_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillMedicineDataGrid();
        }

        private void cmbTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillMedicineDataGrid();
        }

        private void cmbMedicine_KeyUp(object sender, KeyEventArgs e)
        {
            FillMedicineDataGrid();
        }

        private void cmbTags_KeyUp(object sender, KeyEventArgs e)
        {
            FillMedicineDataGrid();
        }

        private void dtgMedicineList_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int medId = (int)dtgMedicineList.Rows[e.RowIndex].Cells[0].Value;
            Medicine selectedMedicine = _context.Medicines.First(m => m.M_ID == medId);
            if (selectedMedicine.Quantity != 0 && selectedMedicine.ExperienceDate > DateTime.Now)
            {
                SellMedicinePanel.Visible = true;
                txtBuyMedName.Text = selectedMedicine.MedicineName;
                nmBuyCount.Maximum = selectedMedicine.Quantity;
                nmBuyCount.Value = 1;
            }
        }

        private void AddMedicineToList(string text)
        {
            if (!ckBuyMedLİst.Items.Contains(text))
            {
                ckBuyMedLİst.Items.Add(text, true);
            }
        }

        private void btnAddMedicine_Click(object sender, EventArgs e)
        {
            AddMedicineToList(txtBuyMedName.Text + "-" + nmBuyCount.Value);
        }

        private void ckBuyMedLİst_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = ckBuyMedLİst.SelectedIndex;
            if (selected != -1)
            {
                ckBuyMedLİst.Items.RemoveAt(selected);
            }
        }

        private void ClearAfterOrders()
        {
            ckBuyMedLİst.Items.Clear();
            SellMedicinePanel.Visible = false;
        }
        private void btnBuyMedicine_Click(object sender, EventArgs e)
        {
            string result = "";
            for (int i = 0; i < ckBuyMedLİst.Items.Count; i++)
            {
                string medicineItem = ckBuyMedLİst.Items[i].ToString();
                string medName = medicineItem.Substring(0, medicineItem.LastIndexOf("-"));
                short count = Convert.ToInt16(medicineItem.Substring(medicineItem.LastIndexOf("-") + 1));
                Medicine selectedMed = _context.Medicines.First(f => f.MedicineName == medName);
                _context.Orders.Add(new Order
                {
                    WorkerID = 1,
                    MedicineID = selectedMed.M_ID,
                    PurchaseDate = DateTime.Now,
                    Amount = count,
                    Price = selectedMed.Price
                });
                selectedMed.Quantity -= count;
                _context.SaveChanges();
                result += $"Medicine Name:{selectedMed.MedicineName},Quantity:{count},Price:{selectedMed.Price}Azn \n";
            }
            MessageBox.Show(result, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FillMedicineDataGrid();
            ClearAfterOrders();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var xlapp = new Microsoft.Office.Interop.Excel.Application();
            var xWorkBook = xlapp.Workbooks.Add();
            var xlSheet = xWorkBook.Worksheets[1];
            for (int i = 0; i < dtgMedicineList.ColumnCount; i++)
            {
                xlSheet.Cells[1, i + 1] = dtgMedicineList.Columns[i].HeaderText;
            }
            for (int i = 0; i < dtgMedicineList.RowCount; i++)
            {
                for (int j = 0; j < dtgMedicineList.ColumnCount; j++)
                {
                    xlSheet.Cells[i + 2, j + 1] = dtgMedicineList.Rows[i].Cells[j].Value.ToString();
                }

            }
            string medFileName = "Medicine"+Guid.NewGuid().ToString();
            xWorkBook.SaveAs($@"C:\Users\Elxan\Desktop\{medFileName}.xlsx");
            xWorkBook.Close();
            xlapp.Quit();
            MessageBox.Show("Medicine added Excel successfully");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dtgMedicineList.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Output.pdf";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            PdfPTable pdfTable = new PdfPTable(dtgMedicineList.Columns.Count);
                            pdfTable.DefaultCell.Padding = 3;
                            pdfTable.WidthPercentage = 100;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                            foreach (DataGridViewColumn column in dtgMedicineList.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                                pdfTable.AddCell(cell);
                            }

                            foreach (DataGridViewRow row in dtgMedicineList.Rows)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    pdfTable.AddCell(cell.Value.ToString());
                                }
                            }

                            using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                                PdfWriter.GetInstance(pdfDoc, stream);
                                pdfDoc.Open();
                                pdfDoc.Add(pdfTable);
                                pdfDoc.Close();
                                stream.Close();
                            }

                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            BarcodeReaderForm brd = new BarcodeReaderForm();
            brd.ShowDialog();
        }
    }
}
