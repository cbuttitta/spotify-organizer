🎵 Spotify Organizer

A tool to categorize, analyze, and organize Spotify tracks using genre data and your own workflows.

📘 Overview

Spotify Organizer is a C#/.NET application that connects to the Spotify Web API to fetch track, artist, and genre information. It provides utility classes and workflows to:

Retrieve Spotify data using authenticated API calls

Determine track genres based on associated artists

Cache artist genre data to reduce API calls

Run custom workflows across playlists, albums, or track groups

Organize musical content into structured output

This project is structured around clean separation of concerns, strong typing, and extensible service classes.

🚀 Features
🔹 API Layer

SpotifyApiClient handles all authenticated requests

Strong models for Spotify tracks, artists, and metadata

Environment configuration loader (Client ID, Secret, Scopes, Redirect URI)

🔹 Genre Classification

GenreService fetches and caches artist genres

Genre caching reduces redundant Spotify API calls

Defaults to misc when genre cannot be found

🔹 Workflow System

Modular workflow runner

Supports custom processing logic

Extensible design for different output types

🔹 Configuration

Loads environment variables from api.env

Validates presence of required credentials

Easy to extend with additional config fields


🔧 Setup & Installation
1. Clone the repository
git clone https://github.com/yourusername/SpotifyOrganizer.git
cd SpotifyOrganizer

2. Install dependencies

Ensure you have:

.NET 8.0 or later

A Spotify Developer account

A Spotify application (Client ID + Secret)

3. Create an api.env file
CLIENT_ID=yourClientId
CLIENT_SECRET=yourSecret
REDIRECT_URI=http://localhost:5000/callback
SCOPE=user-library-read playlist-read-private

4. Run the project
dotnet run

🔑 Authentication Flow

Spotify Organizer uses Spotify's OAuth 2.0 authorization code flow.

Flow:

Generate auth URL

Log in via Spotify

Spotify redirects to your callback URL

OAuth code is exchanged for an access token

All API requests use this token

Tokens refresh automatically as needed.

🎼 How Genre Lookup Works

For each track:

Iterate through all its artists

For each artist:

Check in-memory cache for genre

If missing, fetch artist info from Spotify

Extract first available genre

Store in cache

Return genre or "misc"

Caching ensures minimal API calls during batch workflows.

🧪 Extending the Project

You can easily add:

New organizational workflows

Filtering systems (by decade, popularity, energy, danceability, genre groups)

Batch playlist exporters

File outputs (CSV, JSON, Markdown, etc.)

Genre normalization mappings