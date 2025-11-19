# Solunar Calculator with Weather and Tide Modifiers

## Overview

This enhanced version of the Solunar Calculator integrates **Weather** and **Tide** data as modifiers to the base solunar activity scores, providing more accurate fishing and hunting activity predictions.

## Key Features

### 1. **Base Solunar Theory (Primary Signal)**
- Major Times: ±1 hour windows around lunar transits (upper/lower)
- Minor Times: 1-hour windows around moonrise/moonset
- Moon Phase multipliers
- Solar time-of-day adjustments

### 2. **Weather Modifiers**
Weather conditions act as activity enhancers or suppressors:

| Factor | Modifier Range | Importance |
|--------|----------------|------------|
| Water Temperature | -15 to +15 | **Critical** (weight: 0.9) |
| Atmospheric Pressure | -15 to +15 | **Very High** (weight: 0.8) |
| Wind Speed/Direction | -10 to +10 | **High** (weight: 0.7) |
| Waves/Swell/Current | -10 to +10 | **Moderate-High** (weight: 0.6) |
| Cloud Cover | -5 to +10 | **Moderate** (weight: 0.5) |
| Air Temperature | -5 to +5 | **Low-Moderate** (weight: 0.4) |
| Humidity | -3 to +3 | **Low** (weight: 0.2) |

### 3. **Tide Modifiers**
Tidal movement is the **second strongest influence** after solunar periods:

| Factor | Modifier Range | Importance |
|--------|----------------|------------|
| Tidal Movement | -10 to +10 | **Critical** (weight: 1.0) |
| Tide Level | -10 to +10 | **High** (weight: 0.8) |

**Combined Tide Modifier: -20 to +20**

#### Tidal Activity Principles:
- **Best**: 1-2 hours before high tide + strong incoming current
- **Good**: Early incoming tide, moderate current
- **Fair**: High tide period, early outgoing
- **Poor**: Low tide, slack tide (no movement)

## Implementation

### New Components

#### Models
1. **WeatherData.cs** - Hourly weather observations
2. **TideData.cs** - High/low tide events
3. **ModifierWeights.cs** - Configurable weighting system
4. **ActivityBreakdown.cs** - Detailed score component breakdown (in SolunarResult.cs)

#### Services
1. **WeatherModifierCalculator.cs** - Calculates weather-based activity modifiers
2. **TideModifierCalculator.cs** - Calculates tide-based activity modifiers

#### Updated Components
1. **SolunarInput.cs** - Extended to accept Weather, Tide, and Weights
2. **SolunarResult.cs** - Extended with breakdown and modifier flags
3. **SolunarCalculator.cs** - Integrated modifier calculations
4. **Program.cs** - Loads JSON data files automatically

### Configuration

Create a `Weights.json` file in `_ReferenceFiles-Solunar/` to customize factor weights:

```json
{
  "solunar": {
    "major": 1.0,
    "minor": 0.6,
    "moonPhase": 0.3
  },
  "weather": {
    "waterTemperature": 0.9,
    "pressure": 0.8,
    "wind": 0.7,
    "cloudCover": 0.5,
    "waves": 0.6,
    "airTemperature": 0.4,
    "humidity": 0.2
  },
  "tide": {
    "level": 0.8,
    "movement": 1.0
  }
}
```

## Usage

### Command Line

```powershell
dotnet run -- <latitude> <longitude> <date> [timezone]
```

Example:
```powershell
dotnet run -- 37.9847291 23.4271869 2025-10-24 "Europe/Athens"
```

### Data Files

Place these JSON files in `_ReferenceFiles-Solunar/`:

1. **Weather.json** - Hourly weather observations
   ```json
   {
     "Weather": [
       {
         "Id": 26,
         "Timestamp": "2025-10-24T00:00:00",
         "AirTemperature": 20.0,
         "WaterTemperature": 22.95,
         "CloudCover": 100.0,
         "WindSpeed": 1.01,
         "Pressure": 1011.04,
         ...
       }
     ]
   }
   ```

2. **Tide.json** - High/low tide events
   ```json
   {
     "Tide": [
       {
         "Id": 5,
         "Timestamp": "2025-10-24T04:14:00",
         "Height": 0.0499,
         "TideType": "high",
         ...
       }
     ]
   }
   ```

