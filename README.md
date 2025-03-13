# Sliding Tiles Puzzle

A simple sliding tiles puzzle game built with .NET MAUI targeting .NET 9.

## Prerequisites

- .NET 9 SDK or later
- Visual Studio Code with the following extensions:
  - C# Dev Kit
  - .NET MAUI (Preview)

## Building in Visual Studio Code

1. Open the project folder in VS Code
2. Make sure you have the required extensions installed
3. Open the Command Palette (Ctrl+Shift+P or Cmd+Shift+P on macOS)
4. Select ".NET: Build" to build the project
5. Or select ".NET: Run" to build and run the project

## Running the Project

You can run the project in several ways:

1. From the VS Code Command Palette:
   - Run ".NET: Run" command

2. Using the .NET CLI in the VS Code terminal:
   ```bash
   # Navigate to the project directory (if not already there)
   cd c:\Users\mattge\Source\Repos\SlidingTiles
   
   # Build the solution
   dotnet build
   
   # Run the project
   dotnet run --project ./SlidingTiles/SlidingTiles.csproj
   
   # Or specify a target framework for a specific platform
   dotnet run --project ./SlidingTiles/SlidingTiles.csproj -f net9.0-windows10.0.19041.0
   dotnet run --project ./SlidingTiles/SlidingTiles.csproj -f net9.0-android
   dotnet run --project ./SlidingTiles/SlidingTiles.csproj -f net9.0-ios
   dotnet run --project ./SlidingTiles/SlidingTiles.csproj -f net9.0-maccatalyst
   ```

3. Debug the application:
   - Open the "Run and Debug" sidebar (Ctrl+Shift+D)
   - Select a launch configuration from the dropdown
   - Click the Start Debugging button (green play icon) or press F5

## Project Structure

A typical .NET MAUI project structure:
```
SlidingTiles/                  <- Solution directory
├── SlidingTiles.sln           <- Solution file
├── SlidingTiles/              <- Project directory
│   ├── SlidingTiles.csproj    <- Project file
│   ├── MainPage.xaml          <- XAML files 
│   ├── MainPage.xaml.cs       <- Code-behind files
│   └── Models/                <- Models directory
│       ├── Tile.cs
│       └── GameBoard.cs
└── README.md
```

## Troubleshooting

- If you don't see .NET MAUI targets, ensure you've installed the .NET MAUI workload:
  ```bash
  dotnet workload install maui
  ```
- For Android debugging, make sure you have Android SDK installed and configured
- For iOS/macOS debugging, you'll need a Mac with the relevant SDKs installed
- If running from the command line fails, try locating and specifying your project file path explicitly

## Common Issues and Solutions

- **Error about SupportedOSPlatformVersion**: Make sure the Windows version in the project file matches your available Windows SDK. For Windows 11 or newer Windows 10 installations, the 10.0.19041.0 version should work.

- **Missing workloads**: If you see errors about missing workloads, install them with:
  ```bash
  dotnet workload install maui
  dotnet workload install android ios maccatalyst
  ```

- **Windows-specific issues**: If targeting Windows, make sure Windows App SDK is installed:
  ```bash
  dotnet workload install wasm-tools wasm-experimental
  ```

# First Time Setup

If this is your first time with the project, you may need to:

1. Install required workloads:
   ```bash
   dotnet workload install maui
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
