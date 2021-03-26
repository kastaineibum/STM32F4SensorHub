using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace STMCounterFormServer
{
    public partial class Form2 : Form
    {
        ToolTip[] toolTip1 = new ToolTip[63];
        ToolTip[] toolTip2 = new ToolTip[20];

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            SerialPort_Load(sender, e);
            btnCheckCom_Click(sender, e);
            btnOpenCom_Click(sender, e);
            //AlexData.receiveBuff = new byte[32];
            //AlexData.receiveTick = 0;
            //AlexData.receiveLength = 0;
            AlexData.received = new List<byte[]>();

            AlexData.mid = 1;
            AlexData.brewflag = 0;
            AlexData.operateflag = 0;
            AlexData.normaltemp1 = ZAlexFunctions.getNormaltemp1();
            AlexData.normaltemp2 = ZAlexFunctions.getNormaltemp2();
            AlexData.currentcupcount = ZAlexFunctions.getCupcount();

            AlexData.Redis = new RedisClient("127.0.0.1", 6379);
            AlexData.Redis.AddItemToList(AlexData.mid + "-yog", "redis started.");
            string tempstr = AlexData.Redis.PopItemFromList(AlexData.mid + "-yog");
            //string tempstr = AlexData.Redis.GetItemFromList(AlexData.mid + "-yog",0);

            tbxRecvData.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            tbxSendData.LanguageOption = RichTextBoxLanguageOptions.UIFonts;

            tbxRecvData.AppendText(tempstr+"\r\n");
            tbxSendData.ScrollToCaret();

            timer1.Interval = 1000;

            for (int i = 0; i < 63; i++)
            {
                toolTip1[i] = new ToolTip();
            }

            for (int i = 0; i < 20; i++)
            {
                toolTip2[i] = new ToolTip();
            }

            for (int i = 0; i < 63; i++)
            {
                //toolTip1[i] = new ToolTip();
                toolTip1[i].AutoPopDelay = 5000;
                toolTip1[i].InitialDelay = 500;
                toolTip1[i].ReshowDelay = 500;
                // Force the ToolTip text to be displayed whether or not the form is active.
                toolTip1[i].ShowAlways = true;
            }

            for (int i = 0; i < 20; i++)
            {
                //toolTip2[i] = new ToolTip();
                toolTip2[i].AutoPopDelay = 5000;
                toolTip2[i].InitialDelay = 500;
                toolTip2[i].ReshowDelay = 500;
                // Force the ToolTip text to be displayed whether or not the form is active.
                toolTip2[i].ShowAlways = true;
            }

            toolTip1[1].SetToolTip(this.button1, " 清空当前数据显示");
            toolTip1[2].SetToolTip(this.button2, " 开始监听数据");
            toolTip1[3].SetToolTip(this.button3, " 停止监听数据");
            toolTip1[4].SetToolTip(this.button4, " 初始化IO");
            toolTip1[5].SetToolTip(this.button5, " 复位机械手");
            toolTip1[6].SetToolTip(this.button6, " 步进电机1使能");
            toolTip1[7].SetToolTip(this.button7, " 步进电机1禁用");
            toolTip1[8].SetToolTip(this.button8, " 步进电机2使能");
            toolTip1[9].SetToolTip(this.button9, " 步进电机2禁用");
            toolTip1[10].SetToolTip(this.button10, " 步进电机3使能");
            toolTip1[11].SetToolTip(this.button11, " 步进电机3禁用");
            toolTip1[12].SetToolTip(this.button12, " 打开电磁铁");
            toolTip1[13].SetToolTip(this.button13, " 关闭电磁铁");
            toolTip1[14].SetToolTip(this.button14, " 步进电机前进步数");
            toolTip1[15].SetToolTip(this.button15, " 步进电机后退步数");
            toolTip1[16].SetToolTip(this.button16, " 打开进料阀（缓动）");
            toolTip1[17].SetToolTip(this.button17, " 关闭进料阀（缓动）");
            toolTip1[18].SetToolTip(this.button18, " 打开进料振动电机");
            toolTip1[19].SetToolTip(this.button19, " 关闭进料震动电机");
            toolTip1[20].SetToolTip(this.button20, " 打开消毒剂泵");
            toolTip1[21].SetToolTip(this.button21, " 关闭消毒剂泵");
            toolTip1[22].SetToolTip(this.button22, " 打开清洁剂泵");
            toolTip1[23].SetToolTip(this.button23, " 关闭清洁剂泵");
            toolTip1[24].SetToolTip(this.button24, " 打开空气泵1");
            toolTip1[25].SetToolTip(this.button25, " 关闭空气泵1");
            toolTip1[26].SetToolTip(this.button26, " 打开空气泵2");
            toolTip1[27].SetToolTip(this.button27, " 关闭空气泵2");
            toolTip1[28].SetToolTip(this.button28, " 打开高压水泵1");
            toolTip1[29].SetToolTip(this.button29, " 关闭高压水泵1");
            toolTip1[30].SetToolTip(this.button30, " 打开高压水泵2");
            toolTip1[31].SetToolTip(this.button31, " 关闭高压水泵2");
            toolTip1[32].SetToolTip(this.button32, " 打开罐间泵");
            toolTip1[33].SetToolTip(this.button33, " 关闭罐间泵");
            toolTip1[34].SetToolTip(this.button34, " 打开罐1排出口");
            toolTip1[35].SetToolTip(this.button35, " 关闭罐1排出口");
            toolTip1[36].SetToolTip(this.button36, " 打开罐2排出口");
            toolTip1[37].SetToolTip(this.button37, " 关闭罐2排出口");
            toolTip1[38].SetToolTip(this.button38, " 打开加热器");
            toolTip1[39].SetToolTip(this.button39, " 关闭加热器");
            toolTip1[40].SetToolTip(this.button40, " 打开制冷剂阀1");
            toolTip1[41].SetToolTip(this.button41, " 关闭制冷剂阀1");
            toolTip1[42].SetToolTip(this.button42, " 打开制冷剂阀2");
            toolTip1[43].SetToolTip(this.button43, " 关闭制冷剂阀2");
            toolTip1[44].SetToolTip(this.button44, " 打开制冷压缩机");
            toolTip1[45].SetToolTip(this.button45, " 关闭制冷压缩机");
            toolTip1[46].SetToolTip(this.button46, " 打开门1");
            toolTip1[47].SetToolTip(this.button47, " 关闭门1");
            toolTip1[48].SetToolTip(this.button48, " 打开门2");
            toolTip1[49].SetToolTip(this.button49, " 关闭门2");
            toolTip1[50].SetToolTip(this.button50, " 打开螺旋桨1");
            toolTip1[51].SetToolTip(this.button51, " 关闭螺旋桨1");
            toolTip1[52].SetToolTip(this.button52, " 打开螺旋桨2");
            toolTip1[53].SetToolTip(this.button53, " 关闭螺旋桨2");
            toolTip1[54].SetToolTip(this.button54, " 开始发酵");
            toolTip1[55].SetToolTip(this.button55, " 停止发酵");
            toolTip1[56].SetToolTip(this.button56, " 开始清洁全部");
            toolTip1[57].SetToolTip(this.button57, " 获得当前传感器数值");
            toolTip1[58].SetToolTip(this.button58, " 停止清洁全部");
            toolTip1[59].SetToolTip(this.button59, " 开始制冷");
            toolTip1[60].SetToolTip(this.button60, " 停止制冷");
            toolTip1[61].SetToolTip(this.button61, " 开始恒温加热");
            toolTip1[62].SetToolTip(this.button62, " 停止恒温加热");

            toolTip2[0].SetToolTip(this.btnSend, " 测试发送命令");

            toolTip2[1].SetToolTip(this.textBox1, " 罐1温度");
            toolTip2[2].SetToolTip(this.textBox2, " 罐2温度");
            toolTip2[3].SetToolTip(this.textBox3, " 罐1液位");
            toolTip2[4].SetToolTip(this.textBox4, " 罐2液位");
            toolTip2[5].SetToolTip(this.textBox5, " 大桶流量计");
            toolTip2[6].SetToolTip(this.textBox6, " 酸奶流量计");
            toolTip2[7].SetToolTip(this.textBox7, " 罐间流量计");
            toolTip2[8].SetToolTip(this.textBox8, " 其他流量计");

            toolTip2[9].SetToolTip(this.btnCheckCom, " 检查串口设备变化");
            toolTip2[10].SetToolTip(this.btnOpenCom, " 打开串口");

            toolTip2[11].SetToolTip(this.label1, " 罐1温度");
            toolTip2[12].SetToolTip(this.label2, " 罐2温度");
            toolTip2[13].SetToolTip(this.label3, " 罐1液位");
            toolTip2[14].SetToolTip(this.label4, " 罐2液位");
            toolTip2[15].SetToolTip(this.label5, " 大桶流量计");
            toolTip2[16].SetToolTip(this.label6, " 酸奶流量计");
            toolTip2[17].SetToolTip(this.label7, " 罐间流量计");
            toolTip2[18].SetToolTip(this.label8, " 其他流量计");


        }



        private static SerialPort sp = new SerialPort();
        bool isOpen = false; //
        bool isSetProperty = false; //
        bool isHex = false; //


        private void SerialPort_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                cbxComPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxComPort.SelectedIndex = 0; 

            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("57600");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.Items.Add("230400");
            cbxBaudRate.SelectedIndex = 4;
            //stop bit
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;
            //data bit
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;
            //checksum bit
            cbxParity.Items.Add("None");
            cbxParity.Items.Add("Odd");
            cbxParity.Items.Add("Even");
            cbxParity.SelectedIndex = 0;

            rbnHex.Checked = true;
        }

        private void btnCheckCom_Click(object sender, EventArgs e) 
        {
            bool comExist = false;
            cbxComPort.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxComPort.Items.Add("COM" + (i + 1).ToString());
                    comExist = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (comExist)
            {
                cbxComPort.SelectedIndex = 0;
                try
                {
                    cbxComPort.SelectedIndex = 1;
                }
                catch (Exception)
                {
                    
                }
            }
            else
            {
                MessageBox.Show("no port discovered", " ");
            }

        }

        private bool CheckPortSetting() 
        {
            if (cbxComPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;
        }
        private void SetPortProperty()
        {
            sp = new SerialPort();
            sp.PortName = cbxComPort.Text.Trim(); 
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim()); 
            int f = (int)Convert.ToSingle(cbxStopBits.Text.Trim()) * 10;         
            switch (f)
            {
                case 0:
                    sp.StopBits = StopBits.None;
                    break;
                case 10:
                    sp.StopBits = StopBits.One;
                    break;
                case 15:
                    sp.StopBits = StopBits.OnePointFive;
                    break;
                case 20:
                    sp.StopBits = StopBits.Two;
                    break;
                default:
                    sp.StopBits = StopBits.None;
                    break;
            }
            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim()); 
            string parityType = cbxParity.Text.Trim();
            switch (parityType)
            {
                case "None":
                    sp.Parity = Parity.None;
                    break;
                case "Odd":
                    sp.Parity = Parity.Odd;
                    break;
                case "Even":
                    sp.Parity = Parity.Even;
                    break;
                default:
                    sp.Parity = Parity.None;
                    break;
            }
            sp.ReadTimeout = -1; 
            sp.RtsEnable = true; 

            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())
                {
                    MessageBox.Show("no port", " ");
                    return;
                }
                if (isSetProperty == false) 
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "Close Port";

                    cbxBaudRate.Enabled = false;
                    cbxComPort.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                }
                catch (Exception)
                {
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("cant open", " ");
                }
            }
            else
            {
                sp.Close();
                isOpen = false;
                isSetProperty = false;
                btnOpenCom.Text = "Open Port";

                cbxBaudRate.Enabled = true;
                cbxComPort.Enabled = true;
                cbxDataBits.Enabled = true;
                cbxParity.Enabled = true;
                cbxStopBits.Enabled = true;
            }

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] writeBytes;
                writeBytes = new byte[10];
                writeBytes[0] = 0x33;
                writeBytes[1] = 0x03;
                writeBytes[2] = 0x03;
                writeBytes[3] = 0x00;
                writeBytes[4] = 0x00;
                writeBytes[5] = 0x00;
                writeBytes[6] = 0xAA;
                writeBytes[7] = 0x55;
                writeBytes[8] = 0xAA;
                writeBytes[9] = 0x55;
                byte[] crcBytes;
                crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

                byte[] modbusBytes;
                modbusBytes = new byte[12];
                writeBytes.CopyTo(modbusBytes, 0);

                modbusBytes[10] = crcBytes[0];
                modbusBytes[11] = crcBytes[1];

                tbxSendData.AppendText(CRC.BytesToHexString(modbusBytes));
                tbxSendData.ScrollToCaret();
                if (tbxSendData.Text.Length > AlexData.maxDisplaychars) tbxSendData.Clear();

                sp.Write(modbusBytes, 0, modbusBytes.Length);

            }
            catch (Exception)
            {
                MessageBox.Show("sending failed", " ");
                return;

            }
        }
        private void sp_DataReceived(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(AlexData.receiveGap2); 
            this.Invoke((EventHandler)(
                delegate {
                    AlexData.newdatastamp = ZAlexFunctions.GetShort3mnStamp();
                    if (isHex == false)
                    {
                        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
                        Byte[] readBytes = new Byte[sp.BytesToRead];
                        sp.Read(readBytes, 0, readBytes.Length);
                        string decodedString = utf8.GetString(readBytes);
                        tbxRecvData.AppendText(decodedString);
                    }
                    else
                    {
                        if(sp.BytesToRead>0)
                        {
                            byte[] tb = new byte[sp.BytesToRead];
                            sp.Read(tb, 0, sp.BytesToRead);
                            AlexData.received.Add(tb);

                            Debug.WriteLine("AlexData.received.Count = " + AlexData.received.Count);

                            if (AlexData.received.Count > AlexData.maxReceived) AlexData.received.RemoveAt(0);

                            tbxRecvData.AppendText(CRC.BytesToHexString(tb)); 
                        }
                    }
                    tbxRecvData.ScrollToCaret();
                    if (tbxRecvData.Text.Length > AlexData.maxDisplaychars) tbxRecvData.Clear();
                }

                ));
        }

        private void rbnChar_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnChar.Checked)
            {
                isHex = false;
            }
            else
            {
                isHex = true;
            }
        }

        private void rbnHex_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnChar.Checked)
            {
                isHex = false;
            }
            else
            {
                isHex = true;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            tbxRecvData.Clear();
            tbxSendData.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);

            /*
            initios();
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 0, 77);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 1, 78);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 2, 79);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 3, 80);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 6, 30);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotorinit(1, 7, 31);
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotor1disable();
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotor1disable();
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            stepmotor1disable();
            System.Threading.Thread.Sleep(AlexData.receiveGap);
            */

            timer1.Start();

            AlexData.datareceiver = new Thread(new ThreadStart(dataReceiver));
            AlexData.datasender = new Thread(new ThreadStart(dataSender));
            

            AlexData.datareceiver.Start();
            AlexData.datasender.Start();

            button2.Enabled = false;
        }

        private static void dataReceiver()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(AlexData.receiveGap2);

                try
                {
                    if (AlexData.received.Count > 0)
                    {
                        byte[] tb = AlexData.received[0];
                        byte[] tbdata = new byte[tb.Length - 2];
                        for (int i = 0; i < tbdata.Length; i++)
                        {
                            tbdata[i] = tb[i];
                        }
                        byte[] crcBytes;
                        crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(tbdata));
                        if (tb.Length > 4 && tb[tb.Length - 2] == crcBytes[0] && tb[tb.Length - 1] == crcBytes[1])
                        {
                            AlexData.currentaddr = tb[0];
                            AlexData.currentfunc = tb[1];
                            AlexData.dataPrepared = new byte[tb.Length - 4];
                            for (int j = 0; j < tb.Length - 4; j++)
                            {
                                AlexData.dataPrepared[j] = tb[j + 2];
                            }
                            dataParser();
                        }

                        AlexData.received.RemoveAt(0);
                        Debug.WriteLine("AlexData.received.Count = "+ AlexData.received.Count);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }

        private static void dataParser()
        {
            if (AlexData.currentfunc == AlexData.COILREAD)
            {
                AlexData.dataTemp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp[i] = AlexData.dataPrepared[i];
                }
                AlexData.pinnum = BitConverter.ToInt32(AlexData.dataTemp, 0);
                AlexData.dataTemp2 = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp2[i] = AlexData.dataPrepared[i + 4];
                }
                AlexData.pinstate = BitConverter.ToInt32(AlexData.dataTemp2, 0);

                Debug.WriteLine("read modbus address = " + AlexData.currentaddr);
                Debug.WriteLine("read pinnum = " + AlexData.pinnum);
                Debug.WriteLine("read pinstate = " + AlexData.pinstate);
            }
            else if (AlexData.currentfunc == AlexData.REGREAD)
            {
                AlexData.dataTemp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp[i] = AlexData.dataPrepared[i];
                }
                AlexData.regaddr = BitConverter.ToInt32(AlexData.dataTemp, 0);

                //Debug.WriteLine("modbus address = " + AlexData.currentaddr);

                if (AlexData.currentaddr == 0x32)
                {
                    AlexData.dataTemp2 = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        AlexData.dataTemp2[i] = AlexData.dataPrepared[i + 4];
                    }
                    AlexData.regval = BitConverter.ToInt32(AlexData.dataTemp2, 0);
                    AlexData.regvalf = BitConverter.ToSingle(AlexData.dataTemp2, 0);

                    if (AlexData.regaddr == AlexData.RBARO)
                    {
                        Debug.WriteLine("baro0 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RHUMIDITY)
                    {
                        Debug.WriteLine("humidity0 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RTEMPERATURE)
                    {
                        Debug.WriteLine("temperature0 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RELEVATION)
                    {
                        Debug.WriteLine("elevation0 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM1)
                    {
                        Debug.WriteLine("0-pwm1 = " + AlexData.regval);
                        AlexData.pwm1 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RPWM2)
                    {
                        Debug.WriteLine("0-pwm2 = " + AlexData.regval);
                        AlexData.pwm2 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RPWM3)
                    {
                        Debug.WriteLine("0-pwm3 = " + AlexData.regval);
                        AlexData.pwm3 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RPWM4)
                    {
                        Debug.WriteLine("0-pwm4 = " + AlexData.regval);
                        AlexData.pwm4 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RPITCH)
                    {
                        Debug.WriteLine("pitch0 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RROLL)
                    {
                        Debug.WriteLine("roll0 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RYAW)
                    {
                        Debug.WriteLine("yaw0 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RDISTANCE)
                    {
                        Debug.WriteLine("distance0 = " + AlexData.regval);
                    }
                }
                else if (AlexData.currentaddr == 0x33)
                {
                    AlexData.dataTemp2 = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        AlexData.dataTemp2[i] = AlexData.dataPrepared[i+4];
                    }
                    AlexData.regval = BitConverter.ToInt32(AlexData.dataTemp2, 0);
                    AlexData.regvalf = BitConverter.ToSingle(AlexData.dataTemp2, 0);

                    if (AlexData.regaddr == AlexData.RBARO)
                    {
                        Debug.WriteLine("baro1 = "+ AlexData.regval);
                        ZAlexFunctions.setBaro1(AlexData.regval);
                        AlexData.pressure1 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RHUMIDITY)
                    {
                        Debug.WriteLine("humidity1 = " + AlexData.regval);
                        ZAlexFunctions.setHumidity1(AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RTEMPERATURE)
                    {
                        Debug.WriteLine("temperature1 = " + AlexData.regval);
                        ZAlexFunctions.setTemperature1(AlexData.regval);
                        AlexData.temperature1 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RELEVATION)
                    {
                        Debug.WriteLine("elevation1 = " + AlexData.regval);
                        ZAlexFunctions.setElevation1(AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM1)
                    {
                        Debug.WriteLine("1-pwm1 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM2)
                    {
                        Debug.WriteLine("1-pwm2 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM3)
                    {
                        Debug.WriteLine("1-pwm3 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM4)
                    {
                        Debug.WriteLine("1-pwm4 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPITCH)
                    {
                        Debug.WriteLine("pitch1 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RROLL)
                    {
                        Debug.WriteLine("roll1 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RYAW)
                    {
                        Debug.WriteLine("yaw1 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RDISTANCE)
                    {
                        Debug.WriteLine("distance1 = " + AlexData.regval);
                        ZAlexFunctions.setLiquidlevel1(AlexData.barrelheight - AlexData.regval);
                        AlexData.liquidlevel1 = AlexData.barrelheight - AlexData.regval;
                    }

                }
                else if (AlexData.currentaddr == 0x34)
                {
                    AlexData.dataTemp2 = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        AlexData.dataTemp2[i] = AlexData.dataPrepared[i + 4];
                    }
                    AlexData.regval = BitConverter.ToInt32(AlexData.dataTemp2, 0);
                    AlexData.regvalf = BitConverter.ToSingle(AlexData.dataTemp2, 0);

                    if (AlexData.regaddr == AlexData.RBARO)
                    {
                        Debug.WriteLine("baro2 = " + AlexData.regval);
                        ZAlexFunctions.setBaro2(AlexData.regval);
                        AlexData.pressure2 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RHUMIDITY)
                    {
                        Debug.WriteLine("humidity2 = " + AlexData.regval);
                        ZAlexFunctions.setHumidity2(AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RTEMPERATURE)
                    {
                        Debug.WriteLine("temperature2 = " + AlexData.regval);
                        ZAlexFunctions.setTemperature2(AlexData.regval);
                        AlexData.temperature2 = AlexData.regval;
                    }
                    else if (AlexData.regaddr == AlexData.RELEVATION)
                    {
                        Debug.WriteLine("elevation2 = " + AlexData.regval);
                        ZAlexFunctions.setElevation2(AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM1)
                    {
                        Debug.WriteLine("2-pwm1 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM2)
                    {
                        Debug.WriteLine("2-pwm2 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM3)
                    {
                        Debug.WriteLine("2-pwm3 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPWM4)
                    {
                        Debug.WriteLine("2-pwm4 = " + AlexData.regval);
                    }
                    else if (AlexData.regaddr == AlexData.RPITCH)
                    {
                        Debug.WriteLine("pitch2 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RROLL)
                    {
                        Debug.WriteLine("roll2 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RYAW)
                    {
                        Debug.WriteLine("yaw2 = " + AlexData.regvalf);
                    }
                    else if (AlexData.regaddr == AlexData.RDISTANCE)
                    {
                        Debug.WriteLine("distance2 = " + AlexData.regval);
                        ZAlexFunctions.setLiquidlevel2(AlexData.barrelheight2 - AlexData.regval);
                        AlexData.liquidlevel2 = AlexData.barrelheight2 - AlexData.regval;
                    }
                }
            }
            else if (AlexData.currentfunc == AlexData.COILWRITE)
            {
                AlexData.dataTemp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp[i] = AlexData.dataPrepared[i];
                }
                AlexData.pinnum = BitConverter.ToInt32(AlexData.dataTemp, 0);
                AlexData.dataTemp2 = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp2[i] = AlexData.dataPrepared[i + 4];
                }
                AlexData.pinstate = BitConverter.ToInt32(AlexData.dataTemp2, 0);

                Debug.WriteLine("write modbus address = " + AlexData.currentaddr);
                Debug.WriteLine("write pinnum = " + AlexData.pinnum);
                Debug.WriteLine("write pinstate = " + AlexData.pinstate);
            }
            else if (AlexData.currentfunc == AlexData.REGWRITE)
            {
                AlexData.dataTemp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp[i] = AlexData.dataPrepared[i];
                }
                AlexData.regaddr = BitConverter.ToInt32(AlexData.dataTemp, 0);
                AlexData.dataTemp2 = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    AlexData.dataTemp2[i] = AlexData.dataPrepared[i + 4];
                }
                AlexData.regval = BitConverter.ToInt32(AlexData.dataTemp2, 0);

                Debug.WriteLine("write modbus address = " + AlexData.currentaddr);
                Debug.WriteLine("write register address = " + AlexData.regaddr);
                Debug.WriteLine("write register value = " + AlexData.regval);
            }
        }

        private static void dataSender()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(AlexData.receiveGap);

                try
                {
                    AlexData.tempstr = AlexData.Redis.PopItemFromList(AlexData.mid + "-yog");
                    if (AlexData.tempstr!=null && AlexData.tempstr.Length > 2)
                    {
                        redisParser();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Object reference") || ex.Message.Contains("NullReferenceException"))
                    {

                    }
                    else
                    {
                        //Debug.WriteLine(ex.Message);
                    }     
                }

            }
        }

        private static void initios()
        {
            byte[] outpins = {77,78,79,80,81,82,84,85,86,87,88,90,91,92,93,95,96,98,1,2,3,4,5,7,15,16,17,18,24,29};
            byte[] inpins = { 30,31 };

            for (int i = 0; i < outpins.Length; i++)
            {
                byte[] writeBytes;
                writeBytes = new byte[10];
                writeBytes[0] = 0x32;
                writeBytes[1] = (byte)AlexData.COILWRITE;
                writeBytes[2] = outpins[i]; 
                writeBytes[3] = 0x00;
                writeBytes[4] = 0x00;
                writeBytes[5] = 0x00;
                writeBytes[6] = 0x02; //2
                writeBytes[7] = 0x00;
                writeBytes[8] = 0x00;
                writeBytes[9] = 0x00;
                byte[] crcBytes;
                crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

                byte[] modbusBytes;
                modbusBytes = new byte[12];
                writeBytes.CopyTo(modbusBytes, 0);

                modbusBytes[10] = crcBytes[0];
                modbusBytes[11] = crcBytes[1];

                sp.Write(modbusBytes, 0, modbusBytes.Length);

                System.Threading.Thread.Sleep(AlexData.receiveGap);
                System.Threading.Thread.Sleep(AlexData.receiveGap);

                byte[] writeBytes2;
                writeBytes2 = new byte[10];
                writeBytes2[0] = 0x32;
                writeBytes2[1] = (byte)AlexData.COILWRITE;
                writeBytes2[2] = outpins[i];
                writeBytes2[3] = 0x00;
                writeBytes2[4] = 0x00;
                writeBytes2[5] = 0x00;
                writeBytes2[6] = 0x00; //0
                writeBytes2[7] = 0x00;
                writeBytes2[8] = 0x00;
                writeBytes2[9] = 0x00;
                byte[] crcBytes2;
                crcBytes2 = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes2));

                byte[] modbusBytes2;
                modbusBytes2 = new byte[12];
                writeBytes2.CopyTo(modbusBytes2, 0);

                modbusBytes2[10] = crcBytes2[0];
                modbusBytes2[11] = crcBytes2[1];

                sp.Write(modbusBytes2, 0, modbusBytes2.Length);

                System.Threading.Thread.Sleep(AlexData.receiveGap);
                System.Threading.Thread.Sleep(AlexData.receiveGap);
            }

            for (int i = 0; i < inpins.Length; i++)
            {
                byte[] writeBytes2;
                writeBytes2 = new byte[10];
                writeBytes2[0] = 0x32;
                writeBytes2[1] = (byte)AlexData.COILWRITE;
                writeBytes2[2] = outpins[i];
                writeBytes2[3] = 0x00;
                writeBytes2[4] = 0x00;
                writeBytes2[5] = 0x00;
                writeBytes2[6] = 0x00; //0
                writeBytes2[7] = 0x00;
                writeBytes2[8] = 0x00;
                writeBytes2[9] = 0x00;
                byte[] crcBytes2;
                crcBytes2 = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes2));

                byte[] modbusBytes2;
                modbusBytes2 = new byte[12];
                writeBytes2.CopyTo(modbusBytes2, 0);

                modbusBytes2[10] = crcBytes2[0];
                modbusBytes2[11] = crcBytes2[1];

                sp.Write(modbusBytes2, 0, modbusBytes2.Length);

                System.Threading.Thread.Sleep(AlexData.receiveGap);
                System.Threading.Thread.Sleep(AlexData.receiveGap);

                byte[] writeBytes;
                writeBytes = new byte[10];
                writeBytes[0] = 0x32;
                writeBytes[1] = (byte)AlexData.COILWRITE;
                writeBytes[2] = outpins[i];
                writeBytes[3] = 0x00;
                writeBytes[4] = 0x00;
                writeBytes[5] = 0x00;
                writeBytes[6] = 0x04; //4
                writeBytes[7] = 0x00;
                writeBytes[8] = 0x00;
                writeBytes[9] = 0x00;
                byte[] crcBytes;
                crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

                byte[] modbusBytes;
                modbusBytes = new byte[12];
                writeBytes.CopyTo(modbusBytes, 0);

                modbusBytes[10] = crcBytes[0];
                modbusBytes[11] = crcBytes[1];

                sp.Write(modbusBytes, 0, modbusBytes.Length);

                System.Threading.Thread.Sleep(AlexData.receiveGap);
                System.Threading.Thread.Sleep(AlexData.receiveGap);
            }

        }

        private static void opendoor1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 0x10; //16
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closedoor1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 0x10; //16
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void opendoor2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 17; //17
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closedoor2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 17; //17
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startmaterialsmotor()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 88; //88
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopmaterialsmotor()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 88; //88
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openmaterialsswitchstart()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 87; //87
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openmaterialsswitchend()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 87; //87
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closematerialsswitchstart()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 86; //86
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closematerialsswitchend()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 86; //86
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getpwm1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RPWM1;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getpwm2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RPWM2;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getpwm3()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RPWM3;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getpwm4()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RPWM4;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void gettemperature1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x33;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RTEMPERATURE;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void gettemperature2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x34;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RTEMPERATURE;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getliquidlevel1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x33;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RDISTANCE;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void getliquidlevel2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x34;
            writeBytes[1] = (byte)AlexData.REGREAD;
            writeBytes[2] = (byte)AlexData.RDISTANCE;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0xAA;
            writeBytes[7] = 0x55;
            writeBytes[8] = 0xAA;
            writeBytes[9] = 0x55;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openrefrigvalve1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 7; //7
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closerefrigvalve1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 7; //7
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openrefrigvalve2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 15; //15
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closerefrigvalve2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 15; //15
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startcompressor()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 5; //5
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopcompressor()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 5; //5
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startimpeller1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 18; //18
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopimpeller1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 18; //18
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startimpeller2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 24; //24
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopimpeller2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 24; //24
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closebarrelvalve()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 96; //96
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openbarrelvalve()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 96; //96
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startheater()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 4; //4
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopheater()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 4; //4
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startairpump1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 92; //92
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopairpump1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 92; //92
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void startairpump2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 93; //93
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stopairpump2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 93; //93
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void opendrainout()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 3; //3
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closedrainout()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 3; //3
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotorinit(int motorid,int funcno,int pinnum)
        {
            byte[] writeBytes;
            writeBytes = new byte[18];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGWRITE;
            writeBytes[2] = (byte)AlexData.WMOTORINIT;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = (byte)motorid; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            writeBytes[10] = (byte)funcno; //0
            writeBytes[11] = 0x00;
            writeBytes[12] = 0x00;
            writeBytes[13] = 0x00;
            writeBytes[14] = (byte)pinnum; //0
            writeBytes[15] = 0x00;
            writeBytes[16] = 0x00;
            writeBytes[17] = 0x00;

            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[20];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[18] = crcBytes[0];
            modbusBytes[19] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotorfwd(int motorid, int step)
        {
            byte[] writeBytes;
            writeBytes = new byte[14];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGWRITE;
            writeBytes[2] = (byte)AlexData.WMOTORFWD;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = (byte)motorid; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            writeBytes[10] = (byte)step; //0
            writeBytes[11] = 0x00;
            writeBytes[12] = 0x00;
            writeBytes[13] = 0x00;

            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[16];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[14] = crcBytes[0];
            modbusBytes[15] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotorrvs(int motorid, int step)
        {
            byte[] writeBytes;
            writeBytes = new byte[14];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.REGWRITE;
            writeBytes[2] = (byte)AlexData.WMOTORRVS;
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = (byte)motorid; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            writeBytes[10] = (byte)step; //0
            writeBytes[11] = 0x00;
            writeBytes[12] = 0x00;
            writeBytes[13] = 0x00;

            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[16];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[14] = crcBytes[0];
            modbusBytes[15] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor1enable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 81; //81
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor2enable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 82; //82
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor3enable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 84; //84
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor1disable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 81; //81
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor2disable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 82; //82
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void stepmotor3disable()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 84; //84
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void opendrain1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 98; //98
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closedrain1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 98; //98
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void opendisinfector()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 90; //90
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closedisinfector()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 90; //90
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void opencleanser()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 91; //91
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closecleanser()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 91; //91
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openhppump1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 95; //95
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }
        private static void closehppump1()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 95; //95
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openhppump2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 2; //2
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }
        private static void closehppump2()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 2; //2
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void openmag()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 85; //85
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x01; //1
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void closemag()
        {
            byte[] writeBytes;
            writeBytes = new byte[10];
            writeBytes[0] = 0x32;
            writeBytes[1] = (byte)AlexData.COILWRITE;
            writeBytes[2] = 85; //85
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x00;
            writeBytes[5] = 0x00;
            writeBytes[6] = 0x00; //0
            writeBytes[7] = 0x00;
            writeBytes[8] = 0x00;
            writeBytes[9] = 0x00;
            byte[] crcBytes;
            crcBytes = CRC.StringToHexByte(CRC.ToModbusCRC16(writeBytes));

            byte[] modbusBytes;
            modbusBytes = new byte[12];
            writeBytes.CopyTo(modbusBytes, 0);

            modbusBytes[10] = crcBytes[0];
            modbusBytes[11] = crcBytes[1];

            sp.Write(modbusBytes, 0, modbusBytes.Length);
        }

        private static void material2tank()
        {
            AlexData.td_material2tank = new Thread(new ThreadStart(td_material2tank));
            AlexData.td_material2tank.Start();

            ZAlexFunctions.setPowderwater(AlexData.powder,AlexData.water);
            AlexData.bid = ZAlexFunctions.getLastbid();
        }

        private static void td_material2tank()
        {
            AlexData.sp_material2tank = 0;
            try
            {
                AlexData.operateflag = 1;
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap);


                    if (AlexData.sp_material2tank == 0)
                    {
                        closedoor1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedoor1();
                        AlexData.sp_material2tank++;
                    }
                    else if (AlexData.sp_material2tank == 1)
                    {
                        openmaterialsswitchstart();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openmaterialsswitchstart();
                        System.Threading.Thread.Sleep(AlexData.longGap*4);
                        openmaterialsswitchend();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openmaterialsswitchend();
                        AlexData.sp_material2tank++;
                    }
                    else if (AlexData.sp_material2tank == 2)
                    {
                        startmaterialsmotor(); //solid motor and water pump valve etc.
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startmaterialsmotor();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getpwm1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getpwm1();

                        AlexData.pwm1laststart = AlexData.pwm1;
                        AlexData.sp_material2tank++;
                    }
                    else if (AlexData.sp_material2tank == 3)
                    {
                        if (AlexData.pwm1 >= AlexData.pwm1laststart + (AlexData.water * AlexData.pwm1factor))
                        {
                            stopmaterialsmotor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopmaterialsmotor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            AlexData.sp_material2tank++;
                        }
                        else
                        {
                            getpwm1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            getpwm1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            getliquidlevel1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            getliquidlevel1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                    }
                    else if (AlexData.sp_material2tank == 4)
                    {
                        closematerialsswitchstart();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closematerialsswitchstart();
                        System.Threading.Thread.Sleep(AlexData.longGap * 4);
                        closematerialsswitchend();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closematerialsswitchend();

                        AlexData.sp_material2tank++;
                    }
                    else if (AlexData.sp_material2tank == 5)
                    {
                        break;
                    }


                }
                AlexData.operateflag = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void startbrewing()
        {
            AlexData.td_startbrewing = new Thread(new ThreadStart(td_startbrewing));
            AlexData.td_startbrewing.Start();

            ZAlexFunctions.setTpd(AlexData.bid, AlexData.brewtemperature, AlexData.brewpressure, AlexData.brewduration);
        }

        private static void td_startbrewing()
        {
            AlexData.sp_startbrewing = 0;
            try
            {
                AlexData.brewflag = 1;
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap*7);

                    if (AlexData.sp_startbrewing == 0)
                    {
                        AlexData.operateflag = 1;
                        closedoor1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedoor1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        //startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        //startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startheater();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startheater();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        ZAlexFunctions.setBrewstartstamp(DateTime.Now.ToString());
                        ZAlexFunctions.setBrewendstamp(DateTime.Now.AddMinutes(AlexData.brewduration).ToString());

                        AlexData.sp_startbrewing++;
                    }
                    else if (AlexData.sp_startbrewing == 1)
                    {
                        AlexData.operateflag = 1;

                        gettemperature1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        gettemperature1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        if (AlexData.temperature1 >= AlexData.brewtemperature)
                        {
                            stopheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                        if (AlexData.temperature1 < AlexData.brewtemperature - AlexData.temperatureRange)
                        {
                            startheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                        AlexData.operateflag = 0;

                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getBrewendstamp())>0)
                        {
                            AlexData.sp_startbrewing++;
                        }

                    }
                    else if (AlexData.sp_startbrewing == 2)
                    {
                        AlexData.operateflag = 1;

                        openbarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openbarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        AlexData.liquidlevelmincount = 0;

                        AlexData.sp_startbrewing++;
                    }
                    else if (AlexData.sp_startbrewing == 3)
                    {
                        AlexData.operateflag = 1;

                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        if (AlexData.liquidlevel2 >= AlexData.liquidlevelmax || AlexData.liquidlevelmincount >= 10)
                        {
                            closebarrelvalve();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closebarrelvalve();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopairpump1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopairpump1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);

                            AlexData.sp_startbrewing++;
                        }

                        AlexData.operateflag = 0;

                        if (AlexData.liquidlevel1<=100) 
                        {
                            AlexData.liquidlevelmincount++;
                        }

                    }
                    else if (AlexData.sp_startbrewing == 4)
                    {
                        AlexData.sp_startbrewing++;
                    }
                    else if (AlexData.sp_startbrewing == 5)
                    {
                        break;
                    }


                }
                AlexData.brewflag = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void td_normaltemp()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap*5);

                    if (AlexData.brewflag == 0 && AlexData.operateflag == 0)
                    {
                        gettemperature1();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        gettemperature2();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.longGap);

                        if (AlexData.temperature1 <= AlexData.normaltemp1)
                        {
                            //stop refrigerating 1
                            AlexData.refrigvalve1 = 0;

                            if (AlexData.refrigvalve2 == 0)
                            {
                                stopcompressor();
                                System.Threading.Thread.Sleep(AlexData.longGap);
                                stopcompressor();
                                System.Threading.Thread.Sleep(AlexData.receiveGap);
                            }

                            closerefrigvalve1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closerefrigvalve1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                        if (AlexData.temperature1 >= AlexData.normaltemp1 + AlexData.temperatureRange)
                        {
                            //start refrigerating 1
                            AlexData.refrigvalve1 = 1;

                            openrefrigvalve1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            openrefrigvalve1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startcompressor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startcompressor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            //startimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            //startimpeller1();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);

                        }
                    }

                    if (AlexData.operateflag == 0)
                    {
                        if (AlexData.temperature2 <= AlexData.normaltemp2)
                        {
                            //stop refrigerating 2
                            AlexData.refrigvalve2 = 0;

                            if (AlexData.refrigvalve1 == 0)
                            {
                                stopcompressor();
                                System.Threading.Thread.Sleep(AlexData.longGap);
                                stopcompressor();
                                System.Threading.Thread.Sleep(AlexData.receiveGap);
                            }

                            closerefrigvalve2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closerefrigvalve2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                        if (AlexData.temperature2 >= AlexData.normaltemp2 + AlexData.temperatureRange)
                        {
                            //start refrigerating 2
                            AlexData.refrigvalve2 = 1;

                            openrefrigvalve2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            openrefrigvalve2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startcompressor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startcompressor();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            //startimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            //startimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void td_heattemp()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap * 5);

                    if (AlexData.endtime.CompareTo(DateTime.Now)>0
                        )
                    {
                        gettemperature1();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        gettemperature2();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        gettemperature1();
                        System.Threading.Thread.Sleep(AlexData.longGap);

                        if (AlexData.temperature1 >= AlexData.heattemp)
                        {


                            stopheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }

                        if (AlexData.temperature1 <= AlexData.heattemp - AlexData.temperatureRange)
                        {
                            startheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            startheater();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                AlexData.heatover = 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
        }

        private static void outputyog()
        {
            AlexData.td_outputyog = new Thread(new ThreadStart(td_outputyog));
            AlexData.td_outputyog.Start();

            AlexData.currentcupcount--;
            ZAlexFunctions.setCupcountdec(1);
        }

        private static void td_outputyog()
        {
            AlexData.sp_outputyog = 0;
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap);

                    if (AlexData.sp_outputyog == 0)
                    {
                        AlexData.operateflag = 1;
                        getpwm2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getpwm2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;
                        AlexData.sp_outputyog++;
                    }
                    else if (AlexData.sp_outputyog == 1)
                    {
                        AlexData.pwm2laststart = AlexData.pwm2;
                        AlexData.operateflag = 1;
                        startairpump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        //startimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        //startimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendrainout();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendrainout();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        AlexData.operateflag = 0;
                        AlexData.sp_outputyog++;
                    }
                    else if (AlexData.sp_outputyog == 2)
                    {
                        AlexData.operateflag = 1;
                        getpwm2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getpwm2();
                        System.Threading.Thread.Sleep(AlexData.longGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        if (AlexData.pwm2 >= AlexData.pwm2laststart + AlexData.pwm2onecup)
                        {
                            closedrainout();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closedrainout();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopairpump2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopairpump2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            stopimpeller2();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            AlexData.sp_outputyog++;
                        }

                        AlexData.operateflag = 0;

                    }
                    else if (AlexData.sp_outputyog == 3)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void resetmanipulator()
        {
            AlexData.td_resetmanipulator = new Thread(new ThreadStart(td_resetmanipulator));
            AlexData.td_resetmanipulator.Start();

        }

        private static void td_resetmanipulator()
        {
            AlexData.sp_resetmanipulator = 0;
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap);

                    if (AlexData.sp_resetmanipulator == 0)
                    {
                        AlexData.operateflag = 1;

                        stepmotor1enable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotorfwd(1, 1000);
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotor1disable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        stepmotor2enable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotorfwd(1, 1000);
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotor2disable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        stepmotor3enable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotorfwd(1, 1000);
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stepmotor3disable();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        openmag();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openmag();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closemag();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closemag();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;
                        AlexData.sp_resetmanipulator++;
                    }
                    else if (AlexData.sp_resetmanipulator == 1)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void startclean1()
        {
            AlexData.td_startclean1 = new Thread(new ThreadStart(td_startclean1));
            AlexData.td_startclean1.Start();

        }

        private static void td_startclean1()
        {
            AlexData.sp_startclean1 = 0;
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap);

                    if (AlexData.sp_startclean1 == 0)
                    {
                        ZAlexFunctions.setClean();
                        AlexData.cid = ZAlexFunctions.getLastcid();
                        ZAlexFunctions.setCleanstartstamp(DateTime.Now.ToString());
                        ZAlexFunctions.setCleanendstamp(DateTime.Now.AddMinutes(AlexData.cleanduration).ToString());

                        AlexData.operateflag = 1;

                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opencleanser();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opencleanser();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendisinfector();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendisinfector();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        AlexData.sp_startclean1++;
                    }
                    else if (AlexData.sp_startclean1 == 1)
                    {
                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanstartstamp().AddSeconds(5)) > 0)
                        {
                            AlexData.operateflag = 1;

                            closecleanser();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closecleanser();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closedisinfector();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closedisinfector();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);

                            AlexData.operateflag = 0;

                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 2)
                    {
                        AlexData.operateflag = 1;

                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanendstamp()) > 0)
                        {
                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 3)
                    {
                        AlexData.operateflag = 1;

                        closehppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closehppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanendstamp().AddSeconds(20)) > 0)
                        {
                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 4)
                    {
                        AlexData.operateflag = 1;

                        stopairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        AlexData.sp_startclean1++;
                    }
                    else if (AlexData.sp_startclean1 == 5)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void startclean2()
        {
            AlexData.td_startclean2 = new Thread(new ThreadStart(td_startclean2));
            AlexData.td_startclean2.Start();

        }

        private static void td_startclean2()
        {
            AlexData.sp_startclean2 = 0;
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AlexData.longGap);

                    if (AlexData.sp_startclean2 == 0)
                    {
                        ZAlexFunctions.setClean();
                        AlexData.cid = ZAlexFunctions.getLastcid();
                        ZAlexFunctions.setCleanstartstamp(DateTime.Now.ToString());
                        ZAlexFunctions.setCleanendstamp(DateTime.Now.AddMinutes(AlexData.cleanduration).ToString());

                        AlexData.operateflag = 1;

                        openbarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openbarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        startimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opencleanser();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opencleanser();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendisinfector();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        opendisinfector();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closedrain1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        openhppump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        AlexData.sp_startclean1++;
                    }
                    else if (AlexData.sp_startclean1 == 1)
                    {
                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanstartstamp().AddSeconds(5)) > 0)
                        {
                            AlexData.operateflag = 1;

                            closecleanser();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closecleanser();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closedisinfector();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);
                            closedisinfector();
                            System.Threading.Thread.Sleep(AlexData.receiveGap);

                            AlexData.operateflag = 0;

                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 2)
                    {
                        AlexData.operateflag = 1;

                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanendstamp()) > 0)
                        {
                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 3)
                    {
                        AlexData.operateflag = 1;

                        closehppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closehppump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closehppump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closehppump2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        getliquidlevel2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        if (DateTime.Compare(DateTime.Now, ZAlexFunctions.getCleanendstamp().AddSeconds(20)) > 0)
                        {
                            AlexData.sp_startclean1++;
                        }
                    }
                    else if (AlexData.sp_startclean1 == 4)
                    {
                        AlexData.operateflag = 1;

                        stopairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopairpump1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller1();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        stopimpeller2();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);
                        closebarrelvalve();
                        System.Threading.Thread.Sleep(AlexData.receiveGap);

                        AlexData.operateflag = 0;

                        AlexData.sp_startclean1++;
                    }
                    else if (AlexData.sp_startclean1 == 5)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void redisParser()
        {
            string[] commandargs;
            commandargs = AlexData.tempstr.Split('|');
            if (commandargs.Length >= 1)
            {
                if (commandargs[0].CompareTo("opendoor1") == 0)
                {
                    Debug.WriteLine("opendoor1");
                    AlexData.operateflag = 1;
                    opendoor1();
                    System.Threading.Thread.Sleep(AlexData.receiveGap);
                    opendoor1();
                    AlexData.operateflag = 0;
                }
                else if (commandargs[0].CompareTo("closedoor1") == 0)
                {
                    Debug.WriteLine("closedoor1");
                    AlexData.operateflag = 1;
                    closedoor1();
                    System.Threading.Thread.Sleep(AlexData.receiveGap);
                    closedoor1();
                    AlexData.operateflag = 0;
                }
                else if (commandargs[0].CompareTo("material2tank") == 0)
                {
                    Debug.WriteLine("material2tank");
                    int powder = Convert.ToInt32(commandargs[1]);
                    int water = Convert.ToInt32(commandargs[2]);
                    AlexData.powder = powder;
                    AlexData.water = water;
                    material2tank();
                }
                else if (commandargs[0].CompareTo("startbrewing") == 0)
                {
                    Debug.WriteLine("startbrewing");
                    int temperature = Convert.ToInt32(commandargs[1]);
                    int pressure = Convert.ToInt32(commandargs[2]);
                    int duration = Convert.ToInt32(commandargs[3]);
                    AlexData.brewtemperature = temperature;
                    AlexData.brewpressure = pressure;
                    AlexData.brewduration = duration;
                    startbrewing();
                }
                else if (commandargs[0].CompareTo("stopbrewing") == 0)
                {
                    Debug.WriteLine("stopbrewing");
                    AlexData.brewflag = 0;
                    AlexData.td_startbrewing.Abort();
                }
                else if (commandargs[0].CompareTo("setnormaltemperature") == 0)
                {
                    Debug.WriteLine("setnormaltemperature");
                    int temp1 = Convert.ToInt32(commandargs[1]);
                    int temp2 = Convert.ToInt32(commandargs[2]);
                    AlexData.normaltemp1 = temp1;
                    AlexData.normaltemp2 = temp2;
                    ZAlexFunctions.setNormaltemp(temp1, temp2);
                }
                else if (commandargs[0].CompareTo("opendoor2") == 0)
                {
                    Debug.WriteLine("opendoor2");
                    AlexData.operateflag = 1;
                    opendoor2();
                    System.Threading.Thread.Sleep(AlexData.receiveGap);
                    opendoor2();
                    AlexData.operateflag = 0;
                }
                else if (commandargs[0].CompareTo("closedoor2") == 0)
                {
                    Debug.WriteLine("closedoor2");
                    AlexData.operateflag = 1;
                    closedoor2();
                    System.Threading.Thread.Sleep(AlexData.receiveGap);
                    closedoor2();
                    AlexData.operateflag = 0;
                }
                else if (commandargs[0].CompareTo("addcup") == 0)
                {
                    Debug.WriteLine("addcup");
                    AlexData.operateflag = 1;
                    opendoor2();
                    System.Threading.Thread.Sleep(AlexData.receiveGap);
                    opendoor2();
                    AlexData.operateflag = 0;

                    int cupcount = Convert.ToInt32(commandargs[1]);
                    ZAlexFunctions.setCupcount(cupcount);
                    AlexData.currentcupcount = ZAlexFunctions.getCupcount();
                }
                else if (commandargs[0].CompareTo("outputyog") == 0)
                {
                    Debug.WriteLine("outputyog");
                    outputyog();
                }
                else if (commandargs[0].CompareTo("resetmanipulator") == 0)
                {
                    Debug.WriteLine("resetmanipulator");
                    resetmanipulator();
                }
                else if (commandargs[0].CompareTo("startclean1") == 0)
                {
                    Debug.WriteLine("startclean1");
                    int cleanduration = Convert.ToInt32(commandargs[1]);
                    AlexData.cleanduration = cleanduration;
                    startclean1();
                }
                else if (commandargs[0].CompareTo("stopclean1") == 0)
                {
                    Debug.WriteLine("stopclean1");
                    AlexData.td_startclean1.Abort();
                }
                else if (commandargs[0].CompareTo("startclean2") == 0)
                {
                    Debug.WriteLine("startclean2");
                    int cleanduration = Convert.ToInt32(commandargs[1]);
                    AlexData.cleanduration = cleanduration;
                    startclean2();
                }
                else if (commandargs[0].CompareTo("stopclean2") == 0)
                {
                    Debug.WriteLine("stopclean2");
                    AlexData.td_startclean2.Abort();
                }

            }
        }

        private void tbxRecvData_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbxSendData_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                AlexData.datareceiver.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.datasender.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            timer1.Stop();
            button2.Enabled = true;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = AlexData.temperature1.ToString();
            textBox2.Text = AlexData.temperature2.ToString();
            textBox3.Text = AlexData.liquidlevel1.ToString();
            textBox4.Text = AlexData.liquidlevel2.ToString();
            textBox5.Text = AlexData.pwm1.ToString();
            textBox6.Text = AlexData.pwm2.ToString();
            textBox7.Text = AlexData.pwm3.ToString();
            textBox8.Text = AlexData.pwm4.ToString();

            if (label9.BackColor != Color.Green)
            {
                label9.BackColor = Color.Green;
            }
            else
            {
                label9.BackColor = Color.Yellow;
            }

            if (AlexData.heatover == 1)
            {
                //button61.Enabled = true;
                AlexData.heatover = 0;
                button62_Click(sender, e);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            initios();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            resetmanipulator();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stepmotor1enable();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            stepmotor1disable();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            stepmotor2disable();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            stepmotor2enable();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            stepmotor3enable();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            stepmotor3disable();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            openmag();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            closemag();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            stepmotorfwd(1, 50);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            stepmotorrvs(1, 50);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            openmaterialsswitchstart();
            Thread.Sleep(12000);
            openmaterialsswitchend();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            closematerialsswitchstart();
            Thread.Sleep(12000);
            closematerialsswitchend();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            startmaterialsmotor();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            stopmaterialsmotor();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            opendisinfector();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            closedisinfector();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            opencleanser();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            closecleanser();
        }

        private void button57_Click(object sender, EventArgs e)
        {
            getpwm1();
            Thread.Sleep(AlexData.receiveGap);
            getpwm2();
            Thread.Sleep(AlexData.receiveGap);
            getpwm3();
            Thread.Sleep(AlexData.receiveGap);
            getpwm4();
            Thread.Sleep(AlexData.receiveGap);
            gettemperature1();
            Thread.Sleep(AlexData.longGap);
            gettemperature2();
            Thread.Sleep(AlexData.longGap);
            getliquidlevel1();
            Thread.Sleep(AlexData.longGap);
            getliquidlevel2();

        }

        private void button24_Click(object sender, EventArgs e)
        {
            startairpump1();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            stopairpump1();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            startairpump2();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            stopairpump2();
        }

        private void button28_Click(object sender, EventArgs e)
        {
            openhppump1();
        }

        private void button29_Click(object sender, EventArgs e)
        {
            closehppump1();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            openhppump2();
        }

        private void button31_Click(object sender, EventArgs e)
        {
            closehppump2();
        }

        private void button32_Click(object sender, EventArgs e)
        {
            openbarrelvalve();
        }

        private void button33_Click(object sender, EventArgs e)
        {
            closebarrelvalve();
        }

        private void button34_Click(object sender, EventArgs e)
        {
            opendrain1();
        }

        private void button35_Click(object sender, EventArgs e)
        {
            closedrain1();
        }

        private void button36_Click(object sender, EventArgs e)
        {
            opendrainout();
        }

        private void button37_Click(object sender, EventArgs e)
        {
            closedrainout();
        }

        private void button38_Click(object sender, EventArgs e)
        {
            startheater();
        }

        private void button40_Click(object sender, EventArgs e)
        {
            openrefrigvalve1();
            AlexData.refrigvalve1 = 1;
        }

        private void button41_Click(object sender, EventArgs e)
        {            
            if (AlexData.refrigvalve2 == 0)
            {
                stopcompressor();
            }
            Thread.Sleep(AlexData.receiveGap);

            closerefrigvalve1();
            AlexData.refrigvalve1 = 0;

        }

        private void button42_Click(object sender, EventArgs e)
        {
            openrefrigvalve2();
            AlexData.refrigvalve2 = 1;
        }

        private void button43_Click(object sender, EventArgs e)
        {
            if (AlexData.refrigvalve1 == 0)
            {
                stopcompressor();
            }
            Thread.Sleep(AlexData.receiveGap);

            closerefrigvalve2();
            AlexData.refrigvalve2 = 0;
        }

        private void button44_Click(object sender, EventArgs e)
        {
            if (AlexData.refrigvalve1 == 1 || AlexData.refrigvalve2 == 1)
            {
                startcompressor();
            }
        }

        private void button45_Click(object sender, EventArgs e)
        {
            stopcompressor();
        }

        private void button46_Click(object sender, EventArgs e)
        {
            opendoor1();
        }

        private void button47_Click(object sender, EventArgs e)
        {
            closedoor1();
        }

        private void button48_Click(object sender, EventArgs e)
        {
            opendoor2();
        }

        private void button49_Click(object sender, EventArgs e)
        {
            closedoor2();
        }

        private void button50_Click(object sender, EventArgs e)
        {
            startimpeller1();
        }

        private void button51_Click(object sender, EventArgs e)
        {
            stopimpeller1();
        }

        private void button53_Click(object sender, EventArgs e)
        {
            stopimpeller2();
        }

        private void button52_Click(object sender, EventArgs e)
        {
            startimpeller2();
        }

        private void button54_Click(object sender, EventArgs e)
        {
            try
            {
                AlexData.brewtemperature = 42;
                AlexData.brewpressure = 1000;
                AlexData.brewduration = 1080;
                startbrewing();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void button55_Click(object sender, EventArgs e)
        {
            try
            {
                AlexData.brewflag = 0;
                AlexData.td_startbrewing.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void button56_Click(object sender, EventArgs e)
        {
            AlexData.cleanduration = 1;
            startclean2();
        }

        private void button58_Click(object sender, EventArgs e)
        {          
            try
            {
                AlexData.td_startclean2.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void button39_Click(object sender, EventArgs e)
        {
            stopheater();
        }

        private void button59_Click(object sender, EventArgs e)
        {
            button60_Click(sender, e);

            AlexData.td_normaltemp = new Thread(new ThreadStart(td_normaltemp));
            AlexData.td_normaltemp.Start();

            button59.Enabled = false;
        }

        private void button60_Click(object sender, EventArgs e)
        {
            try
            {
                AlexData.td_normaltemp.Abort();

                stopcompressor();
                System.Threading.Thread.Sleep(AlexData.receiveGap);
                stopcompressor();
                System.Threading.Thread.Sleep(AlexData.receiveGap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            button59.Enabled = true;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                AlexData.td_material2tank.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_normaltemp.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_outputyog.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_resetmanipulator.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_startbrewing.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_startclean1.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.td_startclean2.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                AlexData.datareceiver.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                AlexData.datasender.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Application.Exit();
        }

        private void button61_Click(object sender, EventArgs e)
        {
            
            button62_Click(sender, e);

            AlexData.brewflag = 1;
            AlexData.heatover = 0;

            AlexData.td_heattemp = new Thread(new ThreadStart(td_heattemp));
            AlexData.td_heattemp.Start();

            AlexData.starttime = DateTime.Now;
            AlexData.endtime = AlexData.starttime.AddHours(Convert.ToInt32(textBox9.Text));

            label10.Text = AlexData.endtime.ToLongTimeString();

            button61.Enabled = false;
        }

        private void button62_Click(object sender, EventArgs e)
        {
            try
            {
                AlexData.td_heattemp.Abort();

                stopheater();
                System.Threading.Thread.Sleep(AlexData.receiveGap);
                stopheater();
                System.Threading.Thread.Sleep(AlexData.receiveGap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            button61.Enabled = true;
            AlexData.brewflag = 0;
        }
    }

    public static class AlexData
    {
        public const int COILREAD = 0x01;
        public const int REGREAD = 0x03;
        public const int COILWRITE = 0x05;
        public const int REGWRITE = 0x06;
        public const int EXCEPTIONREAD = 0x07;
        public const int BROADCASTTEST = 0x08;
        public const int NCOILWRITE = 0x0f;
        public const int NREGWRITE = 0x10;
        public const int NREGREADWRITE = 0x17;

        public const int RBARO = 0x01;
        public const int RELEVATION = 0x02;
        public const int RTEMPERATURE = 0x03;
        public const int RHUMIDITY = 0x04;
        public const int RPWM1 = 0x05;
        public const int RPWM2 = 0x06;
        public const int RPWM3 = 0x07;
        public const int RPWM4 = 0x08;
        public const int RPITCH = 0x09;
        public const int RROLL = 0x0a;
        public const int RYAW = 0x0b;
        public const int WMOTORFWD = 0x0c;
        public const int WMOTORRVS = 0x0d;
        public const int WMOTORINIT = 0x0e;
        public const int RDISTANCE = 0x0f;
        public const int SENSORMAX = 0x10;
        public const int REBOOTMCU = 0xfc;

        public static Thread datareceiver;   //Thread that receive data from rs485 modbus
        public static Thread datasender;     //Thread that send data to rs485 modbus
        public static List<byte[]> received; //Data received
        public static long newdatastamp;     //Data receive timestamp
        //public static byte[] receiveBuff;    //Data receive buff
        //public static int receiveTick;       //Data buff current index
        //public static int receiveLength;     //Data length

        public static int receiveGap = 50;               //modbus receive gap miliseconds
        public static int receiveGap2 = 7;              //modbus receive gap 2 miliseconds
        public static int maxDisplaychars = 8192;        //max display chars in richtextbox
        public static int maxReceived = 65535;           //max buffed items in receive list
        public static int barrelheight = 575;            //barrel height in milimeter
        public static int barrelheight2 = 575;           //barrel height 2 in milimeter
        public static int liquidlevelmax = 450;          //max liquid height in milimeter
        public static int longGap = 1000;                //business logic thread sleep gap miliseconds
        public static int temperatureRange = 2;          //temperature control range

        public static int currentaddr;                  //current received packet modbus address
        public static int currentfunc;                  //current received packet modbus function
        public static byte[] dataPrepared;              //data prepared for parser
        public static byte[] dataTemp;                  //data cache for parser
        public static byte[] dataTemp2;                 //data cache for parser
        public static byte[] dataTemp3;                 //data cache for parser
        public static int regaddr;                      //current register address
        public static int regval;                       //current register value
        public static float regvalf;                    //current register value float
        public static int pinnum;                       //current coil pin number
        public static int pinstate;                     //current coil pin state


        public static RedisClient Redis;
        public static int mid;                          //machine id 
        public static string tempstr;                   //temp string for redis reading
        public static int bid;                          //current yogurt make brewing id
        public static int cid;                          //current clean operation id

        //business logic vars
        public static Thread td_material2tank;
        public static Thread td_startbrewing;
        public static Thread td_normaltemp;
        public static Thread td_heattemp;
        public static Thread td_outputyog;
        public static Thread td_resetmanipulator;
        public static Thread td_startclean1;
        public static Thread td_startclean2;

        public static int sp_material2tank = 0;
        public static int sp_startbrewing = 0;
        public static int sp_outputyog = 0;
        public static int sp_resetmanipulator = 0;
        public static int sp_startclean1 = 0;
        public static int sp_startclean2 = 0;

        public static int powder;
        public static int water;
        public static int pwm1;
        public static int pwm2;
        public static int pwm3;
        public static int pwm4;
        public static int pwm1laststart;
        public static int pwm1factor = 50;
        public static int brewtemperature;
        public static int brewpressure;
        public static int brewduration;
        public static int temperature1;
        public static int pressure1;
        public static int liquidlevel1;
        public static int temperature2;
        public static int pressure2;
        public static int liquidlevel2;
        public static int normaltemp1;
        public static int normaltemp2;
        public static int heattemp = 42;
        public static int brewflag;
        public static int operateflag;
        public static int refrigvalve1;
        public static int refrigvalve2;
        public static int liquidlevelmincount;
        public static int currentcupcount;
        public static int pwm2laststart;
        public static int pwm2onecup = 10;
        public static int cleanduration;

        public static DateTime starttime;
        public static DateTime endtime;
        public static int heatover=0;
    }
}
