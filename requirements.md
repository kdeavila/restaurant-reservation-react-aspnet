# Restaurant Reservation System - Requirements

## 1. Overview
The Restaurant Reservation System is designed to allow administrators and restaurant staff to manage table reservations, clients, and availability in real time.  
Only authorized personnel (admins or employees) can perform CRUD operations. Clients cannot reserve directly.

---

## 2. Core Features
- **Client Management**
    - Add, edit, delete, and view clients.
    - Store contact information and reservation history.

- **Table Management**
    - Manage physical tables and their capacity.
    - Assign a table type (Normal, Terrace, VIP) with different base prices.
    - Track table status (active, maintenance, inactive).

- **Reservation Management**
    - Create, edit, cancel, and list reservations.
    - Check table availability for a specific date/time range.
    - Store calculated prices (base price × duration + surcharges).
    - Support surcharges for weekends and peak hours.

- **User Management**
    - Add, edit, delete, and view users (staff accounts).
    - Role-based access (Admin, Employee).

---

## 3. Database Entities

### 3.1 TableType
| Field              | Type     | Description                              |
|--------------------|----------|------------------------------------------|
| Id                 | PK       | Unique identifier                        |
| Name               | string   | Table type (Normal, Terrace, VIP)        |
| Description        | string   | Optional description                     |
| BasePricePerHour   | decimal  | Price per hour for this table type       |
| DaySurcharge       | decimal  | Optional surcharge for weekends/holidays |
| PeakHourSurcharge  | decimal  | Optional surcharge for peak hours        |
| IsActive           | bool     | Whether this type is available           |
| CreatedAt          | datetime | Creation timestamp                       |

---

### 3.2 Table
| Field        | Type     | Description                                 |
|--------------|----------|---------------------------------------------|
| Id           | PK       | Unique identifier                           |
| TableCode    | string   | Human-friendly alphanumeric identifier      |
| Capacity     | int      | Number of guests the table can seat         |
| TableTypeId  | FK       | Reference to TableType                      |
| Location     | string   | Physical location (optional)                |
| Status       | enum     | Active, Maintenance, Inactive               |
| CreatedAt    | datetime | Creation timestamp                          |

---

### 3.3 Client
| Field       | Type     | Description                     |
|-------------|----------|---------------------------------|
| Id          | PK       | Unique identifier               |
| FirstName   | string   | Client's first name             |
| LastName    | string   | Client's last name              |
| Email       | string   | Unique email address            |
| Phone       | string   | Optional phone number           |
| CreatedAt   | datetime | Creation timestamp              |
| Status      | enum     | Active, Inactive                |

---

### 3.4 Reservation
| Field              | Type     | Description                                 |
|--------------------|----------|---------------------------------------------|
| Id                 | PK       | Unique identifier                           |
| ClientId           | FK       | Reference to Client                         |
| TableId            | FK       | Reference to Table                          |
| Date               | date     | Reservation date                            |
| StartTime          | time     | Start time                                  |
| EndTime            | time     | End time                                    |
| NumberOfGuests     | int      | Number of guests for the reservation        |
| BasePrice          | decimal  | Calculated base price                       |
| DaySurcharge       | decimal  | Applied weekend/holiday surcharge           |
| PeakHourSurcharge  | decimal  | Applied peak hour surcharge                 |
| TotalPrice         | decimal  | Final total price                           |
| Status             | enum     | Confirmed, Canceled, NoShow                 |
| Notes              | string   | Optional notes                              |
| CreatedAt          | datetime | Creation timestamp                          |
| UpdatedAt          | datetime | Last update timestamp                       |

---

### 3.5 User
| Field        | Type     | Description                     |
|--------------|----------|---------------------------------|
| Id           | PK       | Unique identifier               |
| Username     | string   | Unique username                 |
| PasswordHash | string   | Hashed password                 |
| Role         | enum     | Admin, Employee                 |
| CreatedAt    | datetime | Creation timestamp              |
| Status       | enum     | Active, Inactive                |

---

## 4. Business Rules
1. Only available tables can be booked for a given time range.
2. Prices are calculated based on:
    - Base price per hour from TableType.
    - Duration in hours.
    - Optional surcharges for weekends and peak hours.
3. Clients must have unique email addresses.
4. Users must have unique usernames.
5. Reservations cannot overlap for the same table and time range.
6. Deleting a client will not remove their past reservations (for history/audit).

---

## 5. Technology Stack
- **Backend**: ASP.NET Core Web API, Entity Framework Core
- **Database**: SQL Server (default, configurable)
- **Frontend**: React (JavaScript/TypeScript)
- **Authentication**: JWT-based auth for Users
- **Styling**: Tailwind CSS (frontend)

---

## 6. Next Steps
1. Create ERD diagram and confirm database schema.
2. Implement database models in backend.
3. Run initial migration and create the database.
4. Build CRUD endpoints for all entities.
5. Connect frontend to backend API.
