using System;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Interfaces.ExtensionDevice;
using Crestron.RAD.DeviceTypes.ExtensionDevice;
using Crestron.RAD.Common.Enums;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.Common.Attributes.Programming;

namespace FutureNow_FNIP
{
    public class FutureNow_FNIP : AExtensionDevice, ITcp
    {
        #region Declarations
        internal int ChannelNumber;
        public TcpTransport Transport;
        public FutureNow_FNIP_Protocol Protocol;
        #endregion Declarations

        #region Property Keys
        private string[] Lights_Toggle_Keys;
        private string[] Lights_Slider_Keys;
		private string[] Lights_Percent_Keys;
		#endregion Property Keys

		#region UI properties
		private PropertyValue<bool>[] Toggle_Property;
        private PropertyValue<int>[] Slider_Property;
		private PropertyValue<string>[] Percent_Property;
		#endregion UI properties

        #region Delegates
        public delegate void UI_Update_Delegate();
        #endregion Delegates

        public FutureNow_FNIP()
        {
            #region Debug Message
            Log("Device_Name - Constructor - Start");
            #endregion Debug Message

            #region Debug Message
            Log("Device_Name - Constructor - Finish");
            #endregion Debug Message
        }

		public void Initialize(IPAddress ipAddress, int port)
		{
			EnableLogging = true;

			#region Debug Message
			Log("Device_Name - Initialize - Start");
			#endregion Debug Message


			#region Setup UI
			ChannelNumber = 6;

			Create_Device_Definition();
			#endregion Setup UI

			#region Setup Transport
			try
			{
				Transport = new TcpTransport
				{
					EnableAutoReconnect = EnableAutoReconnect
				};
				Transport.Initialize(ipAddress, port);
				ConnectionTransport = Transport;
			}
			catch (Exception)
			{
			}
			#endregion Setup Transport

			#region Setup Protocol
			try
			{
				Protocol = new FutureNow_FNIP_Protocol(Transport, Id, this)
				{
					EnableLogging = InternalEnableLogging,
					CustomLogger = InternalCustomLogger
				};

				DeviceProtocol = Protocol;
				DeviceProtocol.Initialize(DriverData);
			}
			catch (Exception e)
			{
				string err = "FutureNow_FNIP - Initialize - Error Setting Up Protocol: " + e;
				#region Debug Message
				Log(err);
				#endregion Debug Message
				Crestron.SimplSharp.ErrorLog.Error(err + "\n");
			}
			#endregion Setup Protocol

			#region Debug Message
			Log("Device_Name - Initialize - Finish");
			#endregion Debug Message
		}

		private void Create_Device_Definition()
        {
			#region Debug Message
			Log("Device_Name - Create_Device_Definition - Start");
			#endregion Debug Message
			Toggle_Property = new PropertyValue<bool>[ChannelNumber];
			Slider_Property = new PropertyValue<int>[ChannelNumber];
			Percent_Property = new PropertyValue<string>[ChannelNumber];
			Lights_Toggle_Keys = new string[ChannelNumber];
			Lights_Slider_Keys = new string[ChannelNumber];
			Lights_Percent_Keys = new string[ChannelNumber];

			for (int i = 0; i < ChannelNumber; i++)
            {
				Lights_Toggle_Keys[i] = "Light" + Convert.ToString(i + 1) + "value";
				Lights_Slider_Keys[i] = "Slider" + Convert.ToString(i + 1) + "value";
				Lights_Percent_Keys[i] = "Percent" + Convert.ToString(i + 1) + "value";
				Toggle_Property[i] = CreateProperty<bool>(new PropertyDefinition(Lights_Toggle_Keys[i], null, DevicePropertyType.Boolean));
				Slider_Property[i] = CreateProperty<int>(new PropertyDefinition(Lights_Slider_Keys[i], null, DevicePropertyType.Int32, 0, 100, 1));
				Percent_Property[i] = CreateProperty<string>(new PropertyDefinition(Lights_Percent_Keys[i], null, DevicePropertyType.String));
				Percent_Property[i].Value = "0";
			}
			#region Debug Message
			Log("Device_Name - Create_Device_Definition - Finish");
			#endregion Debug Message

		}

