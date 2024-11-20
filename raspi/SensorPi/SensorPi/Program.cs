// See https://aka.ms/new-console-template for more information

using System.Device.Gpio;
using SensorPi.Accessories;
using SensorPi.Accessories.Buttons;
using SensorPi.Accessories.Temperature;
using SensorPi.Controllers;

const string ServerAddress = "CHANGEME";
const string ServerToken = "CHANGEME";

const int LocationButtonPin = 16;
const int DarkModeButtonPin = 12;
const int LedRPin = 17;
const int LedGPin = 22;
const int LedBPin = 27;
const int I2CBus = 1;
const int DisplayAddress = 0x27;
const int BmeAddress = 0x77;

const string IrRemoteName = "funduino";

using var gpioController = new GpioController();

using var display = new WideDisplay(I2CBus, DisplayAddress);
using var statusLight = new StatusLight(LedRPin, LedGPin, LedBPin, gpioController, false);

using var temperatureSensor = new AdvancedTemperatureSensor(I2CBus, BmeAddress);

var locationsController = new LocationsController(display);
var nightModeController = new NightModeController(display, statusLight);

using var serverController = new ServerController(new Uri(ServerAddress), ServerToken);
using var sensorsController = new SensorsController<AdvancedTemperatureSensor>(temperatureSensor, statusLight, display);

using var nightModeButton = new NightModeButton(DarkModeButtonPin, nightModeController, gpioController, false);
using var locationButton = new LocationButton(LocationButtonPin, locationsController, gpioController, false);
using var irRemote = new IrRemote(IrRemoteName, locationsController, nightModeController);

try {
    sensorsController.StartReading(serverController, locationsController);
    await irRemote.StartReceivingSignals();
    await Task.Delay(Timeout.Infinite);
} finally {
    sensorsController.StopReading();
    await irRemote.StopReceivingSignals();
}
