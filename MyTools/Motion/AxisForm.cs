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
using ToolTotal;

namespace SingleAxisMotion
{
    public partial class AxisForm : Form
    {

        //public DrvIO pReelArriveIn;

        //private DrvMotor _motion.SingleMotor = null;
        private readonly string _xmlPath = ".\\Config.xml";
        //private bool _motion.IsMotorServoOn;
        //private readonly string[,] _motion.PointsArray = new string[4, 2];
        private readonly Motion _motion = null;
        public readonly Timer Timer = new Timer { Interval = 100 };
        private readonly IniFile _myIniFile = null;

        public AxisForm(Motion motion, IniFile myIniFile)
        {
            _motion = motion;
            _myIniFile = myIniFile;
            Timer.Tick += Timer_Tick;
            InitializeComponent();
        }

        private void AxisForm_Load(object sender, EventArgs e)
        {
            ComboBox_MotionSpace.SelectedIndex = 4;
            ComboBox_Point.SelectedIndex = 0;

            //pReelArriveIn = clsIO_Ports.IOList["Y1100"];
            //pReelArriveIn.SetIO(true);
            //ReadXml();
            for (int i = 0; i < _motion.PointsArray.GetLength(0); i++)
            {
                CheckedListBox.SetItemChecked(i, _motion.PointsArray[i, 0] == "1");
            }
            ComboBox_Point_SelectedIndexChanged(ComboBox_Point, EventArgs.Empty);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Label_CurPos.Text = _motion.SingleMotor.GetPosition().ToString();
            if (_motion.IsMotorServoOn) // _motion.SingleAxis.IsSerOn
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
            bool result = false;
            double dPos;
            if (Double.TryParse(Label_CurPos.Text, out dPos))
            {
                _motion.PointsArray[ComboBox_Point.SelectedIndex, 1] = Label_CurPos.Text;
                for (int i = 0; i < CheckedListBox.Items.Count; i++)
                {
                    _motion.PointsArray[i, 0] = CheckedListBox.GetItemCheckState(i) == CheckState.Checked ? "1" : "0";
                }
                
                _myIniFile.IniWriteValue("Axis", "PointF_Enable", _motion.PointsArray[0, 0]);
                _myIniFile.IniWriteValue("Axis", "PointF_Pos", _motion.PointsArray[0, 1]);
                _myIniFile.IniWriteValue("Axis", "PointS_Enable", _motion.PointsArray[1, 0]);
                _myIniFile.IniWriteValue("Axis", "PointS_Pos", _motion.PointsArray[1, 1]);
                _myIniFile.IniWriteValue("Axis", "PointT_Enable", _motion.PointsArray[2, 0]);
                _myIniFile.IniWriteValue("Axis", "PointT_Pos", _motion.PointsArray[2, 1]);

                TextBox_PointPos.Text = _motion.PointsArray[ComboBox_Point.SelectedIndex, 1];
                result = true;
            }
            MessageBox.Show(result ? "保存成功" : "保存失败");
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
                    list.Item(i).InnerText = _motion.PointsArray[i, 0];
                    list.Item(i).Attributes[0].InnerText = _motion.PointsArray[i, 1];
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
                _motion.PointsArray[i, 0] = list.Item(i)?.InnerText;
                _motion.PointsArray[i, 1] = list.Item(i)?.Attributes?[0].InnerText;
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
            if (!_motion.IsMotorServoOn) { return; }
            _motion.SingleMotor.RevMove(Convert.ToDouble(ComboBox_MotionSpace.SelectedItem), 25);
        }

        private void Button_LeftMove_Click(object sender, EventArgs e)
        {
            if (!_motion.IsMotorServoOn) { return; }
            _motion.SingleMotor.RevMove(-Convert.ToDouble(ComboBox_MotionSpace.SelectedItem), 25);
        }

        private void Button_Servo_Click(object sender, EventArgs e)
        {
            bool back;
            if (!_motion.IsMotorServoOn)
            {
                back = _motion.SingleMotor.SetSevON(true);
                if (back) { _motion.IsMotorServoOn = true; }
            }
            else
            {
                back = _motion.SingleMotor.SetSevON(false);
                if (back) { _motion.IsMotorServoOn = false; }
            }
        }

        private void Button_Reversion_Click(object sender, EventArgs e)
        {
            bool? back = _motion.SingleMotor?.DoHome();
            if (back == true) { _motion.IsMotorServoOn = true; }
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            _motion.SingleMotor?.Stop();
        }

        private void Button_MovePoint_Click(object sender, EventArgs e)
        {
            //if (!_motion.IsMotorServoOn) { return; }
            _motion.SingleMotor.AbsMove(Convert.ToDouble(_motion.PointsArray[ComboBox_Point.SelectedIndex, 1]), 25);
        }

        private void ComboBox_Point_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBox_PointPos.Text = _motion.PointsArray[Convert.ToInt32(ComboBox_Point.SelectedIndex), 1];
        }

        private void AxisForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Timer.Stop();
            Timer.Tick -= Timer_Tick;
        }
    }
}
