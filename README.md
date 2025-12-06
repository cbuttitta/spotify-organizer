# SpotifyOrganizer

SpotifyOrganizer is a C#/.NET console application that connects to the Spotify Web API and retrieves data about the authenticated user — including their profile, playlists, tracks, and more. The data is organized into strongly typed classes and can be used for reporting, exporting, analytics, or future automation features.

The project uses `System.Text.Json` for deserialization and `HttpClient` for communicating with Spotify’s REST API.

---

## Features

- 🔐 Authenticate with Spotify using an OAuth token  
- 👤 Retrieve detailed Spotify user profile information  
- 🎵 Fetch playlists, tracks, artists, and metadata  
- 📦 Organize Spotify API responses into clean C# models  
- 🛠 Demonstrates API consumption with `HttpClient` and async/await  
- 🧩 Easily extensible for playlist sorting, exporting, or bulk management

---

## Requirements

- .NET 6.0 or later  
- A Spotify Developer account  
- A Spotify OAuth **Access Token** with the required scopes  
- Internet connection for API calls

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/yourusername/SpotifyOrganizer.git
cd SpotifyOrganizer

Open the Program.cs file and locate the envConfig function

var env = EnvironmentConfig.Load("api.env");

and replace it with the location of your SpotifyAPI token

Running the Project

From the project folder (where the .csproj file is located):

dotnet build
dotnet run


The application will:

Send a request to Spotify’s API

Deserialize the JSON response into your model classes

Print the organized user data to the console

Project Structure
SpotifyOrganizer/
│
├── Models/
│   ├── ProfileObject.cs
│   ├── Followers.cs
│   ├── ImageObject.cs
│   ├── ExternalUrls.cs
│   └── (other Spotify response types)
│
├── Program.cs
├── SpotifyOrganizer.csproj
└── README.md

How It Works

The program sends GET requests to endpoints like:

https://api.spotify.com/v1/me


It passes your OAuth access token in the Authorization header:

Authorization: Bearer <token>


Spotify returns data in JSON format.

The app deserializes JSON into strongly typed C# classes.

Data is printed or used for analysis/organization.

Future Enhancements (Ideas)

📂 Export playlists or profile data to JSON/CSV

🔄 Auto-refresh OAuth tokens

🧹 Sort or clean playlists automatically

⭐ Track listening stats

🗂 Build a UI dashboard for your library
