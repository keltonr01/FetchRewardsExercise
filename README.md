# About
This project is part of my job application to FetchRewards. It is a ASP.NET 5 API web app.

## How To Run
Included in the source is the all the files needed to run the project on Windows.
1. Clone/Download the repo.
2. Navigate to `FetchRewardsExercise/bin/Debug/net5.0/win-x86/`
3. From within that folder, open a command prompt and enter `FetchRewardsExercise.exe`
4. The application will be running on `localhost:5000` for http and `localhost:5001` for https. 

## Build From Source
Building from source will allow you to target non-windows platforms.
To build from the included source, you will need to have .NET installed on your computer. To download, please see dotnet.microsoft.com/download/dotnet/5.0.

Once installed, please see [this article](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build) on how to build using `dotnet build`.

## Swagger Documentation
This project also includes Swagger UI API documentation. Once the project is running, navigate to `http://localhost:5000/swagger/index.html` in your web browser.