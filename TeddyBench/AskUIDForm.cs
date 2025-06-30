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
            txtUid.Text = cleanHexInput(txtUid.Text);

            if (!txtUid.Text.StartsWith("E00403"))
            {
                txtUid.Text = "E00403";
                txtUid.Select(6, 1);
            }            

            validateExistingUid();
        }

        private void txtUidLE_TextChanged(object sender, EventArgs e)
        {
            txtUidLE.Text = cleanHexInput(txtUidLE.Text);

            if (!txtUidLE.Text.EndsWith("0304E0"))
            {
                txtUidLE.Text = "0304E0";
                txtUidLE.Select(0, 0);
            }

            validateExistingUidLE();
        }

        private void txtUidLE_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOk.PerformClick();
            } else
            {
                pasteCheck(sender, e);
            }
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

        private void validateExistingUidLE()
        {
            bool invalid = txtUidLE.Text.Length != 16 || !txtUidLE.Text.All("0123456789abcdefABCDEF".Contains);

            if (invalid)
            {
                txtUidLE.BackColor = Color.PaleVioletRed;
                btnOk.Enabled = false;
            }
            else
            {
                txtUidLE.BackColor = Color.LightGreen;
                btnOk.Enabled = true;

                // take the value from textUidLE and convert it to little-endian format and set the value in textUid
                // Wert aus txtUidLE nehmen, in 8 Bytes aufteilen, umkehren und als txtUid setzen
                txtUid.Text = convertEndianToHex(txtUidLE.Text);
            }
        }

        private string convertEndianToHex(string le)
        {
            if (le.Length == 16)
            {
                var bytes = Enumerable.Range(0, 8)
                    .Select(i => le.Substring(i * 2, 2))
                    .Reverse()
                    .ToArray();
                return string.Concat(bytes).ToUpper();
            }
            return string.Empty;
        }

        private void txtUid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOk.PerformClick();
            } else
            {
                pasteCheck(sender, e);
            }
        }

        private string cleanHexInput(string input)
        {
            // Entfernt alle Zeichen, die keine Hexadezimalzeichen sind
            return new string(input.Where(c => "0123456789abcdefABCDEF".Contains(c)).ToArray());
        }

        private bool pasteCheck(object sender, KeyEventArgs e)
        {
            // Prüfen, ob STRG+V (Paste) gedrückt wurde
            if (e.Control && e.KeyCode == Keys.V)
            {
                TextBox box = sender as TextBox;
                if (box != null)
                {
                    // Clipboard-Text holen und filtern
                    string paste = Clipboard.GetText();
                    string filtered = cleanHexInput(paste);

                    if (filtered.Length == 16)
                    {
                        if (filtered.StartsWith("E00403"))
                        {
                            // Einfügen in Textbox
                            txtUid.Text = filtered;
                            txtUid.SelectionStart = filtered.Length; // Cursor ans Ende setzen
                            txtUid.SelectionLength = 0; // Keine Auswahl

                            txtUidLE.Text = convertEndianToHex(filtered);
                        }

                        else if (filtered.EndsWith("0304E0"))
                        {
                            // Einfügen in Textbox
                            txtUid.Text = convertEndianToHex(filtered);
                            txtUid.SelectionStart = filtered.Length; // Cursor ans Ende setzen
                            txtUid.SelectionLength = 0; // Keine Auswahl

                            txtUidLE.Text = filtered;
                        } else {
                            MessageBox.Show("The UID must start with 'E00403'.", "Invalid UID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Einfügen an Cursor-Position
                        int selStart = box.SelectionStart;
                        int selLength = box.SelectionLength;
                        string orig = box.Text;
                        string newText = orig.Substring(0, selStart) + filtered + orig.Substring(selStart + selLength);
                        box.Text = newText;
                        box.SelectionStart = selStart + filtered.Length;
                        box.SelectionLength = 0;
                    }
                    e.SuppressKeyPress = true;
                }
                return true;
            } else
            {
                return false;
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
                int start = selectedText.LastIndexOf('[');
                int end = selectedText.LastIndexOf(']');
                if (start >= 0 && end > start)
                {                    
                    txtUid.Text = selectedText.Substring(start + 1, end - start - 1).ToUpper();                    
                }
            }

            validateExistingUid();
        }
    }
}
