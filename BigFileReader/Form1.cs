using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BigFileReader
{
    public partial class Form1 : Form
    {
        private FileStream stream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        private Graphics rtg;
        private double lineHeight;
        private long lineCount;
        private double maxUsableLines;
        private double maxArea;
        private double maxCharsPerLine;

        public Form1()
        {
            InitializeComponent();
            rtg = richTextBox1.CreateGraphics();
            setTextBoxProperties();
        }

        private void setTextBoxProperties()
        {
            if (rtg != null)
            {
                string testString = "e";//"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                SizeF testSize = rtg.MeasureString(testString, richTextBox1.Font);
                textBox1.Text = "(" + testSize.Width.ToString() + "," + testSize.Height.ToString() + ")";
                maxCharsPerLine = Math.Floor(richTextBox1.Size.Width / (testSize.Width / testString.Length));
                lineHeight = testSize.Height;
                double maxLines = richTextBox1.Size.Height / lineHeight;
                maxUsableLines = Math.Floor(maxLines);
            }
            maxArea = richTextBox1.Size.Width * richTextBox1.Size.Height;
            BoxBox.Text = "(" + richTextBox1.Size.Width.ToString() + "," + richTextBox1.Size.Height.ToString() + ")";
        }

        private void LineRenderTextBox()
        {
            if(stream == null) { richTextBox1.Text = "No loaded file"; return; }
            //richTextBox1.Clear();
            //stream.Seek(0, SeekOrigin.Begin);
            stream.Position = 0;
            streamReader.DiscardBufferedData();
            for(int i = 0; i < lineCount; i++) //This may be quite slow, but it is reliable
            {
                if (streamReader.ReadLine() == null)
                    return;
            }
            Count.Value = lineCount;
            if(rtg != null)
            {
                StringBuilder sb = new StringBuilder();
                double usableLines;
                for (usableLines = maxUsableLines; usableLines > 0;)
                {
                    string line = streamReader.ReadLine();
                    if (line == null) { break; }
                    if (line == "") { usableLines--; sb.AppendLine(line); continue; }
                    var size = rtg.MeasureString(line, richTextBox1.Font);
                    double usedLines = Math.Ceiling(size.Width / richTextBox1.Size.Width); //Line wrap count
                    //line += "(" + usedLines.ToString() + "," + size.Width.ToString() + ")";
                    if (usedLines < usableLines || usableLines == maxUsableLines) // If Space or only option
                    {
                        sb.AppendLine(line);
                        usableLines -= usedLines;
                    } else { break; }
                }
                try
                {
                    LineLeft.Value = (decimal)usableLines;
                }
                catch (Exception)
                {
                    LineLeft.Value = 0;
                }
                
                richTextBox1.Text = sb.ToString();
            }
        }
        private void RenderTextBox()
        {
            richTextBox1.Clear();
            if (rtg != null)
            {
                StringBuilder sb = new StringBuilder();
                for (double usableLines = maxUsableLines; usableLines > 0;)
                {
                    string line = streamReader.ReadLine();
                    if(line == null) { break; }
                    var size = rtg.MeasureString(line, richTextBox1.Font);
                    if(size.Width * size.Height > maxArea && usableLines < maxUsableLines)
                    {
                        stream.Seek(line.Length * -1, SeekOrigin.Current);
                        break;
                    }
                    double usedLines = Math.Ceiling(size.Width / richTextBox1.Size.Width);
                    if (usedLines < usableLines || usableLines == maxUsableLines) // If Space or only option
                    {
                        sb.AppendLine(line);
                        usableLines -= usedLines;
                    }
                    else
                    {
                        
                    }
                }
                richTextBox1.Text = sb.ToString();
                try
                {
                    stream.Seek((richTextBox1.Text.Length * sizeof(char)) * -1, SeekOrigin.Current);
                }
                catch (Exception)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string path = openFileDialog1.FileName;
            if (stream != null)
            {
                stream.Close();
                richTextBox1.Text = "Already got a file: " + stream.Name;
            }

            stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            Seekable.Checked = stream.CanSeek;
            streamReader = new StreamReader(stream);
            streamWriter = new StreamWriter(stream);

            lineCount = 0;
            LineRenderTextBox();
            //RenderTextBox();

        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(stream == null) { return; }
            if(e.KeyData == Keys.Up)
            {
                lineCount = lineCount > 0 ? lineCount - 1 : 0;
                LineRenderTextBox();
                /*
                int textlength = richTextBox1.Text.Length > 0 ? richTextBox1.Text.Length : sizeof(char);
                try
                {
                    stream.Seek((textlength * sizeof(char)) * -1, SeekOrigin.Current);
                }
                catch (Exception)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                } 
                RenderTextBox(); 
                */
            } else if (e.KeyData == Keys.Down)
            {
                lineCount++;
                LineRenderTextBox();
                /*
                int textlength = richTextBox1.Text.Length > 0 ? richTextBox1.Text.Length : sizeof(char);
                stream.Seek((textlength * sizeof(char)), SeekOrigin.Current);
                RenderTextBox();
                */
            }
        }

        private void richTextBox1_Resize(object sender, EventArgs e)
        {
            setTextBoxProperties();
            LineRenderTextBox();
            //RenderTextBox();
        }

        private void FntBtn_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {
            richTextBox1.Font = fontDialog1.Font;
            richTextBox1.ForeColor = fontDialog1.Color;

            FontInfo.Text = richTextBox1.Font.ToString();

            setTextBoxProperties();
            LineRenderTextBox();
        }

        private void Count_ValueChanged(object sender, EventArgs e)
        {
            if(Count.Value >= 0) { lineCount = (int)Count.Value; }
            LineRenderTextBox();
        }
    }
}
