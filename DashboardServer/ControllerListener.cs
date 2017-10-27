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
        private Joystick controller = null;
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
            Thread findControllerThread = new Thread(new ThreadStart(SearchForController));
            findControllerThread.Start();
            
            // Poll events from joystick
            while (true)
            {
                if (controller == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (ButtonPressed != null)
                {
                    try
                    {
                        controller.Poll();
                        var datas = controller.GetBufferedData();

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
                    catch (Exception)
                    {
                        Console.WriteLine("Lost joystick. Start looking for a new one.");
                        controller = null;
                    }
                }
            }
        }

        private void SearchForController()
        {
            while (true)
            {
                var joystickGuid = Guid.Empty;

                // Search for wheels.
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Driving, DeviceEnumerationFlags.AttachedOnly))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                }

                // Use gamepad as fallback.
                if (joystickGuid == Guid.Empty)
                {
                    foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
                    {
                        joystickGuid = deviceInstance.InstanceGuid;
                    }
                }

                // If Joystick not found, throws an error
                if (joystickGuid != Guid.Empty && (controller == null || !controller.Information.InstanceGuid.Equals(joystickGuid)))
                {
                    if (controller != null)
                    {
                        controller.Unacquire();
                    }
                    
                    // Instantiate the joystick
                    controller = new Joystick(directInput, joystickGuid);

                    Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);
                    
                    // Set BufferSize in order to use buffered data.
                    controller.Properties.BufferSize = 128;

                    // Acquire the joystick
                    controller.Acquire();
                }

                while (controller != null)
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
