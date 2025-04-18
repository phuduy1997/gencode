using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SqlGeneratorApp.Forms
{
    public partial class WaitingForm : Form
    {
        private string _statusText;
        private readonly Label _lblStatus;
        private readonly ProgressBar _progressBar;
        private readonly PictureBox _loadingPictureBox;
        private readonly System.Windows.Forms.Timer _timer;
        private int _progressValue = 0;
        private int _rotationAngle = 0;

        public WaitingForm(string statusText = "Đang tạo file...")
        {
            InitializeComponent();
            _statusText = statusText;
            
            // Set form properties
            this.Text = "Vui lòng chờ";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 150);
            this.ShowInTaskbar = false;
            
            // Create status label
            _lblStatus = new Label
            {
                Text = _statusText,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.None,
                Location = new Point(20, 20),
                Size = new Size(360, 30)
            };
            
            // Create progress bar
            _progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Location = new Point(20, 60),
                Size = new Size(360, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            
            // Create loading animation
            _loadingPictureBox = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(this.ClientSize.Width / 2 - 16, 90),
                BackColor = Color.Transparent
            };
            
            // Create a timer to update loading animation
            _timer = new System.Windows.Forms.Timer(this.components)
            {
                Interval = 100
            };
            _timer.Tick += Timer_Tick;
            
            // Add controls to form
            this.Controls.Add(_lblStatus);
            this.Controls.Add(_progressBar);
            this.Controls.Add(_loadingPictureBox);
            
            // Start the timer
            _timer.Start();
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update progress bar
            _progressValue = (_progressValue + 3) % 100;
            _progressBar.Value = _progressValue;
            
            // Update loading animation by rotating an arrow or similar shape
            _rotationAngle = (_rotationAngle + 30) % 360;
            _loadingPictureBox.Image = DrawLoading(_rotationAngle);
        }
        
        private Bitmap DrawLoading(int angle)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                Point center = new Point(16, 16);
                int radius = 12;
                
                // Draw circle
                g.DrawEllipse(new Pen(Color.LightGray, 2), center.X - radius, center.Y - radius, radius * 2, radius * 2);
                
                // Draw rotating segment
                float startAngle = angle;
                float sweepAngle = 90;
                g.DrawArc(new Pen(Color.Blue, 3), center.X - radius, center.Y - radius, radius * 2, radius * 2, startAngle, sweepAngle);
            }
            return bmp;
        }
        
        public void SetStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetStatus), status);
                return;
            }
            
            _statusText = status;
            _lblStatus.Text = _statusText;
        }
        
        public void SetProgress(int progress)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(SetProgress), progress);
                return;
            }
            
            _progressBar.Value = Math.Min(100, Math.Max(0, progress));
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer.Stop();
            base.OnFormClosing(e);
        }
    }
} 