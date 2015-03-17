using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MDSLogAnalyzerCommon;
namespace Watson
{
    public partial class RDPFailures : Form
    {
        public RDPFailures()
        {
            InitializeComponent();
        }

        private void RDPFailures_Load(object sender, EventArgs e)
        {
            foreach(var item in Enum.GetValues(typeof(EndPoint)))
            {
                endpointSelectionComboBox.Items.Add(item);
            }
        }
    }
}
