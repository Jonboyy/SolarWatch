# Solar Watch

   SolarWatch is an API that provides sunrise and sunset times for a given city on a specified date.

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Jonboyy/SolarWatch.git
   
2. Navigate to the directory:
   ```bash
   cd SolarWatch
   
3. Restore dependencies:
   ```bash
   dotnet restore
   
4. Configure Secrets (API keys):
   ```bash
   dotnet user-secrets set "OpenWeatherApiKey" "your-openweather-api-key"
   dotnet user-secrets set "TimeZoneDbApiKey" "your-timezonedb-api-key"

## Setup Docker

1. Make sure docker is installed.
   
2. Build the docker image:
   ```bash
   docker build -t solarwatch .
   ```
3. Run the container:
   ```bash
   docker run -d -p 5000:5000 --name solarwatch-container solarwatch
   ```
4. The API will be available at http://localhost:5000

## Usage

   - Get Sunrise/Sunset times: 
   ```
  GET /api/Sun?City={city}&Date={YYYY-MM-DD}&Timezone={UTC/local}
   ```
## Testing
   ``
  dotnet test
   ``
   
## Stopping & Removing the Container

1. Stopping the container:
```bash
  docker stop solarwatch-container
```
2. Removing the container: 
```bash
  docker rm solarwatch-container
```
![SolarWatch Screenshot](https://github.com/Jonboyy/SolarWatch/blob/main/SolarWatch/docs/Screenshot%202025-02-10%20at%2011.01.11%E2%80%AFAM.png?raw=true)