3. **Weights.json** (optional) - Custom modifier weights

### Output

The application generates:

1. **Console Output**: Full JSON result with activity breakdown
2. **File Output**: Saved to `Output/solunar_{lat}_{lon}_{date}.json`

#### Output Structure

```json
{
  "date": "2025-10-24",
  "location": { "latitude": 37.98, "longitude": 23.43 },
  "majorTimes": [...],
  "minorTimes": [...],
  "hourlyActivity": [
    { "hour": 0, "score": 27 },
    { "hour": 15, "score": 98 },
    ...
  ],
  "activityBreakdown": [
    {
      "hour": 15,
      "solunarScore": 75.48,
      "weatherModifier": 22.42,
      "tideModifier": -0.06,
      "totalScore": 98
    },
    ...
  ],
  "hasWeatherModifiers": true,
  "hasTideModifiers": true
}
```

## Scoring Algorithm

### Final Score Calculation

```
TotalScore = SolunarBaseScore + WeatherModifier + TideModifier
```

Where:
- **SolunarBaseScore**: 0-100 (from major/minor periods, moon phase, time of day)
- **WeatherModifier**: -50 to +50 (weighted sum of weather factors)
- **TideModifier**: -20 to +20 (tide level + movement)
- **TotalScore**: Clamped to 0-100

### Example Hour Breakdown

For hour 15 (3:00 PM):
- Solunar Score: 75.48 (major period at 3:15 PM)
- Weather Modifier: +22.42 (good conditions)
- Tide Modifier: -0.06 (neutral tide)
- **Total: 98/100** ✅ Excellent

For hour 20 (8:00 PM):
- Solunar Score: 65.16 (minor period at 8:25 PM)
- Weather Modifier: +31.52 (excellent weather)
- Tide Modifier: +10.09 (strong incoming tide!)
- **Total: 100/100** ✅ Perfect conditions!

## Backward Compatibility

The system is fully backward compatible:
- Without Weather/Tide data: works as original solunar calculator
- With partial data: applies available modifiers only
- Default weights: automatically used if Weights.json not provided

## Testing

Tested with sample data for:
- Location: 37.9847291°N, 23.4271869°E (Athens, Greece area)
- Date: October 24, 2025
- 24 hours of weather observations
- 4 tide events

Results show:
- Peak activity at hour 20 (score: 100) - minor period + excellent weather + incoming tide
- Secondary peak at hour 15 (score: 98) - major period + good weather
- Lowest activity at hours 6-7 (score: 21) - no solunar activity + neutral conditions

## Technical Details

### Weather Scoring Examples

**Water Temperature** (most critical):
- 18-24°C: +15 (optimal)
- 15-18°C or 24-27°C: +5 to +15 (good)
- <12°C or >30°C: -5 to -15 (poor)

**Pressure** (very important):
- Rising pressure (>2mb/hr): +15 (excellent)
- Stable high (1013-1023mb): +5 to +10 (good)
- Falling rapidly (>2mb/hr): -15 (poor, storm approaching)

**Wind**:
- Moderate (2-5 m/s): +10 (ideal - oxygenates water)
- Calm (<2 m/s): +3 (neutral)
- Strong (>10 m/s): -10 (difficult conditions)

### Tide Scoring Examples

**Tidal Movement** (strongest at mid-tide):
- Strong incoming current (mid-tide): +10
- Moderate movement: +5 to +8
- Slack tide (high/low): -10

**Tide Level**:
- 1-2 hours before high tide: +10 (peak feeding)
- At high tide: +8
- Outgoing tide: +5 to -5 (decreasing)
- At low tide: -5

## Future Enhancements

Potential additions:
- Barometric pressure trend analysis (3-hour windows)
- Water clarity/turbidity factors
- Seasonal temperature adjustments per species
- Location-specific tide coefficients
- Moon distance (perigee/apogee) modifiers

---

**Version**: 2.0 (with Weather & Tide Modifiers)  
**Date**: November 2025  
**Based on**: John Alden Knight's Solunar Theory + Modern Weather/Oceanographic Data
