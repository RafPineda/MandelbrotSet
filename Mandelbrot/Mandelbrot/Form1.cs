/* Program that displays the Mandelbrot set. User can input iteration count.
 * Mandelbrot set will be displayed to window based on Iterations done. Notifies user of incorrect input. 
 * Additionally, user can zoom in by drag selecting a region in the picturebox. 
 * The program will then display that section of the mandelbrot set where the user rectangle was drawn. 
 * While Drawing, the mouse will indicate a "work in progress" symbol to let user know program is working. 
 **/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        double x, x0, y, y0, z, xCount, yCount, xSquared, ySquared, twoXY; // x, y, z and c components for Mandelbrot function
        const double xDefault = -2.0, yDefault = -1.5, scale = (1.0 / 333); //default values for real and imaginary plane, going from -2 to 1 and -1.5 to 1.5
        double oldScaleX = scale, oldScaleY = scale, newScaleX = 0.0, newScaleY = 0.0;  //scale zoom values for zoom functionality  
        double oldX = xDefault, oldY = yDefault, newX = 0.0, newY = 0.0;                //x and y values for new range of zoom
        int iterCnt, px, py;   // iterations count, pixel position     
        bool divergent, startOK, isMousePress = false, canvasDrawn = false; //bool values to check if a point diverges, if user input is good
        Point x1y1, x2y2; //x1y1 represents where user first clicked with mouse, x2y2 represents where user released
        Rectangle rect;


        public Form1()
        {
            InitializeComponent();
            Reset.Enabled = false;
        }

       
        /********************************/
        /*  Mandel function. Creates and draws the mandelbrot graph to the screen
         *  Receives input of xscale and y scale, along with where the initial point of the function for
         *  the imaginary plane is (normally x= -2 to 0 and y= -1.5 to 1.5). By inputting different values aside from
         *  default x/default y, we can zoom into a specific region using the iterations from user input. 
         */

        public void makeMandel(double scaleX, double scaleY, double xStartingPnt, double yStartingPnt)
        {
            Bitmap bm = new Bitmap(Canvas.ClientSize.Width, Canvas.ClientSize.Height);
            using (Graphics g = Graphics.FromImage(bm))
            {
                for (int i = 0; i < 1000; i++)
                {
                    yCount = yStartingPnt + i * scaleY;
                    px = 0;
                    xCount = xStartingPnt;

                    for (int j = 0; j < 1000; j++)
                    {
                        divergent = false;
                        x0 = xCount;
                        y0 = yCount;
                        x = 0;
                        y = 0;


                        for (int k = 0; k < iterCnt; k++) //calculate the real and imaginary values 
                        {
                            xSquared = x * x;
                            ySquared = y * y;
                            twoXY = 2 * x * y;
                            x = xSquared - ySquared + x0; //new x = x squared - y squared + old x
                            y = twoXY + y0;               //new y = 2 x y + old y  
                            xSquared = x * x;
                            ySquared = y * y;
                            z = xSquared + ySquared;            //check if z > 20 for divergence of consequent points
                            if (z > 20)
                            {
                                divergent = true;
                            }

                            if (divergent == true)
                            {
                                break;
                            }
                        }

                        if (divergent == false) //point does not diverge
                        {
                            bm.SetPixel(j, i, Color.Cyan);
                        }

                        else //point diverges
                        {
                            bm.SetPixel(j, i, Color.LightSalmon);

                        }

                        px++;       // increment x pixel by one, and increase x count by .004
                        xCount = xStartingPnt + j * scaleX;

                    }
                    py++;       // increment y pixel by one, and increase y count by .004                        
                }
                Canvas.Image = bm;
                canvasDrawn = true;
            }
        }


        /********************************/

        public bool iterationsOK() //return true if iteration value is valid, false otherwise. 
        {
            if (int.TryParse((Iterations.Text), out iterCnt)) //if user values are not valid(not an interger), return error statement
            {
                if (iterCnt <= 0 || iterCnt > 8192) //errmsg if user requests for "negative" iterations, or too many.
                {
                    MessageBox.Show("Invalid Entry. \nPlease Try again using a valid iteration value (Positive).");
                    return false;
                }
                return true;
            }
            else
            {
                MessageBox.Show("Invalid Entry. \nPlease Try again using a valid integer iteration value (Positive).");
                return false;
            }

        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Enter.Enabled = false; //disable button click until calculations are done
            Reset.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            makeMandel(scale, scale, xDefault, yDefault);
            oldScaleX = scale;
            oldScaleY = scale;
            oldX = xDefault;
            oldY = yDefault;
            Enter.Enabled = true; // enable button click again.
            Reset.Enabled = true;
        } //reset to original mandelbrot set with original iteration value
        private void Enter_Click(object sender, EventArgs e) //upon button press, take in new iteration count and calculate Mandelbrot set. 
        {
            Enter.Text = "Making...";
            Enter.Enabled = false; //disable button click until calculations are done
            Reset.Enabled = false;
            startOK = iterationsOK(); //if iterations is invalid, return error statement
            if (startOK)
            {
                Cursor.Current = Cursors.WaitCursor;
                makeMandel(scale, scale, xDefault, yDefault);
                oldScaleX = scale;
                oldScaleY = scale;
                oldX = xDefault;
                oldY = yDefault;
                Reset.Enabled = true;
                Enter.Text = "Make Mandelbrot Set";
            }
            Enter.Enabled = true; // enable button click again.
        }


        private void Canvas_MouseDown(object sender, MouseEventArgs e) //track initial mouse click
        {
            if (canvasDrawn != false)
            {
                isMousePress = true;
                x1y1 = e.Location;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) //track mouse movement, drawing a box as it moves
        {
            if (isMousePress == true)
            {
                x2y2 = e.Location;
                Refresh();
            }
        }
        private void Canvas_MouseUp(object sender, MouseEventArgs e) //on mouse release, zoom in
        {
            if (canvasDrawn != false)
            {
                if (isMousePress == true)
                {
                    x2y2 = e.Location;
                    isMousePress = false;

                    /*
                     * Once we draw a rectangle, zoom into the area using x,y points generated by user
                     *  
                     * */
                    Canvas.Enabled = false;
                    Enter.Text = "Zooming...";
                    Enter.Enabled = false; //disable button click until calculations are done
                    Reset.Enabled = false;

                    double zoomX = oldX + Math.Min(x1y1.X, x2y2.X) * (oldScaleX); //generate a scaled zoom and scaled x,y coordinates based on the porportion from the user box to the current window
                    double zoomY = oldY + Math.Min(x1y1.Y, x2y2.Y) * (oldScaleY);
                    oldX = zoomX;
                    oldY = zoomY;
                    double zoomScaleX = oldScaleX * (Math.Abs(x2y2.X - x1y1.X) / 999.000);
                    double zoomScaleY = oldScaleY * (Math.Abs(x2y2.Y - x1y1.Y) / 999.000);
                    oldScaleX = zoomScaleX;
                    oldScaleY = zoomScaleY;

                    Cursor.Current = Cursors.WaitCursor;
                    makeMandel(zoomScaleX, zoomScaleY, zoomX, zoomY);

                    Canvas.Enabled = true;
                    Enter.Enabled = true; // enable button click again.
                    Reset.Enabled = true;
                    Enter.Text = "Make Mandelbrot Set";

                }
            }

        }

        private void Canvas_Paint(object sender, PaintEventArgs e) //draw the rectangle red, so the user can decide where to zoom in
        {
            if (rect != null && canvasDrawn != false && isMousePress == true)
            {
                e.Graphics.DrawRectangle(Pens.Red, GetRect());
            }

        }

        private Rectangle GetRect() //make a rectangle
        {
            rect = new Rectangle();
            rect.X = Math.Min(x1y1.X, x2y2.X);
            rect.Y = Math.Min(x1y1.Y, x2y2.Y);
            rect.Width = Math.Abs(x1y1.X - x2y2.X);
            rect.Height = Math.Abs(x1y1.Y - x2y2.Y);

            return rect;
        }

    }
}