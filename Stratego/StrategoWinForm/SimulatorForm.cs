using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameCore;
using Stratego.Sprites;

namespace Stratego
{
    public partial class SimulatorForm : Form
    {
        public SimulatorForm()
        {
            InitializeComponent();
        }

        public Session _session;
        private BoardSprite boardSprite;

        private void Form1_Load(object sender, EventArgs e)
        {
            // combine the data structures with the user interfaces
            boardSprite = new BoardSprite(pnl_zoom, tableBoardPanel, this)
            {
                _session = this._session,
            };

        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("new game...");
        }



        private void zoomTo100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // zoom to fit
            boardSprite.ZoomToFit();
        }

        private void recenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // recenter
            boardSprite.RecenterBoardDisplay();
        }
    }
}
