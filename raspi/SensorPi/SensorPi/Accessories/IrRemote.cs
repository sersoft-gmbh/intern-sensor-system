using SensorPi.Accessories.Buttons;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using SensorPi.Controllers;

namespace SensorPi.Accessories;

public sealed class IrRemote(string remoteName, LocationsController locationsController, NightModeButton nightModeButton) : IDisposable 
{
    const string IrSocketName = "/var/run/lirc/lircd";

    private readonly string _remoteName = remoteName;
    private readonly LocationsController _locationsController = locationsController;
    private readonly NightModeButton _nightModeButton = nightModeButton;


    private CancellationTokenSource _cancellationSource = new();
    private Task? _readingTask;

    public void Dispose() 
    {
        _cancellationSource.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task HandleIrKey(string remoteName, string keyCommand) 
    {
        if (remoteName != _remoteName) return;
        switch (keyCommand) {
            case "KEY_A": break;
            case "KEY_B": break;
            case "KEY_C": break;
            case "KEY_X": 
                _nightModeButton.ToggleNightMode();
                break; 
            case "KEY_UP":
                await _locationsController.SwitchCurrentLocationBy(1);
                break;
            case "KEY_DOWN": 
                await _locationsController.SwitchCurrentLocationBy(-1);
                break;
            case "KEY_RIGHT":
                await _locationsController.SwitchCurrentLocationBy(1);
                break;
            case "KEY_LEFT": 
                await _locationsController.SwitchCurrentLocationBy(-1);
                break;
            case "KEY_0": break;
            case "KEY_1":
                await _locationsController.SwitchCurrentLocationTo(0);
                break;
            case "KEY_2":
                await _locationsController.SwitchCurrentLocationTo(1);
                break;
            case "KEY_3": 
                await _locationsController.SwitchCurrentLocationTo(2);
                break;
            case "KEY_4": break;
            case "KEY_5": break;
            case "KEY_6": break;
            case "KEY_7": break;
            case "KEY_8": break;
            case "KEY_9": break;
            default: 
                Console.WriteLine($"Unknown IR key: {keyCommand}"); 
                break;
        }
    }

    private async Task CheckSignalAsync(CancellationToken cancellationToken)
    {
        using var socket  = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        var endpoint = new UnixDomainSocketEndPoint(IrSocketName);
        await socket.ConnectAsync(endpoint, cancellationToken);

        var buffer = new byte[500];
        while (socket.Connected && !cancellationToken.IsCancellationRequested) {
            var size = await socket.ReceiveAsync(buffer, cancellationToken);
            // Weird way to detect whether the socket has been closed / terminated outside of our control.
            if (size == 0 && socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0)
                socket.Close();
            var command = Encoding.ASCII.GetString(buffer, 0, size);
            var commandParts = command.Split(' ');
            if (commandParts.Length < 4) continue;
            await HandleIrKey(commandParts[3].Trim(), commandParts[2].Trim());
        }
        if (socket.Connected)
            socket.Close();
        else
            Debug.WriteLine("Socket seems to have closed...");
    }

    public void StartReceivingSignals() 
    {
        if (_readingTask != null) _cancellationSource.Cancel();
        _cancellationSource = new();
        _readingTask = Task.Run(async () => {
            while (!_cancellationSource.IsCancellationRequested)
                await CheckSignalAsync(_cancellationSource.Token);
        }, _cancellationSource.Token);
    }

    public async Task StopReceivingSignals() 
    {
        _cancellationSource.Cancel();
        if (_readingTask != null) {
            await _readingTask;
            _readingTask = null;
        }
    }
}