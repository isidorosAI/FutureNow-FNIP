using System;
using System.IO;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Transports;

namespace FutureNow_FNIP
{
    public class FutureNow_FNIP_Protocol : ABaseDriverProtocol, IDisposable
    {



        private FutureNow_FNIP Device { get; }
        public FutureNow_FNIP_Protocol(ISerialTransport transport, byte id, FutureNow_FNIP Parent) : base(transport, id)
        {
            ((TcpTransport)Transport).DataHandler = HandleMessage;
            Transport = (ATransportDriver)transport;
            Device = Parent;
        }

        public const string Lights_Name1_Key = "Name1value";
        public const string Lights_Name2_Key = "Name2value";
        public const string Lights_Name3_Key = "Name3value";
        public const string Lights_Name4_Key = "Name4value";
        public const string Lights_Name5_Key = "Name5value";
        public const string Lights_Name6_Key = "Name6value";



        public void HandleMessage(string msg)
        {
            int[] togglestate = new int[Device.ChannelNumber];
            int[] sliderstate = new int[Device.ChannelNumber];
            int[] secondarylabel = new int[Device.ChannelNumber];

            for (int i = 0; i < togglestate.Length; i++)
            {
                togglestate[i] = -1;
                sliderstate[i] = -1;
                secondarylabel[i] = -1;
            }

            using (StringReader sr = new StringReader(msg))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    
                    Log("Received message from FNIP:" + line);
                    
                    if (line.Contains("OUT,STOP"))
                    {
                        //Split the arguments of the message
                        string[] parts = line.Split(',');
                        
                        try
                        {
                            //If this line contains "ON", we set it to 1. If not, we set it to 0
                            sliderstate[Convert.ToInt32(parts[3]) - 1] = Convert.ToInt32(parts[4]);
                            secondarylabel[Convert.ToInt32(parts[3]) - 1] = Convert.ToInt32(parts[4]);
                            Log("OUT,STOP, Values are: Togglestate:" + togglestate[Convert.ToInt32(parts[3]) - 1] + "  Sliderstate:" + sliderstate[Convert.ToInt32(parts[3]) - 1] +
                                "  Secondarylabel:" + secondarylabel[Convert.ToInt32(parts[3]) - 1]);
                            Log("SUCCESS--------------" + line + "--------------------------------------");
                            Device.Update_UI(sliderstate, secondarylabel);

                        }
                            catch (Exception e)
                        {
                            Log("Received invalid message! Error: " + e.Message);
                        }
                    }

                    if (line.Contains("LEV"))
                    {
                        string[] parts = line.Split(',');

                        try
                        {
                            
                            togglestate[Convert.ToInt32(parts[2]) - 1] = Convert.ToInt32(parts[3]);
                            
                            Log("LEV, Values are: Togglestate:" + togglestate[Convert.ToInt32(parts[2]) - 1] + "  Sliderstate:" + sliderstate[Convert.ToInt32(parts[2]) - 1] +
                                "  Secondarylabel:" + secondarylabel[Convert.ToInt32(parts[2]) - 1]);
                            Log("SUCCESS--------------" + line + "--------------------------------------");
                            Device.Update_UI(togglestate);

                        }
                        catch (Exception e)
                        {
                            Log("Received invalid message! Error: " + e.Message);
                        }
                    }
                }
            }
            

        }

        public void TurnOffLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",0,1\r", null);
            Log("Turning off Light index: " + index);
        }

        public void TurnOnLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",100,1\r", null);
            Log("Turning on Light index: " + index);
        }

        public void ToggleLight(int index)
        {
            Transport.SendMethod("FN,TOG," + index + "\r", null);
            Log("Toggling light index: " + index);
        }

        public void SliderLight(int index, int slidervalue)
        {
            Transport.SendMethod("FN,RAMP," + index + "," + slidervalue + ",1\r", null);
            Log("Setting LEV on index: " + index + " to value:" + slidervalue);
        }

        public void GetDIMState()
        {
            Transport.SendMethod("FN,SRE\r", null);
        }

        public override void SetUserAttribute(string attributeId, string attributeValue)
        {
            for (int i = 0; i < Device.ChannelNumber; i++)
            {
                /*if (attributeId == Lights_Names_Keys[i])
                {
                    Device.Light_Name_Property[i].Value = attributeValue;
                }
                else
                {
                    
                }*/

                switch (attributeId)
                {
                    case Lights_Name1_Key:
                        Device.Light_Name_Property[0].Value = attributeValue;
                        break;
                    case Lights_Name2_Key:
                        Device.Light_Name_Property[1].Value = attributeValue;
                        break;
                    case Lights_Name3_Key:
                        Device.Light_Name_Property[2].Value = attributeValue;
                        break;
                    case Lights_Name4_Key:
                        Device.Light_Name_Property[3].Value = attributeValue;
                        break;
                    case Lights_Name5_Key:
                        Device.Light_Name_Property[4].Value = attributeValue;
                        break;
                    case Lights_Name6_Key:
                        Device.Light_Name_Property[5].Value = attributeValue;
                        break;
                    default:
                        Log($"Attribute {attributeId} is not supported");
                        break;
                }
            }
        }

        protected override void ConnectionChangedEvent(bool connection)
        {
            //When the connection's status is changed, inform the
            //interface about the status of this device
            Device.SetStatus(connection);
        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
        }
    }
}
