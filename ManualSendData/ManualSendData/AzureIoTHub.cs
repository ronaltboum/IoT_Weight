using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "myPi". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=TAUIoT.azure-devices.net;DeviceId=myPi;SharedAccessKey=C78kTFWwXM9o9avA0S37XroWYOvi/cbJ0SASJ76Al4k=";

    //
    // To monitor messages sent to device "myPi" use iothub-explorer as follows:
    //    iothub-explorer monitor-events --login HostName=TAUIoT.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=ZXzEIObhgwdLY8wHfSfV83t+HHFc4Y14oAMUQ01Oahk= "myPi"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync(string msg)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

#if WINDOWS_UWP
        var str = msg;
#else
        var str = msg";
#endif
        var message = new Message(Encoding.ASCII.GetBytes(str));

        await deviceClient.SendEventAsync(message);
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
