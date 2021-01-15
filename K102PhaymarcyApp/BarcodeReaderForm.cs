using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using K102PhaymarcyApp.Models;
using ZXing;

namespace K102PhaymarcyApp
{
    public partial class BarcodeReaderForm : Form
    {
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        Medicine selectedMedicine;
        PhaymarcyDB _context = new PhaymarcyDB();
        public BarcodeReaderForm()
        {
            InitializeComponent();
        }


        private void BarcodeReaderForm_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filterInfoCollection)
            {
                cmbCamera.Items.Add(device.Name);
            }
            cmbCamera.SelectedIndex = 0;
        }

        private void btnBarcode_Click(object sender, EventArgs e)
        {
            SellMedicinePanel.Visible = true;
            videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cmbCamera.SelectedIndex].MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(bitmap);
            if (result != null)
            {
                txtBarcode.Invoke(new MethodInvoker(delegate ()
                {
                    txtBarcode.Text = result.ToString();
                    selectedMedicine = _context.Medicines.FirstOrDefault(m => m.Barcode == txtBarcode.Text);
                    if (selectedMedicine != null)
                    {
                        txtBuyMedName.Text = selectedMedicine.MedicineName;
                    }
                }));
            }
            pictureBox.Image = bitmap;
        }

        private void BarcodeReaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice != null)
            {
                if (videoCaptureDevice.IsRunning)
                    videoCaptureDevice.Stop();
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
            txtBarcode.Text = "";
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
            ClearAfterOrders();
        }
    }
}
