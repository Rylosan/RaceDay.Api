# RaceDay API Endpoint Plan

## Overview

The RaceDay API supports two user roles:

- **Organiser** — creates and manages events, categories, routes, and participant results.
- **Participant** — browses events, enrols in categories, manages a profile, and views enrolments and results.

JWT authentication is used for protected endpoints, with role-based authorization controlling Organiser and Participant functionality.

## Authentication Endpoints

| Method | Endpoint | Role | Purpose |
|---|---|---|---|
| POST | `/api/Auth/register` | Public | Register a new Organiser or Participant |
| POST | `/api/Auth/login` | Public | Authenticate a user and return a JWT |

### Register Request

```json
{
  "firstName": "Race",
  "lastName": "Participant",
  "email": "participant@example.com",
  "password": "password123!",
  "role": "Participant"
}

//Login Request

{
  "email": "participant@example.com",
  "password": "password123!"
}

//Create event request

{
  "name": "Cape Town 10K",
  "description": "A 10 km road running event.",
  "eventDate": "2026-11-21T07:00:00",
  "location": "Cape Town",
  "distanceKm": 10,
  "eventType": "Running"
}

//Category Request

{
  "name": "Open 10K",
  "description": "Open category for the 10 km race."
}

//Enrollment Request

{
  "eventId": 1,
  "categoryId": 1
}

//Create Result Request

{
  "enrollmentId": 1,
  "finishTime": "00:51:45",
  "position": 1
}

//Route Request 

{
  "startPoint": "Cape Town Stadium",
  "finishPoint": "Cape Town Stadium",
  "routeDescription": "A 10 km road route through the Cape Town city area, returning to the stadium.",
  "elevationGainMetres": 85
}

//Security Plan
//- Passwords are stored as BCrypt password hashes.
//- Successful authentication returns a JWT.
//- Protected endpoints require JWT authentication.
//- Role-based authorization separates Organiser and Participant operations.
//- Organisers may modify only events that they own.
//- Participants may modify only their own enrolments.

