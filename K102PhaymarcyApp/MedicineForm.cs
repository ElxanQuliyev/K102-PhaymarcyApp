using K102PhaymarcyApp.Helpers;
using K102PhaymarcyApp.Models;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace K102PhaymarcyApp
{
    public partial class MedicineForm : Form
    {
        PhaymarcyDB _context = new PhaymarcyDB();

        #region Constructor
        public MedicineForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Firm Combobox Fill
        public void FillFirmCombo()
        {
            cmbFirms.Items.AddRange(_context.Firms.Select(f => f.Name).ToArray());
        }
        #endregion

        #region Tag Combobox Fill
        public void FillTagsCombo()
        {
            cmbTags.Items.AddRange(_context.Tags.Select(f => f.Name).ToArray());
        }
        #endregion

        #region Medicine Load
        private void MedicineForm_Load(object sender, EventArgs e)
        {
            FillFirmCombo();
            FillTagsCombo();
            FillMedicineDataGrid();
        }
        #endregion


        private void FillMedicineDataGrid()
        {
            dtgMedicineList.DataSource = _context.Medicines.Select(md => new
            {
                Medicine_Name=md.MedicineName,
                md.Price,
                md.Quantity,
                Receipt=md.IsReceipt?"Reseptli":"Resptsiz",
                Firm_Name= md.Firm.Name ,
                ProductionDate=md.ProDate,
                md.ExperienceDate
            }).ToList();
            dtgMedicineList.Columns[5].DefaultCellStyle.Format = "dddd, dd MMMM yyyy";
            dtgMedicineList.Columns[6].DefaultCellStyle.Format = "dddd, dd MMMM yyyy";
            for (int i = 0; i < dtgMedicineList.RowCount; i++)
            {
                if (dtgMedicineList.Rows[i].Index % 2 == 0)
                {
                    dtgMedicineList.Rows[i].DefaultCellStyle.BackColor = Color.Teal;
                    dtgMedicineList.Rows[i].DefaultCellStyle.ForeColor = Color.White;

                }
            }
        }

        public int FindFirm(string firmName)
        {
            Firm selectedFirm = _context.Firms.FirstOrDefault(fr => fr.Name == firmName);
            if (selectedFirm == null)
            {
                Firm firm = _context.Firms.Add(new Firm
                {
                    Name = firmName
                });
                _context.SaveChanges();
                return firm.F_ID;
            }
            return selectedFirm.F_ID;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            string medicineName = txtMedicineName.Text;
            string firmName = cmbFirms.Text;
            short count = (short)nmQuantity.Value;
            decimal price = nmPrice.Value;
            string barcode = txtBarcode.Text;
            string description = rcDescription.Text;
            DateTime proDate = dtProDate.Value;
            DateTime exDate = dtExpDate.Value;
            bool isRes = cbReceipt.Checked;
            string[] ar = { medicineName, firmName, description };

            if (Utilites.IsEmpty(ar))
            {
                lblError.Visible = false;
                int firmId = FindFirm(firmName);
                if (proDate > exDate)
                {
                    Medicine medicine = _context.Medicines.Add(new Medicine
                    {
                        MedicineName = medicineName,
                        Price = price,
                        Quantity = count,
                        Description = description,
                        ProDate = proDate,
                        ExperienceDate = exDate,
                        FirmID = firmId,
                        IsReceipt = cbReceipt.Checked ? true : false,
                        Barcode = barcode
                    });
                    _context.SaveChanges();
                    MedicineAddTag(medicine.M_ID);
                    MessageBox.Show("Medicine added successfully!");
                    ClearFormData();
                    FillMedicineDataGrid();
                }
                else
                {
                    lblError.Text = "Experience date is valid!";
                    lblError.Visible = true;
                }
           
            }
            else
            {
                lblError.Text = "Please,all the fiel!";
                lblError.Visible = true;
            }
        }
        private bool CheckTagName(string tg)
        {
            Tag tag = _context.Tags.FirstOrDefault(x => x.Name == tg);
            if (tag == null)
            {
                return false;
            }
            return true;
        }
        private void MedicineAddTag(int medicineID)
        {
            for (var i = 0; i < ckTagList.Items.Count; i++)
            {
                string tagName = ckTagList.Items[i].ToString();
                int tagID;
                if (CheckTagName(tagName))
                {
                    tagID = _context.Tags.First(x => x.Name == tagName).T_ID;
                }
                else
                {
                    Tag tag = _context.Tags.Add(new Tag
                    {
                        Name = tagName
                    });
                    _context.SaveChanges();
                    tagID = tag.T_ID;

                }

                _context.Medicine_To_Tags.Add(new Medicine_To_Tags
                {
                    MedicineID = medicineID,
                    TagID = tagID
                }) ;
                _context.SaveChanges();
            }
        }

        private void txtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57 || txtBarcode.TextLength > 11) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void cmbTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddTagItem();
        }
        private void AddTagItem()
        {
            string tagName = cmbTags.Text;
            if (!ckTagList.Items.Contains(tagName) && !string.IsNullOrWhiteSpace(tagName))
            {
                ckTagList.Items.Add(tagName, true);
            }
        }
        private void cmbTags_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddTagItem();
            }

        }

        private void ClearFormData()
        {
            foreach (Control item in this.Controls)
            {
                 if(item is TextBox || item is ComboBox || item is RichTextBox)
                {
                    item.Text = "";
                }  
                 else if(item is NumericUpDown)
                {
                    NumericUpDown nm = (NumericUpDown)item;
                    nm.Value = 1;
                }else if (item is DateTimePicker)
                {
                    DateTimePicker dt = (DateTimePicker)item;
                    dt.Value = DateTime.Now;
                }else if(item is CheckedListBox)
                {
                    CheckedListBox ckList = (CheckedListBox)item;
                    ckList.Items.Clear();
                }
                 else if(item is CheckBox)
                {
                    CheckBox ck = (CheckBox)item;
                    ck.Checked = false;
                }
            }
        }
       
        private void ckTagList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = ckTagList.SelectedIndex;
            if (selected != -1)
            {
                ckTagList.Items.RemoveAt(selected);
            }
        }
    }
}
