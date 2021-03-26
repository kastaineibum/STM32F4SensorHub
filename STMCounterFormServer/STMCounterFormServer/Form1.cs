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

// 336D6E2E-2200-3C00-0851-383136343536

namespace STMCounterFormServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DataPool.serveraddr = textBox5.Text;
                DataPool.serverport = textBox6.Text;
                DataPool.listenerThread = new Thread(new ThreadStart(flowListener));
                //DataPool.listenerThread.IsBackground = true;
                DataPool.listenerThread.Start();

                DataPool.Redis = new RedisClient("127.0.0.1", 6379);
                timer1.Interval = 100;
                timer1.Start();

                button1.Enabled = false;
                button2.Enabled = true;
            }
            catch (Exception ex)
            {
                if (DataPool.traceflag >= 3) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Exception " + ex.ToString() + "\r\n");
            }
        }

        private static void flowListener()
        {
            DataPool.clients = new List<TcpDevice>();
            IPAddress localIP = IPAddress.Parse("192.168.1.2");
            int localPort = 2431;
            try
            {
                localIP = IPAddress.Parse(DataPool.serveraddr);
                localPort = Convert.ToInt32(DataPool.serverport);
                DataPool.listener = new TcpListener(localIP, localPort);
                DataPool.listener.Start();
                if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Listener Start\r\n");
                DataPool.listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), DataPool.listener);
            }
            catch (Exception ex)
            {

            }
        }

        private static void DoAcceptTcpclient(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient client = listener.EndAcceptTcpClient(ar);

                TcpDevice td = new TcpDevice();

                td.tcpclient = client;
                td.deviceaddr = client.Client.RemoteEndPoint.ToString();
                td.commandbuff = "";
                td.adjusttimegap = DataPool.DefaultAdjInterval;
                td.logtimegap = DataPool.DefaultLogInterval;
                td.starttempstamp = 0;
                td.lasttempstamp = 0;
                td.lastlogstamp = 0;
                td.endtempstamp = 0;
                td.usertel = "-";
                td.tempname = "-";
                td.lasteledeviation = 0;
                td.vfdfrequency = 0;
                td.intake1 = 1;
                td.intake2 = 1;
                td.lastintake1 = -1;
                td.lastintake2 = -1;
                td.accintakecount = 0;
                td.decintakecount = 0;
                td.accpumpcount = 0;
                td.decpumpcount = 0;

                DataPool.clients.Add(td);

                if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Device Connected " + td.deviceaddr + "\r\n");

                Thread t = new Thread(new ParameterizedThreadStart(ReceiveMessageFromClient));
                t.Start(td);
                Thread t2 = new Thread(new ParameterizedThreadStart(SendMessageToClient));
                t2.Start(td);

                DataPool.listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), DataPool.listener);

                DataPool.recvlist.Add(t);
                DataPool.sendlist.Add(t2);

            }
            catch (Exception ex)
            {

            }
        }

        private static void DisplayDebugMessage(string msg)
        {
            try
            {
                //DataPool.rt1.AppendText(msg);
                DataPool.debugtxt.Add(msg);
            }
            catch (Exception ex)
            {
                
            }
        }

        private static void DisplayVFDFrequency(int f)
        {
            try
            {
                DataPool.vfdfrequencydisplay=f;
            }
            catch (Exception ex)
            {
                
            }
        }

        private static void SendTCPCommand(TcpDevice client, NetworkStream stream,string command)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(command);
            stream.Write(byteArray, 0, command.Length);
            if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] TO:" + client.deviceaddr + " " + command + "\r\n");
        }

        private static void SendMessageToClient(object reciveClient)
        {
            string tbuff0;
            string tbuff;
            string[] cmdsplit;
            long curstamp;
            TcpDevice client = reciveClient as TcpDevice;
            if (client.tcpclient == null)
            {
                if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Client Lost\r\n");
                return;
            }
            while (true)
            {
                try
                {
                    NetworkStream stream = client.tcpclient.GetStream();

                    //cabin operation
                    if (client.starttempstamp != 0)
                    {
                        curstamp = ZMoridaFunctions.GetShort3mnStamp();
                        if (curstamp - client.lasttempstamp > client.adjusttimegap)
                        {
                            client.lasttempstamp = curstamp;

                            //current real data frontend display
                            string rst3 = (curstamp - client.starttempstamp).ToString() + "^" + client.elevation.ToString() + "^";
                            //string rst3 = (curstamp - client.starttempstamp).ToString() + "^" + client.elevation.ToString() + "^" + client.baro.ToString() + "^" + client.temperature.ToString() + "^" + client.humidity.ToString() + "^";
                            DataPool.Redis.AddItemToList(client.deviceid + "-S", "realdata|" + rst3);

                            //environment control
                            int wantele = ZMoridaFunctions.getWantedElevation(client.tempname, Convert.ToInt32(curstamp - client.starttempstamp));
                            int cureledeviation = wantele - client.elevation;

                            if (wantele >= 0)
                            {
                                if (cureledeviation > 0)
                                {
                                    client.accintakecount = 0;
                                    client.decintakecount = 0;

                                    if (
                                        client.lasteledeviation > 0
                                        )
                                    {
                                        if (cureledeviation > client.lasteledeviation)
                                        {
                                            //accelerate pumping
                                            if (client.vfdfrequency > (DataPool.VFDFrequencyMIN * DataPool.VFDFrequencyPumpFactor))
                                            {
                                                if (client.accpumpcount >= 1)
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }

                                            }
                                            else
                                            {
                                                if (client.accpumpcount >= 1)
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP/2);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }

                                            }

                                            client.accpumpcount++;
                                            //client.decpumpcount = 0;
                                        }
                                        else
                                        {
                                            //decelerate pumping
                                            if (client.vfdfrequency > (DataPool.VFDFrequencyMIN * DataPool.VFDFrequencyPumpFactor))
                                            {
                                                if (client.accpumpcount == 1)
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else if(client.accpumpcount >= 2)
                                                {
                                                    if (client.elevation > 1000)
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                    }
                                                    else
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                    }
                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                            }
                                            else
                                            {
                                                //client.vfdfrequency = (DataPool.VFDFrequencyMIN * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }

                                            client.decpumpcount++;
                                            //client.accpumpcount = 0;
                                        }

                                    }
                                    else
                                    {
                                        //avr pumping
                                        if (client.vfdfrequency > (DataPool.VFDFrequencyMIN * DataPool.VFDFrequencyPumpFactor))
                                        {
                                            if (client.elevation > 800)
                                            {
                                                if (Math.Abs(cureledeviation) > 2*Math.Abs(client.lasteledeviation))
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP*DataPool.VFDFrequencyPumpFactor);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                            }
                                            else
                                            {
                                                if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                            {
                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }
                                            else
                                            {
                                                //client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }
                                        }

                                    }
                                }
                                else if (cureledeviation < 0)
                                {
                                    client.accpumpcount = 0;
                                    client.decpumpcount = 0;

                                    if (
                                        client.lasteledeviation < 0
                                        )
                                    {
                                        if (cureledeviation <= client.lasteledeviation)
                                        {
                                                //accelerate intake
                                                if (
                                                    client.vfdfrequency > DataPool.VFDFrequencyMIN && (client.vfdfrequency - DataPool.VFDFrequencySTEP > 0)
                                                    )
                                                {
                                                    if (client.vfdfrequency > (DataPool.VFDFrequencyMIN + DataPool.VFDFrequencyIntakeBias))
                                                    {
                                                        if (client.accintakecount == 1)
                                                        {
                                                            client.vfdfrequency += (DataPool.VFDFrequencySTEP);
                                                            client.intake1 = 1;
                                                            client.intake2 = 0;
                                                        }
                                                        else if (client.accintakecount >= 2)
                                                        {
                                                            client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                            client.intake1 = 1;
                                                            client.intake2 = 0;
                                                        }
                                                        else
                                                        {
                                                            if (client.elevation > 800 && client.elevation < 1200)
                                                            {
                                                                //client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }
                                                            else if (client.elevation >= 1200)
                                                            {
                                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }
                                                            else
                                                            {
                                                                client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }

                                                        }

                                                    }
                                                    else
                                                    {
                                                        if (client.accintakecount == 1)
                                                        {
                                                            client.vfdfrequency += (DataPool.VFDFrequencySTEP);
                                                            client.intake1 = 1;
                                                            client.intake2 = 0;
                                                        }
                                                        else if(client.accintakecount >= 2)
                                                        {
                                                            client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                            client.intake1 = 1;
                                                            client.intake2 = 0;
                                                        }
                                                        else
                                                        {
                                                            if (client.elevation > 800 && client.elevation < 1200)
                                                            {
                                                                //client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }
                                                            else if (client.elevation >= 1200)
                                                            {
                                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }
                                                            else
                                                            {
                                                                client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                                client.intake1 = 0;
                                                                client.intake2 = 0;
                                                            }
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    if (client.accintakecount == 0)
                                                    {
                                                        client.vfdfrequency = DataPool.VFDFrequencyMIN;
                                                        //client.vfdfrequency = 0;
                                                        //SendTCPCommand(client, stream, "setvfdstop|");
                                                        client.intake1 = 1;
                                                        client.intake2 = 0;
                                                    }
                                                    else if (client.accintakecount >= 2)
                                                    {
                                                        client.vfdfrequency -= (DataPool.VFDFrequencySTEP);
                                                        client.intake1 = 1;
                                                        client.intake2 = 0;
                                                    }
                                                }
                                                client.accintakecount++;
                                                //client.decintakecount = 0;

                                        }
                                        else
                                        {
                                            //decelerate intake
                                            if (client.vfdfrequency < (DataPool.VFDFrequencyMIN + DataPool.VFDFrequencyIntakeBias))
                                            {

                                                if (client.decintakecount == 1)
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 1;
                                                    client.intake2 = 0;
                                                }
                                                else if (client.decintakecount >= 2)
                                                {
                                                    client.vfdfrequency -= DataPool.VFDFrequencySTEP;

                                                    client.intake1 = 1;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    if (client.elevation > 800 && client.elevation < 1200)
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                    else if (client.elevation >= 1200)
                                                    {
                                                        client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                    else
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //client.vfdfrequency = (DataPool.VFDFrequencyMIN + DataPool.VFDFrequencyIntakeBias);
                                                if (client.decintakecount == 1)
                                                {
                                                    //client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                    client.intake1 = 1;
                                                    client.intake2 = 0;
                                                }
                                                else if (client.decintakecount >= 2)
                                                {
                                                    client.vfdfrequency -= DataPool.VFDFrequencySTEP;

                                                    client.intake1 = 1;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    if (client.elevation > 800 && client.elevation < 1200)
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                    else if (client.elevation >= 1200)
                                                    {
                                                        client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                    else
                                                    {
                                                        //client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);
                                                        client.intake1 = 0;
                                                        client.intake2 = 0;
                                                    }
                                                }

                                            }
                                            client.decintakecount++;
                                            //client.accintakecount = 0;
                                        }

                                    }
                                    else
                                    {
                                        //avr intake
                                        if (
                                            client.vfdfrequency > DataPool.VFDFrequencyMIN && (client.vfdfrequency - DataPool.VFDFrequencySTEP > 0)
                                            )
                                        {
                                            if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                            {
                                                client.vfdfrequency -= (DataPool.VFDFrequencySTEP/2);
                                                client.intake1 = 1;
                                                client.intake2 = 0;
                                            }
                                            else
                                            {
                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP);
                                                client.intake1 = 1;
                                                client.intake2 = 0;
                                            }

                                            if (client.elevation>1200) client.vfdfrequency += (DataPool.VFDFrequencySTEP*DataPool.VFDFrequencyPumpFactor);

                                        }
                                        else
                                        {
                                            client.vfdfrequency = DataPool.VFDFrequencyMIN;
                                            //client.vfdfrequency = 0;
                                            //SendTCPCommand(client, stream, "setvfdstop|");
                                            client.intake1 = 1;
                                            client.intake2 = 0;
                                        }



                                    }
                                }
                                else
                                {
                                    if (
                                        client.lasteledeviation > 0
                                        )
                                    {
                                        //avr pumping
                                        if (client.vfdfrequency > (DataPool.VFDFrequencyMIN * DataPool.VFDFrequencyPumpFactor))
                                        {
                                            if (client.elevation > 800)
                                            {
                                                client.vfdfrequency -= (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }
                                            else
                                            {
                                                if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                                {
                                                    client.vfdfrequency += (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                                else
                                                {
                                                    client.vfdfrequency -= (DataPool.VFDFrequencySTEP);

                                                    client.intake1 = 0;
                                                    client.intake2 = 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                            {
                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }
                                            else
                                            {
                                                //client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                                client.intake1 = 0;
                                                client.intake2 = 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //avr intake
                                        if (
                                            client.vfdfrequency > DataPool.VFDFrequencyMIN && (client.vfdfrequency - DataPool.VFDFrequencySTEP > 0)
                                            )
                                        {
                                            if (Math.Abs(cureledeviation) > Math.Abs(client.lasteledeviation))
                                            {
                                                client.vfdfrequency -= (DataPool.VFDFrequencySTEP / 2);
                                                client.intake1 = 1;
                                                client.intake2 = 0;
                                            }
                                            else
                                            {
                                                client.vfdfrequency += (DataPool.VFDFrequencySTEP);
                                                client.intake1 = 1;
                                                client.intake2 = 0;
                                            }

                                            if (client.elevation > 1200) client.vfdfrequency += (DataPool.VFDFrequencySTEP * DataPool.VFDFrequencyPumpFactor);

                                        }
                                        else
                                        {
                                            client.vfdfrequency = DataPool.VFDFrequencyMIN;
                                            //client.vfdfrequency = 0;
                                            //SendTCPCommand(client, stream, "setvfdstop|");
                                            client.intake1 = 1;
                                            client.intake2 = 0;
                                        }
                                    }
                                }

                                client.lasteledeviation = cureledeviation;
                            }

                            if(client.intake1!=client.lastintake1)
                            {
                                if (client.intake1 == 0)
                                {
                                    SendTCPCommand(client, stream, "airintake1close|");
                                }
                                else
                                {
                                    SendTCPCommand(client, stream, "airintake1open|");
                                }
                                Thread.Sleep(500);
                                if (client.intake1 == 0)
                                {
                                    SendTCPCommand(client, stream, "airintake1close|");
                                }
                                else
                                {
                                    SendTCPCommand(client, stream, "airintake1open|");
                                }
                                Thread.Sleep(500);
                            }
                            if (client.intake2 != client.lastintake2)
                            {
                                if (client.intake2 == 0)
                                {
                                    SendTCPCommand(client, stream, "airintake2close|");
                                }
                                else
                                {
                                    SendTCPCommand(client, stream, "airintake2open|");
                                }
                                Thread.Sleep(500);
                                if (client.intake2 == 0)
                                {
                                    SendTCPCommand(client, stream, "airintake2close|");
                                }
                                else
                                {
                                    SendTCPCommand(client, stream, "airintake2open|");
                                }
                                Thread.Sleep(500);
                            }

                            if (client.vfdfrequency > DataPool.VFDFrequencyMAX)
                            {
                                client.vfdfrequency = DataPool.VFDFrequencyMAX;
                            }

                            SendTCPCommand(client, stream, "setvfdf|" + client.vfdfrequency.ToString() + "|");
                            Thread.Sleep(500);
                            SendTCPCommand(client, stream, "setvfdf|" + client.vfdfrequency.ToString() + "|");
                            Thread.Sleep(500);

                            SendTCPCommand(client, stream, "setvfdrun|");
                            Thread.Sleep(500);

                            client.lastintake1 = client.intake1;
                            client.lastintake2 = client.intake2;

                            if (DataPool.traceflag >= 1)
                            {
                                DisplayDebugMessage("[" + DateTime.Now.ToString() + "] ExpectElevation:" + wantele.ToString() + "\r\n");
                                DisplayVFDFrequency(client.vfdfrequency);
                            }

                            if (wantele < 0)
                            {
                                int time3sec = Convert.ToInt32(client.endtempstamp - client.starttempstamp);
                                string time3str = (time3sec / 60).ToString() + "分" + (time3sec % 60).ToString() + "秒";
                                string time4str = "0分0秒";
                                string rst5 = client.userfullname + "|" + client.tempname + "|" + time3str + "|" + time4str + "|";
                                DataPool.Redis.AddItemToList(client.deviceid + "-S", "curinfo|" + rst5);
                                Thread.Sleep(100);

                                if (client.tempname.Contains("--"))
                                {
                                    ZMoridaFunctions.deleteSimpleTemp(client.tempname);
                                }

                                client.starttempstamp = 0;
                                client.lasttempstamp = 0;
                                client.lastlogstamp = 0;
                                client.endtempstamp = 0;
                                client.lasteledeviation = 0;
                                client.vfdfrequency = 0;
                                client.intake1 = 1;
                                client.intake2 = 1;
                                SendTCPCommand(client, stream, "setvfdstop|");
                                DataPool.Redis.AddItemToList(client.deviceid + "-S", "stopworking|");

                            }

                            if (client.elevation > DataPool.MAXElevation)
                            {
                                //safe stop
                                if (client.tempname.Contains("--"))
                                {
                                    ZMoridaFunctions.deleteSimpleTemp(client.tempname);
                                }

                                client.starttempstamp = 0;
                                client.lasttempstamp = 0;
                                client.lastlogstamp = 0;
                                client.endtempstamp = 0;
                                client.lasteledeviation = 0;
                                client.vfdfrequency = 0;
                                client.intake1 = 1;
                                client.intake2 = 1;
                                SendTCPCommand(client, stream, "setvfdstop|");
                                DataPool.Redis.AddItemToList(client.deviceid + "-S", "stopworking|");
                            }

                        }

                        if (curstamp - client.lastlogstamp > client.logtimegap)
                        {
                            client.lastlogstamp = curstamp;

                            //device log
                            ZMoridaFunctions.addDevicelog(client.elevation, client.baro, client.temperature, client.humidity, client.usertel, client.userfullname, client.curoptlogid);

                        }

                        //cabin using time and userinfo
                        int time1sec = Convert.ToInt32(curstamp - client.starttempstamp);
                        int time2sec = Convert.ToInt32(client.endtempstamp - curstamp);
                        string time1str = "-";
                        string time2str = "-";
                        if (time1sec >= 0)
                        { time1str = (time1sec / 60).ToString() + "分" + (time1sec % 60).ToString() + "秒"; }
                        if (time2sec >= 0)
                        { time2str = (time2sec / 60).ToString() + "分" + (time2sec % 60).ToString() + "秒"; }
                        string rst4 = client.userfullname + "|" + client.tempname + "|" + time1str + "|" + time2str + "|";
                        DataPool.Redis.AddItemToList(client.deviceid + "-S", "curinfo|" + rst4);

                    }

                    //send command if exists
                    if (client.commandbuff.Length > 0)
                    {
                        SendTCPCommand(client, stream, client.commandbuff);
                        client.commandbuff = "";
                    }

                    //get frontend command from redis (for unity3d client or mobile client)
                    tbuff0 = DataPool.Redis.PopItemFromList(client.deviceid + "-W");
                    tbuff = System.Web.HttpUtility.UrlDecode(tbuff0, System.Text.Encoding.UTF8);
                    if (tbuff0 != null && tbuff0.Length >0)
                    {
                        if (DataPool.traceflag >= 1)
                        {
                            DisplayDebugMessage("[" + DateTime.Now.ToString() + "] FROM REDIS:" + tbuff + "\r\n");
                        }
                        DataPool.Redis.RemoveAllFromList(client.deviceid + "-W");
                        tbuff0 = "";

                        if (tbuff.CompareTo("OK") == 0)
                        {
                            break;
                        }

                        cmdsplit = tbuff.Split('|');
                        if (cmdsplit.Length > 0)
                        {
                            if (cmdsplit[0].CompareTo("newuser") == 0) //new user created
                            {
                                string fullnamestr = cmdsplit[1];
                                string sexstr = cmdsplit[2];
                                string birthstr = cmdsplit[3];
                                string telstr = cmdsplit[4];
                                string addrstr = cmdsplit[5];
                                string medhisstr = cmdsplit[6];

                                string ssql = "insert into med_userext(user_login,fullname,sex,birth,tel,addr,medhis) values('" + telstr + "','" + fullnamestr + "','" + sexstr + "','" + birthstr + "','" + telstr + "','" + addrstr + "','" + medhisstr + "')";
                                int exrst = ZMoridaFunctions.ExecuteNonQuery(ssql);

                                string jsonstr = "{ \"token\": \"\",\"username\": \"" + telstr + "\",\"password\": \"1234\",\"email\": \"" + telstr + "@prt.fun\"}";
                                string postrst = ZMoridaFunctions.PostJson(DataPool.tx11.Text + "/wp/api-reg.php", jsonstr);

                                if (postrst.Contains("ok") && exrst >= 1)
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "newuserok");
                                }
                                else
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "newusererr");
                                }


                            }
                            else if (cmdsplit[0].CompareTo("verifylog1") == 0)//check user password
                            {
                                string telstr = cmdsplit[1];
                                string passstr = cmdsplit[2];

                                string jsonstr = "{ \"token\": \"\",\"username\": \"" + telstr + "\",\"password\": \""+passstr+"\",\"email\": \"" + telstr + "@prt.fun\"}";
                                string postrst = ZMoridaFunctions.PostJson(DataPool.tx11.Text + "/wp/api-verifylog.php", jsonstr);

                                if (postrst.Contains("verifylogok"))
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "verifylog1ok");
                                }
                                else
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "verifylog1err");
                                }
                            }
                            else if (cmdsplit[0].CompareTo("verifylog2") == 0)//check user password
                            {
                                string telstr = cmdsplit[1];
                                string passstr = cmdsplit[2];

                                string jsonstr = "{ \"token\": \"\",\"username\": \"" + telstr + "\",\"password\": \"" + passstr + "\",\"email\": \"" + telstr + "@prt.fun\"}";
                                string postrst = ZMoridaFunctions.PostJson(DataPool.tx11.Text + "/wp/api-verifylog.php", jsonstr);

                                if (postrst.Contains("verifylogok"))
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "verifylog2ok");
                                }
                                else
                                {
                                    DataPool.Redis.AddItemToList(client.deviceid + "-B", "verifylog2err");
                                }
                            }
                            else if (cmdsplit[0].CompareTo("startworking") == 0) //start vfd running with time/elevation
                            {
                                SendTCPCommand(client, stream, "setvfdf|" + DataPool.VFDFrequencyMIN + "|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdrun|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdf|0|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdf|0|");
                                Thread.Sleep(800);

                                client.usertel = cmdsplit[1];
                                ZMoridaFunctions.addSimpleTemp(Convert.ToInt32(cmdsplit[2]) * 60, Convert.ToInt32(cmdsplit[3]));
                                client.tempname = "--" + cmdsplit[2] + "min" + cmdsplit[3] + "meter";

                                client.userfullname = ZMoridaFunctions.getUserFullname(client.usertel);

                                string rst5 = ZMoridaFunctions.getTemplateData(client.tempname);
                                DataPool.Redis.AddItemToList(client.deviceid + "-B", "tempdata|" + rst5);

                                client.starttempstamp = ZMoridaFunctions.GetShort3mnStamp();
                                client.lasttempstamp = client.starttempstamp;
                                client.endtempstamp = ZMoridaFunctions.getEndtempstamp(client.tempname, client.starttempstamp);

                                ZMoridaFunctions.addOptlog(Convert.ToInt32(client.endtempstamp - client.starttempstamp), client.tempname, client.deviceid, client.usertel, Convert.ToInt32(cmdsplit[3]));
                                client.curoptlogid = ZMoridaFunctions.getLatestOptlogid(client.deviceid);

                            }
                            else if (cmdsplit[0].CompareTo("stopworking") == 0) //stop vfd running
                            {
                                if (client.endtempstamp - client.lasttempstamp > 300)
                                {
                                    ZMoridaFunctions.updateOptlogtime(client.curoptlogid, Convert.ToInt32(client.lasttempstamp - client.starttempstamp));
                                }

                                if (client.tempname.Contains("--"))
                                {
                                    ZMoridaFunctions.deleteSimpleTemp(client.tempname);
                                }

                                client.starttempstamp = 0;
                                client.lasttempstamp = 0;
                                client.lastlogstamp = 0;
                                client.endtempstamp = 0;

                                SendTCPCommand(client, stream, "setvfdstop|");

                            }
                            else if (cmdsplit[0].CompareTo("starttemp") == 0) //start vfd running with template
                            {
                                SendTCPCommand(client, stream, "setvfdf|" + DataPool.VFDFrequencyMIN + "|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdrun|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdf|0|");
                                Thread.Sleep(800);
                                SendTCPCommand(client, stream, "setvfdf|0|");
                                Thread.Sleep(800);

                                client.usertel = cmdsplit[1];
                                client.tempname = cmdsplit[2];
                                client.userfullname = ZMoridaFunctions.getUserFullname(client.usertel);

                                string rst2 = ZMoridaFunctions.getTemplateData(client.tempname);
                                DataPool.Redis.AddItemToList(client.deviceid + "-B", "tempdata|" + rst2);

                                client.starttempstamp = ZMoridaFunctions.GetShort3mnStamp();
                                client.lasttempstamp = client.starttempstamp;
                                client.endtempstamp = ZMoridaFunctions.getEndtempstamp(client.tempname, client.starttempstamp);

                                ZMoridaFunctions.addOptlog(Convert.ToInt32(client.endtempstamp - client.starttempstamp), client.tempname, client.deviceid, client.usertel);
                                client.curoptlogid = ZMoridaFunctions.getLatestOptlogid(client.deviceid);
                            }
                            else if (cmdsplit[0].CompareTo("gettempname") == 0) //get template names
                            {
                                string rst = ZMoridaFunctions.getTemplateNames();
                                DataPool.Redis.AddItemToList(client.deviceid + "-B", "tempnames|" + rst);
                            }
                        }
                        DataPool.Redis.RemoveAllFromList(client.deviceid + "-W");
                        tbuff0 = "";
                    }

                }
                catch (Exception e)
                {
                    try
                    {
                        if (DataPool.traceflag >= 3) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Exception " + e.ToString() + "\r\n");
                    }
                    catch (Exception ee)
                    {

                    }
                    break;
                }
                Thread.Sleep(500);
            }


            if (client.starttempstamp != 0)
            {
                DataPool.bakstarttempstamp = client.starttempstamp;
                DataPool.baklasttempstamp = client.lasttempstamp;
                DataPool.bakdeviceid = client.deviceid;
                DataPool.baktempname = client.tempname;
                DataPool.bakvfdfrequency = client.vfdfrequency;
            }

            //DataPool.clients.Remove(client);
            //DataPool.listenerThread.Abort();
            DataPool.suspendstart = 1;
            DataPool.frm1.button2_Click(DataPool.frm1, null);

            //client.tcpclient.Close();

        }

        private static void ReceiveMessageFromClient(object reciveClient)
        {
            string tbuff;
            string[] cmdsplit;
            byte[] result = new byte[2048];
            TcpDevice client = reciveClient as TcpDevice;
            if (client.tcpclient == null)
            {
                if(DataPool.traceflag==1)DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Client Lost\r\n");
                return;
            }
            while (true)
            {
                try
                {
                    NetworkStream stream = client.tcpclient.GetStream();
                    int num = stream.Read(result, 0, result.Length);       
                    if (num != 0)
                    {
                        string str = Encoding.UTF8.GetString(result, 0, num);
                        if (DataPool.traceflag >= 2) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] "+ client.deviceaddr + " " + str + "\r\n");

                        //process board client data
                        cmdsplit = str.Split('|');
                        if (cmdsplit.Length > 0)
                        {
                            if (cmdsplit[0].CompareTo("reg") == 0) //board register
                            {
                                client.deviceid = cmdsplit[1];
                                client.devicekey = cmdsplit[2];

                                int idxtd1=-1;
                                for(int i=0;i<DataPool.clients.Count;i++)
                                {
                                    if (DataPool.clients[i].deviceid.CompareTo(client.deviceid) == 0 && DataPool.clients[i].deviceaddr.CompareTo(client.deviceaddr) != 0)
                                    {
                                        idxtd1 = i;
                                        DataPool.clients[i].tcpclient.Close();
                                        break;
                                    }
                                }
                                if (idxtd1 != -1)
                                {
                                    DataPool.clients.RemoveAt(idxtd1);
                                }
                                
                                DataPool.tx1.Text = client.deviceid;

                                if (DataPool.bakdeviceid.CompareTo(client.deviceid) == 0)
                                {
                                    client.starttempstamp = DataPool.bakstarttempstamp;
                                    client.lasttempstamp = DataPool.baklasttempstamp;
                                    client.tempname = DataPool.baktempname;
                                    client.vfdfrequency = DataPool.bakvfdfrequency;
                                    DataPool.bakstarttempstamp = 0;
                                    DataPool.baklasttempstamp = 0;
                                    DataPool.bakdeviceid = "-";
                                    DataPool.bakvfdfrequency = 0;
                                }
                            }
                            else if (cmdsplit[0].CompareTo("fct") == 0) //flowmeter count
                            {
                                client.valvecontrol = 0;
                                client.flowcount = Convert.ToInt32(cmdsplit[2]);
                                //DataPool.tx4.Text = cmdsplit[2];
                            }
                            else if (cmdsplit[0].CompareTo("fcc") == 0) //emptying flowmeter torlerance
                            {
                                client.valvecontrol = 1;
                                client.thistimestartflowcount = Convert.ToInt32(cmdsplit[1]);
                                client.thistimecount = Convert.ToInt32(cmdsplit[2]);
                                client.flowcount = client.thistimestartflowcount + client.thistimecount;
                                DataPool.tx3.Text = cmdsplit[2];
                                //DataPool.tx4.Text = client.flowcount.ToString();
                            }
                            else if (cmdsplit[0].CompareTo("bth") == 0) //baro elevation temperature humidity
                            {
                                client.baro = Convert.ToInt32(cmdsplit[1]);
                                client.elevation = Convert.ToInt32(cmdsplit[2]);
                                client.temperature = Convert.ToInt32(cmdsplit[3]);
                                client.humidity = Convert.ToInt32(cmdsplit[4]);
                                DataPool.tx7.Text = cmdsplit[1];
                                DataPool.tx8.Text = cmdsplit[2];
                                DataPool.tx9.Text = cmdsplit[3];
                                DataPool.tx10.Text = cmdsplit[4];
                            }
                        }

                        //add to redis (for server UI or webapi)
                        DataPool.Redis.AddItemToList(client.deviceid + "-P", str);
                        if (DataPool.Redis.GetListCount(client.deviceid + "-P") > 256)
                        {
                            DataPool.Redis.RemoveAllFromList(client.deviceid + "-P");
                        }

                        //get command from redis (server UI or webapi)
                        foreach (TcpDevice td in DataPool.clients)
                        {
                            tbuff = DataPool.Redis.PopItemFromList(td.deviceid + "-C");
                            if (tbuff != null && tbuff.Length >0)
                            {
                                try
                                {
                                    if (Convert.ToInt32(tbuff) > 1000) //remove servicestack tick report
                                    {
                                        td.commandbuff = "";
                                        tbuff = "";
                                        continue;
                                    } 
                                }
                                catch (Exception)
                                {

                                }
                                td.commandbuff = tbuff;
                                DataPool.Redis.RemoveAllFromList(td.deviceid + "-C");
                                tbuff = "";
                            }
                        }

                    }
                    else
                    {
                        if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Client Lost\r\n");
                        break;
                    }


                }
                catch (Exception e)
                {
                    try
                    {
                        if (DataPool.traceflag >= 3) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] Exception " + e.ToString() + "\r\n");
                    }
                    catch (Exception ee)
                    {

                    }
                    break;
                }

            }

            if (client.starttempstamp != 0)
            {
                DataPool.bakstarttempstamp = client.starttempstamp;
                DataPool.baklasttempstamp = client.lasttempstamp;
                DataPool.bakdeviceid = client.deviceid;
                DataPool.baktempname = client.tempname;
                DataPool.bakvfdfrequency = client.vfdfrequency;
            }

            //DataPool.clients.Remove(client);
            //DataPool.listenerThread.Abort();
            DataPool.suspendstart = 1;
            DataPool.frm1.button2_Click(DataPool.frm1, null);
            
            //client.tcpclient.Close();


        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if(DataPool.debugtxt.Count>0)
            {
                DataPool.rt1.AppendText(DataPool.debugtxt[0]);
                DataPool.debugtxt.RemoveAt(0);

                if (DataPool.traceflag >= 1) DataPool.rt1.ScrollToCaret();
                if (DataPool.rt1.Lines.Length > DataPool.maxLines)
                {
                    DataPool.rt1.Clear();
                    /*
                    int moreLines = DataPool.rt1.Lines.Length - DataPool.maxLines;
                    string[] lines = DataPool.rt1.Lines;
                    Array.Copy(lines, moreLines, lines, 0, DataPool.maxLines);
                    Array.Resize(ref lines, DataPool.maxLines);
                    DataPool.rt1.Lines = lines;
                    */
                }

                if (DataPool.traceflag >= 1) DataPool.tx12.Text = DataPool.vfdfrequencydisplay.ToString();
            }
            if (DataPool.suspendstart == 1)
            {
                DataPool.suspendstartcnt++;
                if (DataPool.suspendstartcnt > 10)
                {
                    DataPool.suspendstart = 0;
                    DataPool.suspendstartcnt = 0;
                    button1_Click(sender, e);
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ZMoridaFunctions.deleteSimpleTemp();

            DataPool.baklasttempstamp = 0;
            DataPool.bakstarttempstamp = 0;
            DataPool.baktempname = "-";
            DataPool.bakdeviceid = "-";
            DataPool.bakvfdfrequency = 0;
            DataPool.suspendstartcnt = 0;
            DataPool.suspendstart = 0; // 1 auto start tcp listener
            DataPool.vfdfrequencydisplay = 0;
            DataPool.recvlist = new List<Thread>();
            DataPool.sendlist = new List<Thread>();
            DataPool.debugtxt = new List<string>();
            DataPool.Redis = new RedisClient("127.0.0.1", 6379);
            DataPool.frm1 = this;
            DataPool.rt1 = richTextBox1;
            DataPool.tx1 = textBox1;
            DataPool.tx2 = textBox2;
            DataPool.tx3 = textBox3;
            DataPool.tx4 = textBox4;
            DataPool.tx7 = textBox7;
            DataPool.tx8 = textBox8;
            DataPool.tx9 = textBox9;
            DataPool.tx10 = textBox10;
            DataPool.tx12 = textBox12;
            DataPool.tx11 = textBox11;
            DataPool.maxLines = 256;
            groupBox3.Text = "Debug Message (show "+ DataPool.maxLines + " lines)";
            timer1.Interval = 100;
            timer1.Start();
            DataPool.traceflag = 1;

            //button9_Click(sender, e);
            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (TcpDevice td in DataPool.clients)
                {
                   td.tcpclient.Close();
                }
                DataPool.clients.Clear();

                if(DataPool.listenerThread!=null)
                {
                    DataPool.listener.Stop();
                    DataPool.listenerThread.Abort();
                    DataPool.listenerThread = null;
                }

                if (DataPool.traceflag >= 1) DisplayDebugMessage("[" + DateTime.Now.ToString() + "] listener stop\r\n");
            }
            catch (Exception ex)
            {

            }

                button1.Enabled = true;
                button2.Enabled = false;

            if (DataPool.recvlist != null)
            {
                for (int i = 0; i < DataPool.recvlist.Count; i++)
                {
                    try
                    {
                        DataPool.recvlist[i].Abort();
                    }
                    catch (Exception exx)
                    {

                    }
                }
                DataPool.recvlist.Clear();
            }
            if (DataPool.sendlist != null)
            {
                for (int i = 0; i < DataPool.sendlist.Count; i++)
                {
                    try
                    {
                        DataPool.sendlist[i].Abort();
                    }
                    catch (Exception exx)
                    {

                    }
                }
                DataPool.sendlist.Clear();
            }

            //Thread.Sleep(30);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                DisplayDebugMessage("-------------device list------------\r\n");
                foreach (TcpDevice td in DataPool.clients)
                {
                    DisplayDebugMessage("deviceid:"+td.deviceid+"\r\n");
                    DisplayDebugMessage("deviceaddr:" + td.deviceaddr + "\r\n");
                    DisplayDebugMessage("devicekey:" + td.devicekey + "\r\n");
                    DisplayDebugMessage("flowcount:" + td.flowcount + "\r\n");
                    DisplayDebugMessage("thistimecount:" + td.thistimecount + "\r\n");
                    DisplayDebugMessage("thistimestartflowcount:" + td.thistimestartflowcount + "\r\n");
                    DisplayDebugMessage("valvecontrol:" + td.valvecontrol + "\r\n");
                    DisplayDebugMessage("baro:" + td.baro + "\r\n");
                    DisplayDebugMessage("elevation:" + td.elevation + "\r\n");
                    DisplayDebugMessage("temperature:" + td.temperature + "\r\n");
                    DisplayDebugMessage("humidity:" + td.humidity + "\r\n");
                    DisplayDebugMessage("\r\n");
                }
                DisplayDebugMessage("------------------------------------\r\n");
            }
            catch (Exception ex)
            {

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                DataPool.rt1.Clear();
            }
            catch (Exception ex)
            {

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (DataPool.traceflag >= 1)
                {
                    DataPool.traceflag = 0;
                    button9.Text = "Start Trace";
                }
                else
                {
                    DataPool.traceflag = 1;
                    button9.Text = "End Trace";
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DataPool.tx3.Text = "0";
                tdwant.commandbuff = "tor|"+DataPool.tx2.Text+"|0|";
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DataPool.tx3.Text = "-";
                tdwant.commandbuff = "waterclose";
                //tdwant.commandbuff = "tor|0|0|"; //将允许量置零同样可以关闭阀门
            }
            catch (Exception ex)
            {

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DataPool.tx4.Text = tdwant.flowcount.ToString();
            }
            catch (Exception ex)
            {

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DisplayDebugMessage("-------------device status---------------\r\n");
                    DisplayDebugMessage("deviceid:" + tdwant.deviceid + "\r\n");
                    DisplayDebugMessage("deviceaddr:" + tdwant.deviceaddr + "\r\n");
                    DisplayDebugMessage("devicekey:" + tdwant.devicekey + "\r\n");
                    DisplayDebugMessage("flowcount:" + tdwant.flowcount + "\r\n");
                    DisplayDebugMessage("thistimecount:" + tdwant.thistimecount + "\r\n");
                    DisplayDebugMessage("thistimestartflowcount:" + tdwant.thistimestartflowcount + "\r\n");
                    DisplayDebugMessage("valvecontrol:" + tdwant.valvecontrol + "\r\n");
                    DisplayDebugMessage("baro:" + tdwant.baro + "\r\n");
                    DisplayDebugMessage("elevation:" + tdwant.elevation + "\r\n");
                    DisplayDebugMessage("temperature:" + tdwant.temperature + "\r\n");
                    DisplayDebugMessage("humidity:" + tdwant.humidity + "\r\n");
                DisplayDebugMessage("\r\n");
                DisplayDebugMessage("------------------------------------\r\n");
            }
            catch (Exception ex)
            {

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DataPool.tx3.Text = "-";
                tdwant.commandbuff = "resettick";
            }
            catch (Exception ex)
            {

            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                DataPool.tx3.Text = "-";
                tdwant.commandbuff = "restart";
            }
            catch (Exception ex)
            {

            }
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = textBox15.Text;
            }
            catch (Exception ex)
            {

            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "setvfdf|" + DataPool.tx12.Text + "|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "setvfdrun|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "setvfdstop|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "setvfdpositiverot|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "setvfdreverserot|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(textBox11.Text+"/wp/wp-admin");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            ZMoridaFunctions.connStr = "server="+ textBox11.Text.Replace("http://","") + ";port=3306;user=root;password=ll123456!;database=wp503;";
            string dbrst = ZMoridaFunctions.TestReadWpusers();
            if (dbrst.Length < 1)
            {
                ZMoridaFunctions.connStr = "server=127.0.0.1;port=3306;user=root;password=ll123456!;database=wp503;";
                dbrst = ZMoridaFunctions.TestReadWpusers();
            }
            string result = ZMoridaFunctions.connStr + "\r\n" + dbrst;
            MessageBox.Show(result);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string testjson = "{ \"token\": \"\",\"username\": \"test001\",\"password\": \"\",\"email\": \"3mn@3mn.net\"}";
            string result = ZMoridaFunctions.PostJson(textBox11.Text + textBox13.Text, textBox14.Text);
            MessageBox.Show(result);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            button2_Click(sender, e);
            Application.Exit();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "airintake1close|";
            }
            catch (Exception ex)
            {

            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                TcpDevice tdwant = null;
                foreach (TcpDevice td in DataPool.clients)
                {
                    if (td.deviceid.CompareTo(DataPool.tx1.Text) == 0)
                    {
                        tdwant = td;
                        break;
                    }
                }
                if (tdwant == null)
                {
                    MessageBox.Show("cant find in online devices " + DataPool.tx1.Text);
                    return;
                }
                tdwant.commandbuff = "airintake1open|";
            }
            catch (Exception ex)
            {

            }
        }
    }

    public static class DataPool
    {
        public static int suspendstartcnt; //tcp listener auto starter count
        public static int suspendstart;
        public static int traceflag; //debug message display priority
        public static int maxLines; //how many debug message lines can be displayed on UI
        public static Thread listenerThread;
        public static TcpListener listener;
        public static List<TcpDevice> clients;
        public static Form1 frm1;
        public static RichTextBox rt1;
        public static TextBox tx1;
        public static TextBox tx2;
        public static TextBox tx3;
        public static TextBox tx4;
        public static TextBox tx7;
        public static TextBox tx8;
        public static TextBox tx9;
        public static TextBox tx10;
        public static TextBox tx12;
        public static TextBox tx11;
        public static string serveraddr;
        public static string serverport;
        public static RedisClient Redis;

        public static List<Thread> recvlist; //receiver threads pool
        public static List<Thread> sendlist; //sender threads pool
        public static List<string> debugtxt; //debug text queue

        public static int VFDFrequencyMAX = 6000; //
        public static int VFDFrequencyMIN = 1000; //the real min value may be this - (VFDFrequencySTEP*VFDFrequencyPumpFactor+VFDFrequencySTEP)
        public static int VFDFrequencySTEP = 100;
        public static int VFDFrequencyPumpFactor = 2; //value greater than 1
        public static int VFDFrequencyIntakeBias = 1200;
        public static int vfdfrequencydisplay;
        public static int DefaultAdjInterval = 1;
        public static int DefaultLogInterval = 30;

        public static int MAXElevation = 2500; //when greater than this, the cabin stop working

        public static long bakstarttempstamp;
        public static long baklasttempstamp;
        public static string bakdeviceid;
        public static string baktempname;
        public static int bakvfdfrequency;
    }

    public class TcpDevice
    {
        public TcpClient tcpclient; //
        public string commandbuff; //operation command buffer
        public string deviceaddr; //device tcp address
        public string deviceid; //device id
        public string devicekey; //device 3mn.net key
        public int flowcount; //flowmeter count
        public int valvecontrol; //flowmeter and valve control
        public int thistimestartflowcount; //flowmeter count when this time operation start
        public int thistimecount; //flowmeter this time count
        public int baro; //
        public int elevation; //
        public int temperature; //
        public int humidity; //
        public long starttempstamp; //timestamp when cabin start working with template
        public long endtempstamp; //timestamp when cabin end working with template
        public long lasttempstamp; //timestamp when cabin latest operation occured
        public long lastlogstamp; //timestamp when cabin latest operation log occured
        public int adjusttimegap; //vfd operation interval
        public int logtimegap; //operation log interval

        public string usertel; //user's telephone number
        public string tempname; //current cabin operation template name
        public int lasteledeviation; //the latest cabin elevation deviation
        public int vfdfrequency; //the motor vfd output frequency
        public int intake1; //cabin air-intake valve 1 (intake on cabin 0-close 1-open)
        public int intake2; //cabin air-intake valve 2 (intake on pump motor 0-close 1-open)
        public int lastintake1;
        public int lastintake2;
        public int accintakecount; //
        public int decintakecount; //
        public int accpumpcount;
        public int decpumpcount;

        public string userfullname; //user's fullname
        public int curoptlogid; //latest optlogid (operation log id)
    }



}
