using System;
using System.IO;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Transports;

namespace FutureNow_FNIP
{
    public class FutureNow_FNIP_Protocol : ABaseDriverProtocol, IDisposable
    {
        private FutureNow_FNIP Device { get; }

        #region Initialization
        public FutureNow_FNIP_Protocol(ISerialTransport transport, byte id, FutureNow_FNIP Parent) : base(transport, id)
        {
            //Set the data handler for the received messages
            ((TcpTransport)Transport).DataHandler = HandleMessage;
            Transport = (ATransportDriver)transport;
            //Keep a reference to the Parent Device
            Device = Parent;
        }
        #endregion

        #region User Attributes Keys
        //Used to recognise the user input on Home Setup
        //Must be bound to a UI property
        public const string Lights_Name1_Key = "Name1value";
        public const string Lights_Name2_Key = "Name2value";
        public const string Lights_Name3_Key = "Name3value";
        public const string Lights_Name4_Key = "Name4value";
        public const string Lights_Name5_Key = "Name5value";
        public const string Lights_Name6_Key = "Name6value";

        public const string Lights_Ramp1_Key = "Ramp1value";
        public const string Lights_Ramp2_Key = "Ramp2value";
        public const string Lights_Ramp3_Key = "Ramp3value";
        public const string Lights_Ramp4_Key = "Ramp4value";
        public const string Lights_Ramp5_Key = "Ramp5value";
        public const string Lights_Ramp6_Key = "Ramp6value";
        public const string Lights_Ramp7_Key = "Ramp7value";
        #endregion User Attributes Keys

        #region Communication
        public void HandleMessage(string msg)
        {
            int[] state = new int[Device.ChannelNumber];
            //Initialize the array to -1
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = -1;
            }

            using (StringReader sr = new StringReader(msg))
            {
                string line;

                //For every line in the message string
                while ((line = sr.ReadLine()) != null)
                {
                    Log("Received message from FNIP:" + line);
                    
                    //If the relay reports it has sent the final dimming level value
                    if (line.Contains("OUT,STOP"))
                    {
                        /*Format of message:
						 *FN,OUT,STOP,channel,level<LF><CR>
						 *The index of the channel is the 4th element
						 *or element 3 if we have a zero indexed array.
						 *The index of the level value is 4 and its value 
						 *can range from 0 to 100*/

                        //Split the arguments of the message
                        string[] parts = line.Split(',');
                        
                        try
                        {
                            //Set the dimming level value to the correct channel (index)
                            state[Convert.ToInt32(parts[3]) - 1] = Convert.ToInt32(parts[4]);
                            Log("OUT,STOP, Value is: " + state[Convert.ToInt32(parts[3]) - 1]);
                            Device.Update_UI(state);

                        }
                            catch (Exception e)
                        {
                            Log("Received invalid message! Error: " + e.Message);
                        }
                    }
                    /*If any lines are still at -1 by now, it means that they were not
			        * included in the message, and UpdateUI will have to ignore them*/
                }
            }
        }
        #endregion Communication

        #region SendMethods
        //Methods to send Data to the TCP Server

        //FN,RAMP,channel,level,ramprate<CR> – (channel: 0-12; level: 0-255; ramprate: 0-65535 sec)
        //Ramps the specified channel to the specified dim level using the specified ramp rate

        //Turns off the indexed light
        public void TurnOffLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",0," + Device.Light_Ramp_Property[index - 1].Value.ToString() + "\r", null);
        }

        //Turns off the indexed light
        //Used for All Lights (Different ramp rate)
        public void TurnOffAllLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",0," + Device.Light_Ramp_Property[Device.ChannelNumber].Value.ToString() + "\r", null);
        }

        //Turns on the indexed light
        public void TurnOnLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",100," + Device.Light_Ramp_Property[index - 1].Value.ToString() + "\r", null);
        }

        //Turns on the indexed light
        //Used for All Lights (Different ramp rate)
        public void TurnOnAllLight(int index)
        {
            Transport.SendMethod("FN,RAMP," + index + ",100," + Device.Light_Ramp_Property[Device.ChannelNumber].Value.ToString() + "\r", null);
        }

        //Changes the dimming level of the indexed light to a certain value
        public void SliderLight(int index, int slidervalue)
        {
            Transport.SendMethod("FN,RAMP," + index + "," + slidervalue + "," + Device.Light_Ramp_Property[index - 1].Value.ToString() + "\r", null);
        }

        //Sets the preset dimming level of the indexed light
        public void SetDIMPresets(int index)
        {
            Transport.SendMethod("FN,PRESLEV," + index + ",100/r", null);
        }
        #endregion SendMethods

        #region User Inputs from Home Setup
        //Sets the user inputs on Home Setup
        public override void SetUserAttribute(string attributeId, string attributeValue)
        {
             switch (attributeId)
             {
                    #region Light Names
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
                        #endregion Light Names

                    #region Light Ramp Values
                        break;
                    case Lights_Ramp1_Key:
                        Device.Light_Ramp_Property[0].Value = attributeValue;
                        break;
                    case Lights_Ramp2_Key:
                        Device.Light_Ramp_Property[1].Value = attributeValue;
                        break;
                    case Lights_Ramp3_Key:
                        Device.Light_Ramp_Property[2].Value = attributeValue;
                        break;
                    case Lights_Ramp4_Key:
                        Device.Light_Ramp_Property[3].Value = attributeValue;
                        break;
                    case Lights_Ramp5_Key:
                        Device.Light_Ramp_Property[4].Value = attributeValue;
                        break;
                    case Lights_Ramp6_Key:
                        Device.Light_Ramp_Property[5].Value = attributeValue;
                        break;
                    case Lights_Ramp7_Key:
                        Device.Light_Ramp_Property[6].Value = attributeValue;
                        break;
                    #endregion Light Ramp Values

                    default:
                        Log($"Attribute {attributeId} is not supported");
                        break;
             }
        }
        #endregion User Inputs from Home Setup

        #region Overrides
        protected override void ConnectionChangedEvent(bool connection)
        {
            //When the connection's status is changed, inform the
            //interface about the status of this device
            Device.SetStatus(connection);
        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
        }
        #endregion Overrides
    }
}
