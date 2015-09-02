using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InjectCS
{
    public partial class DynamicInput : Form
    {
        Compiler Comp;
        public DynamicInput()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var source =textBox2.Text;
            if (Comp == null)
                Comp = new Compiler(source);
            Comp.Run(source);
        }
    }
}
