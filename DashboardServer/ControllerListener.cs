using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DashboardServer
{
    
    class ControllerListener
    {
        private DirectInput directInput = new DirectInput();
        private Joystick joystick = null;
        List<int> detectedButtonPresses = new List<int>();

        public delegate void ButtonPressedEventHandler(int button);

        public event ButtonPressedEventHandler ButtonPressed;
        
        internal void Listen()
        {
            Thread listener = new Thread(new ThreadStart(Run));
            listener.Start();
        }

        private void Run()
        {
            Thread findControllerThread = new Thread(new ThreadStart(SearchForGamepads));
            findControllerThread.Start();
            
            // Poll events from joystick
            while (true)
            {
                if (joystick == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (ButtonPressed != null)
                {
                    joystick.Poll();
                    var datas = joystick.GetBufferedData();

                    foreach (var state in datas)
                    {
                        // Only use buttons.
                        if (state.Value <= 0 || state.Offset < JoystickOffset.Buttons0 || state.Offset > JoystickOffset.Buttons127)
                        {
                            continue;
                        }
                        
                        ButtonPressed((int)state.Offset);
                    }

                    Thread.Sleep(25);
                }
            }
        }

        private void SearchForGamepads()
        {
            while (true)
            {
                var joystickGuid = Guid.Empty;

                // Search for wheels.
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Driving, DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                }

                // Use gamepad as fallback.
                if (joystickGuid == Guid.Empty)
                {
                    foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                    {
                        joystickGuid = deviceInstance.InstanceGuid;
                    }
                }

                // If Joystick not found, throws an error
                if (joystickGuid != Guid.Empty && (joystick == null || !joystick.Information.InstanceGuid.Equals(joystickGuid)))
                {
                    if (joystick != null)
                    {
                        joystick.Unacquire();
                    }
                    
                    // Instantiate the joystick
                    joystick = new Joystick(directInput, joystickGuid);

                    Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);
                    
                    // Set BufferSize in order to use buffered data.
                    joystick.Properties.BufferSize = 128;

                    // Acquire the joystick
                    joystick.Acquire();
                }

                Thread.Sleep(5000);
            }
        }
    }
}
