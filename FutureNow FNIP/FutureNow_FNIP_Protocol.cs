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
                    /*if (line.Contains("IN") || line.Contains("OUT"))
                    {
                        /*Format of message:
						 *FN,IN,status,channel<LF><CR> or FN,OUT,status,channel<LF><CR>
						 *The index of the channel is the 4th element
						 *or element 3 if we have a zero indexed array.
						 *The index of the status is 2 and it can 
						 *have the value ON for 1 or OFF for 0*/

                        //Split the arguments of the message
                       /* string[] parts = line.Split(',');

                        try
                        {
                            //If this line contains "ON", we set it to 1. If not, we set it to 0
                            togglestate[Convert.ToInt32(parts[3]) - 1] = Convert.ToInt32(parts[2] == "ON");
                        }
                        catch (Exception e)
                        {
                            Log("Received invalid message! Error: " + e.Message);
                        }
                    }*/
                    //else if (line.Contains("LEV"))
                    //{
                        /*Format of message:
						 *FN,LEV,channel,status<LF><CR>
						 *The index of the channel is the 3rd element
						 *or element 2 if we have a zero indexed array.
						 *The index of the status is 3 and it can 
						 *have a value ranging from 0 to 100
						 *These edge values are the only ones 
						 *that are important to us 0 represents boolean 0
						 *and 100 represents 1 for the status of the light*/

                        //Split the arguments of the message
                        string[] parts = line.Split(',');

                        try
                        {
                        //If the status part is > 0, the light is on
                        sliderstate[Convert.ToInt32(parts[2]) - 1] = Convert.ToInt32(parts[3]);
                        togglestate[Convert.ToInt32(parts[2]) - 1] = Convert.ToInt32(Convert.ToInt32(parts[3]) > 0);
                        secondarylabel[Convert.ToInt32(parts[2]) - 1] = Convert.ToInt32(parts[3]);
                        Log("SUCCESS----------------------------------------------------");
                        }
                        catch (Exception e)
                        {
                            Log("Received invalid message! Error: " + e.Message);
                        }
                    //}
                }
            }

            Device.Update_UI(togglestate, sliderstate, secondarylabel);

        }

        public void TurnOffLight(int index)
        {
            Transport.SendMethod("FN,OFF," + index + "\r", null);
        }

        public void TurnOnLight(int index)
        {
            Transport.SendMethod("FN,ON," + index + "\r", null);
        }

        public void ToggleLight(int index)
        {
            Transport.SendMethod("FN,TOG," + index + "\r", null);
        }

        public void SliderLight(int index, int slidervalue)
        {
            Transport.SendMethod("FN,LEV," + index + "," + slidervalue + "\r", null);
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
