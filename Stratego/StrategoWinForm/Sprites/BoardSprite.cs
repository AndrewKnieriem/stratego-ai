using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameCore;

namespace Stratego.Sprites
{
    public class BoardSprite
    {
        public Session _session;

        public Panel pnl_zoom;
        public TableLayoutPanel tableBoardPanel;
        public Form controllerForm;
        
        private float currentBoardSize = 400;

        private bool IsDraggingBoard = false;
        private Size DragOffset = new Size(0, 0);
        private Point DragInitialCursorLocation = new Point(0, 0);

        public BoardSprite(Panel zoompnl, TableLayoutPanel table, Form theForm)
        {
            pnl_zoom = zoompnl;
            tableBoardPanel = table;
            controllerForm = theForm;


            tableBoardPanel.Controls.Clear();
            tableBoardPanel.RowCount = 3;
            tableBoardPanel.ColumnCount = 3;
            // this allows for a max/min size based on desired max/min sizes of cells
            tableBoardPanel.MaximumSize = new Size(300 * tableBoardPanel.RowCount, 300 * tableBoardPanel.RowCount);
            tableBoardPanel.MinimumSize = new Size(20 * tableBoardPanel.RowCount, 20 * tableBoardPanel.RowCount);

            pnl_zoom.MouseWheel += (sendr, args) =>
            {
                if (!pnl_zoom.ClientRectangle.Contains(args.Location))
                    return;

                // the desired zoom factor is by 30% each scroll
                // my mouse registers 120 for each delta
                float zoomFactor;
                if (args.Delta > 0)
                    zoomFactor = 1.3f * (args.Delta / 120f);
                else
                    zoomFactor = 0.7f * (args.Delta / -120f);

                float newSize = currentBoardSize * zoomFactor;
                currentBoardSize = newSize;
                Console.WriteLine("New Size: " + newSize);
                ResizeBoardDisplay(newSize);
            };


            pnl_zoom.MouseDown += Pnl_zoom_MouseDown;
            pnl_zoom.MouseUp += Pnl_zoom_MouseUp;
            pnl_zoom.MouseMove += Pnl_zoom_MouseMove;


            pnl_zoom.MouseEnter += (sendr, args) => pnl_zoom.Focus(); //focus to grab mouse wheel events over board
            pnl_zoom.MouseLeave += (sendr, args) => controllerForm.Focus(); // prevent zoom if the mouse isn't over the board



            //tableLayoutPanel1.Controls.Add(new Label() {Text = "SAMPLE"}, 1, 1);

            Image grass = Tools.ImageTools.ResizeImage(Image.FromFile("Assets/grass.jpg"), 100, 100);

            #region Generate the cells and images for the board

            for (int row = 0; row < tableBoardPanel.RowCount; row++)
            for (int col = 0; col < tableBoardPanel.ColumnCount; col++)
            {

                var cellPictureBox = new PictureBox()
                {
                    Image = grass,
                    Padding = new Padding(1, 1, 1, 1),
                    Margin = Padding.Empty,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.AliceBlue,
                    //Size = new Size(48, 48),
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    MinimumSize = new Size(20, 20),
                };

                cellPictureBox.MouseWheel += Cell_MouseWheel;

                // any way to have them simply pass up these events to be controlled by the zoom panel?
                cellPictureBox.MouseDown += Pnl_zoom_MouseDown;
                cellPictureBox.MouseUp += Pnl_zoom_MouseUp;
                cellPictureBox.MouseMove += Pnl_zoom_MouseMove;

                cellPictureBox.MouseEnter += (sendr, args) =>
                {
                    cellPictureBox.BackColor = Color.Cyan;
                    cellPictureBox.Padding = new Padding(5, 5, 5, 5);
                };
                cellPictureBox.MouseLeave += (sendr, args) =>
                {
                    cellPictureBox.BackColor = Color.AliceBlue;
                    cellPictureBox.Padding = new Padding(1, 1, 1, 1);
                };
                tableBoardPanel.Controls.Add(cellPictureBox, col, row);
            }

            tableBoardPanel.RowStyles.Clear();
            for (int row = 0; row < tableBoardPanel.RowCount; row++)
            {
                tableBoardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); //  / tableBoardPanel.RowCount

            }
            tableBoardPanel.ColumnStyles.Clear();
            for (int col = 0; col < tableBoardPanel.ColumnCount; col++)
            {
                tableBoardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f )); // / tableBoardPanel.ColumnCount
            }

            #endregion


            ResizeBoardDisplay(currentBoardSize);
        }


        private void Cell_MouseWheel(object sender, MouseEventArgs e)
        {
            Console.WriteLine(e.Delta);
        }


        private void ResizeBoardDisplay(float newRectSize)
        {
            //tableLayoutPanel1.Scale(new SizeF(newSize, newSize));
            int newSize = (int)newRectSize;


            // this system assumes a square board with square cells
            if (newSize > tableBoardPanel.MaximumSize.Height)
                newSize = tableBoardPanel.MaximumSize.Height;
            else if (newSize < tableBoardPanel.MinimumSize.Height)
                newSize = tableBoardPanel.MinimumSize.Height;

            Console.WriteLine("Resizing cells to " + currentBoardSize);
            tableBoardPanel.SuspendLayout();

            int oldSize = tableBoardPanel.Height;

            tableBoardPanel.Height = newSize;
            tableBoardPanel.Width = newSize;

            // to center the zoom lets adjust the board position equally in each direction

            int sizeDiff = oldSize - newSize;

            tableBoardPanel.Top += sizeDiff / 2;
            tableBoardPanel.Left += sizeDiff / 2;

            tableBoardPanel.Invalidate();
            tableBoardPanel.ResumeLayout();

            currentBoardSize = newSize;

        }

        private void Pnl_zoom_MouseMove(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && IsDraggingBoard)
            {
                pnl_zoom.Focus();
                
                tableBoardPanel.Left = Cursor.Position.X + DragOffset.Width;
                tableBoardPanel.Top = Cursor.Position.Y + DragOffset.Height;
                
                Console.WriteLine("Moving board w/ offset " + DragOffset.Width + " " + DragOffset.Height + " to " + tableBoardPanel.Left + " " + tableBoardPanel.Top);
            }
        }

        private void Pnl_zoom_MouseUp(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && IsDraggingBoard)
            {
                pnl_zoom.Focus();

                // stop the drag process
                IsDraggingBoard = false;
                controllerForm.Cursor = Cursors.Default;
                Cursor.Position = DragInitialCursorLocation;
                Cursor.Show();
            }
        }

        private void Pnl_zoom_MouseDown(object sender, MouseEventArgs args)
        {
            // make sure we dont trigger a drag more than once
            if (args.Button == MouseButtons.Left && !IsDraggingBoard)
            {
                pnl_zoom.Focus();

                // start the drag process
                IsDraggingBoard = true;
                DragOffset = new Size(tableBoardPanel.Left - Cursor.Position.X, tableBoardPanel.Top - Cursor.Position.Y);

                Console.WriteLine("Beginning drag w/ offset " + DragOffset.Width + " " + DragOffset.Height);
                controllerForm.Cursor = Cursors.SizeAll;
                DragInitialCursorLocation = Cursor.Position;
                Cursor.Hide();
            }
        }

        public void ZoomToFit()
        {
            // get the smallest dimension so that the entire board remains within view
            int newSize = pnl_zoom.Height > pnl_zoom.Width ? pnl_zoom.Width : pnl_zoom.Height;

            ResizeBoardDisplay(newSize);

            // recenter the board as well, do it after the resize because otherwise the center will be off
            RecenterBoardDisplay();
        }


        public void RecenterBoardDisplay()
        {
            tableBoardPanel.Location = new Point(
                (int)((pnl_zoom.Width / 2f) - (tableBoardPanel.Width / 2f)),
                (int)((pnl_zoom.Height / 2f) - (tableBoardPanel.Width / 2f))
            );
        }
    }
}
