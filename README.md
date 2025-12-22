# DeskBookingService
## Features

### Core Functionality
- **Desk Reservation**: Book individual desks or conference rooms for specific dates
- **Multi-Building Support**: Manage multiple office locations with different operating hours
- **Floor Plan Visualization**: Interactive floor plan showing desk layouts and availability

- **Multi-day Bookings**: Reserve desks for consecutive or non-consecutive days as a group

- **Profile Management**: View and manage personal reservations

### Administrative Features
- **Database Management**: Admin interface for CRUD operations on all entities
- **Building Management**: Configure buildings, and desk layouts
- **Maintenance Tracking**: Mark desks as under maintenance with reasons

## Architecture
### Backend (C# ASP.NET Core)
- ASP.NET Core 8.0
- Entity Framework Core (In Memory)
- AutoMapper (Mapster) for DTO mapping
- FluentValidation for input validation
- xUnit for testing

### Frontend (React TypeScript)
- React 19
- TypeScript
- Bootstrap 5
- Axios for HTTP requests
- React Router for navigation

## Database Schema

### Core Entities
- **Buildings**: Office locations with floor plans and operating hours
- **Desks**: Individual workspaces with types (Regular, Conference Room)
- **Users**: System users with roles (Admin/User)
- **Reservations**: Bookings with status tracking and group support
- **OperatingHours**: Building-specific closed days (informational openning and closing times not implemented yet)

### Key Relationships
- Building → Desks (one-to-many)
- Building → OperatingHours (one-to-many)
- Desk → Reservations (one-to-many)
- User → Reservations (one-to-many)


## Getting Started

### Prerequisites
- **.NET 1.0 SDK**
- **Node.js 18+**
- **Yarn** (recommended) or npm

### Backend Setup

1. **Navigate to Backend Directory**
   ```bash
   cd BackendServer
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Navigate to Frontend Directory**
   ```bash
   cd frontendserver
   ```

2. **Install Dependencies**
   ```bash
   yarn install
   # or
   npm install
   ```

3. **Start Development Server**
   ```bash
   yarn start
   # or
   npm start
   ```
   The app will open at `http://localhost:3000`.

### Environment Configuration
- Copy `.env.example` files to `.env` in both backend and frontend directories
- Configure database connection strings and API URLs as needed

#### Backend (.env) Setup

1. **Navigate to Backend Directory**
   ```bash
   cd BackendServer
   ```

2. **Copy Environment File**
   ```bash
   cp .env.example .env
   ```

3. **Configure Backend Environment Variables**
   ```env
   # Server Configuration
   BACKEND_PORT=5000
   FRONTEND_PORT=3000

   # URLs
   ASPNETCORE_URLS=http://localhost:5000
   FRONTEND_URL=http://localhost:3000
   ```

   **Available Variables:**
   - `BACKEND_PORT`: Port for the ASP.NET Core API (default: 5000)
   - `FRONTEND_PORT`: Port where the React frontend runs (default: 3000)
   - `ASPNETCORE_URLS`: Full URL for the backend server
   - `FRONTEND_URL`: URL of the frontend application

#### Frontend (.env) Setup

1. **Navigate to Frontend Directory**
   ```bash
   cd frontendserver
   ```

2. **Copy Environment File**
   ```bash
   cp .env.example .env
   ```

3. **Configure Frontend Environment Variables**
   ```env
   # Server Configuration
   BACKEND_PORT=5000
   FRONTEND_PORT=3000

   # Frontend Development Server
   PORT=3000

   # Backend API Configuration
   REACT_APP_API_BASE_URL=http://localhost:5000/api/
   ```

   **Available Variables:**
   - `PORT`: Port for the React development server (default: 3000)
   - `REACT_APP_API_BASE_URL`: Base URL for API calls to the backend
     - Use `http://localhost:5000/api/` for local development
     - Use your machine's IP (e.g., `http://192.168.1.100:5000/api/`) for testing via mobile phone (or other machine)

### Database Seeding

The application automatically seeds sample data on startup, including:
- 2 buildings with different operating hours
- 20+ desks with various types and positions
- 8 users with different roles
- User data for testing:
  - `sarah@company.com` with password `sarah123` (Admin role)
  - `michael@company.com` with password `michael123` (User role)
- Sample reservations (past, present, and future)

## Usage

### For Regular Users
1. **Register/Login**: Create an account or sign in
2. **Browse Buildings**: Select your preferred office location
3. **Choose Dates**: Pick dates for your booking
4. **Select Desk**: Use the floor plan or list view to choose a desk
5. **Book**: Confirm your reservation
6. **Manage**: View active and past reservations in your profile

### For Administrators
- Access the Admin page from the navigation menu
- View and edit all database entities

## Development
### Running Tests

Multiple unit tests for backend controllers and reservation validation logic

```bash
# Backend tests
cd BackendServer.Tests
dotnet test
```



