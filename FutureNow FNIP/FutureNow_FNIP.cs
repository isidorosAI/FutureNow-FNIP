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
		private const string AllLightsIcon_Key = "AllLightsIcon";
		public bool AllLightsON = false;
		#endregion Property Keys

		#region UI properties
		public PropertyValue<bool>[] Toggle_Property;
        public PropertyValue<int>[] Slider_Property;
		public PropertyValue<string>[] Percent_Property;
		public PropertyValue<string>[] Light_Name_Property;
		private PropertyValue<string> AllLightsIcon_Property;
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

			CrestronConsole.AddNewConsoleCommand(GetDIMState, "UpdateState", "Updates the state of all lights", ConsoleAccessLevelEnum.AccessAdministrator);

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

			Protocol.GetDIMState();

			#region Debug Message
			Log("Device_Name - Initialize - Finish");
			#endregion Debug Message
		}

		private void GetDIMState (string cmdParameters)
        {
			Log("GetDIMState");
			Protocol.GetDIMState();
			Log("GetDIMState");
        }

		private void Create_Device_Definition()
        {
			#region Debug Message
			Log("Device_Name - Create_Device_Definition - Start");
			#endregion Debug Message
			Toggle_Property = new PropertyValue<bool>[ChannelNumber];
			Slider_Property = new PropertyValue<int>[ChannelNumber];
			Percent_Property = new PropertyValue<string>[ChannelNumber];
			Light_Name_Property = new PropertyValue<string>[ChannelNumber];
			Lights_Toggle_Keys = new string[ChannelNumber];
			Lights_Slider_Keys = new string[ChannelNumber];
			Lights_Percent_Keys = new string[ChannelNumber];

			Light_Name_Property[0] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name1_Key, null, DevicePropertyType.String));
			Light_Name_Property[1] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name2_Key, null, DevicePropertyType.String));
			Light_Name_Property[2] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name3_Key, null, DevicePropertyType.String));
			Light_Name_Property[3] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name4_Key, null, DevicePropertyType.String));
			Light_Name_Property[4] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name5_Key, null, DevicePropertyType.String));
			Light_Name_Property[5] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name6_Key, null, DevicePropertyType.String));
			AllLightsIcon_Property = CreateProperty<string>(new PropertyDefinition(AllLightsIcon_Key, null, DevicePropertyType.String));
			

			for (int i = 0; i < ChannelNumber; i++)
            {
				Lights_Toggle_Keys[i] = "Light" + Convert.ToString(i + 1) + "value";
				Lights_Slider_Keys[i] = "Slider" + Convert.ToString(i + 1) + "value";
				Lights_Percent_Keys[i] = "Percent" + Convert.ToString(i + 1) + "value";
				Toggle_Property[i] = CreateProperty<bool>(new PropertyDefinition(Lights_Toggle_Keys[i], null, DevicePropertyType.Boolean));
				Slider_Property[i] = CreateProperty<int>(new PropertyDefinition(Lights_Slider_Keys[i], null, DevicePropertyType.Int32, 0, 100, 1));
				Percent_Property[i] = CreateProperty<string>(new PropertyDefinition(Lights_Percent_Keys[i], null, DevicePropertyType.String));
				Percent_Property[i].Value = "OFF";
				Slider_Property[i].Value = 0;
				Toggle_Property[i].Value = false;
				AllLightsIcon_Property.Value = "icLightsOffDisabled";
			}
			#region Debug Message
			Log("Device_Name - Create_Device_Definition - Finish");
			#endregion Debug Message

		}

		public void Update_UI(int[] togglestate)
		{
			#region Debug Message
			Log("Device_Name - Update_UI(Toggle) - Start");
			#endregion Debug Message

			int counter = 0; ;
			if(togglestate.Length != ChannelNumber)
            {
				return;
            }
            for (int i = 0; i < ChannelNumber; i++)
            {
				if (togglestate[i] != -1)
				{
					Toggle_Property[i].Value = Convert.ToBoolean(togglestate[i]);
				}

				if (Toggle_Property[i].Value == true)
				{
					counter++;
				}

                if (counter > 0)
                {
					AllLightsON = true;
					AllLightsIcon_Property.Value = "icLightsOnRegular";

				}
				else
                {
					AllLightsON = false;
					AllLightsIcon_Property.Value = "icLightsOffDisabled";
				}
			}
            Commit();

			#region Debug Message
			Log("Device_Name - Update_UI - Finish");
			#endregion Debug Message
		}

		public void Update_UI(int[] sliderstate, int[] secondarylabel)
        {
			#region Debug Message
			Log("Device_Name - Update_UI(Slider,Percent) - Start");
			#endregion Debug Message

			if (sliderstate.Length != ChannelNumber)
            {
				return;
            }
            for (int i = 0; i < ChannelNumber; i++)
            {
				if (sliderstate[i] != -1)
				{
					if (sliderstate[i] > 0)
					{
						Toggle_Property[i].Value = true;
						Slider_Property[i].Value = sliderstate[i];
						AllLightsIcon_Property.Value = "icLightsOnRegular";
						//Protocol.SliderLight(i + 1, sliderstate[i]);
						//Protocol.TurnOnLight(i + 1);
						//Percent_Property[i].Value = secondarylabel[i].ToString();
					}
					else
					{
						Toggle_Property[i].Value = false;
						Slider_Property[i].Value = 0;
						//Protocol.SliderLight(i + 1, 0);
						//Protocol.TurnOffLight(i + 1);
						//Percent_Property[i].Value = "OFF";
					}

					Slider_Property[i].Value = sliderstate[i];
				}
				if (secondarylabel[i] != -1)
				{
					if (secondarylabel[i] == 0)
					{
						Percent_Property[i].Value = "OFF";
					}
					else
					{
						Percent_Property[i].Value = Convert.ToString(secondarylabel[i]) + "%";
						AllLightsIcon_Property.Value = "icLightsOnRegular";
					}
				}
			}
			Commit();
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
			//icLightsOffDisabled
			//icLightsOnRegular
			//icSpinner

			//The tile of the Device was pressed
			if (command == "AllOn")
			{
				AllLightsIcon_Property.Value = "icSpinner";
				Commit();
				if (AllLightsON == true)
                {
					AllLightsOff();
					AllLightsIcon_Property.Value = "icLightsOffDisabled";
				}
                else
                {
					AllLightsOn();
					AllLightsIcon_Property.Value = "icLightsOnRegular";
				}
				Commit();
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
					if (state != null)
					{
						if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
							//Protocol.SliderLight(i + 1, 100);
							Slider_Property[i].Value = 100;
							Percent_Property[i].Value = "100%";
							Toggle_Property[i].Value = true;
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
							//Protocol.SliderLight(i + 1, 0);
							Slider_Property[i].Value = 0;
							Percent_Property[i].Value = "OFF";
							Toggle_Property[i].Value = false;
						}
						//Protocol.ToggleLight(i + 1);
						//Protocol.SliderLight(i + 1, (int)sliderstate);
					}
					//Save the state of the toggle
					//Toggle_Property[i].Value = (bool)state;
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Slider_Keys[i])
				{
					Log("Slider pressed");
					var sliderstate = value as int?;
					Log("sliderstate is: " + sliderstate);
					//var togglestate = value as bool?;
					if (sliderstate != null)
					{
						Log("Sliderstate is not null");
						//Slider_Property[i].Value = (int)sliderstate;

						if (sliderstate > 0)
						{
							//Protocol.TurnOnLight(i + 1);
							Protocol.SliderLight(i + 1, (int)sliderstate);
							Toggle_Property[i].Value = true;
							Percent_Property[i].Value = Convert.ToString(sliderstate) + "%";
							Log("Changed slider value to:" + (int)sliderstate);
						}
						else
						{
							//Protocol.TurnOffLight(i + 1);
							Protocol.SliderLight(i + 1, 0);
							Toggle_Property[i].Value = false;
							Percent_Property[i].Value = "OFF";
						}

					}
					Slider_Property[i].Value = (int)sliderstate;

					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Percent_Keys[i])
				{
					var state = value as string;
					Percent_Property[i].Value = state + "%";
					Commit();

					return new OperationResult(OperationResultCode.Success);
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
					if (state != null)
					{
						if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
							//Protocol.SliderLight(i + 1, 100);
							Slider_Property[i].Value = 100;
							Percent_Property[i].Value = "100%";
							Toggle_Property[i].Value = true;
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
							//Protocol.SliderLight(i + 1, 0);
							Slider_Property[i].Value = 0;
							Percent_Property[i].Value = "OFF";
							Toggle_Property[i].Value = false;
						}
						//Protocol.ToggleLight(i + 1);
						//Protocol.SliderLight(i + 1, (int)sliderstate);
					}
					//Save the state of the toggle
					//Toggle_Property[i].Value = (bool)state;
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Slider_Keys[i])
				{
					Log("Slider pressed");
					var sliderstate = value as int?;
					Log("sliderstate is: " + sliderstate);
					//var togglestate = value as bool?;
					if (sliderstate != null)
					{

						//Slider_Property[i].Value = (int)sliderstate;

						if (sliderstate > 0)
						{
							//Protocol.TurnOnLight(i + 1);
							Protocol.SliderLight(i + 1, (int)sliderstate);
							Toggle_Property[i].Value = true;
							Percent_Property[i].Value = Convert.ToString(sliderstate) + "%";
							Log("Changed slider value to:" + (int)sliderstate);
						}
						else
						{
							//Protocol.TurnOffLight(i + 1);
							Protocol.SliderLight(i + 1, (int)sliderstate);
							Toggle_Property[i].Value = false;
							Percent_Property[i].Value = "OFF";
						}

					}
					Slider_Property[i].Value = (int)sliderstate;

					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				else if (propertyKey == Lights_Percent_Keys[i])
				{
					var state = value as string;
					Percent_Property[i].Value = state + "%";
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
