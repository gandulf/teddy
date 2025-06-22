using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace TeddyBench
{
    public partial class AskUIDForm : Form
    {
        internal string Uid;        
        private RfidReaderBase RfidReader;

        public AskUIDForm(RfidReaderBase rfidReader, Dictionary<string, string> customTonieInfos)
        {
            RfidReader = rfidReader;
            InitializeComponent();
            txtUid.Select();
            txtUid.Select(6, 10);
            txtUid_TextChanged(null, null);
            
            // Leere Option am Anfang hinzufügen
            
            if (customTonieInfos != null && customTonieInfos.Count > 0)
            {
                existingTonies.Enabled = true;
                existingTonies.Items.Add("<Choose existing tonie>");
                existingTonies.Items.AddRange(customTonieInfos.Values.ToArray());
                existingTonies.SelectedIndex = 0;

                // Eventhandler für Auswahl in ComboBox
                existingTonies.SelectedIndexChanged += ExistingTonies_SelectedIndexChanged;
            } else
            {
                existingTonies.Enabled = false;
            }


            if (RfidReader != null)
            {
                RfidReader.UidFound += RfidReaderBase_UidFound;
            }
        }

        private void RfidReaderBase_UidFound(object sender, string e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => RfidReaderBase_UidFound(sender, e)));
                return;
            }

            if (e != null && txtUid.Text != e)
            {
                txtUid.Text = e;
                txtUid.Select();
                txtUid.Select(6, 10);
                SystemSounds.Beep.Play();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            txtUid.Text = Uid;
            txtUid.Select();
            txtUid.Select(6, 10);
            base.OnShown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (RfidReader != null)
            {
                RfidReader.UidFound -= RfidReaderBase_UidFound;
            }

            Uid = txtUid.Text.ToUpper();            

            base.OnClosing(e);
        }

        private void txtUid_TextChanged(object sender, EventArgs e)
        {
            if (!txtUid.Text.StartsWith("E00403"))
            {
                txtUid.Text = "E00403";
                txtUid.Select(6, 1);
            }            

            validateExistingUid();
        }

        private void validateExistingUid()
        {
            bool invalid = txtUid.Text.Length != 16 || !txtUid.Text.All("0123456789abcdefABCDEF".Contains);

            if (invalid)
            {
                txtUid.BackColor = Color.PaleVioletRed;
                btnOk.Enabled = false;
            }
            else
            {
                txtUid.BackColor = Color.LightGreen;
                btnOk.Enabled = true;
            }
        }

        private void txtUid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOk.PerformClick();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {

        }

        private void ExistingTonies_SelectedIndexChanged(object sender, EventArgs e)
        {           

            if (existingTonies.SelectedIndex > 0)
            {
                string selectedText = existingTonies.Items[existingTonies.SelectedIndex].ToString();
                // UID aus eckigen Klammern extrahieren
                int start = selectedText.IndexOf('[');
                int end = selectedText.IndexOf(']');
                if (start >= 0 && end > start)
                {                    
                    txtUid.Text = selectedText.Substring(start + 1, end - start - 1).ToUpper();                    
                }
            }

            validateExistingUid();
        }
    }
}
