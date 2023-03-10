using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Cowain_Form.FormView;
using System.Windows.Forms;
using System.Xml;
using MotionBase;
using Cowain_AutoMachine.Flow.IO_Cylinder_Motor;
using Cowain_AutoMachine.Flow.IO_Cylinder;

namespace SingleAxisMotion
{
    public partial class AxisForm : Form
    {
        /// <summary>
        /// 料盘新放入信号
        /// </summary>
        //public DrvIO pReelArriveIn;

        private DrvMotor _pMotor = null;
        private readonly string _xmlPath = ".\\Config.xml";
        private bool _isMotorServoOn;
        private readonly string[,] _pointsArray = new string[4, 2];

        public AxisForm()
        {
            Motion motion = new Motion();
            frm_LoadingDlg loading = new frm_LoadingDlg(ref motion);
            loading.ShowDialog();
            InitializeComponent();
        }

        private void AxisForm_Load(object sender, EventArgs e)
        {
            ComboBox_MotionSpace.SelectedIndex = 4;
            ComboBox_Point.SelectedIndex = 0;
            _pMotor = clsMotors.MotorList["M01_00"];

            Timer timer = new Timer { Interval = 10, Enabled = true };
            timer.Tick += Timer_Tick;
            //pReelArriveIn = clsIO_Ports.IOList["Y1100"];
            //pReelArriveIn.SetIO(true);
            ReadXml();
            for (int i = 0; i < _pointsArray.GetLength(0); i++)
            {
                CheckedListBox.SetItemChecked(i, _pointsArray[i, 1] == "1");
            }
            ComboBox_Point_SelectedIndexChanged(ComboBox_Point, EventArgs.Empty);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Label_CurPos.Text = _pMotor.GetPosition().ToString();
            if (_isMotorServoOn)
            {
                Button_Servo.Text = "关使能";
                Button_Servo.BackColor = Color.Lime;
            }
            else
            {
                Button_Servo.Text = "开使能";
                Button_Servo.BackColor = DefaultBackColor;
            }
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            var s = CheckedListBox.GetItemChecked(0);
            var ss=CheckedListBox.GetItemCheckState(0) == CheckState.Checked;
            _pointsArray[ComboBox_Point.SelectedIndex, 0] = Label_CurPos.Text;
            for (int i = 0; i < CheckedListBox.Items.Count; i++)
            {
                _pointsArray[i, 1] = CheckedListBox.GetItemCheckState(i) == CheckState.Checked ? "1" : "0";
            }

            bool back = WriteXml();
            MessageBox.Show(back ? "保存成功" : "保存失败");
        }

        private bool WriteXml()
        {
            bool result;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlPath);
                XmlNodeList list = xmlDoc.SelectSingleNode("Axis").ChildNodes;
                for (int i = 0; i < list.Count; i++)
                {
                    list.Item(i).InnerText = _pointsArray[i, 0];
                    list.Item(i).Attributes[0].InnerText = _pointsArray[i, 1];
                }
                xmlDoc.Save(_xmlPath);
                result = true;
            }
            catch (Exception)
            { result = false; }
            return result;
        }

        private void ReadXml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_xmlPath);
            }
            catch (Exception e)
            {
                MessageBox.Show($"未找到{_xmlPath}，将重新创建");
                CreateXml();
                xmlDoc.Load(_xmlPath);
            }

            XmlNode element = xmlDoc.SelectSingleNode("Axis");
            XmlNodeList list = element.ChildNodes;
            for (int i = 0; i < list.Count; i++)
            {
                _pointsArray[i, 0] = list.Item(i)?.InnerText;
                _pointsArray[i, 1] = list.Item(i)?.Attributes?[0].InnerText;
            }
        }

        private void CreateXml()
        {

            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(declaration);
            XmlElement rootElement = xmlDoc.CreateElement("Axis");
            xmlDoc.AppendChild(rootElement);

            for (int i = 0; i < 4; i++)
            {
                XmlElement firstPoint = xmlDoc.CreateElement("Point_" + i);
                XmlAttribute attribute = xmlDoc.CreateAttribute("Enable");
                attribute.InnerText = "1";
                firstPoint.SetAttributeNode(attribute);
                firstPoint.InnerText = "45";
                rootElement.AppendChild(firstPoint);
            }

            xmlDoc.Save(_xmlPath);
        }

        private void Button_RightMove_Click(object sender, EventArgs e)
        {
            if (!_isMotorServoOn) { return; }
            _pMotor.RevMove(Convert.ToDouble(ComboBox_MotionSpace.SelectedItem), 25);
        }

        private void Button_LeftMove_Click(object sender, EventArgs e)
        {
            if (!_isMotorServoOn) { return; }
            _pMotor.RevMove(-Convert.ToDouble(ComboBox_MotionSpace.SelectedItem), 25);
        }

        private void Button_Servo_Click(object sender, EventArgs e)
        {
            bool back;
            if (!_isMotorServoOn)
            {
                back = _pMotor.SetSevON(true);
                if (back) { _isMotorServoOn = true; }
            }
            else
            {
                back = _pMotor.SetSevON(false);
                if (back) { _isMotorServoOn = false; }
            }
        }

        private void Button_Reversion_Click(object sender, EventArgs e)
        {
            bool? back = _pMotor?.DoHome();
            if (back == true) { _isMotorServoOn = true; }
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            _pMotor?.Stop();
        }

        private void Button_MovePoint_Click(object sender, EventArgs e)
        {
            //if (!_isMotorServoOn) { return; }
            _pMotor.AbsMove(Convert.ToDouble(_pointsArray[ComboBox_Point.SelectedIndex, 0]), 25);
        }

        private void ComboBox_Point_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBox_PointPos.Text = _pointsArray[Convert.ToInt32(ComboBox_Point.SelectedIndex), 0];
        }
    }
}