		public void Update_UI(int[] togglestate, int[] sliderstate, int[] secondarylabel)
		{
			#region Debug Message
			Log("Device_Name - Update_UI - Start");
			#endregion Debug Message
			

			if(togglestate.Length != ChannelNumber || sliderstate.Length != ChannelNumber)
            {
				return;
            }
            for (int i = 0; i < ChannelNumber; i++)
            {
				secondarylabel[i] = 0;
				if(togglestate[i] != -1)
                {
					Toggle_Property[i].Value = Convert.ToBoolean(togglestate[i]);
					Slider_Property[i].Value = sliderstate[i];
					secondarylabel[i] = sliderstate[i];
					Percent_Property[i].Value = secondarylabel[i].ToString();
				}
				if(sliderstate[i] != -1)
                {
                    if (sliderstate[i] > 0)
                    {
						Toggle_Property[i].Value = true;
					}
					else
                    {
						Toggle_Property[i].Value = false;
                    }

					Slider_Property[i].Value = sliderstate[i];
					secondarylabel[i] = sliderstate[i];
					Percent_Property[i].Value = secondarylabel[i].ToString();

				}
			}

			Commit();

			#region Debug Message
			Log("Device_Name - Update_UI - Finish");
			#endregion Debug Message
		}

		public void SetStatus(bool status)
		{
			Connected = status;
		}

        #region Events
		[ProgrammableOperation ("Lights On")]
		public void AllLightsOn()
        {
			Protocol.TurnOnLight(0);
        }

		[ProgrammableOperation("Lights Off")]
		public void AllLightsOff()
		{
			Protocol.TurnOffLight(0);
		}
		#endregion Events

		#region Overrides
		protected override IOperationResult DoCommand(string command, string[] parameters)
		{
			//The tile of the Device was pressed
			if (command == "AllOn")
			{
				//Invert the state of all lights
				Protocol.ToggleLight(0);
			}

			return new OperationResult(OperationResultCode.Success);
		}

		protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
		{
			//Search through all the property keys
			for (int i = 0; i < ChannelNumber; i++)
			{
				if (propertyKey == Lights_Toggle_Keys[i])
				{
					//Turn on or off the Light i, depending on the state of the property
					var state = value as bool?;
					var sliderstate = value as int?;
					if (state != null)
					{
						/*if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
						}*/
						Protocol.ToggleLight(i + 1);
						Protocol.SliderLight(i + 1, (int)sliderstate);
					}
					//Save the state of the toggle
					Toggle_Property[i].Value = (bool)state;
					Percent_Property[i].Value = sliderstate.ToString();
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
                else if (propertyKey == Lights_Slider_Keys[i])
                {
					var sliderstate = value as int?;
                    if (sliderstate != null)
                    {
						Protocol.SliderLight(i + 1, (int)sliderstate);

						if ((int)sliderstate > 0)
						{
							Toggle_Property[i].Value = true;
						}
						else
						{
							Toggle_Property[i].Value = false;
						}
					}

					Percent_Property[i].Value = sliderstate.ToString();
					Slider_Property[i].Value = (int)sliderstate;

					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Percent_Keys[i])
                {
					var sliderstate = value as int?;
					Percent_Property[i].Value = sliderstate.ToString();
				}
			}
			return new OperationResult(OperationResultCode.Error);
		}

		protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
		{
			//Search through all the property keys
			for (int i = 0; i < ChannelNumber; i++)
			{
				if (propertyKey == Lights_Toggle_Keys[i])
				{
					//Turn on or off the Light i, depending on the state of the property
					var state = value as bool?;
					var sliderstate = value as int?;
					if (state != null)
					{
						/*if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
						}*/
						Protocol.ToggleLight(i + 1);
						Protocol.SliderLight(i + 1, (int)sliderstate);
					}
					//Save the state of the toggle
					Percent_Property[i].Value = sliderstate.ToString();
					Toggle_Property[i].Value = (bool)state;
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Slider_Keys[i])
				{
					var sliderstate = value as int?;
					if (sliderstate != null)
					{
						
						if ((int)sliderstate > 0)
						{
							Toggle_Property[i].Value = true;
						}
						else
						{
							Toggle_Property[i].Value = false;
						}
						Protocol.SliderLight(i + 1, (int)sliderstate);
					}

					Percent_Property[i].Value = sliderstate.ToString();
					Slider_Property[i].Value = (int)sliderstate;

					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
			}
			return new OperationResult(OperationResultCode.Error);
		}

		public override void Connect()
		{
			if (Protocol == null)
			{
				return;
			}
			else
			{
				//Connect to the Server
				base.Connect();

				//Set "Connected" to true if the connection succeeded
				if (Transport.Connected)
				{
					Connected = true;
				}
				else
				{
					Connected = false;
				}
			}
		}

		public override void Disconnect()
		{
			if (Protocol == null)
			{
				return;
			}
			else
			{
				//Disconnect from the Server
				base.Disconnect();

				Connected = false;
			}
		}
		#endregion Overrides
	}
}
