using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GDUIPlus_turial
{
    public partial class oscilloscope : Form
    {
        LinkedList<Point> m_points = new LinkedList<Point>();
        LinkedList<PointF> m_pointFs = new LinkedList<PointF>();
        int m_x = 0;
        Random m_r = new Random();
        //前两个为x轴的范围，初始为[0, 1],
        //后两个为y轴的范围，初始为[0, 1].
        float[] m_rangeX = new float[2];
        float[] m_rangeY = new float[2];
        float[] m_rangeXOld = new float[2];
        float[] m_rangeYOld = new float[2];
        bool m_rangeXChangeFlag = false;
        bool m_rangeYChangeFlag = false;
        int m_height;
        int m_width;
        bool m_windowSizeChange = false;

        public oscilloscope()
        {
            InitializeComponent();
            m_rangeX[0] = 0;
            m_rangeX[1] = 1.0f;
            m_rangeY[0] = 0;
            m_rangeY[1] = 1.0f;
            m_rangeXOld[0] = 0f;
            m_rangeXOld[1] = 1.0f;
            m_rangeYOld[0] = 0f;
            m_rangeYOld[1] = 1.0f;
            m_height = this.ClientSize.Height;
            m_width = this.ClientSize.Width;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.Paint += new PaintEventHandler(Form1_Paint);
        }
        private void addNewPoint(float x, float y, System.Windows.Forms.PaintEventArgs e)
        {
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;
            if ((height != m_height) || (width != m_width))
            {
                m_windowSizeChange = true;
            }
            if (x > m_rangeX[1])
            {
                m_rangeXOld[1] = m_rangeX[1];               
                m_rangeX[1] = x + (int)(0.5 * Math.Abs(x));                
                m_rangeXChangeFlag = true;

            }
            else if (x < m_rangeX[0])
            {
                m_rangeXOld[0] = m_rangeX[0];
                m_rangeX[0] = x - (int)(0.5 * Math.Abs(x));
                m_rangeXChangeFlag = true;
            }                
            if (y > m_rangeY[1])
            {
                m_rangeYOld[1] = m_rangeY[1];
                m_rangeY[1] = y + (int)(0.5 * Math.Abs(y));
                m_rangeYChangeFlag = true;
            }               
            else if (y < m_rangeY[0])
            {
                m_rangeYOld[0] = m_rangeY[0];
                m_rangeY[0] = y - (int)(0.5 * Math.Abs(y));
                m_rangeYChangeFlag = true;
            }

            if ((m_rangeXChangeFlag || m_rangeYChangeFlag || m_windowSizeChange) && (m_pointFs.Count != 0))
            {
                float x_pointf = 0.0f;
                float y_pointf = 0.0f;
                int xWindow = 0;
                int yWindow = 0;
                m_points.Clear();
                for (int i = 0; i < m_pointFs.Count; i++)
                {
                    PointF point_f = m_pointFs.ElementAt<PointF>(i);
                    x_pointf = point_f.X;
                    y_pointf = point_f.Y;

                    xWindow = (int)((x_pointf - m_rangeX[0]) * width * 0.8 / (m_rangeX[1] - m_rangeX[0]) + 0.1 * width);
                    yWindow = (int)((y_pointf - m_rangeY[0]) * height * 0.8 / (m_rangeY[1] - m_rangeY[0]) + 0.1 * height);
                    yWindow = height - yWindow;

                    m_points.AddLast(new Point(xWindow, yWindow));
                }
                m_pointFs.AddLast(new PointF(x, y));
                //新加入的点在屏幕上的像素点位置
                xWindow = (int)((x - m_rangeX[0]) * width * 0.8 / (m_rangeX[1] - m_rangeX[0]) + 0.1 * width);

                yWindow = (int)((y - m_rangeY[0]) * height * 0.8 / (m_rangeY[1] - m_rangeY[0]) + 0.1 * height);
                yWindow = height - yWindow;


                m_points.AddLast(new Point(xWindow, yWindow));
            }
            else
            {
                m_pointFs.AddLast(new PointF(x, y));
                //新加入的点在屏幕上的像素点位置
                int xWindow = (int)((x - m_rangeX[0]) * width * 0.8 / (m_rangeX[1] - m_rangeX[0]) + 0.1 * width);
                
                int yWindow = (int)((y - m_rangeY[0]) * height * 0.8 / (m_rangeY[1] - m_rangeY[0]) + 0.1 * height);
                yWindow = height - yWindow;

                m_points.AddLast(new Point(xWindow, yWindow));
            }
        }

        private int getBitsNum(float n)
        {
            int bits = 0;
            if (Math.Abs(n) > 1)
            {
                float nTmp = Math.Abs(n);
                while (nTmp / 10 > 1)
                {
                    nTmp /= 10;
                    bits++;
                }
                return bits;
            }
            else if(Math.Abs(n) == 0)
            {
                return bits;
            }
            else if(Math.Abs(n) < 1)
            {
                float nTmp = Math.Abs(n);
                while (nTmp * 10 < 1)
                {
                    bits--;
                }
                return bits;
            }
            else
            {
                return bits;
            }
            
        }

        private void drawAxis(int height, int width, System.Windows.Forms.PaintEventArgs e)
        {
            //由于y坐标是沿屏幕向下的，所以要将y轴坐标反向
            int xAxisBegin = (int) (width * 0.1);
            int xAxisEnd = (int)(width * 0.9);

            int yAxisBegin = (int)(height - height * 0.1);
            int yAxisEnd = (int)(height - height * 0.9);

            //x轴箭头
            e.Graphics.DrawLine(Pens.Red, new Point(xAxisBegin, yAxisBegin), new Point(xAxisEnd, yAxisBegin));
            Point[] pointsXArrow = new Point[3];
            pointsXArrow[0] = new Point(xAxisEnd, yAxisBegin);
            pointsXArrow[1] = new Point((int)(0.99 * (xAxisEnd - xAxisBegin) + xAxisBegin), (int)(yAxisBegin + 0.01 * (yAxisBegin - yAxisEnd)));
            pointsXArrow[2] = new Point((int)(0.99 * (xAxisEnd - xAxisBegin) + xAxisBegin), (int)(yAxisBegin - 0.01 * (yAxisBegin - yAxisEnd)));
            e.Graphics.FillPolygon(Brushes.Red, pointsXArrow);

            //y轴箭头
            e.Graphics.DrawLine(Pens.Green, new Point(xAxisBegin, yAxisBegin), new Point(xAxisBegin, yAxisEnd));
            Point[] pointsYArrow = new Point[3];
            pointsYArrow[0] = new Point(xAxisBegin, yAxisEnd);
            pointsYArrow[1] = new Point((int)(xAxisBegin - 0.01 * (xAxisEnd - xAxisBegin)), (int)(0.01 * (yAxisBegin - yAxisEnd) + yAxisEnd));
            pointsYArrow[2] = new Point((int)(xAxisBegin + 0.01 * (xAxisEnd - xAxisBegin)), (int)(0.01 * (yAxisBegin - yAxisEnd) + yAxisEnd));
            e.Graphics.FillPolygon(Brushes.Green, pointsYArrow);
            float fontSize = this.Font.Size;
            int AxisNum = 10;
            float strf = 0.0f;
            string str = "";
            //y坐标及其虚线
            int offsetX;
            if (m_rangeY[1] > m_rangeY[0])
                offsetX = getBitsNum(m_rangeY[1]);
            else
                offsetX = getBitsNum(m_rangeY[0]);            
            int YPosTmp = 0;
            string MarkY = "Y/10^" + offsetX;
            e.Graphics.DrawString(MarkY, this.Font, Brushes.Green, xAxisBegin, (float)(yAxisEnd - 0.05 * (yAxisBegin - yAxisEnd)));
            Pen myPen = new Pen(Color.Silver, 2.0f);
            myPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            for (int i = 0; i < AxisNum + 1; ++i)
            {
                strf = m_rangeY[0] + i * (m_rangeY[1] - m_rangeY[0]) / AxisNum;
                strf = (float)(strf / Math.Pow(10, offsetX));
                str = strf.ToString("#0.000");
                //str += "e" + offsetX.ToString();
                YPosTmp = (int)((yAxisBegin - i * (yAxisBegin - yAxisEnd) / 10.0f) - fontSize / 2);
                if (strf >= 0)
                    e.Graphics.DrawString(str, this.Font, Brushes.Green, xAxisBegin - (2 + 3) * fontSize, YPosTmp);
                else
                    e.Graphics.DrawString(str, this.Font, Brushes.Green, xAxisBegin - (3 + 3) * fontSize, YPosTmp);
                if(i != 0)
                    e.Graphics.DrawLine(myPen, xAxisBegin, (yAxisBegin - i * (yAxisBegin - yAxisEnd) / 10.0f), xAxisEnd, (yAxisBegin - i * (yAxisBegin - yAxisEnd) / 10.0f));
            }
            //x坐标及其虚线
            int offsetY;
            if (m_rangeX[1] > m_rangeX[0])
                offsetY = getBitsNum(m_rangeX[1]);
            else
                offsetY = getBitsNum(m_rangeX[0]);
            int XPosTmp = 0;
            string MarkX = "X/10^" + offsetY;
            e.Graphics.DrawString(MarkX, this.Font, Brushes.Red, (float)(xAxisEnd + 0.01 * (xAxisEnd - xAxisBegin)), (float)(yAxisBegin - 0.02 * (yAxisBegin - yAxisEnd)));
            for (int i = 0; i < AxisNum + 1; ++i)
            {
                strf = m_rangeX[0] + i * (m_rangeX[1] - m_rangeX[0]) / AxisNum;
                strf = (float)(strf / Math.Pow(10, offsetY));
                str = strf.ToString("#0.000");
                //str += "e" + offsetY.ToString();
                XPosTmp = (int)(xAxisBegin + i * (xAxisEnd - xAxisBegin) / 10.0f);
                if (strf >= 0)
                    e.Graphics.DrawString(str, this.Font, Brushes.Red, XPosTmp - (2) * fontSize, (float)(yAxisBegin + 0.02 * (yAxisBegin - yAxisEnd)));
                else
                    e.Graphics.DrawString(str, this.Font, Brushes.Red, XPosTmp - (3) * fontSize, (float)(yAxisBegin + 0.02 * (yAxisBegin - yAxisEnd)));
                if (i != 0)
                    e.Graphics.DrawLine(myPen, XPosTmp, yAxisBegin, XPosTmp, yAxisEnd);
            }
            //显示y轴上0坐标位置
            //e.Graphics.DrawString("0.000", this.Font, Brushes.Red, xAxisBegin - (2 + 3) * fontSize, (int)(yAxisBegin - (0 - m_rangeY[0]) / (m_rangeY[1] - m_rangeY[0]) * (yAxisBegin - yAxisEnd)) - fontSize / 2);

            //x轴箭头
            //e.Graphics.DrawLine(Pens.Red, new Point(xAxisBegin, yAxisBegin), new Point(xAxisEnd, yAxisBegin));
            //e.Graphics.DrawLine(Pens.Red, new Point(xAxisEnd, yAxisBegin), new Point((int)(0.99 * (xAxisEnd - xAxisBegin) + xAxisBegin), (int)(yAxisBegin + 0.01 * (yAxisBegin - yAxisEnd))));
            //e.Graphics.DrawLine(Pens.Red, new Point(xAxisEnd, yAxisBegin), new Point((int)(0.99 * (xAxisEnd - xAxisBegin) + xAxisBegin), (int)(yAxisBegin - 0.01 * (yAxisBegin - yAxisEnd))));

            ////y轴箭头
            //e.Graphics.DrawLine(Pens.Green, new Point(xAxisBegin, yAxisBegin), new Point(xAxisBegin, yAxisEnd));
            //e.Graphics.DrawLine(Pens.Green, new Point(xAxisBegin, yAxisEnd), new Point((int)(xAxisBegin - 0.01 * (xAxisEnd- xAxisBegin)), (int)(0.01 * (yAxisBegin - yAxisEnd) + yAxisEnd)));
            //e.Graphics.DrawLine(Pens.Green, new Point(xAxisBegin, yAxisEnd), new Point((int)(xAxisBegin + 0.01 * (xAxisEnd - xAxisBegin)), (int)(0.01 * (yAxisBegin - yAxisEnd) + yAxisEnd)));
        }

        private void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            int width = this.Width;
            int height = this.Height;
            Size size = this.ClientSize;
            width = size.Width;
            height = size.Height;           

            float x = m_x;
            float y = m_x * (float)Math.Sin(m_x * 0.01);
            this.addNewPoint(x, y, e);
            m_x++;

            Console.WriteLine(m_points.Count);

            for (int index = 0; index < m_points.Count - 1; ++index)
            {
                Point tmp1 = m_points.ElementAt<Point>(index);
                Point tmp2 = m_points.ElementAt<Point>(index + 1);
                e.Graphics.DrawLine(Pens.Red, tmp1, tmp2);
            }

            this.drawAxis(height, width, e);
            //float x_tmp = 3.12313f;
            //string xs = x_tmp.ToString();
            //e.Graphics.DrawString(xs, this.Font, Brushes.Blue, 100, 100);

            //string xs = x.ToString();
            //e.Graphics.DrawString(xs, this.Font, Brushes.Blue, 100, 100);

            //e.Graphics.DrawPolygon(Pens.Red, points);
            //e.Graphics.FillPolygon(Brushes.Red, points);
            //e.Graphics.DrawLines(Pens.Red, points);
            //e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, 300, 300));

            //e.Graphics.DrawArc(Pens.Red, new Rectangle(0, 0, 100, 100), 50, 225);
            //e.Graphics.DrawPie(Pens.Red, new Rectangle(0, 0, 100, 100), 0, 90);
            //e.Graphics.FillPie(Brushes.Red, new Rectangle(0, 0, 100, 100), 0, 90);

            //string cat = "Hey Im a cat";
            //e.Graphics.DrawString(cat, this.Font, Brushes.Black, 100, 100);
            //e.Graphics.DrawLine(Pens.Red, new Point(0, 0), new Point(120, 95));
            //e.Graphics.DrawLine(Pens.Red, 50, 43, 126, 211);

            //e.Graphics.DrawLines()

            //e.Graphics.DrawImage(Properties.Resources.FTB, 50, 50);
            //e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, 50, 75));
            //e.Graphics.DrawRectangle(Pens.Red, 50, 50, 75, 100);

        }

        private void tnrAppTimer_Tick(object sender, EventArgs e)
        {
            this.Refresh();
        }   
    }
}
