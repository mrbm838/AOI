namespace SingleAxisMotion
{
    partial class AxisForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.Label_CurPos = new System.Windows.Forms.Label();
            this.Button_LeftMove = new System.Windows.Forms.Button();
            this.ComboBox_MotionSpace = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Button_Save = new System.Windows.Forms.Button();
            this.ComboBox_Point = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.TextBox_PointPos = new System.Windows.Forms.TextBox();
            this.Button_MovePoint = new System.Windows.Forms.Button();
            this.CheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.Button_Servo = new System.Windows.Forms.Button();
            this.Button_RightMove = new System.Windows.Forms.Button();
            this.Button_Reversion = new System.Windows.Forms.Button();
            this.Button_Stop = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F);
            this.label1.Location = new System.Drawing.Point(193, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "轴当前位置：";
            // 
            // Label_CurPos
            // 
            this.Label_CurPos.AutoSize = true;
            this.Label_CurPos.Location = new System.Drawing.Point(312, 40);
            this.Label_CurPos.Name = "Label_CurPos";
            this.Label_CurPos.Size = new System.Drawing.Size(11, 12);
            this.Label_CurPos.TabIndex = 1;
            this.Label_CurPos.Text = "0";
            // 
            // Button_LeftMove
            // 
            this.Button_LeftMove.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_LeftMove.Location = new System.Drawing.Point(437, 81);
            this.Button_LeftMove.Name = "Button_LeftMove";
            this.Button_LeftMove.Size = new System.Drawing.Size(45, 46);
            this.Button_LeftMove.TabIndex = 2;
            this.Button_LeftMove.Text = "◁";
            this.Button_LeftMove.UseVisualStyleBackColor = true;
            this.Button_LeftMove.Click += new System.EventHandler(this.Button_LeftMove_Click);
            // 
            // ComboBox_MotionSpace
            // 
            this.ComboBox_MotionSpace.Font = new System.Drawing.Font("宋体", 12F);
            this.ComboBox_MotionSpace.FormattingEnabled = true;
            this.ComboBox_MotionSpace.Items.AddRange(new object[] {
            "0.01",
            "0.1",
            "0.5",
            "1",
            "5",
            "10",
            "20",
            "50",
            "100"});
            this.ComboBox_MotionSpace.Location = new System.Drawing.Point(281, 92);
            this.ComboBox_MotionSpace.Name = "ComboBox_MotionSpace";
            this.ComboBox_MotionSpace.Size = new System.Drawing.Size(121, 24);
            this.ComboBox_MotionSpace.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 12F);
            this.label2.Location = new System.Drawing.Point(193, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "运动度数：";
            // 
            // Button_Save
            // 
            this.Button_Save.Location = new System.Drawing.Point(437, 197);
            this.Button_Save.Name = "Button_Save";
            this.Button_Save.Size = new System.Drawing.Size(103, 35);
            this.Button_Save.TabIndex = 5;
            this.Button_Save.Text = "保存设置";
            this.Button_Save.UseVisualStyleBackColor = true;
            this.Button_Save.Click += new System.EventHandler(this.Button_Save_Click);
            // 
            // ComboBox_Point
            // 
            this.ComboBox_Point.Font = new System.Drawing.Font("宋体", 12F);
            this.ComboBox_Point.FormattingEnabled = true;
            this.ComboBox_Point.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.ComboBox_Point.Location = new System.Drawing.Point(281, 158);
            this.ComboBox_Point.Name = "ComboBox_Point";
            this.ComboBox_Point.Size = new System.Drawing.Size(121, 24);
            this.ComboBox_Point.TabIndex = 7;
            this.ComboBox_Point.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Point_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F);
            this.label3.Location = new System.Drawing.Point(193, 161);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "点位：";
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.TextBox_PointPos);
            this.groupBox.Controls.Add(this.Button_MovePoint);
            this.groupBox.Controls.Add(this.CheckedListBox);
            this.groupBox.Controls.Add(this.Button_Servo);
            this.groupBox.Controls.Add(this.Button_RightMove);
            this.groupBox.Controls.Add(this.label3);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Controls.Add(this.ComboBox_Point);
            this.groupBox.Controls.Add(this.Label_CurPos);
            this.groupBox.Controls.Add(this.Button_LeftMove);
            this.groupBox.Controls.Add(this.Button_Save);
            this.groupBox.Controls.Add(this.ComboBox_MotionSpace);
            this.groupBox.Controls.Add(this.label2);
            this.groupBox.Location = new System.Drawing.Point(23, 24);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(602, 247);
            this.groupBox.TabIndex = 9;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "点位设置";
            // 
            // TextBox_PointPos
            // 
            this.TextBox_PointPos.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TextBox_PointPos.Location = new System.Drawing.Point(437, 156);
            this.TextBox_PointPos.Name = "TextBox_PointPos";
            this.TextBox_PointPos.Size = new System.Drawing.Size(100, 26);
            this.TextBox_PointPos.TabIndex = 12;
            // 
            // Button_MovePoint
            // 
            this.Button_MovePoint.Location = new System.Drawing.Point(281, 197);
            this.Button_MovePoint.Name = "Button_MovePoint";
            this.Button_MovePoint.Size = new System.Drawing.Size(103, 35);
            this.Button_MovePoint.TabIndex = 11;
            this.Button_MovePoint.Text = "运动点位";
            this.Button_MovePoint.UseVisualStyleBackColor = true;
            this.Button_MovePoint.Click += new System.EventHandler(this.Button_MovePoint_Click);
            // 
            // CheckedListBox
            // 
            this.CheckedListBox.BackColor = System.Drawing.SystemColors.Control;
            this.CheckedListBox.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CheckedListBox.FormattingEnabled = true;
            this.CheckedListBox.Items.AddRange(new object[] {
            "启用点位1",
            "启用点位2",
            "启用点位3",
            "启用点位4"});
            this.CheckedListBox.Location = new System.Drawing.Point(36, 54);
            this.CheckedListBox.Name = "CheckedListBox";
            this.CheckedListBox.Size = new System.Drawing.Size(120, 88);
            this.CheckedListBox.TabIndex = 6;
            // 
            // Button_Servo
            // 
            this.Button_Servo.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Servo.Location = new System.Drawing.Point(437, 29);
            this.Button_Servo.Name = "Button_Servo";
            this.Button_Servo.Size = new System.Drawing.Size(103, 35);
            this.Button_Servo.TabIndex = 10;
            this.Button_Servo.Text = "开使能";
            this.Button_Servo.UseVisualStyleBackColor = false;
            this.Button_Servo.Click += new System.EventHandler(this.Button_Servo_Click);
            // 
            // Button_RightMove
            // 
            this.Button_RightMove.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_RightMove.Location = new System.Drawing.Point(495, 81);
            this.Button_RightMove.Name = "Button_RightMove";
            this.Button_RightMove.Size = new System.Drawing.Size(45, 46);
            this.Button_RightMove.TabIndex = 9;
            this.Button_RightMove.Text = "▷";
            this.Button_RightMove.UseVisualStyleBackColor = true;
            this.Button_RightMove.Click += new System.EventHandler(this.Button_RightMove_Click);
            // 
            // Button_Reversion
            // 
            this.Button_Reversion.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_Reversion.Location = new System.Drawing.Point(145, 277);
            this.Button_Reversion.Name = "Button_Reversion";
            this.Button_Reversion.Size = new System.Drawing.Size(126, 61);
            this.Button_Reversion.TabIndex = 11;
            this.Button_Reversion.Text = "归原";
            this.Button_Reversion.UseVisualStyleBackColor = true;
            this.Button_Reversion.Click += new System.EventHandler(this.Button_Reversion_Click);
            // 
            // Button_Stop
            // 
            this.Button_Stop.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_Stop.Location = new System.Drawing.Point(392, 277);
            this.Button_Stop.Name = "Button_Stop";
            this.Button_Stop.Size = new System.Drawing.Size(126, 61);
            this.Button_Stop.TabIndex = 12;
            this.Button_Stop.Text = "停止";
            this.Button_Stop.UseVisualStyleBackColor = true;
            this.Button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // AxisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 361);
            this.Controls.Add(this.Button_Stop);
            this.Controls.Add(this.Button_Reversion);
            this.Controls.Add(this.groupBox);
            this.Name = "AxisForm";
            this.Text = "轴";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxisForm_FormClosing);
            this.Load += new System.EventHandler(this.AxisForm_Load);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Label_CurPos;
        private System.Windows.Forms.Button Button_LeftMove;
        private System.Windows.Forms.ComboBox ComboBox_MotionSpace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Button_Save;
        private System.Windows.Forms.ComboBox ComboBox_Point;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Button Button_RightMove;
        private System.Windows.Forms.Button Button_Servo;
        private System.Windows.Forms.Button Button_Reversion;
        private System.Windows.Forms.Button Button_Stop;
        private System.Windows.Forms.CheckedListBox CheckedListBox;
        private System.Windows.Forms.Button Button_MovePoint;
        private System.Windows.Forms.TextBox TextBox_PointPos;
    }
}

