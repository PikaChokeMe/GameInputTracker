# GameInputTracker
An Asp.net core project combining the use of global system key hooks and SignalR to mirror the current keyboard state of common FPS keys to a webpage that can be used as a browser source in OBS Studio.


## InputTracker
This is the main IIS ASP.Net core module for the web pages which will run and OBS will load to display the keys

## CaptureInput
This the low level C++ input capture DLL which windows injects globally into running process to hook and capture key strokes and mouse clicks

## CaptureInputDotNet
This is a managed C# library which wraps the native DLL for ease of use in InputTracker and other C# applications

## Screenshots
![Overlay Sample](/OverlaySample.png)