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
		public bool AllLightsON = false;
		public TcpTransport Transport;
        public FutureNow_FNIP_Protocol Protocol;
        #endregion Declarations

        #region Property Keys
		//Used to recognise what is being pressed/changed on the UI
		//Must be bound to a UI property
        private string[] Lights_Toggle_Keys;
        private string[] Lights_Slider_Keys;
		private string[] Lights_Percent_Keys;
		private const string AllLightsIcon_Key = "AllLightsIcon";
		#endregion Property Keys

		#region UI properties
		//Used to trigger a change on the UI
		//Must be initialized
		public PropertyValue<bool>[] Toggle_Property;
        public PropertyValue<int>[] Slider_Property;
		public PropertyValue<string>[] Percent_Property;
		public PropertyValue<string>[] Light_Name_Property;
		public PropertyValue<string>[] Light_Ramp_Property;
		private PropertyValue<string> AllLightsIcon_Property;
		#endregion UI properties

		#region Delegates
		public delegate void UI_Update_Delegate();
        #endregion Delegates

        public FutureNow_FNIP()
        {
           
        }

		public void Initialize(IPAddress ipAddress, int port)
		{
			EnableLogging = true;

			#region Setup UI
			ChannelNumber = 6;

			//Create all neccessary properties and bindings
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

            #region Initialize Presets
			//Sets the preset dim levels to 100 for all channels
			//Preset dim level 0 means the last dim level will be used when next turned on
            for (int i = 0; i < ChannelNumber; i++)
            {
				Protocol.SetDIMPresets(i + 1);
            }
			#endregion Initialize Presets
		}

		private void Create_Device_Definition()
        {
			#region Init Arrays
			//Initialize the arrays for the properties and the keys
			Toggle_Property = new PropertyValue<bool>[ChannelNumber];
			Slider_Property = new PropertyValue<int>[ChannelNumber];
			Percent_Property = new PropertyValue<string>[ChannelNumber];
			Light_Name_Property = new PropertyValue<string>[ChannelNumber];
			Light_Ramp_Property = new PropertyValue<string>[ChannelNumber + 1];
			Lights_Toggle_Keys = new string[ChannelNumber];
			Lights_Slider_Keys = new string[ChannelNumber];
			Lights_Percent_Keys = new string[ChannelNumber];
			#endregion Init Arrays

			#region Init Properties
			//Create the properties and bind them to the corresponding key
			Light_Name_Property[0] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name1_Key, null, DevicePropertyType.String));
			Light_Name_Property[1] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name2_Key, null, DevicePropertyType.String));
			Light_Name_Property[2] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name3_Key, null, DevicePropertyType.String));
			Light_Name_Property[3] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name4_Key, null, DevicePropertyType.String));
			Light_Name_Property[4] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name5_Key, null, DevicePropertyType.String));
			Light_Name_Property[5] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Name6_Key, null, DevicePropertyType.String));
			AllLightsIcon_Property = CreateProperty<string>(new PropertyDefinition(AllLightsIcon_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[0] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp1_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[1] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp2_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[2] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp3_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[3] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp4_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[4] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp5_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[5] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp6_Key, null, DevicePropertyType.String));
			Light_Ramp_Property[6] = CreateProperty<string>(new PropertyDefinition(FutureNow_FNIP_Protocol.Lights_Ramp7_Key, null, DevicePropertyType.String));


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
			#endregion Init Properties
		}

        public void Update_UI(int[] state)
        {
			int counter = 0;

            #region segfault
            //Make sure we don't get a segfault
            if (state.Length != ChannelNumber)
            {
				return;
            }
			#endregion segfault

			for (int i = 0; i < ChannelNumber; i++)
            {
				#region Update All Channels
				//If value is -1, the message did not mention the channel i.
				//Thus we will not change it's value
				if (state[i] != -1)
				{
					//state is equal to the current dimming level of the Light
					if (state[i] > 0)
					{
						Toggle_Property[i].Value = true;
						Slider_Property[i].Value = state[i];
						Percent_Property[i].Value = Convert.ToString(state[i]) + "%";
						AllLightsIcon_Property.Value = "icLightsOnRegular";
					}
					else
					{
						Toggle_Property[i].Value = false;
						Slider_Property[i].Value = 0;
						Percent_Property[i].Value = "OFF";
					}

					Slider_Property[i].Value = state[i];
				}
			#endregion Update All Channels

				#region All Lights ON/OFF State
				//Keeps count of the state of the lights
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
				#endregion All Lights ON/OFF State
			}
			//Save the changes made to the UI
			Commit();
		}

        #region SetStatus Method
        //Since the "Connected" property is readonly for other classes,
        //we have to provide a method for them to be able to modify it
        public void SetStatus(bool status)
		{
			Connected = status;
		}
		#endregion SetStatus Method

		#region Events
		[ProgrammableOperation ("Lights On")]
		public void AllLightsOn()
        {
			//0 == the method applies to all indexes (lights)
			Protocol.TurnOnAllLight(0);
        }

		[ProgrammableOperation("Lights Off")]
		public void AllLightsOff()
		{
			//0 == the method applies to all indexes (lights)
			Protocol.TurnOffAllLight(0);
		}
        #endregion Events

        #region Overrides

        protected override IOperationResult DoCommand(string command, string[] parameters)
		{
            #region AllOn Command
            //The tile of the Device was pressed
            if (command == "AllOn")
			{
				//momentarily change the light icon to a loading icon
				AllLightsIcon_Property.Value = "icSpinner";
				Commit();

				//Check the state based on the counter in Update_UI
				if (AllLightsON == true)
                {
					//Turn off all lights, change icon, change the state of the UI, apply changes to the UI
					AllLightsOff();
					AllLightsIcon_Property.Value = "icLightsOffDisabled";
                    for (int i = 0; i < ChannelNumber; i++)
                    {
						Toggle_Property[i].Value = false;
                    }
					Commit();
				}
                else
                {
					//Turn on all lights, change icon, change the state of the UI, apply changes to the UI
					AllLightsOn();
					AllLightsIcon_Property.Value = "icLightsOnRegular";
					for (int i = 0; i < ChannelNumber; i++)
					{
						Toggle_Property[i].Value = true;
					}
					Commit();
				}
				Commit();
			}
			#endregion AllOn Command

			return new OperationResult(OperationResultCode.Success);
		}

        protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
		{
			//Search through all the property keys
			for (int i = 0; i < ChannelNumber; i++)
			{
                #region Toggle Property
                if (propertyKey == Lights_Toggle_Keys[i])
				{
					//Turn the Lights i on or off, depending on the state of the property
					//Save and Update the state of all affected elements
					var state = value as bool?;
					if (state != null)
					{
						if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
							Slider_Property[i].Value = 100;
							Percent_Property[i].Value = "100%";
							Toggle_Property[i].Value = true;
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
							Slider_Property[i].Value = 0;
							Percent_Property[i].Value = "OFF";
							Toggle_Property[i].Value = false;
						}
					}
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
                #endregion Toggle Property

                #region Slider Property
                else if (propertyKey == Lights_Slider_Keys[i])
				{
					Toggle_Property[i].Value = true;
					Commit();

					//Change the Dimming i, depending on the state of the property
					//Save and Update the state of all affected elements
					var sliderstate = value as int?;
					if (sliderstate != null)
					{
						if (sliderstate > 0)
						{
							Toggle_Property[i].Value = true;
							Protocol.SliderLight(i + 1, (int)sliderstate);
							Percent_Property[i].Value = Convert.ToString(sliderstate) + "%";
						}
						else
						{
							Toggle_Property[i].Value = false;
							Protocol.SliderLight(i + 1, 0);
							Percent_Property[i].Value = "OFF";
						}

					}
					//Save the state of the slider
					Slider_Property[i].Value = (int)sliderstate;
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
                #endregion Slider Property

                #region Percent Property
                else if (propertyKey == Lights_Percent_Keys[i])
				{
					var state = value as string;
					Percent_Property[i].Value = state + "%";
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
			}
			#endregion Percent Property

			return new OperationResult(OperationResultCode.Error);
		}

        protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
		{
			//Search through all the property keys
			for (int i = 0; i < ChannelNumber; i++)
			{
                #region Toggle Property
                if (propertyKey == Lights_Toggle_Keys[i])
				{
					//Turn the Lights i on or off, depending on the state of the property
					//Save and Update the state of all affected elements
					var state = value as bool?;
					if (state != null)
					{
						if (state == true)
						{
							Protocol.TurnOnLight(i + 1);
							Slider_Property[i].Value = 100;
							Percent_Property[i].Value = "100%";
							Toggle_Property[i].Value = true;
						}
						else
						{
							Protocol.TurnOffLight(i + 1);
							Slider_Property[i].Value = 0;
							Percent_Property[i].Value = "OFF";
							Toggle_Property[i].Value = false;
						}
					}
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				#endregion Toggle Property

				#region Slider Property
				else if (propertyKey == Lights_Slider_Keys[i])
				{
					Toggle_Property[i].Value = true;
					Commit();

					//Change the Dimming i, depending on the state of the property
					//Save and Update the state of all affected elements
					var sliderstate = value as int?;
					if (sliderstate != null)
					{
						if (sliderstate > 0)
						{
							Protocol.SliderLight(i + 1, (int)sliderstate);
							Toggle_Property[i].Value = true;
							Percent_Property[i].Value = Convert.ToString(sliderstate) + "%";
						}
						else
						{
							Protocol.SliderLight(i + 1, 0);
							Toggle_Property[i].Value = false;
							Percent_Property[i].Value = "OFF";
						}

					}
					//Save the state of the slider
					Slider_Property[i].Value = (int)sliderstate;
					//Save the changes made to the UI
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				#endregion Slider Property

				#region Percent Property
				else if (propertyKey == Lights_Percent_Keys[i])
				{
					var state = value as string;
					Percent_Property[i].Value = state + "%";
					Commit();

					return new OperationResult(OperationResultCode.Success);
				}
				#endregion Percent Property
			}
			return new OperationResult(OperationResultCode.Error);
		}

        #region Connect Method
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
        #endregion Connect Method

        #region Disconnect Method
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
		#endregion Disconnect Method
		#endregion Overrides
	}
}
