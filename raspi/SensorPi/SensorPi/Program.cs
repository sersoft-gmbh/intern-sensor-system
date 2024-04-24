// See https://aka.ms/new-console-template for more information

using System.Device.Gpio;
using SensorPi.Accessories;
using SensorPi.Accessories.Temperature;
using SensorPi.Controllers;
// using RaspberryIRDotNet.RX;
// using RaspberryIRDotNet.PacketFormats.NEC;

const string ServerAddress = "CHANGEME";
const string ServerToken = "CHANGEME";

// const int DhtPin = 18;
const int ButtonPin = 16;
const int LedRPin = 17;
const int LedGPin = 22;
const int LedBPin = 27;
const int I2CBus = 1;
const int DisplayAddress = 0x27;
const int Bme680Address = 0x77;

using var gpioController = new GpioController();

using var display = new WideDisplay(I2CBus, DisplayAddress);
using var statusLight = new StatusLight(LedRPin, LedGPin, LedBPin, gpioController, false);

// using var temperatureSensor = new SimpleTemperatureSensor(DhtPin, gpioController);
using var temperatureSensor = new AdvancedTemperatureSensor(I2CBus, Bme680Address);

var locationsController = new LocationsController(statusLight, display);

using var serverController = new ServerController(new Uri(ServerAddress), ServerToken);
using var sensorsController = new SensorsController<AdvancedTemperatureSensor>(temperatureSensor, statusLight, display);

using var locationButton = new LocationButton(ButtonPin, locationsController, gpioController, false);

// var receiverDevicePath = new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().GetPathToTheReceiverDevice();
// var receiver = new RaspberryIRDotNet.RX.PulseSpaceSource.PulseSpaceCaptureLirc(receiverDevicePath);
// receiver.ReceivedPulseSpaceBurst += OnRx;
// receiver.Capture(null);

// var rx = new PacketConsoleWriter<NecExtendedPacket>(new NecBinaryConverter(), receiverDevicePath)
// {
//     UnitDurationMicrosecs = 560,
// };
// rx.RXFilters.Filters.Add(new NecRxFilter());
// rx.Start(null);

try {
    sensorsController.StartReading(serverController, locationsController);
    await Task.Delay(Timeout.Infinite);
} finally {
    sensorsController.StopReading();
}

// static void OnRx(object? s, RaspberryIRDotNet.RX.PulseSpaceSource.ReceivedPulseSpaceBurstEventArgs args) {
//     // Console.WriteLine(s);
//     Console.WriteLine(args.Buffer);
// }
