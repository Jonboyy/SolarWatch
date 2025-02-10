#Solar Watch

   SolarWatch is an API that provides sunrise and sunset times for a given city on a specified date.

##Installation

1. Clone the repository:
   
   git clone https://github.com/Jonboyy/SolarWatch.git

2. Navigate to the directory:
   ``
   cd SolarWatch
   ``
3. Restore dependencies:
   ``
   dotnet restore
   ``
4. Configure Secrets (API keys):
   ``
   dotnet user-secrets set "OpenWeatherApiKey" "your-openweather-api-key"
   ``
   ``
   dotnet user-secrets set "TimeZoneDbApiKey" "your-timezonedb-api-key"
   ``
6. Setup Docker

   - Make sure docker is installed
   - Build the docker image:
   ``
   docker build -t solarwatch .
   ``
   - Run the container:
   ``
   docker run -d -p 5000:5000 --name solarwatch-container solarwatch
   ``
7. The API will be available at http://localhost:5000

##Usage

   - Get Sunrise/Sunset times: 

  GET /api/Sun?City={city}&Date={YYYY-MM-DD}&Timezone={UTC/local}

##Testing
   ``
  dotnet test
   ``
##Stopping & Removing the Container
   ``
  docker stop solarwatch-container
   ``
   ``
  docker rm solarwatch-container
  ``

