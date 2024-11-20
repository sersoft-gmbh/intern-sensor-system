using SensorPi.Accessories.Buttons;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using SensorPi.Controllers;

namespace SensorPi.Accessories;

public sealed class IrRemote(string name, LocationsController locationsController, NightModeController nightModeController) : IDisposable
{
    private const string IrSocketName = "/var/run/lirc/lircd";

    private CancellationTokenSource _cancellationSource = new();
    private Task? _readingTask;

    ~IrRemote() => Dispose(false);

    public void Dispose() 
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
            _cancellationSource.Dispose();
    }

    private async Task HandleIrKey(string remoteName, string keyCommand) 
    {
        if (remoteName != name) return;
        switch (keyCommand) {
            case "KEY_A": break;
            case "KEY_B": break;
            case "KEY_C": break;
            case "KEY_X": 
                await nightModeController.ToggleNightMode();
                break; 
            case "KEY_UP":
                await locationsController.SwitchCurrentLocationBy(1);
                break;
            case "KEY_DOWN": 
                await locationsController.SwitchCurrentLocationBy(-1);
                break;
            case "KEY_RIGHT":
                await locationsController.SwitchCurrentLocationBy(1);
                break;
            case "KEY_LEFT": 
                await locationsController.SwitchCurrentLocationBy(-1);
                break;
            case "KEY_0": break;
            case "KEY_1":
                await locationsController.SwitchCurrentLocationTo(0);
                break;
            case "KEY_2":
                await locationsController.SwitchCurrentLocationTo(1);
                break;
            case "KEY_3": 
                await locationsController.SwitchCurrentLocationTo(2);
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

    public async Task StartReceivingSignals()
    {
        if (_readingTask != null) await _cancellationSource.CancelAsync();
        _cancellationSource = new CancellationTokenSource();
        _readingTask = Task.Run(async () => {
            while (!_cancellationSource.IsCancellationRequested)
                await CheckSignalAsync(_cancellationSource.Token);
        }, _cancellationSource.Token);
    }

    public async Task StopReceivingSignals() 
    {
        await _cancellationSource.CancelAsync();
        if (_readingTask != null) {
            await _readingTask;
            _readingTask = null;
        }
    }
}